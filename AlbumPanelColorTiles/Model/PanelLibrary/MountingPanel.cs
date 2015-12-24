using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{   
   /// <summary>
   /// блок панели СБ - из монтажного плана конструкторов
   /// </summary>
   public class MountingPanel
   {
      public List<AttributeRefDetail> AttrDet { get; private set; }
      public Extents3d ExtTransToModel { get; private set; }
      public ObjectId IdBlRef { get; private set; }      
      public string MarkPainting { get; private set; }
      public string MarkSbWithoutElectric { get; private set; }      
      public PanelAKR PanelAkrLib { get; private set; }
      public Point3d PtCenterPanelSbInModel { get; private set; }

      public MountingPanel(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Matrix3d trans, string mark, string painting)
      {
         MarkSbWithoutElectric = GetMarkWithoutElectric(mark).Replace(' ', '-');
         MarkPainting = painting;
         var extBlRefPanel = blRefPanelSB.GeometricExtentsСlean(); //blRefPanelSB.GeometricExtents;
         ExtTransToModel = new Extents3d(extBlRefPanel.MinPoint.TransformBy(trans),
                                             extBlRefPanel.MaxPoint.TransformBy(trans));
         IdBlRef = blRefPanelSB.Id;
         AttrDet = attrsDet;
         PtCenterPanelSbInModel = getCenterPanelInModel();
      }

      public static string GetMarkWithoutElectric(string markSB)
      {
         string res = markSB;
         var matchs = Regex.Matches(markSB, @"-\d{0,2}[э,Э]($|_)");
         if (matchs.Count == 1)
         {
            res = markSB.Substring(0, matchs[0].Index);
         }
         return res;
      }

      public static List<MountingPanel> GetPanels(BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
      {
         // Поиск всех панелей СБ в определении блока
         List<MountingPanel> panelsSB = new List<MountingPanel>();
         using (var btr = blRefMounting.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in btr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefPanelSB = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     // как определить что это блок панели СБ?
                     // По набору атрибутов: Покраска, МАРКА
                     string mark = string.Empty;
                     string paint = string.Empty;
                     if (!blRefPanelSB.IsDynamicBlock && blRefPanelSB.AttributeCollection != null)
                     {
                        List<AttributeRefDetail> attrsDet = new List<AttributeRefDetail>();
                        foreach (ObjectId idAtrRef in blRefPanelSB.AttributeCollection)
                        {
                           using (var atrRef = idAtrRef.GetObject(OpenMode.ForRead, false, true) as AttributeReference)
                           {
                              // ПОКРАСКА
                              if (string.Equals(atrRef.Tag, Settings.Default.AttributePanelSbPaint, StringComparison.CurrentCultureIgnoreCase))
                              {
                                 var atrDet = new AttributeRefDetail(atrRef);
                                 attrsDet.Add(atrDet);
                                 paint = atrRef.TextString;
                                 // Если выполняется создание альбома и выключена опция проверки покраски, то стираем марку покраски из монтажной панели
                                 if (libLoadServ.Album != null && !libLoadServ.Album.StartOptions.CheckMarkPainting)
                                 {
                                    // Удаление старой марки покраски из блока монтажной панели
                                    atrRef.UpgradeOpen();
                                    atrRef.TextString = string.Empty;
                                 }
                              }
                              // МАРКА
                              else if (string.Equals(atrRef.Tag, Settings.Default.AttributePanelSbMark, StringComparison.CurrentCultureIgnoreCase))
                              {
                                 var atrDet = new AttributeRefDetail(atrRef);
                                 attrsDet.Add(atrDet);
                                 mark = atrRef.TextString;
                              }
                           }
                        }
                        if (attrsDet.Count == 2)
                        {
                           MountingPanel panelSb = new MountingPanel(blRefPanelSB, attrsDet, blRefMounting.BlockTransform, mark, paint);
                           panelsSB.Add(panelSb);
                        }
                     }
                  }
               }
            }
         }
         if (panelsSB.Count == 0)
         {
            // Ошибка - в блоке монтажного плана, не найдена ни одна панель
            Inspector.AddError(string.Format("В блоке монтажного плана {0} не найдена ни одна панель.", blRefMounting.Name), blRefMounting);
         }
         return panelsSB;
      }

      /// <summary>
      /// Загрузка панелей из библиотеки. И запись загруженных блоков в список панелей СБ.
      /// </summary>
      /// <param name="_allPanelsSB">Список всех панелей-СБ в чертеже</param>
      public static void LoadBtrPanels(List<FacadeMounting> facades)
      {
         // файл библиотеки
         if (!File.Exists(PanelLibrarySaveService.LibPanelsFilePath))
         {
            throw new Exception("Не найден файл библиотеки АКР-Панелей - " + PanelLibrarySaveService.LibPanelsFilePath);
         }

         Database dbFacade = HostApplicationServices.WorkingDatabase;
         using (Database dbLib = new Database(false, true))
         {
            dbLib.ReadDwgFile(PanelLibrarySaveService.LibPanelsFilePath, FileShare.ReadWrite, true, "");
            dbLib.CloseInput(true);
            using (var t = dbLib.TransactionManager.StartTransaction())
            {
               // список блоков АКР-Панелей в библиотеке (полные имена блоков).
               List<PanelAKR> panelsAkrInLib = PanelAKR.GetAkrPanelLib(dbLib);
               // словарь соответствия блоков в библиотеке и в чертеже фасада после копирования блоков
               Dictionary<ObjectId, PanelAKR> idsPanelsAkrInLibAndFacade = new Dictionary<ObjectId, PanelAKR>();
               List<MountingPanel> allPanelsSb = facades.SelectMany(f => f.Floors.SelectMany(s => s.PanelsSbInFront)).ToList();
               foreach (var panelSb in allPanelsSb)
               {
                  PanelAKR panelAkrLib = findAkrPanelFromPanelSb(panelSb, panelsAkrInLib);
                  if (panelAkrLib == null)
                  {
                     // Не найден блок в библиотеке
                     Inspector.AddError(string.Format("Не найдена панель в библиотеке соответствующая монтажке - {0}", panelSb.MarkSbWithoutElectric),
                                       panelSb.ExtTransToModel, panelSb.IdBlRef);
                  }
                  else
                  {
                     //panelAkrLib.PanelSb = panelSb;
                     if (!idsPanelsAkrInLibAndFacade.ContainsKey(panelAkrLib.IdBtrAkrPanel))
                     {
                        idsPanelsAkrInLibAndFacade.Add(panelAkrLib.IdBtrAkrPanel, panelAkrLib);
                     }
                     panelSb.PanelAkrLib = panelAkrLib;
                  }
               }
               // Копирование блоков в базу чертежа фасада
               if (idsPanelsAkrInLibAndFacade.Count > 0)
               {
                  IdMapping iMap = new IdMapping();
                  dbFacade.WblockCloneObjects(new ObjectIdCollection(idsPanelsAkrInLibAndFacade.Keys.ToArray()),
                                             dbFacade.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
                  // запись соответствия панелей
                  foreach (var item in idsPanelsAkrInLibAndFacade)
                  {
                     item.Value.IdBtrPanelAkrInFacade = iMap[item.Key].Value;
                     // определение габаритов панели
                     ((PanelAKR)item.Value).DefineGeom(item.Value.IdBtrPanelAkrInFacade);
                  }
               }
               t.Commit();
            }
         }
         // определение отметок этажей Ч и П в фасадах
         facades.ForEach(f => f.DefYForUpperAndParapetStorey());
      }      
      
      private static PanelAKR CompareMarkSbAndAkrs(List<PanelAKR> panelsAkrLib, string markSb, MountingPanel panelSb)
      {
         // Поиск соответствующей АКР-Панели с учетом торцов
         PanelAKR panelAkrLib = null;
         foreach (var panelAkrItem in panelsAkrLib)
         {
            if (string.Equals(markSb, panelAkrItem.MarkAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
            {
               panelAkrLib = panelAkrItem;
               break;
            }
         }
         return panelAkrLib;
      }

      private static PanelAKR findAkrPanelFromPanelSb(MountingPanel panelSb, List<PanelAKR> panelsAkrInLib)
      {
         return CompareMarkSbAndAkrs(panelsAkrInLib, panelSb.MarkSbWithoutElectric, panelSb);
      }
      
      private Point3d getCenterPanelInModel()
      {
         // Определение точки центра блока панели СБ в Модели
         return new Point3d(ExtTransToModel.MinPoint.X + (ExtTransToModel.MaxPoint.X - ExtTransToModel.MinPoint.X) * 0.5,
                            ExtTransToModel.MinPoint.Y + (ExtTransToModel.MaxPoint.Y - ExtTransToModel.MinPoint.Y) * 0.5, 0);
      }
   }
}
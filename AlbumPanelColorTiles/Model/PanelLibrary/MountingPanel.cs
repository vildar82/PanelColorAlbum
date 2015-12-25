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
   // блок панели СБ - из монтажного плана конструкторов
   public class MountingPanel
   {
      private List<AttributeRefDetail> _attrsDet;
      private Extents3d _extBlRefPanel;
      private Extents3d _extTransToModel;
      private ObjectId _idBlRef;      
      private string _markSb;
      private PanelAkrLib _panelAkrLib;
      private Point3d _ptCenterPanelSbInModel; 

      public MountingPanel(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Matrix3d trans, string mark, string painting)
      {
         _markSb = GetMarkWithoutElectric(mark);//.Replace(' ', '-');
         // Проверка есть ли запись Окна _ОК1 в имени марки панели
         string windowSx;
         _markSb = GetMarkWithoutWindowsSuffix(_markSb, out windowSx);
         WindowSuffix = windowSx;

         MarkPainting = painting;
         _extBlRefPanel = blRefPanelSB.GeometricExtentsСlean(); //blRefPanelSB.GeometricExtents;
         _extTransToModel = new Extents3d();
         _extTransToModel.AddPoint(_extBlRefPanel.MinPoint.TransformBy(trans));
         _extTransToModel.AddPoint(_extBlRefPanel.MaxPoint.TransformBy(trans));
         _idBlRef = blRefPanelSB.Id;
         _attrsDet = attrsDet;
         _ptCenterPanelSbInModel = getCenterPanelInModel();
      }      

      public List<AttributeRefDetail> AttrDet { get { return _attrsDet; } }
      public Extents3d ExtTransToModel { get { return _extTransToModel; } }
      public ObjectId IdBlRef { get { return _idBlRef; } }      

      public string MarkPainting { get; private set; }
      public string MarkSb { get { return _markSb; } }
      public string WindowSuffix { get; private set; }      
      public PanelAkrLib PanelAkrLib { get { return _panelAkrLib; } set { _panelAkrLib = value; } }
      public Point3d PtCenterPanelSbInModel { get { return _ptCenterPanelSbInModel; } }

      public static string GetMarkWithoutWindowsSuffix(string markSB, out string windowSuffix)
      {
         windowSuffix = string.Empty;
         string res = markSB;
         var matchs = Regex.Matches(markSB, @"_ок\d{0,2}($|_)", RegexOptions.IgnoreCase);
         if (matchs.Count == 1)
         {
            res = markSB.Substring(0, matchs[0].Index);
            windowSuffix = matchs[0].Value.EndsWith("_")? matchs[0].Value.Substring(1, matchs[0].Length - 2): matchs[0].Value.Substring(1);
         }
         return res;
      }

      public static string GetMarkWithoutElectric(string markSB)
      {
         string res = markSB;
         // "-1э" или в конце строи или перед разделителем "_".
         var matchs = Regex.Matches(markSB, @"-\d{0,2}э($|_)", RegexOptions.IgnoreCase);
         if (matchs.Count == 1)
         {
            int indexAfterElectric = matchs[0].Index + (matchs[0].Value.EndsWith("_") ? matchs[0].Value.Length - 1 : matchs[0].Value.Length);
            res = markSB.Substring(0, matchs[0].Index) + markSB.Substring(indexAfterElectric);
         }
         return res;
      }

      // Поиск всех панелей СБ в определении блока
      public static List<MountingPanel> GetPanels(BlockReference blRefMounting, Point3d ptBase, Matrix3d trans, PanelLibraryLoadService libLoadServ)
      {
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
                           var atrRef = idAtrRef.GetObject(OpenMode.ForRead, false, true) as AttributeReference;
                           // Покраска
                           if (string.Equals(atrRef.Tag, Settings.Default.AttributePanelSbPaint, StringComparison.CurrentCultureIgnoreCase))
                           {
                              atrRef.UpgradeOpen();
                              var atrDet = new AttributeRefDetail(atrRef);
                              attrsDet.Add(atrDet);
                              paint = atrRef.TextString;
                              if (libLoadServ.Album != null && !libLoadServ.Album.StartOptions.CheckMarkPainting)
                              {
                                 // Удаление старой марки покраски из блока монтажной панели
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
                        if (attrsDet.Count == 2)
                        {
                           MountingPanel panelSb = new MountingPanel(blRefPanelSB, attrsDet, trans, mark, paint);
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
               List<PanelAkrLib> panelsAkrInLib = PanelAkrLib.GetAkrPanelLib(dbLib);
               // словарь соответствия блоков в библиотеке и в чертеже фасада после копирования блоков
               Dictionary<ObjectId, PanelAkrLib> idsPanelsAkrInLibAndFacade = new Dictionary<ObjectId, PanelAkrLib>();
               List<MountingPanel> allPanelsSb = facades.SelectMany(f => f.Floors.SelectMany(s => s.PanelsSbInFront)).ToList();
               foreach (var panelSb in allPanelsSb)
               {
                  PanelAkrLib panelAkrLib = findAkrPanelFromPanelSb(panelSb, panelsAkrInLib);
                  if (panelAkrLib == null)
                  {
                     // Не найден блок в библиотеке
                     Inspector.AddError(string.Format("Не найдена панель в библиотеке соответствующая монтажке - {0}", panelSb.MarkSb),
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

      public Point3d GetPtInModel(PanelAkrLib panelAkrLib)
      {
         return new Point3d(PtCenterPanelSbInModel.X - panelAkrLib.DistToCenterFromBase,
                              PtCenterPanelSbInModel.Y + 500, 0);
      }      

      private static PanelAkrLib findAkrPanelFromPanelSb(MountingPanel panelSb, List<PanelAkrLib> panelsAkrInLib)
      {         
         return panelsAkrInLib.Find(
                           p => string.Equals(p.MarkAkr, panelSb.MarkSb, StringComparison.CurrentCultureIgnoreCase) &&
                                string.Equals(p.WindowSuffix, panelSb.WindowSuffix, StringComparison.CurrentCultureIgnoreCase));
      }

      // Определение точки центра блока панели СБ в Модели
      private Point3d getCenterPanelInModel()
      {
         return new Point3d(_extTransToModel.MinPoint.X + (_extTransToModel.MaxPoint.X - _extTransToModel.MinPoint.X) * 0.5,
                                       _extTransToModel.MinPoint.Y + (_extTransToModel.MaxPoint.Y - _extTransToModel.MinPoint.Y) * 0.5,
                                       0);
      }
   }
}
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
      //public Extents3d ExtBlRefClean { get; private set; }
      public ObjectId IdBlRef { get; private set; }
      public ObjectId IdBtr { get; private set; }
      public string MarkPainting { get; private set; }
      public string MarkSb { get; private set; }
      public string MarkSbWithoutElectric { get; private set; }      
      public PanelAKR PanelAkr { get; set; }
      public Point3d PtCenterPanelSbInModel { get; private set; }      
      public Model.Base.PanelBase PanelBase { get; set; }
      public string WindowSuffix { get; private set; }
      public int Thickness { get; private set; }
      /// <summary>
      /// Индекс класса бетона панели (определяется по приставке цифры к короткому имени марки панелои 3НСНг2 - индекс =2, 3НСНг = 0)
      /// </summary>
      public int IndexConcreteClass { get; private set; }

      public MountingPanel(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Matrix3d trans, string mark, string painting)
      {
         MarkSb = mark;
         MarkSbWithoutElectric = GetMarkWithoutElectric(mark);//.Replace(' ', '-');
         // Проверка есть ли запись Окна _ОК1 в имени марки панели
         string windowSx;
         MarkSbWithoutElectric = GetMarkWithoutWindowsSuffix(MarkSbWithoutElectric, out windowSx);
         WindowSuffix = windowSx;

         MarkPainting = painting;
         var extBlRefPanel = blRefPanelSB.GeometricExtentsСlean(); //blRefPanelSB.GeometricExtents;         
         Thickness = getThickness(extBlRefPanel);
         //ExtBlRefClean = extBlRefPanel;
         extBlRefPanel.TransformBy(trans);
         ExtTransToModel = extBlRefPanel;         
         IdBlRef = blRefPanelSB.Id;
         IdBtr = blRefPanelSB.BlockTableRecord;
         AttrDet = attrsDet;         
         PtCenterPanelSbInModel = getCenterPanelInModel();

         IndexConcreteClass = DefineIndexConcreteClass(MarkSb);
      }      

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

      public static List<MountingPanel> GetPanels(BlockTableRecord btr, Point3d ptBase, Matrix3d transform, PanelLibraryLoadService libLoadServ)
      {
         // Поиск всех панелей СБ в определении блока
         List<MountingPanel> panelsSB = new List<MountingPanel>();
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
                              if (libLoadServ?.Album != null && !libLoadServ.Album.StartOptions.CheckMarkPainting)
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
                        MountingPanel panelSb = new MountingPanel(blRefPanelSB, attrsDet, transform, mark, paint);
                        panelsSB.Add(panelSb);
                     }
                  }
               }
            }
         }         
         if (panelsSB.Count == 0)
         {
            // Ошибка - в блоке монтажного плана, не найдена ни одна панель
            Inspector.AddError($"В блоке монтажного плана {btr.Name} не найдена ни одна панель.",
                              new Extents3d(ptBase, new Point3d(ptBase.X + 10000, ptBase.Y + 10000, 0)), 
                              btr.Id, icon: System.Drawing.SystemIcons.Error);
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
                     Inspector.AddError($"Не найдена панель в библиотеке соответствующая монтажке - {panelSb.MarkSbWithoutElectric}",
                                       panelSb.ExtTransToModel, panelSb.IdBlRef, icon: System.Drawing.SystemIcons.Error);
                  }
                  else
                  {
                     //panelAkrLib.PanelSb = panelSb;
                     if (!idsPanelsAkrInLibAndFacade.ContainsKey(panelAkrLib.IdBtrAkrPanel))
                     {
                        idsPanelsAkrInLibAndFacade.Add(panelAkrLib.IdBtrAkrPanel, panelAkrLib);
                     }
                     panelSb.PanelAkr = panelAkrLib;
                  }
               }
               // Копирование блоков в базу чертежа фасада
               if (idsPanelsAkrInLibAndFacade.Count > 0)
               {
                  using (IdMapping iMap = new IdMapping())
                  {
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
               }
               t.Commit();
            }
         }
         // определение отметок этажей Ч и П в фасадах
         facades.ForEach(f => f.DefYForUpperAndParapetStorey());
      }

      private static PanelAKR findAkrPanelFromPanelSb(MountingPanel panelSb, List<PanelAKR> panelsAkrInLib)
      {
         return panelsAkrInLib.Find(
                           p => string.Equals(p.MarkAkr, panelSb.MarkSbWithoutElectric, StringComparison.CurrentCultureIgnoreCase) &&
                                string.Equals(p.WindowSuffix, panelSb.WindowSuffix, StringComparison.CurrentCultureIgnoreCase));
      }

      private Point3d getCenterPanelInModel()
      {
         // Определение точки центра блока панели СБ в Модели
         return new Point3d(ExtTransToModel.MinPoint.X + (ExtTransToModel.MaxPoint.X - ExtTransToModel.MinPoint.X) * 0.5,
                            ExtTransToModel.MinPoint.Y + (ExtTransToModel.MaxPoint.Y - ExtTransToModel.MinPoint.Y) * 0.5, 0);
      }

      public void RemoveWindowSuffix()
      {
         if (!string.IsNullOrEmpty(WindowSuffix))
         {
            var atrMarkInfo = AttrDet.Find(a => string.Equals(a.Tag, Settings.Default.AttributePanelSbMark,
                                                         StringComparison.CurrentCultureIgnoreCase));
            if (atrMarkInfo != null)
            {
               var atr = atrMarkInfo.IdAtrRef.GetObject(OpenMode.ForWrite, false, true) as AttributeReference;
               var indexOfWindowSuf = atr.TextString.LastIndexOf(WindowSuffix, StringComparison.CurrentCultureIgnoreCase);
               if (indexOfWindowSuf != -1)
               {                  
                  atr.TextString = atr.TextString.Substring(0, indexOfWindowSuf-1);
               }               
            }
         }
      }

      public void RemoveMarkPainting()
      {
         AttributeRefDetail atrMarkPaintInfo = AttrDet.Find(a => string.Equals(a.Tag, Settings.Default.AttributePanelSbPaint,
                                                         StringComparison.CurrentCultureIgnoreCase));
         if (atrMarkPaintInfo != null)
         {
            var atrRef = atrMarkPaintInfo.IdAtrRef.GetObject(OpenMode.ForWrite, false, true) as AttributeReference;
            atrRef.TextString = ""; 
         }
      }

      private int getThickness(Extents3d extBlRefPanel)
      {
         var lenX = Convert.ToInt32(extBlRefPanel.MaxPoint.X - extBlRefPanel.MinPoint.X);
         var lenY = Convert.ToInt32(extBlRefPanel.MaxPoint.Y - extBlRefPanel.MinPoint.Y);
         return (lenX > lenY) ? roundTo10(lenY) : roundTo10(lenX);
      }
      private static int roundTo10(int i)
      {         
         if (i % 10 != 0)
         {
            i = ((i + 5) / 10) * 10;
         }
         return i;
      }

      public static int DefineIndexConcreteClass(string markSb)
      {
         var splits = markSb.Split(' ');
         if (splits.Count() > 1)
         {
            int r = (int)char.GetNumericValue(splits[0].Last());
            if (r > 0)
            {
               return r;
            }
         }
         return 0;
      }

      /// <summary>
      /// Возвращает марку без индекса класса бетона. Для '3НСНг2 75.29.42-6' вернет '3НСНг 75.29.42-6'
      /// 
      /// </summary>      
      public static string GetMarkWoConcreteClass(string markSb)
      {
         var splits = markSb.Split(' ');
         if (splits.Count() > 1)
         {
            string shortGost = splits[0];
            int r = (int)char.GetNumericValue(shortGost.Last());
            if (r > 0)
            {
               return markSb.Remove(shortGost.Length - 1, 1);
            }
         }
         return markSb;
      }
   }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // блок панели СБ - из монтажного плана конструкторов
   public class PanelSB
   {
      private List<AttributeRefDetail> _attrsDet;
      private Extents3d _extBlRefPanel;
      private Extents3d _extTransToModel;
      private ObjectId _idBlRef;      
      //// панель торцевая - торец слева
      //private bool _isEndRightPanel;
      //private bool _isEndLeftPanel;

      // границы панели трансформированные в координаты модели
      private bool _isInFloor;

      private string _markSb;      
      private string _markSbWithoutWhite;
      private string _markSbWithoutWhiteAndElectric;
      private PanelAkrLib _panelAkrLib;
      private Point3d _ptCenterPanelSbInModel; // точка вставки в Модели
                                               // панель входит в этаж - внутри блока монтажного плана и внутри блока обозначения стороны фасада
                                               // панель торцевая - торец справа

      public PanelSB(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Matrix3d trans, string mark)
      {
         _markSb = mark;
         _extBlRefPanel = blRefPanelSB.GeometricExtentsСlean(); //blRefPanelSB.GeometricExtents;
         _extTransToModel = new Extents3d();
         _extTransToModel.AddPoint(_extBlRefPanel.MinPoint.TransformBy(trans));
         _extTransToModel.AddPoint(_extBlRefPanel.MaxPoint.TransformBy(trans));
         _idBlRef = blRefPanelSB.Id;
         _attrsDet = attrsDet;
         _ptCenterPanelSbInModel = getCenterPanelInModel();
         _markSbWithoutWhite = _markSb.Replace(' ', '-');
         _markSbWithoutWhiteAndElectric = getMarkWithoutElectric(_markSbWithoutWhite);
      }

      public List<AttributeRefDetail> AttrDet { get { return _attrsDet; } }
      public Extents3d ExtTransToModel { get { return _extTransToModel; } }
      public ObjectId IdBlRef { get { return _idBlRef; } }
      //public bool IsEndLeftPanel { get { return _isEndLeftPanel; } set { _isEndLeftPanel = value; } }
      //public bool IsEndRightPanel { get { return _isEndRightPanel; } set { _isEndRightPanel = value; } }
      public bool IsInFloor { get { return _isInFloor; } set { _isInFloor = value; } }
      public string MarkSb { get { return _markSb; } }
      public string MarkSbBlockName { get { return Panels.MarkSb.GetMarkSbBlockName(_markSb); } }
      public string MarkSbWithoutWhite { get { return _markSbWithoutWhite; } }
      public string MarkSbWithoutWhiteAndElectric { get { return _markSbWithoutWhiteAndElectric; } }
      public PanelAkrLib PanelAkrLib { get { return _panelAkrLib; } set { _panelAkrLib = value; } }
      public Point3d PtCenterPanelSbInModel { get { return _ptCenterPanelSbInModel; } }

      

      // Поиск всех панелей СБ в определении блока
      public static List<PanelSB> GetPanels(BlockReference blRefMounting, Point3d ptBase, Matrix3d trans)
      {
         List<PanelSB> panelsSB = new List<PanelSB>();
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
                     if (!blRefPanelSB.IsDynamicBlock && blRefPanelSB.AttributeCollection != null)
                     {
                        List<AttributeRefDetail> attrsDet = new List<AttributeRefDetail>();
                        foreach (ObjectId idAtrRef in blRefPanelSB.AttributeCollection)
                        {
                           var atrRef = idAtrRef.GetObject(OpenMode.ForRead, false, true) as AttributeReference;
                           // Покраска
                           if (string.Equals(atrRef.Tag, Settings.Default.AttributePanelSbPaint, StringComparison.CurrentCultureIgnoreCase))
                           {
                              var atrDet = new AttributeRefDetail(atrRef);
                              attrsDet.Add(atrDet);
                           }
                           // МАРКА
                           else if (string.Equals(atrRef.Tag, Settings.Default.AttributePanelSbMark, StringComparison.CurrentCultureIgnoreCase))
                           {
                              var atrDet = new AttributeRefDetail(atrRef);
                              attrsDet.Add(atrDet);
                              mark = atrDet.Text;
                           }
                        }
                        if (attrsDet.Count == 2)
                        {
                           PanelSB panelSb = new PanelSB(blRefPanelSB, attrsDet, trans, mark);
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
      public static void LoadBtrPanels(List<Facade> facades)
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
               List<PanelSB> allPanelsSb = facades.SelectMany(f => f.Floors.SelectMany(s => s.PanelsSbInFront)).ToList();               
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

      // Поиск соответствующей АКР-Панели с учетом торцов
      private static PanelAkrLib CompareMarkSbAndAkrs(List<PanelAkrLib> panelsAkrLib, string markSb, PanelSB panelSb)
      {
         PanelAkrLib panelAkrLib = null;
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

      private static PanelAkrLib findAkrPanelFromPanelSb(PanelSB panelSb, List<PanelAkrLib> panelsAkrInLib)
      {
         PanelAkrLib panelAkrLib = null;
         // точное соответствие - торцы можно не проыерять
         panelAkrLib = CompareMarkSbAndAkrs(panelsAkrInLib, panelSb.MarkSbWithoutWhite, panelSb);
         if (panelAkrLib == null)
         {
            // поиск панели без электрики с проверкой торцов
            panelAkrLib = CompareMarkSbAndAkrs(panelsAkrInLib, panelSb.MarkSbWithoutWhiteAndElectric, panelSb);
            if (panelAkrLib != null)
            {
               // Копирование блока АКР-Панели без индекса электрики - с прибавкой индекса электрики
               PanelAkrLib panelAkrElectric = panelAkrLib.CopyLibBlockElectricInTempFile(panelSb);
               if (panelAkrElectric != null)
               {
                  panelAkrLib = panelAkrElectric;
                  panelsAkrInLib.Add(panelAkrElectric);
               }
            }
         }
         return panelAkrLib;
      }

      private static string getMarkWithoutElectric(string markSB)
      {
         string res = string.Empty;

         var matchs = Regex.Matches(markSB, @"-\d{0,2}[э,Э]");
         if (matchs.Count == 1)
         {
            res = markSB.Substring(0, matchs[0].Index);
         }
         return res;
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
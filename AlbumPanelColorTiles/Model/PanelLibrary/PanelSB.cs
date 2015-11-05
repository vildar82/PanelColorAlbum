using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Checks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // блок панели СБ - из монтажного плана конструкторов
   public class PanelSB
   {
      private List<AttributeRefDetail> _attrsDet;
      private string _markSb;
      private string _markSbWithoutWhite;
      private string _markSbWithoutWhiteAndElectric;
      private ObjectId _idBlRef;
      private PanelAKR _panelAKR;
      private Point3d _ptCenterPanelSbInModel; // точка вставки в Модели
      private Extents3d _extBlRefPanel;
      private Extents3d _extTransToModel; // границы панели трансформированные в координаты модели
      private bool _isInFloor; // панель входит в этаж - внутри блока монтажного плана и внутри блока обозначения стороны фасада
      private bool _isEndLeftPanel; // панель торцевая - торец слева
      private bool _isEndRightPanel; // панель торцевая - торец справа

      public PanelSB(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Matrix3d trans, string mark)
      {
         _markSb = mark;
         _extBlRefPanel = blRefPanelSB.GeometricExtents;             
         _extTransToModel = new Extents3d();
         _extTransToModel.AddPoint(_extBlRefPanel.MinPoint.TransformBy(trans));
         _extTransToModel.AddPoint(_extBlRefPanel.MaxPoint.TransformBy(trans));
         _idBlRef = blRefPanelSB.Id;
         _attrsDet = attrsDet;         
         _ptCenterPanelSbInModel = getCenterPanelInModel();
         _markSbWithoutWhite = _markSb.Replace(' ', '-');
         _markSbWithoutWhiteAndElectric = getMarkWithoutElectric(_markSbWithoutWhite);
      }

      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } set { _isEndLeftPanel = value; } }
      public bool IsEndRightPanel { get { return _isEndRightPanel; } set { _isEndRightPanel = value; } }
      public bool IsInFloor { get { return _isInFloor; } set { _isInFloor = value; } }
      public ObjectId IdBlRef { get { return _idBlRef; } }
      public string MarkSb { get { return _markSb; } }
      public string MarkSbWithoutWhite { get { return _markSbWithoutWhite; } }
      public string MarkSbWithoutWhiteAndElectric { get { return _markSbWithoutWhiteAndElectric; } }
      public List<AttributeRefDetail> AttrDet { get { return _attrsDet; } }
      public Point3d PtCenterPanelSbInModel { get { return _ptCenterPanelSbInModel; } }
      public Extents3d ExtTransToModel { get { return _extTransToModel; } }
      public PanelAKR PanelAKR { get { return _panelAKR; } set { _panelAKR = value; } }

      // Поиск всех панелей СБ в определении блока
      public static List<PanelSB> GetPanels(ObjectId idBtr, Point3d ptBase, Matrix3d trans)
      {
         List<PanelSB> panelsSB = new List<PanelSB>();
         using (var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in btr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefPanelSB = idEnt.GetObject(OpenMode.ForRead) as BlockReference)
                  {
                     // как определить что это блок панели СБ?
                     // По набору атрибутов: Покраска, МАРКА
                     string mark = string.Empty; 
                     if (!blRefPanelSB.IsDynamicBlock && blRefPanelSB.AttributeCollection != null)
                     {
                        List<AttributeRefDetail> attrsDet = new List<AttributeRefDetail>();
                        foreach (ObjectId idAtrRef in blRefPanelSB.AttributeCollection)
                        {
                           var atrRef = idAtrRef.GetObject(OpenMode.ForRead) as AttributeReference;
                           // Покраска
                           if (string.Equals(atrRef.Tag, Album.Options.AttributePanelSbPaint, StringComparison.CurrentCultureIgnoreCase))
                           {
                              var atrDet = new AttributeRefDetail(atrRef);
                              attrsDet.Add(atrDet);
                           }
                           // МАРКА
                           else if (string.Equals(atrRef.Tag, Album.Options.AttributePanelSbMark, StringComparison.CurrentCultureIgnoreCase))
                           {
                              var atrDet = new AttributeRefDetail(atrRef);
                              attrsDet.Add(atrDet);
                              mark = atrDet.Text;
                           }
                        }
                        if (attrsDet.Count == 2)
                        {
                           PanelSB panelSb = new PanelSB(blRefPanelSB, attrsDet,trans, mark);
                           panelsSB.Add(panelSb);
                        }                        
                     }
                  }
               }
            }
         }
         return panelsSB;
      }

      // Определение точки центра блока панели СБ в Модели
      private Point3d getCenterPanelInModel()
      {
         return new Point3d(_extTransToModel.MinPoint.X + (_extTransToModel.MaxPoint.X - _extTransToModel.MinPoint.X) * 0.5,
                                       _extTransToModel.MinPoint.Y + (_extTransToModel.MaxPoint.Y - _extTransToModel.MinPoint.Y) * 0.5,
                                       0);         
      }

      public Point3d GetPtInModel(PanelAKR panelAkr)
      {
         return new Point3d(PtCenterPanelSbInModel.X - panelAkr.DistToCenterFromBase, PtCenterPanelSbInModel.Y + 500, 0);
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
         // копирование в temp
         string fileLibPanelsTemp = Path.GetTempFileName();
         File.Copy(PanelLibrarySaveService.LibPanelsFilePath, fileLibPanelsTemp, true);

         Database dbFacade = HostApplicationServices.WorkingDatabase;
         using (Database dbLib = new Database(false, true))
         {
            dbLib.ReadDwgFile(fileLibPanelsTemp, FileShare.ReadWrite, true, "");
            HostApplicationServices.WorkingDatabase = dbLib;
            using (var t = dbLib.TransactionManager.StartTransaction())
            {
               // список блоков АКР-Панелей в библиотеке (полные имена блоков).
               List<PanelAKR> panelsAkrInLib = GetAkrPanelNames(dbLib);               
               // словарь соответствия блоков в библиотеке и в чертеже фасада после копирования блоков
               Dictionary<ObjectId, PanelAKR> idsPanelsAkrInLibAndFacade = new Dictionary<ObjectId, PanelAKR>();
               var allPanelsSb = facades.SelectMany(f => f.Floors.SelectMany(s => s.PanelsSbInFront));
               foreach (var panelSb in allPanelsSb)
               {
                  PanelAKR panelAkr = findAkrPanelFromPanelSb(panelSb, panelsAkrInLib);
                  if (panelAkr == null)
                  {
                     // Не найден блок в библиотеке
                     Inspector.AddError(string.Format("Не найдена панель в библиотеке соответствующая монтажке - {0}", panelSb.MarkSb),
                                       panelSb.ExtTransToModel, panelSb.IdBlRef);
                  }
                  else
                  {
                     panelAkr.PanelSb = panelSb;
                     if (!idsPanelsAkrInLibAndFacade.ContainsKey(panelAkr.IdBtrAkrPanelInLib))
                     {
                        idsPanelsAkrInLibAndFacade.Add(panelAkr.IdBtrAkrPanelInLib, panelAkr);
                     }
                     panelSb.PanelAKR = panelAkr;
                  }
               }
               // Копирование блоков в базу чертежа фасада
               if (idsPanelsAkrInLibAndFacade.Count > 0)
               {
                  IdMapping iMap = new IdMapping();
                  dbFacade.WblockCloneObjects(new ObjectIdCollection(idsPanelsAkrInLibAndFacade.Keys.ToArray()),
                                             dbFacade.BlockTableId, iMap, DuplicateRecordCloning.Ignore, false);
                  // запись соответствия панелей 
                  foreach (var item in idsPanelsAkrInLibAndFacade)
                  {                     
                     item.Value.IdBtrAkrPanelInFacade = iMap[item.Key].Value;
                  }
               }
               t.Commit();
            }
         }
         HostApplicationServices.WorkingDatabase = dbFacade;
         try
         {
            File.Delete(fileLibPanelsTemp);
         }
         catch (Exception ex)
         {
            string errMsg = ex.ToString();
         }         
      }

      private static PanelAKR findAkrPanelFromPanelSb(PanelSB panelSb, List<PanelAKR> panelsAkrInLib)
      {
         PanelAKR panelAkr = null;
         // точное соответствие - торцы можно не проыерять
         panelAkr = CompareMarkSbAndAkrs(panelsAkrInLib, panelSb.MarkSbWithoutWhite, panelSb, false);
         if (panelAkr == null)
         {
            // поиск панели без электрики с проверкой торцов
            panelAkr = CompareMarkSbAndAkrs(panelsAkrInLib, panelSb.MarkSbWithoutWhiteAndElectric, panelSb, true);
            if (panelAkr != null)
            {
               // Копирование блока АКР-Панели без индекса электрики - с прибавкой индекса электрики
               PanelAKR panelAkrElectric = panelAkr.CopyLibBlockElectricInTempFile(panelSb);
               if (panelAkrElectric != null)
               {
                  panelAkr = panelAkrElectric;
                  panelsAkrInLib.Add(panelAkrElectric);
               }
            }
         }
         return panelAkr;
      }

      // Поиск соответствующей АКР-Панели с учетом торцов
      private static PanelAKR CompareMarkSbAndAkrs(List<PanelAKR> panelsAkr, string markSb, PanelSB panelSb, bool checkEnds)
      {
         PanelAKR panelAkr = null;
         foreach (var panelAkrItem in panelsAkr)
         {            
            if (string.Equals(markSb, panelAkrItem.MarkAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
            {
               if (checkEnds)
               {
                  // проверка торцов
                  if (panelSb.IsEndLeftPanel == panelAkrItem.IsEndLeftPanel &&
                     panelSb.IsEndRightPanel == panelAkrItem.IsEndRightPanel)
                  {
                     panelAkr = panelAkrItem;
                     break;
                  }
               }
               else
               {
                  panelAkr = panelAkrItem;
                  break;
               }
            }
         }
         return panelAkr;
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

      public static List<PanelAKR> GetAkrPanelNames(Database dbLib)
      {
         List<PanelAKR> panelsAkr = new List<PanelAKR>();
         using (var bt = dbLib.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            foreach (ObjectId idBtr in bt)
            {
               using (var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord)
               {
                  if (MarkSbPanelAR.IsBlockNamePanel(btr.Name))
                  {
                     PanelAKR panelAkr = new PanelAKR(idBtr, btr.Name);
                     panelsAkr.Add(panelAkr);
                  }
               }
            }
         }
         return panelsAkr;
      }
   }
}

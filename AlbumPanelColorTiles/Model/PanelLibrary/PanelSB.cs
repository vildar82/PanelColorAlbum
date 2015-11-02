using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // блок панели СБ - из монтажного плана конструкторов
   public class PanelSB
   {
      private List<AttributeRefDetail> _attrsDet;
      private ObjectId _idBlRef;
      private PanelAKR _panelAKR;
      private Point3d _ptCenterPanelSbInModel; // точка вставки в Модели

      public PanelSB(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet, Point3d ptCenterPanelSbInModel)
      {
         _idBlRef = blRefPanelSB.Id;
         _attrsDet = attrsDet;
         _ptCenterPanelSbInModel = ptCenterPanelSbInModel;
      }

      public List<AttributeRefDetail> AttrDet { get { return _attrsDet; } }
      public Point3d PtCenterPanelSbInModel { get { return _ptCenterPanelSbInModel; } }
      public PanelAKR PanelAKR { get { return _panelAKR; } set { _panelAKR = value; } }

      // Поиск всех панелей СБ в определении блока
      public static List<PanelSB> GetPanels(ObjectId idBtr, Point3d ptBase)
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
                           }
                        }
                        if (attrsDet.Count == 2)
                        {
                           PanelSB panelSb = new PanelSB(blRefPanelSB, attrsDet, getCenterPanelInModel(blRefPanelSB, ptBase));
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
      private static Point3d getCenterPanelInModel(BlockReference blRefPanelSB, Point3d ptBase)
      {
         var ext = blRefPanelSB.GeometricExtents;
         Point3d ptCenter = new Point3d(ext.MinPoint.X + (ext.MaxPoint.X-ext.MinPoint.X)*0.5, ext.MaxPoint.Y, 0);
         return new Point3d(ptBase.X+ptCenter.X, ptBase.Y+ptCenter.Y, 0);
      }

      /// <summary>
      /// Загрузка панелей из библиотеки. И запись загруженных блоков в список панелей СБ.
      /// </summary>
      /// <param name="_allPanelsSB">Список всех панелей-СБ в чертеже</param>
      public static void LoadBtrPanels(List<PanelSB> _allPanelsSB)
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
            using (var t = dbLib.TransactionManager.StartTransaction())
            {
               // список блоков АКР-Панелей в библиотеке (полные имена блоков).
               Dictionary<ObjectId, string> blAkrPanelsNames = getAkrPanelNames(dbLib);               
               // словарь соответствия блоков в библиотеке и в чертеже фасада после копирования блоков
               Dictionary<ObjectId, PanelAKR> idsPanelsAkrInLibAndFacade = new Dictionary<ObjectId, PanelAKR>();
               foreach (var panelSb in _allPanelsSB)
               {
                  ObjectId idBtrAkrPanelInLib = findAkrPanelFromPanelSb(panelSb, blAkrPanelsNames);
                  if (idBtrAkrPanelInLib.IsNull)
                  {
                     // Не найден блок в библиотеке
                  }
                  else
                  {                     
                     if (!idsPanelsAkrInLibAndFacade.ContainsKey(idBtrAkrPanelInLib))
                     {
                        PanelAKR panelAkr = new PanelAKR(idBtrAkrPanelInLib, panelSb);
                        panelSb.PanelAKR = panelAkr;
                        idsPanelsAkrInLibAndFacade.Add(idBtrAkrPanelInLib, panelAkr);
                     }                     
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
         try
         {
            File.Delete(fileLibPanelsTemp);
         }
         catch (Exception ex)
         {
            string errMsg = ex.ToString();
         }         
      }

      private static ObjectId findAkrPanelFromPanelSb(PanelSB panelSb, Dictionary< ObjectId, string> blAkrPanelsNames)
      {
         ObjectId idBtrAkrPanel = ObjectId.Null;
         string markPanelSb = panelSb.AttrDet.First(
                              p => string.Equals(p.Tag, Album.Options.AttributePanelSbMark, StringComparison.CurrentCultureIgnoreCase)).Text;
         string markSbWithoutWhite = markPanelSb.Replace(' ', '-');
         foreach (var item in blAkrPanelsNames)
         {
            string markAkrWithoutWhite = MarkSbPanelAR.GetMarkSbCleanName(MarkSbPanelAR.GetMarkSbName(item.Value)).Replace(' ', '-');
            if (string.Equals(markSbWithoutWhite, markAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
            {
               idBtrAkrPanel = item.Key;
               break;
            }
         }
         return idBtrAkrPanel;
      }

      private static Dictionary<ObjectId, string> getAkrPanelNames(Database db)
      {
         Dictionary<ObjectId, string> names = new Dictionary<ObjectId, string>();
         using (var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            foreach (ObjectId idBtr in bt)
            {
               using (var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord)
               {
                  if (MarkSbPanelAR.IsBlockNamePanel(btr.Name))
                  {
                     names.Add(btr.Id, btr.Name);
                  }
               }
            }
         }
         return names;
      }
   }
}

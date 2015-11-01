using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using FuzzyStrings;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // блок панели СБ - из монтажного плана конструкторов
   public class PanelSB
   {
      private List<AttributeRefDetail> _attrsDet;
      private ObjectId _idBlRef;    

      public PanelSB(BlockReference blRefPanelSB, List<AttributeRefDetail> attrsDet)
      {
         _idBlRef = blRefPanelSB.Id;
         _attrsDet = attrsDet;
      }

      public List<AttributeRefDetail> AttrDet { get { return _attrsDet; } }

      // Поиск всех панелей СБ в определении блока
      public static List<PanelSB> GetPanels(ObjectId idBtr)
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
                     if (blRefPanelSB.AttributeCollection != null)
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
                           PanelSB panelSb = new PanelSB(blRefPanelSB, attrsDet);
                        }                        
                     }
                  }
               }
            }
         }
         return panelsSB;
      }

      /// <summary>
      /// Загрузка панелей из библиотеки
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

         using (Database dbLib = new Database(false, true))
         {
            dbLib.ReadDwgFile(fileLibPanelsTemp, FileShare.ReadWrite, true, "");
            using (var t = dbLib.TransactionManager.StartTransaction())
            {
               Dictionary<ObjectId, string> blAkrPanelsNames = getAkrPanelNames(dbLib);
               foreach (var panelSb in _allPanelsSB)
               {
                  ObjectId idBtrAkrPanel = findAkrPanelFromPanelSb(panelSb, blAkrPanelsNames);
                  if (idBtrAkrPanel.IsNull)
                  {

                  }
                  else
                  {
                     
                  }
               }
            }
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
            string markAkrWithoutWhite = item.Value.Replace(' ', '-');
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

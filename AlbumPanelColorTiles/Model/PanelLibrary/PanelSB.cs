using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

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
   }
}

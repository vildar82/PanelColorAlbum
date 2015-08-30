using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Альбом колористических решений.
   public class Album
   {
      public static Options options;
      // Набор цветов используемых в альбоме.
      private static List<Paint> _colors;
      private List<MarkSbPanel> _marksSb;
      private List<ColorArea> _colorAreas;

      Document _doc;
      Database _db;
      ObjectId _msId;

      public Album()
      {
         options = new Options();
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
         using (var t = _db.TransactionManager.StartTransaction () )
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            _msId = bt[SymbolUtilityServices.BlockModelSpaceName];
         }
      }

      // Покраска панелей в модели (по блокам зон покраски)
      public void PaintPanels()
      {
         // сброс списка цветов.
         _colors = new List<Paint>();

         // Определение зон покраски
         _colorAreas = ColorArea.GetColorAreas(_msId);
         if (_colorAreas.Count == 0)
         {
            throw new Exception("\nНе найдены блоки зон покраски.");
         }

         // Определение покраски панелей.
         _marksSb = GetMarksSB();
      }

      // Определение покраски панелей текущего чертежа (в Модели)
      private List<MarkSbPanel> GetMarksSB()
      {         
         List<MarkSbPanel> _marks = new List<MarkSbPanel>();

         using (var t = _db.TransactionManager.StartTransaction())
         {
            // Перебор всех блоков в модели и составление списка блоков марок и панелей.  
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(_msId, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefPanel = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  // Определение Марки СБ панели. Если ее еще нето, то она создается и добавляется в список _marks.
                  MarkSbPanel markSb = MarkSbPanel.GetMarkSbPanel(blRefPanel, _marks, bt);
                  if (markSb == null)
                     continue;
                  //TODO: Определение покраски панели (Марки АР)
                  List<Paint> paintAR = MarkArPanel.GetPanelMarkAR(markSb, blRefPanel, _colorAreas);
                  // Добавление панели АР в список панелей для Марки СБ
                  markSb.AddPanelAR(paintAR, blRefPanel);
               }
            }
            t.Commit();
         }
         return _marks;
      }

      public static Paint GetPaint(string layerName)
      {
         Paint paint = _colors.Find(c => c.LayerName == layerName);
         if (paint == null)
         {
            paint = new Paint(layerName);
            _colors.Add(paint);
         }
         return paint;
      }
   }
}

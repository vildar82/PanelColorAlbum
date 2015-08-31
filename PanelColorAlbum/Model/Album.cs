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
      private List<ColorArea> _colorAreasBackground;
      private List<ColorArea> _colorAreasForeground;

      Document _doc;
      Database _db;
      public static ObjectId _msId;

      public Album()
      {
         options = new Options();
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;         
      }

      // Покраска панелей в модели (по блокам зон покраски)
      public void PaintPanels()
      {
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            _msId = bt[SymbolUtilityServices.BlockModelSpaceName];
         }

         // сброс списка цветов.
         _colors = new List<Paint>();

         // Определение зон покраски
         List<ColorArea> colorAreas = ColorArea.GetColorAreas(_msId);
         // Определение фоновых и передних зон покраси
         DefColorAreaGrounds(colorAreas);
         
         // Определение покраски панелей.
         _marksSb = GetMarksSB();

         // Создание определений блоков панелей покраски МаркиАР       
         CreatePanelsMarkAR();

         // Замена вхождений блоков панелей Марки СБ на блоки панелей Марки АР.
         ReplaceBlocksMarkSbOnMarkAr();
      }      

      // Определение покраски панелей текущего чертежа (в Модели)
      private List<MarkSbPanel> GetMarksSB()
      {         
         List<MarkSbPanel> _marksSb = new List<MarkSbPanel>();

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
                  MarkSbPanel markSb = MarkSbPanel.GetMarkSb(blRefPanel, _marksSb, bt);
                  if (markSb == null)
                  {
                     // Значит это не блок панели.
                     continue;
                  }
                  //TODO: Определение покраски панели (Марки АР)
                  List<Paint> paintAR = MarkArPanel.GetPanelMarkAR(markSb, blRefPanel, _colorAreasForeground, _colorAreasBackground);
                  // Добавление панели АР в список панелей для Марки СБ
                  markSb.AddPanelAR(paintAR, blRefPanel);
               }
            }
            t.Commit();
         }
         return _marksSb;
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

      // Разделение зон покраски на фоновые и передние зоны покраски
      private void DefColorAreaGrounds(List<ColorArea> colorAreas)
      {
         _colorAreasBackground = new List<ColorArea>();
         _colorAreasForeground = new List<ColorArea>();
         bool foregroundArea = false;

         foreach (var colorArea in colorAreas.ToArray())
         {
            // Если точка MinPoint или MaxPoint находится внутри другой зоны, то это передняя зона.
            foreach (var colorAreaOther in colorAreas)
            {
               if (Geometry.IsPointInBounds(colorArea.Bounds.MinPoint, colorAreaOther.Bounds) ||
                   Geometry.IsPointInBounds(colorArea.Bounds.MaxPoint, colorAreaOther.Bounds))
               {
                  _colorAreasForeground.Add(colorArea);
                  colorAreas.Remove(colorArea);
                  foregroundArea = true;
                  break;
               }
            }
            if (!foregroundArea)
            {
               _colorAreasBackground.Add(colorArea);
               foregroundArea = false;
            }
         }
      }

      // Создание определений блоков панелей марки АР
      private void CreatePanelsMarkAR()
      {
         foreach (var markSB in _marksSb)
         {
            foreach (var markAR in markSB.MarksAR)
            {
               markAR.CreateBlock(markSB);
            }
         }
      }

      // Замена вхождений блоков панелей Марки СБ на панели Марки АР
      private void ReplaceBlocksMarkSbOnMarkAr()
      {
         foreach (var markSb in _marksSb)
         {
            markSb.ReplaceBlocksSbOnAr(); 
         }
      }
   }
}

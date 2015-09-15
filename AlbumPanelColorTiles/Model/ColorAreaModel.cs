using System.Collections.Generic;
using AlbumPanelColorTiles.Model.Lib;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model
{
   // Зоны покраски в Модели
   public class ColorAreaModel
   {
      private List<ColorArea> _colorAreasBackground;
      private List<ColorArea> _colorAreasForeground;

      public ColorAreaModel(ObjectId ms)
      {
         var colorAreas = GetColorAreas(ms);
         DefColorAreaGrounds(colorAreas);
      }

      public List<ColorArea> ColorAreasBackground { get { return _colorAreasBackground; } }
      public List<ColorArea> ColorAreasForeground { get { return _colorAreasForeground; } }

      // Определение зон покраски в определении блока
      public static List<ColorArea> GetColorAreas(ObjectId idBtr)
      {
         List<ColorArea> colorAreas = new List<ColorArea>();
         using (var t = idBtr.Database.TransactionManager.StartTransaction())
         {
            var btrMarkSb = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefColorArea = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  if (Lib.Blocks.EffectiveName(blRefColorArea) == Album.Options.BlockColorAreaName)
                  {
                     ColorArea colorArea = new ColorArea(blRefColorArea);
                     colorAreas.Add(colorArea);
                  }
               }
            }
            t.Commit();
         }
         return colorAreas;
      }

      // Разделение зон покраски на фоновые и передние зоны покраски
      private void DefColorAreaGrounds(List<ColorArea> colorAreas)
      {
         _colorAreasBackground = new List<ColorArea>();
         _colorAreasForeground = new List<ColorArea>();
         bool foregroundArea;
         foreach (var colorArea in colorAreas.ToArray())
         {
            foregroundArea = false;
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
   }
}
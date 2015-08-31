using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Зона покраски
   public  class ColorArea
   {
      private ObjectId _idblRef;
      private Extents3d _bounds;
      private Paint _paint;

      public Extents3d Bounds
      {
         get { return _bounds; }
         set { _bounds = value; }
      }

      public Paint Paint
      {
         get { return _paint; }
         set { _paint = value; }
      }

      public ColorArea(BlockReference blRef)
      {
         _idblRef = blRef.ObjectId;
         // Определение габаритов
         _bounds = GetBounds(blRef);
         _paint = Album.GetPaint(blRef.Layer ); 
      }

      // Определение зон покраски в определении блока
      public static List<ColorArea> GetColorAreas(ObjectId idBtr)
      {
         List<ColorArea> colorAreas = new List<ColorArea>();         

         //TODO: Определение зон покраски в определении блока Марки СБ
         using (var t = idBtr.Database.TransactionManager.StartTransaction())
         {
            var btrMarkSb = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefColorArea = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  if (Lib.Blocks.EffectiveName(blRefColorArea) == Album.options.BlockColorAreaName)
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

      // Определение покраски. Попадание точки в зону окраски
      public static Paint GetPaint(Extents3d boundsTile,  List<ColorArea> colorAreasForeground, List<ColorArea> colorAreasBackground)
      {
         Paint paint = GetPaintFromColorAreas(boundsTile, colorAreasForeground);
         if (paint == null)
         {
            if (colorAreasBackground != null)
            {
               paint = GetPaintFromColorAreas(boundsTile, colorAreasBackground);
            }
         }
         return paint;
      }

      private static Paint GetPaintFromColorAreas(Extents3d boundsTile, List<ColorArea> colorAreas)
      {         
         // Центр плитки
         Point3d ptCentretile = new Point3d((boundsTile.MaxPoint.X + boundsTile.MinPoint.X) * 0.5,  
                                            (boundsTile.MaxPoint.Y + boundsTile.MinPoint.Y) * 0.5, 0);
         foreach (ColorArea colorArea in colorAreas)
         {                         
            if (Geometry.IsPointInBounds(ptCentretile, colorArea.Bounds))             
            {
               return colorArea.Paint;               
            }
         }
         return null;
      }

      private Extents3d GetBounds(BlockReference blRef)
      {
         Extents3d bounds;
         if (blRef.Bounds.HasValue)
         {
            bounds = blRef.Bounds.Value; 
         }
         else
         {
            bounds = new Extents3d(Point3d.Origin, Point3d.Origin);
         }
         return bounds;
      }
   }
}

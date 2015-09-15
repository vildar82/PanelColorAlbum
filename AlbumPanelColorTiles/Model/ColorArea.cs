using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Model.Lib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model
{
   // Зона покраски
   public class ColorArea : IEquatable<ColorArea>
   {
      private Extents3d _bounds;
      private ObjectId _idblRef;
      private Paint _paint;

      public ColorArea(BlockReference blRef)
      {
         _idblRef = blRef.ObjectId;
         // Определение габаритов
         _bounds = GetBounds(blRef);
         _paint = Album.FindPaint(blRef.Layer);
      }

      public Extents3d Bounds
      {
         get { return _bounds; }
      }

      public Paint Paint
      {
         get { return _paint; }
      }

      // Определение покраски. Попадание точки в зону окраски
      public static Paint GetPaint(Extents3d boundsTile, List<ColorArea> colorAreasForeground, List<ColorArea> colorAreasBackground)
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

      public bool Equals(ColorArea other)
      {
         return _bounds.IsEqualTo(other._bounds, Album.Tolerance);
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
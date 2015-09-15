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
      public static Paint GetPaint(Point3d centerTile, List<ColorArea> colorAreasForeground, List<ColorArea> colorAreasBackground)
      {
         Paint paint = GetPaintFromColorAreas(centerTile, colorAreasForeground);
         if (paint == null)
         {
            if (colorAreasBackground != null)
            {
               paint = GetPaintFromColorAreas(centerTile, colorAreasBackground);
            }
         }
         return paint;
      }

      private static Paint GetPaintFromColorAreas(Point3d centerTile, List<ColorArea> colorAreas)
      {
         // Центр плитки         
         foreach (ColorArea colorArea in colorAreas)
         {
            if (Geometry.IsPointInBounds(centerTile, colorArea.Bounds))
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
using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model
{
   // Зона покраски
   public class ColorArea : IEquatable<ColorArea>, IComparable<ColorArea>
   {
      #region Private Fields

      private Extents3d _bounds;
      private ObjectId _idblRef;
      private Paint _paint;
      private double _size;

      #endregion Private Fields

      #region Public Constructors

      public ColorArea(BlockReference blRef)
      {          
         _idblRef = blRef.ObjectId;
         // Определение габаритов
         _bounds = blRef.GeometricExtents;
         _paint = Album.FindPaint(blRef.Layer);
         _size = (_bounds.MaxPoint.X - _bounds.MinPoint.X) * (_bounds.MaxPoint.Y - _bounds.MinPoint.Y);
      }

      #endregion Public Constructors

      #region Public Properties

      public Extents3d Bounds
      {
         get { return _bounds; }
      }

      public Paint Paint
      {
         get { return _paint; }
      }

      #endregion Public Properties

      #region Public Methods

      // Определение покраски. Попадание точки в зону окраски
      public static Paint GetPaint(Point3d centerTile, List<ColorArea> colorAreas)
      {
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

      public int CompareTo(ColorArea other)
      {
         return _size.CompareTo(other._size);
      }

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
                  if (string.Equals(Lib.Blocks.EffectiveName(blRefColorArea),
                     Album.Options.BlockColorAreaName,
                     System.StringComparison.InvariantCultureIgnoreCase))
                  {
                     ColorArea colorArea = new ColorArea(blRefColorArea);
                     colorAreas.Add(colorArea);
                  }
               }
            }
            t.Commit();
         }
         // Сортировка зон покраски по размеру
         colorAreas.Sort();
         return colorAreas;
      }

      #endregion Private Methods
   }
}
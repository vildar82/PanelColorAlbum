using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RTreeLib;

namespace AlbumPanelColorTiles.Panels
{
   // Зона покраски
   public class ColorArea : IEquatable<ColorArea>, IComparable<ColorArea>
   {
      private Extents3d _bounds;
      private ObjectId _idblRef;
      private Paint _paint;
      private double _size;

      public ColorArea(BlockReference blRef, Album album, Matrix3d trans)
      {
         _idblRef = blRef.ObjectId;
         // Определение габаритов
         _bounds = blRef.GeometricExtents;
         _bounds.TransformBy(trans);
         _paint = album.GetPaint(blRef.Layer);
         _size = (_bounds.MaxPoint.X - _bounds.MinPoint.X) * (_bounds.MaxPoint.Y - _bounds.MinPoint.Y);
      }

      public Extents3d Bounds
      {
         get { return _bounds; }
      }

      public Paint Paint
      {
         get { return _paint; }
      }

      // Определение зон покраски в определении блока
      public static List<ColorArea> GetColorAreas(ObjectId idBtr, Album album)
      {
         List<ColorArea> colorAreas = new List<ColorArea>();
         IterateColorAreasInBtr(idBtr, album, colorAreas, Matrix3d.Identity, string.Empty);
         // Сортировка зон покраски по размеру
         colorAreas.Sort();
         return colorAreas;
      }

      // Определение покраски. Попадание точки в зону окраски
      public static Paint GetPaint(Point3d centerTile, RTree<ColorArea> rtreeColorAreas)
      {
         if (rtreeColorAreas.Count > 0)
         {
            Point p = new Point(centerTile.X, centerTile.Y, 0);
            var colorAreas = rtreeColorAreas.Nearest(p, Settings.Default.TileLenght);
            colorAreas.Sort();
            foreach (ColorArea colorArea in colorAreas)
            {
               if (colorArea.Bounds.IsPointInBounds(centerTile))
               {
                  return colorArea.Paint;
               }
            }
         }
         return null;
      }

      public static Rectangle GetRectangleRTree(Extents3d extents)
      {
         return new Rectangle(extents.MinPoint.X, extents.MinPoint.Y, extents.MaxPoint.X, extents.MaxPoint.Y, 0, 0);
      }

      public static RTree<ColorArea> GetRTree(List<ColorArea> _colorAreas)
      {
         RTree<ColorArea> rtree = new RTree<ColorArea>();
         foreach (var colorArea in _colorAreas)
         {
            Rectangle rectTree = GetRectangleRTree(colorArea.Bounds);
            try
            {
               rtree.Add(rectTree, colorArea);
            }
            catch { }
         }
         return rtree;
      }

      public int CompareTo(ColorArea other)
      {
         return _size.CompareTo(other._size);
      }

      public bool Equals(ColorArea other)
      {
         return _bounds.IsEqualTo(other._bounds, Album.Tolerance);
      }

      private static string getBlNameWithoutXrefPrefix(string blName, string xrefName)
      {
         if (!string.IsNullOrEmpty(xrefName))
         {
            if (blName.IndexOf(xrefName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
               return blName.Substring(xrefName.Length + 1);
            }
         }
         return blName;
      }

      private static void IterateColorAreasInBtr(ObjectId idBtr, Album album,
                                                List<ColorArea> colorAreas, Matrix3d matrix, string xrefName)
      {
         using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in btr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefColorArea = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     var blName = getBlNameWithoutXrefPrefix(blRefColorArea.GetEffectiveName(), xrefName);
                     if (string.Equals(blName, Settings.Default.BlockColorAreaName, StringComparison.InvariantCultureIgnoreCase))
                     {
                        ColorArea colorArea = new ColorArea(blRefColorArea, album, matrix);
                        colorAreas.Add(colorArea);
                     }
                     else
                     {
                        // Если это не блок Панели, то ищем вложенные в блоки зоны покраски
                        if (!MarkSb.IsBlockNamePanel(blName))
                        {
                           using (var btrInner = blRefColorArea.BlockTableRecord.Open(OpenMode.ForRead) as BlockTableRecord)
                           {
                              // Обработка вложенных зон покраски в блок
                              if (btrInner.IsFromExternalReference)
                              {
                                 IterateColorAreasInBtr(btrInner.Id, album, colorAreas,
                                    blRefColorArea.BlockTransform.PostMultiplyBy(matrix), btrInner.Name);
                              }
                              else
                              {
                                 IterateColorAreasInBtr(btrInner.Id, album, colorAreas,
                                    blRefColorArea.BlockTransform.PostMultiplyBy(matrix), xrefName);
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
      }
   }
}
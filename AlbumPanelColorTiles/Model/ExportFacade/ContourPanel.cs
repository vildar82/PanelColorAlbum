using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   // Построение контура панели
   public class ContourPanel
   {
      private enum EnumCorner
      {
         LeftLower,
         LeftTop,
         RightLower,
         RightTop
      }

      private BlockTableRecord btr;
      private Extents3d extentsByTile;
      private List<Extents3d> tiles;
      private RTreeLib.RTree<Extents3d> treeTiles;
      private double  endOffset = 500;
      private ObjectId idLayerContour;

      public ContourPanel(BlockTableRecord btr, List<Extents3d> tiles, Extents3d extentsByTile)
      {
         this.btr = btr;
         this.extentsByTile = extentsByTile;
         this.tiles = tiles;
      }

      public void CreateContour()
      {
         // Создание контура плитки
         var layer = new AcadLib.Layers.LayerInfo("АР_Швы");
         layer.LineWeight = LineWeight.LineWeight030;
         idLayerContour = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layer);

         // из всех плиток отделить торцевые плитки????
         // дерево границ плиток
         treeTiles = new RTreeLib.RTree<Extents3d>();
         tiles.ForEach(t => treeTiles.Add(ColorArea.GetRectangleRTree(t), t));

         // Первый угол панели - левый нижний         
         var pt1 = getCoordTileNoEnd(extentsByTile.MinPoint, EnumCorner.LeftLower);
         var pt2 = getCoordTileNoEnd(new Point3d (extentsByTile.MinPoint.X, extentsByTile.MaxPoint.Y, 0), EnumCorner.LeftTop);
         var pt3 = getCoordTileNoEnd(extentsByTile.MaxPoint, EnumCorner.RightTop);
         var pt4 = getCoordTileNoEnd(new Point3d(extentsByTile.MaxPoint.X, extentsByTile.MinPoint.Y,  0), EnumCorner.RightLower);

         Point3dCollection pts = new Point3dCollection();
         pts.Add(pt1);
         pts.Add(pt2);
         pts.Add(pt3);
         pts.Add(pt4);
         using (Polyline3d poly = new Polyline3d(Poly3dType.SimplePoly, pts, true))
         {
            poly.LayerId = idLayerContour;
            btr.AppendEntity(poly);
         }
      }      

      private Point3d getCoordTileNoEnd(Point3d pt1, EnumCorner corner)
      {
         double x = 0;
         RTreeLib.Point pt1X = new RTreeLib.Point(pt1.X + getOffsetX(corner), pt1.Y, 0);
         var resTiles = treeTiles.Nearest(pt1X, 100);
         if (resTiles.Count == 0)
         {
            // Нет плитки - торец!
            // Найти первую не торцевую плитку
            var extTile1X = findTile(pt1.X + getOffsetX(corner), pt1.Y, true, 100);
            x = getCoordX(extentsByTile, corner);
         }
         else
         {
            // Есть плитки - не торец
            x = pt1.X;
         }

         double y = 0;
         RTreeLib.Point pt1Y = new RTreeLib.Point(pt1.X, pt1.Y + getOffsetY(corner), 0);
         resTiles = treeTiles.Nearest(pt1Y, 100);
         if (resTiles.Count == 0)
         {
            // Нет плитки - торец!
            // Найти первую не торцевую плитку
            var extTile1X = findTile(pt1.X , pt1.Y+ getOffsetX(corner), true, 100);
            y = getCoordY(extentsByTile, corner);
         }
         else
         {
            // Есть плитки - не торец
            y = pt1.Y;
         }
         return new Point3d(x, y, 0);
      }

      private double getCoordX(Extents3d extentsByTile, EnumCorner corner)
      {
         switch (corner)
         {
            case EnumCorner.LeftLower:               
            case EnumCorner.LeftTop:
               return extentsByTile.MinPoint.X;
            case EnumCorner.RightLower:               
            case EnumCorner.RightTop:
               return extentsByTile.MaxPoint.X;
            default:
               return 0;
         }
      }

      private double getCoordY(Extents3d extentsByTile, EnumCorner corner)
      {
         switch (corner)
         {
            case EnumCorner.LeftLower:
            case EnumCorner.RightLower:
               return extentsByTile.MinPoint.Y;            
            case EnumCorner.LeftTop:
            case EnumCorner.RightTop:
               return extentsByTile.MaxPoint.Y;
            default:
               return 0;
         }
      }

      private double getOffsetX(EnumCorner corner)
      {
         switch (corner)
         {
            case EnumCorner.LeftLower:               
            case EnumCorner.LeftTop:
               return this.endOffset;
            case EnumCorner.RightLower:               
            case EnumCorner.RightTop:
               return -this.endOffset;
            default:
               return 0;               
         }
      }

      private double getOffsetY(EnumCorner corner)
      {
         switch (corner)
         {
            case EnumCorner.LeftTop:
            case EnumCorner.RightTop:
               return -this.endOffset;
            case EnumCorner.LeftLower:
            case EnumCorner.RightLower:            
               return this.endOffset;
            default:
               return 0;
         }
      }

      private Extents3d findTile(double x, double y, bool isX, int delta)
      {
         RTreeLib.Point ptNext;
         if (isX)
         {
            x += delta;
         }
         else
         {
            y += delta;
         }
         ptNext = new RTreeLib.Point(x, y, 0);
         var resTiles = treeTiles.Nearest(ptNext, 100);
         if (resTiles.Count == 0)
         {
            return findTile(x, y, isX, delta);
         }
         else
         {
            var resVal = resTiles.First();
            return resVal;
         }
      }
   }
}

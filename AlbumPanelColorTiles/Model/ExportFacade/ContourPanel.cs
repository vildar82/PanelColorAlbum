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

      private ConvertPanelBtr convertBtr;
      private BlockTableRecord btr;
      private Extents3d extentsByTile;
      private List<Extents3d> tiles;
      private RTreeLib.RTree<Extents3d> treeTiles;
      private double  endOffset = 500;
      

      public ContourPanel(BlockTableRecord btr, ConvertPanelBtr convertBtr)
      {
         this.convertBtr = convertBtr;
         this.btr = btr;
         this.extentsByTile = convertBtr.ExtentsByTile;
         this.tiles = convertBtr.Tiles;
      }

      public void CreateContour()
      {
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
            poly.LayerId = convertBtr.Service.IdLayerContour;
            btr.AppendEntity(poly);
         }
      }      

      private Point3d getCoordTileNoEnd(Point3d pt, EnumCorner corner)
      {
         double x = 0;
         RTreeLib.Point pt1X = new RTreeLib.Point(pt.X + getOffsetX(corner, endOffset), pt.Y, 0);
         var resTiles = treeTiles.Nearest(pt1X, 100);
         if (resTiles.Count == 0)
         {
            // Нет плитки - торец!
            // Найти первую не торцевую плитку
            var extTileX = findTile(pt.X + getOffsetX(corner, endOffset), pt.Y,corner, true);
            x = getCoordX(extTileX, corner);
         }
         else
         {
            // Есть плитки - не торец
            x = pt.X;
         }

         double y = 0;
         RTreeLib.Point pt1Y = new RTreeLib.Point(pt.X, pt.Y + getOffsetY(corner, endOffset), 0);
         resTiles = treeTiles.Nearest(pt1Y, 100);
         if (resTiles.Count == 0)
         {
            // Нет плитки - торец!
            // Найти первую не торцевую плитку
            var extTileY = findTile(pt.X , pt.Y+ getOffsetX(corner, endOffset), corner, false);
            y = getCoordY(extTileY, corner);
         }
         else
         {
            // Есть плитки - не торец
            y = pt.Y;
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

      private double getOffsetX(EnumCorner corner, double offset)
      {
         switch (corner)
         {
            case EnumCorner.LeftLower:               
            case EnumCorner.LeftTop:
               return offset;
            case EnumCorner.RightLower:               
            case EnumCorner.RightTop:
               return -offset;
            default:
               return 0;               
         }
      }

      private double getOffsetY(EnumCorner corner, double offset)
      {
         switch (corner)
         {
            case EnumCorner.LeftTop:
            case EnumCorner.RightTop:
               return -offset;
            case EnumCorner.LeftLower:
            case EnumCorner.RightLower:            
               return offset;
            default:
               return 0;
         }
      }

      private Extents3d findTile(double x, double y, EnumCorner corner, bool isX)
      {
         RTreeLib.Point ptNext;
         if (isX)
         {
            x += getOffsetX(corner, 100);
         }
         else
         {
            y += getOffsetY(corner, 100);
         }
         ptNext = new RTreeLib.Point(x, y, 0);
         var resTiles = treeTiles.Nearest(ptNext, 100);
         if (resTiles.Count == 0)
         {
            return findTile(x, y, corner, isX);
         }
         else
         {
            var resVal = resTiles.First();
            return resVal;
         }
      }
   }
}

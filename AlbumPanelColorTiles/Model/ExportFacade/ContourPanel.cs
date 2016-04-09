using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib;
using AcadLib.RTree.SpatialIndex;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
    // Построение контура панели
    public class ContourPanel
    {
        private double endOffset = 500; // Отступ торца от панелей (расст между плитками в свету)
        private double distTileMinInPanel = 30; // минимальное расстояние между плитками при котром они считаются панелью, а не торцом

        private PanelBtrExport panelBtr;

        private RTree<Extents3d> treeTiles;

        public ContourPanel(PanelBtrExport panelBtr)
        {
            this.panelBtr = panelBtr;
        }

        private enum EnumCorner
        {
            LeftLower,
            LeftTop,
            RightLower,
            RightTop
        }

        public static ObjectId CreateLayerContourPanel()
        {
            // Создание контура плитки
            var layer = new AcadLib.Layers.LayerInfo("АР_Контур-панели");
            layer.LineWeight = LineWeight.LineWeight030;
            return AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layer);
        }

        public void CreateContour2(BlockTableRecord btr)
        {
            if (panelBtr.ExtentsByTile.Diagonal() < endOffset)
            {
                return;
            }

            // из всех плиток отделить торцевые плитки????
            // дерево границ плиток
            treeTiles = new RTree<Extents3d>();
            panelBtr.Tiles.ForEach(t =>
            {
                try
                {
                    var r = new Rectangle(t.MinPoint.X, t.MinPoint.Y, t.MaxPoint.X, t.MaxPoint.Y, 0, 0);
                    treeTiles.Add(r, t);
                }
                catch { }
            });

            // Отфильтровать плитки - панели от торцевых.
            List<Polyline> colPlTile = new List<Polyline>();            
            foreach (var item in panelBtr.Tiles)
            {
                // Проверить наличие плиток справа и слева - если есть плитка с одной из сторон то это плитка панели а не торца
                // Проверка наличия плиток слева от этой
                double yCenter = (item.MaxPoint.Y - item.MinPoint.Y) * 0.5;
                var ptCenterLeft = new Point(item.MinPoint.X, yCenter, 0);
                var textX = item.MinPoint.X;
                var finds = treeTiles.Nearest(ptCenterLeft, distTileMinInPanel);
                if (!finds.Skip(1).Any())
                {
                    // Нет плиток слева, проверка плиток справа
                    var ptCenterRight = new Point(item.MaxPoint.X, yCenter, 0);
                    finds = treeTiles.Nearest(ptCenterRight, distTileMinInPanel);
                    if (!finds.Skip(1).Any())
                    {
                        // Нет плиток справа - это торцевая плитка! Пропускаем
                        continue;
                    }
                }                
                // Плитка панели, а не торца
                Polyline pl = item.GetPolyline();
                colPlTile.Add(pl);
            }
            var pl3d = colPlTile.GetExteriorContour();
            pl3d.LayerId = panelBtr.CPS.IdLayerContour;
            btr.AppendEntity(pl3d);
            btr.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(pl3d, true);

            panelBtr.ExtentsNoEnd = pl3d.GeometricExtents;
        }        

        [Obsolete("Используй CreateContour2")]
        public void CreateContour(BlockTableRecord btr)
        {
            if (panelBtr.ExtentsByTile.Diagonal() < endOffset)
            {
                return;
            }

            // из всех плиток отделить торцевые плитки????
            // дерево границ плиток
            treeTiles = new RTree<Extents3d>();
            panelBtr.Tiles.ForEach(t =>
                           {
                               try
                               {
                                   var r = new Rectangle(t.MinPoint.X, t.MinPoint.Y, t.MaxPoint.X, t.MaxPoint.Y, 0, 0);
                                   treeTiles.Add(r, t);
                               }
                               catch { }
                           });

            // Первый угол панели - левый нижний
            var pt1 = getCoordTileNoEnd(panelBtr.ExtentsByTile.MinPoint, EnumCorner.LeftLower);
            var pt2 = getCoordTileNoEnd(new Point3d(panelBtr.ExtentsByTile.MinPoint.X, panelBtr.ExtentsByTile.MaxPoint.Y, 0), EnumCorner.LeftTop);
            var pt3 = getCoordTileNoEnd(panelBtr.ExtentsByTile.MaxPoint, EnumCorner.RightTop);
            var pt4 = getCoordTileNoEnd(new Point3d(panelBtr.ExtentsByTile.MaxPoint.X, panelBtr.ExtentsByTile.MinPoint.Y, 0), EnumCorner.RightLower);

            Extents3d extNoEnd = new Extents3d(pt1, pt2);
            extNoEnd.AddPoint(pt3);
            extNoEnd.AddPoint(pt4);
            panelBtr.ExtentsNoEnd = extNoEnd;

            Point3dCollection pts = new Point3dCollection();
            pts.Add(pt1);
            pts.Add(pt2);
            pts.Add(pt3);
            pts.Add(pt4);
            using (Polyline3d poly = new Polyline3d(Poly3dType.SimplePoly, pts, true))
            {
                poly.LayerId = panelBtr.CPS.IdLayerContour;
                btr.AppendEntity(poly);
                btr.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(poly, true);
            }
        }

        private Extents3d findTile(double x, double y, EnumCorner corner, bool isX)
        {
            Point ptNext;
            if (isX)
            {
                x += getOffsetX(corner, 100);
            }
            else
            {
                y += getOffsetY(corner, 100);
            }
            ptNext = new Point(x, y, 0);
            var resTiles = treeTiles.Nearest(ptNext, distTileMinInPanel);
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

        private Point3d getCoordTileNoEnd(Point3d pt, EnumCorner corner)
        {
            double x = 0;
            Point pt1X = new Point(pt.X + getOffsetX(corner, endOffset), pt.Y, 0);
            var resTiles = treeTiles.Nearest(pt1X, distTileMinInPanel);
            if (resTiles.Count == 0)
            {
                // Нет плитки - торец!
                // Найти первую не торцевую плитку
                var extTileX = findTile(pt.X + getOffsetX(corner, endOffset), pt.Y, corner, true);
                x = getCoordX(extTileX, corner);
            }
            else
            {
                // Есть плитки - не торец
                x = pt.X;
            }

            double y = 0;
            Point pt1Y = new Point(pt.X, pt.Y + getOffsetY(corner, endOffset), 0);
            resTiles = treeTiles.Nearest(pt1Y, distTileMinInPanel);
            if (resTiles.Count == 0)
            {
                // Нет плитки - торец!
                // Найти первую не торцевую плитку
                var extTileY = findTile(pt.X, pt.Y + getOffsetY(corner, endOffset), corner, false);
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base.CreatePanel
{
   public class Contour
   {
      private PanelBase panelBase;

      private List<Extents2d> undercutsExt = new List<Extents2d>();

      private List<Point2d> ptsLeftSide = new List<Point2d>();
      private List<Point2d> ptsTopSide = new List<Point2d>();
      private List<Point2d> ptsRightSide = new List<Point2d>();
      private List<Point2d> ptsBotSide = new List<Point2d>();     

      public Contour(PanelBase panelBase)
      {
         this.panelBase = panelBase;         
      }

      public void Create(BlockTableRecord btrPanel, Transaction t)
      {
         Polyline plContour = new Polyline();
         plContour.LayerId = panelBase.Service.Env.IdLayerContourPanel;

         // Определение подрезок и пустот
         defineUndercuts();

         // Outsides - части сторон панели без плитки - заходит под торец другой угловой панели
         addOutsides(btrPanel, t);

         definePtsLeftSide();
         definePtsTopSide();
         definePtsRightSide();
         definePtsBotSide();

         int i = 0;
         ptsLeftSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsTopSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsRightSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsBotSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));

         plContour.Closed = true;
         btrPanel.AppendEntity(plContour);
         t.AddNewlyCreatedDBObject(plContour, true);
      }

      private void addOutsides(BlockTableRecord btrPanel, Transaction t)
      {
         if (panelBase.Panel.outsides?.outside?.Count()>0)
         {
            foreach (var outside in panelBase.Panel.outsides.outside)
            {
               Polyline plOut = new Polyline();
               Point2d pt;
               double width = Math.Abs(outside.width) + 70;
               if (outside.posi.X<0)
               {
                  pt = new Point2d(outside.posi.X, outside.posi.Y);
                  panelBase.XMinContour = 70;
                  panelBase.XStartTile = 70 + 11;
                  panelBase.XMinPanel = pt.X;
                  panelBase.IsOutsideLeft = true;
               }               
               else
               {
                  pt = new Point2d(outside.posi.X-70, outside.posi.Y);
                  panelBase.XMaxContour += -70;
                  panelBase.XMaxPanel = pt.X + width;
                  panelBase.IsOutsideRight = true;
               }              
                               
               plOut.AddVertexAt(0, pt, 0, 0, 0);
               pt = new Point2d(pt.X+ width, pt.Y);
               plOut.AddVertexAt(1, pt, 0, 0, 0);
               pt = new Point2d(pt.X, pt.Y+outside.height);
               plOut.AddVertexAt(2, pt, 0, 0, 0);
               pt = new Point2d(pt.X- width, pt.Y);
               plOut.AddVertexAt(3, pt, 0, 0, 0);
               plOut.Closed = true;

               plOut.Layer = "0";
               btrPanel.AppendEntity(plOut);
               t.AddNewlyCreatedDBObject(plOut, true);
            }
         }         
      }

      private void defineUndercuts()
      {
         if (panelBase.Panel.undercuts?.undercut?.Count() > 0)
         {
            //var undercuts = panelBase.Panel.undercuts?.undercut?.Select(u => new { posi = u.posi, width = u.width, height = u.height });
            //var outsides = panelBase.Panel.outsides?.outside?.Select(b => new { posi = b.posi, width = b.width, height = b.height });
            //var cuts = undercuts == null ? outsides : outsides?.Union(undercuts) ?? undercuts;
            //if (cuts != null)

            foreach (var undercut in panelBase.Panel.undercuts.undercut)
            {
               Point2d ptMin = new Point2d(undercut.posi.X, undercut.posi.Y);
               Point2d ptMax = new Point2d(undercut.posi.X + undercut.width, undercut.posi.Y + undercut.height);
               var ext2d = new Extents2d(ptMin, ptMax);

               undercutsExt.Add(ext2d);
               panelBase.Openings.Add(new Extents3d
                  (
                     ext2d.MinPoint.Convert3d(),
                     ext2d.MaxPoint.Convert3d()
                  )
               );
            }
         }
      }

      private void definePtsLeftSide()
      {
         ptsLeftSide.Add(new Point2d(panelBase.XMinContour, 0));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MinPoint.X-0)<50)
            {
               ptsLeftSide.Add(new Point2d (panelBase.XMinContour, undercut.MinPoint.Y));
               ptsLeftSide.Add(new Point2d (undercut.MaxPoint.X, undercut.MinPoint.Y));
               ptsLeftSide.Add(undercut.MaxPoint);
               ptsLeftSide.Add(new Point2d(panelBase.XMinContour, undercut.MaxPoint.Y));
            }
         }
      }

      private void definePtsTopSide()
      {
         ptsTopSide.Add(new Point2d(panelBase.XMinContour, panelBase.Height));
         foreach (var cut in undercutsExt)
         {
            if (Math.Abs(cut.MaxPoint.Y - panelBase.Height)<50)
            {
               double y = panelBase.Height;
               ptsTopSide.Add(new Point2d(cut.MinPoint.X, y));               
               ptsTopSide.Add(cut.MinPoint);
               ptsTopSide.Add(new Point2d(cut.MaxPoint.X, cut.MinPoint.Y));
               ptsTopSide.Add(new Point2d (cut.MaxPoint.X, y));

               panelBase.PtsForTopDim.Add(cut.MinPoint.X);
               panelBase.PtsForTopDim.Add(cut.MaxPoint.X);
            }
         }
      }

      private void definePtsRightSide()
      {
         ptsRightSide.Add(new Point2d(panelBase.XMaxContour, panelBase.Height));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MaxPoint.X - panelBase.Length)<50)
            {               
               ptsRightSide.Add(new Point2d (panelBase.XMaxContour, undercut.MaxPoint.Y));
               ptsRightSide.Add(new Point2d(undercut.MinPoint.X, undercut.MaxPoint.Y));
               ptsRightSide.Add(undercut.MinPoint);
               ptsRightSide.Add(new Point2d(panelBase.XMaxContour, undercut.MinPoint.Y));               
            }
         }
      }

      private void definePtsBotSide()
      {
         ptsBotSide.Add(new Point2d(panelBase.XMaxContour, 0));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MaxPoint.Y - 0)<50)
            {
               ptsBotSide.Add(new Point2d(undercut.MaxPoint.X, 0));
               ptsBotSide.Add(undercut.MaxPoint);
               ptsBotSide.Add(new Point2d(undercut.MinPoint.X, undercut.MaxPoint.Y));
               ptsBotSide.Add(new Point2d (undercut.MinPoint.X, 0));               
            }
         }
      }     
   }
}

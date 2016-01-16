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

      public Polyline Create()
      {
         Polyline plContour = new Polyline();
         plContour.LayerId = panelBase.Service.Env.IdLayerContourPanel;

         if (panelBase.Panel.undercuts?.undercut?.Count() > 0)
         {
            foreach (var undercut in panelBase.Panel.undercuts.undercut)
            {
               Extents2d ext2d = getExtents(undercut);
               undercutsExt.Add(ext2d);
               panelBase.Openings.Add(new Extents3d
                  (
                     ext2d.MinPoint.Convert3d(),
                     ext2d.MaxPoint.Convert3d()
                  )
               );
            }
         }

         definePtsLeftSide();
         definePtsTopSide();
         definePtsRightSide();
         definePtsBotSide();

         int i = 0;
         ptsLeftSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsTopSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsRightSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));
         ptsBotSide.ForEach(p => plContour.AddVertexAt(i++, p, 0, 0, 0));

         //plContour.Closed = true;
         return plContour;
      }     

      private void definePtsLeftSide()
      {
         ptsLeftSide.Add(new Point2d(0, 0));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MinPoint.X-0)<50)
            {
               ptsLeftSide.Add(new Point2d (0, undercut.MinPoint.Y));
               ptsLeftSide.Add(new Point2d (undercut.MaxPoint.X, undercut.MinPoint.Y));
               ptsLeftSide.Add(undercut.MaxPoint);
               ptsLeftSide.Add(new Point2d(0, undercut.MaxPoint.Y));
            }
         }
      }

      private void definePtsTopSide()
      {
         ptsTopSide.Add(new Point2d(0, panelBase.Panel.gab.height));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MaxPoint.Y - panelBase.Panel.gab.height)<50)
            {
               double y = panelBase.Panel.gab.height;
               ptsTopSide.Add(new Point2d(undercut.MinPoint.X, y));               
               ptsTopSide.Add(undercut.MinPoint);
               ptsTopSide.Add(new Point2d(undercut.MaxPoint.X, undercut.MinPoint.Y));
               ptsTopSide.Add(new Point2d (undercut.MaxPoint.X, y));
            }
         }
      }

      private void definePtsRightSide()
      {
         ptsRightSide.Add(new Point2d(panelBase.Panel.gab.length, panelBase.Panel.gab.height));
         foreach (var undercut in undercutsExt)
         {
            if (Math.Abs(undercut.MaxPoint.X - panelBase.Panel.gab.length)<50)
            {
               double x = panelBase.Panel.gab.length;
               ptsRightSide.Add(new Point2d (x, undercut.MaxPoint.Y));
               ptsRightSide.Add(new Point2d(undercut.MinPoint.X, undercut.MaxPoint.Y));
               ptsRightSide.Add(undercut.MinPoint);
               ptsRightSide.Add(new Point2d(x, undercut.MinPoint.Y));               
            }
         }
      }

      private void definePtsBotSide()
      {
         ptsBotSide.Add(new Point2d(panelBase.Panel.gab.length, 0));
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

      private Extents2d getExtents(Undercut undercut)
      {
         Point2d ptMin = new Point2d(undercut.posi.X, undercut.posi.Y);
         Point2d ptMax = new Point2d(undercut.posi.X+undercut.width,undercut.posi.Y+undercut.height);
         return new Extents2d(ptMin, ptMax);
      }
   }
}

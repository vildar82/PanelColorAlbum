using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Sheets
{
    /// <summary>
    /// Настройки для видового экрана
    /// </summary>
    public class ViewportPanel
    {
        private readonly double ScaleDefault = 25.0;
        private Point2d ViewCenter { get; set; }
        private bool NeedToScale { get; set; }
        private double ViewHeight { get; set; }
        private bool NeedToChangeScale { get; set; }
        private Viewport vp;
        private Extents3d panelExt;

        public ViewportPanel(Viewport vp, BlockReference blRef, bool isFacadeView)
        {
            this.vp = vp;
            panelExt = getPanelExt(blRef, isFacadeView);
        }

        public void Setup()
        {
            defineOptions();

            if (NeedToChangeScale)
            {
                vp.ViewHeight = ViewHeight;
            }
            vp.ViewCenter = ViewCenter;
            vp.Locked = true;
        }

        private void defineOptions()
        {
            int clearance = 100;

            double aspectRatio = 207.0 / 395.0;
            double viewHeight = 207 * ScaleDefault; // высота видового экрана в ед модели
            double maxWidthPanel = 395 * ScaleDefault; // по ширине видового экрана         
            double maxHeightPanel = 207 * ScaleDefault; // часть видового экрана под панель

            double panelWidth = panelExt.MaxPoint.X - panelExt.MinPoint.X;
            double panelHeight = panelExt.MaxPoint.Y - panelExt.MinPoint.Y;

            double deltaH = panelHeight - maxHeightPanel;
            double deltaW = panelWidth - maxWidthPanel;

            double yViewMax = panelExt.MaxPoint.Y + clearance;

            // панель неумещается по ширине видового экрана  и невписвывается в допустимую высоту        
            if (deltaH > 0 || deltaW > 0)
            {
                NeedToChangeScale = true;
                if (deltaH > deltaW)
                {
                    viewHeight += deltaH / aspectRatio;
                }
                else
                {
                    viewHeight += deltaW * aspectRatio;
                    yViewMax += deltaH > 0 ? deltaH : -deltaH;
                }
                ViewHeight = viewHeight + clearance;
            }
            else
            {
                yViewMax += deltaH > 0 ? deltaH * 0.5 : -deltaH * 0.5;
                yViewMax -= clearance;
            }

            ViewCenter = new Point2d(panelExt.MinPoint.X + (panelExt.MaxPoint.X - panelExt.MinPoint.X) * 0.5,
                                     yViewMax - viewHeight * 0.5);
        }

        /// <summary>
        /// Определение границ блока панели для вида формы или фасада
        /// </summary>            
        private Extents3d getPanelExt(BlockReference blRef, bool isFacadeView)
        {
            Extents3d resVal = new Extents3d();
            string ignoreLayer = isFacadeView ? Settings.Default.LayerDimensionForm : Settings.Default.LayerDimensionFacade;

            var btr = blRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btr)
            {
                var ent = idEnt.GetObject(OpenMode.ForRead, false, true) as Entity;
                if (ent.Layer.Equals(ignoreLayer, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                try
                {
                    var curExt = ent.GeometricExtents;
                    if (!IsEmptyExt(ref curExt))
                    {
                        resVal.AddExtents(curExt);
                    }
                }
                catch { }
            }
            //resVal.TransformBy(blRef.BlockTransform);
            return resVal;
        }

        private static bool IsEmptyExt(ref Extents3d ext)
        {
            if (ext.MinPoint.DistanceTo(ext.MaxPoint) < Tolerance.Global.EqualPoint)
                return true;
            else
                return false;
        }
    }
}
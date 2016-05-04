using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.ChangeJob
{
    /// <summary>
    /// Панель с изменением покраски
    /// </summary>
    public class ChangePanel :IComparable<ChangePanel>
    {
        public static AcadLib.Comparers.AlphanumComparator alphaComparer = AcadLib.Comparers.AlphanumComparator.New;
        public static Tolerance ToleranceAngle = new Tolerance(1, 1);
        public string MarkSb {get; set;}
        public string PaintOld { get; set; }
        public string PaintNew { get; set; }
        /// <summary>
        /// Блок АКР панели
        /// </summary>
        public Panels.Panel PanelAKR { get; set; }
        /// <summary>
        /// Блок монтажной панели
        /// </summary>
        public PanelLibrary.MountingPanel PanelMount { get; set; }
        public Extents3d ExtMountPanel { get; set; }
        //public SectionColumn SecCol { get; set; }
        //public FloorRow FloorRow { get; set; }  
        public bool IsHorizontal { get; private set; }              

        public ChangePanel (Panels.Panel panelAkr, PanelLibrary.MountingPanel panelMount)
        {            
            PanelAKR = panelAkr;
            PanelMount = panelMount;
            MarkSb = PanelMount.MarkSb;
            PaintOld = PanelMount.MarkPainting;
            PaintNew = PanelAKR.MarkAr.MarkPaintingFull;
            ExtMountPanel = PanelMount.ExtBlRef.HasValue ? PanelMount.ExtBlRef.Value : PanelMount.ExtBlRefClean;
            //SecCol = SectionColumn.GetSectionColumn(PanelMount);
            //FloorRow = FloorRow.GetFloorRow(PanelMount);
            var extP = PanelMount.ExtBlRefClean;
            IsHorizontal = (extP.MaxPoint.X - extP.MinPoint.X) > (extP.MaxPoint.Y - extP.MinPoint.Y);
        }

        public override string ToString()
        {
            return MarkSb + ", было '" + PaintOld + "', стало '" + PaintNew + "'";
        }

        public int CompareTo(ChangePanel other)
        {
            return alphaComparer.Compare(MarkSb, other.MarkSb);
        }
    }
}

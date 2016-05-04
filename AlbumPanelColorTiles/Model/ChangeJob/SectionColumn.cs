using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.ChangeJob
{
    public class SectionColumn : IEquatable<SectionColumn>, IComparable<SectionColumn>
    {
        /// <summary>
        /// Координата X столбца секции для вставки монтажных планов
        /// </summary>
        public double X { get; set; }
        public int Section { get; set; }
        public double LengthMax { get; set; }                

        public SectionColumn (MountingPanel panelMount)
        {
            Section = panelMount.Floor.Section;
            LengthMax = panelMount.Floor.PlanExtentsLength;
        }

        public SectionColumn(int sec, double maxLen)
        {
            Section = sec;
            LengthMax = maxLen;
        }

        public static SectionColumn GetSectionColumn(MountingPanel panelMount)
        {
            var sec = ChangeJobService.SecCols.Find(s => s.Section == panelMount.Floor.Section);
            if (sec == null)
            {
                // Нет еще такой секции, добавление
                sec = new SectionColumn(panelMount);
                ChangeJobService.SecCols.Add(sec);
            }
            else
            {
                // Проверка длины и высоты
                var len = panelMount.Floor.PlanExtentsLength;
                if (len > sec.LengthMax) sec.LengthMax = len;                
            }
            return sec;
        }

        public bool Equals(SectionColumn other)
        {
            return Section.Equals(other.Section);
        }

        public int CompareTo(SectionColumn other)
        {
            return Section.CompareTo(other.Section);
        }
    }
}

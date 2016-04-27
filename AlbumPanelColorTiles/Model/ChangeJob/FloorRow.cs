using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.ChangeJob
{
    public class FloorRow : IComparable<FloorRow>
    {
        /// <summary>
        /// Координата Y для вставки монтажного плана этого этажа
        /// </summary>
        public double Y { get; set; }
        public double HeightMax { get; set; }
        public Storey Storey { get; set; }

        public FloorRow(MountingPanel panelMount)
        {            
            Storey = panelMount.Floor.Storey;
            HeightMax = panelMount.Floor.PlanExtentsHeight;
        }

        public static FloorRow GetFloorRow(MountingPanel panelMount)
        {
            var fr = ChangeJobService.FloorRows.Find(f => f.Storey == panelMount.Floor.Storey);
            if (fr == null)
            {
                // Нет еще такого этажа, добавление
                fr = new  FloorRow (panelMount);
                ChangeJobService.FloorRows.Add(fr);
            }
            else
            {
                // Проверка длины и высоты
                var hey = panelMount.Floor.PlanExtentsHeight;
                if (hey > fr.HeightMax) fr.HeightMax = hey;
            }
            return fr;
        }

        public int CompareTo(FloorRow other)
        {
            return Storey.CompareTo(other.Storey);
        }
    }
}

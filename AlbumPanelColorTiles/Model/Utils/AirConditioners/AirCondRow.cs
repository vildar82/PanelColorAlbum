using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Model.Utils.AirConditioners
{
    public class AirCondRow
    {
        public int Mark { get; set; }
        public string ColorName { get; set; }        
        public Color Color { get; set; }
        public int Count { get; set; }
        public List<AirConditioner> AirConds { get; set; }

        public AirCondRow(IGrouping<Color, AirConditioner> groupCond)
        {
            var firstAir = groupCond.First();
            Color = groupCond.Key;
            AirConds = groupCond.ToList();
            Count = AirConds.Count;
            ColorName = firstAir.ColorName;
            Mark = firstAir.Mark;
        }
    }
}

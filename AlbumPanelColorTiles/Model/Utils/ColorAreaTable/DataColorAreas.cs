using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles.Utils.ColorAreaTable
{
    class DataColorAreas
    {
        private List<ColorArea> areas;

        public List<Tuple<ColorArea,int, double>> Colors { get; set; }
        public List<DataRow> Rows { get; set; }
        public Tuple<int, double> Total { get; set; }

        public DataColorAreas (List<ColorArea> areas)
        {
            this.areas = areas;
            Calc();
        }

        private void Calc ()
        {
            Colors = areas.GroupBy(g => g.BlLayer).Select(s=>new Tuple<ColorArea,int, double>(s.First(),s.Count(), s.Sum(a=>a.Area))).ToList();
            Rows = areas.GroupBy(g => g.Size).OrderBy(o => o.First().Height).ThenBy(o=>o.First().Width).
                Select(s => new DataRow(s.First().Size, 
                        s.GroupBy(g => g.BlLayer).OrderBy(o => o.Key).
                        ToDictionary(k=>k.First(), 
                                    v=> v.ToList()), areas.Count)).ToList();
            Total = new Tuple<int, double>(areas.Count, areas.Sum(s => s.Area));
        }        
    }
}

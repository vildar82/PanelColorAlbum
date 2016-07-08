using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.TileTable
{
    /// <summary>
    /// Данные по блокам плитки
    /// </summary>
    public class TileData
    {
        private Dictionary<ObjectId, Color> colors = new Dictionary<ObjectId, Color> ();

        public List<Tile> Tiles { get; set; }
        public List<IGrouping<Tile, Tile>> TileInPanels { get; set; }
        public List<IGrouping<Tile, Tile>> TileInMonolith { get; set; }

        public Color GetColor (ObjectId layerId)
        {
            Color color;
            if (!colors.TryGetValue(layerId, out color))
            {
                var layer = layerId.GetObject( OpenMode.ForRead) as LayerTableRecord;
                color = layer.Color;
                colors.Add(layerId, color);
            }
            return color;
        }

        /// <summary>
        /// Подсчет кол плиток одного цвета
        /// </summary>
        public void Calc ()
        {
            TileInPanels = Tiles.Where(w=>w.IsTileInPanelAkr).GroupBy(g=>g).OrderByDescending(o=>o.Count()).ToList();
            TileInMonolith = Tiles.Where(w => !w.IsTileInPanelAkr).GroupBy(g => g).OrderByDescending(o => o.Count()).ToList();
        }
    }
}

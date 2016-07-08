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
    /// Описание блока плитки
    /// </summary>
    public class Tile : IEquatable<Tile>
    {
        private string blName;
        private string layer;

        public string NCS { get; set; }
        public string Article { get; set; }
        public Color Color { get; set; }
        public bool IsTileInPanelAkr { get; set; }

        public Tile (BlockReference blRef, string blName, string ownerBtrName, Color color)
        {
            bool isPanelAkr = false;
            if (!string.IsNullOrEmpty(ownerBtrName))
            {
                if (Panels.MarkSb.IsBlockNamePanel(ownerBtrName))
                {
                    isPanelAkr = true;
                }
            }
            IsTileInPanelAkr = isPanelAkr;

            this.blName = blName;

            Color = color;

            layer = blRef.Layer;
            string article;
            string ncs;
            Panels.Tile.GetColorNameFromLayer(layer, out article, out ncs);
            NCS = ncs;
            Article = article;
        }

        public bool Equals (Tile other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            var res = layer == other.layer &&
                Color == other.Color;
            return res;
        }        

        public override int GetHashCode ()
        {
            return layer.GetHashCode();
        }
    }
}

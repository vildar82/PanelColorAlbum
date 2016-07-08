using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Panels
{
    // Краска
    public class Paint : IEquatable<Paint>
    {
        // цвет плтитки
        public Color Color { get; private set; }
        // Имя слоя. (для каждой краски свой слой с именем марки краски)
        public string Layer { get; private set; }
        public string Article { get; private set; }
        public string Name { get; private set; }

        public Paint(string layerName, Color color)
        {
            Layer = layerName;
            string article;
            string ncs;
            Tile.GetColorNameFromLayer(layerName, out article, out ncs);
            Article = article;
            Name = ncs;

            Color = color;
        }        

        public bool Equals(Paint other)
        {
            return Layer.Equals(other.Layer) &&
               Color.Equals(other.Color);
        }

        public static bool HasColorName(List<Paint> colors)
        {
            return colors.Any(p => p.Name != string.Empty);
        }
    }
}
using System;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Panels
{
   // Краска
   public class Paint : IEquatable<Paint>
   {
      // цвет плтитки
      private Color _color;

      // Имя слоя. (для каждой краски свой слой с именем марки краски)
      private string _layerName;

      public Paint(string layerName, Color color)
      {
         _layerName = layerName;
         _color = color;
      }

      public Color Color { get { return _color; } }
      public string LayerName { get { return _layerName; } }

      public bool Equals(Paint other)
      {
         return _layerName.Equals(other._layerName) &&
            _color.Equals(other._color);
      }
   }
}
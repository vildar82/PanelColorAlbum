using System;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Panels
{
   // Краска
   public class Paint : IEquatable<Paint>
   {
      // цвет плтитки
      private Color _color;
      // кол плиток этого цвета - для итоговой таблицы плитки
      private int _count;
      // Имя слоя. (для каждой краски свой слой с именем марки краски)
      private string _layerName;

      public Paint(string layerName, Color color)
      {
         _layerName = layerName;
         _color = color;
      }

      public int Count { get { return _count; } }
      public Color Color { get { return _color; } }
      public string LayerName { get { return _layerName; } }

      /// <summary>
      /// Добавление одной плитки с таким цветом
      /// </summary>
      public void AddOneTileCount()
      {
         _count++;
      }

      public bool Equals(Paint other)
      {
         return _layerName.Equals(other._layerName) &&
            _color.Equals(other._color);
      }
   }
}
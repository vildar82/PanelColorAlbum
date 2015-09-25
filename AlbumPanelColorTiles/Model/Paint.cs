using System;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Model
{
   // Краска
   public class Paint : IEquatable<Paint>
   {
      #region Private Fields

      private Color _color;

      // Имя слоя. (для каждой краски свой слой с именем марки краски)
      private string _layerName;

      #endregion Private Fields

      #region Public Constructors

      public Paint(string layerName, Color color)
      {
         _layerName = layerName;
         _color = color;
      }

      #endregion Public Constructors

      #region Public Properties

      public Color Color
      {
         get { return _color; }
      }

      public string LayerName
      {
         get { return _layerName; }
      }

      #endregion Public Properties

      #region Public Methods

      public bool Equals(Paint other)
      {
         return _layerName.Equals(other._layerName) &&
            _color.Equals(other._color);
      }

      #endregion Public Methods
   }
}
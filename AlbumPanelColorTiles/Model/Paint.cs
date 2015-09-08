using Autodesk.AutoCAD.Colors;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Краска
   public class Paint
   {
      // Имя слоя. (для каждой краски свой слой с именем марки краски)
      private string _layerName;

      private Color _color;

      public Paint(string layerName, Color color)
      {
         _layerName = layerName;
         _color = color;
      }

      public string LayerName
      {
         get { return _layerName; }
      }

      public Color Color
      {
         get { return _color; }
      }
   }
}
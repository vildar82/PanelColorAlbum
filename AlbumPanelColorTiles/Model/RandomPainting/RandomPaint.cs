using System.Drawing;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.RandomPainting
{
   public class RandomPaint
   {
      private Color _color;
      private ObjectId _idLayer;
      private string _layerName;
      private int _num;
      private int _percent; // процент от всех плиток всего участка покраски
      private int _tailCount; // Плиток красится этим цветом

      public RandomPaint(string layer, int num, Color color, ObjectId idLayer)
      {
         _num = num;
         _layerName = layer;
         _color = color;
         _idLayer = idLayer;
      }

      public Color Color { get { return _color; } }
      public ObjectId IdLayer { get { return _idLayer; } }
      public string LayerName { get { return _layerName; } }
      public int Num { get { return _num; } }
      public int Percent { get { return _percent; } set { _percent = value; } }
      public int TailCount { get { return _tailCount; } set { _tailCount = value; } }
   }
}
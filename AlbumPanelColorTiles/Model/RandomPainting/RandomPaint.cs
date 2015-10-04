using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model
{
   public class RandomPaint
   {
      private int _num;
      private string _layerName;
      private Color _color;
      private ObjectId _idLayer;      
      private int _percent; // процент от всех плиток всего участка покраски
      private int _tailCount; // Плиток красится этим цветом

      public ObjectId IdLayer { get { return _idLayer; } }
      public Color Color { get { return _color; } }
      public string LayerName { get { return _layerName; } }
      public int Num { get { return _num; } }      
      public int Percent { get { return _percent; } set { _percent = value; } }
      public int TailCount { get { return _tailCount; } set { _tailCount = value; } }

      public RandomPaint(string layer, int num, Color color, ObjectId idLayer)
      {
         _num = num;
         _layerName = layer;
         _color = color;
         _idLayer = idLayer;         
      }
   }
}

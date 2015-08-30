using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Плитка
   public class Tile
   {
      // Краска
      private Paint _paint;
      // Id внутри определения блока панели (марки СБ).
      private ObjectId _idBlRef;
      // Коорд. точки вставки блока плитки в блоке панели.
      private Point3d _insPoint;      

      public Paint Paint
      {
         get { return _paint; }         
      }

      public Point3d InsPoint
      {
         get { return _insPoint; }
         set { _insPoint = value; }
      }

      public Tile(BlockReference blRefTile)
      {
         _idBlRef = blRefTile.ObjectId;
         _insPoint = blRefTile.Position;
      }      
   }
}

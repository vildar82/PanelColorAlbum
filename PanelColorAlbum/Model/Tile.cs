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
      // Id внутри определения блока панели (марки СБ).
      private ObjectId _idBlRef;
      Extents3d _bounds;      

      public Extents3d Bounds
      {
         get { return _bounds; }
      }

      public Tile(BlockReference blRefTile)
      {
         _idBlRef = blRefTile.ObjectId;
         _bounds = blRefTile.Bounds.Value;
      }      
   }
}

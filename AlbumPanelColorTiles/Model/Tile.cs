using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Плитка
   public class Tile
   {
      // Id внутри определения блока панели (марки СБ).
      private ObjectId _idBlRef;

      private Extents3d _bounds;

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
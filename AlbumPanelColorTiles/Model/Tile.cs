using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Плитка
   public class Tile
   {
      private Extents3d _bounds;
      // Id внутри определения блока панели (марки СБ).
      private ObjectId _idBlRef;

      public Tile(BlockReference blRefTile)
      {
         _idBlRef = blRefTile.ObjectId;
         _bounds = blRefTile.Bounds.Value;
      }

      public Extents3d Bounds
      {
         get { return _bounds; }
      }
   }
}
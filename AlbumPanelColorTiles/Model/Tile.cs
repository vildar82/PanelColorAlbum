using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model
{
   // Плитка
   public class Tile : IEquatable<Tile>
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

      public bool Equals(Tile other)
      {
         return _idBlRef.Equals(other._idBlRef) &&
            _bounds.IsEqualTo(other._bounds, Album.Tolerance);
      }
   }
}
using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Panels
{
   // Плитка
   public class Tile : IEquatable<Tile>
   {
      private Extents3d _bounds;
      private Point3d _centerTile;
      private ObjectId _idBlRef;

      // Id внутри определения блока панели (марки СБ).
      public Tile(BlockReference blRefTile)
      {
         _idBlRef = blRefTile.ObjectId;
         _bounds = blRefTile.Bounds.Value;
         _centerTile = new Point3d((_bounds.MaxPoint.X + _bounds.MinPoint.X) * 0.5,
                                   (_bounds.MaxPoint.Y + _bounds.MinPoint.Y) * 0.5, 0);
      }

      public Point3d CenterTile { get { return _centerTile; } }

      public bool Equals(Tile other)
      {
         return _idBlRef.Equals(other._idBlRef) &&
            _bounds.IsEqualTo(other._bounds, Album.Tolerance);
      }
   }
}
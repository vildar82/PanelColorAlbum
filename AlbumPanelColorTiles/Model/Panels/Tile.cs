using System;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RTreeLib;

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
         _bounds = blRefTile.GeometricExtents;
         _centerTile = new Point3d((_bounds.MaxPoint.X + _bounds.MinPoint.X) * 0.5,
                                   (_bounds.MaxPoint.Y + _bounds.MinPoint.Y) * 0.5, 0);
      }

      public Point3d CenterTile { get { return _centerTile; } }

      /// <summary>
      /// Покраска блоков плитки в Модели (без блоков АКР-Панелей)
      /// </summary>
      public static void PaintTileInModel(RTree<ColorArea> rtreeColorAreas)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var btr = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefTile = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  string blName = blRefTile.GetEffectiveName();
                  if (blName.StartsWith(Settings.Default.BlockTileName, StringComparison.OrdinalIgnoreCase))
                  {
                     Tile tile = new Tile(blRefTile);
                     //Определение покраски плитки
                     Paint paint = ColorArea.GetPaint(tile.CenterTile, rtreeColorAreas);
                     if (paint != null)
                     {
                        blRefTile.UpgradeOpen();
                        blRefTile.Layer = paint.LayerName;
                     }
                  }                  
               }
            }
            t.Commit();
         }
      }

      public bool Equals(Tile other)
      {
         return _idBlRef.Equals(other._idBlRef) &&
            _bounds.IsEqualTo(other._bounds, Album.Tolerance);
      }
   }
}
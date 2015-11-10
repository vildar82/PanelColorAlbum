using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Panels
{
   // Панель Марки АР - вхождение блока на чертеже фасада.
   public class PanelAR : IEquatable<PanelAR>
   {
      // Границы блока
      private Extents3d _extents;

      // Вхождение блок Марки АР после выполнения операции замены блоков мрарки СБ на АР (после определения всех Марок Ар).
      private ObjectId _idBlRefAr;

      // Исходное вхождение блока на чертеже (Марки СБ). Которое нужно будет заменить на блок МаркиАР
      private ObjectId _idBlRefSb;

      // Точка вставки блока исходного (Марки СБ)
      private Point3d _insPt;

      private MarkArPanelAR _markAr;

      // Этаж панели
      private Storey _storey;

      public PanelAR(BlockReference blRefPanel, MarkArPanelAR markAr)
      {
         _idBlRefSb = blRefPanel.ObjectId;
         _insPt = blRefPanel.Position;
         _extents = blRefPanel.GeometricExtents;
         _markAr = markAr;
      }

      /// <summary>
      ///  Границы блока по GeometricExtents
      ///  Для границ по плитке используй getExtentsTiles(MarkSbPanel)
      /// </summary>
      public Extents3d Extents { get { return _extents; } }

      public ObjectId IdBlRefAr { get { return _idBlRefAr; } }

      /// <summary>
      /// Точка вставки блока панели
      /// </summary>
      public Point3d InsPt { get { return _insPt; } }

      public MarkArPanelAR MarkAr { get { return _markAr; } }

      public Storey Storey
      {
         get { return _storey; }
         set { _storey = value; }
      }

      public bool Equals(PanelAR other)
      {
         return _insPt.Equals(other._insPt);
      }

      /// <summary>
      /// Определение границ блока по блокам плитки внутри
      /// </summary>
      /// <param name="markSB"></param>
      /// <returns></returns>
      public Extents3d GetExtentsTiles(MarkSbPanelAR markSB)
      {
         // границы в определении блока
         var extTilesBtr = markSB.ExtentsTiles;
         // трансформация границ из BTR в BlRef
         if (_idBlRefAr.IsNull || _idBlRefAr.IsErased || !_idBlRefAr.IsValid)
         {
            return _extents;
         }
         using (var blRef = _idBlRefAr.Open(OpenMode.ForRead) as BlockReference)
         {
            var matrix = blRef.BlockTransform;
            extTilesBtr.TransformBy(matrix);
            _extents = extTilesBtr;
         }
         return extTilesBtr;
      }

      // Замена вхождения блока СБ на АР
      public void ReplaceBlockSbToAr(MarkArPanelAR markAr, Transaction t, BlockTableRecord ms)
      {
         var blRefMarkSb = t.GetObject(_idBlRefSb, OpenMode.ForWrite, false, true) as BlockReference;
         var blRefPanelAr = new BlockReference(blRefMarkSb.Position, markAr.IdBtrAr);
         blRefPanelAr.SetDatabaseDefaults();
         blRefPanelAr.Layer = blRefMarkSb.Layer;
         _extents = blRefPanelAr.GeometricExtents;
         //_insPt = blRefPanelAr.Position;
         blRefMarkSb.Erase(true);
         _idBlRefAr = ms.AppendEntity(blRefPanelAr);
         t.AddNewlyCreatedDBObject(blRefPanelAr, true);

         //Database db = HostApplicationServices.WorkingDatabase;
         //using (var t = db.TransactionManager.StartTransaction())
         //{
         //   var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
         //   var blRefMarkSb = t.GetObject(_idBlRefSb, OpenMode.ForWrite, false, true) as BlockReference;
         //   var blRefPanelAr = new BlockReference(blRefMarkSb.Position, markAr.IdBtrAr);
         //   blRefPanelAr.SetDatabaseDefaults();
         //   blRefPanelAr.Layer = blRefMarkSb.Layer;
         //   _extents = blRefPanelAr.GeometricExtents;
         //   //_insPt = blRefPanelAr.Position;
         //   blRefMarkSb.Erase(true);
         //   _idBlRefAr = ms.AppendEntity(blRefPanelAr);
         //   t.AddNewlyCreatedDBObject(blRefPanelAr, true);
         //   t.Commit();
         //}
      }
   }
}
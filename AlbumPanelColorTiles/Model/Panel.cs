using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model
{
   // Панель Марки АР - вхождение блока на чертеже фасада.
   public class Panel : IEquatable<Panel>
   {
      // Вхождение блок Марки АР после выполнения операции замены блоков мрарки СБ на АР (после определения всех Марок Ар).
      private ObjectId _idBlRefAr;
      // Исходное вхождение блока на чертеже (Марки СБ). Которое нужно будет заменить на блок МаркиАР
      private ObjectId _idBlRefSb;
      // Точка вставки блока исходного (Марки СБ)
      private Point3d _insPt;
      // Этаж панели
      private Storey _storey;
      // Границы блока
      private Extents3d _extents;

      public Panel(BlockReference blRefPanel)
      {
         _idBlRefSb = blRefPanel.ObjectId;
         _insPt = blRefPanel.Position;
         _extents = blRefPanel.GeometricExtents; 
      }

      /// <summary>
      /// Точка вставки блока панели
      /// </summary>
      public Point3d InsPt { get { return _insPt; } }
      public Extents3d Extents { get { return _extents; } }

      public Storey Storey
      {
         get { return _storey; }
         set { _storey = value; }
      }

      public bool Equals(Panel other)
      {
         return _insPt.Equals(other._insPt) &&
            _storey.Equals(other._storey);
      }

      // Замена вхождения блока СБ на АР
      public void ReplaceBlockSbToAr(MarkArPanel markAr)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
            var blRefMarkSb = t.GetObject(_idBlRefSb, OpenMode.ForWrite, false, true) as BlockReference;
            var blRefPanelAr = new BlockReference(blRefMarkSb.Position, markAr.IdBtrAr);            
            blRefPanelAr.SetDatabaseDefaults();
            blRefPanelAr.Layer = blRefMarkSb.Layer;
            _extents = blRefPanelAr.GeometricExtents;
            blRefMarkSb.Erase(true);
            _idBlRefAr = ms.AppendEntity(blRefPanelAr);
            t.AddNewlyCreatedDBObject(blRefPanelAr, true);
            t.Commit();
         }
      }
   }
}
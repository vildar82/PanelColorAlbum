using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Панель Марки АР
   public class Panel
   {
      // Исходное вхождение блока на чертеже. Которое нужно будет заменить на блок МаркиАР
      private ObjectId _idBlRefSb;

      private ObjectId _idBlRefAr;
      private Point3d _insPt;
      private Matrix3d _transform;

      public Panel(BlockReference blRefPanel)
      {
         _idBlRefSb = blRefPanel.ObjectId;
         _insPt = blRefPanel.Position;
         _transform = blRefPanel.BlockTransform;
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
            blRefPanelAr.Layer = "0";
            blRefMarkSb.Erase(true);
            _idBlRefAr = ms.AppendEntity(blRefPanelAr);
            t.AddNewlyCreatedDBObject(blRefPanelAr, true);
            t.Commit();
         }
      }
   }
}
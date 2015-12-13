using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Panels
{
   // Панель Марки АР - вхождение блока на чертеже фасада.
   public class Panel : IEquatable<Panel>
   {
      // Границы блока
      private Extents3d _extents;

      // Вхождение блок Марки АР после выполнения операции замены блоков мрарки СБ на АР (после определения всех Марок Ар).
      private ObjectId _idBlRefAr;

      // Исходное вхождение блока на чертеже (Марки СБ). Которое нужно будет заменить на блок МаркиАР
      private ObjectId _idBlRefSb;

      // Точка вставки блока исходного (Марки СБ)
      private Point3d _insPt;

      private MarkAr _markAr;

      // Этаж панели
      private Storey _storey;

      public Panel(BlockReference blRefPanel, MarkAr markAr)
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

      public Model.Panels.Section Section { get; set; }

      /// <summary>
      /// Точка вставки блока панели
      /// </summary>
      public Point3d InsPt { get { return _insPt; } }

      public MarkAr MarkAr { get { return _markAr; } }

      public Storey Storey
      {
         get { return _storey; }
         set { _storey = value; }
      }

      public bool Equals(Panel other)
      {
         return _insPt.Equals(other._insPt);
      }

      /// <summary>
      /// Определение границ блока по блокам плитки внутри
      /// </summary>
      /// <param name="markSB"></param>
      /// <returns></returns>
      public Extents3d GetExtentsTiles(MarkSb markSB)
      {
         // границы в определении блока
         var extTilesBtr = markSB.ExtentsTiles;
         // трансформация границ из BTR в BlRef
         if (_idBlRefAr.IsNull || _idBlRefAr.IsErased || !_idBlRefAr.IsValid)
         {
            return _extents;
         }
         using (var blRef = _idBlRefAr.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            var matrix = blRef.BlockTransform;
            extTilesBtr.TransformBy(matrix);
            _extents = extTilesBtr;
         }
         return extTilesBtr;
      }

      // Замена вхождения блока СБ на АР
      public void ReplaceBlockSbToAr(MarkAr markAr, Transaction t, BlockTableRecord ms)
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

      public static List<ObjectId> GetPanelsBlRefInModel(Database db)
      {         
         List<ObjectId> ids = new List<ObjectId>();
         using (var t = db.TransactionManager.StartTransaction())
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  if (MarkSb.IsBlockNamePanel(blRef.Name))
                  {
                     ids.Add(idEnt);
                  }
               }
            }
            t.Commit();
         }
         return ids;
      }
   }
}
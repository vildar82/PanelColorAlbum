using System;
using AlbumPanelColorTiles.Properties;
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

      public static void AddMarkToPanelBtr(string panelMark, ObjectId idBtr)
      {
         using (var t = idBtr.Database.TransactionManager.StartTransaction())
         {
            var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            PanelAR.AddMarkToPanelBtr(panelMark, t, btr);
            t.Commit();
         }
      }

      public static void AddMarkToPanelBtr(string panelMark, Transaction t, BlockTableRecord btr)
      {
         // Найти панель марки СБ или АР по имени блока
         foreach (ObjectId idEnt in btr)
         {
            if (idEnt.ObjectClass.Name == "AcDbText")
            {
               var textMark = t.GetObject(idEnt, OpenMode.ForRead, false) as DBText;
               if (textMark.Layer == Settings.Default.LayerMarks)
               {
                  textMark.UpgradeOpen();
                  textMark.Erase(true);
               }
            }
         }
         // Если марки нет, то создаем ее.
         if (panelMark.EndsWith(")"))
         {
            int lastDirectBracket = panelMark.LastIndexOf('(');
            string markSb = panelMark.Substring(0, lastDirectBracket);
            string markAr = panelMark.Substring(lastDirectBracket);
            using (var text = new DBText())
            {
               text.TextString = markAr;
               text.Height = Settings.Default.CaptionPanelTextHeight;
               text.Annotative = AnnotativeStates.False;
               text.Layer = getLayerForMark();
               text.Position = Point3d.Origin;
               btr.UpgradeOpen();
               btr.AppendEntity(text);
               t.AddNewlyCreatedDBObject(text, true);
            }
            using (var text = new DBText())
            {
               text.TextString = markSb;
               text.Height = Settings.Default.CaptionPanelTextHeight;
               text.Annotative = AnnotativeStates.False;
               text.Layer = getLayerForMark();
               text.Position = new Point3d(0, Settings.Default.CaptionPanelSecondTextShift, 0);
               btr.UpgradeOpen();
               btr.AppendEntity(text);
               t.AddNewlyCreatedDBObject(text, true);
            }
         }
         else
         {
            using (var text = new DBText())
            {
               text.TextString = panelMark;
               text.Height = Settings.Default.CaptionPanelTextHeight;
               text.Annotative = AnnotativeStates.False;
               text.Layer = getLayerForMark();
               text.Position = Point3d.Origin;
               btr.UpgradeOpen();
               btr.AppendEntity(text);
               t.AddNewlyCreatedDBObject(text, true);
            }
         }
      }

      // Получение слоя для марок (АР_Марки)
      private static string getLayerForMark()
      {
         Database db = HostApplicationServices.WorkingDatabase;
         // Если уже был создан слой, то возвращаем его. Опасно, т.к. перед повторным запуском команды покраски, могут удалить/переименовать слой марок.
         using (var t = db.TransactionManager.StartTransaction())
         {
            var lt = t.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (!lt.Has(Settings.Default.LayerMarks))
            {
               // Если слоя нет, то он создается.
               var ltrMarks = new LayerTableRecord();
               ltrMarks.Name = Settings.Default.LayerMarks;
               ltrMarks.IsPlottable = false;
               lt.UpgradeOpen();
               lt.Add(ltrMarks);
               t.AddNewlyCreatedDBObject(ltrMarks, true);
            }
            t.Commit();
         }
         return Settings.Default.LayerMarks;
      }
   }
}
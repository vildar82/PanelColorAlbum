using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertCaption
   {
      private PanelBtrExport panelBtr;

      public ConvertCaption(PanelBtrExport panelBtr)
      {
         this.panelBtr = panelBtr;
      }

      // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
      public void Convert(BlockTableRecord btr)
      {
         // Расположение и поворот марки СБ
         if (!panelBtr.IdCaptionMarkSb.IsNull)
         {
            Point3d posMarkSb = new Point3d(panelBtr.ExtentsNoEnd.MinPoint.X + 230, panelBtr.ExtentsNoEnd.MinPoint.Y + 20, 0);
            Extents3d extMarkSb = convertText(panelBtr.IdCaptionMarkSb, 90, posMarkSb);
            Point3d posMarkPaint = new Point3d(posMarkSb.X + 250, posMarkSb.Y + 20, 0);
            Extents3d extPaint = convertText(panelBtr.IdCaptionPaint, 90, posMarkPaint);
            Extents3d extTexts = extMarkSb;
            extTexts.AddExtents(extPaint);
            сreateHatch(extTexts, btr);            
         }
      }

      private Extents3d convertText(ObjectId idDbText, int angle, Point3d pos)
      {
         Extents3d resVal = new Extents3d();
         if (idDbText.IsNull)
         {
            return resVal;
         }
         using (var text = idDbText.GetObject(OpenMode.ForWrite, false, true) as DBText)
         {
            text.TextStyleId = idDbText.Database.GetTextStylePIK();
            // Аннотативность???
            if (panelBtr.HeightByTile >= 2000)
            {
               text.Position = pos;
               double angleRadian = Math.PI * angle / 180.0;
               text.Rotation = angleRadian;
            }
            resVal = text.GeometricExtents;
         }
         return resVal;
      }

      private ObjectId replaceText(ObjectId idBdText, BlockTableRecord btr)
      {
         using (var text = idBdText.GetObject(OpenMode.ForWrite, false, true) as DBText)
         {
            using (var copyText = text.Clone() as DBText)
            {
               text.Erase();
               return btr.AppendEntity(copyText);
            }
         }
      }

      private void сreateHatch(Extents3d extText, BlockTableRecord btr)
      {
         if (extText.Diagonal() < 100)
         {
            return;
         }
         // Отступ контура штриховки от границ текста
         Extents3d ext = new Extents3d(new Point3d(extText.MinPoint.X - 10, extText.MinPoint.Y - 10, 0),
                                       new Point3d(extText.MaxPoint.X + 10, extText.MaxPoint.Y + 10, 0));
         var h = new Hatch();
         h.SetDatabaseDefaults(btr.Database);
         if (!panelBtr.CaptionLayerId.IsNull)
            h.LayerId = panelBtr.CaptionLayerId;         
         h.LineWeight = LineWeight.LineWeight015;
         h.Linetype = SymbolUtilityServices.LinetypeContinuousName;
         h.Color = Color.FromRgb(250, 250, 250);
         h.Transparency = new Transparency(80);
         h.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
         h.PatternScale = 25.0;
         btr.AppendEntity(h);
         var t = btr.Database.TransactionManager.TopTransaction;
         t.AddNewlyCreatedDBObject(h, true);
         h.Associative = true;
         h.HatchStyle = HatchStyle.Normal;

         // Полилиния по контуру текста
         Polyline pl = new Polyline();
         pl.SetDatabaseDefaults(btr.Database);
         pl.LineWeight = LineWeight.LineWeight015;
         pl.Linetype = SymbolUtilityServices.LinetypeContinuousName;
         pl.ColorIndex = 256; // ПоСлою
         if (!panelBtr.CaptionLayerId.IsNull)
            pl.LayerId = panelBtr.CaptionLayerId;
         pl.AddVertexAt(0, ext.MinPoint.Convert2d(), 0, 0, 0);
         pl.AddVertexAt(0, new Point2d(ext.MaxPoint.X, ext.MinPoint.Y), 0, 0, 0);
         pl.AddVertexAt(0, ext.MaxPoint.Convert2d(), 0, 0, 0);
         pl.AddVertexAt(0, new Point2d(ext.MinPoint.X, ext.MaxPoint.Y), 0, 0, 0);
         pl.Closed = true;
                  
         ObjectId idPl = btr.AppendEntity(pl);
         t.AddNewlyCreatedDBObject(pl, true);

         // добавление контура полилинии в гштриховку
         var ids = new ObjectIdCollection();
         ids.Add(idPl);
         h.AppendLoop(HatchLoopTypes.Default, ids);
         h.EvaluateHatch(true);

         // Замена текстов - чтобы они стали поверх штриховки.
         panelBtr.IdCaptionMarkSb = replaceText(panelBtr.IdCaptionMarkSb, btr);
         panelBtr.IdCaptionPaint = replaceText(panelBtr.IdCaptionPaint, btr);
      }
   }
}
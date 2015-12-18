using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertCaption
   {
      PanelBtrExport panelBtr;
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
            Extents3d extMarkSb = convertText(panelBtr.IdCaptionMarkSb, 90, 230, 20);
            Extents3d extPaint = convertText(panelBtr.IdCaptionPaint, 90, 230 + 250, 20);
            Extents3d extTexts = extMarkSb;            
            extTexts.AddExtents(extPaint);
            сreateHatch(extTexts, btr);
         }
      }

      private Extents3d convertText(ObjectId idDbText, int angle, int x, int y)
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
               text.Position = new Point3d(x, y, 0);
               double angleRadian = Math.PI * angle / 180.0;
               text.Rotation = angleRadian;
            }
            resVal = text.GeometricExtents;
         }
         return resVal;
      }

      private void сreateHatch(Extents3d ext, BlockTableRecord btr)
      {
         if (ext.Diagonal()<100)
         {
            return;
         }

         using (var h = new Hatch())
         {
            h.SetDatabaseDefaults(btr.Database);
            if (!panelBtr.CaptionLayerId.IsNull)
               h.LayerId = panelBtr.CaptionLayerId;
            //h.Linetype = SymbolUtilityServices.LinetypeByLayerName;
            h.LineWeight = LineWeight.LineWeight015;
            h.Color = Color.FromRgb(250, 250, 250);
            h.Transparency = new Transparency(80);
            h.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
            h.PatternScale = 25.0;
            btr.AppendEntity(h);
            btr.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(h, true);            
            h.Associative = false;
            h.HatchStyle = HatchStyle.Normal;
            Point2dCollection pts = new Point2dCollection();
            pts.Add(ext.MinPoint.Convert2d());
            pts.Add(new Point2d(ext.MaxPoint.X, ext.MinPoint.Y));
            pts.Add(ext.MaxPoint.Convert2d());
            pts.Add(new Point2d(ext.MinPoint.X, ext.MaxPoint.Y));
            DoubleCollection bulges = new DoubleCollection(4);
            h.AppendLoop(HatchLoopTypes.Default, pts, bulges);
            h.EvaluateHatch(true);
         }
         panelBtr.IdCaptionMarkSb = copyText(panelBtr.IdCaptionMarkSb, btr);
         panelBtr.IdCaptionPaint = copyText(panelBtr.IdCaptionPaint, btr);
      }

      private ObjectId copyText(ObjectId idBdText, BlockTableRecord btr)
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
   }
}

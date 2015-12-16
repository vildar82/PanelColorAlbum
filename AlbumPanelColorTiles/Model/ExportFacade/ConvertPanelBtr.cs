using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   // преобразование определения блока панели
   public class ConvertPanelBtr
   {
      private Database _db;
      public ConvertPanelService Service { get; private set; }

      public ObjectId IdBtr { get; private set; }
      public Extents3d ExtentsByTile { get { return _extentsByTile; } }
      public double HeightByTile { get; private set; }
      public string CaptionMarkSb { get; private set; }
      public string CaptionPaint { get; private set; }
      public ObjectId CaptionLayerId { get; private set; }
      private ObjectId _idCaptionMarkSb;
      private ObjectId _idCaptionPaint;
      public List<Extents3d> Tiles { get; private set; }
      private Extents3d _extentsByTile;

      public ConvertPanelBtr(ConvertPanelService service, ObjectId idBtr)
      {
         Service = service;
         IdBtr = idBtr;
         _db = idBtr.Database;
         Tiles = new List<Extents3d>();
      }

      public void Convert()
      {
         // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
         redefineBlockTile();

         using (var btr = IdBtr.Open(OpenMode.ForWrite) as BlockTableRecord)
         {
            // Итерация по объектам в блоке и выполнение различных операций к элементам
            iterateEntInBlock(btr);

            // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
            convertCaption(btr);

            // Контур панели
            ContourPanel contourPanel = new ContourPanel(btr, this);
            contourPanel.CreateContour();
         }
      }

      private void iterateEntInBlock(BlockTableRecord btr)
      {
         foreach (ObjectId idEnt in btr)
         {
            using (var ent = idEnt.Open(OpenMode.ForRead) as Entity)
            {
               // Удаление лишних объектов (мусора)
               if (deleteWaste(ent)) continue; // Если объект удален, то переход к новому объекту в блоке

               // Если это плитка, то определение размеров панели по габаритам всех плиток
               if (ent is BlockReference && string.Equals(((BlockReference)ent).GetEffectiveName(),
                          Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
               {
                  _extentsByTile.AddExtents(ent.GeometricExtents);
                  Tiles.Add(ent.GeometricExtents);
                  continue;
               }

               // Если это подпись Марки (на слое Марок)
               if (ent is DBText && string.Equals(ent.Layer, Settings.Default.LayerMarks, StringComparison.CurrentCultureIgnoreCase))
               {
                  // Как определить - это текст Марки или Покраски - сейчас Покраска в скобках (). Но вдруг будет без скобок.
                  var textCaption = (DBText)ent;
                  if (textCaption.TextString.StartsWith("("))
                  {
                     CaptionPaint = textCaption.TextString;
                     _idCaptionPaint = idEnt;
                  }
                  else
                  {
                     CaptionMarkSb = textCaption.TextString;
                     _idCaptionMarkSb = idEnt;
                     CaptionLayerId = textCaption.LayerId;
                  }
               }
            }
         }
         // Определение высоты панели
         HeightByTile = ExtentsByTile.MaxPoint.Y - ExtentsByTile.MinPoint.Y;
      }

      // Удаление мусора из блока
      private static bool deleteWaste(Entity ent)
      {
         if (string.Equals(ent.Layer, Settings.Default.LayerDimensionFacade, StringComparison.CurrentCultureIgnoreCase) ||
                           string.Equals(ent.Layer, Settings.Default.LayerDimensionForm, StringComparison.CurrentCultureIgnoreCase))
         {
            ent.UpgradeOpen();
            ent.Erase();
            return true;
         }
         return false;
      }

      // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
      private void redefineBlockTile()
      {
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRExportFacadeFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(Settings.Default.BlockTileName, fileBlocksTemplate,
                              _db, DuplicateRecordCloning.Replace);
            }
            catch
            {
            }
         }
      }

      // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
      private void convertCaption(BlockTableRecord btr)
      {
         // Расположение и поворот марки СБ
         if (!_idCaptionMarkSb.IsNull)
         {
            Extents3d extMarkSb = convertText(_idCaptionMarkSb, 90, 230, 20);
            Extents3d extPaint = convertText(_idCaptionPaint, 90, 230 + 250, 20);
            Extents3d extTexts = new Extents3d();
            extTexts.AddExtents(extMarkSb);
            extTexts.AddExtents(extPaint);
            CreateHatch(extTexts, btr);             
         }         
      }     

      private Extents3d convertText(ObjectId idDbText, int angle, int x, int y)
      {
         using (var text = idDbText.Open(OpenMode.ForWrite, false, true) as DBText)
         {
            text.TextStyleId = idDbText.Database.GetTextStylePIK();
            // Аннотативность???
            text.Position = new Point3d(x, y, 0);
            double angleRadian = Math.PI * angle / 180.0;
            text.Rotation = angleRadian;
            return text.GeometricExtents;
         }
      }

      private void CreateHatch(Extents3d ext, BlockTableRecord btr)
      {
         using (var h = new Hatch())
         {
            h.SetDatabaseDefaults(btr.Database);
            if (!CaptionLayerId.IsNull)
               h.LayerId = CaptionLayerId;
            //h.Linetype = SymbolUtilityServices.LinetypeByLayerName;
            h.LineWeight = LineWeight.LineWeight015;
            h.Color = Color.FromRgb(250, 250, 250);
            h.Transparency = new Transparency(80);
            h.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
            h.PatternScale = 25.0;
            btr.AppendEntity(h);
            //_t.AddNewlyCreatedDBObject(h, true);
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
         copyText(_idCaptionMarkSb, btr);
         copyText(_idCaptionPaint, btr);
      }

      private void copyText(ObjectId idBdText, BlockTableRecord btr)
      {
         using (var text = idBdText.Open( OpenMode.ForWrite, false, true) as DBText)
         {
            using (var copyText = text.Clone() as DBText)
            {
               text.Erase();
               btr.AppendEntity(copyText);
            }
         }
      }      
   }
}

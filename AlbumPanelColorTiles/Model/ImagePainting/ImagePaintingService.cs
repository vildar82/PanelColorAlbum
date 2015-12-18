using System;
using System.Collections.Generic;
using System.Drawing;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.RandomPainting;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using FreeImageAPI;

namespace AlbumPanelColorTiles.ImagePainting
{
   public class ImagePaintingService
   {
      private ColorAreaSpotSize _colorAreaSize;
      private Database _db;
      private Document _doc;
      private FormImageCrop _formImage;
      private ObjectId _idBlRefColorAreaTemplate;
      private ObjectIdCollection _idColCopy;
      private ObjectId _idCS;
      private List<ObjectId> _idsInsertBlRefColorArea;
      private Dictionary<Color, ObjectId> _layersColorArea;

      public ImagePaintingService(Document doc)
      {
         _doc = doc;
         _db = doc.Database;
         _colorAreaSize = new ColorAreaSpotSize(Settings.Default.ImagePaintSpotLength, Settings.Default.ImagePaintSpotHeight, "ImagePainting");
      }

      public ColorAreaSpotSize ColorAreaSize { get { return _colorAreaSize; } }
      public Document Doc { get { return _doc; } }

      public static Bitmap ConvertTo4bpp(System.Drawing.Bitmap img)
      {
         Bitmap res;
         FIBITMAP dib = FreeImage.CreateFromBitmap(img);
         dib = FreeImage.ConvertColorDepth(dib, FREE_IMAGE_COLOR_DEPTH.FICD_04_BPP, true);
         res = FreeImage.GetBitmap(dib);
         FreeImage.UnloadEx(ref dib);
         return res;
      }

      public void Go()
      {
         // Запрос области рисования
         PromptExtents();
         // Форма выбора картинки и рамки обрезки
         if (_formImage == null)
         {
            _formImage = new FormImageCrop(this);
            _formImage.Fire += FormImage_Fire;
         }
         Application.ShowModalDialog(_formImage);
      }

      // Запрос выбора зоны покраски на чертеже
      public void PromptExtents()
      {
         string errMsg = string.Empty;
         Extents3d ext;
         Vector3d len;
         do
         {
            if (errMsg != string.Empty)
            {
               _doc.Editor.WriteMessage("\n{0}", errMsg);
            }
            ext = _doc.Editor.PromptExtents("\nУкажите первый угол зоны покраски", "\nУкажите второй угол зоны покраски");
            len = ext.MaxPoint - ext.MinPoint;
            errMsg = "\nНужно выбрать область больше.";
         } while (len.Length < (Settings.Default.ImagePaintSpotLength + Settings.Default.ImagePaintSpotHeight));
         _colorAreaSize.ExtentsColorArea = ext;
         _idsInsertBlRefColorArea = new List<ObjectId>();
      }

      private void clearPreviusBlocks()
      {
         if (_idsInsertBlRefColorArea != null)
         {
            foreach (ObjectId idBlRef in _idsInsertBlRefColorArea)
            {
               if (!idBlRef.IsNull && idBlRef.IsValid && !idBlRef.IsErased)
               {
                  using (var blRef = idBlRef.GetObject(OpenMode.ForWrite, false, true) as BlockReference)
                  {
                     blRef.Erase(true);
                  }
               }
            }
         }
      }

      private void FormImage_Fire(object sender, EventArgs e)
      {
         try
         {
            Bitmap bitmap = getBitmapForColorArea((Bitmap)sender);
            using (var lockDoc = _doc.LockDocument())
            {
               using (var t = _db.TransactionManager.StartTransaction())
               {
                  // Проверка блока зоны покраски. если нет, то копирование из шаблона с блоками.
                  RandomPaintService.CheckBlockColorAre(_db);

                  clearPreviusBlocks();
                  _idsInsertBlRefColorArea = new List<ObjectId>();

                  // блок шаблона зоны покраски
                  var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
                  var cs = t.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                  _idCS = cs.Id;
                  var btrColorArea = t.GetObject(bt[Settings.Default.BlockColorAreaName], OpenMode.ForRead) as BlockTableRecord;
                  var blRefColorAreaTemplate = new BlockReference(Point3d.Origin, btrColorArea.Id);
                  cs.AppendEntity(blRefColorAreaTemplate);
                  t.AddNewlyCreatedDBObject(blRefColorAreaTemplate, true);
                  _idBlRefColorAreaTemplate = blRefColorAreaTemplate.Id;
                  RandomPaintService.SetDynParamColorAreaBlock(blRefColorAreaTemplate, _colorAreaSize);
                  _idColCopy = new ObjectIdCollection();
                  _idColCopy.Add(_idBlRefColorAreaTemplate);

                  Point3d ptStart = new Point3d(_colorAreaSize.ExtentsColorArea.MinPoint.X, _colorAreaSize.ExtentsColorArea.MaxPoint.Y, 0);
                  _layersColorArea = new Dictionary<Color, ObjectId>();

                  ProgressMeter progressMeter = new ProgressMeter();
                  progressMeter.SetLimit(bitmap.Width * bitmap.Height);
                  progressMeter.Start("Вставка блоков зон покраски");

                  for (int i = 0; i < bitmap.Width * bitmap.Height; i++)
                  {
                     if (HostApplicationServices.Current.UserBreak())
                        break;
                     progressMeter.MeterProgress();
                     int x = i / bitmap.Height;
                     int y = i % bitmap.Height;
                     Point3d position = ptStart.Add(new Vector3d(x * _colorAreaSize.LenghtSpot, -(y + 1) * _colorAreaSize.HeightSpot, 0));
                     insertSpot(position, getLayerId(bitmap.GetPixel(x, y), _layersColorArea));
                  }
                  progressMeter.Stop();
                  blRefColorAreaTemplate.Erase(true);
                  t.Commit();

                  _doc.Editor.Regen();
                  _doc.Editor.WriteMessage("\nГотово");
               }
            }
         }
         catch (System.Exception ex)
         {
            Log.Error(ex, "FormImage_Fire()");
         }
      }

      // Привести размер картинки к размеру зоны покраски
      private Bitmap getBitmapForColorArea(Bitmap cropBitmap)
      {
         Bitmap resBitmap = new Bitmap(cropBitmap, new Size(_colorAreaSize.LenghtSize, _colorAreaSize.HeightSize));
         resBitmap = ConvertTo4bpp(resBitmap);
         return resBitmap;
      }

      private ObjectId getLayerId(Color color, Dictionary<Color, ObjectId> layers)
      {
         ObjectId idLayer;
         if (!layers.TryGetValue(color, out idLayer))
         {
            using (var lt = _db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable)
            {
               LayerTableRecord layer;
               if (lt.Has(color.Name))
               {
                  idLayer = lt[color.Name];
                  using (layer = idLayer.GetObject(OpenMode.ForWrite) as LayerTableRecord)
                  {
                     layer.Color = Autodesk.AutoCAD.Colors.Color.FromColor(color);
                  }
               }
               else
               {
                  using (layer = new LayerTableRecord())
                  {
                     layer.Name = color.Name;
                     lt.UpgradeOpen();
                     idLayer = lt.Add(layer);
                     layer.Color = Autodesk.AutoCAD.Colors.Color.FromColor(color);
                  }
               }
            }
            layers.Add(color, idLayer);
         }
         return idLayer;
      }

      // Вставка ячейки покраски (пока = одной плитке)
      private void insertSpot(Point3d position, ObjectId idLayer)
      {
         IdMapping map = new IdMapping();
         _db.DeepCloneObjects(_idColCopy, _idCS, map, false);
         ObjectId idBlRefCopy = map[_idBlRefColorAreaTemplate].Value;
         if (idBlRefCopy.IsValid && !idBlRefCopy.IsNull)
         {
            using (var blRefSpot = idBlRefCopy.GetObject(OpenMode.ForWrite, false, true) as BlockReference)
            {
               blRefSpot.Position = position;
               blRefSpot.LayerId = idLayer;
               _idsInsertBlRefColorArea.Add(idBlRefCopy);
            }
         }
      }
   }
}
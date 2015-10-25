using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.RandomPainting;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using FreeImageAPI;

namespace AlbumPanelColorTiles.ImagePainting
{
   public class ImagePaintingService
   {
      private Document _doc;
      private Database _db;
      private ColorAreaSpotSize _colorAreaSize;
      private ObjectId _idCS;
      private ObjectId _idBlRefColorAreaTemplate;
      private ObjectIdCollection _idColCopy;
      private List<ObjectId> _idsInsertBlRefColorArea;
      FormImageCrop _formImage;
      private Dictionary<Color, ObjectId> _layersColorArea;

      public ImagePaintingService(Document doc)
      {
         _doc = doc;
         _db = doc.Database;
         _colorAreaSize = new ColorAreaSpotSize(300, 300, "ImagePainting");
      }

      public ColorAreaSpotSize ColorAreaSize { get { return _colorAreaSize; } }
      public Document Doc { get { return _doc; } }

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
                  var btrColorArea = t.GetObject(bt[Album.Options.BlockColorAreaName], OpenMode.ForRead) as BlockTableRecord;
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
                  blRefColorAreaTemplate.Erase(true);
                  t.Commit();                  

                  progressMeter.Stop();
                  _doc.Editor.Regen();
               }
            }
         }
         catch (System.Exception ex)
         {
            Log.Error(ex, "FormImage_Fire()");
         }
      }      

      private ObjectId getLayerId(Color color, Dictionary<Color, ObjectId> layers)
      {
         ObjectId idLayer;
         if (!layers.TryGetValue(color, out idLayer))
         {
            using (var lt = _db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable)
            {
               LayerTableRecord layer;               
               if (lt.Has (color.Name ))
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

      // Привести размер картинки к размеру зоны покраски
      private Bitmap getBitmapForColorArea(Bitmap cropBitmap)
      {
         Bitmap resBitmap = new Bitmap(cropBitmap, new Size(_colorAreaSize.LenghtSize, _colorAreaSize.HeightSize));         
         resBitmap = ConvertTo4bpp(resBitmap);
         return resBitmap;
      }
      public static Bitmap ConvertTo4bpp(System.Drawing.Bitmap img)
      {
         Bitmap res;
         FIBITMAP dib = FreeImage.CreateFromBitmap(img);
         dib = FreeImage.ConvertColorDepth(dib, FREE_IMAGE_COLOR_DEPTH.FICD_04_BPP, true);         
         res = FreeImage.GetBitmap(dib);
         FreeImage.UnloadEx(ref dib);
         return res;
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
            ext = Lib.UserPrompt.PromptExtents(_doc.Editor, "\nУкажите первый угол зоны покраски", "\nУкажите второй угол зоны покраски");
            len = ext.MaxPoint - ext.MinPoint;
            errMsg = "\nНужно выбрать область побольше.";
         } while (len.Length < 600);
         _colorAreaSize.ExtentsColorArea = ext;
         _idsInsertBlRefColorArea = new List<ObjectId>();
      }

      // Вставка ячейки покраски (пока = одной плитке)
      private void insertSpot(Point3d position, ObjectId idLayer)
      {         
         IdMapping map = new IdMapping();
         _db.DeepCloneObjects(_idColCopy, _idCS, map, false);
         ObjectId idBlRefCopy = map[_idBlRefColorAreaTemplate].Value;
         if (idBlRefCopy.IsValid && !idBlRefCopy.IsNull)
         {
            using (var blRefSpot = idBlRefCopy.GetObject( OpenMode.ForWrite) as BlockReference)
            {
               blRefSpot.Position = position;
               blRefSpot.LayerId = idLayer;
               _idsInsertBlRefColorArea.Add(idBlRefCopy);
            }
         }
      }
   }
}

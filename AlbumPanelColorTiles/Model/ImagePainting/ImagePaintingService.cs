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

      public ImagePaintingService(Document doc)
      {
         _doc = doc;
         _db = doc.Database;
         _colorAreaSize = new ColorAreaSpotSize(300, 300, "ImagePainting");
      }

      public ColorAreaSpotSize ColorAreaSize { get { return _colorAreaSize; } }

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
         Bitmap bitmap = getBitmapForColorArea ((Bitmap)sender);
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

               Dictionary<Color, ObjectId> _layers = new Dictionary<Color, ObjectId>();

               for (int i = 0; i < bitmap.Width * bitmap.Height; i++)
               {
                  int x = i / bitmap.Height;
                  int y = i % bitmap.Height;                  
                  Point3d position = ptStart.Add(new Vector3d(x * _colorAreaSize.LenghtSpot, -y * _colorAreaSize.HeightSpot, 0));
                  insertSpot(position, getLayerId(bitmap.GetPixel(x, y), _layers));
               }
               blRefColorAreaTemplate.Erase(true);
               t.Commit();
            }
         }
      }

      private ObjectId getLayerId(Color color, Dictionary<Color, ObjectId> _layers)
      {
         ObjectId idLayer;
         if (!_layers.TryGetValue(color, out idLayer))
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
                     layer.Color = Autodesk.AutoCAD.Colors.Color.FromColor(color);
                     lt.UpgradeOpen();
                     idLayer = lt.Add(layer);                     
                  }
               }               
            }
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
         resBitmap = ConvertTo8bpp(resBitmap);
         return resBitmap;
      }
      public static Bitmap ConvertTo8bpp(System.Drawing.Bitmap img)
      {
         Bitmap res;
         FIBITMAP dib = FreeImage.CreateFromBitmap(img);
         dib = FreeImage.ConvertColorDepth(dib, FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP, true);
         res = FreeImage.GetBitmap(dib);
         FreeImage.UnloadEx(ref dib);
         return res;
      }

      // Запрос выбора зоны покраски на чертеже
      public void PromptExtents()
      {
         Extents3d ext = Lib.UserPrompt.PromptExtents(_doc.Editor, "Укажите первый угол зоны покраски", "Укажите второй угол зоны покраски");         
         _colorAreaSize.ExtentsColorArea = ext; 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model
{
   // Произвольная покраска участа с % распределением цветов
   public class RandomPainting
   {
      private Document _doc;
      private Editor _ed;
      private Database _db;
      private Extents3d _extentsPrompted; // зона произвольной покраски
      private Random _rnd;
      private int _xsize; // кол столбцов участков покраски в зоне покраски. Ячейка = Spot (пока равна одной плитке, но потом можно будет задать любой размер кратный плитке).
      private int _ysize; // кол рядов участков в зоне покраски      
      private ObjectId _idMS;      
      private ObjectId _idBlRefColorAreaTemplate;
      private ObjectIdCollection _idColCopy;
      private List<Spot> _spots;

      public RandomPainting()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _ed = _doc.Editor;
         _db = _doc.Database;
         _rnd = new Random();
         _spots = new List<Spot>();
      }

      public void Start()
      {
         // Проверка наличия блока зоны покраски
         checkBlock(_db);

         // Запрос области для покраски            
         PromptExtents();

         // Список всех цветов (по списку листов) - которые можно добавить в распределение покраски зоны
         Dictionary<string, RandomPaint> allProperPaint = getAllProperPaints();

         // Форма для распределения цветов
         FormRandomPainting formProper = new FormRandomPainting(allProperPaint);
         formProper.Fire += FormProper_Fire;
         Application.ShowModalDialog(formProper);         
      }

      // Огонь
      private void FormProper_Fire(object sender, EventArgs e)
      {
         // Удаление предыдущей покраски
         if (_spots.Count>0)
         {
            deleteSpots(_spots);
         }       

         // Красим участок
         Dictionary<string, RandomPaint> trackPropers = sender as Dictionary<string, RandomPaint>;
         List<RandomPaint> propers = trackPropers.Values.ToList(); 
         _xsize = Convert.ToInt32((_extentsPrompted.MaxPoint.X - _extentsPrompted.MinPoint.X) / 300);
         _ysize = Convert.ToInt32((_extentsPrompted.MaxPoint.Y - _extentsPrompted.MinPoint.Y) / 100);
         int totalTileCount = _xsize * _ysize;
         int distributedCount = 0;
         int emptySpotsCount =0;         
         foreach (var proper in propers)
         {
            proper.TailCount = Convert.ToInt32(proper.Percent * totalTileCount / 100d);
            distributedCount += proper.TailCount;            
         }         

         if (distributedCount > totalTileCount)
         {
            RandomPaint lastProper = propers.Last();
            lastProper.TailCount -= distributedCount - totalTileCount;
         }
         else
         {
            emptySpotsCount = totalTileCount - distributedCount;
         }

         // Получение общего списка распределения покроаски
         _spots = new List<Spot>();
         foreach (var proper in propers)
         {
            _spots.AddRange(Spot.GetSpots(proper));
         }
         // Пустые споты
         _spots.AddRange(Spot.GetEmpty(emptySpotsCount));

         // Перемешивание списка
         mixingList(_spots);

         // Вставка блоков зон 
         placementSpots(_spots);

         _ed.Regen(); 
      }

      private void deleteSpots(List<Spot> spots)
      {
         using (DocumentLock ld = _doc.LockDocument())
         {
            using (var t = _db.TransactionManager.StartTransaction())
            {
               foreach (var spot in spots)
               {
                  if (spot != null)
                  {
                     if (!spot.IdBlRef.IsNull)
                     {                      
                        var blRef = t.GetObject(spot.IdBlRef, OpenMode.ForWrite) as BlockReference;
                        blRef.Erase(true);
                     }                     
                  }
               }
               t.Commit();
            }
         }
      }

      private void placementSpots(List<Spot> spots)
      {
         using (var lockdoc = _doc.LockDocument())
         {
            using (var t = _db.TransactionManager.StartTransaction())
            {
               var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
               var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
               _idMS = ms.Id;
               var btrColorArea = t.GetObject(bt[Album.Options.BlockColorAreaName], OpenMode.ForRead) as BlockTableRecord;               
               var blRefColorAreaTemplate = new BlockReference(Point3d.Origin, btrColorArea.Id);
               ms.AppendEntity(blRefColorAreaTemplate);
               t.AddNewlyCreatedDBObject(blRefColorAreaTemplate, true);
               _idBlRefColorAreaTemplate = blRefColorAreaTemplate.Id;               
               setDynParamColorAreaBlock(blRefColorAreaTemplate);
               _idColCopy = new ObjectIdCollection();
               _idColCopy.Add(_idBlRefColorAreaTemplate);

               List<Spot> insertSpotsTest = new List<Spot>();

               int x, y;

               for (int i = 0; i < spots.Count; i++)
               {
                  Spot spot = spots[i];
                  if (spot != null)
                  {
                     x = i / _ysize;
                     y = i % _ysize;
                     insertSpot(spot, x, y, t);
                     insertSpotsTest.Add(spot);
                  }
               }
                              
               blRefColorAreaTemplate.Erase(true);
               t.Commit();
            }
         }
      }

      private void setDynParamColorAreaBlock(BlockReference blRefcolorAreaSpot)
      {
         foreach (DynamicBlockReferenceProperty item in blRefcolorAreaSpot.DynamicBlockReferencePropertyCollection )
         {
            if (string.Equals(item.PropertyName, "Длина", StringComparison.InvariantCultureIgnoreCase))
               item.Value = 300d;
            else if (string.Equals(item.PropertyName, "Высота", StringComparison.InvariantCultureIgnoreCase))
               item.Value = 100d;
         }
      }

      // Вставка ячейки покраски (пока = одной плитке)
      private void insertSpot(Spot spot, int x, int y, Transaction t)
      {
         Point3d position = new Point3d(_extentsPrompted.MinPoint.X + x * 300, _extentsPrompted.MinPoint.Y + y * 100, 0);
         IdMapping map = new IdMapping();
         _db.DeepCloneObjects(_idColCopy, _idMS, map, false);
         var blRefSpot = t.GetObject(map[_idBlRefColorAreaTemplate].Value, OpenMode.ForWrite) as BlockReference;
         blRefSpot.Position = position;
         blRefSpot.LayerId = spot.Proper.IdLayer;         
         spot.IdBlRef = blRefSpot.Id;
      }

      private void mixingList(List<Spot> spots)
      {
         Spot temp;
         var count = spots.Count;
         for (int i = 0; i < count; i++)
         {
            int number = _rnd.Next(count);
            temp = spots[number];
            spots.RemoveAt(number);
            spots.Insert(0, temp);
         }
      }

      private Dictionary<string, RandomPaint> getAllProperPaints()
      {
         Dictionary<string, RandomPaint> propers = new Dictionary<string, RandomPaint>();
         int numProper = 0;
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var lt = _db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            foreach (ObjectId idLayer in lt)
            {
               var layer = idLayer.GetObject(OpenMode.ForRead) as LayerTableRecord;
               RandomPaint proper = new RandomPaint(layer.Name, numProper++, layer.Color.ColorValue, layer.Id);
               propers.Add(proper.LayerName, proper);
            }
         }
         return propers;
      }

      private void PromptExtents()
      {
         var prPtRes = _ed.GetPoint("Укажите первую точку зоны произвольной покраски");
         if (prPtRes.Status == PromptStatus.OK)
         {
            var prCornerRes = _ed.GetCorner("Укажите вторую точку зоны произвольной покраски", prPtRes.Value);
            if (prCornerRes.Status == PromptStatus.OK)
            {
               _extentsPrompted = new Extents3d();
               _extentsPrompted.AddPoint(prPtRes.Value);
               _extentsPrompted.AddPoint(prCornerRes.Value);                
            }
         }
         var dist = _extentsPrompted.MaxPoint - _extentsPrompted.MinPoint;
         if (dist.Length < 650)
         {
            throw new Exception("Указана слишком маленькая зона.");
         }
      }

      private void checkBlock(Database db)
      {
         using (var t = db.TransactionManager.StartTransaction())
         { 
            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
         
            if (!bt.Has (Album.Options.BlockColorAreaName))
            {
               // Скопировать из шаблона
               BlockInsert.CopyBlockFromTemplate(Album.Options.BlockColorAreaName, db);
            }
            t.Commit();
         }
      }
   }
}

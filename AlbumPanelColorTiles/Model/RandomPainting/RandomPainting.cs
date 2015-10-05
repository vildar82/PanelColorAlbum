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
      private Dictionary<string, RandomPaint> _trackRandoms; // Распределяемые цвета

      public Editor Ed { get { return _ed; } }      

      public RandomPainting()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _ed = _doc.Editor;
         _db = _doc.Database;
         _rnd = new Random();         
      }

      public void Start()
      {
         resetData();

         // Проверка наличия блока зоны покраски
         checkBlock(_db);

         // Запрос области для покраски            
         PromptExtents();

         // Список всех цветов (по списку листов) - которые можно добавить в распределение покраски зоны
         Dictionary<string, RandomPaint> allProperPaint = getAllProperPaints();

         // Форма для распределения цветов
         FormRandomPainting formProper = new FormRandomPainting(allProperPaint, this, _trackRandoms);
         formProper.Fire += FormProper_Fire;
         Application.ShowModalDialog(formProper);
         _trackRandoms = formProper.TrackPropers;
      }

      private void resetData()
      {
         _spots = new List<Spot>();         
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

         // Перемешивание списка
         List<Spot> mixSpots = mixingList(_spots, totalTileCount);

         // Вставка блоков зон 
         placementSpots(mixSpots);

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
                     if (!spot.IdBlRef.IsNull && spot.IdBlRef.IsValid && !spot.IdBlRef.IsErased)
                     {                      
                        var blRef = t.GetObject(spot.IdBlRef, OpenMode.ForWrite, false, true) as BlockReference;
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

               int x, y;
               foreach (var spot in spots)
               {                  
                  if (spot != null)
                  {
                     int i = spots.IndexOf(spot);
                     x = i / _ysize;
                     y = i % _ysize;                     
                     insertSpot(spot, x, y, t);                     
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

      private List<Spot> mixingList(List<Spot> spotsReal, int totalCount)
      {
         //List<Spot> mixSpots = Spot.GetEmpty(totalCount);                  
         Spot[] mixSpots = new Spot[totalCount];
         Spot temp;
         foreach (var spot in spotsReal)
         {
            int number = _rnd.Next(totalCount - 1);
            do
            {
               temp = mixSpots[number];
               if (temp == null)
               {
                  mixSpots[number] = spot;
               }
               else
               {
                  number++;
                  if (number >= totalCount)
                  {
                     number = _rnd.Next(totalCount - 1);
                  }
               }               
            } while (temp!=null);
         }
         return mixSpots.ToList();
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

      public void PromptExtents()
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

               _spots = new List<Spot>();
            }
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

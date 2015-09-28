using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.Model
{
   // Панели марки СБ
   public class MarkSbPanel : IEquatable<MarkSbPanel>
   {
      #region Private Fields

      private string _abbr;

      private Point2d _centerPanel;

      // зоны покраски внутри определения блока (приоритет выше чем у зон в модели).
      private List<ColorArea> _colorAreas;

      private ObjectId _idBtr;
      private bool _isEndLeftPanel;
      private bool _isEndRightPanel;
      private bool _isUpperStoreyPanel;
      private List<MarkArPanel> _marksAR;

      private string _markSb; // может быть с _тп или _тл
      private string _markSbBlockName;
      private string _markSbClean; // без _тп или _тл
      private List<Paint> _paints;

      // Список плиток в панели Марки СБ
      private List<Tile> _tiles;

      #endregion Private Fields

      #region Private Constructors

      // Конструктор. Скрытый.
      private MarkSbPanel(BlockReference blRefPanel, ObjectId idBtrMarkSb, string markSbName, string markSbBlockName, string abbr)
      {
         _abbr = abbr;
         _markSb = markSbName;
         _idBtr = idBtrMarkSb;
         _markSbBlockName = markSbBlockName;
         _isUpperStoreyPanel = (blRefPanel.Layer == Album.Options.LayerUpperStoreyPanels);
         _isEndLeftPanel = markSbName.EndsWith(Album.Options.endLeftPanelSuffix);
         _isEndRightPanel = markSbName.EndsWith(Album.Options.endRightPanelSuffix);
         _marksAR = new List<MarkArPanel>();
         _colorAreas = ColorAreaModel.GetColorAreas(_idBtr);
         //TODO: Проверка пересечений зон покраски (не должно быть пересечений). Пока непонятно как сделать.
         //???

         // Список плиток (в определении блока марки СБ)
         GetTiles();

         // Центр панели
         _centerPanel = GetCenterPanel(_tiles);
      }

      #endregion Private Constructors

      #region Public Properties

      public string Abbr { get { return _abbr; } }

      public Point2d CenterPanel { get { return _centerPanel; } }

      public ObjectId IdBtr { get { return _idBtr; } }

      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }

      public bool IsEndRightPanel { get { return _isEndRightPanel; } }

      /// <summary>
      /// Это панель чердака? true - да, false - нет.
      /// </summary>
      public bool IsUpperStoreyPanel { get { return _isUpperStoreyPanel; } }

      public List<MarkArPanel> MarksAR { get { return _marksAR; } }

      public string MarkSb { get { return _markSb; } }

      public string MarkSbBlockName { get { return _markSbBlockName; } }

      public string MarkSbClean
      {
         get
         {
            if (_markSbClean == null)
            {
               if (_isEndLeftPanel || _isEndRightPanel)
               {
                  _markSbClean = _markSb.Substring(0, _markSb.Length - 3);
               }
               else
               {
                  _markSbClean = _markSb;
               }
            }
            return _markSbClean;
         }
      }

      public List<Paint> Paints { get { return _paints; } }

      // Свойства
      public List<Tile> Tiles { get { return _tiles; } }

      // Суммарная площадь плитки на панель (расход м2 на панель).
      public double TotalAreaTiles
      {
         get
         {
            return Math.Round(_paints.Count * TileCalc.OneTileArea, 2);
         }
      }

      #endregion Public Properties

      #region Public Methods

      // Создание определения блока марки СБ из блока марки АР, и сброс покраски плитки (в слой 0)
      public static void CreateBlockMarkSbFromAr(ObjectId idBtrMarkAr, string markSbBlName)
      {
         var idBtrMarkSb = Lib.Blocks.CopyBtr(idBtrMarkAr, markSbBlName);
         // Перенос блоков плиток на слой 0
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var btrMarkSb = t.GetObject(idBtrMarkSb, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as BlockReference;
                  if (Blocks.EffectiveName(blRef) == Album.Options.BlockTileName)
                  {
                     blRef.Layer = "0";
                  }
               }
            }
            t.Commit();
         }
      }

      public static string GetMarkSbBlockName(string markSb)
      {
         return Album.Options.BlockPanelPrefixName + markSb;
      }

      // Определение марки СБ (может быть с суффиксом торца _тл или _тп). Отбрасывается последняя часть имени в скобках (это марка АР).
      public static string GetMarkSbName(string blName)
      {
         string markSb = string.Empty;
         if (IsBlockNamePanel(blName))
         {
            // Хвостовая часть
            markSb = blName.Substring(Album.Options.BlockPanelPrefixName.Length);
            if (markSb.EndsWith(")"))
            {
               int lastDirectBracket = markSb.LastIndexOf('(');
               if (lastDirectBracket > 0)
               {
                  markSb = markSb.Substring(0, lastDirectBracket);
               }
            }
         }
         return markSb;
      }

      // Определение покраски панелей текущего чертежа (в Модели)
      public static List<MarkSbPanel> GetMarksSB(ColorAreaModel colorAreaModel, string abbr)
      {
         List<MarkSbPanel> _marksSb = new List<MarkSbPanel>();
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            // Перебор всех блоков в модели и составление списка блоков марок и панелей.
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

            ProgressMeter progressMeter = new ProgressMeter();
            progressMeter.SetLimit(5000);
            progressMeter.Start("Покраска панелей...");

            foreach (ObjectId idEnt in ms)
            {
               progressMeter.MeterProgress();
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefPanel = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  // Определение Марки СБ панели. Если ее еще нет, то она создается и добавляется в список _marks.
                  MarkSbPanel markSb = GetMarkSb(blRefPanel, _marksSb, bt, abbr);
                  if (markSb == null)
                  {
                     // Значит это не блок панели. Пропускаем.
                     continue;
                  }
                  //Определение покраски панели (Марки АР)
                  List<Paint> paintAR = MarkArPanel.GetPanelMarkAR(markSb, blRefPanel, colorAreaModel.ColorAreasForeground, colorAreaModel.ColorAreasBackground);
                  // Добавление панели АР в список панелей для Марки СБ
                  markSb.AddPanelAR(paintAR, blRefPanel, markSb);
               }
            }
            progressMeter.Stop();
            t.Commit();
         }
         return _marksSb;
      }

      /// <summary>
      /// Возвращает марку панели из имени блока панели (для панелей Марки СБ и Марок АР).
      /// </summary>
      /// <param name="blName">Имя блока панели</param>
      /// <returns>марка панели (СБ или СБ+АР)</returns>
      public static string GetPanelMarkFromBlockName(string blName, List<MarkSbPanel> marksSB)
      {
         //return blName.Substring(Album.Options.BlockPanelPrefixName.Length);
         string panelMark = string.Empty;
         // Найти имя марки панели СБ или АР
         foreach (var markSB in marksSB)
         {
            if (markSB.MarkSbBlockName == blName)
            {
               return markSB.MarkSbClean;
            }
            else
            {
               foreach (var markAR in markSB.MarksAR)
               {
                  if (markAR.MarkArBlockName == blName)
                  {
                     return markAR.MarkARPanelFullName;
                  }
               }
            }
         }
         return panelMark;
      }

      // Проверка, это блок панели, по имени (Марки СБ или Марки АР)
      public static bool IsBlockNamePanel(string blName)
      {
         return blName.StartsWith(Album.Options.BlockPanelPrefixName);
      }

      // Проверка, это имя блоки марки АР, по имени блока.
      public static bool IsBlockNamePanelMarkAr(string blName)
      {
         bool res = false;
         if (IsBlockNamePanel(blName))
         {
            string markSb = GetMarkSbName(blName); // может быть с суффиксом торца _тп или _тл
            string markSbBlName = GetMarkSbBlockName(markSb);// может быть с суффиксом торца _тп или _тл
            res = blName != markSbBlName;
         }
         return res;
      }

      // Определение архитектурных Марок АР (Э1_Яр1)
      // Жуткий вид. ??? Переделать!!!
      public void DefineArchitectMarks(List<MarkSbPanel> marksSB)
      {
         Dictionary<string, MarkArPanel> marksArArchitectIndex = new Dictionary<string, MarkArPanel>();
         if (IsUpperStoreyPanel)
         {
            // Панели чердака
            // (ЭЧ-#_Яр1)
            if (MarksAR.Count == 1)
            {
               // Если одна марка покраски
               string markPaint = "ЭЧ";
               marksArArchitectIndex.Add(markPaint, MarksAR[0]);
               MarksAR[0].MarkPainting = markPaint;
            }
            else
            {
               // Если несколько марок покраски
               int i = 1;
               foreach (var markAR in MarksAR)
               {
                  string markPaint = string.Format("ЭЧ-{0}", i++);
                  marksArArchitectIndex.Add(markPaint, markAR);
                  markAR.MarkPainting = markPaint;
               }
            }
         }
         else if (IsEndRightPanel || IsEndLeftPanel)
         {
            // Торцевые панели (Э1ТЛ_Яр1)
            string endIndex;
            if (IsEndLeftPanel)
               endIndex = "ТЛ";
            else
               endIndex = "ТП";
            // Панели этажей
            int i = 1;
            foreach (var markAR in MarksAR)
            {
               string markPaint;
               var floors = markAR.Panels.GroupBy(p => p.Storey.Number).Select(p => p.First().Storey.Number);
               string floor = String.Join(",", floors);
               markPaint = string.Format("Э{0}{1}", floor, endIndex);
               if (marksArArchitectIndex.ContainsKey(markPaint))
               {
                  markPaint = string.Format("Э{0}{1}-{2}", floor, endIndex, i++);
               }
               marksArArchitectIndex.Add(markPaint, markAR);
               markAR.MarkPainting = markPaint;
            }
         }
         else
         {
            // Панели этажей
            int i = 1;
            foreach (var markAR in MarksAR)
            {
               string markPaint;
               var floors = markAR.Panels.GroupBy(p => p.Storey.Number).Select(p => p.First().Storey.Number);
               string floor = String.Join(",", floors);
               markPaint = string.Format("Э{0}", floor);
               if (marksArArchitectIndex.ContainsKey(markPaint))
               {
                  markPaint = string.Format("Э{0}-{1}", floor, i++);
               }
               marksArArchitectIndex.Add(markPaint, markAR);
               markAR.MarkPainting = markPaint;
            }
         }
      }

      public bool Equals(MarkSbPanel other)
      {
         return _markSb.Equals(other._markSb) &&
            _colorAreas.SequenceEqual(other._colorAreas) &&
            _idBtr.Equals(other._idBtr) &&
            _paints.SequenceEqual(other._paints) &&
            _tiles.SequenceEqual(other._tiles) &&
            _marksAR.SequenceEqual(other._marksAR);
      }

      // Замена вхождений блоков СБ на АР
      public void ReplaceBlocksSbOnAr(Transaction t, BlockTableRecord ms)
      {
         foreach (var markAr in _marksAR)
         {
            markAr.ReplaceBlocksSbOnAr(t, ms);
         }
      }

      #endregion Public Methods

      #region Private Methods

      // Определение марки СБ, если ее еще нет, то создание и добавление в список marks.
      private static MarkSbPanel GetMarkSb(BlockReference blRefPanel, List<MarkSbPanel> marksSb, BlockTable bt, string abbr)
      {
         MarkSbPanel markSb = null;
         if (IsBlockNamePanel(blRefPanel.Name))
         {
            string markSbName = GetMarkSbName(blRefPanel.Name);
            if (markSbName != string.Empty)
            {
               // Поиск панели Марки СБ в коллекции панелей по имени марки СБ.
               markSb = marksSb.Find(m => m._markSb == markSbName);
               if (markSb == null)
               {
                  // Блок Марки СБ
                  Database db = HostApplicationServices.WorkingDatabase;
                  string markSbBlName = GetMarkSbBlockName(markSbName);
                  if (bt.Has(markSbBlName))
                  {
                     var idMarkSbBtr = bt[markSbBlName];
                     markSb = new MarkSbPanel(blRefPanel, idMarkSbBtr, markSbName, markSbBlName, abbr);
                     marksSb.Add(markSb);
                  }
                  else
                  {
                     //TODO: Ошибка в чертеже. Блок с Маркой АР есть, а блока Марки СБ нет. Добавить в колекцию блоков с ошибками.
                     //???
                  }
               }
            }
         }
         return markSb;
      }

      // Добавление панели АР по списку ее покраски
      private void AddPanelAR(List<Paint> paintAR, BlockReference blRefPanel, MarkSbPanel markSb)
      {
         // Проверка нет ли уже такой марки покраси АР
         MarkArPanel panelAR = HasPanelAR(paintAR);
         if (panelAR == null)
         {
            panelAR = new MarkArPanel(paintAR, markSb, blRefPanel);
            _marksAR.Add(panelAR);
         }
         panelAR.AddBlockRefPanel(blRefPanel);
      }

      // Определение центра панели по блокам плиток в ней
      private Point2d GetCenterPanel(List<Tile> _tiles)
      {
         Extents3d ext = new Extents3d();
         foreach (var tile in _tiles)
         {
            ext.AddPoint(tile.CenterTile);
         }
         return new Point2d((ext.MinPoint.X + ext.MaxPoint.X) * 0.5, (ext.MinPoint.Y + ext.MaxPoint.Y) * 0.5);
      }

      private string GetMarkArNextName()
      {
         return "АР-" + _marksAR.Count.ToString();
      }

      // Получение списка плиток в определении блока
      private void GetTiles()
      {
         _tiles = new List<Tile>();
         _paints = new List<Paint>();
         using (var t = _idBtr.Database.TransactionManager.StartTransaction())
         {
            var btrMarkSb = t.GetObject(_idBtr, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefTile = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  if (Blocks.EffectiveName(blRefTile) == Album.Options.BlockTileName)
                  {
                     Tile tile = new Tile(blRefTile);
                     //Определение покраски плитки
                     Paint paint = ColorArea.GetPaint(tile.CenterTile, _colorAreas, null);
                     _tiles.Add(tile);
                     _paints.Add(paint);
                  }
               }
            }
            t.Commit();
         }
      }

      // Поиск покраски марки АР в списке _marksAR
      private MarkArPanel HasPanelAR(List<Paint> paintAR)
      {
         //Поиск панели АР по покраске
         MarkArPanel resPanelAR = null;
         //Сравнение списков покраски
         foreach (MarkArPanel panelAR in _marksAR)
         {
            if (panelAR.EqualPaint(paintAR))
            {
               resPanelAR = panelAR;
               break;
            }
         }
         return resPanelAR;
      }

      #endregion Private Methods
   }
}
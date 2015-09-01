using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Панели марки СБ
   public class MarkSbPanel
   {
      // зоны покраски внутри определения блока (приоритет выше чем у зон в модели).
      private List<ColorArea> _colorAreas;
      private ObjectId _idBtr;
      private List<MarkArPanel> _marksAR;
      private string _markSb;
      private string _markSbBlockName;
      private List<Paint> _paints;

      // Список плиток в панели Марки СБ
      private List<Tile> _tiles;
      // Конструктор. Скрытый.
      private MarkSbPanel(ObjectId idBtrMarkSb, string markSbName, string markSbBlockName)
      {
         _markSb = markSbName;
         _idBtr = idBtrMarkSb;
         _markSbBlockName = markSbBlockName;
         _marksAR = new List<MarkArPanel>();
         _colorAreas = ColorAreaModel.GetColorAreas(_idBtr);
         //TODO: Проверка пересенений зон покраски (не должно быть пересечений)
         //??

         // Список плиток (в определении блока марки СБ)
         GetTiles();
      }

      public ObjectId IdBtr { get { return _idBtr; } }

     

      public List<MarkArPanel> MarksAR { get { return _marksAR; } }

      public string MarkSbBlockName { get { return _markSbBlockName; } }

      public List<Paint> Paints { get { return _paints; } }

      // Свойства
      public List<Tile> Tiles { get { return _tiles; } }

      // Определение покраски панелей текущего чертежа (в Модели)
      public static List<MarkSbPanel> GetMarksSB(ColorAreaModel colorAreaModel)
      {
         List<MarkSbPanel> _marksSb = new List<MarkSbPanel>();
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            // Перебор всех блоков в модели и составление списка блоков марок и панелей.
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefPanel = t.GetObject(idEnt, OpenMode.ForRead, false , true) as BlockReference;
                  // Определение Марки СБ панели. Если ее еще нет, то она создается и добавляется в список _marks.
                  MarkSbPanel markSb = GetMarkSb(blRefPanel, _marksSb, bt);
                  if (markSb == null)
                  {
                     // Значит это не блок панели. Пропускаем.
                     continue;
                  }
                  //TODO: Определение покраски панели (Марки АР)
                  List<Paint> paintAR = MarkArPanel.GetPanelMarkAR(markSb, blRefPanel, colorAreaModel.ColorAreasForeground, colorAreaModel.ColorAreasBackground);
                  // Добавление панели АР в список панелей для Марки СБ
                  markSb.AddPanelAR(paintAR, blRefPanel);
               }
            }
            t.Commit();
         }
         return _marksSb;
      }

      // Замена вхождений блоков СБ на АР
      public void ReplaceBlocksSbOnAr()
      {
         foreach (var markAr in _marksAR)
         {
            markAr.ReplaceBlocksSbOnAr();
         }
      }

      // Определение марки СБ, если ее еще нет, то создание и добавление в список marks.
      private static MarkSbPanel GetMarkSb(BlockReference blRefPanel, List<MarkSbPanel> marksSb, BlockTable bt)
      {
         MarkSbPanel markSb = null;
         if (IsBlockNamePanel(blRefPanel.Name))
         {
            string markSbName = GetMarkSbName(blRefPanel.Name);
            if (markSbName != string.Empty)
            {
               markSb = marksSb.Find(m => m._markSb == markSbName);
               if (markSb == null)
               {
                  // Блок Марки СБ
                  Database db = HostApplicationServices.WorkingDatabase;
                  string markSbBlName = GetMarkSbBlockName(markSbName);
                  if (bt.Has(markSbBlName))
                  {
                     var idMarkSbBtr = bt[markSbBlName];
                     markSb = new MarkSbPanel(idMarkSbBtr, markSbName, markSbBlName);
                     marksSb.Add(markSb);
                  }
                  else
                  {
                     //TODO: Ошибка в чертеже. Блок с Маркой АР есть, а блока Марки СБ нет. Добавить в колекцию блоков с ошибками.
                  }
               }
            }
         }
         return markSb;
      }

      // Создание определения блока марки СБ из блока марки АР, и сброс покраски плитки (в слой 0)
      public static void CreateBlockMarkSbFromAr(ObjectId idBtrMarkAr, string markSbBlName)
      {
         var idBtrMarkSb = Lib.Blocks.CopyBtr(idBtrMarkAr, markSbBlName);
         // Перенос блоков плиток на слой 0
         Database db = HostApplicationServices.WorkingDatabase; 
         using (var t =db.TransactionManager.StartTransaction ()  )
         {
            var btrMarkSb = t.GetObject(idBtrMarkSb, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForWrite, false, true ) as BlockReference;
                  if (blRef.Name == Album.Options.BlockTileName)
                  {                     
                     blRef.Layer = "0";
                  }
               }
            }
            t.Commit();
         }         
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
            string markSb = GetMarkSbName(blName);
            string markSbBlName = GetMarkSbBlockName(markSb);
            res = blName != markSbBlName;
         }
         return res;
      }

      public static string GetMarkSbBlockName(string markSb)
      {
         return Album.Options.BlockPanelPrefixName + "_" + markSb;
      }

      // Определение марки СБ
      public static string GetMarkSbName(string blName)
      {
         string markSb = string.Empty;
         if (IsBlockNamePanel(blName))
         {
            // Хвостовая часть
            markSb = blName.Substring(Album.Options.BlockPanelPrefixName.Length + 1);
            // Если есть "_", то после него идет уже МаркаАР. Она нам не нужна.
            var unders = markSb.Split('_');
            if (unders.Length > 1)
            {
               markSb = unders[0];
            }
         }
         return markSb;
      }

      // Добавление панели АР по списку ее покраски
      private void AddPanelAR(List<Paint> paintAR, BlockReference blRefPanel)
      {
         // Проверка нет ли уже такой марки покраси АР
         MarkArPanel panelAR = HasPanelAR(paintAR);
         if (panelAR == null)
         {
            panelAR = new MarkArPanel(paintAR, GetMarkArNextName());
            _marksAR.Add(panelAR);
         }
         panelAR.AddBlockRefPanel(blRefPanel);
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
                  var blRefTile = t.GetObject(idEnt, OpenMode.ForRead, false , true) as BlockReference;
                  if (blRefTile.Name == Album.Options.BlockTileName)
                  {
                     Tile tile = new Tile(blRefTile);
                     //TODO: Определение покраски плитки
                     Paint paint = ColorArea.GetPaint(tile.Bounds, _colorAreas, null);
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
         //TODO: Поиск панели АР по покраске
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
   }
}
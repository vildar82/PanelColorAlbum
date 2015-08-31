using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Панели марки СБ
   public class MarkSbPanel
   {
      private string _markSb;
      private string _markSbBlockName;
      private ObjectId _idBtr;
      // Список плиток в панели Марки СБ
      private List<Tile> _tiles;
      private List<Paint> _paints;
      private List<MarkArPanel> _marksAR;
      // зоны покраски внутри определения блока (приоритет выше чем у зон в модели).
      private List<ColorArea> _colorAreas;

      public List<Tile> Tiles
      {
         get { return _tiles; }
      }

      public List<Paint> Paints
      {
         get { return _paints; }
      }

      public List<MarkArPanel> MarksAR
      {
         get { return _marksAR; }
      }

      public string MarkSbBlockName
      {
         get
         {
            if (_markSbBlockName == null)
               _markSbBlockName = GetMarkSbBlockName(_markSb);
            return _markSbBlockName;
         }         
      }

      public ObjectId IdBtr
      {
         get { return _idBtr; }
      }

      // Конструктор. Скрытый.
      private MarkSbPanel (ObjectId idBtrMarkSb, string markSbName)
      {
         _markSb = markSbName;
         _idBtr = idBtrMarkSb;
         _marksAR = new List<MarkArPanel>();
         _colorAreas = ColorArea.GetColorAreas(_idBtr);
         //Проверка пересенений зон покраски (не должно быть пересечений)
         CheckIntersectColorAreas();
         // Список плиток (в определении блока марки СБ)
         GetTiles();
      }

      // Определение марки СБ, если ее еще нет, то создание и добавление в список marks.
      public static MarkSbPanel GetMarkSb(BlockReference blRefPanel, List<MarkSbPanel> marksSb, BlockTable bt)
      {
         MarkSbPanel markSb = null;
         string markSbName = GetMarkSbName(blRefPanel);
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
                  markSb = new MarkSbPanel(idMarkSbBtr, markSbName);
                  marksSb.Add(markSb);
               }
               else
               {
                  //TODO: Ошибка в чертеже. Блок с Маркой АР есть, а блока Марки СБ нет. Добавить в колекцию блоков с ошибками.
               }
            }            
         }
         return markSb;
      }

      public static string GetMarkSbBlockName(string markSb)
      {
         return Album.options.BlockPanelPrefixName + "_" + markSb;
      }

      // определение имени марки СБ
      private static string GetMarkSbName(BlockReference blRefPanel)
      {
         string markSb = string.Empty; 
         if (blRefPanel.Name.StartsWith(Album.options.BlockPanelPrefixName))
         {
            // Хвостовая часть
            markSb = blRefPanel.Name.Substring(Album.options.BlockPanelPrefixName.Length+1);
            // Если есть "_", то после него идет уже МаркаАР. Она нам не нужна.
            var unders = markSb.Split('_');
            if (unders.Length >1)
            {
               markSb = unders[0];
            }
         }
         return markSb; 
      }

      // Добавление панели АР по списку ее покраски
      public void AddPanelAR(List<Paint> paintAR, BlockReference blRefPanel)
      {
         // Проверка нет ли уже такой марки покраси АР
         MarkArPanel panelAR = HasPanelAR(paintAR);
         if (panelAR == null )
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

      // Получение списка плиток в определении блока
      private void  GetTiles()
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
                  var blRefTile = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  if (blRefTile.Name == Album.options.BlockTileName)
                  {
                     Tile tile = new Tile(blRefTile);

                     //TODO: Определение покраски плитки
                     Paint paint =ColorArea.GetPaint(tile.Bounds, _colorAreas, null); 
                     _tiles.Add(tile);
                     _paints.Add(paint);
                  }
               }
            }
            t.Commit();
         }         
      }

      // Замена вхождений блоков СБ на АР
      public void ReplaceBlocksSbOnAr()
      {
         foreach (var markAr in _marksAR)
         {
            markAr.ReplaceBlocksSbOnAr(); 
         }
      }

      private void CheckIntersectColorAreas()
      {
         //TODO: Проверка пересечений зон покрасики (их не должно быть).
      }
   }
}

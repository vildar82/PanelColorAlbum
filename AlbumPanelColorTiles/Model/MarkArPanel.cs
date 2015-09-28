using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model
{
   // Марка АР покраски панели
   public class MarkArPanel : IEquatable<MarkArPanel>
   {
      #region Private Fields

      private ObjectId _idBtrAr;
      private string _markArBlockName;
      private string _markARPanelFullName;
      private string _markArTemp;
      private string _markPainting;
      private MarkSbPanel _markSB;
      private List<Paint> _paints;
      private List<Panel> _panels;
      private List<TileCalc> _tilesCalc;

      #endregion Private Fields

      #region Public Constructors

      public MarkArPanel(List<Paint> paintAR, MarkSbPanel markSb, BlockReference blRefMarkAr)
      {
         _markSB = markSb;
         _paints = paintAR;
         DefMarkArTempNames(markSb, blRefMarkAr.Name);
         _panels = new List<Panel>();
      }

      #endregion Public Constructors

      #region Public Properties

      public ObjectId IdBtrAr { get { return _idBtrAr; } }

      public string MarkArBlockName { get { return _markArBlockName; } }

      public string MarkARPanelFullName { get { return _markARPanelFullName; } }

      public string MarkPainting
      {
         get { return _markPainting; }
         set
         {
            bool isRename = !string.IsNullOrEmpty(_markPainting);
            _markPainting = value;
            _markArBlockName = string.Format("{0}({1}_{2})", _markSB.MarkSbBlockName, Blocks.GetValidNameForBlock(_markPainting), _markSB.Abbr);
            _markARPanelFullName = string.Format("{0}({1}_{2})", _markSB.MarkSbClean, _markPainting, _markSB.Abbr);
            if (isRename) // Переименование марки покраски пользователем.
            {
               // Переименование подписей марок панелей
               Album.AddMarkToPanelBtr(_markARPanelFullName, _idBtrAr);
            }
         }
      }

      public MarkSbPanel MarkSB { get { return _markSB; } }
      public List<Paint> Paints { get { return _paints; } }
      public List<Panel> Panels { get { return _panels; } }

      public List<TileCalc> TilesCalc
      {
         get
         {
            if (_tilesCalc == null)
            {
               _tilesCalc = CalculateTiles();
            }
            return _tilesCalc;
         }
      }

      #endregion Public Properties

      #region Public Methods

      // Определение покраски панели (список цветов по порядку списка плитов в блоке СБ)
      public static List<Paint> GetPanelMarkAR(MarkSbPanel markSb, BlockReference blRefPanel, List<ColorArea> colorAreasForeground, List<ColorArea> colorAreasBackground)
      {
         List<Paint> paintsAR = new List<Paint>();

         int i = 0;
         foreach (Tile tileMarSb in markSb.Tiles)
         {
            Paint paintSb = markSb.Paints[i++];
            Paint paintAR;
            if (paintSb == null)
            {
               // Опрделение покраски по зонам
               Point3d centerTileInBlRef = GetCenterTileInBlockRef(blRefPanel.Position, tileMarSb.CenterTile);
               paintAR = ColorArea.GetPaint(centerTileInBlRef, colorAreasForeground, colorAreasBackground);
               if (paintAR == null)
               {
                  //Ошибка. Не удалось определить покраску плитки.???
                  //В итоге, будет проверка, все ли плитки покрашены. Поэтому тут можно ничего ене делать.
               }
            }
            else
            {
               paintAR = paintSb;
            }
            paintsAR.Add(paintAR);
         }
         return paintsAR;
      }

      public void AddBlockRefPanel(BlockReference blRefPanel)
      {
         Panel panel = new Panel(blRefPanel);
         _panels.Add(panel);
      }

      // Создание определения блока Марки АР
      public void CreateBlock()
      {
         // Создание копии блока маркиСБ, с покраской блоков плиток
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            // Проверка нет ли уже определения блока панели Марки АР в таблице блоков чертежа
            if (bt.Has(_markArBlockName))
            {
               //Ошибка. Не должно быть определений блоков Марки АР.
               throw new System.Exception("\nВ чертеже не должно быть блоков Марки АР - " + _markArBlockName +
                           "\nРекомендуется выполнить команду сброса боков Марки АР до Марок СБ - ResetPanels.");
            }
            var btrMarkSb = t.GetObject(_markSB.IdBtr, OpenMode.ForRead) as BlockTableRecord;
            // Копирование определения блока
            _idBtrAr = Lib.Blocks.CopyBtr(_markSB.IdBtr, _markArBlockName);
            var btrMarkAr = t.GetObject(_idBtrAr, OpenMode.ForRead) as BlockTableRecord;
            int i = 0;
            foreach (ObjectId idEnt in btrMarkAr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as BlockReference;
                  if (Blocks.EffectiveName(blRef) == Album.Options.BlockTileName)
                  {
                     // это блок плитки. Покраска плитки.
                     var paintAr = _paints[i++];
                     if (paintAr == null)
                     {
                        blRef.Layer = "0";
                     }
                     else
                     {
                        blRef.Layer = paintAr.LayerName;
                     }
                  }
                  else if (Lib.Blocks.EffectiveName(blRef) == Album.Options.BlockColorAreaName)
                  {
                     // Блок зоны покраски. Удаляем его
                     blRef.Erase(true);
                  }
               }
            }
            t.Commit();
         }
      }

      public bool EqualPaint(List<Paint> paintAR)
      {
         // ??? сработает такое сравнение списков покраски?
         return paintAR.SequenceEqual(_paints);
      }

      public bool Equals(MarkArPanel other)
      {
         return _markArTemp.Equals(other._markArTemp) &&
            _paints.SequenceEqual(other._paints) &&
            _panels.SequenceEqual(other._panels);
      }

      // Замена вхождений блоков СБ на блоки АР
      public void ReplaceBlocksSbOnAr(Transaction t, BlockTableRecord ms)
      {
         foreach (var panel in _panels)
         {
            panel.ReplaceBlockSbToAr(this, t, ms);
         }
      }

      #endregion Public Methods

      #region Private Methods

      // Определение границы плитки во вхождении блока
      private static Point3d GetCenterTileInBlockRef(Point3d positionBlRef, Point3d centerTileInBtr)
      {
         return new Point3d(positionBlRef.X + centerTileInBtr.X, positionBlRef.Y + centerTileInBtr.Y, 0);
      }

      // Подсчет плитки
      private List<TileCalc> CalculateTiles()
      {
         List<TileCalc> tilesCalc = new List<TileCalc>();
         var paintsSameColor = _paints.GroupBy(p => p.LayerName);
         foreach (var item in paintsSameColor)
         {
            TileCalc tileCalc = new TileCalc(item.Key, item.Count(), item.First().Color);
            tilesCalc.Add(tileCalc);
         }
         return tilesCalc;
      }

      private void DefMarkArTempNames(MarkSbPanel markSB, string blName)
      {
         _markArTemp = "АР-" + markSB.MarksAR.Count.ToString();
      }

      #endregion Private Methods
   }
}
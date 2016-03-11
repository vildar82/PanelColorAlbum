using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RTreeLib;

namespace AlbumPanelColorTiles.Panels
{
   // Марка АР покраски панели
   public class MarkAr : IEquatable<MarkAr>
   {
      private string _abbrIndex;
      private ObjectId _idBtrAr;
      private string _markArBlockName;
      private string _markARPanelFullName;
      private string _markARPanelFullNameCalculated;
      private string _markArTemp;
      private string _markPainting;
      private string _markPaintingCalulated;

      // вычесленная программой марка покраски, в методе DefineArchitectMarks класса MarkSbPanel
      private MarkSb _markSB;

      private List<Paint> _paints;
      private List<Panel> _panels;
      private List<TileCalc> _tilesCalc;

      public MarkAr(List<Paint> paintAR, MarkSb markSb, BlockReference blRefMarkAr)
      {
         Album = markSb.Album;
         _markSB = markSb;
         _abbrIndex = string.IsNullOrEmpty(_markSB.Abbr) ? "" : "_" + _markSB.Abbr;
         _paints = paintAR;
         DefMarkArTempNames(markSb, blRefMarkAr.Name);
         _panels = new List<Panel>();
      }

      public Album Album { get; private set; }
      public ObjectId IdBtrAr { get { return _idBtrAr; } }

      public string MarkArBlockName { get { return _markArBlockName; } }

      public string MarkARPanelFullName
      {
         get
         {
            if (string.IsNullOrEmpty(_markARPanelFullName))
            {
               return _markARPanelFullNameCalculated;
            }
            return _markARPanelFullName;
         }
      }

      public string MarkARPanelFullNameCalculated { get { return _markARPanelFullNameCalculated; } }

      public string MarkPainting
      {
         get
         {
            if (string.IsNullOrEmpty(_markPainting))
            {
               return _markPaintingCalulated;
            }
            return _markPainting;
         }
         set
         {
            _markPainting = value;
            //_markARPanelFullName = string.Format("{0}({1}_{2})", _markSB.MarkSbClean, _markPainting, _markSB.Abbr);
            _markARPanelFullName = string.Format("{0}{1}", _markSB.MarkSbClean, MarkPaintingFull);
            // Переименование подписей марок панелей (текстовый объект внутри блока панелели)
            Caption caption = new Caption(_idBtrAr.Database);
            caption.AddMarkToPanelBtr(MarkARPanelFullName, _idBtrAr);
         }
      }

      public string MarkPaintingCalulated
      {
         get { return _markPaintingCalulated; }
         set
         {
            _markPaintingCalulated = value;
            _markArBlockName = string.Format("{0}{1}", _markSB.MarkSbBlockName, MarkPaintingFull.GetValidDbSymbolName());
            _markARPanelFullNameCalculated = string.Format("{0}{1}", _markSB.MarkSbClean, MarkPaintingFull);
         }
      }

      /// <summary>
      /// Марка покраски со скобками - (Э2_Н47Г)
      /// </summary>
      public string MarkPaintingFull
      {
         get
         {
            string paint = _markPainting ?? _markPaintingCalulated;
            return string.Format("({0}{1})", paint, _abbrIndex);
         }
      }

      public MarkSb MarkSB { get { return _markSB; } }
      public List<Paint> Paints { get { return _paints; } }
      public List<Panel> Panels { get { return _panels; } }

      /// <summary>
      /// Кол плитки в одной панели марки АР
      /// </summary>
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

      // Определение покраски панели (список цветов по порядку списка плитов в блоке СБ)
      public static List<Paint> GetPanelMarkAR(MarkSb markSb, BlockReference blRefPanel, RTree<ColorArea> rtreeColorAreas)
      {
         List<Paint> paintsAR = new List<Paint>();

         int i = 0;
         bool hasTileWithoutPaint = false;
         foreach (Tile tileMarSb in markSb.Tiles)
         {
            Paint paintSb = markSb.Paints[i++];
            Paint paintAR;
            if (paintSb == null)
            {
               // Опрделение покраски по зонам
               Point3d centerTileInBlRef = GetCenterTileInBlockRef(blRefPanel.Position, tileMarSb.CenterTile);
               paintAR = ColorArea.GetPaint(centerTileInBlRef, rtreeColorAreas);
               if (paintAR == null)
               {
                  if (!hasTileWithoutPaint)
                  {
                     //Ошибка. Не удалось определить покраску плитки.???
                     Inspector.AddError($"{markSb.MarkSbClean} - не все плитки покрашены", blRefPanel, icon: System.Drawing.SystemIcons.Error);
                     hasTileWithoutPaint = true;
                  }
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
         Panel panel = new Panel(blRefPanel, this);
         _panels.Add(panel);
      }

      // Создание определения блока Марки АР
      public void CreateBlock()
      {
         // Создание копии блока маркиСБ, с покраской блоков плиток
         Database db = HostApplicationServices.WorkingDatabase;
         using (var bt = db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            // Проверка нет ли уже определения блока панели Марки АР в таблице блоков чертежа
            if (bt.Has(_markArBlockName))
            {
               //Ошибка. Не должно быть определений блоков Марки АР.
               throw new System.Exception("\nВ чертеже не должно быть блоков Марки АР - " + _markArBlockName +
                           "\nРекомендуется выполнить команду сброса боков Марки АР до Марок СБ - ResetPanels.");
            }
         }
         // Копирование определения блока
         _idBtrAr = Lib.Block.CopyBtr(_markSB.IdBtr, _markArBlockName);
         using (var btrMarkAr = _idBtrAr.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            int i = 0;
            foreach (ObjectId idEnt in btrMarkAr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRef = idEnt.Open(OpenMode.ForWrite, false, true) as BlockReference)
                  {
                     var blNameEff = blRef.GetEffectiveName();
                     if (string.Equals(blNameEff, Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        // это блок плитки. Покраска плитки.
                        var paintAr = _paints[i++];
                        if (paintAr == null)
                        {
                           blRef.Layer = "0";
                        }
                        else
                        {
                           blRef.Layer = paintAr.Layer;
                        }
                     }
                     else if (string.Equals( blNameEff, Settings.Default.BlockColorAreaName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        // Блок зоны покраски. Удаляем его
                        blRef.Erase(true);
                     }
                  }
               }
            }
         }
      }

      public bool EqualPaint(List<Paint> paintAR)
      {
         // сравнение списков покраски
         return paintAR.SequenceEqual(_paints);
      }

      public bool Equals(MarkAr other)
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

      // Определение границы плитки во вхождении блока
      private static Point3d GetCenterTileInBlockRef(Point3d positionBlRef, Point3d centerTileInBtr)
      {
         return new Point3d(positionBlRef.X + centerTileInBtr.X, positionBlRef.Y + centerTileInBtr.Y, 0);
      }

      // Подсчет плитки в одной панели Марки АР
      private List<TileCalc> CalculateTiles()
      {
         List<TileCalc> tilesCalc = new List<TileCalc>();
         var paintsSameColor = _paints.GroupBy(p => p.Layer);
         foreach (var item in paintsSameColor)
         {
            TileCalc tileCalc = new TileCalc(item.First(), item.Count());
            tilesCalc.Add(tileCalc);
         }
         return tilesCalc;
      }

      private void DefMarkArTempNames(MarkSb markSB, string blName)
      {
         _markArTemp = markSB.MarksAR.Count.ToString(); // "АР-" + markSB.MarksAR.Count.ToString();
      }
   }
}
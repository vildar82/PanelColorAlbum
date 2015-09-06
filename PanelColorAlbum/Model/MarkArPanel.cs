using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Lib;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Марка АР покраски панели
   public class MarkArPanel
   {
      private ObjectId _idBtrAr;
      // Временная марка покраски
      private string _markArTemp;
      // Определенная марка покраски архитектурная
      private string _markArArch;
      private string _markARPanelFullName;
      private string _markARPanelFullValidName;
      private string _markArBlockName;      

      private List<Paint> _paints;
      private List<Panel> _panels;
      private List<TileCalc> _tilesCalc;
      private MarkSbPanel _markSB;

      public MarkArPanel(List<Paint> paintAR, MarkSbPanel markSb, BlockReference blRefMarkAr)
      {
         _markSB = markSb;
         _paints = paintAR;
         DefMarkArTempNames(markSb, blRefMarkAr.Name);
         _panels = new List<Panel>();
      }

      public ObjectId IdBtrAr { get { return _idBtrAr; } }

      public string MarkArArch {
         get { return _markArArch; }
         set
         {
            // Переименовать _markARPanelFullName и _markArBlockName
            _markArArch = value;
            _markARPanelFullName = _markSB.MarkSb + _markArArch;
            _markArBlockName = _markSB.MarkSbBlockName + Blocks.GetValidNameForBlock (_markArArch);
         }
      }
      
      public string MarkARPanelFullValidName
      {
         get
         {
            if (_markARPanelFullValidName == null)
            {
               _markARPanelFullValidName = _markArBlockName.Substring(Album.Options.BlockPanelPrefixName.Length);
            }
            return _markARPanelFullValidName;
         }         
      }

      public List<Paint> Paints { get { return _paints; } }

      /// <summary>
      /// Блоки панели с такой покраской
      /// </summary>
      public List<Panel> Panels { get { return _panels; } }

      public string MarkArBlockName { get { return _markArBlockName; } }

      /// <summary>
      /// Полное имя панели (Марка СБ + Марка АР)
      /// </summary>
      public string MarkARPanelFullName { get { return _markARPanelFullName; } }

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

      // Подсчет плитки
      private List<TileCalc> CalculateTiles()
      {
         List<TileCalc> tilesCalc = new List<TileCalc>();
         var paintsSameColor = _paints.GroupBy(p => p.LayerName);
         foreach (var item in paintsSameColor)
         {
            TileCalc tileCalc = new TileCalc();
            tileCalc.ColorMark = item.Key;
            tileCalc.Count = item.Count();
            tileCalc.Pattern = item.First().Color;
            tilesCalc.Add(tileCalc);
         }
         return tilesCalc;
      }

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
               Extents3d boundsTileInBlRef = GetBoundsTileInBlockRef(blRefPanel, tileMarSb);
               paintAR = ColorArea.GetPaint(boundsTileInBlRef, colorAreasForeground, colorAreasBackground);
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
      public bool CreateBlock(MarkSbPanel markSB)
      {
         bool res = true;
         // Создание копии блока маркиСБ, с покраской блоков плиток
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            // Проверка нет ли уже определения блока панели Марки АР в таблице блоков чертежа
            if (bt.Has(_markArBlockName))
            {
               //Ошибка. Не должно быть определений блоков Марки АР.
               Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
               ed.WriteMessage("\nВ чертеже не должно быть блоков Марки АР - " + _markArBlockName);
               ed.WriteMessage("\nРекомендуется выполнить команду сброса боков Марки АР до Марок СБ - ResetPanels.");
               return false;
            }
            var btrMarkSb = t.GetObject(markSB.IdBtr, OpenMode.ForRead) as BlockTableRecord;
            // Копирование определения блока
            _idBtrAr = Lib.Blocks.CopyBtr(markSB.IdBtr, _markArBlockName);
            var btrMarkAr = t.GetObject(_idBtrAr, OpenMode.ForRead) as BlockTableRecord;
            int i = 0;
            foreach (ObjectId idEnt in btrMarkAr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as BlockReference;
                  if (blRef.Name == Album.Options.BlockTileName)
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
         return res;
      }     

      public bool EqualPaint(List<Paint> paintAR)
      {
         // ??? сработает такое сравнение списков покраски?
         return paintAR.SequenceEqual(_paints);
      }

      private void DefMarkArTempNames(MarkSbPanel markSB, string blName)
      {
         _markArTemp = "АР-" + markSB.MarksAR.Count.ToString();
         //_markArBlockName = blName + "_" + _markAR;
         //_markARPanelFullName = _markArBlockName.Substring(Album.Options.BlockPanelPrefixName.Length);
      }

      // Замена вхождений блоков СБ на блоки АР
      public void ReplaceBlocksSbOnAr()
      {
         foreach (var panel in _panels)
         {
            panel.ReplaceBlockSbToAr(this);
         }
      }

      // Определение границы плитки во вхождении блока
      private static Extents3d GetBoundsTileInBlockRef(BlockReference blRefPanel, Tile tile)
      {
         Point3d ptBlRef = blRefPanel.Position;
         Point3d ptMin = new Point3d(ptBlRef.X + tile.Bounds.MinPoint.X, ptBlRef.Y + tile.Bounds.MinPoint.Y, 0);
         Point3d ptMax = new Point3d(ptBlRef.X + tile.Bounds.MaxPoint.X, ptBlRef.Y + tile.Bounds.MaxPoint.Y, 0);
         Extents3d boundsTileInBlRef = new Extents3d(ptMin, ptMax);
         return boundsTileInBlRef;
      }
   }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Марка АР покраски панели
   public class MarkArPanel
   {
      private List<Paint> _paints;
      private List<Panel> _panels;
      private string _markAR;
      private ObjectId _idBtrAr;

      public string MarkAR
      {
         get { return _markAR; }
      }

      public ObjectId IdBtrAr
      {
         get { return _idBtrAr; }
      }

      public MarkArPanel(List<Paint> paintAR, string markAr)
      {
         _paints = paintAR;
         _markAR = markAr;
         _panels = new List<Panel>();
      }

      public string GetMarkArBlockName(MarkSbPanel markSb)
      {
         return markSb.MarkSbBlockName + "_" + _markAR;
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
                  //TODO: Ошибка. Не удалось определить покраску плитки.                  
                  Debug.Assert(paintAR == null, "Не удалось определить покраску плитки.");
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

      private static Extents3d GetBoundsTileInBlockRef(BlockReference blRefPanel, Tile tile)
      {
         Point3d ptBlRef = blRefPanel.Position;
         Point3d ptMin = new Point3d(ptBlRef.X + tile.Bounds.MinPoint.X, ptBlRef.Y + tile.Bounds.MinPoint.Y, 0);
         Point3d ptMax = new Point3d(ptBlRef.X + tile.Bounds.MaxPoint.X, ptBlRef.Y + tile.Bounds.MaxPoint.Y, 0);
         Extents3d boundsTileInBlRef = new Extents3d(ptMin, ptMax);
         return boundsTileInBlRef;
      }

      public void AddBlockRefPanel(BlockReference blRefPanel)
      {
         //TODO: Добавление ссылки на блок этой марки покраски
         Panel panel = new Panel(blRefPanel);
         _panels.Add(panel);
      }

      public bool EqualPaint(List<Paint> paintAR)
      {
         // ??? сработает такое сравнение списков покраски?
         return paintAR.SequenceEqual(_paints);
      }

      // Создание определения блока Марки АР
      public void CreateBlock(MarkSbPanel markSB)
      {
         // Создание копии блока маркиСБ, с покраской блоков плиток
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            string markArBlockName = GetMarkArBlockName(markSB);
            // Проверка нет ли уже определения блока панели Марки АР в таблице блоков чертежа
            if (bt.Has(markArBlockName))
            {
               //TODO: Ошибка. Не должно быть определений блоков Марки АР.
               Debug.Assert(false, "Не должно быть определений блоков Марки АР.");
            }
            var btrMarkSb = t.GetObject(markSB.IdBtr, OpenMode.ForRead) as BlockTableRecord;
            // Копирование определения блока
            _idBtrAr = Lib.Blocks.CopyBtr(markSB.IdBtr, markArBlockName);             
            var btrMarkAr = t.GetObject(_idBtrAr, OpenMode.ForRead) as BlockTableRecord;
            int i = 0;
            foreach (ObjectId idEnt in btrMarkAr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  if (blRef.Name == Album.Options.BlockTileName)
                  {
                     // это блок плитки. Красим ее (слоем).
                     var paintAr = _paints[i++];                     
                     blRef.UpgradeOpen();
                     blRef.Layer = paintAr.LayerName;
                  }
                  else if (Lib.Blocks.EffectiveName(blRef) == Album.Options.BlockColorAreaName)
                  {
                     // Блок зоны покраски. Удаляем его
                     blRef.UpgradeOpen();
                     blRef.Erase(true);
                  }
               }
            }                        
            t.Commit();
         }
      }

      // Замена вхождений блоков СБ на блоки АР
      public void ReplaceBlocksSbOnAr()
      {
         foreach (var panel in _panels)
         {
            panel.ReplaceBlockSbToAr(this);
         }
      }
   }
}

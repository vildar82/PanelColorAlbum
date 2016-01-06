﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   public partial class Panel
   {
      private BaseService _service;

      /// <summary>
      /// Создание определения блока панели по описанию из базы XML от конструкторов.
      /// Должна быть открыта транзакция.
      /// </summary>
      /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">DuplicateBlockName</exception>
      /// <returns>ObjectId созданного определения блока в текущей базе.</returns>            
      public ObjectId CreateBlock(BaseService service)
      {
         _service = service;
         CreatePanelData panelData = new CreatePanelData();
         ObjectId resIdBtrPanel = ObjectId.Null;
         Database db = HostApplicationServices.WorkingDatabase;
         Transaction t = db.TransactionManager.TopTransaction;
         BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForWrite) as BlockTable;
         // Имя для блока панели АКР
         // Пока без "Щечек" и без окон
         
         panelData.BlNameAkr = Settings.Default.BlockPanelAkrPrefixName + mark;

         // Ошибка если блок с таким именем уже есть
         if (bt.Has(this.mark))
         {
            throw new Autodesk.AutoCAD.Runtime.Exception(
                           Autodesk.AutoCAD.Runtime.ErrorStatus.DuplicateBlockName, 
                           "Блок с именем {0} уже определен в чертеже".f(this.mark));
         }

         BlockTableRecord btrPanel = new BlockTableRecord();
         btrPanel.Name = panelData.BlNameAkr;
         resIdBtrPanel = bt.Add(btrPanel);
         t.AddNewlyCreatedDBObject(btrPanel, true);         
         // Добавление полилинии контура
         Polyline plContour = createContour();         
         btrPanel.AppendEntity(plContour);
         t.AddNewlyCreatedDBObject(plContour, true);

         // Добавление окон
         addWindows(btrPanel, t);

         // заполнение плиткой
         addTiles(btrPanel, t, panelData);

         return resIdBtrPanel;
      }      

      private Polyline createContour()
      {
         Polyline plContour = new Polyline();
         plContour.LayerId = _service.Env.IdLayerContourPanel;

         plContour.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
         plContour.AddVertexAt(0, new Point2d(0, this.gab.height), 0, 0, 0);
         plContour.AddVertexAt(0, new Point2d(this.gab.length, this.gab.height), 0, 0, 0);
         plContour.AddVertexAt(0, new Point2d(this.gab.length, 0), 0, 0, 0);
         plContour.Closed= true;

         return plContour;
      }

      private void addWindows(BlockTableRecord btrPanel, Transaction t)
      {
         // Пока данных по окнам из Архитектуры нет - добавление контура окон.
         if (this.windows?.window != null)
         {
            foreach (var item in this.windows.window)
            {
               Polyline plWindow = new Polyline();
               plWindow.LayerId = _service.Env.IdLayerContourPanel;
               Point2d ptBaseWindow = new Point2d(item.posi.X, item.posi.Y);
               plWindow.AddVertexAt(0, ptBaseWindow, 0, 0, 0);
               plWindow.AddVertexAt(0, new Point2d(ptBaseWindow.X, ptBaseWindow.Y + item.height), 0, 0, 0);
               plWindow.AddVertexAt(0, new Point2d(ptBaseWindow.X + item.width, ptBaseWindow.Y + item.height), 0, 0, 0);
               plWindow.AddVertexAt(0, new Point2d(ptBaseWindow.X + item.width, ptBaseWindow.Y), 0, 0, 0);
               plWindow.Closed = true;
               btrPanel.AppendEntity(plWindow);
               t.AddNewlyCreatedDBObject(plWindow, true);
            }
         }
      }

      private void addTiles(BlockTableRecord btrPanel, Transaction t, CreatePanelData panelData)
      {
         for (int x = 0; x < this.gab.length- Settings.Default.TileLenght*0.5; x+=Settings.Default.TileLenght+ Settings.Default.TileSeam)
         {
            for (int y = 0; y < this.gab.height- Settings.Default.TileHeight*0.5; y+=Settings.Default.TileHeight+Settings.Default.TileSeam)
            {
               Point3d pt = new Point3d(x, y, 0);
               BlockReference blRefTile = new BlockReference(pt, _service.Env.IdBtrTile);
               blRefTile.Layer = "0";
               blRefTile.ColorIndex = 256; // ByLayer

               btrPanel.AppendEntity(blRefTile);
               t.AddNewlyCreatedDBObject(blRefTile, true);
            }
         }
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Xml.Serialization;

namespace AlbumPanelColorTiles.Model.Base
{
   public partial class Panel
   {
      [XmlIgnore]
      public BaseService Service { get; set; }
      [XmlIgnore]
      public string BlNameAkr { get; set; }
      [XmlIgnore]
      public List<Extents3d> Openings { get; set; }
      [XmlIgnore]
      public ObjectId IdBtrPanel { get; set; }

      /// <summary>
      /// Создание определения блока панели по описанию из базы XML от конструкторов.
      /// Должна быть открыта транзакция.
      /// </summary>
      /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">DuplicateBlockName</exception>
      /// <returns>ObjectId созданного определения блока в текущей базе.</returns>            
      public void CreateBlock(BaseService service)
      {
         Service = service;
         Openings = new List<Extents3d>();         
         Database db = HostApplicationServices.WorkingDatabase;
         Transaction t = db.TransactionManager.TopTransaction;
         BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForWrite) as BlockTable;
         // Имя для блока панели АКР
         // Пока без "Щечек" и без окон
         
         BlNameAkr = Settings.Default.BlockPanelAkrPrefixName + mark;

         // Ошибка если блок с таким именем уже есть
         if (bt.Has(this.mark))
         {
            throw new Autodesk.AutoCAD.Runtime.Exception(
                           Autodesk.AutoCAD.Runtime.ErrorStatus.DuplicateBlockName, 
                           "Блок с именем {0} уже определен в чертеже".f(this.mark));
         }

         BlockTableRecord btrPanel = new BlockTableRecord();
         btrPanel.Name = BlNameAkr;
         IdBtrPanel = bt.Add(btrPanel);
         t.AddNewlyCreatedDBObject(btrPanel, true);         
              
         // Добавление полилинии контура
         Polyline plContour = createContour();         
         btrPanel.AppendEntity(plContour);
         t.AddNewlyCreatedDBObject(plContour, true);

         // Добавление окон
         addWindows(btrPanel, t);

         // заполнение плиткой
         addTiles(btrPanel, t);

         // Образмеривание (на Фасаде)
         DimensionFacade dimFacade = new DimensionFacade(btrPanel, t, this);
         dimFacade.Create();
         // Образмеривание (в Форме)
         DimensionForm dimForm = new DimensionForm(btrPanel, t, this);
         dimForm.Create();         
      }      

      private Polyline createContour()
      {
         Polyline plContour = new Polyline();
         plContour.LayerId = Service.Env.IdLayerContourPanel;

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
               plWindow.LayerId = Service.Env.IdLayerContourPanel;
               Point2d ptMinWindow = new Point2d(item.posi.X, item.posi.Y);
               plWindow.AddVertexAt(0, ptMinWindow, 0, 0, 0);
               plWindow.AddVertexAt(0, new Point2d(ptMinWindow.X, ptMinWindow.Y + item.height), 0, 0, 0);
               Point2d ptMaxWindow = new Point2d(ptMinWindow.X + item.width, ptMinWindow.Y + item.height);
               plWindow.AddVertexAt(0, ptMaxWindow, 0, 0, 0);
               plWindow.AddVertexAt(0, new Point2d(ptMinWindow.X + item.width, ptMinWindow.Y), 0, 0, 0);
               plWindow.Closed = true;
               btrPanel.AppendEntity(plWindow);
               t.AddNewlyCreatedDBObject(plWindow, true);

               Openings.Add(new Extents3d(ptMinWindow.Convert3d(), ptMaxWindow.Convert3d()));
            }
            // Сортировка окон слева-направо
            Openings.Sort((w1, w2) => w1.MinPoint.X.CompareTo(w2.MinPoint.X));
         }
      }

      private void addTiles(BlockTableRecord btrPanel, Transaction t)
      {
         for (int x = 0; x < this.gab.length- Settings.Default.TileLenght*0.5; x+=Settings.Default.TileLenght+ Settings.Default.TileSeam)
         {
            for (int y = 0; y < this.gab.height- Settings.Default.TileHeight*0.5; y+=Settings.Default.TileHeight+Settings.Default.TileSeam)
            {
               Point3d pt = new Point3d(x, y, 0);

               if (!openingsContainPoint(pt))
               {
                  BlockReference blRefTile = new BlockReference(pt, Service.Env.IdBtrTile);
               blRefTile.Layer = "0";
               blRefTile.ColorIndex = 256; // ByLayer

               btrPanel.AppendEntity(blRefTile);
               t.AddNewlyCreatedDBObject(blRefTile, true);
            }
         }
         }
      }

      private bool openingsContainPoint(Point3d pt)
      {
         return Openings.Any(b => b.IsPointInBounds(pt));
      }
   }
}

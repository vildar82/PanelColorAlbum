using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Xml.Serialization;
using AlbumPanelColorTiles.PanelLibrary;
using AcadLib.Errors;
using MoreLinq;
using AlbumPanelColorTiles.Model.Base.CreatePanel;

namespace AlbumPanelColorTiles.Model.Base
{
   public class PanelBase
   {
      public string MarkWithoutElectric { get; private set; }

      public List<double> PtsForTopDim { get; set; } = new List<double>();
      public List<double> PtsForBotDimCheek { get; set; } = new List<double>();

      public BaseService Service { get; private set; }      
      public string BlNameAkr { get; set; }
      public int WindowsIndex { get; set; }
      public List<Extents3d> Openings { get; set; }      
      public ObjectId IdBtrPanel { get; set; }
      public Panel Panel { get; private set; }
      public Dictionary<Point3d, string> WindowsBaseCenters { get; set; } = new Dictionary<Point3d, string>();
      public bool IsCheekRight { get; private set; }
      public bool IsCheekLeft { get; private set; }
      public bool IsOutsideRight { get; set; }
      public bool IsOutsideLeft { get; set; }
      public bool HasWindows { get; set; }
      /// <summary>
      /// Панель ОЛ - ограждение лоджии
      /// </summary>
      public bool IsOL { get; set; }
      /// <summary>
      /// Панель чердака - ?? как определять
      /// </summary>
      public bool IsUpperStoreyPanel { get; set; }      
      /// <summary>
      /// Скольки слойная стеновая панель (1, 3)
      /// </summary>
      public int NLayerPanel { get; set; }
      public double Length { get; private set; }
      /// <summary>
      /// Длина по плитке
      /// </summary>
      public double LengthByTiles { get; private set; }
      public double Height { get; private set; }
      public int Thickness { get; private set; }
      /// <summary>
      /// Край контура панели - контур для заполнения плиткой
      /// </summary>
      public double XMinContour { get; set; }
      /// <summary>
      /// Край контура панели - контур для заполнения плиткой
      /// </summary>
      public double XMaxContour { get; set; }
      /// <summary>
      /// Край панели - с учетом угловых торцов
      /// </summary>
      public double XMinPanel { get; set; }
      /// <summary>
      /// Край панели - с учетом угловых торцов
      /// </summary>
      public double XMaxPanel { get; set; }
      public double XStartTile { get; set; }
      public MountingPanel PanelMount { get; private set; }

      

      public PanelBase(Panel panelXml, BaseService service, MountingPanel panelMount = null)
      {
         Panel = panelXml;
         Service = service;

         MarkWithoutElectric = MountingPanel.GetMarkWithoutElectric(Panel.mark);

         Length = panelXml.gab.length;
         Height = panelXml.gab.height;

         XMinContour = 0;
         XMaxContour = Length;

         XMinPanel = 0;
         XMaxPanel = Length;
         LengthByTiles = Length;

         Thickness = getThickness (panelXml, panelMount);

         setNLayerPanel();
         IsOL = Panel.mark.StartsWith("ол", StringComparison.OrdinalIgnoreCase);
         IsUpperStoreyPanel = defineIsUpperStoreyPanel();

         HasWindows = Panel.windows?.window?.Count() > 0;
      }     

      private int getThickness(Panel panelXml, MountingPanel panelMount = null)
      {
         int resVal = 0;
         if (panelMount !=null && panelMount.Thickness >0)
         {
            // Может быть неточная.
            resVal = panelMount.Thickness;
         }
         else
         {
            // Пока задам по-умолчанию 320. В описании панели в xml должна скоро появиться толщина
            resVal = 320;
         }
         return resVal;
      }      

      /// <summary>
      /// Создание определения блока панели по описанию из базы XML от конструкторов.
      /// Должна быть открыта транзакция.
      /// </summary>
      /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">DuplicateBlockName</exception>
      /// <returns>ObjectId созданного определения блока в текущей базе.</returns>            
      public void CreateBlock()
      {
         // Имя для блока панели АКР
         BlNameAkr = defineBlockPanelAkrName();

         Openings = new List<Extents3d>();

         Database db = Service.Db;
         //Transaction t = db.TransactionManager.TopTransaction;
         using (var t = db.TransactionManager.StartTransaction())
         {            
            BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            // Ошибка если блок с таким именем уже есть
            if (bt.Has(BlNameAkr))
            {
               IdBtrPanel = bt[BlNameAkr];
               Inspector.AddError($"Блок панели с именем {BlNameAkr} уже определен в чертеже.", icon: System.Drawing.SystemIcons.Error);
               return;
            }
            BlockTableRecord btrPanel = new BlockTableRecord();
            btrPanel.Name = BlNameAkr;
            bt.UpgradeOpen();
            IdBtrPanel = bt.Add(btrPanel);
            bt.DowngradeOpen();
            t.AddNewlyCreatedDBObject(btrPanel, true);

            //корректировка точки отсчета панели
            correctStartPointCoordinatesPanel();

            // Добавление полилинии контура
            createContour(btrPanel, t);

            // Добавление окон
            addWindows(btrPanel, t);

            // заполнение плиткой
            addTiles(btrPanel, t);

            // Добавление торцов (Cheek)
            addCheek(btrPanel, t);

            // Образмеривание (на Фасаде)
            DimensionFacade dimFacade = new DimensionFacade(btrPanel, t, this);
            dimFacade.Create();
            // Образмеривание (в Форме)
            DimensionForm dimForm = new DimensionForm(btrPanel, t, this);
            dimForm.Create();

            t.Commit();
         }
      }

      private void correctStartPointCoordinatesPanel()
      {
         // если это панель с левым примыканием (Outside), то изменение точки отсчета координат в панели на величину примыкания
         if (Panel.outsides?.outside?.Count()>0)
         {
            var outsideLeft = Panel.outsides.outside.Where(o => o.posi.X < 0).FirstOrDefault();            
            if (outsideLeft !=null)
            {
               var delta = Math.Abs(outsideLeft.posi.X);
               Panel.gab.length += delta;
               Length += delta;
               XMaxContour = Length;
               XMaxPanel = Length;
               LengthByTiles -= 70; 
               Panel.balconys?.balcony?.ForEach(b => b.posi.X += delta);
               Panel.outsides?.outside?.ForEach(o => o.posi.X += delta);
               Panel.undercuts?.undercut?.ForEach(u => u.posi.X += delta);
               Panel.windows?.window?.ForEach(w => w.posi.X += delta);
            }
         }
      }

      private string defineBlockPanelAkrName()
      {
         string blName = Settings.Default.BlockPanelAkrPrefixName + MarkWithoutElectric;

         // щечки
         string cheek = Panel.cheeks?.cheek;
         if (!string.IsNullOrWhiteSpace(cheek))
         {
            string cheekPrefix = string.Empty;
            if (cheek.Equals("right", StringComparison.OrdinalIgnoreCase))
            {
               cheekPrefix = "_тл";
               IsCheekLeft = true;
            }
            else
            {               
               cheekPrefix = "_тп";
               IsCheekRight = true;
            }          
            blName += cheekPrefix;
         }
         if (WindowsIndex >0)
         {
            blName += Settings.Default.WindowPanelSuffix + WindowsIndex;
         }         

         return blName;
      }

      private void createContour(BlockTableRecord btrPanel, Transaction t)
      {
         Contour contour = new Contour(this);
         contour.Create(btrPanel, t);         
      }

      private void addWindows(BlockTableRecord btrPanel, Transaction t)
      {
         // все окна и балеоны в панели
         var windows = Panel.windows?.window?.Select(w => new { posi = w.posi, width = w.width, height = w.height });
         var balconys = Panel.balconys?.balcony?.Select(b => new { posi = b.posi, width = b.width, height = b.height });
         var apertures = balconys==null? windows: windows?.Union(balconys)?? balconys;
         if (apertures != null)
         {
            foreach (var item in apertures)
            {
               // контур окон
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

               // добавление точек для верхнего образмеривания.
               PtsForTopDim.Add(ptMinWindow.X);
               PtsForTopDim.Add(ptMaxWindow.X);

               Openings.Add(new Extents3d(ptMinWindow.Convert3d(), ptMaxWindow.Convert3d()));

               // Вставка окон
               if (WindowsBaseCenters.Count > 0)
               {
                  var xCenter = item.posi.X + item.width * 0.5;
                  var winMarkMin = WindowsBaseCenters.Where(w => Math.Abs(w.Key.X - xCenter) < 600);
                  if (winMarkMin.Count() > 0)
                  {
                     var winMark = winMarkMin.MinBy(g => (g.Key.X - xCenter));
                     if (string.IsNullOrWhiteSpace(winMark.Value))
                     {
                        continue;
                     }

                     // Точка вставки блока окна
                     Point3d ptWin = new Point3d(item.posi.X, item.posi.Y, 0);
                     // Вставка блока окна                  
                     BlockReference blRefWin = new BlockReference(ptWin, Service.Env.IdBtrWindow);
                     blRefWin.LayerId = Service.Env.IdLayerWindow;
                     btrPanel.AppendEntity(blRefWin);
                     t.AddNewlyCreatedDBObject(blRefWin, true);

                     var resSetDyn = BlockWindow.SetDynBlWinMark(blRefWin, winMark.Value);
                     if (!resSetDyn)
                     {
                        // Добавление текста марки окна
                        DBText dbTextWin = new DBText();
                        dbTextWin.Position = ptWin;
                        dbTextWin.LayerId = Service.Env.IdLayerWindow;
                        dbTextWin.TextString = winMark.Value;
                        dbTextWin.Height = 180;
                        btrPanel.AppendEntity(dbTextWin);
                        t.AddNewlyCreatedDBObject(dbTextWin, true);
                     }
#if Test
                     // Test
                     else
                     {
                        // Добавление текста марки окна
                        DBText dbTextWin = new DBText();
                        dbTextWin.Position = ptWin;
                        dbTextWin.LayerId = Service.Env.IdLayerWindow;
                        dbTextWin.TextString = winMark.Value;
                        dbTextWin.Height = 180;
                        btrPanel.AppendEntity(dbTextWin);
                        t.AddNewlyCreatedDBObject(dbTextWin, true);
                     }
#endif
                  }
               }
               // Сортировка окон слева-направо
               Openings.Sort((w1, w2) => w1.MinPoint.X.CompareTo(w2.MinPoint.X));
            }
         }
      }

      private void addTiles(BlockTableRecord btrPanel, Transaction t)
      {
         for (double x = XStartTile; x < XMaxContour - Settings.Default.TileLenght * 0.5; x += Settings.Default.TileLenght + Settings.Default.TileSeam)
         {
            for (double y = 0; y < Height - Settings.Default.TileHeight * 0.5; y += Settings.Default.TileHeight + Settings.Default.TileSeam)
            {
               Point3d pt = new Point3d(x, y, 0);

               if (!tileInOpenings(pt))
               {
                  addTile(btrPanel, t, pt);
               }
            }
         }
      }

      private void addTile(BlockTableRecord btrPanel, Transaction t, Point3d pt)
      {
         BlockReference blRefTile = new BlockReference(pt, Service.Env.IdBtrTile);
         blRefTile.Layer = "0";
         blRefTile.ColorIndex = 256; // ByLayer

         btrPanel.AppendEntity(blRefTile);
         t.AddNewlyCreatedDBObject(blRefTile, true);
      }

      private void addCheek(BlockTableRecord btrPanel, Transaction t)
      {
         if (IsCheekLeft || IsCheekRight)
         {
            int yStep = Settings.Default.TileHeight + Settings.Default.TileSeam;
            double xTile = 0;
            List<Point2d> ptsPlContourCheek = new List<Point2d>();

            // Торец слева
            if (IsCheekLeft)
            {
               xTile = -(600 + Settings.Default.TileLenght);
               // Добавление точек контура в список               
               Point2d pt = new Point2d(xTile, 0);
               ptsPlContourCheek.Add(pt);
               PtsForBotDimCheek.Add(pt.X);

               pt = new Point2d(pt.X + 277, 0);
               PtsForBotDimCheek.Add(pt.X);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X + 12, 0);
               PtsForBotDimCheek.Add(pt.X);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X, Height);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X-30, pt.Y);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X-20, pt.Y+20);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(xTile, pt.Y);
               ptsPlContourCheek.Add(pt);
            }

            // Торец справа
            else if (IsCheekRight)
            {
               xTile = Length + 600;
               // Добавление точек контура в список               
               Point2d pt = new Point2d(xTile, 0);
               ptsPlContourCheek.Add(pt);
               PtsForBotDimCheek.Add(pt.X);

               pt = new Point2d(pt.X + 12, 0);
               PtsForBotDimCheek.Add(pt.X);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X + 277, 0);
               PtsForBotDimCheek.Add(pt.X);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X, Height+20);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X - 240, pt.Y);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(pt.X - 20, pt.Y - 20);
               ptsPlContourCheek.Add(pt);

               pt = new Point2d(xTile, pt.Y);
               ptsPlContourCheek.Add(pt);
            }

            // Заполнение торца плиткой
            for (int y = 0; y < Height; y += yStep)
            {
               Point3d pt = new Point3d(xTile, y, 0);
               addTile(btrPanel, t, pt);
            }
            // Полилиния контура торца
            Polyline plCheekContour = new Polyline();
            plCheekContour.LayerId = Service.Env.IdLayerContourPanel;
            int i = 0;
            ptsPlContourCheek.ForEach(p => plCheekContour.AddVertexAt(i++, p, 0, 0, 0));
            plCheekContour.Closed = true;
            btrPanel.AppendEntity(plCheekContour);
            t.AddNewlyCreatedDBObject(plCheekContour, true);
         }
      }

      private bool tileInOpenings(Point3d pt)
      {
         // Проверка попадаетли точка вставки блока плитки в один из проемов
         Point3d ptMax = new Point3d(pt.X + 288, pt.Y + 88, 0);         
         return (Openings.Any(b => b.IsPointInBounds(pt, 4))) || (Openings.Any(b => b.IsPointInBounds(ptMax, 4))); 
      }

      //private void insertWindowBlock(string mark, Point3d pt, BlockTableRecord btrPanel, Transaction t)
      //{
      //   // Вставка блока окна                  
      //   BlockReference blRefWin = new BlockReference(pt, Service.Env.IdBtrWindow);
      //   blRefWin.LayerId = Service.Env.IdLayerWindow;
      //   btrPanel.AppendEntity(blRefWin);
      //   t.AddNewlyCreatedDBObject(blRefWin, true);

      //   BlockWindow.SetDynBlWinMark(blRefWin, mark);
      //}

      private void setNLayerPanel()
      {
         int n = (int)Char.GetNumericValue(Panel.mark.First());
         if (n>0)
         {
            NLayerPanel = n;
         }
      }
      
      private bool defineIsUpperStoreyPanel()
      {
         var splits = Panel.mark.Split(' ');
         if (splits.Count() > 1)
         {
            string shortGost = splits[0];
            return shortGost.IndexOf("ч", StringComparison.OrdinalIgnoreCase) > 0;
         }
         return false;
      }
   }
}

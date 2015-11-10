using System.Collections.Generic;
using System.Linq;
using AcadLib.Comparers;
using AlbumPanelColorTiles.Properties;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Фасад - это ряд блоков монтажных планов этажей с блоками обозначения стороны плана фасада - составляющие один фасада дома
   public class Facade
   {
      // Этажи фасада (блоки АКР-Панелей и соотв блок Монтажки)
      private List<Floor> _floors;

      private double _xmax;

      // коорднината X для данного фасада
      private double _xmin;

      public Facade(double x)
      {
         _xmin = x;
         _floors = new List<Floor>();
      }

      public List<Floor> Floors { get { return _floors; } }
      public double XMax { get { return _xmax; } }
      public double XMin { get { return _xmin; } }

      // Создание фасадов по монтажным планам
      public static void CreateFacades(List<Facade> facades)
      {
         if (facades.Count == 0) return;
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
            double yFirstFloor = getFirstFloorY(facades); // Y для первых этажей всех фасадов

            using (ProgressMeter progress = new ProgressMeter())
            {
               progress.SetLimit(facades.SelectMany(f => f.Floors).Count());
               progress.Start("Создание фасадов");

               foreach (var facade in facades)
               {
                  double yFloor = yFirstFloor;
                  foreach (var floor in facade.Floors)
                  {
                     // Подпись номера этажа
                     captionFloor(facade.XMin, yFloor, floor.Name, ms, t);
                     foreach (var panelSb in floor.PanelsSbInFront)
                     {
                        if (panelSb.PanelAKR != null)
                        {
                           Point3d ptPanelAkr = new Point3d(panelSb.GetPtInModel(panelSb.PanelAKR).X, yFloor, 0);
                           var blRefPanelAkr = new BlockReference(ptPanelAkr, panelSb.PanelAKR.IdBtrAkrPanelInFacade);
                           panelSb.PanelAKR.IdBlRef = ms.AppendEntity(blRefPanelAkr);
                           t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
                           blRefPanelAkr.Draw();
                        }
                     }
                     yFloor += Settings.Default.FacadeFloorHeight;// 2800;// высота этажа
                     progress.MeterProgress();
                  }
               }
               t.Commit();
               progress.Stop();
            }
         }
      }

      /// <summary>
      /// Получение фасадов из блоков монтажных планов и обозначений стороны фасада в чертеже
      /// </summary>
      /// <returns></returns>
      public static List<Facade> GetFacadesFromMountingPlans(PanelLibraryLoadService libLoadServ)
      {
         List<Facade> facades = new List<Facade>();

         // Поиск всех блоков монтажных планов в Модели чертежа с соотв обозначением стороны фасада
         List<Floor> floors = Floor.GetMountingBlocks(libLoadServ);

         // определение торцов панелей
         floors.ForEach(f => f.DefineEndsPanelSb());

         // Упорядочивание блоков этажей в фасады (блоки монтажек по вертикали образуют фасад)
         // сортировка блоков монтажек по X, потом по Y (все монтажки в одну вертикаль снизу вверх)
         var comparerFloors = new DoubleEqualityComparer(1000); // FacadeVerticalDeviation
         foreach (var floor in floors)
         {
            Facade facade = facades.Find(f => comparerFloors.Equals(f.XMin, floor.XMin));
            if (facade == null)
            {
               // Новый фасад
               facade = new Facade(floor.XMin);
               facades.Add(facade);
            }
            facade._floors.Add(floor);
         }
         // сортировка этажей в фасадах
         facades.ForEach(f =>
         {
            f.Floors.Sort();
            f._xmax = f.Floors.Max(l => l.XMax);
         });
         return facades;
      }

      // Подпись номера этажа
      private static void captionFloor(double x, double yFloor, string name, BlockTableRecord ms, Transaction t)
      {
         DBText textFloor = new DBText();
         textFloor.SetDatabaseDefaults(ms.Database);
         textFloor.Annotative = AnnotativeStates.False;
         textFloor.Height = Settings.Default.FacadeCaptionFloorTextHeight;// 250;// FacadeCaptionFloorTextHeight
         textFloor.TextString = name;
         textFloor.Position = new Point3d(x - Settings.Default.FacadeCaptionFloorIndent, yFloor + (Settings.Default.FacadeFloorHeight*0.5), 0);
         ms.AppendEntity(textFloor);
         t.AddNewlyCreatedDBObject(textFloor, true);
      }

      // определение уровня по Y для первого этажа всех фасадов - отступить 10000 вверх от самого верхнего блока панели СБ.
      private static double getFirstFloorY(List<Facade> facades)
      {
         double maxYblRefPanelInModel = facades.SelectMany(f => f.Floors).SelectMany(f => f.AllPanelsSbInFloor).Max(p => p.PtCenterPanelSbInModel.Y);
         return maxYblRefPanelInModel + Settings.Default.FacadeIndentFromMountingPlanes;// 10000; // FacadeIndentFromMountingPlanes
      }
   }
}
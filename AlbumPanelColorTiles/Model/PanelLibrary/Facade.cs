using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Фасад - это ряд блоков монтажных планов этажей с блоками обозначения стороны плана фасада - составляющие один фасада дома
   public class Facade
   {      
      // Этажи фасада (блоки АКР-Панелей и соотв блок Монтажки)
      private List<Floor> _floors;
      // коорднината X для данного фасада
      private double _x;

      public Facade(double x)
      {
         _x = x;
         _floors = new List<Floor>();
      }

      public List<Floor> Floors { get { return _floors; } }
      public double X { get { return _x; } }

      /// <summary>
      /// Получение фасадов из блоков монтажных планов и обозначений стороны фасада в чертеже
      /// </summary>
      /// <returns></returns>
      public static List<Facade> GetFacadesFromMountingPlans(PanelLibraryLoadService libLoadServ)
      {         
         List<Facade> facades = new List<Facade>();

         // Поиск всех блоков монтажных планов в Модели чертежа с соотв обозначением стороны фасада
         List<Floor> floors = Floor.GetMountingBlocks(libLoadServ);

         // Упорядочивание блоков этажей в фасады (блоки монтажек по вертикали образуют фасад)
         // сортировка блоков монтажек по X, потом по Y (все монтажки в одну вертикаль снизу вверх)
         var comparerFloors = new DoubleEqualityComparer(1000);
         foreach (var floor in floors)
         {
            Facade facade = facades.Find(f => comparerFloors.Equals(f.X, floor.PtBlMounting.X));
            if (facade == null)
            {
               // Новый фасад
               facade = new Facade(floor.PtBlMounting.X);
               facades.Add(facade);
            }
            facade._floors.Add(floor);
         }
         return facades;
      }

      // Создание фасадов по монтажным планам
      public static void CreateFacades(List<Facade> facades)
      {
         if (facades.Count == 0) return;
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction()) 
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
            double yFirstFloor = getFirstFloorY(facades); // Y для первых этажей всех фасадов
            foreach (var facade in facades)
            {
               double yFloor = yFirstFloor;
               foreach (var floor in facade.Floors)
               {                  
                  foreach (var panelSb in floor.PanelsSbInFront)
                  {
                     if (panelSb.PanelAKR != null)
                     {
                        Point3d ptPanelAkr = new Point3d(panelSb.GetPtInModel(panelSb.PanelAKR).X, yFloor, 0);
                        var blRefPanelAkr = new BlockReference(ptPanelAkr, panelSb.PanelAKR.IdBtrAkrPanelInFacade);
                        panelSb.PanelAKR.IdBlRef = ms.AppendEntity(blRefPanelAkr);
                        t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
                     }
                  }
                  yFloor += 2800;// высота этажа
               } 
            }
            t.Commit();
         }
      }

      // определение уровня по Y для первого этажа всех фасадов - отступить 10000 вверх от самого верхнего блока панели СБ.
      private static double getFirstFloorY(List<Facade> facades)
      {
         double maxYblRefPanelInModel = facades.SelectMany(f => f.Floors).SelectMany(f => f.AllPanelsSbInFloor).Max(p => p.PtCenterPanelSbInModel.Y);
         return maxYblRefPanelInModel + 10000;
      }
   }
}

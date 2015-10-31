using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Lib;

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
   }
}

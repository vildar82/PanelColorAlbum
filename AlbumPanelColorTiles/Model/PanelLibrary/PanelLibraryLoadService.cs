using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Checks;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // вставка панелей из библиотеки
   public class PanelLibraryLoadService
   {
      // все блоки панелей-СБ в чертеже
      private List<PanelSB> _allPanelsSB = new List<PanelSB> ();      

      public List<PanelSB> AllPanelsSB { get { return _allPanelsSB; } }

      // в чертеже должны быть расставлены монтажки с блоками обозначения фасадов на них.
      // нужно для блоков панелей СБ найти соответствующую панель покраски в библиотеки 
      // но, марки панелей у СБ и у АР могут немного отличаться пробелами и -, нужно это учесть.
      // в результате должны получится фасады из панелей

      // задача архитектора: 
      // проверить вставленные блоки панелей, т.к. могут быть новые изменения, а вставленные панели не соответствовать этим изменениям.
      // проверить расстановку панелей по фасаду. хз как оно должно быть.

      // найти блоки монтажек (они должны распологаться в столбик для каждого фасада)
      // допустимое отклонение по вертикали между точками вставки блоков монтажек = +- 1000мм.

      // 1. Найти фасады в чертеже
      // Фасад - это ряд блоков монтажных планов этажей с блоками обозначения стороны плана как фасада составляющие один фасада дома


      // загрузка АКР-панелей из библиотеки с попыткой расстановить их в виде фасадов если правильно расставлены монтажки
      public void LoadPanels()
      {
         Inspector.Clear();
         // Попытка определить фасады по монтажкам
         List<Facade> facades = Facade.GetFacadesFromMountingPlans(this);
         if (Inspector.HasErrors)
         {
            // Показать ошибки.
            Inspector.Show();
            // запрос простой расстановки имеющихся в бибилтоеке АКР-Панелей
         }

         // загрузка АКР-панелей из библиотеки

         // расстановка АКР-Панелей по фасадам
      }
   }
}

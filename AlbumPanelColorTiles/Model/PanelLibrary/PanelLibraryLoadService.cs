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
      //private List<PanelSB> _allPanelsSB = new List<PanelSB> ();      

      //public List<PanelSB> AllPanelsSB { get { return _allPanelsSB; } }

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
         // загрузка АКР-панелей из библиотеки
         PanelSB.LoadBtrPanels(facades);                  
         if (facades.Count > 0)
         {
            // расстановка АКР-Панелей по фасадам
            Facade.CreateFacades(facades);
         }
         else
         {
            // простая расстановки имеющихся в бибилтоеке АКР-Панелей
            //PanelAKR.SimpleInsert(_allPanelsSB);
            Inspector.AddError("Не удалось определить фасады по монтажным планам.");
         }
         if (Inspector.HasErrors)
         {
            // Показать ошибки.            
            Inspector.Show();
         }
      }

      /// <summary>
      /// Заполнение марок покраски в блоки монтажных панелей СБ
      /// </summary>
      public void FillMarkPainting(Album album)
      {         
         // Определение фасадов по монтажным планам                  
         List<Facade> facades = Facade.GetFacadesFromMountingPlans(this);
         Inspector.Clear();

         if (facades.Count == 0)
         {
            Log.Info("Не найдены фасады по монтажным планам для заполнения марок покраски в монтажках");
            return;
         }

         // список всех блоков АКР-Панелей      
         foreach (var markSbAkr in album.MarksSB)
         {
            foreach (var markAr in markSbAkr.MarksAR)
            {
               foreach (var panelAr in markAr.Panels)
               {
                  bool isFound = false;
                  // Границы блока АКР-Панели по плитке
                  var extPanelAkr = panelAr.GetExtentsTiles(markSbAkr);
                  double xCenterPanelAkr = extPanelAkr.MinPoint.X + (extPanelAkr.MaxPoint.X - extPanelAkr.MinPoint.X) * 0.5;
                  // поиск фасада - X центра панели АКР попадает в границы фасада Xmin и Xmax
                  var facade = facades.Find(f => f.XMin < xCenterPanelAkr && f.XMax > xCenterPanelAkr);
                  if (facade != null)
                  {
                     // поиск нужного этажа                     
                     var floor = facade.Floors.Find(f=> string.Equals(f.Name, panelAr.Storey.NumberAsNumber, StringComparison.OrdinalIgnoreCase));
                     if (floor != null)
                     {
                        // Поск монтажки по линии от центра панели АКР
                        var mountingPanelSb = floor.PanelsSbInFront.Find(
                           p => p.ExtTransToModel.MinPoint.X < xCenterPanelAkr && p.ExtTransToModel.MaxPoint.X > xCenterPanelAkr);
                        // Проверка имени панели
                        if (mountingPanelSb != null)
                        {
                           string markSbWithoutWhite = mountingPanelSb.MarkSb.Replace(' ', '-');                           
                           string markAkrWithoutWhite = markSbAkr.MarkSbClean.Replace(' ', '-');
                           if (string.Equals(markSbWithoutWhite, markAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
                           {
                              // Заполнение атрибута покраски
                              var atrInfo = mountingPanelSb.AttrDet.Find(a => string.Equals(a.Tag, Album.Options.AttributePanelSbPaint, StringComparison.CurrentCultureIgnoreCase));
                              if (atrInfo != null)
                              {
                                 using (var atrRef = atrInfo.IdAtrRef.Open(OpenMode.ForWrite) as AttributeReference)
                                 {
                                    atrRef.TextString = string.Format("{0}_{1}", markAr.MarkPainting, markSbAkr.Abbr);
                                    isFound = true;
                                 }
                              }
                           }
                        }
                     }
                  }
                  if (!isFound)
                  {
                     Inspector.AddError(
                        string.Format("{0} - Не найдена соответствующая монтажная панель для заполнения атрибута марки покраски.", markAr.MarkARPanelFullName),
                        extPanelAkr, panelAr.IdBlRefAr);
                  }
               }
            }
         }   
      }
   }
}

using System;
using System.Collections.Generic;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // вставка панелей из библиотеки
   public class PanelLibraryLoadService
   {
      public PanelLibraryLoadService()
      {
      }

      public Album Album { get; private set; }
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

      /// <summary>
      /// Заполнение марок покраски в блоки монтажных панелей СБ
      /// </summary>
      public void FillMarkPainting(Album album)
      {
         Album = album;
         // Определение фасадов по монтажным планам
         List<FacadeMounting> facades = FacadeMounting.GetFacadesFromMountingPlans(this);

         //testPtFacades(facades);

         Inspector.Clear();

         if (facades.Count == 0)
         {
            string errMsg = "Не найдены фасады по монтажным планам для заполнения марок покраски в монтажках.";
            Log.Info(errMsg);
            Album.Doc.Editor.WriteMessage("\n{0}", errMsg);
            return;
         }

         // список всех блоков АКР-Панелей
         foreach (var markSbAkr in Album.MarksSB)
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
                  var facadesFound = facades.FindAll(f => f.XMin < xCenterPanelAkr && f.XMax > xCenterPanelAkr);
                  if (facadesFound != null)
                  {
                     foreach (var facade in facadesFound)
                     {
                        // поиск нужного этажа
                        var floor = facade.Floors.Find(f => f.Storey.Equals(panelAr.Storey));
                        if (floor != null)
                        {
                           // Поск монтажки по линии от центра панели АКР
                           var mountingsPanelSb = floor.PanelsSbInFront.FindAll(
                              p => p.ExtTransToModel.MinPoint.X < xCenterPanelAkr && p.ExtTransToModel.MaxPoint.X > xCenterPanelAkr);
                           // Проверка имени панели
                           if (mountingsPanelSb != null)
                           {
                              foreach (var mountingPanelSb in mountingsPanelSb)
                              {
                                 string markSbWithoutWhite = mountingPanelSb.MarkSb.Replace(' ', '-');
                                 string markAkrWithoutWhite = markSbAkr.MarkSbClean.Replace(' ', '-');
                                 if (string.Equals(markSbWithoutWhite, markAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
                                 {
                                    // Проверка индекса окна
                                    if (!string.Equals(mountingPanelSb.WindowSuffix, "ок" + markSbAkr.WindowSuffix, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                       //Inspector.AddError(
                                       //    "Предупреждение. Не совпали индексы окон в монтажной панели и в АКР панели. Панель АКР {0}, Монтажная панель {1}",
                                       //    panelAr.Extents, panelAr.IdBlRefAr);
                                       continue;
                                    }

                                    //Найдена монтажная панель
                                    // Проверка марки покраски
                                    if (Album.StartOptions.CheckMarkPainting)
                                    {
                                       isFound = true;
                                       if (!string.Equals(mountingPanelSb.MarkPainting, markAr.MarkPaintingFull, StringComparison.CurrentCultureIgnoreCase))
                                       {
                                          // Ошибка - марки покраски не совпали.
                                          string errMsg = string.Format("Не совпала марка покраски. Панель АКР {0}, Монтажная панель {1}{2}",
                                                markAr.MarkARPanelFullName, mountingPanelSb.MarkSb, mountingPanelSb.MarkPainting);
                                          Inspector.AddError(errMsg, panelAr.Extents, panelAr.IdBlRefAr);
                                          Log.Error(errMsg);
                                       }
                                       break;
                                    }
                                    // Заполнение атрибута покраски
                                    else
                                    {
                                       var atrInfo = mountingPanelSb.AttrDet.Find (a => 
                                                         string.Equals(a.Tag, Settings.Default.AttributePanelSbPaint,
                                                         StringComparison.CurrentCultureIgnoreCase));
                                       if (atrInfo != null)
                                       {
                                          using (var atrRef = atrInfo.IdAtrRef.Open(OpenMode.ForWrite) as AttributeReference)
                                          {
                                             atrRef.TextString = markAr.MarkPaintingFull;
                                             isFound = true;
                                             break;
                                          }
                                       }
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

      // загрузка АКР-панелей из библиотеки с попыткой расстановить их в виде фасадов если правильно расставлены монтажки
      public void LoadPanels()
      {
         Inspector.Clear();
         // Попытка определить фасады по монтажкам
         List<FacadeMounting> facades = FacadeMounting.GetFacadesFromMountingPlans(this);
         if (Inspector.HasErrors)
         {
            Inspector.Show();
            return;
         }
         if (facades.Count > 0)
         {
            // загрузка АКР-панелей из библиотеки
            MountingPanel.LoadBtrPanels(facades);
            // удаление АКР-Панелей старых фасадов
            FacadeMounting.DeleteOldAkrPanels(facades);
            // расстановка АКР-Панелей по фасадам
            FacadeMounting.CreateFacades(facades);
         }
         else
         {
            Inspector.AddError("Не удалось определить фасады по монтажным планам.");
         }
         if (Inspector.HasErrors)
         {
            // Показать ошибки.
            Inspector.Show();
         }
      }
   }
}
﻿using System;
using System.Linq;
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
        public List<FacadeMounting> Facades { get; private set; }
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
            Facades = FacadeMounting.GetFacadesFromMountingPlans(this);
            var allMountPanels = Facades.SelectMany(s=>s.Panels);

            //testPtFacades(facades);

            Inspector.Clear();

            if (Facades.Count == 0)
            {
                string errMsg = "Не найдены фасады по монтажным планам для заполнения марок покраски в монтажках.";
                Logger.Log.Info(errMsg);
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
                        
                        // Поск монтажки по линии от центра панели АКР
                        var mountingsPanelSb = allMountPanels.Where(p => 
                            p.Floor.Storey.Equals(panelAr.Storey) &&
                            p.ExtTransToModel.MinPoint.X <= xCenterPanelAkr && p.ExtTransToModel.MaxPoint.X >= xCenterPanelAkr);
                        // Проверка имени панели
                        if (mountingsPanelSb.Any())
                        {
                            foreach (var mountingPanelSb in mountingsPanelSb)
                            {
                                string markSbWithoutWhite = mountingPanelSb.MarkSbWithoutElectric.Replace(' ', '-');
                                string markAkrWithoutWhite = AkrHelper.GetMarkWithoutElectric(markSbAkr.MarkSbClean).Replace(' ', '-');
                                if (string.Equals(markSbWithoutWhite, markAkrWithoutWhite, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    //Проверка индекса окна
                                    //if (!album.StartOptions.NewMode &&
                                    //    markSbAkr.WindowIndex != 0 &&
                                    //    !string.Equals(mountingPanelSb.WindowSuffix, markSbAkr.WindowName,
                                    //                   StringComparison.CurrentCultureIgnoreCase)
                                    //   )
                                    //{
                                    //    Inspector.AddError("Предупреждение. Не совпали индексы окон в монтажной панели и в АКР панели. " +
                                    //       $"Панель АКР {markAr.MarkARPanelFullName}, Монтажная панель {mountingPanelSb.MarkSbWithoutElectric}",
                                    //        panelAr.Extents, panelAr.IdBlRefAr, icon: System.Drawing.SystemIcons.Information);
                                    //    //continue;
                                    //}

                                    //Найдена монтажная панель
                                    isFound = true;
                                    // Проверка марки покраски
                                    if (Album.StartOptions.CheckMarkPainting)
                                    {
                                        if (!string.Equals(mountingPanelSb.MarkPainting, markAr.MarkPaintingFull, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            //// Ошибка - марки покраски не совпали.
                                            //string errMsg = $"Не совпала марка покраски. Панель АКР {markAr.MarkARPanelFullName}, " +
                                            //   $"Монтажная панель {mountingPanelSb.MarkSbWithoutElectric}{mountingPanelSb.MarkPainting}";
                                            //Inspector.AddError(errMsg, panelAr.Extents, panelAr.IdBlRefAr, icon: System.Drawing.SystemIcons.Error);
                                            //Logger.Log.Error(errMsg);
                                            ChangeJob.ChangeJobService.AddChangePanel(panelAr, mountingPanelSb);
                                        }
                                        break;
                                    }
                                    // Заполнение атрибута покраски
                                    else
                                    {
                                        mountingPanelSb.SetPaintingToAttr(markAr);
                                    }
                                }
                            }
                        }                        
                        if (!isFound)
                        {
                            Inspector.AddError($"{markAr.MarkARPanelFullName} - Не найдена соответствующая монтажная панель для заполнения атрибута марки покраски.",
                               extPanelAkr, panelAr.IdBlRefAr, icon: System.Drawing.SystemIcons.Error);
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
            Facades = FacadeMounting.GetFacadesFromMountingPlans(this);
            if (Inspector.HasErrors)
            {
                Inspector.Show();
                return;
            }
            if (Facades.Count > 0)
            {
                // загрузка АКР-панелей из библиотеки
                MountingPanel.LoadBtrPanels(Facades);
                // удаление АКР-Панелей старых фасадов
                FacadeMounting.DeleteOldAkrPanels(Facades);
                // расстановка АКР-Панелей по фасадам
                FacadeMounting.CreateFacades(Facades);
            }
            else
            {
                Inspector.AddError("Не удалось определить фасады по монтажным планам.", icon: System.Drawing.SystemIcons.Error);
            }
            if (Inspector.HasErrors)
            {
                // Показать ошибки.
                Inspector.Show();
            }
        }
    }
}
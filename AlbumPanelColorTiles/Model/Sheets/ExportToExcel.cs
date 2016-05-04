using System;
using System.IO;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Panels;
using Microsoft.Office.Interop.Excel;

namespace AlbumPanelColorTiles.Sheets
{
    public static class ExportToExcel
    {
        public static void Export(Album album)
        {
            // Експорт списка панелей в ексель.

            // Открываем приложение
            var excelApp = new Microsoft.Office.Interop.Excel.Application { DisplayAlerts = false };
            if (excelApp == null)
                return;

            // Открываем книгу
            Workbook workBook = excelApp.Workbooks.Add();

            // Содержание  
            try
            {
                Worksheet sheetContent = workBook.ActiveSheet as Worksheet;
                sheetContent.Name = "Содержание";
                sheetContentAlbumFill(sheetContent, album);
            }
            catch { }

            // Секции
            if (album.Sections.Count > 0)
            {
                try
                {
                    Worksheet sheetSections = workBook.Sheets.Add();
                    sheetSections.Name = "Секции";
                    ExportToExcel.sheetSectionFill(sheetSections, album);
                }
                catch { }
            }

            // Плитка
            try
            {
                Worksheet sheetTiles = workBook.Sheets.Add();
                sheetTiles.Name = "Плитка";
                ExportToExcel.sheetTileFill(sheetTiles, album);
            }
            catch { }

            // Этажи
            try
            {
                Worksheet sheetFloors = workBook.Sheets.Add();
                sheetFloors.Name = "Этажи";
                ExportToExcel.sheetFloorFill(sheetFloors, album);
            }
            catch { }

            // Панели
            try
            {
                Worksheet sheetPanels = workBook.Sheets.Add();
                sheetPanels.Name = "Панели";
                ExportToExcel.sheetPanelFill(album, sheetPanels);
            }
            catch { }

            // Ошибки
            if (Inspector.HasErrors)
            {
                try
                {
                    Worksheet sheetError = workBook.Sheets.Add();
                    sheetError.Name = "Ошибки";
                    sheetErrorFill(sheetError, album);
                }
                catch { }
            }

            // Измененные марки покраски
            if (ChangeJob.ChangeJobService.ChangePanels.Count > 0)
            {
                try
                {
                    Worksheet sheetPanels = workBook.Sheets.Add();
                    sheetPanels.Name = "Изменение";
                    sheetChangesFill(sheetPanels, album);
                }
                catch { }
            }

            // Показать ексель.
            // Лучше сохранить файл и закрыть!!!???         
            string excelFile = Path.Combine(album.AlbumDir, "АКР_" + Path.GetFileNameWithoutExtension(album.DwgFacade) + ".xlsx");
            excelApp.Visible = true;
            workBook.SaveAs(excelFile);
        }

        private static int addTitle(Worksheet sheet, Album album)
        {
            int row = 1;
            sheet.Cells[row, 1].Value = string.Format("{0}; {1}", album.Date, album.DwgFacade);
            return row;
        }

        private static void sheetFloorFill(Worksheet sheetFloors, Album album)
        {
            int row = addTitle(sheetFloors, album);
            row++;
            sheetFloors.Cells[row, 1].Value = "Список панелей на этажах";
            row++;
            int totalCountPanels = 0;
            sheetFloors.Cells[row, 1].Value = "Этаж";
            sheetFloors.Cells[row, 2].Value = "Панели";
            sheetFloors.Cells[row, 3].Value = "Кол";
            row++;
            // TODO: Список панелей по этажам.
            var panelsGroupByStoreys = album.MarksSB.SelectMany(sb =>
                     sb.MarksAR).SelectMany(ar => ar.Panels).OrderBy(s => s.Storey).GroupBy(p => p.Storey);
            foreach (var panelGroupStorey in panelsGroupByStoreys)
            {
                totalCountPanels += panelGroupStorey.Count();
                var panelsArOnStorey = panelGroupStorey.OrderBy(p => p.MarkAr.MarkARPanelFullName).GroupBy(p => p.MarkAr);
                foreach (var panelOnStorey in panelsArOnStorey)
                {
                    sheetFloors.Cells[row, 1].Value = panelGroupStorey.Key.ToString(); //"Этаж";
                    sheetFloors.Cells[row, 2].Value = panelOnStorey.Key.MarkARPanelFullName; //"Панели";
                    sheetFloors.Cells[row, 3].Value = panelOnStorey.Count();// Кол
                    row++;
                }
            }
            sheetFloors.Cells[row, 2].Value = "Итого";
            sheetFloors.Cells[row, 3].Value = totalCountPanels;

            sheetFloors.Columns.AutoFit();
            var col1 = sheetFloors.Columns[1];
            col1.ColumnWidth = 5;
            col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            sheetFloors.Columns[3].HorizontalAlignment = XlHAlign.xlHAlignCenter;
        }

        private static void sheetPanelFill(Album album, Worksheet sheetPanels)
        {
            int row = addTitle(sheetPanels, album);
            // Название
            row++;
            sheetPanels.Cells[row, 1].Value = "Список панелей";
            // Заголовки
            row++;
            sheetPanels.Cells[row, 1].Value = "№пп";
            sheetPanels.Cells[row, 2].Value = "Марка АР";
            sheetPanels.Cells[row, 3].Value = "Кол";

            // Записываем данные
            int i = 1;
            int totalCountPanels = 0;
            foreach (var sheetSb in album.SheetsSet.SheetsMarkSB)
            {
                foreach (var sheetAr in sheetSb.SheetsMarkAR)
                {
                    row++;
                    sheetPanels.Cells[row, 1].Value = i++.ToString();
                    sheetPanels.Cells[row, 2].Value = sheetAr.MarkArFullName;
                    sheetPanels.Cells[row, 3].Value = sheetAr.MarkAR.Panels.Count;
                    totalCountPanels += sheetAr.MarkAR.Panels.Count;
                }
            }
            row++;
            sheetPanels.Cells[row, 2].Value = "Итого";
            sheetPanels.Cells[row, 3].Value = totalCountPanels.ToString();

            sheetPanels.Columns.AutoFit();
            var col1 = sheetPanels.Columns[1];
            col1.ColumnWidth = 5;
            col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            sheetPanels.Columns[3].HorizontalAlignment = XlHAlign.xlHAlignCenter;
        }

        private static void sheetSectionFill(Worksheet sheetSections, Album album)
        {
            // Список панелей по Секциям
            int row = addTitle(sheetSections, album);
            row++;
            sheetSections.Cells[row, 1].Value = "Список панелей в секциях";

            row++;
            sheetSections.Cells[row, 1].Value = "№пп";
            sheetSections.Cells[row, 2].Value = "Панель";
            sheetSections.Cells[row, 3].Value = "Кол";
            row++;

            foreach (var section in album.Sections)
            {
                int pp = 1;
                sheetSections.Cells[row, 2].Value = "Секция " + section.Name;
                sheetSections.Cells[row, 2].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                row++;
                var panelByMarkAr = section.Panels.OrderBy(p => p.MarkAr.MarkARPanelFullName).GroupBy(p => p.MarkAr);
                foreach (var panelMarkAr in panelByMarkAr)
                {
                    sheetSections.Cells[row, 1].Value = pp++;
                    sheetSections.Cells[row, 2].Value = panelMarkAr.Key.MarkARPanelFullName;
                    sheetSections.Cells[row, 3].Value = panelMarkAr.Count();
                    row++;
                }
            }

            sheetSections.Columns.AutoFit();
            var col1 = sheetSections.Columns[1];
            col1.ColumnWidth = 5;
            col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            sheetSections.Columns[3].HorizontalAlignment = XlHAlign.xlHAlignCenter;
        }

        private static void sheetTileFill(Worksheet sheetTiles, Album album)
        {
            // Название
            int row = addTitle(sheetTiles, album);
            row++;
            sheetTiles.Cells[row, 1].Value = "Расход плитки на альбом " + album.StartOptions.Abbr;
            row++;

            // Есть ли столбец цвета
            bool hasColorName = Paint.HasColorName(album.Colors);

            // Заголовки
            sheetTiles.Cells[row, 1].Value = "Поз.";
            sheetTiles.Cells[row, 2].Value = "Артикул";
            sheetTiles.Cells[row, 3].Value = "Образец";
            sheetTiles.Cells[row, 4].Value = "Расход, шт.";
            sheetTiles.Cells[row, 5].Value = "Расход, м.кв.";
            if (hasColorName)
            {
                sheetTiles.Cells[row, 6].Value = "Цвет";
            }
            row++;
            int i = 1;
            int totalCountTile = 0;
            double totalArea = 0;

            foreach (var tileCalcSameColor in album.TotalTilesCalc)
            {
                sheetTiles.Cells[row, 1].Value = i++.ToString(); //"Поз.";
                sheetTiles.Cells[row, 2].Value = tileCalcSameColor.Paint.Article;  //"Артикул";
                sheetTiles.Cells[row, 3].Interior.Color = System.Drawing.ColorTranslator.ToOle(tileCalcSameColor.Paint.Color.ColorValue);// "Образец";
                sheetTiles.Cells[row, 4].Value = tileCalcSameColor.Count.ToString();// "Расход, шт.";
                sheetTiles.Cells[row, 5].Value = tileCalcSameColor.TotalArea.ToString();  // "Расход, м.кв.";
                if (hasColorName)
                {
                    sheetTiles.Cells[row, 6].Value = tileCalcSameColor.Paint.Name;  //"Цвет";
                }

                totalCountTile += tileCalcSameColor.Count;
                totalArea += tileCalcSameColor.TotalArea;

                row++;
            }
            sheetTiles.Range[sheetTiles.Cells[row, 1], sheetTiles.Cells[row, 3]].Merge();
            sheetTiles.Cells[row, 1].Value = "Итого:";
            sheetTiles.Cells[row, 4].Value = totalCountTile.ToString();
            sheetTiles.Cells[row, 5].Value = totalArea.ToString();

            sheetTiles.Columns.AutoFit();
            var col1 = sheetTiles.Columns[1];
            col1.ColumnWidth = 5;
            col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            sheetTiles.Columns[4].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            sheetTiles.Columns[5].HorizontalAlignment = XlHAlign.xlHAlignCenter;
        }

        private static void sheetContentAlbumFill(Worksheet sheetContent, Album album)
        {
            int row = addTitle(sheetContent, album);
            row++;
            sheetContent.Cells[row, 1].Value = "Содержание альбома панелей";
            row++;
            sheetContent.Cells[row, 1].Value = "Лист";
            sheetContent.Cells[row, 2].Value = "Панель";
            row++;
            foreach (var sheetMarkSb in album.SheetsSet.SheetsMarkSB)
            {
                foreach (var sheetMarkAr in sheetMarkSb.SheetsMarkAR)
                {
                    sheetContent.Cells[row, 1].Value = sheetMarkAr.SheetNumber; //Лист;
                    sheetContent.Cells[row, 2].Value = sheetMarkAr.MarkAR.MarkARPanelFullName; //"Панели";               
                    row++;
                }
            }

            sheetContent.Columns.AutoFit();
            var col1 = sheetContent.Columns[1];
            col1.ColumnWidth = 5;
            col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
        }

        private static void sheetErrorFill(Worksheet sheetError, Album album)
        {
            int row = addTitle(sheetError, album);
            row++;
            sheetError.Cells[row, 1].Value = "Ошибки при создании альбома";
            row++;
            foreach (var error in Inspector.Errors)
            {
                sheetError.Cells[row, 1].Value = error.Message;
                row++;
            }
            sheetError.Columns.AutoFit();
        }

        private static void sheetChangesFill(Worksheet sheetChange, Album album)
        {
            int row = addTitle(sheetChange, album);
            row++;
            sheetChange.Cells[row, 1].Value = "Измененные марки покраски";
            row++;
            sheetChange.Cells[row, 1].Value = "№пп";
            sheetChange.Cells[row, 2].Value = "Панель";
            sheetChange.Cells[row, 3].Value = "Старая марка";
            sheetChange.Cells[row, 4].Value = "Новая марка";
            sheetChange.Cells[row, 5].Value = "Секция";
            sheetChange.Cells[row, 6].Value = "Этаж";
            row++;
            int count = 1;

            var chPanelsSorted = ChangeJob.ChangeJobService.ChangePanels.OrderBy(n=>n.MarkSb)
                .ThenBy(s => s.PanelMount.Floor.Section).ThenBy(f => f.PanelMount.Floor.Storey);
            foreach (var chPanel in chPanelsSorted)
            {
                sheetChange.Cells[row, 1].Value = count++.ToString();
                sheetChange.Cells[row, 2].Value = chPanel.MarkSb;
                sheetChange.Cells[row, 3].Value = chPanel.PaintOld;
                sheetChange.Cells[row, 4].Value = chPanel.PaintNew;
                sheetChange.Cells[row, 5].Value = chPanel.PanelMount.Floor.Section.ToString();
                sheetChange.Cells[row, 6].Value = chPanel.PanelMount.Floor.Storey.ToString();
                row++;
            }
            sheetChange.Columns.AutoFit();
            var col1 = sheetChange.Columns[1];
            col1.ColumnWidth = 5;
        }
    }
}
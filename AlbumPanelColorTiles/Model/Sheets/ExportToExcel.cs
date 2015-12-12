using Microsoft.Office.Interop.Excel;
using System.Linq;

namespace AlbumPanelColorTiles.Sheets
{
   public static class ExportToExcel
   {
      public static void Export(SheetsSet sheetsSet, Album album)
      {
         // Експорт списка панелей в ексель.

         // Открываем приложение
         var excelApp = new Microsoft.Office.Interop.Excel.Application { DisplayAlerts = false };
         if (excelApp == null)
            return;

         // Открываем книгу
         Workbook workBook = excelApp.Workbooks.Add();

         // Получаем активную таблицу
         Worksheet worksheet = workBook.ActiveSheet as Worksheet;
         worksheet.Name = "Panels";

         int row = 1;
         // Название
         worksheet.Cells[row, 1].Value = "Панели Марки АР к чертежу фасада " + album.DwgFacade;
         // Заголовки
         row++;
         worksheet.Cells[row, 1].Value = "№пп";
         worksheet.Cells[row, 2].Value = "Марка АР";
         worksheet.Cells[row, 3].Value = "Кол блоков";

         // Записываем данные
         int i = 1;
         int totalCountPanels = 0;
         foreach (var sheetSb in sheetsSet.SheetsMarkSB)
         {
            foreach (var sheetAr in sheetSb.SheetsMarkAR)
            {
               row++;
               worksheet.Cells[row, 1].Value = i++.ToString();
               worksheet.Cells[row, 2].Value = sheetAr.MarkArFullName;
               worksheet.Cells[row, 3].Value = sheetAr.MarkAR.Panels.Count;
               totalCountPanels += sheetAr.MarkAR.Panels.Count;
            }
         }
         row++;
         worksheet.Cells[row, 2].Value = "Итого";
         worksheet.Cells[row, 3].Value = totalCountPanels.ToString();

         worksheet.Columns.AutoFit();
         var col1 = worksheet.Columns[1];
         col1.ColumnWidth = 5;
         col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
         worksheet.Columns[3].HorizontalAlignment = XlHAlign.xlHAlignCenter;

         // Список панелей по этажам
         listPanelsOnFloors(workBook, album, sheetsSet);

         // Показать ексель.
         // Лучше сохранить файл и закрыть!!!???
         excelApp.Visible = true;
      }

      private static void listPanelsOnFloors(Workbook workBook, Album album, SheetsSet sheetsSet)
      {
         Worksheet sheetFloor = workBook.Sheets.Add();
         sheetFloor.Name = "Floors";
         int row = 1;
         int totalCountPanels = 0;
         sheetFloor.Cells[row, 1].Value = "Этаж";
         sheetFloor.Cells[row, 2].Value = "Панели";
         sheetFloor.Cells[row, 3].Value = "Кол";
         row++;
         // TODO: Список панелей по этажам.
         var panelsGroupByStoreys = album.MarksSB.SelectMany(sb => 
                  sb.MarksAR).SelectMany(ar=>ar.Panels).OrderBy(s=>s.Storey).GroupBy(p=>p.Storey);
         foreach (var panelGroupStorey in panelsGroupByStoreys)
         {
            totalCountPanels += panelGroupStorey.Count();
            var panelsArOnStorey = panelGroupStorey.OrderBy(p=>p.MarkAr.MarkARPanelFullName).GroupBy(p => p.MarkAr);
            foreach (var panelOnStorey in panelsArOnStorey)
            {
               sheetFloor.Cells[row, 1].Value = panelGroupStorey.Key.ToString(); //"Этаж";               
               sheetFloor.Cells[row, 2].Value = panelOnStorey.Key.MarkARPanelFullName; //"Панели";
               sheetFloor.Cells[row, 3].Value = panelOnStorey.Count();// Кол
               row++;
            }            
         }
         sheetFloor.Cells[row, 2].Value ="Итого";
         sheetFloor.Cells[row, 3].Value = totalCountPanels;

         sheetFloor.Columns.AutoFit();
         var col1 = sheetFloor.Columns[1];
         col1.ColumnWidth = 5;
         col1.HorizontalAlignment = XlHAlign.xlHAlignLeft;
         sheetFloor.Columns[3].HorizontalAlignment = XlHAlign.xlHAlignCenter;
      }
   }
}
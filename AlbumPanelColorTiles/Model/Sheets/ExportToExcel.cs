using Microsoft.Office.Interop.Excel;

namespace AlbumPanelColorTiles.Model.Sheets
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

         int row = 1;
         // Название
         worksheet.Cells[row, 1].Value = "Панели Марки АР к чертежу фасада " + album.DwgFacade;
         // Заголовки
         row++;
         worksheet.Cells[row, 1].Value = "Марка АР";
         worksheet.Cells[row, 2].Value = "Кол блоков";

         // Записываем данные
         foreach (var sheetSb in sheetsSet.SheetsMarkSB)
         {
            foreach (var sheetAr in sheetSb.SheetsMarkAR)
            {
               row++;
               worksheet.Cells[row, 1].Value = sheetAr.MarkArDocumentation;
               worksheet.Cells[row, 2].Value = sheetAr.MarkAR.Panels.Count;
            }
         }

         // Показать ексель.
         // Лучше сохранить файл и закрыть!!!???
         excelApp.Visible = true;
      }
   }
}
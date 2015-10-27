using System;
using System.Linq;
using Microsoft.Office.Interop.Excel;

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
         worksheet.Cells[row, 1].Value = "Марка АР";
         worksheet.Cells[row, 2].Value = "Кол блоков";

         // Записываем данные
         foreach (var sheetSb in sheetsSet.SheetsMarkSB)
         {
            foreach (var sheetAr in sheetSb.SheetsMarkAR)
            {
               row++;
               worksheet.Cells[row, 1].Value = sheetAr.MarkArFullName;//.MarkArDocumentation;
               worksheet.Cells[row, 2].Value = sheetAr.MarkAR.Panels.Count;
            }
         }

         // выгрузка списка панелей по этажам
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
         sheetFloor.Cells[row, 1].Value = "Этаж";
         sheetFloor.Cells[row, 2].Value = "Панели";
         
         foreach (var storey in album.Storeys)
         {            
            foreach (var markAr in storey.MarksAr)
            {
               sheetFloor.Cells[++row, 1].Value = storey.Number;
               sheetFloor.Cells[row, 2].Value = markAr.MarkARPanelFullName;
            }
         } 
      }
   }
}
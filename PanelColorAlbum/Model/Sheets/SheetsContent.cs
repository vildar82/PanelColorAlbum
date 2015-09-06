﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Vil.Acad.AR.PanelColorAlbum.Model.Lib;

namespace Vil.Acad.AR.PanelColorAlbum.Model.Sheets
{
   // Листы содержания
   public class SheetsContent
   {
      private SheetsSet _sheetsSet;
      private Album _album;
      private List<SheetMarkSB> _sheetsMarkSB;
      private string _albumDir;
      private int _countContentSheets;
      private BlockReference _blRefStampOnFirstContentSheet;
      private readonly int _countSheetsBeforContent = 2; // кол листов до содержания
      private readonly int _firstRowInTableForSheets = 2;

      public SheetsContent (SheetsSet sheetsSet)
      {
         _sheetsSet = sheetsSet;
         _album = sheetsSet.Album;
         _sheetsMarkSB = sheetsSet.SheetsMarkSB;
         _albumDir = sheetsSet.AlbumDir;
         Contents();
      }

      // Содержание тома (Общие данные. Ведомость комплекта чертежей.)
      private void Contents()
      {
         // Создание файла содержания и титульных листов
         string fileContent = Path.Combine(_albumDir, "Содержание" + _album.AbbreviateProject + ".dwg");
         File.Copy(_sheetsSet.SheetTemplateFileContent, fileContent);

         // Кол листов содержания = Суммарное кол лисчтов панелей / на кол строк в таблице на одном листе
         // Но на первом листе содержания первые 5 строк заняты (обложка, тит, общие данные, наруж стен панели, том1).
         Database dbOrig = HostApplicationServices.WorkingDatabase;
         using (Database dbContent = new Database(false, true))
         {
            dbContent.ReadDwgFile(fileContent, FileShare.ReadWrite, false, "");
            dbContent.CloseInput(true);

            //Копирование листа "Содержание" для следующего листа содержания (заполнять буду предыдущий лист содержания).            
            using (var t = dbContent.TransactionManager.StartTransaction())
            {
               int curContentLayout = 1;
               Table tableContent;
               CopyContentSheet(dbContent, t, curContentLayout, out tableContent);
               // Определение кол-ва марок АР
               int countMarkArs = CalcMarksArNumber(_sheetsMarkSB);
               // Определение кол-ва листов содержания (только для панелей без учета лтстов обложек и тп.)
               _countContentSheets = CalcSheetsContentNumber(tableContent.Rows.Count, countMarkArs);
               // Заполнение штампа на первом листе содержания
               FillingStampContent(_blRefStampOnFirstContentSheet, curContentLayout, t);
               // текущая строка для записи листа               
               int row = _firstRowInTableForSheets;
               // На первом листе содержания заполняем строки для Обложки, Тит, Общ дан, НСП, Том1.
               tableContent.Cells[row++, 1].TextString = "Обложка";
               tableContent.Cells[row++, 1].TextString = "Титульный лист";
               tableContent.Cells[row, 1].TextString = "Общие данные. Ведомость комплекта чертежей (начало)";
               tableContent.Cells[row, 2].TextString = _countContentSheets > 1 ? "3-" + (3 + _countContentSheets).ToString() : "3";
               tableContent.Cells[row, 2].Alignment = CellAlignment.MiddleCenter;
               row++;
               tableContent.Cells[row++, 1].TextString = "Наружные стеновые панели";
               tableContent.Cells[row++, 1].TextString = "ТОМ";

               int curSheetArNum = _countContentSheets + _countSheetsBeforContent;// номер для первого листа Марки АР               
               foreach (var sheetMarkSB in _sheetsMarkSB)
               {
                  foreach (var sheetMarkAR in sheetMarkSB.SheetsMarkAR)
                  {                     
                     tableContent.Cells[row, 1].TextString = sheetMarkAR.MarkArFullName + ".Раскладка плитки на фасаде";
                     sheetMarkAR.SheetNumber = ++curSheetArNum;
                     tableContent.Cells[row, 2].TextString = curSheetArNum.ToString();
                     tableContent.Cells[row, 2].Alignment = CellAlignment.MiddleCenter;
                     row++;
                     if (row == tableContent.Rows.Count)
                     {
                        // Новый лист содержания
                        tableContent.RecomputeTableBlock(true);
                        CopyContentSheet(dbContent, t, ++curContentLayout, out tableContent);
                        row = _firstRowInTableForSheets;
                     }
                     tableContent.Cells[row, 1].TextString = sheetMarkAR.MarkArFullName + ".Раскладка плитки в форме";
                     tableContent.Cells[row, 2].TextString = curSheetArNum.ToString() + ".1";
                     sheetMarkAR.SheetNumberInForm = curSheetArNum.ToString() + ".1";
                     tableContent.Cells[row, 2].Alignment = CellAlignment.MiddleCenter;
                     row++;
                     if (row == tableContent.Rows.Count)
                     {
                        // Новый лист содержания
                        tableContent.RecomputeTableBlock(true);
                        CopyContentSheet(dbContent, t, ++curContentLayout, out tableContent);
                        row = _firstRowInTableForSheets;
                     }                     
                  }
               }

               //Удаление пустых строк в таблице содержания
               if (tableContent.Rows.Count > row+1)
               {
                  tableContent.DeleteRows(row, tableContent.Rows.Count-row);
               }

               // Удаление последнего листа содержания (пустой копии)
               HostApplicationServices.WorkingDatabase = dbContent;
               LayoutManager lm = LayoutManager.Current;
               lm.DeleteLayout(Album.Options.SheetTemplateLayoutNameForContent + (++_countContentSheets).ToString());

               t.Commit();
            }
            HostApplicationServices.WorkingDatabase = dbOrig;
            dbContent.SaveAs(fileContent, DwgVersion.Current);
         }
      }

      // Заполнение штампа содержания.
      private void FillingStampContent(BlockReference blRefStamp, int curContentLayout, Transaction t)
      {
         if (_countContentSheets == 0)
         {
            // Еще не определено кол листов содержания.
            // значит это первый лист содержания.
            _blRefStampOnFirstContentSheet = blRefStamp;
            // Его заполним вконце 
         }
         else
         {
            var atrs = blRefStamp.AttributeCollection;
            foreach (ObjectId idAtrRef in atrs)
            {
               var atrRef = t.GetObject(idAtrRef, OpenMode.ForRead) as AttributeReference;
               string text = string.Empty; 
               if (atrRef.Tag.Equals("Наименование", StringComparison.OrdinalIgnoreCase) )
               {
                  if (curContentLayout == 1 && _countContentSheets>1)
                     text = "Общие данные. Ведомость комплекта чертежей (начало)";
                  if (curContentLayout == 1 && _countContentSheets == 1)
                     text = "Общие данные. Ведомость комплекта чертежей.";
                  else if (curContentLayout == _countContentSheets)
                     text = "Общие данные. Ведомость комплекта чертежей (окончание)";
                  else                  
                     text = "Общие данные. Ведомость комплекта чертежей (продолжение)";                  
               }
               else if (atrRef.Tag.Equals("Лист", StringComparison.OrdinalIgnoreCase))
               {
                  text = (_countSheetsBeforContent + curContentLayout).ToString();
               }
               if (text != string.Empty)
               {
                  atrRef.UpgradeOpen();
                  atrRef.TextString = text; 
               }               
            } 
         }
      }

      private Layout GetCurLayoutContentAndCopyNext(int curSheetContentNum, Transaction t, Database db)
      {
         ObjectId idLayoutContentCur;
         HostApplicationServices.WorkingDatabase = db;
         LayoutManager lm = LayoutManager.Current;
         string nameLay;
         string nameCopy;
         if (curSheetContentNum == 1)
         {
            nameLay = Album.Options.SheetTemplateLayoutNameForContent;
            nameCopy = Album.Options.SheetTemplateLayoutNameForContent + (++curSheetContentNum).ToString();
            idLayoutContentCur = lm.GetLayoutId(nameLay);
            lm.CopyLayout(nameLay, nameCopy);
            lm.RenameLayout(nameLay, nameLay + "1");
         }
         else
         {
            nameLay = Album.Options.SheetTemplateLayoutNameForContent + curSheetContentNum.ToString();
            nameCopy = Album.Options.SheetTemplateLayoutNameForContent + (++curSheetContentNum).ToString();
            idLayoutContentCur = lm.GetLayoutId(nameLay);
            lm.CopyLayout(nameLay, nameCopy);
         }
         return t.GetObject(idLayoutContentCur, OpenMode.ForRead) as Layout;
      }

      // Определение кол-ва марок АР
      private int CalcMarksArNumber(List<SheetMarkSB> marksSB)
      {
         return marksSB.Sum(sb => sb.SheetsMarkAR.Count);
      }

      // Определение кол листов содержания по кол марок Ар и кол строк в таблице содержания на одном листе.      
      private static int CalcSheetsContentNumber(int rowsTable, int marksAr)
      {
         int res = 0; // Нужное кол листов содержания
         int rowsToSheetsInTable = rowsTable - 2; // строк в таблице под листы
         int rowsInFirstSheet = rowsToSheetsInTable - 5;// строк под листы марки АР на первом листе содерж (без облажки тит и т.п )                 
         int numSheetsMarksAr = marksAr * 2; // кол листов марок АР (листы фасада и форм)         
         numSheetsMarksAr -= rowsInFirstSheet; // вычитаем листы первого листа содержания
         res = 1;
         res += numSheetsMarksAr / rowsToSheetsInTable; // целое кол листов         
         var remaindSheets = numSheetsMarksAr % rowsToSheetsInTable; // остаток листов
         if (remaindSheets > 0)
         {
            res++;
         }
         return res;
      }

      private BlockReference FindBlRefStampContent(BlockTableRecord btrLayoutContent, Transaction t)
      {
         foreach (ObjectId idEnt in btrLayoutContent)
         {
            if (idEnt.ObjectClass.Name == "AcDbBlockReference")
            {
               var blRefStampContent = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
               if (Blocks.EffectiveName(blRefStampContent) == Album.Options.BlockStampContent)
               {
                  return blRefStampContent;
               }
            }
         }
         throw new Exception("Не найден блок штампа в шаблоне содержания.");
      }

      private Table FindTableContent(BlockTableRecord btrLayoutContent, Transaction t)
      {
         // Поиск таблицы
         foreach (ObjectId idEnt in btrLayoutContent)
         {
            if (idEnt.ObjectClass.Name == "AcDbTable")
            {
               return t.GetObject(idEnt, OpenMode.ForWrite) as Table;
            }
         }
         throw new Exception("Не найдена заготовка таблицы в шаблоне содержания.");
      }

      private void CopyContentSheet(Database dbContent, Transaction t, int curContentLayout, out Table tableContent)
      {
         Layout layout = GetCurLayoutContentAndCopyNext(curContentLayout, t, dbContent);
         var btrLayoutContent = t.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
         tableContent = FindTableContent(btrLayoutContent, t);
         BlockReference blRefStamp = FindBlRefStampContent(btrLayoutContent, t);
         // Заполнение штампа содержания.
         FillingStampContent(blRefStamp, curContentLayout, t);
      }
   }
}

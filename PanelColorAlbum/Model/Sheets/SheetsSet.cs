using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Vil.Acad.AR.PanelColorAlbum.Model.Lib;

namespace Vil.Acad.AR.PanelColorAlbum.Model.Sheets
{
   // Ведомость альбома панелей
   public class SheetsSet
   {
      private Album _album;
      private string _albumDir;      
      private List<SheetMarkSB> _sheetsMarkSB;

      public string AlbumDir { get { return _albumDir; } }

      public SheetsSet(Album album)
      {
         _album = album;
         _sheetsMarkSB = new List<SheetMarkSB>();
      }

      // Создание альбома панелей
      public void CreateAlbum()
      {
         // Проверка наличия файла шаблона листов         
         if (!File.Exists(Album.Options.SheetTemplateFileMarkSB))         
            throw new Exception("\nНе найден файл шаблона для листов панелей - " + Album.Options.SheetTemplateFileMarkSB);
         if (!File.Exists(Album.Options.SheetTemplateFileContent))
            throw new Exception("\nНе найден файл шаблона для содержания альбома - " + Album.Options.SheetTemplateFileContent);

         // Создаение папки для альбома панелей
         CreateAlbumFolder();

         // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
         _sheetsMarkSB = ProcessingSheets(_album.MarksSB);

         // Титульные листы и обложеи в одном файле "Содержание".
         // Создание титульных листов         
         // Листы содержания
         Contents(_sheetsMarkSB);

         // Создание файлов марок СБ и листов марок АР в них.
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            sheetMarkSB.CreateFileMarkSB(); 
         }
      }     

      // Содержание тома (Общие данные. Ведомость комплекта чертежей.)
      private void Contents(List<SheetMarkSB> marksSB)
      {
         // Создание файла содержания и титульных листов
         string fileContent = Path.Combine(_albumDir, "Содержание" + _album.AbbreviateProject + ".dwg");
         File.Copy(Album.Options.SheetTemplateFileContent, fileContent);

         // Кол листов содержания = Суммарное кол лисчтов панелей / на кол строк в таблице на одном листе
         // Но на первом листе содержания первые 5 строк заняты (обложка, тит, общие данные, наруж стен панели, том1).
         Database dbPrev = HostApplicationServices.WorkingDatabase; 
         using (Database dbContent = new Database(false, true))
         {
            dbContent.ReadDwgFile(fileContent, FileShare.ReadWrite, false, "");
            dbContent.CloseInput(true);            

            //Копирование листа "Содержание" для следующего листа содержания (заполнять буду предыдущий лист содержания).            
            using (var t = dbContent.TransactionManager.StartTransaction())
            {
               int curContentLayout = 0;
               BlockTableRecord btrLayoutContent;
               Table tableContent;
               CopyContentSheet(dbContent, t, curContentLayout, out btrLayoutContent, out tableContent);
               // Определение кол-ва марок АР
               int countMarkArs = CalcMarksArNumber(marksSB);
               // Определение кол-ва листов содержания
               int countContentSheets = CalcSheetsContentNumber(tableContent.Rows.Count, countMarkArs);
               // текущая строка для записи листа               
               int row = 3;
               // На первом листе содержания заполняем строки для Обложки, Тит, Общ дан, НСП, Том1.
               tableContent.Cells[row++, 1].TextString = "Обложка";
               tableContent.Cells[row++, 1].TextString = "Титульный лист";
               tableContent.Cells[row++, 1].TextString = "Общие данные. Ведомость комплекта чертежей (начало)";
               tableContent.Cells[row++, 2].TextString = "3-" + countContentSheets.ToString();
               tableContent.Cells[row++, 1].TextString = "Наружные стеновые панели";
               tableContent.Cells[row++, 1].TextString = "Том";

               int curSheetArNum = countContentSheets + 1;// номер для первого листа Марки АР               
               foreach (var markSB in marksSB)
               {
                  foreach (var markAR in markSB.SheetsMarkAR)
                  {
                     markAR.SheetNumber = curSheetArNum++;
                     tableContent.Cells[row, 1].TextString = markAR.MarkAR + ".Раскладка плитки на фасаде";
                     tableContent.Cells[row, 2].TextString = curSheetArNum.ToString();
                     row++;
                     if (row > tableContent.Rows.Count)
                     {
                        // Новый лист содержания
                        CopyContentSheet(dbContent, t, curContentLayout, out btrLayoutContent, out tableContent);
                        row = 3;
                     }
                     tableContent.Cells[row, 1].TextString = markAR.MarkAR + ".Раскладка плитки на фасаде";
                     tableContent.Cells[row, 2].TextString = curSheetArNum.ToString() + ".1";
                     row++;
                     if (row > tableContent.Rows.Count)
                     {
                        // Новый лист содержания
                        CopyContentSheet(dbContent, t, curContentLayout, out btrLayoutContent, out tableContent);
                        row = 3;
                     }
                  }
               }
               t.Commit();
            }
            dbContent.SaveAs(fileContent, DwgVersion.Current);
         }
      }

      private void CopyContentSheet(Database dbContent, Transaction t, int curContentLayout, out BlockTableRecord btrLayoutContent, out Table tableContent)
      {
         Layout layout = GetCurLayoutContentAndCopyNext(curContentLayout, t, dbContent);
         btrLayoutContent = t.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
         tableContent = FindTableContent(btrLayoutContent, t);
         BlockReference blRefStamp = FindBlRefStampContent(btrLayoutContent, t);
      }

      private Layout GetCurLayoutContentAndCopyNext(int curSheetContentNum, Transaction t, Database db)
      {         
         HostApplicationServices.WorkingDatabase = db;
         LayoutManager lm = LayoutManager.Current;
         ObjectId idLayoutContentCur;
         string nameLay;
         string nameCopy;
         if (curSheetContentNum == 0)
         {
            nameLay = Album.Options.SheetTemplateLayoutNameForContent;            
            nameCopy = Album.Options.SheetTemplateLayoutNameForContent + curSheetContentNum.ToString();
         }
         else
         {
            nameLay = Album.Options.SheetTemplateLayoutNameForContent + curSheetContentNum.ToString();
            nameCopy = Album.Options.SheetTemplateLayoutNameForContent + curSheetContentNum++.ToString();
         }
         idLayoutContentCur = lm.GetLayoutId(nameLay);
         lm.CopyLayout(nameLay, nameCopy);
         return  t.GetObject(idLayoutContentCur, OpenMode.ForRead) as Layout;
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
         int rowsInFirstSheet = rowsToSheetsInTable - 5;// листы облажки тит и т.п                  
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

      // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
      private List<SheetMarkSB> ProcessingSheets(List<MarkSbPanel> marksSB)
      {
         List<SheetMarkSB> sheetsMarkSb = new List<SheetMarkSB>();
         foreach (var markSB in marksSB)
         {
            // Создание листа марки СБ
            SheetMarkSB sheetMarkSb = new SheetMarkSB(markSB);
            sheetsMarkSb.Add(sheetMarkSb);
         }
         // Сортировка
         sheetsMarkSb.Sort();
         return sheetsMarkSb;
      }

      // Создание папки Альбома панелей
      private void CreateAlbumFolder()
      {
         // Папка альбома панелей
         string albumFolderName = "Альбом панелей";
         string curDwgFacadeFolder = Path.GetDirectoryName(_album.Doc.Name);
         _albumDir = Path.Combine(curDwgFacadeFolder, albumFolderName);
         if (Directory.Exists(_albumDir))
         {            
            Directory.Delete(_albumDir, true);
         }
         Directory.CreateDirectory(_albumDir);
      }
   }
}
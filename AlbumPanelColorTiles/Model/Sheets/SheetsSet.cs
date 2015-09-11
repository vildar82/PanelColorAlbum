using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model.Sheets
{
   // Ведомость альбома панелей
   public class SheetsSet
   {
      private Album _album;
      private string _albumDir;
      private List<SheetMarkSB> _sheetsMarkSB;
      private string _sheetTemplateFileContent;
      private string _sheetTemplateFileMarkSB;
      public SheetsSet(Album album)
      {
         _album = album;
         _sheetsMarkSB = new List<SheetMarkSB>();
      }

      public Album Album { get { return _album; } }
      public string AlbumDir { get { return _albumDir; } }
      public List<SheetMarkSB> SheetsMarkSB { get { return _sheetsMarkSB; } }
      public string SheetTemplateFileContent { get { return _sheetTemplateFileContent; } }
      public string SheetTemplateFileMarkSB { get { return _sheetTemplateFileMarkSB; } }

      // Создание альбома панелей
      public void CreateAlbum()
      {
         //Создание файлов марок СБ и листов марок АР в них.
         // Проверка наличия файла шаблона листов
         _sheetTemplateFileMarkSB = Path.Combine(Commands.CurDllDir, Album.Options.TemplateSheetMarkSBFileName);
         _sheetTemplateFileContent = Path.Combine(Commands.CurDllDir, Album.Options.TemplateSheetContentFileName);
         if (!File.Exists(_sheetTemplateFileMarkSB))
            throw new System.Exception("\nНе найден файл шаблона для листов панелей - " + _sheetTemplateFileMarkSB);
         if (!File.Exists(_sheetTemplateFileContent))
            throw new System.Exception("\nНе найден файл шаблона для содержания альбома - " + _sheetTemplateFileContent);         

         // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
         _sheetsMarkSB = ProcessingSheets(_album.MarksSB);
         if (_sheetsMarkSB.Count ==0)
         {
            throw new System.Exception("Не определены панели марок АР");
         }         

         // Создаение папки для альбома панелей
         CreateAlbumFolder();         

         // Титульные листы и обложеи в одном файле "Содержание".
         // Создание титульных листов
         // Листы содержания
         SheetsContent content = new SheetsContent(this);

         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.SetLimit(_sheetsMarkSB.Count);
         progressMeter.Start("Создание файлов панелей марок СБ с листами марок АР...");
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            progressMeter.MeterProgress();
            sheetMarkSB.CreateSheetMarkSB(this);
         }
         progressMeter.Stop();
                  
         // Еспорт списка панелей в ексель.
         ExportToExcel.Export(this, _album);         
      }

      // Создание папки Альбома панелей
      private void CreateAlbumFolder()
      {
         // Папка альбома панелей
         string albumFolderName = "Альбом панелей";
         string curDwgFacadeFolder = Path.GetDirectoryName(_album.DwgFacade);
         _albumDir = Path.Combine(curDwgFacadeFolder, albumFolderName);
         if (Directory.Exists(_albumDir))
         {
            Directory.Delete(_albumDir, true);
         }
         Directory.CreateDirectory(_albumDir);
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
   }
}
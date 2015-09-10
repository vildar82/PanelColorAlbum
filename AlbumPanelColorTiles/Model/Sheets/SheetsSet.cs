using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
         // Проверка наличия файла шаблона листов
         _sheetTemplateFileMarkSB = GetTemplateFile(Album.Options.SheetTemplateFileMarkSB, true);
         _sheetTemplateFileContent = GetTemplateFile(Album.Options.SheetTemplateFileContent, false);
         if (!File.Exists(_sheetTemplateFileMarkSB))
            throw new Exception("\nНе найден файл шаблона для листов панелей - " + _sheetTemplateFileMarkSB);
         if (!File.Exists(_sheetTemplateFileContent))
            throw new Exception("\nНе найден файл шаблона для содержания альбома - " + _sheetTemplateFileContent);

         // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
         _sheetsMarkSB = ProcessingSheets(_album.MarksSB);
         if (_sheetsMarkSB.Count ==0)
         {
            throw new Exception ("Не определены панели марок АР");
         }

         // Создаение папки для альбома панелей
         CreateAlbumFolder();         

         // Титульные листы и обложеи в одном файле "Содержание".
         // Создание титульных листов
         // Листы содержания
         SheetsContent content = new SheetsContent(this);

         //Создание файлов марок СБ и листов марок АР в них.
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            sheetMarkSB.CreateSheetMarkSB(this);
         }

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

      private string GetTemplateFile(string optionsPathToTemplate, bool markSbOrContent)
      {
         if (optionsPathToTemplate == "root")
         {
            string fileName;
            if (markSbOrContent)
               fileName = "АКР_Шаблон_МаркаСБ.dwg";
            else
               fileName = "АКР_Шаблон_Содержание.dwg";
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
         }
         else
            return optionsPathToTemplate;
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
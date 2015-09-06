using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Lib;
using System.Reflection;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model.Sheets
{
   // Ведомость альбома панелей
   public class SheetsSet
   {
      private Album _album;
      private string _albumDir;      
      private List<SheetMarkSB> _sheetsMarkSB;
      private string _sheetTemplateFileMarkSB;
      private string _sheetTemplateFileContent;

      public string AlbumDir { get { return _albumDir; } }
      public string SheetTemplateFileMarkSB { get { return _sheetTemplateFileMarkSB; } }
      public string SheetTemplateFileContent { get { return _sheetTemplateFileContent; } }
      public Album Album { get { return _album; } }
      public List<SheetMarkSB> SheetsMarkSB { get { return _sheetsMarkSB; } }


      public SheetsSet(Album album)
      {
         _album = album;
         _sheetsMarkSB = new List<SheetMarkSB>();
      }

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

         // Создаение папки для альбома панелей
         CreateAlbumFolder();

         // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
         _sheetsMarkSB = ProcessingSheets(_album.MarksSB);

         // Титульные листы и обложеи в одном файле "Содержание".
         // Создание титульных листов         
         // Листы содержания
         SheetsContent content = new SheetsContent(this);         

         //Создание файлов марок СБ и листов марок АР в них.
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            sheetMarkSB.CreateFileMarkSB(this);
         }
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
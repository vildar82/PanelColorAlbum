﻿using System;
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
         SheetsContent content = new SheetsContent(_album, _sheetsMarkSB, _albumDir);         

         //Создание файлов марок СБ и листов марок АР в них.
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            sheetMarkSB.CreateFileMarkSB(_albumDir);
         }
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Библиотека панелей покраски.
   // DWG файл
   public class PanelLibraryService
   {
      private Album _album;

      public PanelLibraryService(Album album)
      {
         _album = album;
      }

      public void SavePanelsToLibrary()
      {
         // Сохранить блоки панелей в файл библиотеки блоков панелей.
         // Куда сохранять? - [CAD_Settings\AutoCAD_server\ShareSettings\]АР\AlbumPanelColorTiles\AKR_Panels.dwg
         // Если такой блок уже есть в бибилиотеке? - старому блоку изменить имя с приставкой сегодняшней даты - [АКР_Панель_МаркаСБ]_25.10.2015-14:15
         // Если файл занят другим процессом? - подождать 3 секунды и повторить.

         // Файл библиотеки блоков панелей.
         string libPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.dwg");
         if (!File.Exists(libPanelsFilePath))
         {
            Log.Error("Нет файла библиотеки панелей {0}", libPanelsFilePath);
            return;
         }
         // Открываем и блокируем от изменений файл библиотеки блоков
         using (var libDwg = new Database(false, true))
         {            
            int waitCount=0;
            bool isBusy = false;
            do
            {               
               try
               {
                  libDwg.ReadDwgFile(libPanelsFilePath, FileShare.ReadWrite, true, "");
                  isBusy = false;
               }
               catch
               {
                  if (++waitCount > 1)
                  {
                     Log.Error("Файл библиотеки панелей занят. Панели не сохранены в библиотеку.");
                     return;
                  }                  
                  Thread.Sleep(1000);
                  isBusy = true;
               }
            } while (isBusy);

            // Переименовать блоки панелей которые уже есть в библиотеке
            renameOlderPanels(libDwg);
            // Копирование новых панелей
            copyNewPanels(libDwg);

            libDwg.SaveAs(libPanelsFilePath, DwgVersion.Current);
         }
      }

      // Копирование новых панелей
      private void copyNewPanels(Database dbLib)
      {
         var ids = new ObjectIdCollection();
         foreach (var markSb in _album.MarksSB)
         {
            ids.Add(markSb.IdBtr);
         }
         using (var t = dbLib.TransactionManager.StartTransaction())
         {            
            IdMapping iMap = new IdMapping();
            dbLib.WblockCloneObjects(ids, dbLib.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);            
            t.Commit();
         }
      }

      // Переименование старых блоков панелей
      private void renameOlderPanels(Database dbLib)
      {
         string suffix = string.Format("_{0}", DateTime.Now.ToString("dd.MM.yyyy-HH.mm"));
         using (var t = dbLib.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(dbLib.BlockTableId, OpenMode.ForRead) as BlockTable;
            foreach (var markSb in _album.MarksSB)
            {
               if (bt.Has(markSb.MarkSbBlockName))
               {
                  var btr = t.GetObject(bt[markSb.MarkSbBlockName], OpenMode.ForWrite) as BlockTableRecord;
                  btr.Name += suffix;
               }
            }
            t.Commit();
         }
      }      
   }
}

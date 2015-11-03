using System.Collections.Generic;
using System.IO;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.Sheets
{
   // Ведомость альбома панелей
   public class SheetsSet
   {
      private Album _album;

      private List<SheetMarkSB> _sheetsMarkSB;
      private string _sheetTemplateFileContent;
      private string _sheetTemplateFileMarkSB;

      public SheetsSet(Album album)
      {
         _album = album;
         _sheetsMarkSB = new List<SheetMarkSB>();
      }

      public Album Album { get { return _album; } }
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
         if (_sheetsMarkSB.Count == 0)
         {
            throw new System.Exception("Не определены панели марок АР");
         }

         // Создаение папки для альбома панелей
         CreateAlbumFolder();

         //Поиск блока рамки на текущем чертеже фасада
         BlockFrameAKR blFrameSearch = new BlockFrameAKR();
         blFrameSearch.Search();

         // Титульные листы и обложеи в одном файле "Содержание".
         // Создание титульных листов
         // Листы содержания
         SheetsContent content = new SheetsContent(this, blFrameSearch);

         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.SetLimit(_sheetsMarkSB.Count);
         progressMeter.Start("Создание файлов панелей марок СБ с листами марок АР...");
         int countMarkSB = 1;
         foreach (var sheetMarkSB in _sheetsMarkSB)
         {
            if (HostApplicationServices.Current.UserBreak())
               throw new System.Exception("Отменено пользователем.");
            progressMeter.MeterProgress();
            sheetMarkSB.CreateSheetMarkSB(this, countMarkSB++, blFrameSearch);
         }
         progressMeter.Stop();

         try
         {
            PanelLibraryLoadService libService = new PanelLibraryLoadService();
            libService.FillMarkPainting(_album);
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Ошибка заполнения марок покраски в монтажки - libService.FillMarkPainting(_album);");
         }

         // Проверка новых панелей, которых нет в библиотеке
         try
         {
            PanelLibrarySaveService.CheckNewPanels();
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Не удалось проверить есть ли новые панели в чертеже фасада, которых нет в библиотеке.");
         }

         // Еспорт списка панелей в ексель.
         try
         {
            ExportToExcel.Export(this, _album);
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Не удалось экспортировать панели в Excel.");
         }

         // вставка итоговой таблицы по плитке
         try
         {
            TotalTileTable tableTileTotal = new TotalTileTable(_album);
            tableTileTotal.InsertTableTotalTile();
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Не удалось вставить итоговую таблицу плитки на альбом.");
         }
      }

      // Создание папки Альбома панелей
      private void CreateAlbumFolder()
      {
         // Папка альбома панелей
         string albumFolderName = "АКР_" + Path.GetFileNameWithoutExtension(_album.DwgFacade);
         string curDwgFacadeFolder = Path.GetDirectoryName(_album.DwgFacade);
         _album.AlbumDir = Path.Combine(curDwgFacadeFolder, albumFolderName);
         if (Directory.Exists(_album.AlbumDir))
         {
            Directory.Delete(_album.AlbumDir, true);
         }
         Directory.CreateDirectory(_album.AlbumDir);
      }

      // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
      private List<SheetMarkSB> ProcessingSheets(List<MarkSbPanelAR> marksSB)
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
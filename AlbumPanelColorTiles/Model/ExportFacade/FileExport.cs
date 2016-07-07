using System;
using System.IO;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.ExportFacade
{
   /// <summary>
   /// Определение файла в котроый будет экспортирован фасад.
   /// </summary>
   public class FileExport
   {
      private const string _keyDict = "FileExportFacade";
      private Document _docAkr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

      public FileInfo FileAkrFacade { get; private set; }
      public FileInfo FileExportFacade { get; private set; }
      public bool IsExistsFileExport { get; private set; }

      public void Backup()
      {
         var backupFile = Path.Combine(FileExportFacade.DirectoryName,
                                       Path.GetFileNameWithoutExtension(FileExportFacade.Name) +
                                       "_Backup_" + DateTime.Now.ToString("dd.MM.yyyy-HH.mm") + ".bak");
         FileExportFacade.CopyTo(backupFile, true);
      }

      public void DefineFile()
      {
         FileAkrFacade = new FileInfo(_docAkr.Name);
         if (!FileAkrFacade.Exists)
         {
            throw new Exception("Нужно сохранить текущий чертеж.");
         }

         var fileExportFullName = DictNOD.LoadString(_keyDict, string.Empty);
         if (string.IsNullOrEmpty(fileExportFullName) || !File.Exists(fileExportFullName))
         {  
            var fileAkrName = Path.GetFileNameWithoutExtension(FileAkrFacade.Name);
            string fileExportName = fileAkrName + "_Экспорт";
            fileExportFullName = Path.Combine(FileAkrFacade.DirectoryName, fileExportName + ".dwg");
         }
         FileExportFacade = new FileInfo(fileExportFullName);

         promptUserExportFile();
         // сохранение имени экспортируемого файла фасада в словыарь
         DictNOD.SaveString(FileExportFacade.FullName, _keyDict);
      }

      private string getDwgFileName(string fileName)
      {
         // Подстановка расширения в имя файла, если его нет
         if (fileName.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase))
         {
            return fileName;
         }
         else
         {
            return fileName + ".dwg";
         }
      }

      private void promptUserExportFile()
      {
         // Запрос имени экспортированного файла у пользователя
         Editor ed = _docAkr.Editor;
         var prOpt = new PromptSaveFileOptions("Имя экспортируемого файла");
         prOpt.InitialFileName = FileExportFacade.Name;
         prOpt.InitialDirectory = FileExportFacade.DirectoryName;
         //prOpt.DialogCaption = "";
         prOpt.DialogName = "Экспорт";
         prOpt.Message = "Новый файл или существующий - в существующем файле заменятся блоки панелей.";
         prOpt.Filter = "Чертеж |*.dwg";
         var res = ed.GetFileNameForSave(prOpt);
         if (res.Status != PromptStatus.OK)
         {
            throw new Exception("Отменено пользователем.");
         }
         FileExportFacade = new FileInfo(getDwgFileName(res.StringResult));
         IsExistsFileExport = FileExportFacade.Exists;
         // Если файл существует, то спросить подтверждение экспорта фасадов в существующий файл.
         // Не нужно, т.к. в диалоге уже выдается такой вопрос.
         //if (IsExistsFileExport)
         //{
         //   var messageRes = MessageBox.Show("Такой файл уже существует. Выполнить повторный экспорт фасада в этот файл? \n" +
         //      "Старые панели в файле будут заменены на новые.", "Повторный экспорт",
         //      MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
         //   if (messageRes == MessageBoxResult.No)
         //   {
         //      // Повтор запроса имени файла
         //      promptUserExportFile();
         //   }
         //   else if (messageRes == MessageBoxResult.Cancel)
         //   {
         //      throw new Exception("Отменено пользователем.");
         //   }
         //}
      }
   }
}
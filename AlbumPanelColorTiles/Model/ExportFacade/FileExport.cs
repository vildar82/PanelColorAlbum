using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   /// <summary>
   /// Определение файла в котроый будет экспортирован фасад.
   /// </summary>
   public class FileExport
   {
      private const string _keyDict = "FileExportFacade";
      private Document _docAkr = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

      public string FileNameExport { get; private set; }
      public FileInfo FileAkrFacade { get; private set; }
      public bool IsExistsFileExport { get; private set; }


      public void DefineFile()
      {
         FileAkrFacade = new FileInfo(_docAkr.Name);
         if (!FileAkrFacade.Exists)
         {
            throw new Exception("Нужно сохранить текущий чертеж.");
         }

         FileNameExport = DictNOD.LoadString(_keyDict);
         if (string.IsNullOrEmpty(FileNameExport))
         {
            // Если в имени файла есть АКР, то убираем его и предлагаем пользователю согласиться с этим именем или изменить
            if (FileAkrFacade.Name.Contains("АКР"))
            {
               FileNameExport = FileAkrFacade.Name.Replace("АКР", "");
            }
            else
            {
               FileNameExport = FileAkrFacade.Name + "_Экспорт";
            }
         }
         
         promptUserExportFile();
         // сохранение имени экспортируемого файла фасада в словыарь
         DictNOD.SaveString(FileNameExport, _keyDict);
      }

      private void promptUserExportFile()
      {
         // Запрос имени экспортированного файла у пользователя
         Editor ed = _docAkr.Editor;
         var prOpt = new PromptSaveFileOptions("Имя экспортируемого файла");
         prOpt.InitialFileName = FileNameExport;
         prOpt.InitialDirectory = FileAkrFacade.DirectoryName;
         prOpt.DialogCaption = "DialogCaption";
         prOpt.DialogName = "DialogName";
         prOpt.Message = "Message";         
         var res = ed.GetFileNameForSave(prOpt);
         if (res.Status != PromptStatus.OK)
         {
            throw new Exception("Отменено пользователем.");
         }
         FileNameExport = res.StringResult;
         // Если файл существует, то спросить подтверждение экспорта фасадов в существующий файл.
         FileInfo fiRes = new FileInfo(FileNameExport);
         IsExistsFileExport = fiRes.Exists;
         if (IsExistsFileExport)
         {
            var messageRes = MessageBox.Show("Такой файл уже существует. Выполнить повторный экспорт фасада в этот файл? \n" +
               "Старые панели в файле будут заменены на новые.", "Повторный экспорт",
               MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (messageRes == MessageBoxResult.No)
            {
               // Повтор запроса имени файла
               promptUserExportFile();
            }
            else if (messageRes == MessageBoxResult.Cancel)
            {
               throw new Exception("Отменено пользователем.");
            }            
         }         
      }
   }
}

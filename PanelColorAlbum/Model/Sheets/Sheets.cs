using System.Collections.Generic;
using System.IO;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Ведомость альбома панелей
   public class Sheets
   {
      private Album _album;
      private string _albumDir;
      private string _fileTemplateSheet;
      private List<SheetMarkSB> _sheetsMarkSB;

      public string AlbumDir { get { return _albumDir; } }

      public Sheets(Album album)
      {
         _album = album;
         _sheetsMarkSB = new List<SheetMarkSB>();
      }

      // Создание альбома панелей
      public bool CreateAlbum()
      {
         bool res = true;
         // Проверка наличия файла шаблона листов
         _fileTemplateSheet = GetFileTemplateSheet();
         if (!File.Exists(_fileTemplateSheet))
         {
            // Не найден файл шаблона листа. Продолжение невозможно.
            _album.Doc.Editor.WriteMessage("\nНе найден файл шаблона для листов панелей - " + _fileTemplateSheet);
            return false;
         }

         // Создаение папки для альбома панелей
         CreateAlbumFolder();

         foreach (var markSB in _album.MarksSB)
         {
            // Создание листов
            SheetMarkSB sheetMarkSb = new SheetMarkSB(markSB, _albumDir, _fileTemplateSheet);
            _sheetsMarkSB.Add(sheetMarkSb);
         }
         return res;
      }

      private string GetFileTemplateSheet()
      {
         return Album.Options.SheetTemplateFile;
      }

      // Создание папки Альбома панелей
      private void CreateAlbumFolder()
      {
         // Папка альбома панеелей
         string albumFolderName = "Альбом панелей";
         string curDwgFacadeFolder = Path.GetDirectoryName(_album.Doc.Name);
         _albumDir = Path.Combine(curDwgFacadeFolder, albumFolderName);
         if (Directory.Exists(_albumDir))
         {
            // Что делать? Удалить? спросить уу пользователя?
            Directory.Delete(_albumDir, true);
         }
         Directory.CreateDirectory(_albumDir);
      }
   }
}
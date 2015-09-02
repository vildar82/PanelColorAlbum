using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Ведомость альбома панелей
   public class Sheets
   {
      private Album _album;
      DirectoryInfo _albumDir;
      FileInfo _fileTemplateSheet;
      List<SheetMarkSB> _sheetsMarkSB;

      public Sheets (Album album)
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
         if (!_fileTemplateSheet.Exists)
         {
            // Не найден файл шаблона листа. Продолжение невозможно.
            _album.Doc.Editor.WriteMessage("\nНе найден файл шаблона для листов панелей - " + _fileTemplateSheet.FullName);
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

      private FileInfo GetFileTemplateSheet()
      {
         return new FileInfo(Album.Options.SheetTemplateFile);         
      }     

      // Создание папки Альбома панелей
      private void CreateAlbumFolder()
      {
         // Папка альбома панеелей
         string albumFolderName = "Альбом панелей";
         _albumDir = new DirectoryInfo(Path.Combine (_album.Doc.Name, albumFolderName));
         if (_albumDir.Exists)
         {
            // Что делать? Удалить? спросить уу пользователя?
            _albumDir.Delete(true);
         }
         _albumDir.Create();
      }
   }
}

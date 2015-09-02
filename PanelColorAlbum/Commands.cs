using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Vil.Acad.AR.PanelColorAlbum.Model;

[assembly: CommandClass(typeof(Vil.Acad.AR.PanelColorAlbum.Commands))]

namespace Vil.Acad.AR.PanelColorAlbum
{
   // Команды автокада.
   // Для каждого документа свой объект Commands (один чертеж - один альбом).
   public class Commands : IExtensionApplication
   {
      Album _album;

      // Покраска панелей в Моделе (по блокам зон покраски)
      [CommandMethod("PIK", "PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            if (_album == null)
            {
               _album = new Album();
            }
            _album.PaintPanels();
         }
      }

      // Удалекние блоков панелей марки АР и их замена на блоки панелей марок СБ.
      [CommandMethod("PIK", "ResetPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void ResetPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            Album album = new Album();
            album.Resetblocks();
         }
      }

      // Создание альбома колористических решений панелей (Альбома панелей).
      [CommandMethod("AlbumPanels")]
      public void AlbumPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            if (_album == null)
            {
               doc.Editor.WriteMessage("\nСначала нужно выполнить команду PaintPanels для покраски плитки.");               
            }
            else
            {
               _album.CreateAlbum(); 
            }
         }
      }

      void IExtensionApplication.Initialize()
      {
         // Загрузка сборки в автокад.
         //TODO Написать описание команд в ком строке.     
         Document doc = Application.DocumentManager.MdiActiveDocument;
         if (doc == null)
         {
            return;
         }
         Editor ed = doc.Editor;
         string msg = "\nЗагружена программа для покраски плитки и создания альбома панелей." +                      
                      "\nКоманды: PaintPanels - покраска блоков панелей." +
                      "\nResetPanels - удаление блоков панелей Марки АР и замена их на блоки панелей Марки СБ." +
                      "\nAlbumPanels - создание альбома панелей.";
         ed.WriteMessage(msg);
      }

      void IExtensionApplication.Terminate()
      {         
      }
   }
}

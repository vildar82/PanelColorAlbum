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
         string msg = "\n Команда PaintPanels - покраска блоков панелей.";
         ed.WriteMessage(msg);
      }

      void IExtensionApplication.Terminate()
      {         
      }
   }
}

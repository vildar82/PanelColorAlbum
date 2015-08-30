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
      Album album;

      // Покраска панелей в Моделе (по блокам зон покраски)
      [CommandMethod("PIK", "PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            if (album == null)
            {
               album = new Album();
            }
            album.PaintPanels();
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

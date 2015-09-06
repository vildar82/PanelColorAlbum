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
      private Album _album;

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
            try
            {
               _album.PaintPanels();
               doc.Editor.Regen();
               doc.Editor.WriteMessage("\nПокраска панелей выполнена успешно.");
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить покраску панелей. " + ex.Message);
            }
         }
      }

      // Удалекние блоков панелей марки АР и их замена на блоки панелей марок СБ.
      [CommandMethod("PIK", "ResetPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void ResetPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {            
            try
            {
               Album.Resetblocks();                              
               doc.Editor.WriteMessage("\nСброс блоков выполнен успешно.");
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить сброс панелей. " + ex.Message);
            }
         }
      }

      // Создание альбома колористических решений панелей (Альбома панелей).
      [CommandMethod("PIK", "AlbumPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
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
               try
               {
                  _album.CreateAlbum();                                                         
                  doc.Editor.WriteMessage("\nАльбом панелей выполнен успешно:" + _album.SheetsSet.AlbumDir);
               }
               catch (System.Exception ex)
               {
                  doc.Editor.WriteMessage("\nНе удалось создать альбом панелей. " + ex.Message);
               }
            }
         }
      }

      void IExtensionApplication.Initialize()
      {
         // Загрузка сборки в автокад.
         Document doc = Application.DocumentManager.MdiActiveDocument;
         if (doc == null)
         {
            return;
         }
         Editor ed = doc.Editor;
         //Album albumForOptions = new Album();
         string msg = "\nЗагружена программа для покраски плитки и создания альбома панелей." +
                      "\nКоманды: PaintPanels - покраска блоков панелей." +
                      "\nResetPanels - удаление блоков панелей Марки АР и замена их на блоки панелей Марки СБ." +
                      "\nAlbumPanels - создание альбома панелей." +
                      "\nСправка: имена блоков:" +
                      "\nБлоки панелей с префиксом - " + Album.Options.BlockPanelPrefixName + ", дальше марка СБ, без скобок вконце." +
                      "\nБлок зоны покраски (на слое марки цвета для плитки) - " + Album.Options.BlockColorAreaName + 
                      "\nБлок плитки (разложенная в блоке панели) - " + Album.Options.BlockTileName + 
                      "\nПанели чердака на слое - " + Album.Options.LayerUpperStoreyPanels + 
                      "\nПанели торцевые на слое - " + Album.Options.LayerPanelEndLeft + " или " + Album.Options.LayerPanelEndRight + 
                      "\n";            
         ed.WriteMessage(msg);
      }

      void IExtensionApplication.Terminate()
      {
      }
   }
}
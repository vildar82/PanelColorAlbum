using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Vil.Acad.AR.AlbumPanelColorTiles.Model;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Sheets;

[assembly: CommandClass(typeof(Vil.Acad.AR.AlbumPanelColorTiles.Commands))]

namespace Vil.Acad.AR.AlbumPanelColorTiles
{
   // Команды автокада.
   // Для каждого документа свой объект Commands (один чертеж - один альбом).
   public class Commands : IExtensionApplication
   {
      private Album _album;

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
                  _album.ChecksBeforeCreateAlbum();
                  // После покраски панелей, пользователь мог изменить панели на чертеже, а в альбом это не попадет.
                  // Нужно или выполнить перекраску панелей перед созданием альбома
                  // Или проверить список панелей в _albom и список панелей на чертеже, и выдать сообщение если есть изменения.                  
                  _album.CheckPanelsInDrawingAndMemory();
                  // Покраска панелей
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
                      "\nPlotPdf - печать листов текущего чертежа в PDF. Файл создается в корне текущего чертежа с таким же именем." +
                      "\nСправка: имена блоков:" +
                      "\nБлоки панелей с префиксом - " + Album.Options.BlockPanelPrefixName + ", дальше марка СБ, без скобок вконце." +
                      "\nБлок зоны покраски (на слое марки цвета для плитки) - " + Album.Options.BlockColorAreaName +
                      "\nБлок плитки (разложенная в блоке панели) - " + Album.Options.BlockTileName +
                      "\nПанели чердака на слое - " + Album.Options.LayerUpperStoreyPanels +
                      "\nПанели торцевые с суффиксом _тп или _тл после марки СБ в имени блока панели." +
                      "\nСлой для окон в панелях - " + Album.Options.LayerWindows +
                      "\nСлой для размеров на фасаде в панели - " + Album.Options.LayerDimensionFacade +
                      "\nСлой для размеров в форме в панели - " + Album.Options.LayerDimensionFacade +
                      "\nОбрабатываются только блоки в текущем чертеже. Внешние ссылки не учитываются.";
         ed.WriteMessage(msg);
      }

      void IExtensionApplication.Terminate()
      {
      }

      // Покраска панелей в Моделе (по блокам зон покраски)
      [CommandMethod("PIK", "PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               if (_album == null)
               {
                  _album = new Album();
               }
               else
               {
                  // Повторный запуск программы покраски панелей.
                  // Сброс данных
                  _album.ResetData(); 
               }
               _album.PaintPanels();
               doc.Editor.Regen();
               doc.Editor.WriteMessage("\nПокраска панелей выполнена успешно.");
               doc.Editor.WriteMessage("\nВыполните команду AlbumPanels для создания альбома покраски панелей с плиткой.");
               doc.Editor.WriteMessage("\nИли ResetPanels для сброса блоков панелей до марок СБ.");
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить покраску панелей. " + ex.Message);
            }
         }
      }

      // Удаление блоков панелей марки АР и их замена на блоки панелей марок СБ.
      [CommandMethod("PIK", "ResetPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void ResetPanelsCommand()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               if (_album != null)
               {
                  _album.ResetData(); 
               }
               Album.Resetblocks();
               doc.Editor.WriteMessage("\nСброс блоков выполнен успешно.");
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить сброс панелей. " + ex.Message);
            }
         }
      }

      [CommandMethod("AKR", "PlotPdf", CommandFlags.Modal)]
      public static void PlotPdf()
      {
         Database db = HostApplicationServices.WorkingDatabase;
         short bgp = (short)Application.GetSystemVariable("BACKGROUNDPLOT");
         try
         {
            Application.SetSystemVariable("BACKGROUNDPLOT", 0);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
               List<Layout> layouts = new List<Layout>();
               DBDictionary layoutDict =
                   (DBDictionary)db.LayoutDictionaryId.GetObject(OpenMode.ForRead);
               foreach (DBDictionaryEntry entry in layoutDict)
               {
                  if (entry.Key != "Model")
                  {
                     layouts.Add((Layout)tr.GetObject(entry.Value, OpenMode.ForRead));
                  }
               }
               layouts.Sort((l1, l2) => l1.TabOrder.CompareTo(l2.TabOrder));

               string filename = Path.ChangeExtension(db.Filename, "pdf");

               MultiSheetsPdf plotter = new MultiSheetsPdf(filename, layouts);
               plotter.Publish();

               tr.Commit();
            }
         }
         catch (System.Exception e)
         {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\nError: {0}\n{1}", e.Message, e.StackTrace);
         }
         finally
         {
            Application.SetSystemVariable("BACKGROUNDPLOT", bgp);
         }
      }
   }
}
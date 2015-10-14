﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlbumPanelColorTiles.Checks;
using AlbumPanelColorTiles.ImagePainting;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.Plot;
using AlbumPanelColorTiles.RandomPainting;
using AlbumPanelColorTiles.RenamePanels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(AlbumPanelColorTiles.Commands))]

namespace AlbumPanelColorTiles
{
   // Команды автокада.
   // Для каждого документа свой объект Commands (один чертеж - один альбом).
   public class Commands
   {
      private static string _curDllDir;
      private Album _album;
      private string _msgHelp;
      private RandomPaintService _randomPainting;
      private ImagePaintingService _imagePainting;

      public static string CurDllDir
      {
         get
         {
            if (_curDllDir == null)
            {
               _curDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            return _curDllDir;
         }
      }

      private string MsgHelp
      {
         get
         {
            if (_msgHelp == null)
            {
               _msgHelp = "\nПрограмма для покраски плитки и создания альбома панелей." +
                      "\nВерсия программы " + Assembly.GetExecutingAssembly().GetName().Version +
                      "\nКоманды:" +
                      "\nAKR-PaintPanels - покраска блоков панелей." +
                      "\nAKR-ResetPanels - удаление блоков панелей Марки АР и замена их на блоки панелей Марки СБ." +
                      "\nAKR-AlbumPanels - создание альбома панелей." +
                      "\nAKR-PlotPdf - печать в PDF текущего чертежа или выбранной папки с чертежами. Файлы создается в корне чертежа с тем же именем. Печать выполняется по настройкам на листах." +
                      "\nAKR-SelectPanels - выбор блоков панелей в Модели." +
                      "\nAKR-RandomPainting - случайное распределение зон покраски в указанной области." +
                      "\nAKR-ImagePainting - покраска области по выбранной картинке." +
                      "\nAKR-InsertBlockColorArea - вставка блока зоны покраски." +
                      "\nAKR-InsertBlockPanel - вставка блока зоны покраски." +
                      "\nAKR-InsertBlockFrame - вставка блока зоны покраски." +
                      "\nAKR-InsertBlockTile - вставка блока зоны покраски." +
                      "\nСправка: имена блоков:" +
                      "\nБлоки панелей с префиксом - " + Album.Options.BlockPanelPrefixName + ", дальше марка СБ, без скобок в конце." +
                      "\nБлок зоны покраски (на слое марки цвета для плитки) - " + Album.Options.BlockColorAreaName +
                      "\nБлок плитки (разложенная в блоке панели) - " + Album.Options.BlockTileName +
                      "\nБлок рамки со штампом - " + Album.Options.BlockFrameName +
                      "\nПанели чердака на слое - " + Album.Options.LayerUpperStoreyPanels +
                      "\nПанели торцевые с суффиксом _тп или _тл после марки СБ в имени блока панели." +
                      "\nСлой для окон в панелях (замораживается на листе формы панели марки АР) - " + Album.Options.LayerWindows +
                      "\nСлой для размеров на фасаде в панели (замораживается на листе формы) - " + Album.Options.LayerDimensionFacade +
                      "\nСлой для размеров в форме в панели (замораживается на листе фасада) - " + Album.Options.LayerDimensionFacade +
                      "\nОбрабатываются только блоки в текущем чертеже в Модели. Внешние ссылки не учитываются.\n";
            }
            return _msgHelp;
         }
      }

      // Создание альбома колористических решений панелей (Альбома панелей).
      [CommandMethod("PIK", "AKR-AlbumPanels", CommandFlags.Modal | CommandFlags.Session | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void AlbumPanelsCommand()
      {
         Log.Info("Start Command: AKR-AlbumPanels");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         if (!File.Exists(doc.Name))
            throw new System.Exception("Нужно сохранить файл.");
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
                  Inspector.Reset(); 
                  _album.ChecksBeforeCreateAlbum();
                  // После покраски панелей, пользователь мог изменить панели на чертеже, а в альбом это не попадет.
                  // Нужно или выполнить перекраску панелей перед созданием альбома
                  // Или проверить список панелей в _albom и список панелей на чертеже, и выдать сообщение если есть изменения.
                  _album.CheckPanelsInDrawingAndMemory();

                  // Переименование марок пользователем.
                  // Вывод списка панелей для возможности переименования марок АР пользователем
                  FormRenameMarkAR formRenameMarkAR = new FormRenameMarkAR(_album);
                  if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(formRenameMarkAR) == DialogResult.OK)
                  {
                     var renamedMarksAR = formRenameMarkAR.RenamedMarksAr();
                     // сохранить в словарь
                     DictNOD.SaveToDict(renamedMarksAR);

                     // Переименовать марки АР
                     renamedMarksAR.ForEach(r => r.MarkAR.MarkPainting = r.MarkPainting);                     

                     // Покраска панелей
                     _album.CreateAlbum();
                     doc.Editor.WriteMessage("\nАльбом панелей выполнен успешно:" + _album.AlbumDir);
                     doc.Editor.Regen();
                     Log.Info("Альбом панелей выполнен успешно: {0}", _album.AlbumDir);
                  }
                  else
                  {
                     Log.Info("Отменено пользователем.");
                     throw new System.Exception("Отменено пользователем.");
                  }
               }
               catch (System.Exception ex)
               {
                  doc.Editor.WriteMessage("\nНе удалось создать альбом панелей. " + ex.Message);
                  Log.Error(ex, "Не удалось создать альбом панелей. {0}", doc.Name);
               }
            }
         }
      }

      [CommandMethod("PIK", "AKR-Help", CommandFlags.Modal)]
      public void HelpCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;
         ed.WriteMessage("\n{0}", MsgHelp);
      }

      [CommandMethod("AKR", "AKR-InsertBlockColorArea", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void InsertBlockColorAreaCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         try
         {
            BlockInsert.Insert(Album.Options.BlockColorAreaName);
         }
         catch (System.Exception ex)
         {
            ed.WriteMessage("\n" + ex.Message);
         }
      }

      [CommandMethod("AKR", "AKR-InsertBlockFrame", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void InsertBlockFrameCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;
         try
         {
            BlockInsert.Insert("АКР_Рамка");
         }
         catch (System.Exception ex)
         {
            ed.WriteMessage("\n" + ex.Message);
         }
      }

      [CommandMethod("AKR", "AKR-InsertBlockPanel", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void InsertBlockPanelCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;
         try
         {
            BlockInsert.Insert("АКР_Панель_[Марка СБ]");
         }
         catch (System.Exception ex)
         {
            ed.WriteMessage("\n" + ex.Message);
         }
      }

      [CommandMethod("AKR", "AKR-InsertBlockTile", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void InsertBlockTileCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;
         try
         {
            BlockInsert.Insert("АКР_Плитка");
         }
         catch (System.Exception ex)
         {
            ed.WriteMessage("\n" + ex.Message);
         }
      }

      [CommandMethod("PIK", "AKR-PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Log.Info("Start Command: AKR-PaintPanels");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
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
               Log.Info("Покраска панелей выполнена успешно. {0}", doc.Name);
            }
            catch (System.Exception ex)
            {
               if (Inspector.Errors.Count > 0)
               {
                  Inspector.Show();
               }
               doc.Editor.WriteMessage("\nНе выполнена покраска панелей. " + ex.Message);
               Log.Error(ex, "Не выполнена покраска панелей. {0}", doc.Name);
            }
         }
      }

      [CommandMethod("AKR", "AKR-PlotPdf", CommandFlags.Modal | CommandFlags.Session)]
      public void PlotPdf()
      {
         Log.Info("Start Command AKR-PlotPdf");
         // Печать в PDF. Текущий чертеж или чертежи в указанной папке
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;

         using (var lockDoc = doc.LockDocument())
         {
            var optPrompt = new PromptKeywordOptions("\nПечатать листы текущего чертежа или из папки?");
            optPrompt.Keywords.Add("Текущего");
            optPrompt.Keywords.Add("Папки");

            var resPrompt = ed.GetKeywords(optPrompt);
            PlotMultiPDF plotter = new PlotMultiPDF();
            if (resPrompt.Status == PromptStatus.OK)
            {
               if (resPrompt.StringResult == "Текущего")
               {
                  Log.Info("Текущего");
                  try
                  {
                     plotter.PlotCur();
                  }
                  catch (System.Exception ex)
                  {
                     ed.WriteMessage("\n" + ex.Message);
                     Log.Error(ex, "plotter.PlotCur();");
                  }
               }
               else if (resPrompt.StringResult == "Папки")
               {
                  Log.Info("Папки");
                  var dialog = new FolderBrowserDialog();
                  dialog.Description = "Выбор папки для печати чертежов из нее в PDF";
                  dialog.ShowNewFolderButton = false;
                  if (_album == null)
                  {
                     dialog.SelectedPath = Path.GetDirectoryName(doc.Name);
                  }
                  else
                  {
                     dialog.SelectedPath = _album.AlbumDir == null ? Path.GetDirectoryName(doc.Name) : _album.AlbumDir;
                  }
                  if (dialog.ShowDialog() == DialogResult.OK)
                  {
                     try
                     {
                        plotter.PlotDir(dialog.SelectedPath);
                     }
                     catch (System.Exception ex)
                     {
                        ed.WriteMessage("\n" + ex.Message);
                        Log.Error(ex, "plotter.PlotDir({0});", dialog.SelectedPath);
                     }
                  }
               }
            }
         }
      }

      [CommandMethod("AKR", "AKR-RandomPainting", CommandFlags.Modal)]
      public void RandomPaintingCommand()
      {
         Log.Info("Start Command: AKR-RandomPainting");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         try
         {            
            // Произвольная покраска участка, с % распределением цветов зон покраски.
            if (_randomPainting == null)
            {
               _randomPainting = new RandomPaintService();
            }
            _randomPainting.Start();            
         }
         catch (System.Exception ex)
         {            
            doc.Editor.WriteMessage("\n{0}", ex.ToString());
            Log.Error(ex, "Command: AKR-RandomPainting");
         }
      }

      // Удаление блоков панелей марки АР и их замена на блоки панелей марок СБ.
      [CommandMethod("PIK", "AKR-ResetPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void ResetPanelsCommand()
      {
         Log.Info("Start Command: AKR-ResetPanels");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               if (_album != null)
               {
                  _album.ResetData();
               }
               Album.ResetBlocks();
               doc.Editor.Regen();
               doc.Editor.WriteMessage("\nСброс блоков выполнен успешно.");
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить сброс панелей. " + ex.Message);
               Log.Error(ex, "Не удалось выполнить сброс панелей. ");
            }
         }
      }

      [CommandMethod("AKR", "AKR-SelectPanels", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void SelectPanelsCommand()
      {
         Log.Info("Start Command: AKR-SelectPanels");
         // Выбор блоков панелей на чертеже в Модели
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         using (var DocLock = doc.LockDocument())
         {
            Dictionary<string, List<ObjectId>> panels = new Dictionary<string, List<ObjectId>>();
            int countMarkSbPanels = 0;
            int countMarkArPanels = 0;

            using (var t = db.TransactionManager.StartTransaction())
            {
               var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead) as BlockTableRecord;
               foreach (ObjectId idEnt in ms)
               {
                  if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                  {
                     var blRef = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                     if (MarkSbPanel.IsBlockNamePanel(blRef.Name))
                     {
                        if (MarkSbPanel.IsBlockNamePanelMarkAr(blRef.Name))
                           countMarkArPanels++;
                        else
                           countMarkSbPanels++;

                        if (panels.ContainsKey(blRef.Name))
                        {
                           panels[blRef.Name].Add(blRef.ObjectId);
                        }
                        else
                        {
                           List<ObjectId> idBlRefs = new List<ObjectId>();
                           idBlRefs.Add(blRef.ObjectId);
                           panels.Add(blRef.Name, idBlRefs);
                        }
                     }
                  }
               }
               t.Commit();
            }
            foreach (var panel in panels)
            {
               ed.WriteMessage("\n" + panel.Key + " - " + panel.Value.Count);
            }
            ed.SetImpliedSelection(panels.Values.SelectMany(p => p).ToArray());
            ed.WriteMessage("\nВыбрано блоков панелей в Модели: Марки СБ - {0}, Марки АР - {1}", countMarkSbPanels, countMarkArPanels);
            Log.Info("Выбрано блоков панелей в Модели: Марки СБ - {0}, Марки АР - {1}", countMarkSbPanels, countMarkArPanels);
         }
      }

      [CommandMethod("AKR", "AKR-ImagePainting", CommandFlags.Modal)]
      public void ImagePaintingCommand()
      {
         Log.Info("Start Command: AKR-ImagePainting");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         try
         {
            if (_imagePainting == null)
            {
               _imagePainting = new ImagePaintingService(doc);
            }            
            _imagePainting.Go(); 
         }
         catch (System.Exception ex)
         {
            doc.Editor.WriteMessage("\n{0}", ex.ToString());
            Log.Error(ex, "Command: AKR-ImagePainting");
         }
      }
   }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AcadLib.Errors;
using AlbumPanelColorTiles.ImagePainting;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Model.ExportFacade;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.PanelLibrary.LibEditor;
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
      private static DateTime _lastStartCommandDateTime;
      private static string _lastStartCommandName = string.Empty;
      private static string _curDllDir;
      private Album _album;
      private ImagePaintingService _imagePainting;
      private string _msgHelp;
      private RandomPaintService _randomPainting;

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
                      "\nAKR-SavePanelsToLibrary - сохранение АКР-Панелей из текущего чертежа в библиотеку." +
                      "\nAKR-LoadPanelsFromLibrary - загрузка АКР-Панелей из библиотеки в текущий чертеж в соответствии с монтажными планами конструкторов." +
                      "\nAKR-CreateMountingPlanBlocks - создание блоков монтажек из монтажных планов конструкторов." +
                      "\nAKR-CopyDictionary - копирование словаря текущего чертежа в другой чертеж. В словаре хранится список переименований панелей, индекс проекта, номер первого этажа." +
                      "\nИмена блоков и слоев:" +
                      "\nБлоки панелей с префиксом - " + Settings.Default.BlockPanelPrefixName + ", дальше марка СБ, без скобок в конце." +
                      "\nБлок зоны покраски (на слое марки цвета для плитки) - " + Settings.Default.BlockColorAreaName +
                      "\nБлок плитки (разложенная в блоке панели) - " + Settings.Default.BlockTileName +
                      "\nБлок обозначения стороны фасада на монтажном плане - " + Settings.Default.BlockFacadeName +
                      "\nБлок рамки со штампом - " + Settings.Default.BlockFrameName +
                      "\nПанели Чердака на слое - " + Settings.Default.LayerUpperStoreyPanels +
                      "\nПанели Парапета на слое - " + Settings.Default.LayerParapetPanels +
                      "\nПанели торцевые с суффиксом _тп или _тл после марки СБ в конце имени блока панели." +
                      "\nПанели с разными окнами - с суффиксом _ок (русскими буквами) и номером в конце имени блока панели. Например _ОК1" +
                      "\nСлой для окон в панелях (замораживается на листе формы панели марки АР) - " + Settings.Default.LayerWindows +
                      "\nСлой для размеров на фасаде в панели (замораживается на листе формы) - " + Settings.Default.LayerDimensionFacade +
                      "\nСлой для размеров в форме в панели (замораживается на листе фасада) - " + Settings.Default.LayerDimensionFacade +
                      "\nОбрабатываются только блоки в текущем чертеже в Модели. Внешние ссылки не учитываются.\n";
            }
            return _msgHelp;
         }
      }

      // Создание альбома колористических решений панелей (Альбома панелей).
      [CommandMethod("PIK", "AKR-AlbumPanels", CommandFlags.Modal | CommandFlags.Session | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void AlbumPanelsCommand()
      {         
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;

         string commandName = "AlbumPanels";         
         if (string.Equals(_lastStartCommandName, commandName))
         {
            if ((DateTime.Now - _lastStartCommandDateTime).Seconds < 5)
            {
               doc.Editor.WriteMessage("Между запусками команды прошло меньше 5 секунд. Отмена.");
               return;
            }
         }

         if (!File.Exists(doc.Name))
         {
            doc.Editor.WriteMessage("\nНужно сохранить текущий чертеж.");
            return;
         }

         Log.Info("Start Command: AKR-AlbumPanels");

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
                  Inspector.Clear();
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
                     DictNOD.SaveRenamedMarkArToDict(renamedMarksAR);

                     // Переименовать марки АР
                     renamedMarksAR.ForEach(r => r.MarkAR.MarkPainting = r.MarkPainting);

                     // Создание альбома
                     _album.CreateAlbum();

                     if (Inspector.HasErrors)
                     {
                        Inspector.Show();
                     }
                     doc.Editor.Regen();
                     doc.Editor.WriteMessage("\nАльбом панелей выполнен успешно:" + _album.AlbumDir);
                     Log.Info("Альбом панелей выполнен успешно: {0}", _album.AlbumDir);
                  }
                  else
                  {
                     doc.Editor.WriteMessage("\nОтменено пользователем.");
                  }
               }
               catch (System.Exception ex)
               {
                  doc.Editor.WriteMessage("\nНе удалось создать альбом панелей. {0}", ex.Message);
                  if (!ex.Message.Contains("Отменено пользователем"))
                  {
                     Log.Error(ex, "Не удалось создать альбом панелей. {0}", doc.Name);
                  }                  
               }
            }
         }
         _lastStartCommandName = commandName;
         _lastStartCommandDateTime = DateTime.Now;
      }

      /// <summary>
      /// Создание блоков монтажных планов (создаются блоки с именем вида АКР_Монтажка_2).
      /// </summary>
      [CommandMethod("PIK", "AKR-CreateMountingPlanBlocks", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void CreateMountingPlanBlocksCommand()
      {
         Log.Info("Start Command: AKR-CreateMountingPlanBlocks");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               MountingPlans mountingPlans = new MountingPlans();
               mountingPlans.CreateMountingPlans();
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\n{0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-CreateMountingPlanBlocks. {0}", doc.Name);
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

      [CommandMethod("PIK", "AKR-ImagePainting", CommandFlags.Modal)]
      public void ImagePaintingCommand()
      {
         Log.Info("Start Command: AKR-ImagePainting");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
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
               doc.Editor.WriteMessage("\n{0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-ImagePainting. {0}", doc.Name);
               }               
            }
         }
      }

      /// <summary>
      /// Создание фасадов из правильно расставленных блоков монтажных планов с блоками обозначения сторон фасада
      /// Загрузка панелей-АКР из библиотеки
      /// </summary>
      [CommandMethod("PIK", "AKR-LoadPanelsFromLibrary", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void LoadPanelsFromLibraryCommand()
      {
         Log.Info("Start Command: AKR-LoadPanelsFromLibrary");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               PanelLibraryLoadService loadPanelsService = new PanelLibraryLoadService();
               loadPanelsService.LoadPanels();
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\n{0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-LoadPanelsFromLibrary. {0}", doc.Name);
               }               
            }
         }
      }

      [CommandMethod("PIK", "AKR-PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {         
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;

         // Принудительное сохранение файла 
         if (File.Exists(doc.Name))
         {
            object obj = Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("DBMOD");
            // Проверка значения системной переменной DBMOD. Если 0 - значит чертёж не был изменён
            if (Convert.ToInt16(obj) != 0)
            {
               var db = doc.Database;
               db.SaveAs(db.Filename, true, DwgVersion.Current, db.SecurityParameters);
            }            
         }

         string commandName = "PaintPanels";
         if (string.Equals(_lastStartCommandName, commandName))
         {            
            if ((DateTime.Now - _lastStartCommandDateTime).Seconds < 5)
            {
               doc.Editor.WriteMessage("Между запусками команды прошло меньше 5 секунд. Отмена.");
               return;
            }            
         }

         Log.Info("Start Command: AKR-PaintPanels");

         using (var DocLock = doc.LockDocument())
         {
            try
            {
               Inspector.Clear();
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
               Log.Info("Покраска панелей выполнена успешно. {0}", doc.Name);
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе выполнена покраска панелей. {0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Не выполнена покраска панелей. {0}", doc.Name);
               }               
            }
            if (Inspector.HasErrors)
            {
               Inspector.Show();
            }
         }
         _lastStartCommandName = commandName;
         _lastStartCommandDateTime = DateTime.Now;
      }

      [CommandMethod("PIK", "AKR-PlotPdf", CommandFlags.Modal | CommandFlags.Session)]
      public void PlotPdfCommand()
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
                     if (!string.Equals(ex.Message, "Отменено пользователем.", System.StringComparison.CurrentCultureIgnoreCase))
                     {
                        Log.Error(ex, "plotter.PlotCur(); {0}", doc.Name);
                     }
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
                        ed.WriteMessage("\n{0}", ex.Message);
                        if (!ex.Message.Contains("Отменено пользователем"))
                        {
                           Log.Error(ex, "plotter.PlotDir({0}); {1}", dialog.SelectedPath, doc.Name);
                        }                        
                     }
                  }
               }
            }
         }
      }

      [CommandMethod("PIK", "AKR-RandomPainting", CommandFlags.Modal)]
      public void RandomPaintingCommand()
      {
         Log.Info("Start Command: AKR-RandomPainting");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
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
               doc.Editor.WriteMessage("\n{0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-RandomPainting. {0}", doc.Name);
               }               
            }
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
               doc.Editor.WriteMessage("\nНе удалось выполнить сброс панелей. {0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Не удалось выполнить сброс панелей. {0}", doc.Name);
               }               
            }
         }
      }

      [CommandMethod("PIK", "AKR-SavePanelsToLibrary", CommandFlags.Session | CommandFlags.Modal | CommandFlags.NoBlockEditor)]
      public void SavePanelsToLibraryCommand()
      {
         Log.Info("Start Command: AKR-SavePanelsToLibrary");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var lockDoc = doc.LockDocument())
         {
            try
            {
               PanelLibrarySaveService panelLib = new PanelLibrarySaveService();
               panelLib.SavePanelsToLibrary();
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\nНе удалось выполнить сохранение панелей. {0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-SavePanelsToLibrary. {0}", doc.Name);
               }               
            }
         }
      }

      [CommandMethod("PIK", "AKR-SelectPanels", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
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
                     var blRef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                     if (MarkSb.IsBlockNamePanel(blRef.Name))
                     {
                        if (MarkSb.IsBlockNamePanelMarkAr(blRef.Name))
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

      [CommandMethod("PIK", "AKR-EditPanelLibrary", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void EditPanelLibraryCommand()
      {
         Log.Info("Start Command: AKR-EditPanelLibrary");         
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         using (var DocLock = doc.LockDocument())
         {
            LibraryEditor libEditor = new LibraryEditor();
            try
            {
               libEditor.Edit();
            }
            catch (System.Exception ex)
            {
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-EditPanelLibrary. {0}", doc.Name);
               }
               ed.WriteMessage(ex.Message);
            }            
         }
      }

      /// <summary>
      /// Копирование словаря АКР из этого чертежа в другой
      /// </summary>
      [CommandMethod("PIK", "AKR-CopyDictionary", CommandFlags.Modal | CommandFlags.Session)]
      public void CopyDictionaryCommand()
      {         
         Log.Info("Start Command: AKR-CopyDictionary");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         using (var DocLock = doc.LockDocument())
         {            
            try
            {
               Inspector.Clear();
               // Запрос имени открытого чертежа в который нужно скопировать словарь
               var res = doc.Editor.GetString("Имя чертежа в который копировать словарь АКР");
               if (res.Status == PromptStatus.OK)
               {
                  // Поиск чертежа среди открытых документов
                  foreach (Document itemDoc in Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager)
                  {
                     if (string.Equals(Path.GetFileName(itemDoc.Name), res.StringResult, System.StringComparison.OrdinalIgnoreCase))
                     {
                        using (var lockItemDoc = itemDoc.LockDocument())
                        {
                           DictNOD.CopyDict(itemDoc.Database);
                        }
                     }
                  }
               }
               else
               {
                  return;
               }               
               if (Inspector.HasErrors)
               {
                  Inspector.Show();
               }
            }
            catch (System.Exception ex)
            {
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-CopyDictionary. {0}", doc.Name);
               }               
               doc.Editor.WriteMessage("Ошибка копирования словаря - {0}", ex.Message);
            }
         }
      }

      /// <summary>
      /// Экспорт фасадов (для Архитекторов - создающих листы фасадов)
      /// </summary>
      [CommandMethod("PIK", "AKR-ExportFacade", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void ExportFacadeCommand()
      {
         Log.Info("Start Command: AKR-ExportFacade");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               Inspector.Clear();

               ExportFacadeService export = new ExportFacadeService();
               export.Export();
               
               if (Inspector.HasErrors)
               {
                  Inspector.Show();
               }
            }
            catch (System.Exception ex)
            {
               if (!ex.Message.Contains("Отменено пользователем"))
               {
                  Log.Error(ex, "Command: AKR-ExportFacade. {0}", doc.Name);
               }               
               doc.Editor.WriteMessage("Ошибка при экспорте фасада - {0}", ex.Message);
            }
         }
      }
   }
}
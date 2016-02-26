using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AcadLib;
using AcadLib.Blocks;
using AcadLib.Blocks.Dublicate;
using AcadLib.Errors;
using AcadLib.Plot;
using AlbumPanelColorTiles.ImagePainting;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Model.Base;
using AlbumPanelColorTiles.Model.ExportFacade;
using AlbumPanelColorTiles.Model.Utils;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.PanelLibrary.LibEditor;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.RandomPainting;
using AlbumPanelColorTiles.RenamePanels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
      private static DateTime _lastStartCommandDateTime;
      private static string _lastStartCommandName = string.Empty;
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
                      "\nБлоки панелей с префиксом - " + Settings.Default.BlockPanelAkrPrefixName + ", дальше марка СБ, без скобок в конце." +
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
      [CommandMethod("PIK", "AKR-AlbumPanels", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void AlbumPanels()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;

         var minVer = new Version(20, 1);
         if (Autodesk.AutoCAD.ApplicationServices.Application.Version < minVer)
         {
            doc.Editor.WriteMessage("\nКоманда создания альбома временно не работает на версиях ниже 2016 sp1.");
            return;
         }

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
               if (AcAp.ShowModalDialog(formRenameMarkAR) == DialogResult.OK)
               {
                  var renamedMarksAR = formRenameMarkAR.RenamedMarksAr();
                  // сохранить в словарь
                  Lib.DictNOD.SaveRenamedMarkArToDict(renamedMarksAR);

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
         _lastStartCommandName = commandName;
         _lastStartCommandDateTime = DateTime.Now;
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
                           Lib.DictNOD.CopyDict(itemDoc.Database);
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
      /// Создание блоков монтажных планов (создаются блоки с именем вида АКР_Монтажка_2).
      /// </summary>
      [CommandMethod("PIK", "AKR-CreatePlanBlocks", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void CreatePlanBlocksCommand()
      {
         Log.Info("Start Command: AKR-CreatePlanBlocks");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               BlockPlans mountingPlans = new BlockPlans();
               mountingPlans.CreateBlockPlans();
            }
            catch (System.Exception ex)
            {
               doc.Editor.WriteMessage("\n{0}", ex.Message);
               if (!ex.Message.Contains("Отменено пользователем"))
               {                  
                  Log.Error(ex, "Command: AKR-CreatePlanBlocks. {0}", doc.Name);
               }
            }
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
               else
               {
                  doc.Editor.WriteMessage("\nГотово.");
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

      [CommandMethod("PIK", "AKR-Help", CommandFlags.Modal)]
      public void HelpCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;
         Editor ed = doc.Editor;
         ed.WriteMessage("\n{0}", MsgHelp);
         // Открытие папки с инструкциями в проводнике
         try
         {
            System.Diagnostics.Process.Start("explorer", @"\\dsk2.picompany.ru\project\CAD_Settings\_Шаблоны & типовые решения\30_АР\3.01_Обучение_АКР");
         }
         catch (System.Exception ex)
         {
            Log.Error(ex, "HelpCommand");
         }         
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

      /// <summary>
      /// Создание фасадов из правильно расставленных блоков монтажных планов с блоками обозначения сторон фасада
      /// Панели АКР создаются по описанию из базы Конструкторов
      /// </summary>
      [CommandMethod("PIK", "AKR-CreateFacadeCommand", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void CreateFacadeCommand()
      {
         Log.Info("Start Command: AKR-CreateFacadeCommand");
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
                  Log.Error(ex, "Command: AKR-CreateFacadeCommand. {0}", doc.Name);
               }
            }
         }
      }

      [CommandMethod("PIK", "AKR-PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;

         //// Принудительное сохранение файла
         //if (File.Exists(doc.Name))
         //{
         //   object obj = AcAp.GetSystemVariable("DBMOD");
         //   // Проверка значения системной переменной DBMOD. Если 0 - значит чертёж не был изменён
         //   if (Convert.ToInt16(obj) != 0)
         //   {
         //      var db = doc.Database;
         //      try
         //      {
         //         db.SaveAs(db.Filename, true, DwgVersion.Current, db.SecurityParameters);
         //      }
         //      catch (System.Exception ex)
         //      {
         //         doc.Editor.WriteMessage($"Ошибка сохранения файла. {ex.Message}");
         //         Log.Error(ex, "Ошибка при сохранении чертеже перед покраской");
         //      }
         //   }
         //}

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


         try
         {
            using (doc.LockDocument())
            {
               // Проверка дубликатов блоков            
               CheckDublicateBlocks.Check();

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
               //doc.Editor.WriteMessage("\nВыполните команду AlbumPanels для создания альбома покраски панелей с плиткой.");
               Log.Info("Покраска панелей выполнена успешно. {0}", doc.Name);

               if (Inspector.HasErrors)
               {
                  Inspector.Show();
               }
            }
         }
         catch (System.Exception ex)
         {
            doc.Editor.WriteMessage("\nНе выполнена покраска панелей. {0}", ex.Message);
            if (!ex.Message.Contains("Отменено пользователем"))
            {
               Log.Error(ex, "Не выполнена покраска панелей. {0}", doc.Name);
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
            bool repeat = false;
            PlotDirToPdf.EnumLayoutsSort layoutSort = PlotDirToPdf.EnumLayoutsSort.TabOrder;
            do
            {
               var optPrompt = new PromptKeywordOptions($"Печать листов в PDF из текущего чертежа, выбранных файлов или из всех чертежей в папке. Сортировка - {PlotDirToPdf.GetLayoutSortName(layoutSort)}");
               optPrompt.Keywords.Add("Текущего");
               optPrompt.Keywords.Add("Папки");
               optPrompt.Keywords.Add("Сортировка");
               optPrompt.Keywords.Default = "Папки";

               var resPrompt = ed.GetKeywords(optPrompt);
               if (resPrompt.Status == PromptStatus.OK)
               {
                  if (resPrompt.StringResult == "Текущего")
                  {
                     repeat = false;
                     Log.Info("Текущего");
                     try
                     {
                        if (!File.Exists(doc.Name))
                        {
                           throw new System.Exception("Нужно сохранить текущий чертеж.");
                        }
                        string filePdfName = Path.Combine(Path.GetDirectoryName(doc.Name), Path.GetFileNameWithoutExtension(doc.Name) + ".pdf");
                        PlotDirToPdf plotter = new PlotDirToPdf(new string[] { doc.Name }, filePdfName);
                        plotter.LayoutSort = layoutSort;
                        plotter.Plot();
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
                     repeat = false;
                     Log.Info("Папки");
                     var dialog = new AcadLib.UI.FileFolderDialog();
                     dialog.Dialog.Multiselect = true;
                     dialog.IsFolderDialog = true;
                     dialog.Dialog.Title = "Выбор папки или файлов для печати чертежей в PDF.";                     
                     dialog.Dialog.Filter = "Чертежи|*.dwg";
                     if (_album == null)
                     {
                        dialog.Dialog.InitialDirectory = Path.GetDirectoryName(doc.Name);
                     }
                     else
                     {
                        dialog.Dialog.InitialDirectory = _album.AlbumDir == null ? Path.GetDirectoryName(doc.Name) : _album.AlbumDir;
                     }
                     if (dialog.ShowDialog() == DialogResult.OK)
                     {
                        try
                        {
                           PlotDirToPdf plotter;
                           string firstFileNameWoExt = Path.GetFileNameWithoutExtension(dialog.Dialog.FileNames.First());
                           if (dialog.Dialog.FileNames.Count()>1)
                           {
                              plotter = new PlotDirToPdf(dialog.Dialog.FileNames, Path.GetFileName(dialog.SelectedPath));
                           }
                           else if (firstFileNameWoExt.Equals("п", StringComparison.OrdinalIgnoreCase))
                           {                              
                              plotter = new PlotDirToPdf(dialog.SelectedPath);
                           }
                           else
                           {
                              plotter = new PlotDirToPdf(dialog.Dialog.FileNames, firstFileNameWoExt);
                           }                         
                           plotter.LayoutSort = layoutSort;
                           plotter.Plot();
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
                  else if (resPrompt.StringResult == "Сортировка")
                  {
                     repeat = true;
                     var keyOpSort = new PromptKeywordOptions("Сортировка листов по порядку вкладок или по именам листов");
                     keyOpSort.Keywords.Add("Вкладки");
                     keyOpSort.Keywords.Add("Имена");
                     keyOpSort.Keywords.Default = "Вкладки";
                     var res = ed.GetKeywords(keyOpSort);
                     if (res.Status == PromptStatus.OK)
                     {                        
                        if (res.StringResult == "Вкладки")
                        {
                           layoutSort = AcadLib.Plot.PlotDirToPdf.EnumLayoutsSort.TabOrder;
                        }
                        else if (res.StringResult == "Имена")
                        {
                           layoutSort = AcadLib.Plot.PlotDirToPdf.EnumLayoutsSort.LayoutNames;
                        }
                     }
                  }
               }
               else
               {
                  ed.WriteMessage("\nОтменено пользователем.");
                  return;
               }
            } while (repeat);
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

      /// <summary>
      /// Построение фасадов из создаваемых панелей по описанию в XML.
      /// </summary>
      [CommandMethod("PIK", "AKR-CreateFacade", CommandFlags.Modal)]
      public void CreateFacade()
      {
         Log.Info("Start Command: AKR-CreateFacade");
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         if (doc == null) return;         
         Database db = doc.Database;
         try
         {            
            Inspector.Clear();

            // Определение фасадов
            List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();

            if (facadesMounting.Count == 0)
            {
               Inspector.AddError("Не определены фасады в чертеже - по монтажным планам.", icon: System.Drawing.SystemIcons.Error);
               throw new System.Exception("Отменено пользователем");
            }

            // Загрузка базы панелей из XML
            BaseService baseService = new BaseService();
            baseService.ReadPanelsFromBase();

            // Очиста чертежа от блоков панелей АКР
            try
            {
               baseService.ClearPanelsAkrFromDrawing(db);
            }
            catch (System.Exception ex)
            {
               Log.Error(ex, "baseService.ClearPanelsAkrFromDrawing(db);");
            }
            // Подготовка - копирование блоков, слоев, стилей, и т.п.
            baseService.InitToCreationPanels(db);

            // Определение арх планов
            List<FloorArchitect> floorsAr = FloorArchitect.GetAllPlanes(db, baseService);
                        
            // Создание определений блоков панелей по базе                
            baseService.CreateBtrPanels(facadesMounting, floorsAr);

            // Заморозка слоев образмеривания панелей
            baseService.FreezeDimLayers();            

            //Создание фасадов
            FacadeMounting.CreateFacades(facadesMounting);

            // Замена ассоциативных штриховок к блоках сечений
            using (var t = db.TransactionManager.StartTransaction())
            {
               var secBlocks = baseService.Env.BlPanelSections;
               foreach (var item in secBlocks)
               {
                  item.ReplaceAssociateHatch();
               }
               t.Commit();
            }

            doc.Editor.WriteMessage("\nПостроение фасада завершено.");
            doc.Editor.WriteMessage("\nНеобходимо выполнить проверку чертежа с исправлением ошибок!");            
         }
         catch (System.Exception ex)
         {
            doc.Editor.WriteMessage($"\nНе удалось выполнить построение фасада. {ex.Message}");
            if (!ex.Message.Contains("Отменено пользователем"))
            {
               Log.Error(ex, $"Command: AKR-CreateFacade. {doc.Name}");
            }
         }
         if (Inspector.HasErrors)
         {
            Inspector.Show();
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

      //[CommandMethod("PIK", "AKR-RemoveWindowSuffixFromMountingPanels", CommandFlags.Modal)]
      //public void RemoveWindowSuffixFromMountingPanelsCommands()
      //{
      //   Document doc = AcAp.DocumentManager.MdiActiveDocument;
      //   Editor ed = doc.Editor;
      //   Database db = doc.Database;

      //   using (var t = db.TransactionManager.StartTransaction())
      //   {
      //      // Все монтажные блоки панелей в модели
      //      var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
      //      var mountingsPanelsInMs = MountingPanel.GetPanels(ms, Point3d.Origin, Matrix3d.Identity, null);
      //      mountingsPanelsInMs.ForEach(p => p.RemoveWindowSuffix());
      //      foreach (ObjectId idEnt in ms)
      //      {
      //         if (idEnt.ObjectClass.Name == "AcDbBlockReference")
      //         {
      //            var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;                  
      //            if (blRefMounting.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName, StringComparison.CurrentCultureIgnoreCase))
      //            {
      //               var btr = blRefMounting.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
      //               var mountingsPanels = MountingPanel.GetPanels(btr, blRefMounting.Position, blRefMounting.BlockTransform , null);
      //               mountingsPanels.ForEach(p => p.RemoveWindowSuffix());
      //            }
      //         }
      //      }
      //      t.Commit();
      //   }
      //   ed.Regen();
      //}

      [CommandMethod("PIK", "AKR-RemoveMarkPaintingFromMountingPanels", CommandFlags.Modal)]
      public void RemoveMarkPaintingFromMountingPanels()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         Editor ed = doc.Editor;
         Database db = doc.Database;

         using (var t = db.TransactionManager.StartTransaction())
         {
            // Все монтажные блоки панелей в модели
            var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
            var mountingsPanelsInMs = MountingPanel.GetPanels(ms, Point3d.Origin, Matrix3d.Identity, null);
            mountingsPanelsInMs.ForEach(p => p.RemoveMarkPainting());
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  if (blRefMounting.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName, StringComparison.CurrentCultureIgnoreCase))
                  {
                     var btr = blRefMounting.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                     var mountingsPanels = MountingPanel.GetPanels(btr, blRefMounting.Position, blRefMounting.BlockTransform, null);
                     mountingsPanels.ForEach(p => p.RemoveMarkPainting());
                  }
               }
            }
            t.Commit();
         }
         ed.Regen();
      }

      [CommandMethod("PIK", "TestClearXdataAKRPanels", CommandFlags.Modal)]
      public void TestClearXdataAKRPanels()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         Editor ed = doc.Editor;
         Database db = doc.Database;

         int countRemovedDict = 0;
         int countRemovedXData = 0;

         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            Point3d pt = Point3d.Origin;
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (btr.Name.StartsWith(Settings.Default.BlockPanelAkrPrefixName))
               {
                  if (!btr.ExtensionDictionary.IsNull)
                  {
                     btr.RemoveAllExtensionDictionary();
                     ed.WriteMessage("{0} удален словарь.", btr.Name);
                     countRemovedDict++;
                  }
                  if (btr.XData != null)
                  {
                     btr.RemoveAllXData();
                     ed.WriteMessage("{0} удалы расш данные.", btr.Name);
                     countRemovedXData++;
                  }
               }
            }
            ed.WriteMessage("Удалено словарей {0}, удалено расшданных {1}", countRemovedDict, countRemovedXData);
            t.Commit();
         }
      }

      [CommandMethod("PIK", "TestInsertAKRPanels", CommandFlags.Modal)]
      public void TestInsertAKRPanels()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         Editor ed = doc.Editor;
         Database db = doc.Database;

         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            Point3d pt = Point3d.Origin;
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (btr.Name.StartsWith(Settings.Default.BlockPanelAkrPrefixName))
               {
                  var blRef = new BlockReference(pt, idBtr);
                  ms.AppendEntity(blRef);
                  t.AddNewlyCreatedDBObject(blRef, true);
                  pt = new Point3d(pt.X, pt.Y + 5000, 0);
               }
            }
            t.Commit();
         }
      }
      
      [CommandMethod("PIK", "TestReplaceWindows", CommandFlags.Modal)]
      public void TestReplaceWindows()
      {
         // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
         UtilsReplaceWindows testReplaceWindows = new UtilsReplaceWindows();
         testReplaceWindows.Replace();
      }

      [CommandMethod("PIK", "TestRemoveDashAKR", CommandFlags.Modal)]
      public void TestRemoveDashAKR()
      {
         // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
         UtilsRemoveDash testRemoveDash = new UtilsRemoveDash();
         testRemoveDash.RemoveDashAKR();
      }         
   }
}
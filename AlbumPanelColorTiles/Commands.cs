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
using AlbumPanelColorTiles.Base;
using AlbumPanelColorTiles.ExportFacade;
using AlbumPanelColorTiles.Utils;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.PanelLibrary.LibEditor;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.RandomPainting;
using AlbumPanelColorTiles.RenamePanels;
using AlbumPanelColorTiles.Utils.CopyDict;
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
        private const string groupPik = AcadLib.Commands.Group;
        private static string _curDllDir;
        private static DateTime _lastStartCommandDateTime;
        private static string _lastStartCommandName = string.Empty;
        private Album _album;
        private ImagePaintingService _imagePainting;
        private string _msgHelp;
        private RandomPaintService _randomPainting;

        public static string CurDllDir {
            get {
                if (_curDllDir == null)
                {
                    _curDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _curDllDir;
            }
        }

        private string MsgHelp {
            get {
                if (_msgHelp == null)
                {
                    _msgHelp = $"\nПрограмма для покраски плитки и создания альбома панелей." +
                           $"\nВерсия программы " + Assembly.GetExecutingAssembly().GetName().Version +
                           $"\nКоманды:" +
                           $"\n{nameof(AKR_PaintPanels)} - покраска блоков панелей." +
                           $"\n{nameof(AKR_ResetPanels)} - удаление блоков панелей Марки АР и замена их на блоки панелей Марки СБ." +
                           $"\n{nameof(AKR_AlbumPanels)} - создание альбома панелей." +
                           $"\n{nameof(AKR_PlotPdf)} - печать в PDF текущего чертежа или выбранной папки с чертежами. Файлы создается в корне чертежа с тем же именем. Печать выполняется по настройкам на листах." +
                           $"\n{nameof(AKR_SelectPanels)} - выбор блоков панелей в Модели." +
                           $"\n{nameof(AKR_RandomPainting)} - случайное распределение зон покраски в указанной области." +
                           $"\n{nameof(AKR_ImagePainting)} - покраска области по выбранной картинке." +
                           $"\n{nameof(AKR_SavePanelsToLibrary)} - сохранение АКР-Панелей из текущего чертежа в библиотеку." +
                           $"\n{nameof(AKR_LoadPanelsFromLibrary)} - загрузка АКР-Панелей из библиотеки в текущий чертеж в соответствии с монтажными планами конструкторов." +
                           $"\n{nameof(AKR_CreatePlanBlocks)} - создание блоков монтажек из монтажных планов конструкторов." +
                           $"\n{nameof(AKR_ExportFacade)} - экспорт панелей АКР в файл фасада." +                           
                           $"\n{nameof(AKR_CopyDictionary)} - копирование словаря текущего чертежа в другой чертеж. В словаре хранится список переименований панелей, индекс проекта, номер первого этажа." +
                           $"\nИмена блоков и слоев:" +
                           $"\nБлоки панелей с префиксом - " + Settings.Default.BlockPanelAkrPrefixName + ", дальше марка СБ, без скобок в конце." +
                           $"\nБлок зоны покраски (на слое марки цвета для плитки) - " + Settings.Default.BlockColorAreaName +
                           $"\nБлок плитки (разложенная в блоке панели) - " + Settings.Default.BlockTileName +
                           $"\nБлок обозначения стороны фасада на монтажном плане - " + Settings.Default.BlockFacadeName +
                           $"\nБлок рамки со штампом - " + Settings.Default.BlockFrameName +
                           $"\nПанели Чердака на слое - " + Settings.Default.LayerUpperStoreyPanels +
                           $"\nПанели Парапета на слое - " + Settings.Default.LayerParapetPanels +
                           $"\nПанели торцевые с суффиксом _тп или _тл после марки СБ в конце имени блока панели." +
                           $"\nПанели с разными окнами - с суффиксом _ок (русскими буквами) и номером в конце имени блока панели. Например _ОК1" +
                           $"\nСлой для окон в панелях (замораживается на листе формы панели марки АР) - " + Settings.Default.LayerWindows +
                           $"\nСлой для размеров на фасаде в панели (замораживается на листе формы) - " + Settings.Default.LayerDimensionFacade +
                           $"\nСлой для размеров в форме в панели (замораживается на листе фасада) - " + Settings.Default.LayerDimensionFacade +
                           $"\nОбрабатываются только блоки в текущем чертеже в Модели. Внешние ссылки не учитываются.\n";
                }
                return _msgHelp;
            }
        }

        public void CheckAcadVer2016 ()
        {
            var minVer = new Version(20, 1);
            if (Autodesk.AutoCAD.ApplicationServices.Application.Version < minVer)
            {
                string msg = "Команда может работать с фатальными ошибками на версиях AutoCAD ниже 2016 sp1.";
                if (MessageBox.Show(msg, "Предупреждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                {
                    throw new System.Exception(General.CanceledByUser);
                }
                //doc.Editor.WriteMessage("\nКоманда создания альбома временно не работает на версиях ниже 2016 sp1.");
                //return;
            }
        }

        [CommandMethod(groupPik, nameof(AKR_Help), CommandFlags.Modal)]
        public void AKR_Help ()
        {
            CommandStart.Start(doc =>
            {
                Editor ed = doc.Editor;
                ed.WriteMessage("\n{0}", MsgHelp);
                // Открытие папки с инструкциями в проводнике                
                System.Diagnostics.Process.Start("explorer", @"\\picompany.ru\root\ecp_sapr_exchange\01_Публикация\02_Справочные материалы и инструкции\02_AutoCAD");
                //"\\dsk2.picompany.ru\project\CAD_Settings\_Шаблоны & типовые решения\30_АР\3.01_Обучение_АКР"                
            });
        }

        [CommandMethod(groupPik, nameof(AKR_PaintPanels), CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
        public void AKR_PaintPanels ()
        {
            CommandStart.Start(doc =>
            {
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
                //         Logger.Log.Error(ex, "Ошибка при сохранении чертеже перед покраской");
                //      }
                //   }
                //}

                string commandName = nameof(AKR_PaintPanels);
                if (string.Equals(_lastStartCommandName, commandName))
                {
                    if ((DateTime.Now - _lastStartCommandDateTime).Seconds < 5)
                    {
                        doc.Editor.WriteMessage("Между запусками команды прошло меньше 5 секунд. Отмена.");
                        return;
                    }
                }

                // Проверка дубликатов блоков            
                Select.SelectionBlocks sel = new Select.SelectionBlocks (doc.Database);
                sel.SelectBlRefsInModel(false);
                var panelsToCheck = sel.IdsBlRefPanelAr;
                panelsToCheck.AddRange(sel.IdsBlRefPanelSb);
                CheckDublicateBlocks.Check(panelsToCheck);

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

                // Покраска
                _album.PaintPanels();

                doc.Editor.Regen();
                doc.Editor.WriteMessage("\nПокраска панелей выполнена успешно.");
                Logger.Log.Info("Покраска панелей выполнена успешно. {0}", doc.Name);

                _lastStartCommandName = commandName;
                _lastStartCommandDateTime = DateTime.Now;
            });
        }

        // Удаление блоков панелей марки АР и их замена на блоки панелей марок СБ.
        [CommandMethod(groupPik, nameof(AKR_ResetPanels), CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
        public void AKR_ResetPanels ()
        {
            CommandStart.Start(doc =>
            {
                using (var DocLock = doc.LockDocument())
                {
                    if (_album != null)
                    {
                        _album.ResetData();
                    }
                    Album.ResetBlocks();
                    doc.Editor.Regen();
                    doc.Editor.WriteMessage("\nСброс блоков выполнен успешно.");
                }
            });
        }

        // Создание альбома колористических решений панелей (Альбома панелей).
        [CommandMethod(groupPik, nameof(AKR_AlbumPanels), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void AKR_AlbumPanels ()
        {
            CommandStart.Start(doc =>
            {
                CheckAcadVer2016();

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

                if (_album == null)
                {
                    doc.Editor.WriteMessage("\nСначала нужно выполнить команду PaintPanels для покраски плитки.");
                }
                else
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
                        Logger.Log.Info("Альбом панелей выполнен успешно: {0}", _album.AlbumDir);
                    }
                    else
                    {
                        doc.Editor.WriteMessage("\nОтменено пользователем.");
                    }
                }
                _lastStartCommandName = commandName;
                _lastStartCommandDateTime = DateTime.Now;
            });
        }

        [CommandMethod(groupPik, nameof(AKR_PlotPdf), CommandFlags.Modal | CommandFlags.Session)]
        public void AKR_PlotPdf ()
        {
            CommandStart.Start(doc =>
            {
                Editor ed = doc.Editor;
                CheckAcadVer2016();

                PlotOptions plotOpt = new PlotOptions();

                using (var lockDoc = doc.LockDocument())
                {
                    bool repeat = false;
                    do
                    {
                        var optPrompt = new PromptKeywordOptions($"\nПечать листов в PDF из текущего чертежа, выбранных файлов или из всех чертежей в папке.");
                        optPrompt.Keywords.Add("Текущего");
                        optPrompt.Keywords.Add("Папки");
                        optPrompt.Keywords.Add("Настройки");
                        optPrompt.Keywords.Default = "Папки";

                        var resPrompt = ed.GetKeywords(optPrompt);
                        if (resPrompt.Status == PromptStatus.OK)
                        {
                            if (resPrompt.StringResult == "Текущего")
                            {
                                repeat = false;
                                Logger.Log.Info("Текущего");
                                if (!File.Exists(doc.Name))
                                {
                                    throw new System.Exception("Нужно сохранить текущий чертеж.");
                                }
                                string filePdfName = Path.Combine(Path.GetDirectoryName(doc.Name), Path.GetFileNameWithoutExtension(doc.Name) + ".pdf");
                                PlotDirToPdf plotter = new PlotDirToPdf(new string[] { doc.Name }, filePdfName);
                                plotter.Options = plotOpt;
                                plotter.Plot();
                            }
                            else if (resPrompt.StringResult == "Папки")
                            {
                                repeat = false;
                                Logger.Log.Info("Папки");
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
                                    PlotDirToPdf plotter;
                                    string firstFileNameWoExt = Path.GetFileNameWithoutExtension(dialog.Dialog.FileNames.First());
                                    if (dialog.Dialog.FileNames.Count() > 1)
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
                                    plotter.Options = plotOpt;
                                    plotter.Plot();
                                }
                            }
                            else if (resPrompt.StringResult == "Настройки")
                            {
                                // Сортировка; Все файлы в один пдф или для каждого файла отдельная пдф
                                plotOpt.Show();
                                repeat = true;
                            }
                        }
                        else
                        {
                            ed.WriteMessage("\nОтменено пользователем.");
                            return;
                        }
                    } while (repeat);
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_SelectPanels), CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
        public void AKR_SelectPanels ()
        {
            CommandStart.Start(doc =>
            {
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
                    Logger.Log.Info("Выбрано блоков панелей в Модели: Марки СБ - {0}, Марки АР - {1}", countMarkSbPanels, countMarkArPanels);
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_RandomPainting), CommandFlags.Modal)]
        public void AKR_RandomPainting ()
        {
            CommandStart.Start(doc =>
            {
                using (var DocLock = doc.LockDocument())
                {
                    // Произвольная покраска участка, с % распределением цветов зон покраски.
                    if (_randomPainting == null)
                    {
                        _randomPainting = new RandomPaintService();
                    }
                    _randomPainting.Start();
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_ImagePainting), CommandFlags.Modal)]
        public void AKR_ImagePainting ()
        {
            CommandStart.Start(doc =>
            {
                using (var DocLock = doc.LockDocument())
                {
                    if (_imagePainting == null)
                    {
                        _imagePainting = new ImagePaintingService(doc);
                    }
                    _imagePainting.Go();
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_SavePanelsToLibrary), CommandFlags.Session | CommandFlags.Modal | CommandFlags.NoBlockEditor)]
        public void AKR_SavePanelsToLibrary ()
        {
            CommandStart.Start(doc =>
            {
                using (var lockDoc = doc.LockDocument())
                {
                    PanelLibrarySaveService panelLib = new PanelLibrarySaveService();
                    panelLib.SavePanelsToLibrary();
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_LoadPanelsFromLibrary), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void AKR_LoadPanelsFromLibrary ()
        {
            CommandStart.Start(doc =>
            {
                using (var DocLock = doc.LockDocument())
                {
                    PanelLibraryLoadService loadPanelsService = new PanelLibraryLoadService();
                    loadPanelsService.LoadPanels();
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_EditPanelLibrary), CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
        public void AKR_EditPanelLibrary ()
        {
            CommandStart.Start(doc =>
            {
                LibraryEditor libEditor = new LibraryEditor();
                libEditor.Edit();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_CreatePlanBlocks), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void AKR_CreatePlanBlocks ()
        {
            CommandStart.Start(doc =>
            {
                BlockPlans mountingPlans = new BlockPlans();
                mountingPlans.CreateBlockPlans();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_ExportFacade), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void AKR_ExportFacade ()
        {
            CommandStart.Start(doc =>
            {
                ExportFacadeService export = new ExportFacadeService();
                export.Export();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_InsertWindows), CommandFlags.Modal)]
        public void AKR_InsertWindows ()
        {
            CommandStart.Start(doc =>
            {
                // Файл шаблонов окон.
                string fileWins = Path.Combine(CurDllDir, Settings.Default.TemplateBlocksAkrWindows);
                // Слой для окон
                AcadLib.Layers.LayerInfo layerWin = new AcadLib.Layers.LayerInfo(Settings.Default.LayerWindows);
                layerWin.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 4);
                // Выбор и вставка блока окна. 
                AcadLib.Blocks.Visual.VisualInsertBlock.InsertBlock(fileWins, n => n.StartsWith(Settings.Default.BlockWindowName), layerWin);
            });
        }

        /// <summary>
        /// Копирование словаря АКР из этого чертежа в другой
        /// </summary>
        [CommandMethod(groupPik, nameof(AKR_CopyDictionary), CommandFlags.Modal | CommandFlags.Session)]
        public void AKR_CopyDictionary ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                Editor ed = doc.Editor;
                using (var DocLock = doc.LockDocument())
                {
                    // Запрос имени открытого чертежа в который нужно скопировать словарь
                    //var res = doc.Editor.GetString("Имя чертежа в который копировать словарь АКР");
                    List<SelectObject> docs = new List<SelectObject>();
                    foreach (Document item in Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager)
                    {
                        if (item == doc) continue;
                        docs.Add(new SelectObject(item, item.Name));
                    }

                    FormSelect formSelectDoc = new FormSelect(docs);
                    var res = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(formSelectDoc);
                    if (res == DialogResult.OK)
                    {
                        var docSelected = (Document)((SelectObject)formSelectDoc.SelectedItem).Object;
                        if (doc == docSelected)
                        {
                            MessageBox.Show("Выбран текущий чертеж!");
                        }
                        else
                        {
                            using (var lockItemDoc = docSelected.LockDocument())
                            {
                                Lib.DictNOD.CopyDict(docSelected.Database);
                                doc.Editor.WriteMessage("Словарь скопирован успешно.");
                            }
                        }
                        // Поиск чертежа среди открытых документов
                        //foreach (Document itemDoc in Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager)
                        //{
                        //    if (string.Equals(Path.GetFileName(itemDoc.Name), res.StringResult, System.StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        using (var lockItemDoc = itemDoc.LockDocument())
                        //        {
                        //            Lib.DictNOD.CopyDict(itemDoc.Database);
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        return;
                    }
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_AirConditionersCalc), CommandFlags.Modal)]
        public void AKR_AirConditionersCalc ()
        {
            CommandStart.Start(doc =>
            {
                Utils.AirConditioners.AirConditionersCalc airCondCalc = new Utils.AirConditioners.AirConditionersCalc();
                airCondCalc.Calc();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_ColorAreaCopy), CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void AKR_ColorAreaCopy ()
        {
            CommandStart.Start(doc =>
            {
                UtilsCopyColorArea.Copy();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_ColorAreaPaste), CommandFlags.Modal)]
        public void AKR_ColorAreaPaste ()
        {
            CommandStart.Start(doc =>
            {
                UtilsCopyColorArea.Paste();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_Navigator), CommandFlags.Modal)]
        public void AKR_Navigator ()
        {
            CommandStart.Start((doc) =>
            {
                Utils.Navigator.UtilsSelectPanelsByHeight.ShowPanelsByHeight();
            });
        }

        [CommandMethod(groupPik, nameof(AKR_TileCalc), CommandFlags.Modal)]
        public void AKR_TileCalc ()
        {
            CommandStart.Start((doc) =>
            {
                Utils.TileTable.UtilsTileTable tileTable = new Utils.TileTable.UtilsTileTable (doc);
                tileTable.CreateTable();
            });
        }        

        [CommandMethod(groupPik, nameof(AKR_RemoveMarkPaintingFromMountingPanels), CommandFlags.Modal)]
        public void AKR_RemoveMarkPaintingFromMountingPanels ()
        {
            CommandStart.Start(doc =>
            {
                Editor ed = doc.Editor;
                Database db = doc.Database;

                using (var t = db.TransactionManager.StartTransaction())
                {
                    // Все монтажные блоки панелей в модели
                    var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
                    var mountingsPanelsInMs = MountingPanel.GetPanels(ms, Point3d.Origin, Matrix3d.Identity, null, null);
                    mountingsPanelsInMs.ForEach(p => p.RemoveMarkPainting());
                    foreach (ObjectId idEnt in ms)
                    {
                        if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                        {
                            var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                            if (blRefMounting.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                var btr = blRefMounting.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                                var mountingsPanels = MountingPanel.GetPanels(btr, blRefMounting.Position, blRefMounting.BlockTransform, null, null);
                                mountingsPanels.ForEach(p => p.RemoveMarkPainting());
                            }
                        }
                    }
                    t.Commit();
                }
                ed.Regen();
            });
        }        

        [CommandMethod(groupPik, nameof(AKR_Utils_InsertAKRPanels), CommandFlags.Modal)]
        public void AKR_Utils_InsertAKRPanels ()
        {
            CommandStart.Start(doc =>
            {
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
                            pt = new Point3d(pt.X, pt.Y + 7000, 0);
                        }
                    }
                    t.Commit();
                }
            });
        }

        [CommandMethod(groupPik, nameof(AKR_Utils_WindowsRedefine), CommandFlags.Modal)]
        public void AKR_Utils_WindowsRedefine ()
        {
            CommandStart.Start(doc =>
            {
                UtilsReplaceWindows testReplaceWindows = new UtilsReplaceWindows();
                var count = testReplaceWindows.Redefine();
                doc.Editor.WriteMessage($"\nЗаменено {count} окон.");
            });
        }
        


        //
        //
        //  Вспомогательные, разовые, тестовые, не используемые команды
        //
        //

        //[CommandMethod(groupPik, nameof(AKR_UtilsRemoveDescFromOBR), CommandFlags.Modal)]
        //public void AKR_UtilsRemoveDescFromOBR ()
        //{
        //    CommandStart.Start((doc) =>
        //    {
        //        UtilsRemoveDescrInObr.Remove();
        //    });
        //}

        ///// <summary>
        ///// Создание фасадов из правильно расставленных блоков монтажных планов с блоками обозначения сторон фасада
        ///// Панели АКР создаются по описанию из базы Конструкторов
        ///// </summary>
        //[CommandMethod(groupPik, nameof(AKR_CreateFacadeCommand), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        //public void AKR_CreateFacadeCommand ()
        //{
        //    CommandStart.Start(doc =>
        //    {
        //        using (var DocLock = doc.LockDocument())
        //        {
        //            PanelLibraryLoadService loadPanelsService = new PanelLibraryLoadService();
        //            loadPanelsService.LoadPanels();
        //        }
        //    });
        //}

        ///// <summary>
        ///// Построение фасадов из создаваемых панелей по описанию в XML.
        ///// </summary>
        //[CommandMethod(groupPik, nameof(AKR_CreateFacade), CommandFlags.Modal)]
        //public void AKR_CreateFacade ()
        //{
        //    CommandStart.Start(doc =>
        //    {
        //        Database db = doc.Database;
        //        // Определение фасадов
        //        List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();

        //        if (facadesMounting.Count == 0)
        //        {
        //            Inspector.AddError("Не определены фасады в чертеже - по монтажным планам.", icon: System.Drawing.SystemIcons.Error);
        //            throw new System.Exception("Отменено пользователем");
        //        }

        //        // Загрузка базы панелей из XML
        //        BaseService baseService = new BaseService();
        //        baseService.ReadPanelsFromBase();

        //        // Очиста чертежа от блоков панелей АКР
        //        try
        //        {
        //            baseService.ClearPanelsAkrFromDrawing(db);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Logger.Log.Error(ex, "baseService.ClearPanelsAkrFromDrawing(db);");
        //        }
        //        // Подготовка - копирование блоков, слоев, стилей, и т.п.
        //        baseService.InitToCreationPanels(db);

        //        // Определение арх планов
        //        List<FloorArchitect> floorsAr = FloorArchitect.GetAllPlanes(db, baseService);

        //        // Создание определений блоков панелей по базе                
        //        baseService.CreateBtrPanels(facadesMounting, floorsAr);

        //        // Заморозка слоев образмеривания панелей
        //        baseService.FreezeDimLayers();

        //        //Создание фасадов
        //        FacadeMounting.CreateFacades(facadesMounting);

        //        // Замена ассоциативных штриховок к блоках сечений
        //        using (var t = db.TransactionManager.StartTransaction())
        //        {
        //            var secBlocks = baseService.Env.BlPanelSections;
        //            foreach (var item in secBlocks)
        //            {
        //                item.ReplaceAssociateHatch();
        //            }
        //            t.Commit();
        //        }

        //        doc.Editor.WriteMessage("\nПостроение фасада завершено.");
        //        doc.Editor.WriteMessage("\nНеобходимо выполнить проверку чертежа с исправлением ошибок!");
        //    });
        //}

        //[CommandMethod(groupPik, nameof(AKR_RemoveWindowSuffixFromMountingPanels), CommandFlags.Modal)]
        //public void AKR_RemoveWindowSuffixFromMountingPanels ()
        //{
        //    CommandStart.Start(doc =>
        //    {
        //        Editor ed = doc.Editor;
        //        Database db = doc.Database;

        //        using (var t = db.TransactionManager.StartTransaction())
        //        {
        //            // Все монтажные блоки панелей в модели
        //            var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
        //            var mountingsPanelsInMs = MountingPanel.GetPanels(ms, Point3d.Origin, Matrix3d.Identity, null, null);
        //            mountingsPanelsInMs.ForEach(p => p.RemoveWindowSuffix());
        //            foreach (ObjectId idEnt in ms)
        //            {
        //                if (idEnt.ObjectClass.Name == "AcDbBlockReference")
        //                {
        //                    var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
        //                    if (blRefMounting.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName, StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        var btr = blRefMounting.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
        //                        var mountingsPanels = MountingPanel.GetPanels(btr, blRefMounting.Position, blRefMounting.BlockTransform, null, null);
        //                        mountingsPanels.ForEach(p => p.RemoveWindowSuffix());
        //                    }
        //                }
        //            }
        //            t.Commit();
        //        }
        //        ed.Regen();
        //    });
        //}

        //[CommandMethod(groupPik,nameof(AKR_UtilsClearXdataAKRPanels), CommandFlags.Modal)]
        //public void AKR_UtilsClearXdataAKRPanels ()
        //{
        //    CommandStart.Start(doc =>
        //    {
        //        Editor ed = doc.Editor;
        //        Database db = doc.Database;

        //        int countRemovedDict = 0;
        //        int countRemovedXData = 0;

        //        using (var t = db.TransactionManager.StartTransaction())
        //        {
        //            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
        //            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        //            Point3d pt = Point3d.Origin;
        //            foreach (ObjectId idBtr in bt)
        //            {
        //                var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
        //                if (btr.Name.StartsWith(Settings.Default.BlockPanelAkrPrefixName))
        //                {
        //                    if (!btr.ExtensionDictionary.IsNull)
        //                    {
        //                        btr.RemoveAllExtensionDictionary();
        //                        ed.WriteMessage("{0} удален словарь.", btr.Name);
        //                        countRemovedDict++;
        //                    }
        //                    if (btr.XData != null)
        //                    {
        //                        btr.RemoveAllXData();
        //                        ed.WriteMessage("{0} удалы расш данные.", btr.Name);
        //                        countRemovedXData++;
        //                    }
        //                }
        //            }
        //            ed.WriteMessage("Удалено словарей {0}, удалено расшданных {1}", countRemovedDict, countRemovedXData);
        //            t.Commit();
        //        }
        //    });
        //}

        //[CommandMethod(groupPik, nameof(AKR_UtilsRemoveDashAKR), CommandFlags.Modal)]
        //public void AKR_UtilsRemoveDashAKR ()
        //{
        //    CommandStart.Start(doc =>
        //    {
        //        // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
        //        UtilsRemoveDash testRemoveDash = new UtilsRemoveDash();
        //        testRemoveDash.RemoveDashAKR();
        //    });
        //}

        //[CommandMethod("PIK", "UtilsReplaceMtextDescriptionOBR", CommandFlags.Modal)]
        //public void UtilsReplaceMtextDescriptionOBR()
        //{            
        //    UtilDescriptionInOBR.Check();            
        //}
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlbumPanelColorTiles.Model;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Plot;
using Autodesk.AutoCAD.ApplicationServices;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

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
                      "\nPaintPanels - покраска блоков панелей." +
                      "\nResetPanels - удаление блоков панелей Марки АР и замена их на блоки панелей Марки СБ." +
                      "\nAlbumPanels - создание альбома панелей." +
                      "\nPlotPdf - печать в PDF текущего чертежа или выбранной папки с чертежами. Файлы создается в корне чертежа с тем же именем. Печать выполняется по настройкам на листах." +
                      "\nSelectPanels - выбор блоков панелей в Модели." +
                      "\nInsertBlockColorArea - вставка блока зоны покраски." +
                      "\nСправка: имена блоков:" +
                      "\nБлоки панелей с префиксом - " + Album.Options.BlockPanelPrefixName + ", дальше марка СБ, без скобок в конце." +
                      "\nБлок зоны покраски (на слое марки цвета для плитки) - " + Album.Options.BlockColorAreaName +
                      "\nБлок плитки (разложенная в блоке панели) - " + Album.Options.BlockTileName +
                      "\nПанели чердака на слое - " + Album.Options.LayerUpperStoreyPanels +
                      "\nПанели торцевые с суффиксом _тп или _тл после марки СБ в имени блока панели." +
                      "\nСлой для окон в панелях (замораживается на листе формы панели марки АР) - " + Album.Options.LayerWindows +
                      "\nСлой для размеров на фасаде в панели (замораживается на листе формы) - " + Album.Options.LayerDimensionFacade +
                      "\nСлой для размеров в форме в панели (замораживается на листе фасада) - " + Album.Options.LayerDimensionFacade +
                      "\nОбрабатываются только блоки в текущем чертеже. Внешние ссылки не учитываются.\n";
            }
            return _msgHelp;
         }
      }

      [CommandMethod("AKR", "PlotPdf", CommandFlags.Modal | CommandFlags.Session)]
      public void PlotPdf()
      {
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
                  try
                  {
                     plotter.PlotCur();
                  }
                  catch (System.Exception ex)
                  {
                     ed.WriteMessage("\n" + ex.Message);
                  }           
                  
               }
               else if (resPrompt.StringResult == "Папки")
               {
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
                     }                     
                  }
               }
            }
         }
      }

      //[CommandMethod("AKR", "PlotPdfDst", CommandFlags.Modal | CommandFlags.NoBlockEditor)]
      //public void PlotPdfDst()
      //{
      //   Document doc = AcAp.DocumentManager.MdiActiveDocument;
      //   if (doc == null) return;

      //   // Regen
      //   (doc.GetAcadDocument() as dynamic).Regen((dynamic)1);

      //   Database db = HostApplicationServices.WorkingDatabase;
      //   short bgp = (short)AcAp.GetSystemVariable("BACKGROUNDPLOT");
      //   try
      //   {
      //      AcAp.SetSystemVariable("BACKGROUNDPLOT", 0);
      //      using (Transaction tr = db.TransactionManager.StartTransaction())
      //      {
      //         List<Layout> layouts = new List<Layout>();
      //         DBDictionary layoutDict = (DBDictionary)db.LayoutDictionaryId.GetObject(OpenMode.ForRead);
      //         foreach (DBDictionaryEntry entry in layoutDict)
      //         {
      //            if (entry.Key != "Model")
      //            {
      //               layouts.Add((Layout)tr.GetObject(entry.Value, OpenMode.ForRead));
      //            }
      //         }
      //         layouts.Sort((l1, l2) => l1.TabOrder.CompareTo(l2.TabOrder));

      //         string filename = Path.ChangeExtension(db.Filename, "pdf");

      //         MultiSheetsPdf plotter = new MultiSheetsPdf(filename, layouts);
      //         plotter.Publish();

      //         tr.Commit();
      //      }
      //   }
      //   catch (System.Exception e)
      //   {
      //      Editor ed = AcAp.DocumentManager.MdiActiveDocument.Editor;
      //      ed.WriteMessage("\nError: {0}\n{1}", e.Message, e.StackTrace);
      //   }
      //   finally
      //   {
      //      AcAp.SetSystemVariable("BACKGROUNDPLOT", bgp);
      //   }
      //}

      // Создание альбома колористических решений панелей (Альбома панелей).
      [CommandMethod("PIK", "AlbumPanels", CommandFlags.Modal | CommandFlags.Session | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
      public void AlbumPanelsCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
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
                  doc.Editor.WriteMessage("\nАльбом панелей выполнен успешно:" + _album.AlbumDir);

                  var optPromptPlot = new PromptKeywordOptions("\nНапечатать альбом в PDF?");
                  optPromptPlot.Keywords.Add("Да");
                  optPromptPlot.Keywords.Add("Нет");
                  var promptPlotRes = doc.Editor.GetKeywords(optPromptPlot);
                  if (promptPlotRes.Status == PromptStatus.OK)
                  {
                     if (promptPlotRes.StringResult == "Да")
                     {
                        PlotMultiPDF plotMultiPdf = new PlotMultiPDF();
                        try
                        {
                           plotMultiPdf.PlotDir(_album.AlbumDir);
                        }
                        catch (System.Exception ex)
                        {
                           doc.Editor.WriteMessage("\n" + ex.Message);
                        }                        
                     }
                  }
               }
               catch (System.Exception ex)
               {
                  doc.Editor.WriteMessage("\nНе удалось создать альбом панелей. " + ex.Message);
               }
            }
         }
      }

      [CommandMethod("AKR", "InsertBlockColorArea", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void InsertBlockColorAreaCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         try
         {
            // Имя вставляемого блока.
            string blName = Album.Options.BlockColorAreaName;

            using (var t = doc.TransactionManager.StartTransaction())
            {
               var bt = (BlockTable)t.GetObject(db.BlockTableId, OpenMode.ForRead);
               if (!bt.Has(blName))
               {
                  // Копирование определенич блока из файла с блоками.
                  string fileBlocksTemplate = Path.Combine(CurDllDir, Album.Options.TemplateBlocksAKRFileName);
                  if (!File.Exists(fileBlocksTemplate))
                  {
                     throw new System.Exception("Не найден файл-шаблон с блоками " + fileBlocksTemplate);
                  }
                  Blocks.CopyBlockFromExternalDrawing(blName, fileBlocksTemplate, db);
               }
               ObjectId blockId = bt[blName];

               Point3d pt = new Point3d(0, 0, 0);
               BlockReference br = new BlockReference(pt, blockId);
               BlockInsertJig entJig = new BlockInsertJig(br);

               // jig
               var pr = ed.Drag(entJig);
               if (pr.Status == PromptStatus.OK)
               {
                  BlockTableRecord btr = (BlockTableRecord)t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                  btr.AppendEntity(entJig.GetEntity());
                  t.AddNewlyCreatedDBObject(entJig.GetEntity(), true);
               }
               t.Commit();
            }
         }
         catch (System.Exception ex)
         {
            ed.WriteMessage("\n" + ex.Message);
         }
      }

      // Покраска панелей в Моделе (по блокам зон покраски)
      [CommandMethod("PIK", "PaintPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void PaintPanelsCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         using (var DocLock = doc.LockDocument())
         {
            try
            {
               doc.Editor.WriteMessage(MsgHelp);
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
               doc.Editor.WriteMessage("\nНе выполнена покраска панелей. " + ex.Message);
            }
         }
      }

      // Удаление блоков панелей марки АР и их замена на блоки панелей марок СБ.
      [CommandMethod("PIK", "ResetPanels", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace | CommandFlags.Modal)]
      public void ResetPanelsCommand()
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
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
            }
         }
      }

      [CommandMethod("AKR", "SelectPanels", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
      public void SelectPanelsCommand()
      {
         // Выбор блоков панелей на чертеже в Модели
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
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
            ed.WriteMessage("\nВыбрано блоков панелей в Модели: Марки СБ - " + countMarkSbPanels + ", Марки АР - " + countMarkArPanels);
         }
      }
   }
}
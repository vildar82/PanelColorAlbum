using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Model.Select;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   // Экспорт фасада
   public class ExportFacadeService
   {
      private FileExport _fileExport;
      public ConvertPanelService CPS { get; private set; }
      public SelectionBlocks SelectPanels { get; private set; }

      /// <summary>
      /// Экспорт фасада для АР
      /// </summary>
      public void Export()
      {
         // Список панелей для экспорта
         SelectPanels = new SelectionBlocks();
         SelectPanels.SelectBlRefsInModel(false);
         if (SelectPanels.IdsBlRefPanelSb.Count > 0)
         {
            Inspector.AddError("В текущем чертеже в Модели не должно быть панелей Марки СБ (только Марки АР).", icon: System.Drawing.SystemIcons.Error);
            return;
         }
         if (SelectPanels.IdsBlRefPanelAr.Count == 0)
         {
            Inspector.AddError("Не найдены панели Марки АР в Моделе текущего чертежа.", icon: System.Drawing.SystemIcons.Error);
            return;
         }

         // Определить файл в который экспортировать фасад
         _fileExport = new FileExport();
         _fileExport.DefineFile();

         // определение фасадов (вокруг панелей АКР)
         var facades = Facade.GetFacades(SelectPanels.FacadeBlRefs);

         // Определение экспортируемых панелей и фасадов
         CPS = new ConvertPanelService(this);
         CPS.DefinePanels(facades);

         using (Database dbExport = new Database(!_fileExport.IsExistsFileExport, true))
         {            
            if (_fileExport.IsExistsFileExport)
            {
               // удалить старые панели из файла экспорта
               try
               {
                  dbExport.ReadDwgFile(_fileExport.FileExportFacade.FullName, FileShare.Read, true, "");                  
               }
               catch (Exception ex)
               {
                  // файл занят.
                  warningMessageBusyFileExportFacade(ex, _fileExport.FileExportFacade.FullName);
                  throw;
               }
               // сделать копию файла
               _fileExport.Backup();
               dbExport.CloseInput(true);
               deletePanels(dbExport);
            }
            dbExport.CloseInput(true);
            CPS.DbExport = dbExport;

            // Копирование панелей АР в экспортный файл
            copyPanelToExportFile(dbExport);

            // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
            redefineBlockTile(dbExport);

            using (AcadLib.WorkingDatabaseSwitcher switchDb = new AcadLib.WorkingDatabaseSwitcher(dbExport))
            {
               using (var t = dbExport.TransactionManager.StartTransaction())
               {
                  // Преобразования определений блоков
                  CPS.ConvertBtr();

                  // Преобразования торцов фасадов
                  CPS.ConvertEnds();

                  t.Commit();
               }
            }
            CPS.Purge();

            dbExport.SaveAs(_fileExport.FileExportFacade.FullName, DwgVersion.Current);
         }
      }

      private void copyPanelToExportFile(Database dbExport)
      {
         Dictionary<ObjectId, PanelBlRefExport> dictPanelsToExport = new Dictionary<ObjectId, PanelBlRefExport>();

         foreach (var panelBtr in CPS.PanelsBtrExport)
            foreach (var panelBlref in panelBtr.Panels)
               dictPanelsToExport.Add(panelBlref.IdBlRefAkr, panelBlref);

         ObjectIdCollection ids = new ObjectIdCollection(dictPanelsToExport.Keys.ToArray());

         using (IdMapping map = new IdMapping())
         {
            var msExport = SymbolUtilityServices.GetBlockModelSpaceId(dbExport);
            dbExport.WblockCloneObjects(ids, msExport, map, DuplicateRecordCloning.Replace, false);

            // скопированные блоки в экспортированном чертеже
            var idsBtrExport = new HashSet<ObjectId>();
            foreach (var itemDict in dictPanelsToExport)
            {
               itemDict.Value.IdBlRefExport = map[itemDict.Key].Value;
               using (var blRef = itemDict.Value.IdBlRefExport.Open(OpenMode.ForRead, false, true) as BlockReference)
               {
                  idsBtrExport.Add(blRef.BlockTableRecord);
                  itemDict.Value.PanelBtrExport.IdBtrExport = blRef.BlockTableRecord;
               }
            }
         }
      }

      private void deleteBlRefs(List<ObjectId> idsBlRef)
      {
         foreach (ObjectId idBlRefPanel in idsBlRef)
         {
            using (var blRefPanel = idBlRefPanel.Open(OpenMode.ForWrite, false, true) as BlockReference)
            {
               blRefPanel.Erase();
            }
         }
      }

      private void deletePanels(Database dbExport)
      {
         // Удаление блоков панелей из существующего чертежа экпорта фасадов
         SelectionBlocks selPanels = new SelectionBlocks(dbExport);
         selPanels.SelectBlRefsInModel(false);
         deleteBlRefs(selPanels.IdsBlRefPanelAr);
         // Панелей СБ не должно быть, но на всякий случай удалю.
         deleteBlRefs(selPanels.IdsBlRefPanelSb);
      }

      private void redefineBlockTile(Database dbExport)
      {
         // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRExportFacadeFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(Settings.Default.BlockTileName, fileBlocksTemplate,
                              dbExport, DuplicateRecordCloning.Replace);
            }
            catch
            {
            }
         }
      }

      private void warningMessageBusyFileExportFacade(Exception ex, string file)
      {
         // Предупреждение, что файл занят или сообщение об исключении
         string message = string.Empty;
         var whoHas = Autodesk.AutoCAD.ApplicationServices.Application.GetWhoHasInfo(file);
         if (whoHas.IsFileLocked)
         {
            message = string.Format("Файл занят. Повторите позже.\n" +
                                    "{0}\n" +
                                    "Кем занято: {1}, время {2}",
                                    file, whoHas.UserName, whoHas.OpenTime);
         }
         else
         {
            // Другая ошибка при открытии файла
            message = string.Format("Ошибка открытия файла.\n" +
                                    "{0}\n" +
                                    "Ошибка: {1}", file, ex.Message);
         }
         System.Windows.MessageBox.Show(message, "Экспорт фасада", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
      }
   }
}
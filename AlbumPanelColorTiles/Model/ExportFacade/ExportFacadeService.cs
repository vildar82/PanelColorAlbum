using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Select;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   // Экспорт фасада
   public class ExportFacadeService
   {
      private SelectionBlocks _selectPanels;
      private FileExport _fileExport;
      private List<ObjectId> _idsBtrPanelArExport;
      private List<ObjectId> _idsBlRefPanelArExport;

      /// <summary>
      /// Экспорт фасада для АР
      /// </summary>
      public void Export()
      {
         // Список панелей для экспорта
         _selectPanels = new SelectionBlocks();
         _selectPanels.SelectAKRPanelsBlRefInModel();
         if (_selectPanels.IdsBlRefPanelSb.Count>0)
         {
            throw new Exception("В текущем чертеже в Модели не должно быть панелей Марки СБ (только Марки АР).");
         }
         if (_selectPanels.IdsBlRefPanelAr.Count == 0            )
         {
            throw new Exception("Не найдены панели Марки АР в Моделе текущего чертежа.");
         }

         // Определить файл в который экспортировать фасад
         _fileExport = new FileExport();
         _fileExport.DefineFile();
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
                  WarningMessageBusyFileExportFacade(ex, _fileExport.FileExportFacade.FullName);
                  throw;
               }
               // сделать копию файла
               _fileExport.Backup();
               dbExport.CloseInput(true);
               deletePanels(dbExport);
            }           

            // Копирование панелей АР в экспортный файл
            copyPanelToExportFile(dbExport);            

            // Преобразования блоков
            ConversionExportFacade conversion = new ConversionExportFacade(dbExport, _idsBtrPanelArExport);
            conversion.Convert();            

            dbExport.SaveAs(_fileExport.FileExportFacade.FullName, DwgVersion.Current);
         }         
      }

      private void WarningMessageBusyFileExportFacade(Exception ex, string file)
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
         System.Windows.MessageBox.Show(message, "Экспорт фасада",  System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
      }

      private void deletePanels(Database dbExport)
      {
         SelectionBlocks selPanels = new SelectionBlocks(dbExport);
         selPanels.SelectAKRPanelsBlRefInModel();
         deleteBlRefs(selPanels.IdsBlRefPanelAr);
         // Панелей СБ не должно быть, но на всякий случай удалю.
         deleteBlRefs(selPanels.IdsBlRefPanelSb);
      }

      private void copyPanelToExportFile(Database dbExport)
      {         
         ObjectIdCollection ids = new ObjectIdCollection(_selectPanels.IdsBlRefPanelAr.ToArray());
         IdMapping map = new IdMapping();
         var msExport = SymbolUtilityServices.GetBlockModelSpaceId(dbExport);
         dbExport.WblockCloneObjects(ids, msExport, map, DuplicateRecordCloning.Replace, false);

         // скопированные блоки в экспортированном чертеже
         _idsBlRefPanelArExport = new List<ObjectId>();
         var idsBtrExport = new HashSet<ObjectId>();
         foreach (var idblRefFacade in _selectPanels.IdsBlRefPanelAr)
         {
            var idBlRefExport = map[idblRefFacade].Value;
            _idsBlRefPanelArExport.Add(idBlRefExport);
            using (var blRef = idBlRefExport.Open( OpenMode.ForRead, false, true) as BlockReference)
            {
               idsBtrExport.Add(blRef.BlockTableRecord);
            }            
         }
         _idsBtrPanelArExport = idsBtrExport.ToList();
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
   }
}

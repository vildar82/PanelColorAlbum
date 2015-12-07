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
      private SelectionPanel _selectPanels;
      private FileExport _fileExport;      

      /// <summary>
      /// Экспорт фасада для АР
      /// </summary>
      public void Export()
      {
         // Список панелей для экспорта
         _selectPanels = new SelectionPanel();
         _selectPanels.SelectPanelsBlRefInModel();
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
         using (Database dbExport = new Database(false, true))
         {
            if (_fileExport.IsExistsFileExport)
            {
               // удалить старые панели из файла экспорта
               dbExport.ReadDwgFile(_fileExport.FileNameExport, FileShare.Read, true, "");
               dbExport.CloseInput(true);
               deletePanels(dbExport);
            }           

            // Копирование панелей АР в экспортный файл
            copyPanelToExportFile(dbExport);

            // Преобразования блоков
            ConversionExportFacade conversion = new ConversionExportFacade(dbExport);
            conversion.Convert();

            dbExport.Save();
         }         
      }      

      private void deletePanels(Database dbExport)
      {
         SelectionPanel selPanels = new SelectionPanel(dbExport);
         selPanels.SelectPanelsBlRefInModel();
         deleteBlRefs(selPanels.IdsBlRefPanelAr);
         // Панелей СБ не должно быть, но на всякий случай удалю.
         deleteBlRefs(selPanels.IdsBlRefPanelSb);
      }

      private void copyPanelToExportFile(Database dbExport)
      {
         ObjectIdCollection ids = new ObjectIdCollection(_selectPanels.IdsBlRefPanelAr.ToArray());
         IdMapping map = new IdMapping();
         dbExport.WblockCloneObjects(ids, SymbolUtilityServices.GetBlockModelSpaceId(dbExport), map, DuplicateRecordCloning.Replace, false);
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

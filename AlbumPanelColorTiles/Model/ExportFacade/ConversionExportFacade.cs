using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Select;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   /// <summary>
   /// Преобразование экспортированных фасадов
   /// </summary>
   public class ConversionExportFacade
   {
      private Database _dbExport;      
      private List<ObjectId> _idsBtrPanelArExport;

      public ConversionExportFacade(Database db, List<ObjectId> idsBtrPanelArExport)
      {
         _dbExport = db;
         _idsBtrPanelArExport = idsBtrPanelArExport;
      }

      // Преобразование экспортированных фасадов
      public void Convert ()
      {
         // обработка блоков панелей         
         ConvertPanelService convertPanel = new ConvertPanelService(_idsBtrPanelArExport);
         using (AcadLib.WorkingDatabaseSwitcher switchDb = new AcadLib.WorkingDatabaseSwitcher(_dbExport))
         {
            convertPanel.Convert();
            // Очистка чертежа
            purge();
         }
      }

      private void purge()
      {
         ObjectIdGraph graph = new ObjectIdGraph();
         foreach (ObjectId idBtrPanel in _idsBtrPanelArExport)
         {
            ObjectIdGraphNode node = new ObjectIdGraphNode(idBtrPanel);
            graph.AddNode(node);
         }                           
         _dbExport.Purge(graph);
      }
   }
}

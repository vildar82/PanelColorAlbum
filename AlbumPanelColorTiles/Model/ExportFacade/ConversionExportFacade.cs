using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   /// <summary>
   /// Преобразование экспортированных фасадов
   /// </summary>
   public class ConversionExportFacade
   {
      private Database _dbExport;

      public ConversionExportFacade(Database db)
      {
         _dbExport = db;
      }

      // Преобразование экспортированных фасадов
      public void Convert ()
      {

      }
   }
}

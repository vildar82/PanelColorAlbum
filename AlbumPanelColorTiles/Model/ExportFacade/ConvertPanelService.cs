using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertPanelService
   {
      private List<ObjectId> _idsBtrPanelArExport;
      private List<ConvertPanelBtr> _convertedBtr;
      
      public ConvertPanelService(List<ObjectId> idsBtrPanelArExport)
      {
         _idsBtrPanelArExport = idsBtrPanelArExport;
      }

      // Преобразование фасадов
      public void Convert()
      {
         if (_idsBtrPanelArExport.Count == 0)         
            return;

         // Преобразования блоков панелей
         _convertedBtr = new List<ConvertPanelBtr>();
         foreach (var idBtr in _idsBtrPanelArExport)
         {
            ConvertPanelBtr convBtr = new ConvertPanelBtr(idBtr);
            try
            {
               convBtr.Convert();
               _convertedBtr.Add(convBtr);
            }
            catch (Exception ex)
            {
               Log.Error(ex, "Ошибка конвертиации экспортрированного блока панели");
            }            
         }         
      } 
   }
}

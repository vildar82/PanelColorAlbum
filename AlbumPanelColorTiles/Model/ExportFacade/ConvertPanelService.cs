using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertPanelService
   {
      private List<ObjectId> _idsBtrPanelArExport;
      private List<ConvertPanelBtr> _convertedBtr;

      // Слой для контура панелей
      private ObjectId _idLayerContour;
      public ObjectId IdLayerContour
      {
         get
         {
            if (_idLayerContour.IsNull)
            {
               // Создание контура плитки
               var layer = new AcadLib.Layers.LayerInfo("АР_Швы");
               layer.LineWeight = LineWeight.LineWeight030;
               _idLayerContour = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layer);
            }
            return _idLayerContour;
         }
      }

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
            ConvertPanelBtr convBtr = new ConvertPanelBtr(this, idBtr);
            try
            {
               convBtr.Convert();
               _convertedBtr.Add(convBtr);
            }
            catch (Exception ex)
            {
               Inspector.AddError("Ошибка конвертации блока панели - {0}", ex.Message);
               Log.Error(ex, "Ошибка конвертиации экспортрированного блока панели");
            }            
         }         
      } 
   }
}

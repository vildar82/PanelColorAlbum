using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.Layers;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base
{
   // Вспомогательные данные для создания блоков панелей - слои, стили, блоки и т.п.
   public class CreatePanelsBtrEnvironment
   {
      private BaseService _service;

      // Layers
      public ObjectId IdLayerContourPanel { get; private set; }
      public ObjectId IdLayerWindow { get; private set; }
      public ObjectId IdLayerDimFacade { get; private set; }
      public ObjectId IdLayerDimForm { get; private set; }
      // Blocks
      public ObjectId IdBtrTile { get; private set; }
      public ObjectId IdBtrWindow { get; private set; }

      // DimStyle
      public ObjectId IdDimStyle { get; private set; }

      public CreatePanelsBtrEnvironment(BaseService service)
      {
         _service = service;         
      }

      public void LoadAllEnv()
      {
         // Определение слоев
         loadLayers();
         // Определение блоков
         loadBtr();
         // Размерный Стиль
         IdDimStyle = _service.Db.GetDimStylePIK();
      }     

      private void loadLayers()
      {
         // Слой контур панели
         IdLayerContourPanel = ExportFacade.ContourPanel.CreateLayerContourPanel();
         // Слой Окна
         var layer = new LayerInfo(Settings.Default.LayerWindows);
         IdLayerWindow = LayerExt.GetLayerOrCreateNew(layer);
         // Слой размеров на фасаде
         layer = new LayerInfo(Settings.Default.LayerWindows);
         IdLayerDimFacade = LayerExt.GetLayerOrCreateNew(layer);
         // Слой размеров в форме
         layer = new LayerInfo(Settings.Default.LayerDimensionForm);
         IdLayerDimForm = LayerExt.GetLayerOrCreateNew(layer);
      }

      private void loadBtr()
      {
         // Имена блоков для копирования из шаблона
         List<string> blNamesToCopy = new List<string> { Settings.Default.BlockTileName, Settings.Default.BlockWindowName };
         // Копирование блоков
         var blocksCopyed = defineBlockFromTemplate(blNamesToCopy);

         // Определение Блок Плитки
         ObjectId idBtrTile;
         if (blocksCopyed.TryGetValue(Settings.Default.BlockTileName, out idBtrTile))
         {
            IdBtrTile = idBtrTile;
         }
         else
         {
            Inspector.AddError($"Не определен блок плитки {Settings.Default.BlockTileName}");
         }

         // Определение Блок Окна
         ObjectId idBtrWin;
         if (blocksCopyed.TryGetValue(Settings.Default.BlockWindowName, out idBtrWin))
         {
            IdBtrWindow = idBtrWin;
         }
         else
         {
            Inspector.AddError($"Не определен блок плитки {Settings.Default.BlockWindowName}");
         }
      }

      private Dictionary<string, ObjectId> defineBlockFromTemplate(List<string> blNames)
      {       
         // Переопределение блока плитки из файла шаблона блоков
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               return AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(blNames, fileBlocksTemplate,
                              _service.Db, DuplicateRecordCloning.Replace);
            }
            catch (Exception ex)
            {
               Log.Error(ex, "AcadLib.Blocks.Block.CopyBlockFromExternalDrawing");
            }
         }
         else
         {
            Inspector.AddError($"Не найден файл шаблона блоков АКР - {fileBlocksTemplate}");            
         }
         return new Dictionary<string, ObjectId>();
      }
   }
}

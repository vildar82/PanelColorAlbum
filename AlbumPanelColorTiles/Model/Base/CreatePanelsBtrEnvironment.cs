using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Layers;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base
{
   // Вспомогательные данные для создания блоков панелей - слои, стили, блоки и т.п.
   public class CreatePanelsBtrEnvironment
   {
      private BaseService _service;
      private ObjectId _idLayerContourPanel;
      private ObjectId _idBtrTile;
      private ObjectId _idDimStyle;
      private ObjectId _idLayerDimFacade;
      private ObjectId _idLayerDimForm;

      public CreatePanelsBtrEnvironment(BaseService service)
      {
         _service = service;
      }

      public ObjectId IdLayerContourPanel
      {
         get
         {
            if (_idLayerContourPanel.IsNull)
            {
               _idLayerContourPanel = ExportFacade.ContourPanel.CreateLayerContourPanel();
            }
            return _idLayerContourPanel;
         }
      }

      public ObjectId IdLayerDimFacade
      {
         get
         {
            if (_idLayerDimFacade.IsNull)
            {
               var layInfo = new LayerInfo(Settings.Default.LayerDimensionFacade);
               _idLayerDimFacade = LayerExt.GetLayerOrCreateNew(layInfo);
            }
            return _idLayerDimFacade;
         }
      }

      public ObjectId IdLayerDimForm
      {
         get
         {
            if (_idLayerDimForm.IsNull)
            {
               var layInfo = new LayerInfo(Settings.Default.LayerDimensionForm);
               _idLayerDimForm = LayerExt.GetLayerOrCreateNew(layInfo);
            }
            return _idLayerDimForm;
         }
      }

      public ObjectId IdBtrTile
      {
         get
         {
            if (_idBtrTile.IsNull)
            {
               _idBtrTile = defineBlockTile(_service.Db);
            }
            return _idBtrTile;
         }
      }

      public ObjectId IdDimStyle
      {
         get
         {
            if (_idDimStyle.IsNull)
            {
               _idDimStyle = _service.Db.GetDimStylePIK();
            }
            return _idDimStyle;
         }
      }

      private ObjectId defineBlockTile(Database dbDest)
      {
         ObjectId resIdBtrTile = ObjectId.Null;
         // Переопределение блока плитки из файла шаблона блоков
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               resIdBtrTile =AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(Settings.Default.BlockTileName, fileBlocksTemplate,
                              dbDest, DuplicateRecordCloning.Replace);
            }
            catch (Exception ex)
            {
               Log.Error(ex, "AcadLib.Blocks.Block.CopyBlockFromExternalDrawing");
            }
         }
        return resIdBtrTile;
      }
   }
}

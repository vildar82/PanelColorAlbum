using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base
{
   // Вспомогательные данные для создания блоков панелей - слои, стили, блоки и т.п.
   public class CreatePanelsBtrEnvironment
   {
      private ObjectId _idLayerContourPanel;
      private ObjectId _idBtrTile;

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

      public ObjectId IdBtrTile
      {
         get
         {
            if (_idBtrTile.IsNull)
            {
               _idBtrTile = defineBlockTile(HostApplicationServices.WorkingDatabase);
            }
            return _idBtrTile;
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

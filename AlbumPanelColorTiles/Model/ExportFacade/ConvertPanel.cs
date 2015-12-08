﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertPanel
   {
      private List<ObjectId> _idsBtrPanelArExport;
      
      public ConvertPanel(List<ObjectId> idsBtrPanelArExport)
      {
         _idsBtrPanelArExport = idsBtrPanelArExport;
      }

      public void Convert()
      {
         if (_idsBtrPanelArExport.Count == 0)         
            return;

         foreach (var idBtr in _idsBtrPanelArExport)
         {
            convertBtr(idBtr);
         }         
      }

      private void convertBtr(ObjectId idBtr)
      {
         using (var btr = idBtr.Open(OpenMode.ForWrite) as BlockTableRecord)
         {
            // Удаление лишнего из блока панели
            deleteWaste(btr);

            // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
            redefineBlockTile();

            // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
         }
      }      

      private void deleteWaste(BlockTableRecord btr)
      {
         foreach (ObjectId idEnt in btr)
         {
            using (var ent = idEnt.Open(  OpenMode.ForRead) as Entity)
            {
               if (string.Equals(ent.Layer, Settings.Default.LayerDimensionFacade) &&
                  string.Equals(ent.Layer, Settings.Default.LayerDimensionForm))
               {
                  ent.UpgradeOpen();
                  ent.Erase();
               }
            }
         }
      }

      // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
      private void redefineBlockTile()
      {
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRExportFacadeFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {            
            AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(Settings.Default.BlockTileName, fileBlocksTemplate,
                           _idsBtrPanelArExport[0].Database, DuplicateRecordCloning.Replace);
            }
            catch
            {               
            }
         }         
      }
   }
}
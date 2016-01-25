using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.Layers;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   // Вспомогательные данные для создания блоков панелей - слои, стили, блоки и т.п.
   public class CreatePanelsBtrEnvironment
   {
      private BaseService _service;
      private Dictionary<string, ObjectId> _blocks;

      // Layers
      public ObjectId IdLayerContourPanel { get; private set; }
      public ObjectId IdLayerWindow { get; private set; }
      public ObjectId IdLayerDimFacade { get; private set; }
      public ObjectId IdLayerDimForm { get; private set; }
      // Blocks
      public ObjectId IdBtrTile { get; private set; }
      public ObjectId IdBtrWindow { get; private set; }
      public List<string> WindowMarks { get; private set; }
      public ObjectId IdBtrView { get; private set; }
      public ObjectId IdAttrDefView { get; private set; }
      public ObjectId IdBtrCross { get; private set; }
      public ObjectId IdAttrDefCross { get; private set; }
      public ObjectId IdBtrWindowHorSection { get; private set; }
      public List<BlockSectionAbstract> BlPanelSections { get; private set; }

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

         // Отключение контура маскировки
         try
         {
            Application.SetSystemVariable("FRAME", 0);
         }
         catch { }
      }     

      private void loadLayers()
      {
         // Слой контур панели
         IdLayerContourPanel = ExportFacade.ContourPanel.CreateLayerContourPanel();
         // Слой Окна
         var layer = new LayerInfo(Settings.Default.LayerWindows);
         IdLayerWindow = LayerExt.GetLayerOrCreateNew(layer);
         // Слой размеров на фасаде
         layer = new LayerInfo(Settings.Default.LayerDimensionFacade);
         layer.Color = Color.FromColorIndex(ColorMethod.ByLayer, 192);
         IdLayerDimFacade = LayerExt.GetLayerOrCreateNew(layer);
         // Слой размеров в форме
         layer = new LayerInfo(Settings.Default.LayerDimensionForm);
         layer.Color = Color.FromColorIndex(ColorMethod.ByLayer, 63);
         IdLayerDimForm = LayerExt.GetLayerOrCreateNew(layer);
      }

      private void loadBtr()
      {
         List<string> blNamesToCopy = new List<string> {
                     Settings.Default.BlockTileName, Settings.Default.BlockWindowName,
                     Settings.Default.BlockViewName, Settings.Default.BlockCrossName,
                     Settings.Default.BlockWindowHorSection
         };         
         // Копирование блоков
         _blocks = defineBlockFromTemplate(blNamesToCopy);

         // Блок Плитки
         IdBtrTile = GetIdBtrLoaded( Settings.Default.BlockTileName);
         // Блок Окна
         IdBtrWindow = GetIdBtrLoaded( Settings.Default.BlockWindowName);
         WindowMarks = getWindowMarks(IdBtrWindow);
         // Блок Вида
         IdBtrView = GetIdBtrLoaded( Settings.Default.BlockViewName);
         // Атрибут блока вида
         IdAttrDefView = getAttrDef(IdBtrView, "ВИД");
         // Блок Разреза
         IdBtrCross = GetIdBtrLoaded(Settings.Default.BlockCrossName);
         // Атрибут блока разреза
         IdAttrDefCross = getAttrDef(IdBtrCross, "НОМЕР");
         // Блок окна для горизонтального сечения
         IdBtrWindowHorSection = GetIdBtrLoaded(Settings.Default.BlockWindowHorSection);         
      }     

      private ObjectId getAttrDef(ObjectId idBtr, string tag)
      {
         ObjectId idAttrDef = ObjectId.Null;
         if (!idBtr.IsNull)
         {
            using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
            {
               foreach (ObjectId idEnt in btr)
               {
                  if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
                  {
                     using (var attrDef = idEnt.Open(OpenMode.ForRead, false, true) as AttributeDefinition)
                     {
                        if (!attrDef.Constant && attrDef.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
                        {
                           idAttrDef = idEnt;
                           break;
                        }
                     }
                  }
               }
            }
         }
         return idAttrDef;
      }

      public ObjectId GetIdBtrLoaded(string blName)
      {
         ObjectId idBtr;
         if (!_blocks.TryGetValue(blName, out idBtr))
         {
            idBtr = ObjectId.Null;
            Inspector.AddError($"Не определен блок {blName}");
         }
         return idBtr;
      }

      private Dictionary<string, ObjectId> defineBlockFromTemplate(List<string> blNames)
      {       
         // Переопределение блока плитки из файла шаблона блоков
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               // копирование сечений панелей
               BlPanelSections = BlockSectionAbstract.LoadSections(fileBlocksTemplate, _service);
               // копирование остальных блоков
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

      private List<string> getWindowMarks(ObjectId idBtrWindow)
      {
         List<string> marks = new List<string>();
         var blRef = new BlockReference(Point3d.Origin, idBtrWindow);
         var dynParams = blRef.DynamicBlockReferencePropertyCollection;
         if (dynParams != null)
         {
            foreach (DynamicBlockReferenceProperty param in dynParams)
            {
               if (param.PropertyName == "Видимость")
               {
                  marks = param.GetAllowedValues().Cast<string>().ToList();
                  break;
               }
            }
         }
         return marks;
      }
   }
}

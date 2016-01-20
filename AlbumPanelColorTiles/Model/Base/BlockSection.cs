using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockSection
   {
      public ObjectId IdBtr { get; private set; }
      public string BlName { get; private set; }
      public int Length { get; private set; }
      public int Thickness { get; private set; }  
      public bool Window { get; private set; }
      
      public BlockSection (string blName, ObjectId idBtr)
      {
         IdBtr = idBtr;
         BlName = blName;         
      }

      public static List<BlockSection> LoadSections(string fileBlocksTemplate, BaseService service)
      {
         List<BlockSection> blSections = new List<BlockSection>();
         using (var dbBlocks = new Database(false, true))
         {
            dbBlocks.ReadDwgFile(fileBlocksTemplate, FileOpenMode.OpenForReadAndAllShare, false, "");
            dbBlocks.CloseInput(true);
            using (var bt = dbBlocks.BlockTableId.Open( OpenMode.ForRead)as BlockTable)
            {
               Dictionary<ObjectId, string> blSecVerticToCopy = new Dictionary<ObjectId, string>();
               foreach (ObjectId idBtr in bt)
               {
                  using (var btr = idBtr.Open( OpenMode.ForRead)as BlockTableRecord)
                  {
                     if (btr.Name.StartsWith(Settings.Default.BlockPanelSectionVerticalPrefixName, StringComparison.OrdinalIgnoreCase)) 
                     {
                        blSecVerticToCopy.Add(idBtr, btr.Name);
                     }
                  }
               }
               if (blSecVerticToCopy.Count>0)
               {
                  ObjectIdCollection ids = new ObjectIdCollection(blSecVerticToCopy.Keys.ToArray());
                  IdMapping map = new IdMapping();
                  dbBlocks.WblockCloneObjects(ids, service.Db.BlockTableId, map,  DuplicateRecordCloning.Replace, false);
                  foreach (var item in blSecVerticToCopy)
                  {
                     BlockSection blSec = new BlockSection(item.Value, map[item.Key].Value);
                     var resParse = blSec.ParseBlName();
                     if (resParse.Success)
                     {
                        blSections.Add(blSec);
                     }
                     else
                     {
                        Inspector.AddError($"Не определены параметры блока сечения в файле шаблона блоков. {blSecVerticToCopy[item.Key]}. {resParse.Error}");
                     }                     
                  }
               }
               else
               {
                  Inspector.AddError("Не найдены блоки сечений панелей в файле шаблона блоков АКР. " + 
                     $"Файл шаблона {fileBlocksTemplate}. Префикс блоков {Settings.Default.BlockPanelSectionVerticalPrefixName}");
               }
            }
         }
         return blSections;
      }

      public Result ParseBlName()
      {
         var ending = BlName.Substring(Settings.Default.BlockPanelSectionVerticalPrefixName.Length);
         if (string.IsNullOrEmpty(ending))
         {
            return Result.Fail("В имени блока не найдены параметры высоты, толщины и наличия окна");
            //throw new Exception("В имени блока не найдены параметры высоты, толщины и наличия окна");
         }
         var options = ending.ToLower().Split('_');
         foreach (var opt in options)
         {
            if (string.IsNullOrWhiteSpace(opt))
            {
               continue;
            }
            switch (opt.First())
            {
               case 'h':
                  Length = getValue(opt.Substring(1));
                  break;
               case 't':
                  Thickness = getValue(opt.Substring(1));
                  break;
               case 'w':
                  Window = true;
                  break;
               default:
                  break;
            }
         }
         string err = string.Empty;
         if (Length <=0)
         {
            err += "Не определена длина панели для блока сечения.";
         }
         if (Thickness<=0)
         {
            err += "Не определена ширина панели для блока сечения.";
         }
         if (!string.IsNullOrEmpty(err))
         {
            return Result.Fail(err);
            //throw new Exception(err);
         }
         return Result.Ok();
      }

      private int getValue(string v)
      {
         int res;
         int.TryParse(v, out res);
         return res;
      }
   }
}

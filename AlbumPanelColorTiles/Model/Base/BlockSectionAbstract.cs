﻿using System;
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
   /// <summary>
   /// Блоки сечений панелей
   /// </summary>
   public abstract class BlockSectionAbstract
   {
      public ObjectId IdBtr { get; set; }
      public string BlName { get; private set; }
      public BaseService Service { get; set; }
      /// <summary>
      /// Кол слоев панели - Однослойная, Трехслойная
      /// </summary>
      public int NLayerPanel {get; set;}
      /// <summary>
      /// Это сечение для чердачной панели
      /// </summary>
      public bool IsUpperStoreyPanel { get; set; }

      //private Dictionary<ObjectId, HatchInfo> _hatchsAssociatedIdsDictInTemplate;
      //private Dictionary<ObjectId, HatchInfo> _hatchsAssociatedIdsDictInFacade;

      public BlockSectionAbstract(string blName, BaseService service)
      {
         BlName = blName;
         Service = service;
      }

      public abstract Result ParseBlName();

      public static List<BlockSectionAbstract> LoadSections(string fileBlocksTemplate, BaseService service)
      {
         Dictionary<ObjectId, BlockSectionAbstract> blSecToCopy = new Dictionary<ObjectId, BlockSectionAbstract>();         
         using (var dbBlocks = new Database(false, true))
         {
            dbBlocks.ReadDwgFile(fileBlocksTemplate, FileOpenMode.OpenForReadAndAllShare, false, "");
            dbBlocks.CloseInput(true);
            using (var bt = dbBlocks.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
            {
               foreach (ObjectId idBtr in bt)
               {
                  using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
                  {                     
                     BlockSectionAbstract blSec = null;
                     if (btr.Name.StartsWith(Settings.Default.BlockPanelSectionVerticalPrefixName, StringComparison.OrdinalIgnoreCase))
                     {
                        blSec = new BlockSectionVertical(btr.Name, service);
                     }
                     else if (btr.Name.StartsWith(Settings.Default.BlockPanelSectionHorizontalPrefixName, StringComparison.OrdinalIgnoreCase))
                     {
                        blSec = new BlockSectionHorizontal(btr.Name, service);
                     }

                     if (blSec != null)
                     {
                        var resParse = blSec.ParseBlName();
                        if (resParse.Success)
                        {
                           blSecToCopy.Add(idBtr, blSec);
                           //Определение ассоциативных штриховок и объектов с которыми они связаны
                           //var hatchAssDict = getHatchAssociateIds(btr);
                           //if (hatchAssDict.Count > 0)
                           //{
                           //   blSec._hatchsAssociatedIdsDictInTemplate = hatchAssDict;
                           //}
                        }
                        else
                        {
                           Inspector.AddError($"Не определены параметры блока сечения в файле шаблона блоков. {btr.Name}. {resParse.Error}");
                        }
                     }
                  }
               }
            }
            if (blSecToCopy.Count > 0)
            {
               ObjectIdCollection ids = new ObjectIdCollection(blSecToCopy.Keys.ToArray());
               IdMapping map = new IdMapping();
               dbBlocks.WblockCloneObjects(ids, service.Db.BlockTableId, map, DuplicateRecordCloning.Replace, false);
               foreach (var item in blSecToCopy)
               {
                  item.Value.IdBtr = map[item.Key].Value;
                  //if (item.Value._hatchsAssociatedIdsDictInTemplate != null)
                  //{
                  //   item.Value.SetHatchIdsMapping(map);
                  //   //item.Value.ReplaceAssociateHatch();
                  //}
               }               
            }
            else
            {
               Inspector.AddError("Не найдены блоки сечений панелей в файле шаблона блоков АКР. " +
                  $"Файл шаблона {fileBlocksTemplate}. Префикс блоков {Settings.Default.BlockPanelSectionVerticalPrefixName} " +
                  $"и {Settings.Default.BlockPanelSectionHorizontalPrefixName}");
            }
         }
         return blSecToCopy.Values.ToList();         
      }     

      //private static Dictionary<ObjectId, HatchInfo> getHatchAssociateIds(BlockTableRecord btr)
      //{
      //   Dictionary<ObjectId, HatchInfo> idsHatchAssociates = new Dictionary<ObjectId, HatchInfo>();
      //   foreach (ObjectId idEnt in btr)
      //   {
      //      if (idEnt.ObjectClass.Name == "AcDbHatch")
      //      {
      //         using (var h = idEnt.Open(OpenMode.ForRead, false, true) as Hatch)
      //         {
      //            if (h.Associative)
      //            {                     
      //               idsHatchAssociates.Add(h.Id, new HatchInfo(h));
      //            }
      //         }
      //      }
      //   }
      //   return idsHatchAssociates;
      //}

      //private void SetHatchIdsMapping(IdMapping map)
      //{
      //   _hatchsAssociatedIdsDictInFacade = new Dictionary<ObjectId, HatchInfo>();
      //   foreach (var itemDict in _hatchsAssociatedIdsDictInTemplate)
      //   {
      //      ObjectIdCollection idsAssFacade = new ObjectIdCollection();
      //      foreach (ObjectId idVal in itemDict.Value.IdsAssociate)
      //      {
      //         idsAssFacade.Add(map[idVal].Value);
      //      }
      //      itemDict.Value.IdsAssociate = idsAssFacade;
      //      itemDict.Value.Id = map[itemDict.Key].Value;
      //      _hatchsAssociatedIdsDictInFacade.Add(itemDict.Value.Id, itemDict.Value);
      //   }
      //}

      //public  void ReplaceAssociateHatch()
      //{
      //   if (_hatchsAssociatedIdsDictInFacade != null)
      //   {
      //      using (var btr = IdBtr.GetObject(OpenMode.ForWrite) as BlockTableRecord)
      //      {
      //         foreach (var itemDict in _hatchsAssociatedIdsDictInFacade)
      //         {
      //            using (var hErase = itemDict.Key.GetObject(OpenMode.ForWrite, false, true) as Hatch)
      //            {
      //               hErase.Erase();
      //            }                     
      //            itemDict.Value.CreateNewHatch(btr);
      //         }
      //         btr.UpdateAnonymousBlocks();
      //      }
      //   }
      //}
   }
}

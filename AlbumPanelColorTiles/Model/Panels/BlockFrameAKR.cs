using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Properties;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AlbumPanelColorTiles.Panels
{
   // Блок рамки со штампом
   public class BlockFrameAKR
   {
      private Dictionary<string, string> _attrs;
      private string _blFrameName;
      private Database _db;
      private ObjectId _idBtrFrame;
      private bool _isFound;

      public bool IsFound { get { return _isFound; } }

      public void ChangeBlockFrame(Database db, string blName)
      {
         // Замена блока рамки если он есть в чертеже фасада
         if (IsFound)
         {
            IdMapping iMap = new IdMapping();
            var ids = new ObjectIdCollection();
            ids.Add(_idBtrFrame);
            db.WblockCloneObjects(ids, db.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
            // Запись атрибутов (Наименование, и другие если есть)
            changeBlkRefFrame(db, blName);
         }
      }

      public void Search()
      {
         // Поиск блока рамки на текущем чертеже фасада
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         _db = doc.Database;
         _blFrameName = Settings.Default.BlockFrameName;
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (bt.Has(_blFrameName))
            {
               var btrFrame = t.GetObject(bt[_blFrameName], OpenMode.ForRead) as BlockTableRecord;
               // Проверка блока
               if (checkBtrFrame(btrFrame, t))
               {
                  _idBtrFrame = btrFrame.ObjectId;
                  if (checkBlockRefs(btrFrame, t))
                  {
                     _isFound = true;
                  }
                  else
                  {
                     doc.Editor.WriteMessage("\nНе найдено вхождение блока рамки (АКР_Рамка) в чертеже фасада в Модели.");
                  }
               }
               else
               {
                  doc.Editor.WriteMessage("\nБлок рамки (АКР_Рамка) не соответствует требованиям. У него должны быть атрибуты: Наименование, Вид, Лист.");
               }
            }
            else
            {
               doc.Editor.WriteMessage("\nНет блока рамки (АКР_Рамка) в чертеже фасада, для копирования в чертежи альбома панелей.");
            }
            t.Commit();
         }
      }

      private void changeBlkRefFrame(Database db, string blName)
      {
         // Обновление блока рамки
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            ObjectId idBtrFrame;
            if (bt.Has(Settings.Default.BlockFrameName))
               idBtrFrame = bt[Settings.Default.BlockFrameName];
            else
               return;

            // Обновление геометрии вхождениц блока
            var btrFrame = t.GetObject(idBtrFrame, OpenMode.ForRead) as BlockTableRecord;
            btrFrame.UpdateAnonymousBlocks();

            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (btr.IsLayout && !btr.IsErased && btr.Name != BlockTableRecord.ModelSpace)
               {
                  foreach (ObjectId idEnt in btr)
                  {
                     if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                     {
                        var blRef = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                        if (blRef.GetEffectiveName().Equals(blName))
                        {
                           updateBlRefFrame(blRef, btrFrame, t);
                        }
                     }
                  }
               }
            }
            t.Commit();
         }
      }

      private bool checkBlockRefs(BlockTableRecord btrFrame, Transaction t)
      {
         bool res = false;
         var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(_db), OpenMode.ForRead) as BlockTableRecord;
         foreach (ObjectId idEnt in ms)
         {
            if (idEnt.ObjectClass.Name == "AcDbBlockReference")
            {
               var blRef = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
               if (blRef.GetEffectiveName().Equals(_blFrameName, StringComparison.OrdinalIgnoreCase))
               {
                  // считывание атрибутов
                  var atrCol = blRef.AttributeCollection;
                  _attrs = new Dictionary<string, string>();
                  foreach (ObjectId idAtrRef in atrCol)
                  {
                     var atrRef = t.GetObject(idAtrRef, OpenMode.ForRead) as AttributeReference;
                     string key = atrRef.Tag.ToUpper();
                     if (!_attrs.ContainsKey(key))
                     {
                        _attrs.Add(key, atrRef.TextString);
                     }
                  }
                  res = true;
               }
            }
         }
         return res;
      }

      private bool checkBtrFrame(BlockTableRecord btrFrame, Transaction t)
      {
         bool res = false;
         if (btrFrame.HasAttributeDefinitions)
         {
            Dictionary<string, string> attrsChecks = new Dictionary<string, string>();
            foreach (ObjectId idEnt in btrFrame)
            {
               if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
               {
                  var atrDef = t.GetObject(idEnt, OpenMode.ForRead) as AttributeDefinition;
                  switch (atrDef.Tag.ToUpper())
                  {
                     case "ВИД":
                        attrsChecks.Add("ВИД", atrDef.TextString);
                        break;

                     case "НАИМЕНОВАНИЕ":
                        attrsChecks.Add("НАИМЕНОВАНИЕ", atrDef.TextString);
                        break;

                     case "ЛИСТ":
                        attrsChecks.Add("ЛИСТ", atrDef.TextString);
                        break;

                     default:
                        break;
                  }
               }
            }
            if (attrsChecks.Count == 3)
            {
               res = true;
            }
         }
         return res;
      }

      private void updateBlRefFrame(BlockReference blRef, BlockTableRecord btrFrame, Transaction t)
      {
         // Обновление вхождения блока рамки
         if (!IsFound)
            return;

         // Удаление атрибутов
         foreach (ObjectId idAtrRef in blRef.AttributeCollection)
         {
            var atrRef = t.GetObject(idAtrRef, OpenMode.ForWrite) as AttributeReference;
            atrRef.Erase(true);
         }

         blRef.UpgradeOpen();
         foreach (ObjectId idEnt in btrFrame)
         {
            if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
            {
               var atrDef = t.GetObject(idEnt, OpenMode.ForRead) as AttributeDefinition;
               if (!atrDef.Constant)
               {
                  using (var atrRef = new AttributeReference())
                  {
                     atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
                     string key = atrDef.Tag.ToUpper();
                     atrRef.TextString = _attrs.ContainsKey(key) ? _attrs[key] : atrDef.TextString;
                     blRef.AttributeCollection.AppendAttribute(atrRef);
                     t.AddNewlyCreatedDBObject(atrRef, true);
                  }
               }
            }
         }
      }
   }
}
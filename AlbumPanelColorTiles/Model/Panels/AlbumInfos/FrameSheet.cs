using System;
using System.Collections.Generic;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AlbumPanelColorTiles.Panels
{
   // Блок рамки со штампом
   public class FrameSheet
   {
      private Dictionary<string, string> _attrs;  // key - Tag, value - ObjectId AtrRef.    
      private Database _db;
      private ObjectId _idBtrFrame;      
      public bool IsOk { get; private set; }

      public void Check(ObjectId idBlRefFrame)
      {
         if (idBlRefFrame.IsNull)
         {
            Inspector.AddError("Блок {0} не найден.",
                              Settings.Default.BlockFrameName);
            return;
         }

         using (var blRefFrame = idBlRefFrame.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            _idBtrFrame = blRefFrame.BlockTableRecord;
            if (blRefFrame.AttributeCollection == null)
            {
               Inspector.AddError("Блок {0} не соответствует требованиям. У него должны быть атрибуты: Наименование, Вид, Лист.",
                                 Settings.Default.BlockFrameName);
               return;
            }
            _attrs = new Dictionary<string, string>();
            foreach (ObjectId idAtrRef in blRefFrame.AttributeCollection)
            {
               using (var atrRef = idAtrRef.Open(OpenMode.ForRead, false, true) as AttributeReference)
               {
                  string key = atrRef.Tag.ToUpper();
                  if (!_attrs.ContainsKey(key))
                  {
                     string text;
                     if (atrRef.IsMTextAttribute)
                     {
                        text = atrRef.MTextAttribute.Contents;
                     }
                     else
                     {
                        text = atrRef.TextString;
                     }
                     _attrs.Add(key, text);
                  }
               }
            }

            string errMsg = string.Empty;
            if (!_attrs.ContainsKey("ВИД"))
            {
               errMsg += "Не найден атибут ВИД. ";
            }
            if (!_attrs.ContainsKey("НАИМЕНОВАНИЕ"))
            {
               errMsg += "Не найден атибут НАИМЕНОВАНИЕ. ";
            }
            if (!_attrs.ContainsKey("ЛИСТ"))
            {
               errMsg += "Не найден атибут ЛИСТ. ";
            }
            if (!string.IsNullOrEmpty(errMsg))
            {
               Inspector.AddError($"Ошибки в блоке {Settings.Default.BlockFrameName}: {blRefFrame}");
            }
            else
            {
               IsOk = true;
            }
         }
      }

      public void ChangeBlockFrame(Database db, bool insertDescription)
      {
         // Замена блока рамки если он есть в чертеже фасада
         if (IsOk)
         {
            IdMapping iMap = new IdMapping();
            var ids = new ObjectIdCollection();
            ids.Add(_idBtrFrame);
            db.WblockCloneObjects(ids, db.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
            // Запись атрибутов (Наименование, и другие если есть)
            changeBlkRefFrame(db, Settings.Default.BlockFrameName, insertDescription);
         }
      }

      //public void Search()
      //{
      //   // Поиск блока рамки на текущем чертеже фасада
      //   Document doc = AcAp.DocumentManager.MdiActiveDocument;
      //   _db = doc.Database;
      //   _blFrameName = Settings.Default.BlockFrameName;
      //   using (var t = _db.TransactionManager.StartTransaction())
      //   {
      //      var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
      //      if (bt.Has(_blFrameName))
      //      {
      //         var btrFrame = t.GetObject(bt[_blFrameName], OpenMode.ForRead) as BlockTableRecord;
      //         // Проверка блока
      //         if (checkBtrFrame(btrFrame, t))
      //         {
      //            _idBtrFrame = btrFrame.ObjectId;
      //            if (checkBlockRefs(btrFrame, t))
      //            {
      //               IsOk = true;
      //            }
      //            else
      //            {
      //               doc.Editor.WriteMessage("\nНе найдено вхождение блока рамки (АКР_Рамка) в чертеже фасада в Модели.");
      //            }
      //         }
      //         else
      //         {
      //            doc.Editor.WriteMessage("\nБлок рамки (АКР_Рамка) не соответствует требованиям. У него должны быть атрибуты: Наименование, Вид, Лист.");
      //         }
      //      }
      //      else
      //      {
      //         doc.Editor.WriteMessage("\nНет блока рамки (АКР_Рамка) в чертеже фасада, для копирования в чертежи альбома панелей.");
      //      }
      //      t.Commit();
      //   }
      //}    

      private void changeBlkRefFrame(Database db, string blName, bool insertDescription)
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
                        var blRef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                        if (string.Equals(blRef.GetEffectiveName(),blName, StringComparison.CurrentCultureIgnoreCase))
                        {
                           updateBlRefFrame(blRef, btrFrame, t, insertDescription);
                        }
                     }
                  }
               }
            }
            t.Commit();
         }
      }

      //private bool checkBlockRefs(BlockTableRecord btrFrame, Transaction t)
      //{
      //   bool res = false;
      //   var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(_db), OpenMode.ForRead) as BlockTableRecord;
      //   foreach (ObjectId idEnt in ms)
      //   {
      //      if (idEnt.ObjectClass.Name == "AcDbBlockReference")
      //      {
      //         var blRef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
      //         if (blRef.GetEffectiveName().Equals(_blFrameName, StringComparison.OrdinalIgnoreCase))
      //         {
      //            // считывание атрибутов
      //            var atrCol = blRef.AttributeCollection;
      //            _attrs = new Dictionary<string, string>();
      //            foreach (ObjectId idAtrRef in atrCol)
      //            {
      //               var atrRef = t.GetObject(idAtrRef, OpenMode.ForRead, false, true) as AttributeReference;
      //               string key = atrRef.Tag.ToUpper();
      //               if (!_attrs.ContainsKey(key))
      //               {
      //                  _attrs.Add(key, atrRef.TextString);
      //               }
      //            }
      //            res = true;
      //         }
      //      }
      //   }
      //   return res;
      //}

      //private bool checkBtrFrame(BlockTableRecord btrFrame, Transaction t)
      //{
      //   bool res = false;
      //   if (btrFrame.HasAttributeDefinitions)
      //   {
      //      Dictionary<string, string> attrsChecks = new Dictionary<string, string>();
      //      foreach (ObjectId idEnt in btrFrame)
      //      {
      //         if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
      //         {
      //            var atrDef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as AttributeDefinition;
      //            switch (atrDef.Tag.ToUpper())
      //            {
      //               case "ВИД":
      //                  attrsChecks.Add("ВИД", atrDef.TextString);
      //                  break;

      //               case "НАИМЕНОВАНИЕ":
      //                  attrsChecks.Add("НАИМЕНОВАНИЕ", atrDef.TextString);
      //                  break;

      //               case "ЛИСТ":
      //                  attrsChecks.Add("ЛИСТ", atrDef.TextString);
      //                  break;

      //               default:
      //                  break;
      //            }
      //         }
      //      }
      //      if (attrsChecks.Count == 3)
      //      {
      //         res = true;
      //      }
      //   }
      //   return res;
      //}

      private void updateBlRefFrame(BlockReference blRef, BlockTableRecord btrFrame, Transaction t, bool insertDescription)
      {
         // Обновление вхождения блока рамки
         if (!IsOk)
            return;

         // Удаление атрибутов
         foreach (ObjectId idAtrRef in blRef.AttributeCollection)
         {
            var atrRef = t.GetObject(idAtrRef, OpenMode.ForWrite, false, true) as AttributeReference;
            atrRef.Erase(true);
         }

         blRef.UpgradeOpen();
         foreach (ObjectId idEnt in btrFrame)
         {
            if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
            {
               var atrDef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as AttributeDefinition;
               if (!atrDef.Constant)
               {
                  if (!insertDescription && atrDef.Tag.Equals("ПРИМЕЧАНИЕ", StringComparison.OrdinalIgnoreCase))
                  {
                     continue;
                  }
                  using (var atrRef = new AttributeReference())
                  {
                     atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
                     string key = atrDef.Tag.ToUpper();
                     if (_attrs.ContainsKey(key))
                     {
                        atrRef.TextString = _attrs[key];
                     }
                     else
                     {
                        atrRef.TextString = atrDef.TextString;
                     }                          
                     blRef.AttributeCollection.AppendAttribute(atrRef);
                     t.AddNewlyCreatedDBObject(atrRef, true);
                  }
               }
            }
         }
      }
   }
}
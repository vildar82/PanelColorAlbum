using System;
using System.IO;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Lib
{
   public static class BlockInsert
   {
      /// <summary>
      /// Копирование блока из файла шаблона.
      /// </summary>
      /// <param name="blName">Имя блока</param>
      /// <param name="db">База в которую копировать</param>
      /// <exception cref="Exception">Не найден файл-шаблон с блоками</exception>
      public static ObjectId CopyBlockFromTemplate(string blName, Database db)
      {
         // Копирование определения блока из файла с блоками.
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRFileName);
         if (!File.Exists(fileBlocksTemplate))
         {
            throw new Exception("Не найден файл-шаблон с блоками " + fileBlocksTemplate);
         }
         return AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(blName, fileBlocksTemplate, db, DuplicateRecordCloning.Replace);
      }

      //public static void Insert(string blName)
      //{
      //   Document doc = AcAp.DocumentManager.MdiActiveDocument;
      //   Database db = doc.Database;
      //   Editor ed = doc.Editor;
      //   using (var t = db.TransactionManager.StartTransaction())
      //   {
      //      var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
      //      if (!bt.Has(blName))
      //      {
      //         CopyBlockFromTemplate(blName, db);
      //      }
      //      ObjectId idBlBtr = bt[blName];
      //      Point3d pt = Point3d.Origin;
      //      BlockReference br = new BlockReference(pt, idBlBtr);
      //      BlockInsertJig entJig = new BlockInsertJig(br);

      //      // jig
      //      var pr = ed.Drag(entJig);
      //      if (pr.Status == PromptStatus.OK)
      //      {
      //         var btrBl = t.GetObject(idBlBtr, OpenMode.ForRead) as BlockTableRecord;
      //         var blRef = (BlockReference)entJig.GetEntity();
      //         var spaceBtr = (BlockTableRecord)t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
      //         spaceBtr.AppendEntity(blRef);
      //         t.AddNewlyCreatedDBObject(blRef, true);
      //         if (btrBl.HasAttributeDefinitions)
      //            AddAttributes(blRef, btrBl, t);
      //      }
      //      t.Commit();
      //   }
      //}

      //private static void AddAttributes(BlockReference blRef, BlockTableRecord btrBl, Transaction t)
      //{
      //   foreach (ObjectId idEnt in btrBl)
      //   {
      //      if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
      //      {
      //         var atrDef = t.GetObject(idEnt, OpenMode.ForRead) as AttributeDefinition;
      //         if (!atrDef.Constant)
      //         {
      //            using (var atrRef = new AttributeReference())
      //            {
      //               atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
      //               atrRef.TextString = atrDef.TextString;
      //               blRef.AttributeCollection.AppendAttribute(atrRef);
      //               t.AddNewlyCreatedDBObject(atrRef, true);
      //            }
      //         }
      //      }
      //   }
      //}
   }
}
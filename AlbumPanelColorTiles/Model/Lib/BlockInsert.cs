using System;
using System.IO;
using AlbumPanelColorTiles.Model;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AlbumPanelColorTiles.Lib
{
   public static class BlockInsert
   {
      #region Public Methods

      public static void Insert(string blName)
      {
         Document doc = AcAp.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;
         Editor ed = doc.Editor;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (!bt.Has(blName))
            {
               // Копирование определения блока из файла с блоками.
               string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Album.Options.TemplateBlocksAKRFileName);
               if (!File.Exists(fileBlocksTemplate))
               {
                  throw new Exception("Не найден файл-шаблон с блоками " + fileBlocksTemplate);
               }
               Blocks.CopyBlockFromExternalDrawing(blName, fileBlocksTemplate, db);
            }

            ObjectId idBlBtr = bt[blName];
            Point3d pt = Point3d.Origin;
            BlockReference br = new BlockReference(pt, idBlBtr);
            BlockInsertJig entJig = new BlockInsertJig(br);

            // jig
            var pr = ed.Drag(entJig);
            if (pr.Status == PromptStatus.OK)
            {
               var btrBl = t.GetObject(idBlBtr, OpenMode.ForRead) as BlockTableRecord;
               var blRef = (BlockReference)entJig.GetEntity();
               var spaceBtr = (BlockTableRecord)t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
               spaceBtr.AppendEntity(blRef);
               t.AddNewlyCreatedDBObject(blRef, true);
               if (btrBl.HasAttributeDefinitions)
                  AddAttributes(blRef, btrBl, t);
            }
            t.Commit();
         }
      }

      #endregion Public Methods

      #region Private Methods

      private static void AddAttributes(BlockReference blRef, BlockTableRecord btrBl, Transaction t)
      {
         foreach (ObjectId idEnt in btrBl)
         {
            if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
            {
               var atrDef = t.GetObject(idEnt, OpenMode.ForRead) as AttributeDefinition;
               if (!atrDef.Constant)
               {
                  using (var atrRef = new AttributeReference())
                  {
                     atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
                     atrRef.TextString = atrDef.TextString;
                     blRef.AttributeCollection.AppendAttribute(atrRef);
                     t.AddNewlyCreatedDBObject(atrRef, true);
                  }
               }
            }
         }
      }

      #endregion Private Methods
   }
}
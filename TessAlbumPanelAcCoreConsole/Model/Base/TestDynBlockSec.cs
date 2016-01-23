using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace TessAlbumPanelAcCoreConsole.Model.Base
{
   [TestFixture]
   public class TestDynBlockSec
   {
      [Test()]
      public void TestDynBlSec()
      {
         string testFile = @"c:\temp\test\АКР\Base\Tests\TestDynBlSec.dwg";         

         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            db.CloseInput(true);
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               ObjectId idBtrPanel;
               ObjectId idBtrMs;

               using (var t = db.TransactionManager.StartTransaction())
               {
                  BlockTableRecord ms;
                  BlockTableRecord btrPanel;
                  ObjectId idBtrSec;
                  BlockTableRecord btrDim;

                  using (var bt = db.BlockTableId.GetObject(OpenMode.ForWrite) as BlockTable)
                  {
                     ms = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                     idBtrMs = ms.Id;
                     idBtrSec = bt["Test"];

                     btrPanel = new BlockTableRecord();
                     btrPanel.Name = "Panel";
                     idBtrPanel = bt.Add(btrPanel);
                     t.AddNewlyCreatedDBObject(btrPanel, true);

                     btrDim = new BlockTableRecord();
                     btrDim.Name = "Dim";
                     bt.Add(btrDim);
                     t.AddNewlyCreatedDBObject(btrDim, true);
                  }

                  BlockReference blRefDim = new BlockReference(Point3d.Origin, btrDim.Id);
                  btrPanel.AppendEntity(blRefDim);
                  t.AddNewlyCreatedDBObject(blRefDim, true);

                  BlockReference blRef = new BlockReference(Point3d.Origin, idBtrSec);
                  btrDim.AppendEntity(blRef);
                  t.AddNewlyCreatedDBObject(blRef, true);
                  setDynParam(blRef);

                  t.Commit();
               }
               using (var t = db.TransactionManager.StartTransaction())
               {
                  var ms = idBtrMs.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                  BlockReference blRefPanel = new BlockReference(Point3d.Origin, idBtrPanel);
                  ms.AppendEntity(blRefPanel);
                  t.AddNewlyCreatedDBObject(blRefPanel, true);

                  t.Commit();
               }
            }
            db.SaveAs(testFile, DwgVersion.Current);
         }         
      }

      private void setDynParam(BlockReference blRefSecHor)
      {
         foreach (DynamicBlockReferenceProperty prop in blRefSecHor.DynamicBlockReferencePropertyCollection)
         {
            if (prop.PropertyName == "Длина")
            {
               prop.Value = 6000d;
               return;
            }
         }
      }
   }
}

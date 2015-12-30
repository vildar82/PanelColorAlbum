using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Select;
using Autodesk.AutoCAD.DatabaseServices;
using NUnit.Framework;

namespace TessAlbumPanelAcCoreConsole
{
   [TestFixture, Apartment(ApartmentState.STA)]
   public class Class1
   {
      [Test]
      public void Test1()
      {
         using (var db = new Database(false, true))
         {
            string fileDwgTest = @"c:\temp\test\АКР\TestsAcCoreConsole\Test.dwg";
            db.ReadDwgFile(fileDwgTest, FileOpenMode.OpenForReadAndAllShare,  false, "");
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               SelectionBlocks select = new SelectionBlocks();
               select.SelectSectionBlRefs();
               Assert.AreEqual(select.SectionsBlRefs.Count, 1);
            }
         }
      }
   }
}

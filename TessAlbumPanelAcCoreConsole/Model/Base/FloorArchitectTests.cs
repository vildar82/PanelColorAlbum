using NUnit.Framework;
using AlbumPanelColorTiles.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base.Tests
{
   [TestFixture()]
   public class FloorArchitectTests
   {
      [Test()]
      public void GetAllPlanesTest()
      {
         List<FloorArchitect> floorsAr;
         string testFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада.dwg";

         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            db.CloseInput(true);
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               floorsAr = FloorArchitect.GetAllPlanes(db);               
            }
            db.SaveAs(@"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада-WindowMarks.dwg", DwgVersion.Current);
         }

         Assert.AreEqual(floorsAr.Count, 2);
      }
   }
}
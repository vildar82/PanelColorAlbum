using NUnit.Framework;
using AlbumPanelColorTiles.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;
using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;


[assembly: CommandClass(typeof(AlbumPanelColorTiles.Model.Base.Tests.BaseServiceTests))]

namespace AlbumPanelColorTiles.Model.Base.Tests
{
   [TestFixture()]
   public class BaseServiceTests
   {
      BaseService baseService;

      [OneTimeSetUp]
      public void Init()
      {
         baseService = new BaseService();
         // считывание Xml базы панелей
         baseService.ReadPanelsFromBase();
      }

      [Test(Description ="Считывание баы панелей из XML")]
      public void ReadPanelsFromXmlTest()
      {         
         int expectedCount = baseService.CountPanelsInBase;
         // Кол панелей в базе не должно быть равно нулю.
         Assert.AreNotEqual(expectedCount, 0);
      }

      [Test(Description ="Тест создания нескольких блоков панелей АКР")]
      public void CreateBtrPanelFromBase(
         [Values("3НСг 40.29.32-3", "3НСг 37.29.32-5", "3НСг 72.29.32-5", "3НСг 75.29.32-10БП3", "3НСг 73.29.32-4Б", 
                 "3НСг 75.29.32-27П3", "3НСНг 30.29.42-5")] string mark)
         {
         // Тест создания определения блока панели по описанию в xml базе.                  
         PanelBase panelBase;

         string testFile = @"c:\temp\test\АКР\Base\Tests\CreateBlockPanelTest\" + mark + ".dwg";
         File.Copy(@"c:\Autodesk\AutoCAD\Pik\Settings\Template\АР\АР.dwt", testFile, true);

         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            db.CloseInput(true);
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               baseService.InitToCreationPanels(db);
               using (var t = db.TransactionManager.StartTransaction())
               {                  
                  Panel panelXml = baseService.GetPanelXml(mark);
                  panelBase = new PanelBase(panelXml, baseService);
                  panelBase.CreateBlock();

                  if (!panelBase.IdBtrPanel.IsNull)
                  {
                     var blRefPanel = new BlockReference(Point3d.Origin, panelBase.IdBtrPanel);
                     var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                     ms.AppendEntity(blRefPanel);
                     t.AddNewlyCreatedDBObject(blRefPanel, true);
                  }
                  t.Commit();
               }
            }
            db.SaveAs(testFile, DwgVersion.Current);
         }
         Assert.AreNotEqual(panelBase.IdBtrPanel, ObjectId.Null);
      }       
   }
}
using NUnit.Framework;
using AlbumPanelColorTiles.Base;
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
using AcadLib.Errors;

[assembly: CommandClass(typeof(AlbumPanelColorTiles.Base.Tests.BaseServiceTests))]

namespace AlbumPanelColorTiles.Base.Tests
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
      [TestCase(5, new string[] { "3НСг 40.29.32-3","3НСг 37.29.32-5", "3НСг 72.29.32-5", "3НСг 75.29.32-10БП3",
         "3НСг 73.29.32-4Б", "3НСг 75.29.32-27П3", "3НСНг 30.29.42-5"})]
      public void CreateBtrPanelFromBase(int i, string[] marks)
         {
         // Тест создания определения блока панели по описанию в xml базе.                
         PanelBase panelBase;         

         string testFile = @"c:\temp\test\АКР\Base\Tests\CreateBlockPanelTest\TestCreatePanels.dwg";
         //File.Copy(@"c:\Autodesk\AutoCAD\Pik\Settings\Template\АР\АР.dwt", testFile, true);
                  
         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            db.CloseInput(true);
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               baseService.ClearPanelsAkrFromDrawing(db);
               baseService.InitToCreationPanels(db);
               
               Point3d pt = Point3d.Origin;
               List<ObjectId> idsBtrPanels = new List<ObjectId>();

               // Создание определениц блоков панелей
               foreach (var mark in marks)
               {
                  Panel panelXml = baseService.GetPanelXml(mark);
                  panelBase = new PanelBase(panelXml, baseService);
                  panelBase.CreateBlock();

                  if (!panelBase.IdBtrPanel.IsNull)
                  {
                     idsBtrPanels.Add(panelBase.IdBtrPanel);
                  }
               }

               // Вставка вхождениц блоков панелей в модель
               using (var t = db.TransactionManager.StartTransaction())
               {
                  foreach (var idBtrPanel in idsBtrPanels)
                  {
                     var blRefPanel = new BlockReference(pt, idBtrPanel);
                     var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                     ms.AppendEntity(blRefPanel);
                     t.AddNewlyCreatedDBObject(blRefPanel, true);
                     pt = new Point3d(0, pt.Y + 10000, 0);
                  }
                  t.Commit();
               }
            }
            db.SaveAs(testFile, DwgVersion.Current);
         }         
      }       
   }
}
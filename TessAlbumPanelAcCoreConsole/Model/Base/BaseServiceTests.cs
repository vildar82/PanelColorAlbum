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
using Moq;
using System.IO;

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

      [Test]
      public void ReadPanelsFromXmlTest()
      {         
         int expectedCount = baseService.CountPanelsInBase;
         // Кол панелей в базе не должно быть равно нулю.
         Assert.AreNotEqual(expectedCount, 0);
      }

      [Test]
      public void CreateBtrPanelFromBase([Values("3НСг 72.29.32-5", "3НСг 72.29.32-1", "3НСг 72.29.32", "3НСг 72.29.32-16")] string mark) 
      {
         // Тест создания определения блока панели по описанию в xml базе.                  
         Panel panel;

         string testFile = @"c:\temp\test\АКР\Base\Tests\CreateBlockPanelTest\" + mark + ".dwg";
         File.Copy(@"c:\Autodesk\AutoCAD\Pik\Settings\Template\АР\АР.dwt", testFile, true);

         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               using (var t = db.TransactionManager.StartTransaction())
               {
                  baseService.InitToCreationPanels(db);
                  panel = baseService.CreateBtrPanel(mark);
                  t.Commit();
               }
            }
            db.SaveAs(testFile, DwgVersion.Current);
         }                  

         Assert.AreNotEqual(panel.IdBtrPanel, ObjectId.Null);
      }

      [Test, Ignore("Пока не работает")]      
      public void CreateFacadeTest()
      {
         Assert.DoesNotThrow(() =>
         {
            string testFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада.dwg";
            using (var db = new Database(false, true))
            {
               db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
               using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
               {
                  // Определение фасадов
                  List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();
                  using (var t = db.TransactionManager.StartTransaction())
                  {
                     // Очиста чертежа от блоков панелей АКР
                     baseService.ClearPanelsAkrFromDrawing(db);
                     // Создание определений блоков панелей по базе 
                     baseService.InitToCreationPanels(db);
                     baseService.CreateBtrPanels(facadesMounting);

                     // Создание фасадов
                     FacadeMounting.CreateFacades(facadesMounting);

                     t.Commit();
                  }
               }
               db.SaveAs(testFile, DwgVersion.Current);
            }
         });
      }

      [Test]
      public void TestGetFloors()
      {
         string testFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада.dwg";
         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               Assert.DoesNotThrow(() =>
               {
                  var floors = Floor.GetMountingBlocks(null);
               });
            }
         }
      }
   }
}
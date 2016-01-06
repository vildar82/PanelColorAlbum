using NUnit.Framework;
using AlbumPanelColorTiles.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;

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

      [Test()]
      public void ReadPanelsFromXmlTest()
      {         
         int expectedCount = baseService.CountPanelsInBase;
         // Кол панелей в базе не должно быть равно нулю.
         Assert.AreNotEqual(expectedCount, 0);
      }

      [Test()]
      public void CreateBtrPanelFromBase([Values("3НСг 72.29.32-5", "3НСг 72.29.32-1", "3НСг 72.29.32", "3НСг 72.29.32-16")] string mark) 
      {
         // Тест создания определения блока панели по описанию в xml базе.                  
         ObjectId idBtr;

         using (var db = new Database(true, true))
         {  
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {               
               using (var t = db.TransactionManager.StartTransaction())
               {
                  baseService.InitToCreationPanels();
                  idBtr = baseService.CreateBtrPanel(mark);
                  t.Commit();                  
               }               
            }
            db.SaveAs(@"c:\temp\test\АКР\Base\Tests\CreateBlockPanelTest\" + mark + ".dwg", DwgVersion.Current);
         }                  

         Assert.AreNotEqual(idBtr, ObjectId.Null);
      }
   }
}
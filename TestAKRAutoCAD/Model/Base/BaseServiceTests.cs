using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Base;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace TestAKRAutoCAD.Model.Base
{
   public class BaseServiceTests
   {
      BaseService baseService;
      Document doc;

      public BaseServiceTests()
      {
         baseService = new BaseService();
         baseService.ReadPanelsFromBase();
         doc = Application.DocumentManager.MdiActiveDocument;
      }

      public void CreateFacadeTest()
      {
         Inspector.Clear();
         string testFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада.dwg";
         //var docTest = Application.DocumentManager.MdiActiveDocument;
         var docTest = Application.DocumentManager.Open(testFile, false);
         var db = docTest.Database;
         using (var ld = docTest.LockDocument())
         {

            using (new AcadLib.WorkingDatabaseSwitcher(db))
            {
               // Очиста чертежа от блоков панелей АКР
               baseService.ClearPanelsAkrFromDrawing(db);
               // Подготовка - копирование блоков, слоев, стилей, и т.п.
               baseService.InitToCreationPanels(db);

               // Определение фасадов
               List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();
               List<FloorArchitect> floorsAr = FloorArchitect.GetAllPlanes(db, baseService);

               using (var t = db.TransactionManager.StartTransaction())
               {
                  // Создание определений блоков панелей по базе                
                  baseService.CreateBtrPanels(facadesMounting, floorsAr);
                  t.Commit();
               }
               //Создание фасадов
               FacadeMounting.CreateFacades(facadesMounting);

               //using (var t = db.TransactionManager.StartTransaction())
               //{
               //   var secBlocks = baseService.Env.BlPanelSections.OfType<BlockSectionHorizontal>();
               //   foreach (var item in secBlocks)
               //   {
               //      item.ReplaceAssociateHatch();
               //   }
               //   t.Commit();
               //}
            }
         }
         var saveFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада-CreateFacade.dwg";
         db.SaveAs(saveFile, DwgVersion.Current);

         if (Inspector.HasErrors)
         {
            Inspector.Show();
         }
         docTest.Editor.WriteMessage("\nCreateFacadeTest - Ок. см файл " + saveFile);
      }
   }
}

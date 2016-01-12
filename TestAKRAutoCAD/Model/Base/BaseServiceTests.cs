using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
         string testFile = @"c:\temp\test\АКР\Base\Tests\Тест-ПостроениеФасада.dwg";
         using (var db = new Database(false, true))
         {
            db.ReadDwgFile(testFile, FileOpenMode.OpenForReadAndAllShare, false, "");
            using (AcadLib.WorkingDatabaseSwitcher dbSwitcher = new AcadLib.WorkingDatabaseSwitcher(db))
            {
               // Определение фасадов
               List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();
               
               // Очиста чертежа от блоков панелей АКР
               baseService.ClearPanelsAkrFromDrawing(db);

               using (var t = db.TransactionManager.StartTransaction())
               {                  
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
         doc.Editor.WriteMessage("\nCreateFacadeTest - Ок. см файл " + testFile);
      }
   }
}

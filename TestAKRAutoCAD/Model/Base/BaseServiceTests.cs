using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Base;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace TestAKRAutoCAD.Base
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
         var docTest = Application.DocumentManager.MdiActiveDocument;         
         var db = docTest.Database;
         
         baseService.ClearPanelsAkrFromDrawing(db);
         // Подготовка - копирование блоков, слоев, стилей, и т.п.
         baseService.InitToCreationPanels(db);

         // Определение фасадов
         List<FacadeMounting> facadesMounting = FacadeMounting.GetFacadesFromMountingPlans();
         List<FloorArchitect> floorsAr = FloorArchitect.GetAllPlanes(db, baseService);

         // Создание определений блоков панелей по базе                
         baseService.CreateBtrPanels(facadesMounting, floorsAr);         

         //Создание фасадов
         FacadeMounting.CreateFacades(facadesMounting);

         //Восстановление ассоциативной штриховки в дин блоках сечений
         using (var t = db.TransactionManager.StartTransaction())
         {
            var secBlocks = baseService.Env.BlPanelSections;
            foreach (var item in secBlocks)
            {
               item.ReplaceAssociateHatch();
            }
            t.Commit();
         }

         if (Inspector.HasErrors)
         {
            Inspector.Show();
         }         
      }
   }
}

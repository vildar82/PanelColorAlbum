using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using TestAKRAutoCAD.Model.Base;

[assembly: CommandClass(typeof(TestAKRAutoCAD.CommandsTest))]

namespace TestAKRAutoCAD
{
   public class CommandsTest
   {
      [CommandMethod("TestCreateFacade", CommandFlags.Session)]
      public void TestCreateFacade()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;

         using (var ld = doc.LockDocument())
         {
            BaseServiceTests baseTest = new BaseServiceTests();
            baseTest.CreateFacadeTest();
         }
      }
   }
}

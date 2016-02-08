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
      [CommandMethod("TestCreateFacade")]
      public void TestCreateFacade()
      {         
         BaseServiceTests baseTest = new BaseServiceTests();
         baseTest.CreateFacadeTest();
      }
   }
}

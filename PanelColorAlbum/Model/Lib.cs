using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.Lib
{
   public static class Blocks
   {
      public static string EffectiveName (BlockReference blRef)
      {
         using (var btr = blRef.DynamicBlockTableRecord.GetObject( OpenMode.ForRead) as BlockTableRecord)
         {
            return btr.Name;
         }
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockWindow
   {
      public const string PropNameVISIBILITY = "Видимость";

      public string Mark { get; private set; } = string.Empty;
      public string BlName { get; private set; }

      public BlockWindow(BlockReference blRef, string blName)
      {
         BlName = blName;
         defineWindow(blRef);
         checkWindow();
      }

      private void checkWindow()
      {
         if (string.IsNullOrEmpty(Mark) )
         {
            Inspector.AddError($"Не определена марка окна {BlName}");
         }
      }

      private void defineWindow(BlockReference blRef)
      {
         if (blRef != null && blRef.DynamicBlockReferencePropertyCollection != null)
         {
            foreach (DynamicBlockReferenceProperty prop in blRef.DynamicBlockReferencePropertyCollection)
            {
               if (prop.PropertyName.Equals(PropNameVISIBILITY, StringComparison.OrdinalIgnoreCase))
               {
                  Mark = prop.Value.ToString();
                  break;
               }
            }
         }         
      }

      public static bool SetDynBlWinMark(BlockReference blRefWin, string mark)
      {
         bool res = false;
         bool findProp = false;
         var dynProps = blRefWin.DynamicBlockReferencePropertyCollection;
         foreach (DynamicBlockReferenceProperty item in dynProps)
         {
            if (item.PropertyName.Equals(PropNameVISIBILITY, StringComparison.OrdinalIgnoreCase))
            {
               findProp = true;
               var allowedVals = item.GetAllowedValues();
               if (allowedVals.Contains(mark))
               {
                  item.Value = mark;
                  res = true;
               }
               else
               {
                  Inspector.AddError($"Блок окна. Отсутствует видимость для марки окна {mark}");
               }
            }
         }
         if (!findProp)
         {
            Inspector.AddError("Блок окна. Не найдено динамическое свойтво блока окна Видимость");
         }
         return res;
      }

      public static List<string> GetMarks(ObjectId idBtrWindow)
      {
         List<string> marks = new List<string>();
         var blRef = new BlockReference(Point3d.Origin, idBtrWindow);
         var dynParams = blRef.DynamicBlockReferencePropertyCollection;
         if (dynParams != null)
         {
            foreach (DynamicBlockReferenceProperty param in dynParams)
            {
               if (param.PropertyName.Equals(PropNameVISIBILITY, StringComparison.OrdinalIgnoreCase))
               {
                  marks = param.GetAllowedValues().Cast<string>().ToList();
                  break;
               }
            }
         }
         return marks;
      }
   }
}

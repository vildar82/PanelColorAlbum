using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockSectionHorizontal: BlockSectionAbstract
   {
      //public bool IsSimple { get; private set; }
      public bool IsCheekLeft { get; private set; }
      public bool IsCheekRight { get; private set; }
      public bool IsOutsideRight { get; private set; }
      public bool IsOutsideLeft { get; private set; }

      public BlockSectionHorizontal(string blName, BaseService service) : base(blName, service)
      {

      }

      public override Result ParseBlName()
      {
         var ending = BlName.Substring(Settings.Default.BlockPanelSectionHorizontalPrefixName.Length);
         if (string.IsNullOrEmpty(ending))
         {
            //IsSimple = true;
            return Result.Ok();
         }
         var options = ending.Split('_');

         foreach (var opt in options)
         {
            if (string.IsNullOrWhiteSpace(opt))
            {
               continue;
            }
            // Кол слоев в панели (1 или 3 слойная стеновая панель)
            if (opt.Equals("сл", StringComparison.OrdinalIgnoreCase))
            {
               int n;
               if (int.TryParse(opt.Substring("сл".Length), out n))
               {
                  NLayerPanel = n;
               }
               else
               {
                  Inspector.AddError($"В блоке сечения {BlName} не определено количество слоев по префиксу 'сл'");
               }
            }
            // Торец слева (Cheek)
            else if (opt.Equals("тл", StringComparison.OrdinalIgnoreCase))
            {
               IsCheekLeft = true;
            }
            // Торец справа (Cheek)
            else if (opt.Equals("тп", StringComparison.OrdinalIgnoreCase))
            {
               IsCheekRight = true;

            }
            // Примыкание справа (Outside)
            else if (opt.Equals("пп", StringComparison.OrdinalIgnoreCase))
            {
               IsOutsideRight = true;
            }
            // Примыкание слева (Outside)
            else if (opt.Equals("пл", StringComparison.OrdinalIgnoreCase))
            {
               IsOutsideRight = true;
            }
         }         
         return Result.Ok();
      }          
   }
}

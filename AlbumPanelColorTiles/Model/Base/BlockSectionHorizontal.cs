using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockSectionHorizontal: BlockSectionAbstract
   {
      public bool IsSimple { get; private set; }
      public bool IsCheekLeft { get; private set; }
      public bool IsCheekRight { get; private set; }
      public bool IsOutsideRight { get; private set; }
      public bool IsOutsideLeft { get; private set; }

      public BlockSectionHorizontal(string blName) : base(blName)
      {

      }

      public override Result ParseBlName()
      {
         var ending = BlName.Substring(Settings.Default.BlockPanelSectionHorizontalPrefixName.Length);
         if (string.IsNullOrEmpty(ending))
         {
            IsSimple = true;
            return Result.Ok();
         }
         var options = ending.ToLower().Split('_');

         foreach (var opt in options)
         {
            if (string.IsNullOrWhiteSpace(opt))
            {
               continue;
            }
            switch (opt)
            {
               case "тл":
                  IsCheekLeft = true;
                  break;
               case "тп":
                  IsCheekRight = true;
                  break;
               case "пп":
                  IsOutsideRight = true;
                  break;
               case "пл":
                  IsOutsideLeft = true;
                  break;
               default:
                  break;
            }
         }
         return Result.Ok();
      }          
   }
}

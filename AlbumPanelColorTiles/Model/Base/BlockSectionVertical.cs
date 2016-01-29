using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockSectionVertical : BlockSectionAbstract
   {
      public int Height { get; private set; }
      //public int Thickness { get; private set; }
      public bool Window { get; private set; }

      public BlockSectionVertical(string blName, BaseService service) : base(blName, service)
      {         
         
      }

      public override Result ParseBlName()
      {
         var ending = BlName.Substring(Settings.Default.BlockPanelSectionVerticalPrefixName.Length);
         if (string.IsNullOrEmpty(ending))
         {  
            return Result.Fail("В имени блока не найдены параметры высоты и наличия окна");            
         }
         var options = ending.ToLower().Split('_');

         foreach (var opt in options)
         {
            if (string.IsNullOrWhiteSpace(opt))
            {
               continue;
            }
            switch (opt.First())
            {
               case 'h':
                  Height = getValue(opt.Substring(1));
                  break;
               //case 't':
               //   Thickness = getValue(opt.Substring(1));
               //   break;
               case 'w':
                  Window = true;
                  break;
               default:
                  break;
            }
         }
         return checkParamVerticalSec();
      }

      private Result checkParamVerticalSec()
      {
         string err = string.Empty;
         if (Height <= 0)
         {
            err += "Не определена длина панели для блока сечения.";
         }
         //if (Thickness <= 0)
         //{
         //   err += "Не определена ширина панели для блока сечения.";
         //}
         if (!string.IsNullOrEmpty(err))
         {
            return Result.Fail(err);
            //throw new Exception(err);
         }
         return Result.Ok();
      }

      private int getValue(string v)
      {
         int res;
         int.TryParse(v, out res);
         return res;
      }    
   }
}

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
   public class BlockSectionVertical : BlockSectionAbstract
   {
      public int Height { get; private set; }
      //public int Thickness { get; private set; }
      public bool Window { get; private set; }
      public bool IsOL { get; private set; }

      public BlockSectionVertical(string blName, BaseService service) : base(blName, service)
      {         
         
      }

      public override Result ParseBlName()
      {
         var ending = BlName.Substring(Settings.Default.BlockPanelSectionVerticalPrefixName.Length);
         if (string.IsNullOrEmpty(ending))
         {
            return Result.Ok();
            //return Result.Fail("В имени блока не найдены параметры высоты и наличия окна");            
         }
         var options = ending.Split('_');

         foreach (var opt in options)
         {
            if (string.IsNullOrWhiteSpace(opt))
            {
               continue;
            }
            // Кол слоев панели 1-слойная(бетон), 3-слойная(несущий, утеплитель, внешний)
            if (opt.StartsWith("сл", StringComparison.OrdinalIgnoreCase))
            {
               int n;
               if (int.TryParse(opt.Substring("сл".Length), out n))
               {
                  NLayerPanel = n;
               }
               else
               {
                  Inspector.AddError($"В блоке сечения {BlName} не определено количество слоев по префиксу 'сл'",
                     icon: System.Drawing.SystemIcons.Error);
               }
            }
            // Высота сечения
            else if (opt.StartsWith("h", StringComparison.OrdinalIgnoreCase))
            {
               int h;
               if (int.TryParse(opt.Substring(1), out h))
               {
                  Height = h;
               }
               else
               {
                  Inspector.AddError($"В блоке сечения {BlName} не определена высота сечения по префиксу 'h'", 
                     icon: System.Drawing.SystemIcons.Error);
               }               
            }
            // Наличие окна в сечении
            else if (opt.Equals("w", StringComparison.OrdinalIgnoreCase))
            {
               Window = true;
            }
            // Сечение для чердачной панели
            else if (opt.Equals("ч", StringComparison.OrdinalIgnoreCase))
            {
               IsUpperStoreyPanel = true;
            }
            // Сечение для ОЛ панели
            else if (opt.Equals("ол", StringComparison.OrdinalIgnoreCase))
            {
               IsOL = true;
            }
         }
         return checkParamVerticalSec();
      }

      private Result checkParamVerticalSec()
      {
         string err = string.Empty;
         //if (Height <= 0)
         //{
         //   err += "Не определена длина панели для блока сечения.";
         //}
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
   }
}

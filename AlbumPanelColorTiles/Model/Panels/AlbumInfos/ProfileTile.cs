using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Panels.AlbumInfos
{
   public class ProfileTile
   {
      public string ProfileTileName { get; set; } = "";
      public string ProfileTileMark { get; set; } = "";

      public ProfileTile(ObjectId idBlRefProfile)
      {
         // Блок Профиля для торцевых панелей 
         // Считывание атрибу названия профиля и марки профиля
         using (var blRef = idBlRefProfile.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            var atrRefs = AttributeInfo.GetAttrRefs(blRef);
            ProfileTileName = atrRefs.Where(a => a.Tag.Equals("НАЗВАНИЕ", StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Text;
            ProfileTileMark = atrRefs.Where(a => a.Tag.Equals("МАРКА", StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()?.Text;
         }
      }
   }
}

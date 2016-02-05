using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BlockInfo
   {
      public ObjectId IdBtr { get; set; }
      public string BlName { get; set; }
      public List<AttributeInfo> AttrsDef { get; set; }
      public List<AttributeInfo> AttrsRef { get; set; }

      public BlockInfo (BlockReference blRef, string blName)
      {
         IdBtr = blRef.DynamicBlockTableRecord;
         BlName = blName;
         AttrsDef = AttributeInfo.GetAttrDefs(IdBtr);
         AttrsRef = AttributeInfo.GetAttrRefs(blRef);
      }
   }
}

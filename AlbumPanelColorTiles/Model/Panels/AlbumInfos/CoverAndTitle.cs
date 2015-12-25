using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Panels.AlbumInfos
{
   // Обложека и титульный лист
   public class CoverAndTitle
   {
      public ObjectId IdCoverBtr { get; private set; }
      public ObjectId IdTitleBtr { get; private set; }

      public void Search()
      {

      }

      internal void Check(BlockReference blRefCover)
      {
         throw new NotImplementedException();
      }
   }
}

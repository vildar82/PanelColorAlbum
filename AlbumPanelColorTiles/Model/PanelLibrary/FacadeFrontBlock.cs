using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Блок обозначения стороны фасада на монтажке
   public class FacadeFrontBlock
   {
      private ObjectId _idBlRef;
      RTreeLib.Rectangle _rectangleRTree;

      public FacadeFrontBlock(BlockReference blRef)
      {
         _idBlRef = blRef.Id;
         _rectangleRTree = ColorArea.GetRectangleRTree(blRef.GeometricExtents); 
      }

      public ObjectId IdBlRef { get { return _idBlRef; } }
      public RTreeLib.Rectangle RectangleRTree { get { return _rectangleRTree; } }

      public static List<FacadeFrontBlock> GetFacadeFrontBlocks()
      {
         List<FacadeFrontBlock> facadeFrontBlocks = new List<FacadeFrontBlock>();
         var db = HostApplicationServices.WorkingDatabase;
         using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRef = idEnt.GetObject(OpenMode.ForRead) as BlockReference)
                  {
                     // Если это блок обозначения стороны фасада - по имени блока
                     if (string.Equals( Lib.Blocks.EffectiveName(blRef), Album.Options.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        FacadeFrontBlock front = new FacadeFrontBlock(blRef);
                        facadeFrontBlocks.Add(front);
                     }
                  }
               }
            }
         }
         return facadeFrontBlocks;
      }
   }
}

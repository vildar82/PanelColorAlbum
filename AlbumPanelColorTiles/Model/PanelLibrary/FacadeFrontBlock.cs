using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Блок обозначения стороны фасада на монтажке
   public class FacadeFrontBlock
   {
      private Extents3d _extents;
      private ObjectId _idBlRef;
      private RTreeLib.Rectangle _rectangleRTree;

      public FacadeFrontBlock(BlockReference blRef)
      {
         _idBlRef = blRef.Id;
         _extents = blRef.GeometricExtents;
         _rectangleRTree = ColorArea.GetRectangleRTree(_extents);
      }

      public Extents3d Extents { get { return _extents; } }
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
                     if (string.Equals(blRef.GetEffectiveName(), Properties.Settings.Default.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
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
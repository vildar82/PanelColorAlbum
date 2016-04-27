using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Блок обозначения стороны фасада на монтажке
   public class FacadeFrontBlock
   {
      private string _blName;
      private Extents3d _extents;
      private ObjectId _idBlRef;
      private RTreeLib.Rectangle _rectangleRTree;

      public FacadeFrontBlock(BlockReference blRef)
      {
         _blName = blRef.GetEffectiveName();
         _idBlRef = blRef.Id;
         _extents = blRef.GeometricExtentsСlean();
         _rectangleRTree = ColorArea.GetRectangleRTree(_extents);
      }

      public string BlName { get { return _blName; } }
      public Extents3d Extents { get { return _extents; } }
      public ObjectId IdBlRef { get { return _idBlRef; } }
      public RTreeLib.Rectangle RectangleRTree { get { return _rectangleRTree; } }

      public static List<FacadeFrontBlock> GetFacadeFrontBlocks(BlockTableRecord ms)
      {
         List<FacadeFrontBlock> facadeFrontBlocks = new List<FacadeFrontBlock>();

         foreach (ObjectId idEnt in ms)
         {
            var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
            if (blRef == null) { continue; }
            // Если это блок обозначения стороны фасада - по имени блока
            if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
            {
               FacadeFrontBlock front = new FacadeFrontBlock(blRef);
               facadeFrontBlocks.Add(front);
            }
         }
         return facadeFrontBlocks;
      }
   }
}
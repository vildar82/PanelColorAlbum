using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public abstract class PanelAKR
   {
      protected Extents3d _extentsTiles;

      public string BlName { get; private set; }
      public string Description { get; set; }
      public List<EntityInfo> EntInfos { get; private set; }
      public double HeightPanelByTile { get; private set; }
      public ObjectId IdBtrAkrPanel { get; private set; }
      //public bool IsEndLeftPanel { get; private set; }
      //public bool IsEndRightPanel { get; private set; }
      public string MarkAkrWithoutWhite { get; private set; }

      public PanelAKR(ObjectId idBtrAkrPanel, string blName)
      {
         IdBtrAkrPanel = idBtrAkrPanel;
         BlName = blName;
         Description = "";
         MarkAkrWithoutWhite = MarkSb.GetMarkSbCleanName(MarkSb.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         //defineEndsPanel(blName);
         // Список объектов в блоке
         EntInfos = EntityInfo.GetEntInfoBtr(idBtrAkrPanel);
      }

      public void DefineGeom(ObjectId idBtrPanelAkr)
      {
         string blName;
         using (var btrAkr = idBtrPanelAkr.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            blName = btrAkr.Name;
            bool isFirstTile = true;
            foreach (ObjectId idEnt in btrAkr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefTile = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     if (string.Equals(blRefTile.GetEffectiveName(), Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        if (isFirstTile)
                        {
                           _extentsTiles = blRefTile.GeometricExtents;
                           isFirstTile = false;
                        }
                        else
                        {
                           _extentsTiles.AddExtents(blRefTile.GeometricExtents);
                        }
                     }
                  }
               }
            }
         }
         HeightPanelByTile = _extentsTiles.MaxPoint.Y - _extentsTiles.MinPoint.Y + Settings.Default.TileSeam;
         double shiftEnd = 0;
         if (blName.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -Settings.Default.FacadeEndsPanelIndent * 0.5;// 445;// Торец слева - сдвинуть влево FacadeEndsPanelIndent
         }
         else if (blName.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -Settings.Default.FacadeEndsPanelIndent * 0.5;// 445;// Торец спрва - сдвинуть вправо
         }
         var test = _extentsTiles.MaxPoint.X - _extentsTiles.MinPoint.X;
         //_distToCenterFromBase = (_extentsTiles.MaxPoint.X - _extentsTiles.MinPoint.X) * 0.5 + shiftEnd;
      }

      public override string ToString()
      {
         return string.Format("{0}{1}{2}", BlName, string.IsNullOrEmpty(Description) ? "" : " - ", Description);
      }

      // границы блока по плиткам
      //private void defineEndsPanel(string markAkrWithoutWhite)
      //{
      //   if (markAkrWithoutWhite.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
      //   {
      //      _isEndLeftPanel = true; //markSbName.EndsWith(Album.Options.endLeftPanelSuffix); // Торец слева
      //   }
      //   if (markAkrWithoutWhite.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
      //   {
      //      _isEndRightPanel = true; //markSbName.EndsWith(Album.Options.endRightPanelSuffix); // Торец спрва
      //   }
      //}
   }
}
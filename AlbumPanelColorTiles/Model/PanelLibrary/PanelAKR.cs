using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public class PanelAKR
   {
      protected Extents3d _extentsTiles;

      public string BlName { get; private set; }
      public string WindowSuffix { get; private set; }
      public string Description { get; set; }
      public List<EntityInfo> EntInfos { get; private set; }
      public double HeightPanelByTile { get; private set; }
      public ObjectId IdBtrAkrPanel { get; private set; }      
      public string MarkAkr { get; private set; }
      public ObjectId IdBtrPanelAkrInFacade { get; set; }      

      public PanelAKR(ObjectId idBtrAkrPanel, string blName)
      {
         IdBtrAkrPanel = idBtrAkrPanel;
         BlName = blName;
         Description = "";

         var val = MarkSb.GetMarkSbName(blName);
         string windowSx;
         MountingPanel.GetMarkWithoutWindowsSuffix(val, out windowSx);
         WindowSuffix = windowSx;
         MarkAkr = MarkSb.GetMarkSbCleanName(val);//.Replace(' ', '-');
                  
         // Список объектов в блоке
         EntInfos = EntityInfo.GetEntInfoBtr(idBtrAkrPanel);
      }      

      public void DefineGeom(ObjectId idBtrPanelAkr)
      {
         string blName;
         _extentsTiles = new Extents3d();
         using (var btrAkr = idBtrPanelAkr.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            blName = btrAkr.Name;            
            foreach (ObjectId idEnt in btrAkr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefTile = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     if (string.Equals(blRefTile.GetEffectiveName(), Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        _extentsTiles.AddExtents(blRefTile.GeometricExtents);
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

      public static List<PanelAKR> GetAkrPanelLib(Database dbLib)
      {
         List<PanelAKR> panelsAkrLIb = new List<PanelAKR>();
         using (var bt = dbLib.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            foreach (ObjectId idBtr in bt)
            {
               using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
               {
                  if (MarkSb.IsBlockNamePanel(btr.Name))
                  {
                     PanelAKR panelAkr = new PanelAKR(idBtr, btr.Name);
                     panelsAkrLIb.Add(panelAkr);
                  }
               }
         }
         }
         return panelsAkrLIb;
      }
   }
}
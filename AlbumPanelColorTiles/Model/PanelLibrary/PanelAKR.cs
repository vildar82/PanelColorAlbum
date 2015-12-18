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
      protected string _blName;
      protected string _description;
      protected double _distToCenterFromBase;
      protected List<EntityInfo> _entInfos;
      protected Extents3d _extentsTiles; // границы блока по плиткам
      protected double _heightPanelByTile;

      // Список объектов в блоке для сравнения блоков панелей фасада и библмиотеки
      protected ObjectId _idBtrAkrPanel;

      protected bool _isEndLeftPanel;
      protected bool _isEndRightPanel;
      protected string _markAkrWithoutWhite;

      public PanelAKR(ObjectId idBtrAkrPanel, string blName)
      {
         _idBtrAkrPanel = idBtrAkrPanel;
         _blName = blName;
         _description = "";
         _markAkrWithoutWhite = MarkSb.GetMarkSbCleanName(MarkSb.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         defineEndsPanel(blName);
         // Список объектов в блоке
         _entInfos = EntityInfo.GetEntInfoBtr(idBtrAkrPanel);
      }

      public string BlName { get { return _blName; } }
      public string Description { get { return _description; } set { _description = value; } }
      public double DistToCenterFromBase { get { return _distToCenterFromBase; } }
      public List<EntityInfo> EntInfos { get { return _entInfos; } }
      public double HeightPanelByTile { get { return _heightPanelByTile; } }
      public ObjectId IdBtrAkrPanel { get { return _idBtrAkrPanel; } }
      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }
      public bool IsEndRightPanel { get { return _isEndRightPanel; } }
      public string MarkAkrWithoutWhite { get { return _markAkrWithoutWhite; } }

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
         _heightPanelByTile = _extentsTiles.MaxPoint.Y - _extentsTiles.MinPoint.Y + Settings.Default.TileSeam;
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
         _distToCenterFromBase = (_extentsTiles.MaxPoint.X - _extentsTiles.MinPoint.X) * 0.5 + shiftEnd;
      }

      public override string ToString()
      {
         return string.Format("{0}{1}{2}", _blName, string.IsNullOrEmpty(_description) ? "" : " - ", _description);
      }

      private void defineEndsPanel(string markAkrWithoutWhite)
      {
         if (markAkrWithoutWhite.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            _isEndLeftPanel = true; //markSbName.EndsWith(Album.Options.endLeftPanelSuffix); // Торец слева
         }
         if (markAkrWithoutWhite.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            _isEndRightPanel = true; //markSbName.EndsWith(Album.Options.endRightPanelSuffix); // Торец спрва
         }
      }

      //public double GetDistToCenter(ObjectId idBtrPanelAkr)
      //{
      //   if (_distToCenterFromBase != 0)
      //   {
      //      return _distToCenterFromBase;
      //   }
      //   else
      //   {
      //      DefineGeom(idBtrPanelAkr);
      //      return _distToCenterFromBase;
      //   }
      //}
   }
}
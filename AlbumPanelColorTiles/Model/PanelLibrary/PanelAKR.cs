using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public enum EnumReportStatus
   {
      Other, // 0 - default
      New,
      Changed,
      Force
   }

   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public abstract class PanelAKR
   {
      protected string _blName;
      protected List<EntityInfo> _entInfos;// Список объектов в блоке для сравнения блоков панелей фасада и библмиотеки
      protected ObjectId _idBtrAkrPanel;      
      protected bool _isEndLeftPanel;
      protected bool _isEndRightPanel;
      protected string _markAkrWithoutWhite;
      protected double _distToCenterFromBase;
      protected string _description;      

      public PanelAKR(ObjectId idBtrAkrPanel, string blName)
      {
         _idBtrAkrPanel = idBtrAkrPanel;
         _blName = blName;
         _description = "";
         _markAkrWithoutWhite = MarkSbPanelAR.GetMarkSbCleanName(MarkSbPanelAR.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         defineEndsPanel(blName);
         // Список объектов в блоке
         _entInfos = EntityInfo.GetEntInfoBtr(idBtrAkrPanel);
      }      

      public string BlName { get { return _blName; } }     
      public List<EntityInfo> EntInfos { get { return _entInfos; } }      
      public ObjectId IdBtrAkrPanel { get { return _idBtrAkrPanel; } }
      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }
      public bool IsEndRightPanel { get { return _isEndRightPanel; } }
      public string MarkAkrWithoutWhite { get { return _markAkrWithoutWhite; } }
      public string Description { get { return _description; } set { _description = value; } }

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

      public override string ToString()
      {
         return string.Format("{0}{1}{2}",  _blName, string.IsNullOrEmpty(_description) ? "" : " - " , _description);
      }

      public double GetDistToCenter(ObjectId idBtrPanelAkr)
      {
         if (_distToCenterFromBase != 0)
         {
            return _distToCenterFromBase;
         }
         var extTiles = new Extents3d();
         string blName;
         using (var btrAkr = idBtrPanelAkr.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            blName = btrAkr.Name;
            foreach (ObjectId idEnt in btrAkr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefTile = idEnt.Open(OpenMode.ForRead) as BlockReference)
                  {
                     if (string.Equals(blRefTile.GetEffectiveName(), Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        extTiles.AddExtents(blRefTile.GeometricExtents);
                     }
                  }
               }
            }
         }
         double shiftEnd = 0;
         if (blName.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -Settings.Default.FacadeEndsPanelIndent * 0.5;// 445;// Торец слева - сдвинуть влево FacadeEndsPanelIndent
         }
         else if (blName.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -Settings.Default.FacadeEndsPanelIndent * 0.5;// 445;// Торец спрва - сдвинуть вправо
         }
         _distToCenterFromBase = (extTiles.MaxPoint.X - extTiles.MinPoint.X) * 0.5 + shiftEnd;
         return _distToCenterFromBase;
      }
   }
}
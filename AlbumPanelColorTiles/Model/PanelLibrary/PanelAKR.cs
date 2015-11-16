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
      Other,
      New,
      Changed,
      Force
   }

   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public abstract class PanelAKR
   {
      private string _blName;
      private double _distToCenterFromBase;
      private List<EntityInfo> _entInfos;      
      private ObjectId _idBlRefForShow;      
      private ObjectId _idBtrAkrPanelInFacade;
      private ObjectId _idBtrAkrPanelInLib;
      private bool _isElectricCopy;

      // расстояние от точки вставки панели до центра (по плитке) по X
      private bool _isEndLeftPanel;

      private bool _isEndRightPanel;
      private string _markAkrWithoutWhite;

      // блок панели АКР в файле библиотеки панелей
      // блок панели АКР в файле фасада
      private PanelSB _panelSb;

      private EnumReportStatus _reportStatus;
      // Список объектов в блоке для сравнения панелей с блоками в фасаде

      public PanelAKR(ObjectId idBtrAkrPanelInLib, string blName)
      {
         _idBtrAkrPanelInLib = idBtrAkrPanelInLib;
         _blName = blName;
         _markAkrWithoutWhite = MarkSbPanelAR.GetMarkSbCleanName(MarkSbPanelAR.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         defineEndsPanel(blName);
         // Список объектов в блоке
         _entInfos = EntityInfo.GetEntInfoBtr(idBtrAkrPanelInLib);
      }

      public string BlNameInLib { get { return _blName; } }

      public double DistToCenterFromBase
      {
         get
         {
            if (_distToCenterFromBase == 0)
            {
               _distToCenterFromBase = getDistToCenter(_idBtrAkrPanelInFacade);
            }
            return _distToCenterFromBase;
         }
      }

      public List<EntityInfo> EntInfos { get { return _entInfos; } }      

      public ObjectId IdBtrAkrPanelInFacade
      {
         get { return _idBtrAkrPanelInFacade; }
         set { _idBtrAkrPanelInFacade = value; }
      }

      public ObjectId IdBtrAkrPanelInLib { get { return _idBtrAkrPanelInLib; } }
      public bool IsElectricCopy { get { return _isElectricCopy; } set { _isElectricCopy = value; } }
      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }
      public bool IsEndRightPanel { get { return _isEndRightPanel; } }
      public string MarkAkrWithoutWhite { get { return _markAkrWithoutWhite; } }
      public PanelSB PanelSb { get { return _panelSb; } set { _panelSb = value; } }
      public EnumReportStatus ReportStatus { get { return _reportStatus; } set { _reportStatus = value; } }

      public static List<PanelAKR> GetChangedAndNewPanels(List<PanelAKR> panelsAkrInFacade, List<PanelAKR> panelsAkrInLib)
      {
         List<PanelAKR> panelsChangedAndNew = new List<PanelAKR>();
         foreach (var panelAkrInFacade in panelsAkrInFacade)
         {
            var panelAkrInLib = panelsAkrInLib.Find(p => string.Equals(p.BlNameInLib, panelAkrInFacade.BlNameInLib, StringComparison.OrdinalIgnoreCase));
            if (panelAkrInLib == null)
            {
               // panelAkrInFacade - новая панель, которой нет в библмиотеке
               panelAkrInFacade.ReportStatus = EnumReportStatus.New; // "Новая";               
               panelsChangedAndNew.Add(panelAkrInFacade);
            }
            else
            {
               // сравнить панели (по списку объектов в блоке)
               if (!panelAkrInFacade.EntInfos.SequenceEqual(panelAkrInLib.EntInfos))
               {
                  panelAkrInFacade.ReportStatus = EnumReportStatus.Changed; //"Изменившаяся";
                  panelsChangedAndNew.Add(panelAkrInFacade);
               }
            }
         }
         return panelsChangedAndNew;
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

      private double getDistToCenter(ObjectId idBtrPanelAkr)
      {
         var extTiles = new Extents3d();
         string blName;
         using (var btrAkr = idBtrPanelAkr.GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            blName = btrAkr.Name;
            foreach (ObjectId idEnt in btrAkr)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRefTile = idEnt.GetObject(OpenMode.ForRead) as BlockReference)
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
         return (extTiles.MaxPoint.X - extTiles.MinPoint.X) * 0.5 + shiftEnd;
      }

      public string ReportStatusString()
      {
         switch (_reportStatus)
         {
            case EnumReportStatus.New:
               return "Новая";               
            case EnumReportStatus.Changed:
               return "Изменившаяся";
            case EnumReportStatus.Force:
               return "Принудительно";
            default:
               break;
         }
         return "";
      }

      public override string ToString()
      {
         return _blName;
      }

      public void ShowPanelInFacade(Document doc)
      {
         if (_idBlRefForShow.IsNull || _idBlRefForShow.IsErased)
         {
            // поиск панели на чертеже
            if (!getFirstBlRefInFacade(out _idBlRefForShow))
            {
               // вставка блока
               _idBlRefForShow = AcadLib.Blocks.BlockInsert.Insert(_blName);
               if (_idBlRefForShow.IsNull)
               {
                  return;
               }
            }
         }
         Extents3d extents;
         using (var blRef = _idBlRefForShow.Open( OpenMode.ForRead) as BlockReference)
         {
            extents = blRef.GeometricExtents;
         }
         doc.Editor.Zoom(extents);
      }

      private bool getFirstBlRefInFacade(out ObjectId _idBlRefForShow)
      {
         using (var btrPanel = IdBtrAkrPanelInFacade.Open( OpenMode.ForRead) as BlockTableRecord)
         {
            var idsBlRef = btrPanel.GetBlockReferenceIds(true, false);
            if (idsBlRef.Count >0 )
            {
               _idBlRefForShow = idsBlRef[0];
               return true;
            }
         }
         _idBlRefForShow = ObjectId.Null;
         return false;
      }
   }
}
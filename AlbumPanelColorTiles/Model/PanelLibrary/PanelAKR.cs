using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.Properties;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public class PanelAKR
   {
      private string _blNameInLib;
      private double _distToCenterFromBase;
      private ObjectId _idBlRef;
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

      public PanelAKR(ObjectId idBtrAkrPanelInLib, string blName)
      {
         _idBtrAkrPanelInLib = idBtrAkrPanelInLib;
         _blNameInLib = blName;
         _markAkrWithoutWhite = MarkSbPanelAR.GetMarkSbCleanName(MarkSbPanelAR.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         defineEndsPanel(blName);
      }

      public string BlNameInLib { get { return _blNameInLib; } }

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

      /// <summary>
      /// Вхождение блока в файле фасада
      /// </summary>
      public ObjectId IdBlRef { get { return _idBlRef; } set { _idBlRef = value; } }

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

      /// <summary>
      /// Простая расстановка АКР-панелей в точуи вставки панелей СБ
      /// </summary>
      /// <param name="_allPanelsSB"></param>
      public static void SimpleInsert(List<PanelSB> _allPanelsSB)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            using (ProgressMeter progress = new ProgressMeter())
            {
               progress.SetLimit(_allPanelsSB.Count);
               progress.Start("Простая расстановка панелей");

               var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
               foreach (var panelSb in _allPanelsSB)
               {
                  int countNull = 0;
                  if (panelSb.PanelAKR != null)
                  {
                     var blRefPanelAkr = new BlockReference((panelSb.GetPtInModel(panelSb.PanelAKR)), panelSb.PanelAKR.IdBtrAkrPanelInFacade);
                     panelSb.PanelAKR.IdBlRef = ms.AppendEntity(blRefPanelAkr);
                     t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
                  }
                  else
                  {
                     countNull++;
                  }
                  progress.MeterProgress();
               }
               t.Commit();
               progress.Stop();
            }
         }
      }

      public PanelAKR CopyLibBlockElectricInTempFile(PanelSB panelSb)
      {
         PanelAKR panelAkr = null;
         try
         {
            string markAkr = panelSb.MarkSb;
            if (panelSb.IsEndLeftPanel)
            {
               markAkr += Settings.Default.EndLeftPanelSuffix;
            }
            else if (panelSb.IsEndRightPanel)
            {
               markAkr += Settings.Default.EndRightPanelSuffix;
            }
            SymbolUtilityServices.ValidateSymbolName(markAkr, false);
            // копирование блока с новым именем с электрикой
            ObjectId idBtrAkeElectricInTempLib = Lib.Block.CopyBtr(_idBtrAkrPanelInLib, markAkr);
            panelAkr = new PanelAKR(idBtrAkeElectricInTempLib, markAkr);
            panelAkr.IsElectricCopy = true;
         }
         catch
         {
         }
         return panelAkr;
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
            shiftEnd = -446.7;// Торец слева - сдвинуть влево
         }
         else if (blName.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -447;// Торец спрва - сдвинуть вправо
         }
         return (extTiles.MaxPoint.X - extTiles.MinPoint.X) * 0.5 + shiftEnd;
      }
   }
}
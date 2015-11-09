using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public class PanelAKR
   {      
      private string _blNameInLib;
      private string _markAkrWithoutWhite;
      private ObjectId _idBtrAkrPanelInLib; // блок панели АКР в файле библиотеки панелей
      private ObjectId _idBtrAkrPanelInFacade; // блок панели АКР в файле фасада
      private ObjectId _idBlRef;
      private PanelSB _panelSb;
      private double _distToCenterFromBase; // расстояние от точки вставки панели до центра (по плитке) по X
      private bool _isEndLeftPanel;
      private bool _isEndRightPanel;
      private bool _isElectricCopy;

      public ObjectId IdBtrAkrPanelInLib { get { return _idBtrAkrPanelInLib; } }
      public ObjectId IdBtrAkrPanelInFacade
      {
         get { return _idBtrAkrPanelInFacade; }
         set { _idBtrAkrPanelInFacade = value; }
      } 

      /// <summary>
      /// Вхождение блока в файле фасада
      /// </summary>
      public ObjectId IdBlRef { get { return _idBlRef; } set { _idBlRef = value; } }
      public string BlNameInLib { get { return _blNameInLib; } }
      public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }
      public bool IsEndRightPanel { get { return _isEndRightPanel; } }
      public bool IsElectricCopy { get { return _isElectricCopy; } set { _isElectricCopy = value; } }
      public string MarkAkrWithoutWhite { get { return _markAkrWithoutWhite; } }
      public PanelSB PanelSb { get { return _panelSb; } set { _panelSb = value; } }

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

      public PanelAKR(ObjectId idBtrAkrPanelInLib, string blName)
      {
         _idBtrAkrPanelInLib = idBtrAkrPanelInLib;
         _blNameInLib = blName;
         _markAkrWithoutWhite = MarkSbPanelAR.GetMarkSbCleanName(MarkSbPanelAR.GetMarkSbName(blName)).Replace(' ', '-');
         // определение - торцов панели
         defineEndsPanel(blName);
      }

      private void defineEndsPanel(string markAkrWithoutWhite)
      {
         if (markAkrWithoutWhite.IndexOf(Album.Options.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            _isEndLeftPanel = true; //markSbName.EndsWith(Album.Options.endLeftPanelSuffix); // Торец слева
         }
         if (markAkrWithoutWhite.IndexOf(Album.Options.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            _isEndRightPanel = true; //markSbName.EndsWith(Album.Options.endRightPanelSuffix); // Торец спрва  
         }
      }

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
                     if (string.Equals(blRefTile.GetEffectiveName(), Album.Options.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        extTiles.AddExtents(blRefTile.GeometricExtents);
                     }
                  }
               }               
            }
         }
         double shiftEnd = 0;
         if (blName.IndexOf(Album.Options.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -446.7;// Торец слева - сдвинуть влево
         }
         else if (blName.IndexOf(Album.Options.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -447;// Торец спрва - сдвинуть вправо
         }
         return (extTiles.MaxPoint.X - extTiles.MinPoint.X)*0.5 + shiftEnd;
      }

      public PanelAKR CopyLibBlockElectricInTempFile(PanelSB panelSb)
      {
         PanelAKR panelAkr = null; 
         try
         {
            string markAkr = panelSb.MarkSb;
            if (panelSb.IsEndLeftPanel)
            {
               markAkr += Album.Options.EndLeftPanelSuffix;
            }
            else if (panelSb.IsEndRightPanel)
            {
               markAkr += Album.Options.EndRightPanelSuffix;
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
   }
}

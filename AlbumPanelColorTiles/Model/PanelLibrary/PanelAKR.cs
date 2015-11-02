using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Панель АКР - под покраску - блок из библиотеки панелей АКР.
   public class PanelAKR
   {
      private ObjectId _idBtrAkrPanelInLib; // блок панели АКР в файле библиотеки панелей
      private ObjectId _idBtrAkrPanelInFacade; // блок панели АКР в файле фасада
      private PanelSB _panelSb;
      private double _distToCenterFromBase; // расстояние от точки вставки панели до центра (по плитке) по X

      public ObjectId IdBtrAkrPanelInFacade
      {
         get { return _idBtrAkrPanelInFacade; }
         set { _idBtrAkrPanelInFacade = value; }
      }

      /// <summary>
      /// Вхождение блока в файле фасада
      /// </summary>
      public ObjectId IdBlRef { get; set; }

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

      public PanelAKR(ObjectId idBtrAkrPanelInLib, PanelSB panelSb)
      {
         _idBtrAkrPanelInLib = idBtrAkrPanelInLib;
         _panelSb = panelSb;         
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
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
            foreach (var panelSb in _allPanelsSB)
            {
               if (panelSb.PanelAKR != null )
               {
                  var blRefPanelAkr = new BlockReference((panelSb.PanelAKR.GetPtInModel()), panelSb.PanelAKR.IdBtrAkrPanelInFacade);
                  panelSb.PanelAKR.IdBlRef = ms.AppendEntity(blRefPanelAkr);
                  t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
               }
            }
            t.Commit();
         }
      }

      public Point3d GetPtInModel()
      {
         return new Point3d(_panelSb.PtCenterPanelSbInModel.X - DistToCenterFromBase, _panelSb.PtCenterPanelSbInModel.Y + 500, 0);
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
                     if (string.Equals(Lib.Blocks.EffectiveName(blRefTile), Album.Options.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
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
            shiftEnd = -444.5;// Торец слева - сдвинуть влево
         }
         else if (blName.IndexOf(Album.Options.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
         {
            shiftEnd = -444.5; // Торец спрва - сдвинуть вправо
         }
         return (extTiles.MaxPoint.X - extTiles.MinPoint.X)*0.5 + shiftEnd;
      }
   }
}

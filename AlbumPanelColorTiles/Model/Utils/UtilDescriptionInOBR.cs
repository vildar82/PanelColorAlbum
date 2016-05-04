using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.ExportFacade;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils
{
    public static class UtilDescriptionInOBR
    {
        private static Transaction t;
        private static ObjectId idBtrDesc;

        public static void Check()
        {
            Album album = new Album();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (t = db.TransactionManager.StartTransaction())
            {
                var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                idBtrDesc = bt["Примечание"];
                Point3d pt = Point3d.Origin;
                foreach (var idBtr in bt)
                {
                    var btr = idBtr.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    if (Panels.MarkSb.IsBlockNamePanel(btr.Name))
                    {
                        var blRefObr = findOBR(btr);
                        if (blRefObr == null) continue;
                        //var blRef = new BlockReference(pt, blRefObr.BlockTableRecord);
                        //ms.AppendEntity(blRef);
                        //t.AddNewlyCreatedDBObject(blRef, true);
                        //pt = new Point3d(pt.X, pt.Y-7000,0);
                        PanelBtrExport btrPanel = new PanelBtrExport(idBtr, null);
                        btrPanel.iterateEntInBlock(btr, false);
                        ContourPanel contour = new ContourPanel(btrPanel);
                        var pl3dId = contour.CreateContour2(btr);
                        var pl3d = pl3dId.GetObject(OpenMode.ForWrite);
                        pl3d.Erase();
                        var ptDesc = btrPanel.ExtentsNoEnd.Center();
                        int shiftEnd = getShiftEnds(btr.Name);
                        ptDesc = new Point3d(ptDesc.X + 313+shiftEnd, -2218, 0);
                        insertDesc(blRefObr, ptDesc);
                    }
                }
                t.Commit();
            }
        }

        private static int getShiftEnds(string name)
        {
            int res = 0;  
            if (name.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
            {
                res = -700;
            }
            if (name.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
            {
                res = 700;
            }
            return res; 
        }    

        private static void insertDesc(BlockReference blRefObr, Point3d ptDesc)
        {
            
            var blRefDesc = new BlockReference(ptDesc, idBtrDesc);
            blRefDesc.SetDatabaseDefaults();
            var btrObr = blRefObr.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            btrObr.AppendEntity(blRefDesc);
            t.AddNewlyCreatedDBObject(blRefDesc, true);
        }

        private static BlockReference findOBR(BlockTableRecord btr)
        {
            foreach (var idEnt in btr)
            {
                var blRefObr = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                if (blRefObr == null) continue;
                if (blRefObr.Name.StartsWith("ОБР_", StringComparison.OrdinalIgnoreCase))
                {
                    var btrObr = blRefObr.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    foreach (var item in btrObr)
                    {
                        if (item.ObjectClass.Name != "AcDbMText") continue;
                        var mtextDesc = item.GetObject(OpenMode.ForRead, false, true) as MText;
                        if (mtextDesc.Text.StartsWith("Примечан"))
                        {
                            mtextDesc.UpgradeOpen();
                            mtextDesc.Erase();
                            return blRefObr;
                        }
                    }
                    break;
                }
            }
            return null;
        }
    }
}

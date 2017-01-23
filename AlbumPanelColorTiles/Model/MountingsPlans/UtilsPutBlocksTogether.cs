using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;
using AcadLib.Jigs;
using Autodesk.AutoCAD.Geometry;
using AlbumPanelColorTiles.PanelLibrary;

namespace AlbumPanelColorTiles.MountingsPlans
{
    /// <summary>
    /// Собрать блоки в одну точку
    /// </summary>
    public static class UtilsPlanBlocksTogether
    {        
        public static void Together(IEnumerable<ObjectId> ids = null)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;            

            // Выбор блоков
            if (ids == null)
            {
                ids = ed.SelectBlRefs("\nВыбор блоков:").ToArray();
            }
            //// Точка соединения всех блоков (точка вставки).            
            //var ptInsert = ed.GetPointWCS("\nТочка вставки:");

            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                var mountBlRefsBySections = new List<FloorMounting>();
                AcadLib.Layers.LayerExt.CheckLayerState(SymbolUtilityServices.LayerZeroName);
                foreach (var idBlRef in ids)
                {
                    if (!idBlRef.IsValidEx()) continue;
                    var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;                    
                    if (blRef.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName))
                    {
                        var floorMount = new FloorMounting(blRef, null);
                        floorMount.DefineStorey(null);
                        mountBlRefsBySections.Add(floorMount);
                    }
                }

                // Группировка монтажных планов по x
                var idsBlRefMount = new List<ObjectId>();
                var groupMountPlansByX = mountBlRefsBySections.GroupBy(g => g.PosBlMounting.X, new AcadLib.Comparers.DoubleEqualityComparer(30000));
                var pt = Point3d.Origin;
                foreach (var mountsByX in groupMountPlansByX)
                {                    
                    foreach (var mount in mountsByX)
                    {
                        // вставка нового вхождения этого блока                                 
                        var blRefNew = new BlockReference(pt, mount.IdBtrMounting);
                        blRefNew.Layer = SymbolUtilityServices.LayerZeroName;
                        cs.AppendEntity(blRefNew);
                        t.AddNewlyCreatedDBObject(blRefNew, true);
                        idsBlRefMount.Add(blRefNew.Id);
                    }
                    pt = new Point3d(pt.X + mountsByX.Max(m => m.Extents.Diagonal()) + 50000, pt.Y, pt.Z);
                }
                ed.Drag(idsBlRefMount.ToArray(), Point3d.Origin);

                t.Commit();
            }        
        }
    }
}

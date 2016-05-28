using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils
{
    /// <summary>
    /// Удаление описания из блоков панелей в чертеже
    /// </summary>
    public static class UtilsRemoveDescrInObr
    {        
        public static void Remove()
        {            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (var t = db.TransactionManager.StartTransaction())
            {
                var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;                
                
                foreach (var idBtr in bt)
                {
                    var btr = idBtr.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    if (btr.Name.StartsWith("ОБР", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var idEnt in btr)
                        {
                            var ent = idEnt.GetObject(OpenMode.ForRead, false, true);
                            if (ent is MText)
                            {
                                var mtext = (MText)ent;
                                if (mtext.Text.StartsWith("Швы вертикальные", StringComparison.OrdinalIgnoreCase) ||
                                    mtext.Text.StartsWith("Примечани", StringComparison.OrdinalIgnoreCase))
                                {
                                    mtext.UpgradeOpen();
                                    mtext.Erase();
                                }                                
                            }
                            else if (ent is BlockReference)
                            {
                                var blRef = (BlockReference)ent;
                                var blName = blRef.GetEffectiveName();
                                if (blName.StartsWith("Примечани", StringComparison.OrdinalIgnoreCase))
                                {
                                    blRef.UpgradeOpen();
                                    blRef.Erase();
                                }                                
                            }
                        }
                    }
                }
                t.Commit();
            }
        }
    }
}
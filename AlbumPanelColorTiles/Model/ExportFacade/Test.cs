using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
    public class Test
    {
        public static void PrintPltilesPanel(List<Polyline> colPlTile)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
            {
                using (var ms = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord)
                {
                    foreach (var pl in colPlTile)
                    {
                        ms.AppendEntity(pl);
                        db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(pl, true);
                    }
                }
            }
        }
    }
}
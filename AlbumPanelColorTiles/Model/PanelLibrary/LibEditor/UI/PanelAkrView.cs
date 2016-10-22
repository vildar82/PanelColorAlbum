using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using MicroMvvm;
using AcadLib;

namespace AlbumPanelColorTiles.PanelLibrary.LibEditor.UI
{
    public class PanelAkrView : ObservableObject
    {
        public PanelAKR PanelAkr { get; set; }

        public PanelAkrView(PanelAKR panel)
        {
            PanelAkr = panel;            
        }

        public string Name { get { return PanelAkr.MarkAkr; } }
        public ImageSource Image { get { return PanelAkr.Image; } }
        public double Height { get { return PanelAkr.HeightPanelByTile; } }
        public string Description {
            get { return PanelAkr.Description; }
            set {
                PanelAkr.Description = value;
                RaisePropertyChanged();
            }
        }

        public void Insert ()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            using (doc.LockDocument())
            {
                Database db = doc.Database;
                using (var t = db.TransactionManager.StartTransaction())
                {
                    CopyEnt(PanelAkr.IdBtrAkrPanel, db.BlockTableId);                    
                    AcadLib.Blocks.BlockInsert.Insert(PanelAkr.BlName);
                    t.Commit();
                }
            }
        }

        public static ObjectId CopyEnt (ObjectId idEnt, ObjectId idBtrOwner)
        {
            Database destDb = idBtrOwner.Database;
            ObjectId resId = ObjectId.Null;
            using (IdMapping map = new IdMapping())
            {
                using (var ids = new ObjectIdCollection(new ObjectId[] { idEnt }))
                {
                    destDb.WblockCloneObjects(ids, destDb.BlockTableId, map,  DuplicateRecordCloning.Ignore, false);
                    resId = map[idEnt].Value;
                }
            }
            return resId;
        }
    }
}

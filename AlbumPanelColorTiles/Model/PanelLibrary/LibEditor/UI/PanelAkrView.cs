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
        private PanelAKR panelAkr;

        public PanelAkrView(PanelAKR panel)
        {
            panelAkr = panel;            
        }

        public string Name { get { return panelAkr.BlName; } }
        public ImageSource Image { get { return panelAkr.Image; } }
        public double Height { get { return panelAkr.HeightPanelByTile; } }
        public string Description {
            get { return panelAkr.Description; }
            set {
                panelAkr.Description = value;
                RaisePropertyChanged();
            }
        }

        public void Insert ()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (var t = db.TransactionManager.StartTransaction())
            {
                CopyEnt(panelAkr.IdBtrAkrPanel, db.BlockTableId);                
                AcadLib.Blocks.BlockInsert.Insert(panelAkr.BlName);                
                t.Commit();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Select;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.Navigator
{
    public static class UtilsSelectPanelsByHeight
    {
        public static void ShowPanelsByHeight()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            List<PanelAkrFacade> panelsAkr;
            using (var t = db.TransactionManager.StartTransaction())
            {                
                panelsAkr =PanelLibrarySaveService.GetPanelsAkrInDb(db);
                foreach (var item in panelsAkr)
                {
                    item.DefineGeom(item.IdBtrAkrPanel);
                }
                t.Commit();
            }

            ViewNavigator model = new ViewNavigator (panelsAkr);
            WindowNavigator win = new WindowNavigator (model);
            Application.ShowModelessWindow(win);
        }
    }
}

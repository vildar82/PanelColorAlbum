using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.EditorInput;
using MicroMvvm;

namespace AlbumPanelColorTiles.Utils.Navigator
{
    public class ViewPanel : ObservableObject
    {
        PanelAkrFacade panel;        

        public ViewPanel(PanelAkrFacade panel)
        {
            this.panel = panel;
        }

        public string Name {
            get { return panel.MarkAkr; }
        }

        public double Height {
            get { return panel.HeightPanelByTile; }
        }        
    }
}

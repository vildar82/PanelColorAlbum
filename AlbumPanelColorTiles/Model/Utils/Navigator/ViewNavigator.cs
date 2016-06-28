using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AlbumPanelColorTiles.PanelLibrary;
using MicroMvvm;

namespace AlbumPanelColorTiles.Utils.Navigator
{
    public class ViewNavigator : ObservableObject
    {
        public ObservableCollection<ViewPanel> Panels { get; set; }

        public ViewPanel SelectedPanel { get; set; }
        public ViewNavigator(List<PanelAkrFacade> panels)
        {
            Panels = new ObservableCollection<ViewPanel>();
            foreach (var item in panels)
            {
                Panels.Add(new ViewPanel(item));
            }
        }        
    }
}

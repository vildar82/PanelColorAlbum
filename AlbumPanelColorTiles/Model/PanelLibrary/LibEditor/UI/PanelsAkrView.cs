using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using MicroMvvm;

namespace AlbumPanelColorTiles.PanelLibrary.LibEditor.UI
{
    public class PanelsAkrView : ObservableObject
    {
        private List<PanelAKR> panelsAkr;
        private string filter;        

        public PanelsAkrView(List<PanelAKR> panels)
        {
            panelsAkr = panels;
            FillPanels(panels);
        }

        public ObservableCollection<PanelAkrView> Panels { get; set; } = new ObservableCollection<PanelAkrView>();
        public PanelAkrView SelectedPanel { get; set; }
        public string Filter {
            get { return filter; }
            set {
                filter = value;
                FilterPanels(filter);
                RaisePropertyChanged();
            }
        }

        public ICommand Insert { get { return new RelayCommand(() => SelectedPanel.Insert(), () => SelectedPanel != null); } }

        private void FillPanels (IEnumerable<PanelAKR> panels)
        {
            Panels.Clear();
            foreach (var item in panels)
            {
                PanelAkrView panelView = new PanelAkrView(item);
                Panels.Add(panelView);
            }
        }

        private void FilterPanels (string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                FillPanels(panelsAkr);
                return;
            }
            var filteredPanels = panelsAkr.Where(p => isPassFilter(p.BlName, filter));
            FillPanels(filteredPanels);
        }

        private bool isPassFilter (string blName, string filter)
        {
            var res = Regex.IsMatch(blName, filter, RegexOptions.IgnoreCase);
            return res;
        }
    }
}

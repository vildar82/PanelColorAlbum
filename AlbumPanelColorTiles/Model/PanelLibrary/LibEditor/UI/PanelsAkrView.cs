using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MicroMvvm;

namespace AlbumPanelColorTiles.PanelLibrary.LibEditor.UI
{
    public class PanelsAkrView : ObservableObject
    {
        private List<PanelAKR> panelsAkr;
        private string filter;
        private Visibility visible;
        private LibraryEditor libEditor;
        private List<PanelAKR> panelsToDelete= new List<PanelAKR> ();

        public PanelsAkrView(List<PanelAKR> panels, LibraryEditor libEditor)
        {
            this.libEditor = libEditor;
            panelsAkr = panels;
            var alphaComparer = AcadLib.Comparers.AlphanumComparator.New;           
            panelsAkr.Sort((p1, p2) => alphaComparer.Compare(p1.MarkAkr,p2.MarkAkr));
            FillPanels(panels);

            
            Insert = new RelayCommand(OnInsertExecute, OnInsertCanExecute);
            Delete = new RelayCommand<PanelAkrView>(OnDeleteExecute);
            UndoDelete = new RelayCommand(OnUndoDeleteExecute, OnUndoDeleteCanExecute);
        }

        public ICommand Insert { get; set; }
        public ICommand Delete { get; set; }
        public ICommand UndoDelete { get; set; }        

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

        public Visibility Visible {
            get { return visible; }
            set {
                visible = value;
                RaisePropertyChanged();
            }
        }

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
            var filteredPanels = panelsAkr.Where(p => isPassFilter(p.MarkAkr, filter));
            FillPanels(filteredPanels);
        }

        private bool isPassFilter (string name, string filter)
        {
            var res = Regex.IsMatch(name, filter, RegexOptions.IgnoreCase);
            return res;
        }

        private void OnInsertExecute ()
        {
            Visible = Visibility.Hidden;
            SelectedPanel.Insert();
            Visible = Visibility.Visible;
        }
        private bool OnInsertCanExecute ()
        {
            return SelectedPanel != null;
        }

        private void OnDeleteExecute (PanelAkrView panelView)
        {            
            panelsToDelete.Add(panelView.PanelAkr);            
            Panels.Remove(panelView);
            panelsAkr.Remove(panelView.PanelAkr);
        }

        private void OnUndoDeleteExecute ()
        {
            foreach (var item in panelsToDelete)
            {
                panelsAkr.Add(item);
                Panels.Add(new PanelAkrView(item));
            }
            panelsToDelete.Clear();
        }

        private bool OnUndoDeleteCanExecute ()
        {
            return panelsToDelete.Count > 0;
        }

        public void OnClose (object sender, EventArgs e)
        {
            libEditor.CloseAndDelete(panelsToDelete);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlbumPanelColorTiles.PanelLibrary.LibEditor.UI
{
    /// <summary>
    /// Логика взаимодействия для PanelsWindow.xaml
    /// </summary>
    public partial class PanelsWindow : Window
    {
        public PanelsWindow (PanelsAkrView view)
        {
            InitializeComponent();
            Closed += view.OnClose;
            DataContext = view;            
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlbumPanelColorTiles.PanelLibrary;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public partial class FormPanelAkrList : Form
   {
      private List<PanelAkrFacade> _selectedPanels;

      public FormPanelAkrList(List<PanelAkrFacade> panels)
      {
         _selectedPanels = new List<PanelAkrFacade>();
         InitializeComponent();
         listBoxPanels.DataSource = panels;
      }

      public List<PanelAkrFacade> SelectedPanels { get { return _selectedPanels; } }

      private void buttonAdd_Click(object sender, EventArgs e)
      {
         if (listBoxPanels.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбраны панели");
            DialogResult = DialogResult.None;
            return;
         }
         _selectedPanels = listBoxPanels.SelectedItems.Cast<PanelAkrFacade>().ToList();
      }
   }
}

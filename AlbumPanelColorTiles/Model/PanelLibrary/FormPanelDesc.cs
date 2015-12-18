using System;
using System.Windows.Forms;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public partial class FormPanelDesc : Form
   {
      private PanelAKR _panel;

      public FormPanelDesc(PanelAKR panel)
      {
         _panel = panel;
         InitializeComponent();
         textBoxDesc.Text = panel.Description;
         label1.Text = "Примечание к панели " + panel.BlName;
      }

      private void buttonOk_Click(object sender, EventArgs e)
      {
         _panel.Description = textBoxDesc.Text;
      }
   }
}
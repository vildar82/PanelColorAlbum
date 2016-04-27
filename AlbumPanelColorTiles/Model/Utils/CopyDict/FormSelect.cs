using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlbumPanelColorTiles.Utils.CopyDict
{
    public partial class FormSelect : Form
    {
        public SelectObject SelectedItem { get; private set; }

        public FormSelect(List<SelectObject> data)
        {
            InitializeComponent();
            listBoxSelect.DataSource = data;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (listBoxSelect.SelectedItem == null)
            {
                DialogResult = DialogResult.None;
                MessageBox.Show("Ничего не выбрано!");
            }
            else
            {
                SelectedItem = (SelectObject)listBoxSelect.SelectedItem;
            }
        }
    }
}

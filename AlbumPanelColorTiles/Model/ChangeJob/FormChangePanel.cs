using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;

namespace AlbumPanelColorTiles.ChangeJob
{
    public partial class FormChangePanel : Form
    {
        public FormChangePanel()
        {
            InitializeComponent();
            UpdateData();
        }

        public void UpdateData ()
        {
            listBoxChangePanels.DataSource = null;
            listBoxChangePanels.DataSource = ChangeJobService.ChangePanels;
        }

        private void listBoxChangePanels_SelectedIndexChanged(object sender, EventArgs e)
        {
            var changePanel = listBoxChangePanels.SelectedItem as ChangePanel;
            if (changePanel == null)
            {
                textBoxAkrMark.Text = "";
                textBoxAkrPaintNew.Text = "";
                textBoxMountMark.Text = "";
                textBoxMountPaintOld.Text = "";
            }
            else
            {
                textBoxAkrMark.Text = changePanel.PanelAKR.MarkAr.MarkSB.MarkSbName;
                textBoxAkrPaintNew.Text = changePanel.PaintNew;
                textBoxMountMark.Text = changePanel.PanelMount.MarkSb;
                textBoxMountPaintOld.Text =changePanel.PaintOld;
            }
        }

        private void buttonAkrShow_Click(object sender, EventArgs e)
        {
            ShowPanel(true);
        }
        private void buttonMountShow_Click(object sender, EventArgs e)
        {
            ShowPanel(false);
        }

        public void ShowPanel(bool showAkr)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var chPanel = listBoxChangePanels.SelectedItem as ChangePanel;
            if (chPanel == null)
            {
                MessageBox.Show("Не выбрана панель в списке.");
                return;
            }
            Extents3d ext;
            ObjectId idEnt;
            if (showAkr)
            {
                ext = chPanel.PanelAKR.Extents;
                idEnt = chPanel.PanelAKR.IdBlRefAr;
            }
            else
            {
                ext = chPanel.PanelMount.ExtTransToModel;
                idEnt = chPanel.PanelMount.IdBlRef;
            }
            idEnt.ShowEnt(ext, doc);                
        }

        public void SetModaless()
        {
            buttonOk.Enabled = false;
            buttonOk.Visible = false;
        }

        private void buttonExeption_Click(object sender, EventArgs e)
        {
            var chPanel = listBoxChangePanels.SelectedItem as ChangePanel;
            if (chPanel == null)
            {
                MessageBox.Show("Не выбрана панель в списке.");
                return;
            }
            ChangeJobService.ExcludePanel(chPanel);
            UpdateData();
        }
    }
}

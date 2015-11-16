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
using Autodesk.AutoCAD.ApplicationServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public partial class FormSavePanelsToLib : Form
   {
      private Document _doc;
      private List<PanelAKR> _panelsNew;
      private List<PanelAKR> _panelsChanged;
      private List<PanelAKR> _panelsForce;
      private List<PanelAKR> _panelsOtherInFacade;
      private ListBox _curListBox;

      public FormSavePanelsToLib(List<PanelAKR> panelsNew, List<PanelAKR> panelsChanged, List<PanelAKR> panelsOtherInFacade)
      {
         _doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
         _panelsNew = panelsNew;
         _panelsChanged = panelsChanged;
         _curListBox = listBoxNew;
         _panelsOtherInFacade = panelsOtherInFacade;
         _panelsForce = new List<PanelAKR>();         

         InitializeComponent();

         listBoxNew.DataSource = _panelsNew;
         listBoxChanged.DataSource = _panelsChanged;
         listBoxForce.DataSource = _panelsForce;
      }

      private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
      {
         int index = tabControl.SelectedIndex;
         buttonShowInLib.Visible = index != 1; // Для новых паненелей отключить кнопку показа в библиотеке
         buttonAdd.Visible = index == 2; // Добавить панель в список можно только на вкладке Принудительно

         switch (index)
         {
            case 0:
               _curListBox = listBoxNew;
               break;
            case 1:
               _curListBox = listBoxChanged;
               break;
            case 2:
               _curListBox = listBoxForce;
               break;
            default:
               _curListBox = null;
               break;
         }
      }

      private void buttonShow_Click(object sender, EventArgs e)
      {
         // Показать панель в текущем чертеже
         activateDoc();
         if (_curListBox.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбрана панель");
            return;
         }
         PanelAKR panel = (PanelAKR)_curListBox.SelectedItem;
         panel.ShowPanelInFacade(_doc);
      }     

      private void buttonDel_Click(object sender, EventArgs e)
      {
         if (_curListBox.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбрана панель");
            return;
         }
         PanelAKR panel = (PanelAKR)_curListBox.SelectedItem;
         List<PanelAKR> panelsList = (List<PanelAKR>)_curListBox.DataSource;
         panelsList.Remove(panel);
      }

      private void activateDoc()
      {
         // проверка активного документа - фасада
         if (!_doc.IsActive)
         {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = _doc;
         }
      }

      private void buttonAdd_Click(object sender, EventArgs e)
      {
         FormPanelAkrList formPanels = new FormPanelAkrList(_panelsOtherInFacade);
         if (formPanels.ShowDialog() == DialogResult.OK)
         {
            _panelsForce.AddRange(formPanels.SelectedPanels);
            foreach (var panelAdded in formPanels.SelectedPanels)
            {
               _panelsOtherInFacade.Remove(panelAdded);
            }
         }
      }

      private void buttonShowInLib_Click(object sender, EventArgs e)
      {
         if (_curListBox.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбрана панель");
            return;
         }
         PanelAKR panel = (PanelAKR)_curListBox.SelectedItem;
         // Открыть новый чертеж, вставить блок панели, покзать.
         Document docNew = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Add("");

         AcadLib.Blocks.Block.CopyBlockFromExternalDrawing()

         // Копирование блока в новый чертеж из библиотеки
         Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = docNew;
      }
   }
}

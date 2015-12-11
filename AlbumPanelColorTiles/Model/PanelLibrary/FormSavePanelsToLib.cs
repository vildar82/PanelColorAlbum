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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public partial class FormSavePanelsToLib : Form
   {
      private Document _doc;
      private List<PanelAkrFacade> _panelsNew;
      private List<PanelAkrFacade> _panelsChanged;
      private List<PanelAkrFacade> _panelsForce;
      // оставшиеся панели на фасаде
      private List<PanelAkrFacade> _panelsOtherInFacade;
      private List<PanelAkrFacade> _panelsToSave;
      private ListBox _curListBox;

      public List<PanelAkrFacade> PanelsToSave { get { return _panelsToSave; } }

      public FormSavePanelsToLib(List<PanelAkrFacade> panelsNew, List<PanelAkrFacade> panelsChanged, List<PanelAkrFacade> panelsOtherInFacade)
      {
         _doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
         _panelsNew = panelsNew;
         _panelsChanged = panelsChanged;         
         _panelsOtherInFacade = panelsOtherInFacade;
         _panelsForce = new List<PanelAkrFacade>();         

         InitializeComponent();

         refreshDataSource();
         _curListBox = listBoxNew;
      }

      private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
      {
         int index = tabControl.SelectedIndex;
         buttonShowInLib.Visible = false;// index != 0; // Для новых паненелей отключить кнопку показа в библиотеке
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
         PanelAkrFacade panel = (PanelAkrFacade)_curListBox.SelectedItem;
         panel.ShowPanelInFacade(_doc);
      }     

      private void buttonDel_Click(object sender, EventArgs e)
      {
         if (_curListBox.SelectedItems== null)
         {
            MessageBox.Show("Не выбраны панели");
            return;
         }         
         var panels = _curListBox.SelectedItems.Cast<PanelAkrFacade>().ToList();
         List<PanelAkrFacade> panelsList = (List<PanelAkrFacade>)_curListBox.DataSource;
         foreach (var item in panels)
         {
            panelsList.Remove(item);
            _panelsOtherInFacade.Add(item);
         }         
         refreshDataSource();
      }

      private void activateDoc()
      {
         // проверка активного документа - фасада
         if (!_doc.IsActive)
         {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = _doc;
         }
      }

      private void buttonSave_Click(object sender, EventArgs e)
      {
         _panelsToSave = new List<PanelAkrFacade>();
         _panelsToSave.AddRange(_panelsNew);
         _panelsToSave.AddRange(_panelsChanged);
         _panelsToSave.AddRange(_panelsForce);
      }

      private void buttonDesc_Click(object sender, EventArgs e)
      {
         if (_curListBox.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбрана панель");
            return;
         }
         PanelAkrFacade panel = (PanelAkrFacade)_curListBox.SelectedItem;
         FormPanelDesc formPanelDesc = new FormPanelDesc(panel);
         if (formPanelDesc.ShowDialog() == DialogResult.OK)
         {
            refreshDataSource();
         }
      }

      private void refreshDataSource()
      {
         listBoxNew.DataSource = null;
         listBoxChanged.DataSource = null;
         listBoxForce.DataSource = null;
         listBoxNew.DataSource = _panelsNew;
         listBoxChanged.DataSource = _panelsChanged;
         listBoxForce.DataSource = _panelsForce;
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
               if (panelAdded.ReportStatus == EnumReportStatus.Other)
               {
                  panelAdded.ReportStatus = EnumReportStatus.Force;
               }
            }
            refreshDataSource();
         }
      }

      private void buttonShowInLib_Click(object sender, EventArgs e)
      {
         if (_curListBox.SelectedIndex == -1)
         {
            MessageBox.Show("Не выбрана панель");
            return;
         }          
         PanelAkrFacade panelAkrFacade = (PanelAkrFacade)_curListBox.SelectedItem;
         // Открыть новый чертеж, вставить блок панели, покзать.
         Document docNew = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.Add("Acadiso.dwt");         
         Database dbNew = docNew.Database;
         Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = docNew;
         // Копирование блока в новый чертеж из библиотеки
         ObjectId idBtr = ObjectId.Null;
         try
         {
            idBtr = AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(panelAkrFacade.BlName, PanelLibrarySaveService.LibPanelsFilePath,
                                                             dbNew, DuplicateRecordCloning.Replace);
         }
         catch (Exception ex)
         {
            docNew.CloseAndDiscard();
            MessageBox.Show("Неудалось скопировать блок из библиотеки в новый чертеж для показа.");
            Log.Error(ex, "buttonShowInLib_Click");
         }
         if (!idBtr.IsNull && idBtr.IsValid)
         {
            using (var cs = dbNew.CurrentSpaceId.Open(OpenMode.ForWrite) as BlockTableRecord)
            {
               var blRefPanelAkrLib = new BlockReference(Point3d.Origin, idBtr);
               cs.AppendEntity(blRefPanelAkrLib);
               docNew.Editor.ZoomExtents();
            }
         }
      }      
   }
}

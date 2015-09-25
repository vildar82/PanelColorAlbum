using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Model.Forms
{
   public partial class FormRenameMarkAR : Form
   {      
      private Dictionary<string, MarkArRename> _marksArForRename;
      BindingSource _bindingsMarksArRename;

      public FormRenameMarkAR(Album album)
      {
         InitializeComponent();         
         _marksArForRename = MarkArRename.GetMarks(album);
         // Сортировка панелей.
         _marksArForRename = _marksArForRename.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

         _bindingsMarksArRename = new BindingSource();
         _bindingsMarksArRename.DataSource = _marksArForRename.Values;

         listBoxMarksAR.DataSource = _bindingsMarksArRename;
         listBoxMarksAR.DisplayMember = "MarkArCurFull";
         textBoxOldMarkAR.DataBindings.Add("Text", _bindingsMarksArRename, "MarkPainting", false, DataSourceUpdateMode.OnPropertyChanged);         
      }     

      public List<MarkArRename> RenamedMarksAr()
      {
         List<MarkArRename> renamedMarks = new List<MarkArRename>();
         foreach (var markArRename in _marksArForRename.Values)
         {
            if (markArRename.IsRenamed)
            {
               renamedMarks.Add(markArRename);
            }
         }
         return renamedMarks;
      }

      private void textBoxNewMark_TextChanged(object sender, EventArgs e)
      {
         labelPreview.Text = getMarkArPreview();
      }

      private string getMarkArPreview()
      {
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null) return "";
         return  markArForRename.GetMarkArPreview(textBoxNewMark.Text);         
      }

      private void buttonRename_Click(object sender, EventArgs e)
      {
         ClearErrors();
         string newPaintingMark = textBoxNewMark.Text;
         if (string.IsNullOrWhiteSpace(newPaintingMark))
         {            
            errorProviderError.SetError(textBoxNewMark, "Пустое имя!");
            return;
         }
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null)
         {
            errorProviderError.SetError(buttonShow, "Не выбрана панель в списке.");
            return;
         }
         string markArOld = markArForRename.MarkArCurFull;
         string markArNew = markArForRename.GetMarkArPreview(newPaintingMark);

         // Проверка новаой марки            
         if (_marksArForRename.ContainsKey(markArNew))
         {
            MessageBox.Show("Панель с такой маркой уже есть. Переименование отклонено.",
               string.Format("{0} в {1}", markArOld, markArNew), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            errorProviderError.SetError(textBoxNewMark, "Панель с такой маркой уже есть.");
         }
         else
         {            
            markArForRename.RenamePainting(newPaintingMark);
            _marksArForRename.Remove(markArForRename.MarkArCurFull);
            _marksArForRename.Add(markArForRename.MarkArCurFull, markArForRename);            
            _bindingsMarksArRename.ResetBindings(false);
            errorProviderOk.SetError(textBoxNewMark, "Панель переименована.");
         }
      }

      private void buttonShow_Click(object sender, EventArgs e)
      {
         ZoomPanel();
      }
      private void listBoxMarksAR_DoubleClick(object sender, EventArgs e)
      {
         ZoomPanel();
      }

      private void listBoxMarksAR_SelectedIndexChanged(object sender, EventArgs e)
      {
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null) return;
         labelPreview.Text = getMarkArPreview();
         ClearErrors();
      }

      private void ClearErrors()
      {
         errorProviderError.Clear();
         errorProviderOk.Clear();
      }

      private void ZoomPanel()
      {         
         // Приблизить блок панели на чертеже
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null)
         {
            errorProviderError.SetError(buttonShow, "Не выбрана панель в списке.");
            return;
         }
         Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
         Editor ed = doc.Editor;
         ed.Zoom(markArForRename.MarkAR.Panels[0].Extents);
         errorProviderOk.SetError(buttonShow, string.Format("Блок панели показан - {0}", markArForRename.MarkArCurFull));
      }      
   }
}


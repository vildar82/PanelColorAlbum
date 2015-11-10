using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.RenamePanels
{
   public partial class FormRenameMarkAR : Form
   {
      private BindingSource _bindingsMarksArRename;
      private Dictionary<string, MarkArRename> _marksArForRename;

      public FormRenameMarkAR(Album album)
      {
         InitializeComponent();
         _marksArForRename = MarkArRename.GetMarks(album);
         // загрузка сохраненных переименований
         DictNOD.LoadRenameMarkArFromDict(ref _marksArForRename);
         // Сортировка панелей.
         _marksArForRename = _marksArForRename.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

         _bindingsMarksArRename = new BindingSource();
         _bindingsMarksArRename.DataSource = _marksArForRename.Values;

         listBoxMarksAR.DataSource = _bindingsMarksArRename;
         listBoxMarksAR.DisplayMember = "MarkArCurFull";
         textBoxOldMarkAR.DataBindings.Add("Text", _bindingsMarksArRename, "MarkAR.MarkPaintingCalulated", false, DataSourceUpdateMode.OnPropertyChanged);
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
         //string markArOld = markArForRename.MarkArCurFull;
         //string markArNew = markArForRename.GetMarkArPreview(newPaintingMark);

         // Проверка новаой марки
         //if (_marksArForRename.ContainsKey(markArNew))
         if (MarkArRename.ContainsRenamePainting(markArForRename, newPaintingMark, _marksArForRename))
         {
            MessageBox.Show("Панель с такой маркой уже есть. Переименование отклонено.");//, string.Format("{0} в {1}", markArOld, markArNew), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            errorProviderError.SetError(textBoxNewMark, "Панель с такой маркой уже есть.");
         }
         else
         {
            markArForRename.RenameMark(newPaintingMark, _marksArForRename);
            //markArForRename.RenamePainting(newPaintingMark);
            //_marksArForRename.Remove(markArForRename.MarkArCurFull);
            //_marksArForRename.Add(markArForRename.MarkArCurFull, markArForRename);
            _bindingsMarksArRename.ResetBindings(false);
            errorProviderOk.SetError(textBoxNewMark, "Панель переименована.");
         }
      }

      private void buttonShow_Click(object sender, EventArgs e)
      {
         ZoomPanel();
      }

      private void ClearErrors()
      {
         errorProviderError.Clear();
         errorProviderOk.Clear();
      }

      private void FormRenameMarkAR_KeyUp(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Escape) this.Close();
      }

      private string getMarkArPreview()
      {
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null) return "";
         return markArForRename.GetMarkArPreview(textBoxNewMark.Text);
      }

      private void listBoxMarksAR_DoubleClick(object sender, EventArgs e)
      {
         ZoomPanel();
      }

      private void listBoxMarksAR_DrawItem(object sender, DrawItemEventArgs e)
      {
         e.DrawBackground();
         MarkArRename markArForRename = ((ListBox)sender).Items[e.Index] as MarkArRename;
         // Покраска
         if (markArForRename.IsRenamed)
         {
            if (listBoxMarksAR.SelectedIndex == e.Index)
            {
               e.Graphics.FillRectangle(System.Drawing.Brushes.Green, e.Bounds);
            }
            else
            {
               e.Graphics.FillRectangle(System.Drawing.Brushes.LightGreen, e.Bounds);
            }
         }
         else
         {
            //e.Graphics.FillRectangle( System.Drawing.Brushes., e.Bounds);
         }
         // Текст
         e.Graphics.DrawString(markArForRename.MarkArCurFull, ((Control)sender).Font, System.Drawing.Brushes.Black, e.Bounds.X, e.Bounds.Y);
      }

      private void listBoxMarksAR_SelectedIndexChanged(object sender, EventArgs e)
      {
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         if (markArForRename == null) return;
         labelPreview.Text = getMarkArPreview();
         if (markArForRename.IsRenamed)
         {
            textBoxNewMark.Text = markArForRename.MarkPainting;
         }
         ClearErrors();
      }

      private void textBoxNewMark_TextChanged(object sender, EventArgs e)
      {
         labelPreview.Text = getMarkArPreview();
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
         // Определение границ по плитке
         var panel = markArForRename.MarkAR.Panels[0];
         var ext = panel.GetExtentsTiles(markArForRename.MarkAR.MarkSB);
         ed.Zoom(ext);
         errorProviderOk.SetError(buttonShow, string.Format("Блок панели показан - {0}", markArForRename.MarkArCurFull));
      }
   }
}
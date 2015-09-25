using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
         _marksArForRename = _marksArForRename.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

         _bindingsMarksArRename = new BindingSource();
         _bindingsMarksArRename.DataSource = _marksArForRename.Values;

         listBoxMarksAR.DataSource = _bindingsMarksArRename;
         listBoxMarksAR.DisplayMember = "MarkArCurFull";

         textBoxOldMarkAR.DataBindings.Add("Text", _bindingsMarksArRename, "MarkPainting", false, DataSourceUpdateMode.OnPropertyChanged);
         //loginTextBox.DataBindings.Add("Text", usersBindingSource, "Login", true, DataSourceUpdateMode.OnPropertyChanged);

         //listBoxMarksAR.DataSource = null;
         //listBoxMarksAR.DataSource = _marksArForRename;
         //listBoxMarksAR.DisplayMember = "MarkArCurFull";                  
      }

      //private void listBoxMarksAR_SelectedIndexChanged(object sender, EventArgs e)
      //{
      //   MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
      //   textBoxOldMarkAR.Text = markArForRename.MarkArCurFull; 
      //}

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
         return  markArForRename.GetMarkArPreview(textBoxNewMark.Text);
         //return string.Format("{0}({1}_{2})", markAR.MarkSB.MarkSb, textBoxNewMark.Text, _album.AbbreviateProject);
      }

      private void buttonRename_Click(object sender, EventArgs e)
      {
         MarkArRename markArForRename = listBoxMarksAR.SelectedItem as MarkArRename;
         string newPaintingMark = textBoxNewMark.Text;
         string markArOld = markArForRename.MarkArCurFull;
         string markArNew = markArForRename.GetMarkArPreview(newPaintingMark);

         // Проверка новаой марки            
         if (_marksArForRename.ContainsKey(markArNew))
         {
            MessageBox.Show("Панель с такой маркой уже есть. Переименование отклонено.",
               string.Format("{0} в {1}", markArOld, markArNew), MessageBoxButtons.OK, MessageBoxIcon.Hand);
         }
         else
         {
            //var markArRename = _marksArForRename[markArForRename.MarkArCurFull];
            markArForRename.RenamePainting(newPaintingMark);
            _marksArForRename.Remove(markArForRename.MarkArCurFull);
            _marksArForRename.Add(markArForRename.MarkArCurFull, markArForRename);
            MessageBox.Show("Панель переименована.",
               string.Format("{0} в {1}", markArOld, markArNew), MessageBoxButtons.OK, MessageBoxIcon.Information);

            _bindingsMarksArRename.ResetBindings(false);
         }
      }    
   }
}


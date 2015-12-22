using System;
using System.Windows.Forms;

namespace AlbumPanelColorTiles.Model
{
   public partial class FormStartOptions : Form
   {
      private StartOptions _startOptions;

      public FormStartOptions(StartOptions startOptions)
      {
         _startOptions = startOptions;
         InitializeComponent();
         propertyGrid1.SelectedObject = _startOptions;         
      }

      private void buttonDefault_Click(object sender, EventArgs e)
      {
         propertyGrid1.ResetSelectedProperty();
      }

      private void buttonOk_Click(object sender, EventArgs e)
      {
         // Проверка значений
         if (!checkStartOptions())
         {
            DialogResult = DialogResult.None;
            return;
         }
      }

      private bool checkStartOptions()
      {
         bool isOk = true;
         // Аббревиатура проекта должна иметь допустимое имя для блоков
         if (!string.IsNullOrEmpty(_startOptions.Abbr) && !_startOptions.Abbr.IsValidDbSymbolName())
         {
            errorProvider1.SetError(propertyGrid1, "Индекс проекта должен отвечать требованиям для именования блоков.");
            isOk = false;
         }
         return isOk;
      }
   }
}
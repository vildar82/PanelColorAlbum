using System;
using System.Windows.Forms;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles.RandomPainting
{
   public partial class FormColorAreaSize : Form
   {
      public FormColorAreaSize(int lenghtSpot, int heightSpot, bool chess)
      {
         InitializeComponent();
         errorProviderError.Icon = Properties.Resources.errorProviderError;
         textBoxHeight.Text = heightSpot.ToString();
         textBoxLenght.Text = lenghtSpot.ToString();
         checkBoxChess.Checked = chess;
      }

      public bool ChessPattern { get { return checkBoxChess.Checked; } }
      public int HeightSpot { get { return int.Parse(textBoxHeight.Text); } }
      public int LenghtSpot { get { return int.Parse(textBoxLenght.Text); } }

      private void buttonOk_Click(object sender, EventArgs e)
      {
         if (string.IsNullOrEmpty(errorProviderError.GetError(buttonOk)))
         {
            if (!string.IsNullOrEmpty(errorProviderError.GetError(textBoxHeight)) ||
               !string.IsNullOrEmpty(errorProviderError.GetError(textBoxLenght))
               )
            {
               errorProviderError.SetError(buttonOk, "Значения были откорректированы автоматически, проверьте");
               DialogResult = DialogResult.None;
            }
         }
         else
         {
            errorProviderError.Clear();
         }

         // Проверка
         bool isCheckSuccess = true;
         if (!checkValue(textBoxHeight.Text))
         {
            errorProviderError.SetError(textBoxHeight, "Должно быть целое число");
            isCheckSuccess = false;
         }
         if (!checkValue(textBoxLenght.Text))
         {
            errorProviderError.SetError(textBoxLenght, "Должно быть целое число");
            isCheckSuccess = false;
         }

         if (!isCheckSuccess)
         {
            DialogResult = DialogResult.None;
         }
      }

      private bool checkValue(string text)
      {
         int value;
         int.TryParse(text, out value);
         return value > 0;
      }

      private string correctionValue(TextBox textBox, int tileValue)
      {
         string slen = textBox.Text;
         int len;
         int.TryParse(slen, out len);
         int div = len / tileValue;
         string res = (tileValue * div).ToString();
         if (!string.Equals(slen, res, StringComparison.CurrentCultureIgnoreCase))
         {
            errorProviderError.SetError(textBox, string.Format("Введенное значение {0} откорректированно для кратности плитке", slen));
         }
         else
         {
            errorProviderError.SetError(textBox, string.Empty);
         }
         return res;
      }

      private void textBoxHeight_Leave(object sender, EventArgs e)
      {
         errorProviderError.SetError(buttonOk, string.Empty);
         textBoxHeight.Text = correctionValue(textBoxHeight, Settings.Default.TileHeight + Settings.Default.TileSeam);
      }

      private void textBoxLenght_Leave(object sender, EventArgs e)
      {
         errorProviderError.SetError(buttonOk, string.Empty);
         textBoxLenght.Text = correctionValue(textBoxLenght, Settings.Default.TileLenght + Settings.Default.TileSeam);
      }
   }
}
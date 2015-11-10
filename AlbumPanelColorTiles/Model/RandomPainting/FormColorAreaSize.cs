using System;
using System.Windows.Forms;
using AlbumPanelColorTiles.Properties;

namespace AlbumPanelColorTiles.RandomPainting
{
   public partial class FormColorAreaSize : Form
   {
      public FormColorAreaSize(int lenghtSpot, int heightSpot)
      {
         InitializeComponent();
         errorProviderError.Icon = Properties.Resources.errorProviderError;
         textBoxHeight.Text = heightSpot.ToString();
         textBoxLenght.Text = lenghtSpot.ToString();
      }

      public int HeightSpot { get { return int.Parse(textBoxHeight.Text); } }
      public int LenghtSpot { get { return int.Parse(textBoxLenght.Text); } }

      private void buttonOk_Click(object sender, EventArgs e)
      {
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

      private string correctionValue(string slen, int tileValue)
      {
         int len;
         int.TryParse(slen, out len);
         int div = len / tileValue;
         return (len * div).ToString();
      }

      private void textBoxHeight_Leave(object sender, EventArgs e)
      {
         textBoxHeight.Text = correctionValue(textBoxHeight.Text, Settings.Default.TileHeight + Settings.Default.TileSeam);
      }

      private void textBoxLenght_Leave(object sender, EventArgs e)
      {
         textBoxLenght.Text = correctionValue(textBoxLenght.Text, Settings.Default.TileLenght + Settings.Default.TileSeam);
      }
   }
}
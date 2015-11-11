using System;
using System.Drawing;
using System.Windows.Forms;
using AlbumPanelColorTiles.Properties;
using AlbumPanelColorTiles.RandomPainting;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.ImagePainting
{
   public partial class FormImageCrop : Form
   {
      private Bitmap _cropBitmap;

      private ImagePaintingService _imagePaintingService;

      private UserRect _userRect;

      public FormImageCrop(ImagePaintingService imagePaintingService)
      {
         InitializeComponent();
         _imagePaintingService = imagePaintingService;
      }

      public event EventHandler Fire = delegate { };

      public Bitmap CropBitmap { get { return _cropBitmap; } }

      private void buttonBrowse_Click(object sender, EventArgs e)
      {
         OpenFileDialog fileDia = new OpenFileDialog();
         fileDia.Filter = "Картинки | *.jpg; *.jpeg; *.png;";
         if (fileDia.ShowDialog() == DialogResult.OK)
         {
            setPictureImage(new Bitmap(fileDia.FileName));
            setUserRect();
         }
      }

      private void buttonFire_Click(object sender, EventArgs e)
      {
         try
         {
            _cropBitmap = getCropBitmap();
            if (_cropBitmap == null)
            {
               MessageBox.Show("Не определена обрезанная картинка", "АКР Покраска картинкой", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
               Fire(_cropBitmap, e);
            }
         }
         catch (Exception ex)
         {
            Log.Error(ex, "getCropBitmap()");
         }
      }

      private void buttonSelect_Click(object sender, EventArgs e)
      {
         using (EditorUserInteraction UI = _imagePaintingService.Doc.Editor.StartUserInteraction(this))
         {
            try
            {
               _imagePaintingService.PromptExtents();
               setUserRect();
            }
            catch { }
         }
      }

      private void FormImageCrop_Activated(object sender, EventArgs e)
      {
         if (pictureBoxImage.Image != null)
         {
            setUserRect();
         }
      }

      private Bitmap getCropBitmap()
      {
         Bitmap cropBitmap = null;
         try
         {
            Bitmap sourceBitmap = new Bitmap(pictureBoxImage.Image);
            cropBitmap = sourceBitmap.Clone(getImageCropRect(_userRect.rect, sourceBitmap, pictureBoxImage.Size), sourceBitmap.PixelFormat);
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.ToString());
         }
         return cropBitmap;
      }

      // Определение прямоугольника обрезки исходной картинки
      private Rectangle getImageCropRect(Rectangle rectCrop, Bitmap sourceBitmap, System.Drawing.Size sizePictureBox)
      {
         double scaleH = sourceBitmap.Height / (double)sizePictureBox.Height;
         double scaleW = sourceBitmap.Width / (double)sizePictureBox.Width;
         double scale;
         int xCorrect = 0;
         int yCorrect = 0;
         double dX = 0;
         double dY = 0;
         if (scaleH > scaleW)
         {
            scale = scaleH;
            dX = ((double)sizePictureBox.Width - (sourceBitmap.Width / scale)) * 0.5;
         }
         else
         {
            scale = scaleW;
            dY = ((double)sizePictureBox.Height - (sourceBitmap.Height / scale)) * 0.5;
         }
         xCorrect = Convert.ToInt32(((double)rectCrop.Location.X - dX) * scale);
         yCorrect = Convert.ToInt32(((double)rectCrop.Location.Y - dY) * scale);

         return new Rectangle(xCorrect, yCorrect, Convert.ToInt32(rectCrop.Width * scale), Convert.ToInt32(rectCrop.Height * scale));
      }

      private void setPictureImage(Image bitmap)
      {
         // Установка размера формы
         var proportionImage = bitmap.Width / (double)bitmap.Height;
         int maxWidthForm = Settings.Default.ImagePaintFormWidth;// 1200; // ImagePaintFormWidth
         int maxHeightForm = Settings.Default.ImagePaintFormHeight;// 950; // ImagePaintFormHeight

         double scaleW = bitmap.Width / (double)maxWidthForm;
         double scaleH = bitmap.Height / (double)maxHeightForm;
         if (scaleW > scaleH)
         {
            ClientSize = new Size(maxWidthForm, Convert.ToInt32(maxWidthForm / proportionImage));
         }
         else
         {
            ClientSize = new Size(Convert.ToInt32(maxHeightForm * proportionImage), maxHeightForm);
         }

         //// установка размера picturebox
         //int maxWidthPictureBox = Width - 40;
         //int maxHeightPictureBox = Height - 126;

         //scaleW = bitmap.Width / (double)maxWidthPictureBox;
         //scaleH = bitmap.Height / (double)maxHeightPictureBox;
         //if (scaleW > scaleH)
         //{
         //   pictureBoxImage.Width = maxWidthPictureBox;
         //   pictureBoxImage.Height = Convert.ToInt32(maxWidthPictureBox / proportionImage);
         //}
         //else
         //{
         //   pictureBoxImage.Height = maxHeightPictureBox;
         //   pictureBoxImage.Width = Convert.ToInt32(maxHeightPictureBox * proportionImage);
         //}
         pictureBoxImage.Image = bitmap;
         pictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom;
      }

      private void setUserRect()
      {
         Rectangle userRect = new Rectangle();
         if (_imagePaintingService.ColorAreaSize.Lenght / (double)pictureBoxImage.Width > _imagePaintingService.ColorAreaSize.Height / (double)pictureBoxImage.Height)
         {
            // Длина больше высоты. Задаемся макимальной длиной равной длине pictureBox
            userRect.Width = pictureBoxImage.Width;
            userRect.Height = Convert.ToInt32(pictureBoxImage.Width / _imagePaintingService.ColorAreaSize.ProportionWidthToHeight);
         }
         else
         {
            userRect.Height = pictureBoxImage.Height;
            userRect.Width = Convert.ToInt32(pictureBoxImage.Height * _imagePaintingService.ColorAreaSize.ProportionWidthToHeight);
         }
         if (_userRect == null)
         {
            _userRect = new UserRect(userRect);
            _userRect.SetPictureBox(pictureBoxImage);
         }
         else
         {
            _userRect.rect = userRect;
         }         
      }

      private void buttonSpotSize_Click(object sender, EventArgs e)
      {
         _imagePaintingService.ColorAreaSize.ChangeSize();
         setUserRect();
         this.Refresh();
      }
   }
}
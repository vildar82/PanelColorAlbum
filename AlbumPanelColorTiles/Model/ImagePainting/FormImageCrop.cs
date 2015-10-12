using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlbumPanelColorTiles.ImagePainting
{
   public partial class FormImageCrop : Form
   {
      public event EventHandler Fire = delegate { };

      private ImagePaintingService _imagePaintingService;
      private UserRect _userRect;
      private Bitmap _cropBitmap;

      public Bitmap CropBitmap { get { return _cropBitmap; } }

      public FormImageCrop(ImagePaintingService imagePaintingService)
      {
         InitializeComponent();         
         _imagePaintingService = imagePaintingService;         
      }

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

      private void setUserRect()
      {         
         Rectangle userRect = new Rectangle();
         if (_imagePaintingService.ColorAreaSize.Lenght/(double)pictureBoxImage.Width > _imagePaintingService.ColorAreaSize.Height / (double)pictureBoxImage.Height)
         {
            // Длина больше высоты. Задаемся макимальной длиной равной длине pictureBox       
            userRect.Width = pictureBoxImage.Width;
            userRect.Height = Convert.ToInt32(pictureBoxImage.Width / _imagePaintingService.ColorAreaSize.ProportionWidthToHeight);
         }
         else
         {
            userRect.Height = pictureBoxImage.Height;
            userRect.Width = Convert.ToInt32(pictureBoxImage. Height * _imagePaintingService.ColorAreaSize.ProportionWidthToHeight);
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

      private void setPictureImage(Image bitmap)
      {
         var proportionImage = bitmap.Width / (double)bitmap.Height;
         int maxWidthPictureBox = getMaxWidthPictureBox();
         int maxHeightPictureBox = getMaxWidthPictureBox();

         double scaleW = bitmap.Width / (double)maxWidthPictureBox;
         double scaleH = bitmap.Height / (double)maxHeightPictureBox;
         if (scaleW > scaleH)
         {
            pictureBoxImage.Width = maxWidthPictureBox;
            pictureBoxImage.Height = Convert.ToInt32( maxWidthPictureBox / proportionImage);
         }
         else
         {
            pictureBoxImage.Height = maxHeightPictureBox;
            pictureBoxImage.Width = Convert.ToInt32(maxHeightPictureBox * proportionImage);
         }
         pictureBoxImage.Image = bitmap;
         pictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom; 
      }

      private int getMaxWidthPictureBox()
      {
         return Width - 40;
      }
      private int getMaxHeightPictureBox()
      {
         return Height - 126;
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

      private void FormImageCrop_SizeChanged(object sender, EventArgs e)
      {
         if (pictureBoxImage.Image != null)
         {
            setPictureImage(pictureBoxImage.Image);
            setUserRect();
         }
      }

      private void FormImageCrop_Activated(object sender, EventArgs e)
      {
         if (pictureBoxImage.Image != null)
         {
            setPictureImage(pictureBoxImage.Image);
            setUserRect();
         }
      }
   }
}

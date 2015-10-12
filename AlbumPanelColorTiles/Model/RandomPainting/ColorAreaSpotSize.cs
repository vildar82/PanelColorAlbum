using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.RandomPainting
{
   // Размер ячейки зоны покраски
   public class ColorAreaSpotSize
   {
      private int _lenghtSpot;
      private int _heightSpot;
      private string _subkey;
      private Extents3d _extentsColorArea; // Вся область покраски
      private int _lenghtSize; // кол участков покраски в длине
      private int _heightSize; // кол участков покраски в высоте
      private int _lenght;
      private int _height;
      private double _proportionWidthToHeight;

      public int Lenght { get { return _lenght; } }
      public int Height { get { return _height; } }
      public int LenghtSpot { get { return _lenghtSpot; } }
      public int HeightSpot { get { return _heightSpot; } }
      public int LenghtSize { get { return _lenghtSize; } }
      public int HeightSize { get { return _heightSize; } }
      public double ProportionWidthToHeight { get { return _proportionWidthToHeight; } }

      public Extents3d ExtentsColorArea
      {
         get
         {
            return _extentsColorArea;
         }
         set
         {
            _extentsColorArea = value;
            calcSizes();
         }
      }

      public ColorAreaSpotSize(int lenghtSpotDefault, int heightSpotDefault, string subkey)
      {         
         _lenghtSpot = lenghtSpotDefault;
         _heightSpot = heightSpotDefault;
         _subkey = subkey;
         loadSize();         
      }

      private void calcSizes()
      {         
         _lenghtSize =Convert.ToInt32((_extentsColorArea.MaxPoint.X - _extentsColorArea.MinPoint.X) / _lenghtSpot);
         _heightSize = Convert.ToInt32((_extentsColorArea.MaxPoint.Y - _extentsColorArea.MinPoint.Y) / _heightSpot);
         _proportionWidthToHeight = _lenghtSize / (double)_heightSize;
         _lenght = _lenghtSize * _lenghtSpot;
         _height = _heightSize * _heightSpot;
      }

      public void ChangeSize()
      {
         FormColorAreaSize formSize = new FormColorAreaSize(_lenghtSpot, _heightSpot);
         if (Application.ShowModalDialog(formSize) == System.Windows.Forms.DialogResult.OK)
         {
            _lenghtSpot = formSize.LenghtSpot;
            _heightSpot = formSize.HeightSpot;
            calcSizes();
            saveSize();  
         }
      }

      private void saveSize()
      {
         try
         {
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Album.RegAppPath + "\\" + _subkey);
            keyAKR.SetValue("Lenght", _lenghtSpot, Microsoft.Win32.RegistryValueKind.DWord);
            keyAKR.SetValue("Height", _heightSpot, Microsoft.Win32.RegistryValueKind.DWord);
         }
         catch { }
      }

      private void loadSize()
      {                  
         try
         {
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Album.RegAppPath + "\\" + _subkey);
            _lenghtSpot = Convert.ToInt32(keyAKR.GetValue("Lenght", _lenghtSpot));
            _heightSpot = Convert.ToInt32(keyAKR.GetValue("Height", _heightSpot));
         }
         catch { }         
      }
   }
}

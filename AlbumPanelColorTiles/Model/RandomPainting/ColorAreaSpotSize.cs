using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.RandomPainting
{
   // Размер ячейки зоны покраски
   public class ColorAreaSpotSize
   {
      private const string _regKeySpotSizeLength = "SpotLenght";
      private const string _regKeySpotSizeHeight = "SpotHeight";
      private Extents3d _extentsColorArea;
      private int _height;
      private int _heightSize;
      private int _heightSpot;

      // кол участков покраски в высоте
      private int _lenght;

      // Вся область покраски
      private int _lenghtSize;

      private int _lenghtSpot;

      // кол участков покраски в длине
      private double _proportionWidthToHeight;

      private string _subkey;

      public ColorAreaSpotSize(int lenghtSpotDefault, int heightSpotDefault, string subkey)
      {
         _lenghtSpot = lenghtSpotDefault;
         _heightSpot = heightSpotDefault;
         _subkey = subkey;
         loadSize();
      }

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

      public int Height { get { return _height; } }
      public int HeightSize { get { return _heightSize; } }
      public int HeightSpot { get { return _heightSpot; } }
      public int Lenght { get { return _lenght; } }
      public int LenghtSize { get { return _lenghtSize; } }
      public int LenghtSpot { get { return _lenghtSpot; } }
      public double ProportionWidthToHeight { get { return _proportionWidthToHeight; } }

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

      private void calcSizes()
      {
         _lenghtSize = Convert.ToInt32((_extentsColorArea.MaxPoint.X - _extentsColorArea.MinPoint.X) / _lenghtSpot);
         _heightSize = Convert.ToInt32((_extentsColorArea.MaxPoint.Y - _extentsColorArea.MinPoint.Y) / _heightSpot);
         _proportionWidthToHeight = _lenghtSize / (double)_heightSize;
         _lenght = _lenghtSize * _lenghtSpot;
         _height = _heightSize * _heightSpot;
      }

      private void loadSize()
      {
         try
         {
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Album.RegAppPath + "\\" + _subkey);
            _lenghtSpot = Convert.ToInt32(keyAKR.GetValue(_regKeySpotSizeLength, _lenghtSpot));
            _heightSpot = Convert.ToInt32(keyAKR.GetValue(_regKeySpotSizeHeight, _heightSpot));
         }
         catch { }
      }

      private void saveSize()
      {
         try
         {
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Album.RegAppPath + "\\" + _subkey);
            keyAKR.SetValue(_regKeySpotSizeLength, _lenghtSpot, Microsoft.Win32.RegistryValueKind.DWord);
            keyAKR.SetValue(_regKeySpotSizeHeight, _heightSpot, Microsoft.Win32.RegistryValueKind.DWord);
         }
         catch { }
      }
   }
}
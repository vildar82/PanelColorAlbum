using System;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Model
{
   public class TileCalc
   {
      #region Private Fields

      private static double _oneTileArea;
      private string _colorMark;
      private int _count;
      private Color _pattern;
      private double _totalArea;

      #endregion Private Fields

      #region Public Constructors

      public TileCalc(string colorMark, int count, Color pattern)
      {
         _colorMark = colorMark;
         _count = count;
         _pattern = pattern;
      }

      #endregion Public Constructors

      #region Public Properties

      public static double OneTileArea
      {
         get
         {
            if (_oneTileArea == 0.0)
            {
               _oneTileArea = Album.Options.TileLenght * Album.Options.TileHeight * 0.000001;//   0.025344;//288*88 в м2
            }
            return _oneTileArea;
         }
      }

      public string ColorMark { get { return _colorMark; } }
      public int Count { get { return _count; } }
      public Color Pattern { get { return _pattern; } }

      public double TotalArea
      {
         get
         {
            if (_totalArea == 0.0)
            {
               _totalArea = Math.Round(OneTileArea * Count, 2);
            }
            return _totalArea;
         }
      }

      #endregion Public Properties
   }
}
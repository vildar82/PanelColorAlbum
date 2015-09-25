using System;
using System.Collections.Generic;

namespace AlbumPanelColorTiles.Lib
{
   // Сравнение чисел
   public class DoubleEqualityComparer : IEqualityComparer<double>
   {
      #region Private Fields

      private readonly double threshold;

      #endregion Private Fields

      #region Public Constructors

      public DoubleEqualityComparer(double threshold = 10)
      {
         this.threshold = threshold;
      }

      #endregion Public Constructors

      #region Public Methods

      public bool Equals(double x, double y)
      {
         return Math.Abs(x - y) < threshold;
      }

      public int GetHashCode(double obj)
      {
         return 0;
      }

      #endregion Public Methods
   }
}
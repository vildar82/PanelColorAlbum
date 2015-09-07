using System;
using System.Collections.Generic;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model.Lib
{
   // Сравнение чисел
   public class DoubleEqualityComparer : IEqualityComparer<double>
   {
      private readonly double threshold;

      public DoubleEqualityComparer(double threshold = 10)
      {
         this.threshold = threshold;
      }

      public bool Equals(double x, double y)
      {
         return Math.Abs(x - y) < threshold;
      }

      public int GetHashCode(double obj)
      {
         return 0;
      }
   }
}
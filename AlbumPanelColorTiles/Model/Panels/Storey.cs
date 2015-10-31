using System;
using System.Linq;
using System.Collections.Generic;

namespace AlbumPanelColorTiles.Panels
{
   public class StoreyNumberComparer : IComparer<string>
   {
      public int Compare(string x, string y)
      {
         int numberX;
         if (int.TryParse(x, out numberX))
         {
            // x - число numberX
            int numberY;
            if (int.TryParse(y, out numberY))
            {
               // y - число numberY
               return numberX.CompareTo(numberY);
            }
            else
            {
               // y - строка.
               return -1; // число numberX меньше строки y
            }
         }
         else
         {
            // x - строка
            int numberY;
            if (int.TryParse(y, out numberY))
            {
               // y - число numberY
               return 1; // число numberY меньше строки x
            }
            else
            {
               // y - строка.               
               return x.CompareTo(y);
            }
         }
      }
   }

   // Этаж
   public class Storey : IEquatable<Storey>, IComparable<Storey>
   {
      private string _number;
      private double _y;
      private HashSet<MarkArPanelAR> _marksAr;
      private static StoreyNumberComparer _comparer = new StoreyNumberComparer ();

      /// <summary>
      /// Высотная отметка этажа
      /// </summary>
      /// <param name="y">Отметка этажа</param>
      public Storey(double y)
      {
         _y = y;
         _marksAr = new HashSet<MarkArPanelAR>();
      }

      public List<MarkArPanelAR> MarksAr { get { return _marksAr.ToList(); } }

      public void AddMarkAr(MarkArPanelAR markAr)
      {
         _marksAr.Add(markAr);
      }

      public string Number
      {
         get { return _number; }
         set { _number = value; }
      }

      public double Y
      {
         get { return _y; }
      }      

      public bool Equals(Storey other)
      {
         return _number.Equals(other._number) &&
            _y.Equals(other._y);
      }

      public int CompareTo(Storey other)
      {         
         return _comparer.Compare(_number, other._number);         
      }
   }
}
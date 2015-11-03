using System;
using System.Linq;
using System.Collections.Generic;

namespace AlbumPanelColorTiles.Panels
{
   // Этаж
   public class Storey : IEquatable<Storey>, IComparable<Storey>
   {
      private string _number;
      private string _numberAsNumber;
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
         set
         {
            _number = value;
            _numberAsNumber = value;
         }
      }
      public string NumberAsNumber { get { return _numberAsNumber; } }
      public double Y { get { return _y; } }

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
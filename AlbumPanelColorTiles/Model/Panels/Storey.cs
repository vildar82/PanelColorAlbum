using System;
using System.Linq;
using System.Collections.Generic;

namespace AlbumPanelColorTiles.Panels
{
   // Этаж
   public class Storey : IEquatable<Storey>, IComparable<Storey>
   {
      private string _number;
      private double _y;
      private HashSet<MarkArPanel> _marksAr;

      /// <summary>
      /// Высотная отметка этажа
      /// </summary>
      /// <param name="y">Отметка этажа</param>
      public Storey(double y)
      {
         _y = y;
         _marksAr = new HashSet<MarkArPanel>();
      }

      public List<MarkArPanel> MarksAr { get { return _marksAr.ToList(); } }

      public void AddMarkAr(MarkArPanel markAr)
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
         return _number.CompareTo(other._number);
      }
   }
}
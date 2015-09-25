using System;

namespace AlbumPanelColorTiles.Model
{
   // Этаж
   public class Storey : IEquatable<Storey>
   {
      #region Private Fields

      private string _number;
      private double _y;

      #endregion Private Fields

      #region Public Constructors

      /// <summary>
      /// Высотная отметка этажа
      /// </summary>
      /// <param name="y">Отметка этажа</param>
      public Storey(double y)
      {
         _y = y;
      }

      #endregion Public Constructors

      #region Public Properties

      public string Number
      {
         get { return _number; }
         set { _number = value; }
      }

      public double Y
      {
         get { return _y; }
      }

      #endregion Public Properties

      #region Public Methods

      public bool Equals(Storey other)
      {
         return _number.Equals(other._number) &&
            _y.Equals(other._y);
      }

      #endregion Public Methods
   }
}
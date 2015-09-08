﻿namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Этаж
   public class Storey
   {
      private string _number;
      private double _y;

      /// <summary>
      /// Высотная отметка этажа
      /// </summary>
      /// <param name="y">Отметка этажа</param>
      public Storey(double y)
      {
         _y = y;
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
   }
}
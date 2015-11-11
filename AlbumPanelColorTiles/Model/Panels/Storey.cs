using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Comparers;
using AlbumPanelColorTiles.Properties;

namespace AlbumPanelColorTiles.Panels
{
   // Этаж
   public class Storey : IEquatable<Storey>, IComparable<Storey>
   {
      private static StoreyNumberComparer _comparer = new StoreyNumberComparer();
      private HashSet<MarkArPanelAR> _marksAr;
      private string _number;      
      private double _y;

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

      public string Number
      {
         get { return _number; }
         set
         {
            _number = value;            
         }
      }     

      public double Y { get { return _y; } }

      public void AddMarkAr(MarkArPanelAR markAr)
      {
         _marksAr.Add(markAr);
      }

      public int CompareTo(Storey other)
      {
         return _comparer.Compare(_number, other._number);
      }

      public bool Equals(Storey other)
      {
         return _number.Equals(other._number) &&
            _y.Equals(other._y);
      }

      // определение этажей панелей
      public static List<Storey> IdentificationStoreys(List<MarkSbPanelAR> marksSB, int numberFirstFloor)
      {
         // Определение этажей панелей (точек вставки панелей по Y.) для всех панелей в чертеже, кроме панелей чердака.
         var comparerStorey = new DoubleEqualityComparer(Settings.Default.StoreyDefineDeviation); // 2000
         //HashSet<double> panelsStorey = new HashSet<double>(comparerStorey);
         // Этажи
         var storeys = new List<Storey>();
         var panels = marksSB.Where(sb => !sb.IsUpperStoreyPanel).SelectMany(sb => sb.MarksAR.SelectMany(ar => ar.Panels)).OrderBy(p => p.InsPt.Y);
         foreach (var panel in panels)
         {
            Storey storey = storeys.Find(s => comparerStorey.Equals(s.Y, panel.InsPt.Y));
            if (storey == null)
            {
               // Новый этаж
               storey = new Storey(panel.InsPt.Y);
               storeys.Add(storey);
               storeys.Sort((Storey s1, Storey s2) => s1.Y.CompareTo(s2.Y));
            }
            panel.Storey = storey;
            storey.AddMarkAr(panel.MarkAr);
         }
         // Нумерация этажей
         int i = numberFirstFloor;
         var storeysOrders = storeys.OrderBy(s => s.Y).ToList();
         storeysOrders.ForEach((s) => s.Number = i++.ToString());
         // Пока уберем индекс Последнего этажа - сейчас он определяется только для последнего этажа среди всех окрашевыемых фасадов. 
         // А нужно определять последний этаж на каждом фасаде.
         //storeysOrders.Last().Number = Settings.Default.PaintIndexLastStorey;// "П"
         // В итоге у всех панелей (Panel) проставлены этажи (Storey).
         return storeys;
      }
   }
}
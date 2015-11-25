using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Comparers;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles.Panels
{
   // Этаж
   public class Storey : IEquatable<Storey>, IComparable<Storey>
   {      
      private int _number;
      private string _name;
      private string _layer;
      private double _y;
      private EnumStorey _type;      

      /// <summary>
      /// Высотная отметка этажа
      /// </summary>
      /// <param name="y">Отметка этажа</param>
      public Storey(double y)
      {
         _y = y;         
      }
      public Storey(EnumStorey type)
      {
         _type = type;
         _layer = getLayer();
      }

      public Storey (string name)
      {
         if (string.Equals(name, Settings.Default.PaintIndexUpperStorey, StringComparison.OrdinalIgnoreCase))
         {
            _type = EnumStorey.Upper;
         }
         else if (string.Equals(name, Settings.Default.PaintIndexParapet, StringComparison.OrdinalIgnoreCase))
         {
            _type = EnumStorey.Parapet;
         }
         else
         {
            // число
            _type = EnumStorey.Number;
            if (!int.TryParse(name, out _number))
            {
               throw new Exception("Не определен номер этажа по блоку монтажного плана ");
            }
         }
         _layer = getLayer();
      }

      public int Number
      {
         get { return _number; }
         set { _number = value; }
      }

      public double Y { get { return _y; } set { _y = value; } }
      public EnumStorey Type { get { return _type; } }
      public string Layer { get { return _layer; } }

      // определение этажей панелей
      public static List<Storey> IdentificationStoreys(List<MarkSbPanelAR> marksSB, int numberFirstFloor)
      {
         List<Storey> storeys = new List<Storey>();
         // Определение этажей панелей (точек вставки панелей по Y.) для всех панелей в чертеже, кроме панелей чердака.         
         // Этажи с числовой нумерацией
         List<Storey> storeysNumberType = defStoreyNumberType(marksSB, numberFirstFloor);
         storeys.AddRange(storeysNumberType);
         // Этажи Ч и П 
         List<Storey> storeysUpperAndParapetType = defStoreyUpperAndParapetType(marksSB);
         storeys.AddRange(storeysUpperAndParapetType);
         // В итоге у всех панелей (Panel) проставлены этажи (Storey).
         return storeys;
      }

      private static List<Storey> defStoreyUpperAndParapetType(List<MarkSbPanelAR> marksSB)
      {
         // Этажи Ч и П 
         var storeysUpperAndParapetType = new List<Storey>();
         var panelsUpperAndParapetType = marksSB.Where(sb => sb.StoreyTypePanel != EnumStorey.Number).SelectMany(sb => sb.MarksAR.SelectMany(ar => ar.Panels));
         foreach (var panel in panelsUpperAndParapetType)
         {
            Storey storey = storeysUpperAndParapetType.Find(s => s.Type == panel.MarkAr.MarkSB.StoreyTypePanel);
            if (storey == null)
            {
               // Новый этаж
               storey = new Storey(panel.MarkAr.MarkSB.StoreyTypePanel);
               storeysUpperAndParapetType.Add(storey);               
            }
            panel.Storey = storey;
         }
         storeysUpperAndParapetType.Sort((Storey s1, Storey s2) => s1.Type.CompareTo(s2.Type));
         return storeysUpperAndParapetType;
      }

      private static List<Storey> defStoreyNumberType(List<MarkSbPanelAR> marksSB, int numberFirstFloor)
      {
         var storeysNumberType = new List<Storey>();
         var comparerStorey = new DoubleEqualityComparer(Settings.Default.StoreyDefineDeviation); // 2000        
         // Панели этажные (без чердака и парапета)
         var panelsStoreyNumberType = marksSB.Where(sb => sb.StoreyTypePanel == EnumStorey.Number).SelectMany(sb => sb.MarksAR.SelectMany(ar => ar.Panels)).OrderBy(p => p.InsPt.Y);
         foreach (var panel in panelsStoreyNumberType)
         {
            Storey storey = storeysNumberType.Find(s => comparerStorey.Equals(s.Y, panel.InsPt.Y));
            if (storey == null)
            {
               // Новый этаж
               storey = new Storey(panel.InsPt.Y);
               storeysNumberType.Add(storey);
               storeysNumberType.Sort((Storey s1, Storey s2) => s1.Y.CompareTo(s2.Y));
            }
            panel.Storey = storey;
         }
         // Нумерация этажей
         int i = numberFirstFloor;
         var storeys = storeysNumberType.OrderBy(s => s.Y).ToList();
         storeysNumberType.ForEach((s) => s.Number = i++);
         return storeysNumberType;
      }

      public override string ToString()
      {
         if (_name == null)
         {
            _name = GetName();
         }
         return _name;
      }

      public string GetName()
      {
         switch (_type)
         {
            case EnumStorey.Number:
               return _number.ToString();               
            case EnumStorey.Upper:
               return Settings.Default.PaintIndexUpperStorey;
            case EnumStorey.Parapet:
               return Settings.Default.PaintIndexParapet;
            default:
               return string.Empty;               
         }
      }

      private string getLayer()
      {
         switch (_type)
         {
            case EnumStorey.Number:
               return Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices.LayerZeroName;
            case EnumStorey.Upper:
               return Settings.Default.LayerUpperStoreyPanels;
            case EnumStorey.Parapet:
               return Settings.Default.LayerParapetPanels;
            default:
               return Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices.LayerZeroName;
         }
      }

      public int CompareTo(Storey other)
      {
         if (_type == EnumStorey.Number)
         {
            return _number.CompareTo(other._number);
         }
         else
         {
            return _type.CompareTo(other._type);
         }
      }

      public bool Equals(Storey other)
      {
         if (_type == EnumStorey.Number)
         {
            return _number.Equals(other._number);// &&            
            //_y.Equals(other._y);
         }
         else
         {
            return _type.Equals(other._type);
         }         
      }

      //public void DefineYFloor(int minNum)
      //{
      //   // определение уровня этажа относиельно 0 уровня минимального этажа minNum         
      //   _y = (_number - minNum) * Settings.Default.FacadeFloorHeight;
      //}
   }
}
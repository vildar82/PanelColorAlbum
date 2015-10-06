using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model
{
   // Участок покраски
   public class Spot
   {
      private RandomPaint _proper;
      private ObjectId _idBlRef;
      private int _index;

      public RandomPaint Proper { get { return _proper; } }
      public ObjectId IdBlRef { get { return _idBlRef; } set { _idBlRef = value; } }

      public int Index
      {
         get { return _index; }
         set { _index = value; }
      }

      public Spot (RandomPaint proper)
      {
         _proper = proper;
      }

      public static IEnumerable<Spot> GetSpots(RandomPaint proper)
      {
         List<Spot> spots = new List<Spot>();
         for (int i = 0; i < proper.TailCount; i++)
         {
            Spot spot = new Spot(proper);
            spots.Add(spot);
         }
         return spots;
      }

      public static List<Spot> GetEmpty(int emptySpotsCount)
      {
         List<Spot> spots = new List<Spot>(emptySpotsCount);
         for (int i = 0; i < emptySpotsCount; i++)
         {            
            spots.Add(null);
         }
         return spots;
      }
   }
}

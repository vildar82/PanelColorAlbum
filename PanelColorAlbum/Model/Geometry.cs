using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   public static class Geometry
   {
      // Попадает ли точка в границы
      public static bool IsPointInBounds(Point3d pt, Extents3d bounds)
      {
         bool res = false;

         if (pt.X >= bounds.MinPoint.X && pt.Y >= bounds.MinPoint.Y &&
            pt.X <= bounds.MaxPoint.X && pt.Y <= bounds.MaxPoint.Y)
         {
            res = true;
         }
         return res;
      }
   }
}

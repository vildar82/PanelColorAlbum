using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model.Lib
{
   public static class Geometry
   {
      // Попадает ли точка внутрь границы
      public static bool IsPointInBounds(Point3d pt, Extents3d bounds)
      {
         bool res = false;

         if (pt.X > bounds.MinPoint.X && pt.Y > bounds.MinPoint.Y &&
            pt.X < bounds.MaxPoint.X && pt.Y < bounds.MaxPoint.Y)
         {
            res = true;
         }
         return res;
      }      
   }
}
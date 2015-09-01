using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   public static  class Test
   {
      public static void DrawBounds (Extents3d ext)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {            
            Polyline p = new Polyline();
            p.AddVertexAt(0, new Point2d(ext.MinPoint.X, ext.MinPoint.Y), 0, 0, 0);
            p.AddVertexAt(1, new Point2d(ext.MaxPoint.X, ext.MinPoint.Y), 0, 0, 0);
            p.AddVertexAt(2, new Point2d(ext.MaxPoint.X, ext.MaxPoint.Y), 0, 0, 0);
            p.AddVertexAt(3, new Point2d(ext.MinPoint.X, ext.MaxPoint.Y), 0, 0, 0);

            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

            ms.AppendEntity(p);
            t.AddNewlyCreatedDBObject(p, true);
            t.Commit();
         }
      }
   }
}

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Autodesk.AutoCAD.DatabaseServices
{
   public static class ViewTableRecordExtension
   {
      public static Matrix3d EyeToWorld(this ViewTableRecord view)
      {
         if (view == null)
            throw new ArgumentNullException("view");

         return
             Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
             Matrix3d.Displacement(view.Target - Point3d.Origin) *
             Matrix3d.PlaneToWorld(view.ViewDirection);
      }

      public static Matrix3d WorldToEye(this ViewTableRecord view)
      {
         return view.EyeToWorld().Inverse();
      }
   }
}

namespace Autodesk.AutoCAD.EditorInput
{
   public static class EditorExtension
   {
      public static void Zoom(this Editor ed, Extents3d ext)
      {
         if (ed == null)
            throw new ArgumentNullException("ed");

         using (ViewTableRecord view = ed.GetCurrentView())
         {
            ext.TransformBy(view.WorldToEye());
            view.Width = ext.MaxPoint.X - ext.MinPoint.X;
            view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
            view.CenterPoint = new Point2d(
                (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
            ed.SetCurrentView(view);
         }
      }

      public static void ZoomExtents(this Editor ed)
      {
         if (ed == null)
            throw new ArgumentNullException("ed");

         Database db = ed.Document.Database;
         Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1 ?
             new Extents3d(db.Pextmin, db.Pextmax) :
             new Extents3d(db.Extmin, db.Extmax);
         ed.Zoom(ext);
      }
   }
}
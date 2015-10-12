using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Lib
{
   public static class UserPrompt
   {
      public static Extents3d PromptExtents(Editor ed, string msgPromptFirstPoint, string msgPromptsecondPoint)
      {
         Extents3d extentsPrompted = new Extents3d();
         var prPtRes = ed.GetPoint(msgPromptFirstPoint);
         if (prPtRes.Status == PromptStatus.OK)
         {
            var prCornerRes = ed.GetCorner(msgPromptsecondPoint, prPtRes.Value);
            if (prCornerRes.Status == PromptStatus.OK)
            {
               extentsPrompted.AddPoint(prPtRes.Value);
               extentsPrompted.AddPoint(prCornerRes.Value);
            }
            else
            {
               throw new Exception("Отменено пользователем.");
            }
         }
         else
         {
            throw new Exception("Отменено пользователем.");
         }
         return extentsPrompted;
      }
   }
}

using System.Collections.Generic;
using AcadLib.Errors;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Checks
{
   public class CheckDrawing
   {
      private static Database _db;
      private static Document _doc;
      private static Editor _ed;

      public CheckDrawing()
      {
         Clear();
      }

      // Блоки марки АР с непокрашенной плиткой.(если есть хоть одна непокрашенная плитка).
      //private static List<Error> _notPaintedTilesInMarkAR;
      public void Clear()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
         _ed = _doc.Editor;
         //_errors = new List<Error>();
         //_notPaintedTilesInMarkAR = new List<ErrorObject>();
         //_markArBtrNames = new List<string>();
      }

      // Проверка чертежа
      public void CheckForPaint()
      {
         // 1. Не должно быть блоков Марки АР. Т.к. может получится так, что при текущем расчте получится марка панели которая уже определенва в чертеже, и она не сможет создаться, т.к. такой блок уже есть.
         var markArBtrNames = checkBtrMarkAr();
         if (markArBtrNames.Count > 0)
         {
            // Выдать сообщение, со списком блоков панелей марки АР. Которых не должно быть перед расчетом.
            string msg = "В чертеже не должно быть блоков панелей Марки АР перед покраской. ";
            msg += string.Join(", ", markArBtrNames.ToArray());
            //_ed.WriteMessage("\n" + msg);
            //_errors.Add(new Error(msg));
            Inspector.AddError(msg);
         }
      }

      // Проверка, все ли плитки покрашены
      //public static bool CheckAllTileArePainted(List<MarkSbPanel> marksSb)
      //{
      //   bool res = true;
      //   //_notPaintedTilesInMarkAR = new List<ErrorObject>();
      //   foreach (var markSb in marksSb)
      //   {
      //      if (markSb.MarksAR.Count == 0)
      //      {
      //         // Такого не должно быть. Марка СБ есть, а марок АР нет.
      //         Error err = new Error(string.Format("\nЕсть Марка СБ {0}, а Марки АР не определены. Ошибка в программе(.", markSb.MarkSb));
      //      }

      //      foreach (var markAr in markSb.MarksAR)
      //      {
      //         bool isAllTilePainted = true;
      //         foreach (var paint in markAr.Paints)
      //         {
      //            if (paint == null)
      //            {
      //               // Плитка не покрашена!
      //               isAllTilePainted = false;
      //               string errMsg = "Не вся плитка покрашена в блоке " + markSb.MarkSb;
      //               ErrorObject errObj = new ErrorObject(errMsg, ObjectId.Null);
      //               _notPaintedTilesInMarkAR.Add(errObj);
      //               //_ed.WriteMessage("\n" + errMsg);
      //               res = false;
      //               break;
      //            }
      //         }
      //         if (!isAllTilePainted) break;
      //      }
      //   }
      //   return res;
      //}
      // Проверка наличия определений блоков Марки АР
      private List<string> checkBtrMarkAr()
      {
         List<string> markArBtrNames = new List<string>();
         using (var bt = _db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            foreach (var idBtr in bt)
            {
               using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
               {
                  if (MarkSbPanelAR.IsBlockNamePanelMarkAr(btr.Name))
                  {
                     markArBtrNames.Add(btr.Name);
                  }
               }
            }
         }
         return markArBtrNames;
      }
   }
}
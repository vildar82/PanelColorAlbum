using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Checks
{
   public static class Inspector
   {
      private static Database _db;
      private static Document _doc;
      private static Editor _ed;
      private static List<string> _markArBtrNames;
      private static List<Error> _errors;

      public static List<Error> Errors { get { return _errors; } }

      // Блоки марки АР с непокрашенной плиткой.(если есть хоть одна непокрашенная плитка).
      private static List<ErrorObject> _notPaintedTilesInMarkAR;

      static Inspector()
      {
         _errors = new List<Error>();
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
         _ed = _doc.Editor;
      }

      public static void Reset ()
      {
         _errors = new List<Error>();
         _notPaintedTilesInMarkAR = new List<ErrorObject>();
         _markArBtrNames = new List<string>();
      }

      // Проверка, все ли плитки покрашены
      public static bool CheckAllTileArePainted(List<MarkSbPanel> marksSb)
      {
         bool res = true;
         _notPaintedTilesInMarkAR = new List<ErrorObject>();
         foreach (var markSb in marksSb)
         {
            if (markSb.MarksAR.Count == 0)
            {
               // Такого не должно быть. Марка СБ есть, а марок АР нет.
               _ed.WriteMessage(string.Format("\nЕсть Марка СБ {0}, а Марки АР не определены. Ошибка в программе(.", markSb.MarkSb));
            }

            foreach (var markAr in markSb.MarksAR)
            {
               bool isAllTilePainted = true;
               foreach (var paint in markAr.Paints)
               {
                  if (paint == null)
                  {
                     // Плитка не покрашена!
                     isAllTilePainted = false;
                     string errMsg = "Не вся плитка покрашена в блоке " + markSb.MarkSb;
                     ErrorObject errObj = new ErrorObject(errMsg, ObjectId.Null);
                     _notPaintedTilesInMarkAR.Add(errObj);
                     _ed.WriteMessage("\n" + errMsg);
                     res = false;
                     break;
                  }
               }
               if (!isAllTilePainted) break;
            }
         }
         return res;
      }

      // Проверка чертежа
      public static bool CheckDrawing()
      {
         bool res = true;
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            // 1. Не должно быть блоков Марки АР.
            CheckBtrMarkAr(bt, t);
            t.Commit();
         }
         if (_markArBtrNames.Count > 0)
         {
            res = false;
            // Выдать сообщение, со списком блоков панелей марки АР. Которых не должно быть перед расчетом.
            string msg = String.Join(", ", _markArBtrNames.ToArray());
            _ed.WriteMessage("\n" + msg);
         }
         return res;
      }

      // Проверка наличия определений блоков Марки АР
      private static bool CheckBtrMarkAr(BlockTable bt, Transaction t)
      {
         bool res = true;
         _markArBtrNames = new List<string>();
         foreach (var idBtr in bt)
         {
            var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            if (MarkSbPanel.IsBlockNamePanelMarkAr(btr.Name))
            {
               _markArBtrNames.Add(btr.Name);
               res = false;
            }
         }
         return res;
      }

      public static void Show()
      {
         Application.ShowModelessDialog(new FormError());
      }
   }
}
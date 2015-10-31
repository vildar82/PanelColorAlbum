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
      //private static List<string> _markArBtrNames;
      private static List<Error> _errors;

      public static List<Error> Errors { get { return _errors; } }

      public static bool HasErrors { get { return _errors.Count > 0; } }

      // Блоки марки АР с непокрашенной плиткой.(если есть хоть одна непокрашенная плитка).
      //private static List<Error> _notPaintedTilesInMarkAR;

      static Inspector()
      {         
         Clear();
      }

      public static void Clear ()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
         _ed = _doc.Editor;
         _errors = new List<Error>();
         //_notPaintedTilesInMarkAR = new List<ErrorObject>();
         //_markArBtrNames = new List<string>();
      }

      public static void AddError (string msg)
      {
         var err = new Error(msg);
         _errors.Add(err);
      }
      public static void AddError(string msg, Entity ent)
      {
         var err = new Error(msg, ent);
         _errors.Add(err);
      }
      public static void AddError(string msg, Entity ent, Extents3d ext)
      {
         var err = new Error(msg, ext, ent);
         _errors.Add(err);
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

      // Проверка чертежа
      public static void CheckDrawing()
      {
         bool res = true;
         // 1. Не должно быть блоков Марки АР. Т.к. может получится так, что при текущем расчте получится марка панели которая уже определенва в чертеже, и она не сможет создаться, т.к. такой блок уже есть.
         var markArBtrNames = checkBtrMarkAr();                     
         if (markArBtrNames.Count > 0)
         {
            res = false;
            // Выдать сообщение, со списком блоков панелей марки АР. Которых не должно быть перед расчетом.
            string msg = string.Join(", ", markArBtrNames.ToArray());
            //_ed.WriteMessage("\n" + msg);            
            _errors.Add(new Error(msg));
         }         
      }

      // Проверка наличия определений блоков Марки АР
      private static List<string> checkBtrMarkAr()
      {
         List<string> markArBtrNames = new List<string>();
         using (var bt = _db.BlockTableId.Open( OpenMode.ForRead) as BlockTable)
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

      public static void Show()
      {
         Application.ShowModelessDialog(new FormError());
      }
   }
}
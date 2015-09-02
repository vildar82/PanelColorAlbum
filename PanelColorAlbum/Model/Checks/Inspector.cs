using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   public class Inspector
   {
      Document _doc;
      Database _db;
      Editor _ed;
      List<string> _markArBtrNames;
      // Блоки марки АР с непокрашенной плиткой.(если есть хоть одна непокрашенная плитка).
      List<ErrorObject> _notPaintedTilesInMarkAR;

      public Inspector ()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
         _ed = _doc.Editor;   
      }

      // Проверка чертежа
      public bool CheckDrawing()
      {
         bool res = true;
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;

            // 1. Не должно быть блоков Марки АР.
            CheckBtrMarkAr(bt, t);

            t.Commit();
         }

         if (_markArBtrNames.Count >0)
         {
            res = false;
            // Выдать сообщение, со списком блоков панелей марки АР. Которых не должно быть перед расчетом.
            string msg = String.Join(", ", _markArBtrNames.ToArray());
            _ed.WriteMessage("\n" + msg);
         }
         return res;
      }

      // Проверка наличия определений блоков Марки АР
      private bool CheckBtrMarkAr(BlockTable bt, Transaction t)
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

      // Проверка, все ли плитки покрашены
      public bool  CheckAllTileArePainted(List<MarkSbPanel> marksSb)
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
               foreach (var paint in markAr.Paints)
               {
                  if (paint == null)
                  {
                     // Плитка не покрашена! 
                     string errMsg = "Не вся плитка покрашена в блоке " + markAr.MarkArBlockName;
                     ErrorObject errObj = new ErrorObject(errMsg, ObjectId.Null);                     
                     _notPaintedTilesInMarkAR.Add(errObj);
                     _ed.WriteMessage("\n" + errMsg);
                     res = false;
                     break;
                  }
               }
            }
         }
         return res;
      }
   }
}

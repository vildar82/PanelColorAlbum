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
            _markArBtrNames = CheckBtrMarkAr(bt, t);

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
      private List<string> CheckBtrMarkAr(BlockTable bt, Transaction t)
      {
         List<string> btrsMarkAr = new List<string>();
         foreach (var idBtr in bt)
         {
            var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;            
            if (MarkSbPanel.IsBlockNamePanelMarkAr(btr.Name))
            {               
               btrsMarkAr.Add(btr.Name);
            }
         }
         return btrsMarkAr;
      }
   }
}

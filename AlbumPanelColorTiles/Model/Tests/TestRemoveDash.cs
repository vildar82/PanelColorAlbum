using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Tests
{
   class TestRemoveDash
   {      
      public void RemoveDashAKR()
      {
         // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
         Document doc = Application.DocumentManager.MdiActiveDocument;
         Editor ed = doc.Editor;
         Database db = doc.Database;

         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            Point3d pt = Point3d.Origin;
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (btr.Name.StartsWith(Settings.Default.BlockPanelPrefixName))
               {
                  // если есть первое тире, то попытка переименовыать имя блока без тире
                  var markSb = btr.Name.Substring(Settings.Default.BlockPanelPrefixName.Length);
                  var valType = getTypeName(markSb);
                  if (string.IsNullOrEmpty(valType))
                  {
                     ed.WriteMessage("Не определен тип - {0}".f(btr.Name));
                     continue;
                  }
                  // если след символ тире
                  if (markSb.Substring(valType.Length, 1) == "-")
                  {
                     string newBlNameWithoutDash = "{0}{1} {2}".f(Settings.Default.BlockPanelPrefixName, valType,
                                 markSb.Substring(valType.Length + 1));
                     if (bt.Has(newBlNameWithoutDash))
                     {
                        // Сравнение панелей
                        InsertPanel(t, bt, ms, pt, newBlNameWithoutDash);
                        InsertPanel(t, bt, ms, new Point3d(pt.X + 6000, pt.Y, 0), btr.Name);
                        pt = new Point3d(pt.X, pt.Y + 5000, 0);
                     }
                     else
                     {
                        var oldName = btr.Name;
                        btr.UpgradeOpen();
                        btr.Name = newBlNameWithoutDash;
                        ed.WriteMessage("{0} переименована в {1}".f(oldName, newBlNameWithoutDash));
                     }
                  }
               }
            }
            t.Commit();
         }
      }

      private void InsertPanel(Transaction t, BlockTable bt, BlockTableRecord ms, Point3d pt, string name)
      {
         var blRef = new BlockReference(pt, bt[name]);
         ms.AppendEntity(blRef);
         t.AddNewlyCreatedDBObject(blRef, true);
      }

      private string getTypeName(string markSb)
      {
         if (markSb.StartsWith("1НФ"))
         {
            return "1НФ";
         }
         if (markSb.StartsWith("3НСг"))
         {
            return "3НСг";
         }
         if (markSb.StartsWith("3НСНг"))
         {
            return "3НСНг";
         }
         if (markSb.StartsWith("3НСНг2"))
         {
            return "3НСНг2";
         }
         if (markSb.StartsWith("3НЧг"))
         {
            return "3НЧг";
         }
         if (markSb.StartsWith("3НЧНг"))
         {
            return "3НЧНг";
         }
         return null;
      }
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model.Sheets
{
   // Листы Марки СБ
   public class SheetMarkSB : IComparable<SheetMarkSB>
   {
      private string _fileMarkSB;

      // Файл панели Марки СБ с листами Маркок АР.
      private MarkSbPanel _markSB;

      private List<SheetMarkAr> _sheetsMarkAR;
      // Создание листа марки СБ
      public SheetMarkSB(MarkSbPanel markSB)
      {
         _markSB = markSB;
         _sheetsMarkAR = new List<SheetMarkAr>();
         // Обработка Марок АР
         foreach (var markAR in _markSB.MarksAR)
         {
            SheetMarkAr sheetAR = new SheetMarkAr(markAR);
            _sheetsMarkAR.Add(sheetAR);
         }
         // Сортировка листов Марок АР
         _sheetsMarkAR.Sort();
      }

      public string MarkSB { get { return _markSB.MarkSb; } }

      public List<SheetMarkAr> SheetsMarkAR { get { return _sheetsMarkAR; } }
      public int CompareTo(SheetMarkSB other)
      {
         return _markSB.MarkSb.CompareTo(other.MarkSB);
      }

      // Создание файла марки СБ и листов с панелями марок АР
      public void CreateSheetMarkSB(SheetsSet sheetSet)
      {
         // Создание файла панели Марки СБ и создание в нем листов с панелями Марки АР
         _fileMarkSB = CreateFileMarkSB(_markSB, sheetSet.AlbumDir, sheetSet.SheetTemplateFileMarkSB);

         // Создание листов Марок АР
         using (Database dbMarkSB = new Database(false, true))
         {
            Database dbOrig = _markSB.IdBtr.Database;
            dbMarkSB.ReadDwgFile(_fileMarkSB, FileShare.ReadWrite, false, "");
            dbMarkSB.CloseInput(true);

            // Копирование всех определений блоков марки АР в файл Марки СБ
            CopyBtrMarksARToSheetMarkSB(_markSB, dbMarkSB);

            // Слои для заморозки на видовых экранах панелей (Окна, Размеры в форме и на фасаде)  
            // А так же включение и разморозка всех слоев.           
            List<ObjectId> layersToFreezeOnFacadeSheet;
            List<ObjectId> layersToFreezeOnFormSheet;
            GetLayersToFreezeOnSheetsPanel(dbMarkSB, out layersToFreezeOnFacadeSheet, out layersToFreezeOnFormSheet);

            // Создание листов Марок АР
            Point3d pt = Point3d.Origin;
            foreach (var sheetMarkAR in _sheetsMarkAR)
            {
               sheetMarkAR.CreateLayout(dbMarkSB, pt, layersToFreezeOnFacadeSheet, layersToFreezeOnFormSheet);
               // Точка для вставки следующего блока Марки АР
               pt = new Point3d(pt.X + 15000, pt.Y, 0);
            }

            //// Удаление шаблона листа из фала Марки СБ
            HostApplicationServices.WorkingDatabase = dbMarkSB;
            LayoutManager lm = LayoutManager.Current;
            lm.DeleteLayout(Album.Options.SheetTemplateLayoutNameForMarkAR);            

            HostApplicationServices.WorkingDatabase = dbOrig;
            dbMarkSB.SaveAs(_fileMarkSB, DwgVersion.Current);
         }
      }

      // Слои для заморозки на видовых экранах на листах панелей
      private void GetLayersToFreezeOnSheetsPanel(Database dbMarkSB, out List<ObjectId> layersToFreezeOnFacadeSheet, out List<ObjectId> layersToFreezeOnFormSheet)
      {
         layersToFreezeOnFacadeSheet = new List<ObjectId>();
         layersToFreezeOnFormSheet = new List<ObjectId>();
         using (var t = dbMarkSB.TransactionManager.StartTransaction ())
         {
            var lt = t.GetObject(dbMarkSB.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (lt.Has (Album.Options.LayerDimensionFacade))
            {
               layersToFreezeOnFormSheet.Add(lt[Album.Options.LayerDimensionFacade]);
            }
            if (lt.Has(Album.Options.LayerWindows))
            {
               layersToFreezeOnFormSheet.Add(lt[Album.Options.LayerWindows]);
            }
            if (lt.Has(Album.Options.LayerDimensionForm))
            {
               layersToFreezeOnFacadeSheet.Add(lt[Album.Options.LayerDimensionForm]);
            }
            // Включение и разморозка всех слоев
            foreach (var idLayer in lt)
            {
               var layer = t.GetObject(idLayer, OpenMode.ForRead) as LayerTableRecord;
               if (layer.IsOff || layer.IsFrozen)
               {
                  layer.UpgradeOpen();
                  layer.IsOff = false;
                  layer.IsFrozen = false;  
               }
            }
            t.Commit();
         }
      }

      // Копирование определений блоков Марок АР в чертеж листов Марки СБ.
      private void CopyBtrMarksARToSheetMarkSB(MarkSbPanel markSB, Database dbMarkSB)
      {
         Database dbSource = markSB.IdBtr.Database;
         var idsCopy = new ObjectIdCollection();
         foreach (var markAr in markSB.MarksAR)
         {
            idsCopy.Add(markAr.IdBtrAr);
         }
         IdMapping map = new IdMapping();
         dbSource.WblockCloneObjects(idsCopy, dbMarkSB.BlockTableId, map, DuplicateRecordCloning.Replace, false);
      }

      // Создание файла Марки СБ
      private string CreateFileMarkSB(MarkSbPanel markSB, string albumFolder, string templateFileMarkSB)
      {
         string fileDest = Path.Combine(albumFolder, markSB.MarkSb + ".dwg");
         File.Copy(templateFileMarkSB, fileDest);
         return fileDest;
      }
   }
}
using System;
using System.Collections.Generic;
using System.IO;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Sheets
{
   // Листы Марки СБ
   public class SheetMarkSB : IComparable<SheetMarkSB>
   {
      private string _fileMarkSB;

      // Файл панели Марки СБ с листами Маркок АР.
      private MarkSb _markSB;

      private List<SheetMarkAr> _sheetsMarkAR;

      // Конструктор
      public SheetMarkSB(MarkSb markSB)
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

      public string MarkSB { get { return _markSB.MarkSbName; } }

      public List<SheetMarkAr> SheetsMarkAR { get { return _sheetsMarkAR; } }

      public int CompareTo(SheetMarkSB other)
      {
         return _markSB.MarkSbName.CompareTo(other.MarkSB);
      }

      // Создание файла марки СБ и листов с панелями марок АР
      public void CreateSheetMarkSB(SheetsSet sheetSet, int count, BlockFrameAKR blFrameFacade)
      {
         // Создание файла панели Марки СБ и создание в нем листов с панелями Марки АР
         _fileMarkSB = CreateFileMarkSB(_markSB, sheetSet.Album.AlbumDir, sheetSet.SheetTemplateFileMarkSB, count);

         // Создание листов Марок АР
         using (Database dbMarkSB = new Database(false, true))
         {
            Database dbOrig = _markSB.IdBtr.Database;
            dbMarkSB.ReadDwgFile(_fileMarkSB, FileShare.ReadWrite, false, "");
            dbMarkSB.CloseInput(true);

            // Замена блока рамки
            blFrameFacade.ChangeBlockFrame(dbMarkSB, Settings.Default.BlockFrameName);

            // Копирование всех определений блоков марки АР в файл Марки СБ
            CopyBtrMarksARToSheetMarkSB(dbMarkSB);

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
            lm.DeleteLayout(Settings.Default.SheetTemplateLayoutNameForMarkAR);

            HostApplicationServices.WorkingDatabase = dbOrig;
            dbMarkSB.SaveAs(_fileMarkSB, DwgVersion.Current);
         }
      }

      // Копирование определений блоков Марок АР в чертеж листов Марки СБ.
      private void CopyBtrMarksARToSheetMarkSB(Database dbMarkSB)
      {
         Database dbSource = _markSB.IdBtr.Database;
         var idsCopy = new ObjectIdCollection();
         foreach (var sheetMarkAr in _sheetsMarkAR)
         {
            idsCopy.Add(sheetMarkAr.MarkAR.IdBtrAr);
         }
         IdMapping map = new IdMapping();
         dbSource.WblockCloneObjects(idsCopy, dbMarkSB.BlockTableId, map, DuplicateRecordCloning.Replace, false);
         foreach (var sheetMarkAr in _sheetsMarkAR)
         {
            sheetMarkAr.IdBtrArSheet = map[sheetMarkAr.MarkAR.IdBtrAr].Value;
         }
      }

      // Создание файла Марки СБ
      private string CreateFileMarkSB(MarkSb markSB, string albumFolder, string templateFileMarkSB, int count)
      {
         string fileDest = Path.Combine(albumFolder, count.ToString("00") + "_" + markSB.MarkSbName + ".dwg");
         File.Copy(templateFileMarkSB, fileDest);
         return fileDest;
      }

      // Слои для заморозки на видовых экранах на листах панелей
      private void GetLayersToFreezeOnSheetsPanel(Database dbMarkSB, out List<ObjectId> layersToFreezeOnFacadeSheet, out List<ObjectId> layersToFreezeOnFormSheet)
      {
         layersToFreezeOnFacadeSheet = new List<ObjectId>();
         layersToFreezeOnFormSheet = new List<ObjectId>();
         using (var t = dbMarkSB.TransactionManager.StartTransaction())
         {
            var lt = t.GetObject(dbMarkSB.LayerTableId, OpenMode.ForRead) as LayerTable;
            // Слой размеров на фасаде
            if (lt.Has(Settings.Default.LayerDimensionFacade))
            {
               layersToFreezeOnFormSheet.Add(lt[Settings.Default.LayerDimensionFacade]);
            }
            // Слой окон
            if (lt.Has(Settings.Default.LayerWindows))
            {
               layersToFreezeOnFormSheet.Add(lt[Settings.Default.LayerWindows]);
            }
            // Слой размеров в форме
            if (lt.Has(Settings.Default.LayerDimensionForm))
            {
               layersToFreezeOnFacadeSheet.Add(lt[Settings.Default.LayerDimensionForm]);
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

            // отключение печати слоя АР_Марки
            if (lt.Has(Settings.Default.LayerMarks))
            {
               var layMarks = lt[Settings.Default.LayerMarks].GetObject(OpenMode.ForRead) as LayerTableRecord;
               if (layMarks.IsPlottable)
               {
                  layMarks.UpgradeOpen();
                  layMarks.IsPlottable = false;
               }
            }
            t.Commit();
         }
      }
   }
}
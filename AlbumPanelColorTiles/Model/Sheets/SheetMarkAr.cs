using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Sheets
{
   // Лист Марки АР (на фасаде и в форме)
   public class SheetMarkAr : IComparable<SheetMarkAr>
   {
      // Данные для заполнения штампа
      private readonly string _sheetName;

      private ObjectId _idBtrArSheet; // опредедление блока марки АР в этом файле.

      // Блок Марки АР.
      // Лист раскладки плитки на фасаде
      // Лист раскладки плитки в форме (зеркально, без видов и разрезов панели).
      private MarkArPanelAR _markAR;

      private Point3d _ptInsertBlRefMarkAR;

      // Наименование листа

      private int _sheetNumber;
      private string _sheetNumberInForm;

      public SheetMarkAr(MarkArPanelAR markAR)
      {
         _markAR = markAR;
         _sheetName = string.Format("Наружная стеновая панель {0}", MarkArFullName);
      }

      public ObjectId IdBtrArSheet { get { return _idBtrArSheet; } set { _idBtrArSheet = value; } }

      public string LayoutName
      {
         get { return _sheetNumber.ToString("00"); }
      }

      public MarkArPanelAR MarkAR { get { return _markAR; } }

      public string MarkArFullName { get { return _markAR.MarkARPanelFullName; } }

      public int SheetNumber
      {
         get { return _sheetNumber; }
         set { _sheetNumber = value; }
      }

      public string SheetNumberInForm
      {
         get { return _sheetNumberInForm; }
         set { _sheetNumberInForm = value; }
      }

      public int CompareTo(SheetMarkAr other)
      {
         return _markAR.MarkPainting.CompareTo(other._markAR.MarkPainting);
      }

      // Создание листа в файле марки СБ.
      public void CreateLayout(Database dbMarkSB, Point3d pt, List<ObjectId> layersToFreezeOnFacadeSheet, List<ObjectId> layersToFreezeOnFormSheet)
      {
         _ptInsertBlRefMarkAR = pt;
         // Определения блоков марок АР уже скопированы.

         using (var t = dbMarkSB.TransactionManager.StartTransaction())
         {
            //
            //Создание листа для Марки АР ("на Фасаде").
            //
            var idLayoutMarkAR = AcadLib.Blocks.Block.CopyLayout(dbMarkSB, Settings.Default.SheetTemplateLayoutNameForMarkAR, LayoutName);
            // Для первого листа марки АР нужно поменять местами имена листов шаблона и Марки АР (чтобы удалить потом лист шаблона)
            if ((t.GetObject(dbMarkSB.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary).Count == 3)
            {
               Block.ConvertLayoutNames(dbMarkSB, Settings.Default.SheetTemplateLayoutNameForMarkAR, LayoutName);
               HostApplicationServices.WorkingDatabase = dbMarkSB;
               LayoutManager lm = LayoutManager.Current;
               idLayoutMarkAR = lm.GetLayoutId(LayoutName);
            }
            // Вставка блока Марки АР.
            var idBlRefMarkAR = InsertBlRefMarkAR(dbMarkSB, _ptInsertBlRefMarkAR);
            // Направение видового экрана на блок марки АР.
            Extents3d extentsViewPort;
            var idBtrLayoutMarkAR = ViewPortSettings(idLayoutMarkAR, idBlRefMarkAR, t,
                              dbMarkSB, layersToFreezeOnFacadeSheet, null, true, out extentsViewPort);
            // Заполнение таблицы
            ObjectId idTable;
            FillTableTiles(idBtrLayoutMarkAR, out idTable);

            // Проверка расположения таблицы
            CheckTableExtents(extentsViewPort, idTable);

            // Заполнение штампа
            FillingStampMarkAr(idBtrLayoutMarkAR, true);

            //
            // Создание листа "в Форме" (зеркально)
            //
            // Копирование вхождения блока и зеркалирование
            Point3d ptInsertMarkArForm = new Point3d(_ptInsertBlRefMarkAR.X, _ptInsertBlRefMarkAR.Y - 10000, 0);
            var idBlRefMarkArForm = InsertBlRefMarkAR(dbMarkSB, ptInsertMarkArForm);
            // Зеркалирование блока
            MirrorMarkArForFormSheet(idBlRefMarkArForm);
            var idLayoutMarkArForm = AcadLib.Blocks.Block.CopyLayout(dbMarkSB, LayoutName, LayoutName + ".1");
            // Направение видового экрана на блок марки АР(з).
            var idBtrLayoutMarkArForm = ViewPortSettings(idLayoutMarkArForm, idBlRefMarkArForm, t,
                           dbMarkSB, layersToFreezeOnFormSheet, layersToFreezeOnFacadeSheet, false, out extentsViewPort);
            // Заполнение штампа
            FillingStampMarkAr(idBtrLayoutMarkArForm, false);

            t.Commit();
         }
      }

      private void CheckTableExtents(Extents3d extentsViewPort, ObjectId idTable)
      {
         // Таблица выходит за границы видового экрана. (Видовой экран, как ориентир)
         using (var table = idTable.GetObject(OpenMode.ForRead) as Table)
         {
            var extTable = table.GeometricExtents;
            if (!extentsViewPort.IsPointInBounds(extTable.MinPoint))
            {
               Point3d newPt = new Point3d(table.Position.X,
                  extentsViewPort.MinPoint.Y + (extTable.MaxPoint.Y - extTable.MinPoint.Y), 0);
               table.UpgradeOpen();
               table.Position = newPt;
            }
         }
      }

      // Заполнение штампа содержания.
      private void FillingStampMarkAr(ObjectId idBtrLayout, bool isFacadeView)
      {
         var btrLayout = idBtrLayout.GetObject(OpenMode.ForRead) as BlockTableRecord;
         var blRefStamp = FindStamp(btrLayout);
         string textView;
         string textNumber;
         if (isFacadeView)
         {
            textView = "Раскладка плитки на фасаде";
            textNumber = SheetNumber.ToString();
         }
         else
         {
            textView = "Раскладка плитки в форме";
            textNumber = SheetNumberInForm;
         }

         var atrs = blRefStamp.AttributeCollection;
         foreach (ObjectId idAtrRef in atrs)
         {
            if (idAtrRef.IsErased) continue;
            var atrRef = idAtrRef.GetObject(OpenMode.ForRead) as AttributeReference;
            string text = string.Empty;
            if (atrRef.Tag.Equals("Наименование", StringComparison.OrdinalIgnoreCase))
            {
               text = _sheetName;
            }
            else if (atrRef.Tag.Equals("Лист", StringComparison.OrdinalIgnoreCase))
            {
               text = textNumber;
            }
            else if (atrRef.Tag.Equals("Вид", StringComparison.OrdinalIgnoreCase))
            {
               text = textView;
            }
            if (text != string.Empty)
            {
               atrRef.UpgradeOpen();
               atrRef.TextString = text;
            }
         }
      }

      // Создание и Заполнение таблицы расхода плитки
      private void FillTableTiles(ObjectId idBtrLayoutMarkAR, out ObjectId idTable)
      {
         var btrLayout = idBtrLayoutMarkAR.GetObject(OpenMode.ForRead) as BlockTableRecord;
         // Поиск таблицы на листе
         using (Table table = FindTable(btrLayout))
         {
            idTable = table.ObjectId;
            // Расчет плитки
            List<TileCalc> tilesCalc = _markAR.TilesCalc;
            tilesCalc.Sort(); // Сортировка расчетных плиток по количеству.
            // Установка размера таблицы.
            if (table.Rows.Count > 3)
            {
               table.DeleteRows(3, table.Rows.Count - 3);
               table.SetSize(tilesCalc.Count + 3, table.Columns.Count);
            }
            // Заголовок
            table.Cells[0, 0].TextString = "Расход плитки на панель " + MarkArFullName;
            // Подсчет плитки
            int row = 2;

            // Заполнение строк таблицы
            foreach (var tileCalc in tilesCalc)
            {
               table.Cells[row, 1].TextString = tileCalc.ColorMark;
               table.Cells[row, 2].BackgroundColor = tileCalc.Pattern;
               table.Cells[row, 3].TextString = tileCalc.Count.ToString();
               table.Cells[row, 3].Alignment = CellAlignment.MiddleCenter;
               table.Cells[row, 4].TextString = tileCalc.TotalArea.ToString();
               table.Cells[row, 4].Alignment = CellAlignment.MiddleCenter;
               row++;
            }
            // Строка итогов.
            // Объединить строку итогов (1,2 и 3 столбцы).
            table.MergeCells(CellRange.Create(table, row, 0, row, 2));
            table.Cells[row, 0].TextString = "Итого на панель:";
            table.Cells[row, 0].Alignment = CellAlignment.MiddleCenter;
            table.Cells[row, 3].TextString = _markAR.Paints.Count.ToString();//  totalCount.ToString();
            table.Cells[row, 3].Alignment = CellAlignment.MiddleCenter;
            table.Cells[row, 4].TextString = _markAR.MarkSB.TotalAreaTiles.ToString();//totalArea.ToString();
            table.Cells[row, 4].Alignment = CellAlignment.MiddleCenter;

            table.RecomputeTableBlock(true);
            //table.Dispose();//???
         }
      }

      // Поиск штампа на листе
      private BlockReference FindStamp(BlockTableRecord btrLayout)
      {
         foreach (ObjectId idEnt in btrLayout)
         {
            if (idEnt.ObjectClass.Name == "AcDbBlockReference")
            {
               var blRefStampContent = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
               if (blRefStampContent.GetEffectiveName() == Settings.Default.BlockFrameName)
               {
                  return blRefStampContent;
               }
            }
         }
         throw new Exception("Не найден блок штампа на листе в файле шаблона Марки СБ.");
      }

      // Поиск таблицы на листе
      private Table FindTable(BlockTableRecord btrLayout)
      {
         foreach (ObjectId idEnt in btrLayout)
         {
            if (idEnt.ObjectClass.Name == "AcDbTable")
            {
               return idEnt.GetObject(OpenMode.ForWrite, false, true) as Table;
            }
         }
         throw new Exception("Не найдена заготовка таблицы на листе в файле шаблона Марки СБ.");
      }

      // Определение границ блока
      private Extents3d GetBoundsBlRefMarkAR(BlockReference blRef)
      {
         return blRef.Bounds.Value;
      }

      // Поиск видового экрана на листе
      private ObjectId GetViewport(ObjectId idLayoutMarkAR, Transaction t)
      {
         ObjectId idVp = ObjectId.Null;
         var layoutMarkAR = t.GetObject(idLayoutMarkAR, OpenMode.ForRead) as Layout;
         var btrLayout = t.GetObject(layoutMarkAR.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
         foreach (ObjectId idEnt in btrLayout)
         {
            if (idEnt.ObjectClass.Name != "AcDbViewport") continue;
            var vp = t.GetObject(idEnt, OpenMode.ForRead) as Viewport;
            if (vp.Layer != "АР_Видовые экраны") continue;
            idVp = idEnt;
            break;
         }
         return idVp;
      }

      private ObjectId InsertBlRefMarkAR(Database dbMarkSB, Point3d ptInsert)
      {
         ObjectId idBlRefMarkAR = ObjectId.Null;
         using (var bt = dbMarkSB.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            using (var blRefMarkAR = new BlockReference(ptInsert, _idBtrArSheet))
            {
               using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(dbMarkSB).GetObject(OpenMode.ForWrite) as BlockTableRecord)
               {
                  idBlRefMarkAR = ms.AppendEntity(blRefMarkAR);
               }
            }
         }
         return idBlRefMarkAR;
      }

      private void MirrorMarkArForFormSheet(ObjectId idBlRefMarkArForm)
      {
         using (var blRefMarkArMirr = idBlRefMarkArForm.GetObject(OpenMode.ForWrite, false, true) as BlockReference)
         {
            var boundsBlRef = blRefMarkArMirr.Bounds.Value;
            Point3d ptCentreX = new Point3d((boundsBlRef.MinPoint.X + boundsBlRef.MaxPoint.X) * 0.5, 0, 0);
            Line3d lineMirr = new Line3d(ptCentreX, new Point3d(ptCentreX.X, 1000, 0));
            blRefMarkArMirr.TransformBy(Matrix3d.Mirroring(lineMirr));
         }
      }

      private void ViewPortDirection(Viewport vp, Database dbMarkSB, Point2d ptCenterPanel)
      {
         // "прицеливание" ВЭ на нужный фрагмент пространства модели
         var ptCentre = new Point2d(ptCenterPanel.X, ptCenterPanel.Y - Settings.Default.SheetPanelEndUp); // 1400); // SheetPanelEndUp
         vp.ViewCenter = ptCentre;

         //ObjectContextManager ocm = dbMarkSB.ObjectContextManager;
         //ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
         //vp.AnnotationScale = (AnnotationScale)occ.GetContext("1:25");
         vp.CustomScale = 1d / Settings.Default.SheetScale; // 0.04; // SheetScale
      }

      // Направление видового экрана на блок Марки АР
      private ObjectId ViewPortSettings(ObjectId idLayoutMarkAR, ObjectId idBlRefMarkAR,
         Transaction t, Database dbMarkSB, List<ObjectId> layersToFreeze, List<ObjectId> layersToThaw, bool isFacadeView, out Extents3d extentsViewPort)
      {
         ObjectId idBtrLayout = ObjectId.Null;
         // Поиск видового экрана
         var idVP = GetViewport(idLayoutMarkAR, t);
         var vp = t.GetObject(idVP, OpenMode.ForWrite) as Viewport;

         extentsViewPort = vp.GeometricExtents;

         // Отключение слоя на видовом экране
         if (layersToFreeze != null && layersToFreeze.Count > 0)
         {
            vp.FreezeLayersInViewport(layersToFreeze.GetEnumerator());
         }
         if (layersToThaw != null && layersToThaw.Count > 0)
         {
            vp.ThawLayersInViewport(layersToThaw.GetEnumerator());
         }

         idBtrLayout = vp.OwnerId;
         var blRefMarkAr = t.GetObject(idBlRefMarkAR, OpenMode.ForRead, false, true) as BlockReference;
         // Определение границ блока
         Point2d ptCenterMarkAR;
         if (isFacadeView)
         {
            if (_markAR.MarkSB.IsEndLeftPanel && !_markAR.MarkSB.IsEndRightPanel)
               ptCenterMarkAR = new Point2d(blRefMarkAr.Position.X + _markAR.MarkSB.CenterPanel.X + Settings.Default.SheetPanelEndShift,
                                            blRefMarkAr.Position.Y + _markAR.MarkSB.CenterPanel.Y);
            else if (_markAR.MarkSB.IsEndRightPanel && !_markAR.MarkSB.IsEndLeftPanel)
               ptCenterMarkAR = new Point2d(blRefMarkAr.Position.X + _markAR.MarkSB.CenterPanel.X - Settings.Default.SheetPanelEndShift,
                                            blRefMarkAr.Position.Y + _markAR.MarkSB.CenterPanel.Y);
            else
               ptCenterMarkAR = new Point2d(blRefMarkAr.Position.X + _markAR.MarkSB.CenterPanel.X,
                                             blRefMarkAr.Position.Y + _markAR.MarkSB.CenterPanel.Y);
         }
         else
         {
            // Диман гоаорит не нужно сдвигать торцевые панели в форме.
            ptCenterMarkAR = new Point2d(blRefMarkAr.Position.X - _markAR.MarkSB.CenterPanel.X,
                                    blRefMarkAr.Position.Y + _markAR.MarkSB.CenterPanel.Y);
         }
         ViewPortDirection(vp, dbMarkSB, ptCenterMarkAR);
         vp.Dispose();
         return idBtrLayout;
      }
   }
}
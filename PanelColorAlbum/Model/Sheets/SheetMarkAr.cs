using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Лист Марки АР.
   public class SheetMarkAr
   {
      // Блок Марки АР.
      // Лист раскладки плитки на фасаде
      // Лист раскладки плитки в форме (зеркально, без видов и разрезов панели).
      private MarkArPanel _markAR;

      private Point3d _pt;
      private Database _dbMarkSB;

      // Создание листа Марки АР
      public SheetMarkAr(MarkArPanel markAR, Database dbMarkSB, Point3d pt)
      {
         _dbMarkSB = dbMarkSB;
         _markAR = markAR;
         _pt = pt;
         // Определения блоков марок АР уже скопированы.

         using (var t = dbMarkSB.TransactionManager.StartTransaction())
         {
            // Вставка блока Марки АР.
            var idBlRefMarkAR = InsertBlRefMarkAR();
            //Создание листа для Марки АР ("на Фасаде").
            var idLayoutMarkAR = CreateLayoutMarkAR();
            // Направение видового экрана на блок марки АР.
            var idBtrLayoutMarkAR = ViewPortSettings(idLayoutMarkAR, idBlRefMarkAR, t);
            // Заполнение таблицы
            CreateTableTiles(idBtrLayoutMarkAR, t);

            // Создание листа "в Форме" (зеркально)

            t.Commit();
         }
      }

      // Создание и Заполнение таблицы расхода плитки
      private void CreateTableTiles(ObjectId idBtrLayoutMarkAR, Transaction t)
      {
         // Поиск таблицы на листе
         Table table = FindTable(idBtrLayoutMarkAR, t);
         table.SetSize(3, 5);

         // Расчет плитки
         var tilesCalc = _markAR.TilesCalc;
         // Установка размера таблицы.
         if (table.Rows.Count < tilesCalc.Count + 3)
         {
            table.SetSize(tilesCalc.Count + 3, table.Columns.Count);
         }

         // Заголовок
         table.Cells[0, 0].TextString = "Расход плитки на панель " + _markAR.MarkARPanelFullName;
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
         var totalCount = tilesCalc.Sum(c => c.Count);
         var totalArea = tilesCalc.Sum(c => c.TotalArea);
         // Объединить строку итогов (1,2 и 3 столбцы).
         table.MergeCells(CellRange.Create(table, row, 0, row, 2));
         table.Cells[row, 0].TextString = "Итого на панель";
         table.Cells[row, 0].Alignment = CellAlignment.MiddleCenter;
         table.Cells[row, 3].TextString = totalCount.ToString();
         table.Cells[row, 3].Alignment = CellAlignment.MiddleCenter;
         table.Cells[row, 4].TextString = totalArea.ToString();
         table.Cells[row, 4].Alignment = CellAlignment.MiddleCenter;
      }

      // Поиск таблицы на листе
      private Table FindTable(ObjectId idBtrLayoutMarkAR, Transaction t)
      {
         Table table = null;
         var btrLayout = t.GetObject(idBtrLayoutMarkAR, OpenMode.ForRead) as BlockTableRecord;
         foreach (ObjectId idEnt in btrLayout)
         {
            if (idEnt.ObjectClass.Name == "AcDbTable")
            {
               table = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as Table;
               break;
            }
         }

         return table;
      }

      // Направление видового экрана на блок Марки АР
      private ObjectId ViewPortSettings(ObjectId idLayoutMarkAR, ObjectId idBlRefMarkAR, Transaction t)
      {
         ObjectId idBtrLayout = ObjectId.Null;
         // Поиск видового экрана
         var idVP = GetViewport(idLayoutMarkAR, t);
         var vp = t.GetObject(idVP, OpenMode.ForWrite) as Viewport;
         idBtrLayout = vp.OwnerId;
         var blRef = t.GetObject(idBlRefMarkAR, OpenMode.ForRead) as BlockReference;
         // Определение границ блока
         Extents3d bounds = GetBoundsBlRefMarkAR(blRef);
         ViewPortDirection(vp, bounds);
         return idBtrLayout;
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
         //var idsVPid = layoutMarkAR.GetViewports(); // 0
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

      // Создание листа для Марки АР
      private ObjectId CreateLayoutMarkAR()
      {
         ObjectId idLayoutMarAr = ObjectId.Null;
         // Копирование листа шаблона
         Database dbOrig = HostApplicationServices.WorkingDatabase;
         HostApplicationServices.WorkingDatabase = _dbMarkSB;
         LayoutManager lm = LayoutManager.Current;
         if (lm.CurrentLayout == Album.Options.SheetTemplateLayoutNameForMarkAR)
         {
            lm.RenameLayout(lm.CurrentLayout, _markAR.MarkARPanelFullName);
         }
         else
         {
            lm.CopyLayout(lm.CurrentLayout, _markAR.MarkARPanelFullName);
         }
         idLayoutMarAr = lm.GetLayoutId(_markAR.MarkARPanelFullName);
         HostApplicationServices.WorkingDatabase = dbOrig;
         return idLayoutMarAr;
      }

      private ObjectId InsertBlRefMarkAR()
      {
         ObjectId idBlRefMarkAR = ObjectId.Null;
         using (var bt = _dbMarkSB.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            using (var blRefMarkAR = new BlockReference(_pt, bt[_markAR.MarkArBlockName]))
            {
               using (var ms = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord)
               {
                  idBlRefMarkAR = ms.AppendEntity(blRefMarkAR);
               }
            }
         }
         return idBlRefMarkAR;
      }

      /*------------------------------------------------------------------------------------------------------------------*/
      /* Процедура создания видового экрана, указывающего на нужный участок пространства модели, соответствующий странице */
      /* Входные данные:                                                                                                  */
      /* newViewPortId - идентификатор нового видового экрана                                                             */
      /* newLayoutId - идентификатор листа, на котором расположен ВЭ                                                      */
      /* minPoint - левая нижняя точка ВЭ в пространстве модели                                                           */
      /* maxPoint - правая верхняя точка ВЭ в пространстве модели                                                         */
      /* startPoint - стартовая точка ВЭ: центр видового экрана                                                           */
      /*------------------------------------------------------------------------------------------------------------------*/

      private void ViewPortDirection(Viewport vp, Extents3d bounds)
      {
         Point3d maxPoint = bounds.MaxPoint;
         Point3d minPoint = bounds.MinPoint;
         //Point3d startPoint = new Point3d((bounds.MaxPoint.X + bounds.MinPoint.X) * 0.5,
         //(bounds.MaxPoint.Y + bounds.MinPoint.Y) * 0.5, 0);

         // формирование размеров видового экрана в листе
         //vp.CenterPoint = startPoint.Add(new Vector3d(0, -0.15, 0));
         //vp.Height = maxPoint.Y - minPoint.Y + 0.3;
         //vp.Width = maxPoint.X - minPoint.X;
         //vp.CustomScale = 1;    // масштаб ВЭ - 1:1.

         // "прицеливание" ВЭ на нужный фрагмент пространства модели
         vp.ViewCenter = new Point2d((maxPoint.X - minPoint.X) / 2 + minPoint.X, (maxPoint.Y - minPoint.Y) / 2 + minPoint.Y).Add(new Vector2d(0, -0.15));
         vp.ViewHeight = maxPoint.Y - minPoint.Y + 0.3;

         //vp.Locked = true;              // ВЭ блокируется
         //vp.On = true;                  // включен и видим
         //vp.Visible = true;
         ObjectContextManager ocm = _dbMarkSB.ObjectContextManager;
         ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
         vp.AnnotationScale = (AnnotationScale)occ.GetContext("1:25");
         vp.CustomScale = 0.04;
      }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Jigs;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Model.Utils.AirConditioners
{
    public class AirCondTable
    {        
        Database db;
        Editor ed;
        Document doc;
        List<AirCondRow> condRows;

        public AirCondTable(List<AirCondRow> condRows)
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;            
            this.condRows = condRows;
        }

        /// <summary>
        /// Создание таблицы спецификации блоков, с запросом выбора блоков у пользователя.
        /// Таблица будет вставлена в указанное место пользователем в текущем пространстве.
        /// </summary>
        public void CreateTable()
        {
            using (var t = db.TransactionManager.StartTransaction())
            {
                // Создание таблицы
                Table table = getTable();
                // Вставка таблицы
                insertTable(table);
                t.Commit();
            }
        }

        private Table getTable()
        {
            Table table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = db.GetTableStylePIK(true); // если нет стиля ПИк в этом чертеже, то он скопируетс из шаблона, если он найдется            
            // Измпнение отступа в стилше ПИК на 1
            UpdateTableStyle(table.TableStyle);

            bool hasTotalRow = (condRows.Count > 1);
            int rows = hasTotalRow ? condRows.Count + 3 : condRows.Count + 2;            
            
            table.SetSize(rows, 5);

            table.SetRowHeight(8);

            // Название таблицы
            var rowTitle = table.Cells[0, 0];
            rowTitle.Alignment = CellAlignment.MiddleCenter;            
            rowTitle.TextString = "Спецификация на наружное ограждение блока кондиционера";

            // столбец Марка
            var col = table.Columns[0];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 10;
            // столбец Цвет.
            col = table.Columns[1];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 35;
            // столбец Образец
            col = table.Columns[2];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            // столбец Кол
            col = table.Columns[3];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            // столбец Прим
            col = table.Columns[4];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = hasTotalRow? 40 : 60;            

            // Заголовок Маркв
            var cellColName = table.Cells[1, 0];
            cellColName.TextString = "Марка";
            //cellColName.Alignment = CellAlignment.MiddleCenter;
            // Заголовок Цвет
            cellColName = table.Cells[1, 1];
            cellColName.TextString = "Цвет";
            cellColName.Alignment = CellAlignment.MiddleCenter;
            // Заголовок Образец
            cellColName = table.Cells[1, 2];
            cellColName.TextString = "Образец";
            //cellColName.Alignment = CellAlignment.MiddleCenter;
            // Заголовок Кол
            cellColName = table.Cells[1, 3];
            cellColName.TextString = "Кол-во, шт.";
            //cellColName.Alignment = CellAlignment.MiddleCenter;
            // Заголовок прим
            cellColName = table.Cells[1, 4];
            cellColName.TextString = "Примечание";
            //cellColName.Alignment = CellAlignment.MiddleCenter;            

            // Строка заголовков столбцов
            var rowHeaders = table.Rows[1];
            rowHeaders.Height = 15;
            var lwBold = rowHeaders.Borders.Top.LineWeight;
            rowHeaders.Borders.Bottom.LineWeight = lwBold;

            int row = 2;            
            foreach (var itemRow in condRows)
            {                
                table.Cells[row, 0].TextString = itemRow.Mark.ToString();                
                table.Cells[row, 1].TextString = itemRow.ColorName;
                table.Cells[row, 2].BackgroundColor = itemRow.Color;
                table.Cells[row, 3].TextString = itemRow.Count.ToString();                
                row++;
            }

            // Объединение итого
            if (hasTotalRow)
            {
                table.MergeCells(CellRange.Create(table, row, 0, row, 2));
                table.Cells[row, 0].TextString = "Итого на фасад";
                table.Cells[row, 3].TextString = condRows.Sum(c => c.Count).ToString();

                // Объединение примечания                        
                table.MergeCells(CellRange.Create(table, 2, 4, row, 4));
                table.Rows[row].Borders.Top.LineWeight = lwBold;
            }
            
            table.Cells[2, 4].TextString = "Стальной перфорированный лист, окрашенный порошковой эмалью в цвет по таблице.";

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }       

        private void insertTable(Table table)
        {
            double scale = db.TileMode ? 1 / db.Cannoscale.Scale : 1;
            TableJig jigTable = new TableJig(table, scale, "Вставка спецификации кондиционеров");
            if (ed.Drag(jigTable).Status == PromptStatus.OK)
            {
                var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                cs.AppendEntity(table);
                db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(table, true);
            }
        }

        private void UpdateTableStyle(ObjectId tableStyle)
        {
            using (var ts = db.Tablestyle.Open(OpenMode.ForWrite) as TableStyle)
            {
                ts.SetMargin(CellMargins.Left, 1, "_DATA");
            }
        }
    }
}

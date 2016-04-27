using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.ChangeJob
{
    public class ChangeJobTable
    {
        private List<ChangePanel> changePanels;
        private Database db;
        private Point3d ptPlan;

        public ChangeJobTable(List<ChangePanel> changePanels, Database db, Point3d ptPlan)
        {
            this.changePanels = changePanels;
            this.db = db;
            this.ptPlan = ptPlan;
        }

        public void Create(BlockTableRecord btr, Transaction t)
        {
            var table = GetTable();
            table.Position = ptPlan;
            table.TransformBy(Matrix3d.Scaling(100, table.Position));
            btr.AppendEntity(table);
            t.AddNewlyCreatedDBObject(table, true);
        }

        public Table GetTable()
        {
            Table table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = db.GetTableStylePIK("ПИК", true); // если нет стиля ПИк в этом чертеже, то он скопируетс из шаблона, если он найдется

            int rows = changePanels.Count + 2;
            table.SetSize(rows, 4);
            table.SetBorders(LineWeight.LineWeight050);

            // Название таблицы
            var rowTitle = table.Cells[0, 0];
            rowTitle.Alignment = CellAlignment.MiddleCenter;
            rowTitle.TextString = "Изменение марок покраски. " + DateTime.Now;

            // столбец 1
            var col = table.Columns[0];
            col.Width = 10;
            col.Alignment = CellAlignment.MiddleCenter;

            // столбец 1
            col = table.Columns[1];            
            col.Width = 50;

            // столбец 2
            col = table.Columns[2];
            col.Width = 50;

            // столбец 3
            col = table.Columns[3];
            col.Width = 50;

            // Заголовок 1
            var cellColName = table.Cells[1, 0];
            cellColName.TextString = "№пп";
            // Заголовок 2
            cellColName = table.Cells[1, 1];
            cellColName.TextString = "Марка панели";
            // Заголовок 3
            cellColName = table.Cells[1, 2];
            cellColName.TextString = "Новая марка покраски";
            // Заголовок 4
            cellColName = table.Cells[1, 3];
            cellColName.TextString = "Старая марка покраски";

            // Строка заголовков столбцов
            var rowHeaders = table.Rows[1];            
            var lwBold = rowHeaders.Borders.Top.LineWeight;
            rowHeaders.Borders.Bottom.LineWeight = lwBold;

            int row = 2;            
            foreach (var item in changePanels)
            {
                table.Cells[row, 0].TextString = (row-1).ToString();
                table.Cells[row, 1].TextString = item.MarkSb;
                table.Cells[row, 2].TextString = item.PaintNew;
                table.Cells[row, 3].TextString = item.PaintOld;
                row++;
            }
            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }
    }
}

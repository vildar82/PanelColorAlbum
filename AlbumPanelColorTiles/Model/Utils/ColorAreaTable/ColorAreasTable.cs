using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.ColorAreaTable
{
    class ColorAreasTable : AcadLib.Tables.CreateTable
    {
        DataColorAreas data;
        public ColorAreasTable (DataColorAreas data, Database db) : base(db)
        {
            this.data = data;
        }

        public override void CalcRows ()
        {
            NumColumns = data.Colors.Count + 2;
            NumRows = data.Rows.Count+3;
            Title = "Панели " + DateTime.Now;
        }

        protected override void SetColumnsAndCap (ColumnsCollection columns)
        {
            var col = columns[0];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 35;
            col[1, 0].TextString = "Размер, (Высота х Ширина), мм";
            int c = 1;
            for (int i = 1; i <= data.Colors.Count; i++)
            {
                col = columns[i];
                col.Width = 30;
                col.Alignment = CellAlignment.MiddleCenter;
                var cell = col[1, c];
                cell.TextString = $"Цвет {data.Colors[i - 1].Item1.BlLayer}";
                cell.BackgroundColor = data.Colors[i - 1].Item1.Color;
                c++;
            }

            col = columns.Last();
            col.Width = 30;
            col.Alignment = CellAlignment.MiddleCenter;
            col[1, c].TextString ="Итого";
        }

        protected override void FillCells (Table table)
        {
            int row = 2;
            Cell cell;
            foreach (var drow in data.Rows)
            {
                cell = table.Cells[row, 0];
                cell.TextString = drow.Size;

                foreach (var item in drow.LayersCount)
                {
                    var colColor = data.Colors.Find(f=>f.Item1.BlLayer == item.Key.BlLayer);
                    int colIndex = data.Colors.IndexOf(colColor);
                    cell = table.Cells[row, colIndex+1];                    
                    //cell.Alignment = CellAlignment.MiddleLeft;
                    cell.TextString = item.Value.ToString();                    
                    //cell.BackgroundColor = item.Key.Color;
                }
                cell = table.Cells[row, table.Columns.Count-1];
                cell.Alignment = CellAlignment.MiddleCenter;
                cell.TextString = TupleValueToString(drow.Area.Item1, drow.Area.Item2);

                row++;
            }

            table.Cells[row, 0].TextString = $"Итого";

            for (int i = 0; i < data.Colors.Count; i++)
            {
                var item = data.Colors[i];
                cell = table.Cells[row,i+1];
                cell.Alignment = CellAlignment.MiddleCenter;
                cell.TextString = TupleValueToString(item.Item2, item.Item3);
            }

            table.Cells[row, table.Columns.Count - 1].TextString = TupleValueToString(data.Total.Item1, data.Total.Item2);
        }

        private string TupleValueToString (int count, double area)
        {
            // %
            var percent =Math.Round(count * 100d / data.Total.Item1,1);
            return $"{count} ({area}м{AcadLib.General.Symbols.Square}) {percent}%";
        }
    }
}

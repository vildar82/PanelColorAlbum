using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Tables;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.TileTable
{
    public class TileTable : CreateTable
    {
        TileData data;        

        public TileTable (Database db, TileData data) : base(db)
        {
            this.data = data;
            Title = "";
        }

        public override void CalcRows ()
        {
            NumColumns = 6;
            if (data.TileInPanels.Count>0)
            {
                NumRows = data.TileInPanels.Count + 3;
            }
            if (data.TileInMonolith.Count > 0)
            {
                NumRows += data.TileInMonolith.Count + 3;
                if (data.TileInPanels.Count > 0)
                    NumRows++;
            }
        }

        protected override void SetColumnsAndCap (ColumnsCollection columns)
        {
            // столбец Поз.
            var col = columns[0];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 10;
            //col[1, 0].TextString = "Поз.";
            // столбец Артикул
            col = columns[1];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 20;
            //col[1, 1].TextString = "Артикул";
            //col[1, 1].Alignment = CellAlignment.MiddleCenter;
            // столбец Образец
            col = columns[2];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            //col[1, 2].TextString = "Образец";
            // столбец Расход, шт.
            col = columns[3];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            //col[1, 3].TextString = "Расход, шт.";
            // столбец Расход, м.кв.
            col = columns[4];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
            //col[1, 4].TextString = "Расход, м.кв.";
            // столбец Цвет
            col = columns[5];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 30;
            //col[1, 5].TextString = "Цвет";
            //col[1, 5].Alignment = CellAlignment.MiddleCenter;
        }

        private void SetTitleAndCap (Table table,ref int row, string title)
        {
            var cell = table.Cells[row,0];
            if (cell.IsMerged == null || !cell.IsMerged.Value)
            {
                var mCells = CellRange.Create(table, row, 0, row , table.Columns.Count-1);
                table.MergeCells(mCells);
            }
            cell.TextString = title;
            row++;
            table.Rows[row].Height = 15;
            // столбец Поз.
            cell = table.Cells[row, 0];
            cell.TextString = "Поз.";
            // столбец Артикул
            cell = table.Cells[row, 1];
            cell.TextString = "Артикул";
            cell.Alignment = CellAlignment.MiddleCenter;
            // столбец Образец
            cell = table.Cells[row, 2];
            cell.TextString = "Образец";
            // столбец Расход, шт.
            cell = table.Cells[row, 3];
            cell.TextString = "Расход, шт.";
            // столбец Расход, м.кв.
            cell = table.Cells[row, 4];
            cell.TextString = "Расход, м.кв.";
            // столбец Цвет
            cell = table.Cells[row, 5];
            cell.TextString = "Цвет";
            cell.Alignment = CellAlignment.MiddleCenter;
            row++;       
        }

        protected override void FillCells (Table table)
        {
            int row = 0;            
            DateTime now = DateTime.Now;

            if (data.TileInPanels.Count>0)
            {
                SetTitleAndCap(table, ref row, "Расход плитки Сборной части " + now);                
                FillTiles(table,ref row, data.TileInPanels);
            }
            if (data.TileInMonolith.Count > 0)
            {
                if (data.TileInPanels.Count > 0)
                {
                    var mCells = CellRange.Create(table, row, 0, row, table.Columns.Count-1);
                    table.MergeCells(mCells);
                    row++;
                }
                    
                SetTitleAndCap(table,ref row, "Расход плитки Монолитной части " + now);

                var rowHeader = table.Rows[row - 1];                
                rowHeader.Borders.Bottom.LineWeight = lwBold;
                rowHeader.Borders.Top.LineWeight = lwBold;

                FillTiles(table,ref row, data.TileInMonolith);
            }
        }

        private void FillTiles (Table table,ref int row, List<IGrouping<Tile, Tile>> tiles)
        {            
            Cell cell;
            CellRange mCells;
            int index = 1;
            int totalCount = 0;
            double totalArea =0;

            foreach (var tile in tiles)
            {
                cell = table.Cells[row, 0];
                cell.TextString = index++.ToString();

                cell = table.Cells[row, 1];
                cell.TextString = tile.Key.Article;

                cell = table.Cells[row, 2];
                cell.BackgroundColor = tile.Key.Color;

                int count = tile.Count();
                totalCount += count;
                cell = table.Cells[row, 3];
                cell.TextString = count.ToString();

                cell = table.Cells[row, 4];
                var area = Math.Round(count * Panels.TileCalc.OneTileArea, 2);
                totalArea += area;
                cell.TextString = area.ToString("0.00");

                cell = table.Cells[row, 5];
                cell.TextString = tile.Key.NCS;

                row++;
            }

            var rowHeader = table.Rows[row];
            rowHeader.Borders.Top.LineWeight = lwBold;
            rowHeader.Borders.Bottom.LineWeight = lwBold;

            mCells = CellRange.Create(table, row, 0, row, 2);
            table.MergeCells(mCells);
            cell = table.Cells[row, 0];
            cell.TextString = "Итого:";
            cell.Alignment = CellAlignment.MiddleCenter;

            cell = table.Cells[row, 3];
            cell.TextString = totalCount.ToString();
            cell = table.Cells[row, 4];
            cell.TextString = totalArea.ToString();

            row++;
        }
    }
}

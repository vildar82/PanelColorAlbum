using AcadLib.Jigs;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Sheets
{
    // таблица общего расхода плитки на альбом
    public class TotalTileTable
    {
        private Album _album;

        public TotalTileTable(Album album)
        {
            _album = album;
        }

        // вставка итоговой таблицы расхода плитки на альбом
        public void InsertTableTotalTile()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // подсчет итогового кол плитки
            Table table = getTable(db);

            TableJig jigTable = new TableJig(table, 1 / db.Cannoscale.Scale, "\nВставка итоговой таблицы плитки на альбом");
            if (ed.Drag(jigTable).Status == PromptStatus.OK)
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    //table.ScaleFactors = new Scale3d(100);
                    var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    cs.AppendEntity(table);
                    t.AddNewlyCreatedDBObject(table, true);
                    t.Commit();
                }
            }
        }

        private Table getTable(Database db)
        {
            Table table = new Table();
            table.SetDatabaseDefaults(db);
            table.TableStyle = db.GetTableStylePIK(); //getTableStyle(db);

            bool hasColorName = Paint.HasColorName(_album.Colors);
            int numColumns = hasColorName? 6:5;            

            table.SetSize(_album.Colors.Count + 3, numColumns);
            table.Columns[0].Width = 10; // Поз
            table.Columns[1].Width = 20; // Артикул
            table.Columns[2].Width = 20; // Образец
            table.Columns[3].Width = 20; // Расход шт
            table.Columns[4].Width = 20; // Расход м.кв.
            if (hasColorName)
            {
                table.Columns[5].Width = 30; // Цвет
                table.Cells[1, 5].TextString = "Цвет";
            }

            table.Columns[0].Alignment = CellAlignment.MiddleCenter;
            table.Columns[3].Alignment = CellAlignment.MiddleCenter;
            table.Columns[4].Alignment = CellAlignment.MiddleCenter;

            table.Rows[1].Height = 15;

            table.Cells[0, 0].TextString = "Расход плитки на альбом " + _album.StartOptions.Abbr;
            table.Cells[1, 0].TextString = "Поз.";
            table.Cells[1, 1].TextString = "Артикул";
            table.Cells[1, 2].TextString = "Образец";
            table.Cells[1, 3].TextString = "Расход, шт.";
            table.Cells[1, 4].TextString = "Расход, м.кв.";

            int row = 2;
            int i = 1;
            int totalCountTile = 0;
            double totalArea = 0;

            foreach (var tileCalcSameColor in _album.TotalTilesCalc)
            {
                table.Cells[row, 0].TextString = i++.ToString(); //"Поз.";
                table.Cells[row, 1].TextString = tileCalcSameColor.Paint.Article;  //"Артикул";
                table.Cells[row, 2].BackgroundColor = tileCalcSameColor.Paint.Color;  // "Образец";
                table.Cells[row, 3].TextString = tileCalcSameColor.Count.ToString();// "Расход, шт.";
                table.Cells[row, 4].TextString = tileCalcSameColor.TotalArea.ToString();  // "Расход, м.кв.";
                if (hasColorName)
                {
                    table.Cells[row, 5].TextString = tileCalcSameColor.Paint.Name;  //"Цвет";
                }

                totalCountTile += tileCalcSameColor.Count;
                totalArea += tileCalcSameColor.TotalArea;

                row++;
            }
            var mCells = CellRange.Create(table, row, 0, row, 2);
            table.MergeCells(mCells);
            table.Cells[row, 0].TextString = "Итого:";
            table.Cells[row, 3].TextString = totalCountTile.ToString();
            table.Cells[row, 4].TextString = totalArea.ToString();

            table.GenerateLayout();
            return table;
        }
    }
}
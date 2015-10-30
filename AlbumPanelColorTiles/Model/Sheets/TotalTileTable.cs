using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Sheets
{
   // таблица общего расхода плитки на альбом
   public class TotalTileTable
   {
      private Album _album;

      public TotalTileTable (Album album)
      {
         _album = album;
      }

      // вставка итоговой таблицы расхода плитки на альбом
      public void InsertTableTotalTile(Document doc)
      {
         Database db = doc.Database;         
         Editor ed = doc.Editor;

         // подсчет итогового кол плитки
         Table table = getTable(db);        

         TableJig jigTable = new TableJig(table, 100);
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
         table.TableStyle = getTableStyle(db);

         table.SetSize(_album.Colors.Count + 3, 5);
         table.Columns[0].Width = 10; // Поз
         table.Columns[1].Width = 40; // Цвет
         table.Columns[2].Width = 20; // Образец
         table.Columns[3].Width = 20; // Расход шт         
         table.Columns[4].Width = 20; // Расход м.кв.

         table.Columns[0].Alignment = CellAlignment.MiddleCenter;
         table.Columns[3].Alignment = CellAlignment.MiddleCenter;
         table.Columns[4].Alignment = CellAlignment.MiddleCenter;

         table.Rows[1].Height = 15;

         table.Cells[0, 0].TextString = "Расход плитки на альбом " + _album.AbbreviateProject;
         table.Cells[1, 0].TextString = "Поз.";
         table.Cells[1, 1].TextString = "Цвет";
         table.Cells[1, 2].TextString = "Образец";
         table.Cells[1, 3].TextString = "Расход, шт.";
         table.Cells[1, 4].TextString = "Расход, м.кв.";

         int row = 2;
         int i = 1;
         double totalArea = 0;
         foreach (var paint in _album.Colors.OrderBy(p=>p.Count))
         {
            table.Cells[row, 0].TextString = i++.ToString(); //"Поз.";
            table.Cells[row, 1].TextString = paint.LayerName;  //"Цвет";
            table.Cells[row, 2].BackgroundColor = paint.Color;  // "Образец";            
            table.Cells[row, 3].TextString = paint.Count.ToString();// "Расход, шт.";
            var area = paint.Count * TileCalc.OneTileArea;
            totalArea += area;
            table.Cells[row, 4].TextString = Math.Round(area, 2).ToString();  // "Расход, м.кв.";
            row++;
         }
         var mCells = CellRange.Create(table, row, 0, row, 2);
         table.MergeCells(mCells);
         table.Cells[row, 0].TextString = "Итого:";
         table.Cells[row, 3].TextString = _album.Colors.Sum(p => p.Count).ToString();
         table.Cells[row, 4].TextString = Math.Round(totalArea, 2).ToString();

         table.GenerateLayout();
         return table;
      }

      private static ObjectId getTableStyle(Database db)
      {
         using (var dictTableStyles = db.TableStyleDictionaryId.Open(OpenMode.ForRead) as DBDictionary)
         {
            if (dictTableStyles.Contains("ПИК"))
            {
               return dictTableStyles.GetAt("ПИК");
            }
         }
         return db.Tablestyle;
      }
   }
}

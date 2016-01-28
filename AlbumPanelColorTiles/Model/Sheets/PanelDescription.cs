using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Sheets
{
   /// <summary>
   /// Класс для формирования текста описания панели - для листов
   /// </summary>
   public class PanelDescription
   {
      private Database db;
      private MarkSb markSB;
      private static Point3d ptMtextCenter = new Point3d(180.0, 45.0, 0.0);
      
      public PanelDescription(MarkSb markSB, Database db)
      {
         this.markSB = markSB;
         this.db = db;
      }

      /// <summary>
      /// Создание описания панели на листе шаблона 
      /// Должна быть запущена транзакция
      /// </summary>
      public void CreateDescription(Transaction t)
      {         
         // Лист           
         var layoutId = LayoutManager.Current.GetLayoutId(Settings.Default.SheetTemplateLayoutNameForMarkAR);
         if (layoutId.IsNull)
         {
            error("PanelDescription.CreateDescription() - layoutId.IsNull.");
            return;
         }
         var layout = layoutId.GetObject(OpenMode.ForRead) as Layout;
         using (var btrLayout = layout?.BlockTableRecordId.GetObject(OpenMode.ForWrite) as BlockTableRecord)
         {
            if (btrLayout == null)
            {
               error("PanelDescription.CreateDescription() - btrLayout == null");               
               return;
            }
            // Добавление мтекста
            MText mtext = getMtext();
            if (mtext != null)
            {
               btrLayout.AppendEntity(mtext);
               t.AddNewlyCreatedDBObject(mtext, true);
            }
            // Таблица профилей для панелей с торцевыми плитками
            Table tableProfile = getTableProfile(mtext);
            if (tableProfile !=null)
            {
               btrLayout.AppendEntity(tableProfile);
               t.AddNewlyCreatedDBObject(tableProfile, true);
            }            
         }
      }
            
      private MText getMtext()
      {
         MText mtext = new MText();
         mtext.Attachment = AttachmentPoint.MiddleCenter;
         mtext.Location = ptMtextCenter;
         mtext.TextStyleId = db.GetTextStylePIK();
         mtext.Height = 2.5;

         int tileCount = markSB.Tiles.Count;
         double tileTotalArea = Math.Round(TileCalc.OneTileArea * tileCount, 2);         

         string text = $"Швы вертикальные - {Settings.Default.TileSeam} мм, \n" +
                       $"швы горизонтальные {Settings.Default.TileSeam} мм, \n\n" +
                       $"Расход плитки: {Settings.Default.TileLenght}x{Settings.Default.TileHeight}x{Settings.Default.TileThickness} - \n" +
                       $"{tileCount} шт ({tileTotalArea} м\\U+00B2)";
         if (markSB.Windows.Count()>0)
         {
            text += "\n\nОконные блоки в панели:";
            var wins = markSB.Windows.GroupBy(w => w.Mark).OrderBy(w => w.Key);
            foreach (var win in wins)
            {
               text += $"\n{win.Key} - {win.Count()}шт";
            }
         }

         mtext.Contents = text;
         return mtext;
      }

      private void error(string logErr)
      {
         Log.Error("PanelDescription.CreateDescription() - layoutId.IsNull.");
         Inspector.AddError($"Не удалось создать описание панели на листах панелей {markSB.MarkSbBlockName}");
      }

      private Table getTableProfile(MText mtext)
      {
         double lenProfile = 0;
         if (markSB.IsEndLeftPanel || markSB.IsEndRightPanel)
         {
            lenProfile = Math.Round(markSB.HeightPanelByTile*0.001, 1);
         }
         else if (markSB.MarkSbName.StartsWith("ОЛ", StringComparison.OrdinalIgnoreCase))
         {
            // Длина панели ???
            lenProfile = Math.Round(( markSB.ExtentsTiles.MaxPoint.X - markSB.ExtentsTiles.MinPoint.X) * 0.001, 1)*2;
         }
         else
         {
            return null;
         }

         double xTable = ptMtextCenter.X - 35;
         Point3d ptTable;
         if (mtext != null)
         {
            // Определение границ тексчта
            try
            {
               var extMText = mtext.GeometricExtents;
               ptTable = new Point3d(xTable, extMText.MinPoint.Y-15, 0.0);
            }
            catch
            {
               ptTable = new Point3d(xTable, 22.0, 0.0);
            }        
         }
         else
         {
            ptTable = ptMtextCenter;
         }

         Table table = new Table();
         table.Position = ptTable;
         table.TableStyle = db.GetTableStylePIK();

         table.SetSize(3, 3);

         table.DeleteRows(0, 1);

         table.Rows[0].Height = 8;

         table.Columns[0].Width = 30;
         table.Columns[1].Width = 20;
         table.Columns[2].Width = 20;

         table.Columns[0].Alignment = CellAlignment.MiddleCenter;
         table.Columns[1].Alignment = CellAlignment.MiddleCenter;
         table.Columns[2].Alignment = CellAlignment.MiddleCenter;

         table.Cells[0, 0].TextString = "Наименование";
         table.Cells[0, 1].TextString = "Цвет";
         table.Cells[0, 2].TextString = "Кол-во, м.п.";

         table.Cells[1, 0].TextString = "АА-1347";         
         table.Cells[1, 1].TextString = "RAL-7044";
         table.Cells[1, 2].TextString = lenProfile.ToString();

         return table;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Sheets
{
   /// <summary>
   /// Класс для формирования текста описания панели - для листов
   /// </summary>
   public class PanelDescription
   {
      private Database db;
      private MarkSb markSB;      

      public PanelDescription(MarkSb markSB, Database db)
      {
         this.markSB = markSB;
         this.db = db;
      }

      /// <summary>
      /// Создание описания панели на листе шаблоне 
      /// Должна быть запущена транзакция
      /// </summary>
      public void CreateDescription()
      {
         // Данные по панели

         // Лист  
         var layoutId = LayoutManager.Current.GetLayoutId(Settings.Default.SheetTemplateLayoutNameForMarkAR);
         var layout = layoutId.GetObject(OpenMode.ForRead) as Layout;
         using (var btrLayout = layout.BlockTableRecordId.GetObject(OpenMode.ForWrite) as BlockTableRecord)
         {

         }
      }
   }
}

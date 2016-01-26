using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      /// </summary>
      public void CreateDescription()
      {
         // Лист         
      }
   }
}

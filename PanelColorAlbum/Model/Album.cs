using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Альбом колористических решений.
   public class Album
   {
      // Набор цветов используемых в альбоме.
      private static List<Paint> _colors;
      private static Options _options;
      private ColorAreaModel _colorAreaModel;
      private Database _db;
      private Document _doc;
      private List<MarkSbPanel> _marksSb;

      public Album()
      {
         _options = new Options();
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
      }

      public static Options Options { get { return _options; } }
      // Поиск цвета в списке цветов альбома
      public static Paint FindPaint(string layerName)
      {
         Paint paint = _colors.Find(c => c.LayerName == layerName);
         if (paint == null)
         {
            paint = new Paint(layerName);
            _colors.Add(paint);
         }
         return paint;
      }

      // Покраска панелей в модели (по блокам зон покраски)
      public void PaintPanels()
      {
         // В Модели должны быть расставлены панели Марки СБ и зоны покраски.
         // сброс списка цветов.
         _colors = new List<Paint>();

         // Определение зон покраски в Модели
         _colorAreaModel = new ColorAreaModel(SymbolUtilityServices.GetBlockModelSpaceId(_db));

         // Определение покраски панелей.
         _marksSb = MarkSbPanel.GetMarksSB(_colorAreaModel);

         // Создание определений блоков панелей покраски МаркиАР       
         CreatePanelsMarkAR();

         // Замена вхождений блоков панелей Марки СБ на блоки панелей Марки АР.
         ReplaceBlocksMarkSbOnMarkAr();
      }

      // Сброс блоков панелей в чертеже. Замена панелей марки АР на панели марки СБ
      public void Resetblocks()
      {
         // Для покраски панелей, нужно, чтобы в чертеже были расставлены блоки панелей Марки СБ.
         // Поэтому, при изменении зон покраски, перед повторным запуском команды покраски панелей и создания альбома,
         // нужно восстановить блоки Марки СБ (вместо Марок АР).
      }
      // Создание определений блоков панелей марки АР
      private void CreatePanelsMarkAR()
      {
         foreach (var markSB in _marksSb)
         {
            foreach (var markAR in markSB.MarksAR)
            {
               markAR.CreateBlock(markSB);
            }
         }
      }

      // Замена вхождений блоков панелей Марки СБ на панели Марки АР
      private void ReplaceBlocksMarkSbOnMarkAr()
      {
         foreach (var markSb in _marksSb)
         {
            markSb.ReplaceBlocksSbOnAr();
         }
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

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

         // Сброс блоков панелей Марки АР на панели марки СБ.
         if (!Resetblocks())
         {
            // Ошибки при сбросе блоков. В ком строке описаны причины. Нужно исправить чертеж.
            return;
         }

         // Проверка чертежа
         Inspector inspector = new Inspector();
         if (!inspector.CheckDrawing())
         {
            Editor ed = _doc.Editor;
            ed.WriteMessage("\nПокраска панелей не выполнена, т.к. в чертежа найдены ошибки в блоках панелей, см. выше."); 
            return;
         }

         // Определение покраски панелей.
         _marksSb = MarkSbPanel.GetMarksSB(_colorAreaModel);

         // Создание определений блоков панелей покраски МаркиАР       
         CreatePanelsMarkAR();

         // Замена вхождений блоков панелей Марки СБ на блоки панелей Марки АР.
         ReplaceBlocksMarkSbOnMarkAr();
      }

      // Сброс блоков панелей в чертеже. Замена панелей марки АР на панели марки СБ
      public bool Resetblocks()
      {
         // Для покраски панелей, нужно, чтобы в чертеже были расставлены блоки панелей Марки СБ.
         // Поэтому, при изменении зон покраски, перед повторным запуском команды покраски панелей и создания альбома,
         // нужно восстановить блоки Марки СБ (вместо Марок АР).
         // Блоки панелей Марки АР - удалить.

         bool res = true;

         using (var t = _db.TransactionManager.StartTransaction() )
         {            
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRef = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;
                  if (MarkSbPanel.IsBlockNamePanel(blRef.Name))
                  {                     
                     // Если это панель марки АР, то заменяем на панель марки СБ.                     
                     if (MarkSbPanel.IsBlockNamePanelMarkAr (blRef.Name) )
                     {
                        string markSb = MarkSbPanel.GetMarkSbName(blRef.Name);
                        string markSbBlName = MarkSbPanel.GetMarkSbBlockName(markSb);
                        if (!bt.Has(markSbBlName))
                        {
                           // Нет определения блока марки СБ.
                           // Такое возможно, если после покраски панелей, сделать очистку чертежа (блоки марки СБ удалятся).
                           //TODO: Создать блоки марки СБ из Марки АР. Но, зоны покраски внутри бков АР удалены.
                           MarkSbPanel.CreateBlockMarkSbFromAr(blRef.BlockTableRecord, markSbBlName);
                           Editor ed = _doc.Editor;
                           ed.WriteMessage("\nНет определения блока для панели Марки СБ " + markSbBlName +
                                          ". Оно создано из панели Марки АР " + blRef.Name + ". Зоны покраски внутри блока не определены." +
                                          "Необходимо проверить блоки и заново запустить программу.");
                           // Надо чтобы проектировщик проверил эти блоки, может в них нужно добавить зоны покраски (т.к. в блоках марки АР их нет).
                           res = false;
                        }
                        var blRefMarkSb = new BlockReference(blRef.Position, bt[markSbBlName]);
                        blRefMarkSb.SetDatabaseDefaults();
                        blRefMarkSb.Layer = "0";
                        ms.UpgradeOpen();
                        ms.AppendEntity(blRefMarkSb);
                        t.AddNewlyCreatedDBObject(blRefMarkSb, true);
                     }
                  }
               }
            }
            // Удаление определений блоков Марок АР.
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (MarkSbPanel.IsBlockNamePanel(btr.Name))
               {                  
                  // Если это блок панели Марки АР
                  if (MarkSbPanel.IsBlockNamePanelMarkAr (btr.Name))
                  {
                     // Блок Марки АР.
                     var idsBlRef = btr.GetBlockReferenceIds(false, true);
                     foreach (ObjectId idBlRef in idsBlRef)
                     {
                        var blRef = t.GetObject(idBlRef, OpenMode.ForWrite, false, true) as BlockReference;
                        blRef.Erase(true);
                     }
                     // Удаление определение блока Марки АР
                     btr.UpgradeOpen();
                     btr.Erase(true);
                  }
               }
            }
            t.Commit();
         }
         return res;
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

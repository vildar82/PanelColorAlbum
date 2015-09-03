using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

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
      private List<MarkSbPanel> _marksSB;
      private ObjectId _idLayerMarks = ObjectId.Null;

      public Album()
      {
         _options = new Options();
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
      }

      public static Options Options { get { return _options; } }

      public List<MarkSbPanel> MarksSB { get { return _marksSB; } }

      public Document Doc { get { return _doc; } }

      // Поиск цвета в списке цветов альбома
      public static Paint FindPaint(string layerName)
      {
         Paint paint = _colors.Find(c => c.LayerName == layerName);
         if (paint == null)
         {
            // Определение цвета слоя
            Database db = HostApplicationServices.WorkingDatabase;
            Color color = null;
            using (var lt = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable)
            {
               using (var ltr = lt[layerName].GetObject(OpenMode.ForRead) as LayerTableRecord)
               {
                  color = ltr.Color;
               }
            }
            paint = new Paint(layerName, color);
            _colors.Add(paint);
         }
         return paint;
      }

      // Покраска панелей в модели (по блокам зон покраски)
      public bool PaintPanels()
      {
         bool res = true;
         // В Модели должны быть расставлены панели Марки СБ и зоны покраски.
         // сброс списка цветов.
         _colors = new List<Paint>();

         // Определение зон покраски в Модели
         _colorAreaModel = new ColorAreaModel(SymbolUtilityServices.GetBlockModelSpaceId(_db));

         // Сброс блоков панелей Марки АР на панели марки СБ.
         if (!Resetblocks())
         {
            // Ошибки при сбросе блоков. В ком строке описаны причины. Нужно исправить чертеж.
            return false;
         }

         // Проверка чертежа
         Inspector inspector = new Inspector();
         if (!inspector.CheckDrawing())
         {
            _doc.Editor.WriteMessage("\nПокраска панелей не выполнена, т.к. в чертежа найдены ошибки в блоках панелей, см. выше.");
            return false;
         }

         // Определение покраски панелей.
         _marksSB = MarkSbPanel.GetMarksSB(_colorAreaModel);

         // Проверить всели плитки покрашены. Если есть непокрашенные плитки, то выдать сообщение об ошибке.
         if (!inspector.CheckAllTileArePainted(_marksSB))
         {
            _doc.Editor.WriteMessage("\nПокраска не выполнена, т.е. не все плитки покрашены. См. подробности выше.");
            return false;
         }

         // Создание определений блоков панелей покраски МаркиАР
         res = CreatePanelsMarkAR();

         // Замена вхождений блоков панелей Марки СБ на блоки панелей Марки АР.
         ReplaceBlocksMarkSbOnMarkAr();

         // Добавление подписей к панелям
         CaptionPanels();

         return res;
      }

      // Создание альбома панелей
      public bool CreateAlbum()
      {
         bool res = true;
         // Создание папки с файлами марок СБ, в которых создать листы панелей Марок АР.
         if (_marksSB.Count == 0)
         {
            res = false;
            _doc.Editor.WriteMessage("\nНе определены панели марок СБ.");
         }
         else
         {
            Sheets sheets = new Sheets(this);
            res = sheets.CreateAlbum();
            _doc.Editor.WriteMessage("\nСоздана папка альбома панелей: " + sheets.AlbumDir);
         }
         return res;
      }

      // Сброс блоков панелей в чертеже. Замена панелей марки АР на панели марки СБ
      public bool Resetblocks()
      {
         // Для покраски панелей, нужно, чтобы в чертеже были расставлены блоки панелей Марки СБ.
         // Поэтому, при изменении зон покраски, перед повторным запуском команды покраски панелей и создания альбома,
         // нужно восстановить блоки Марки СБ (вместо Марок АР).
         // Блоки панелей Марки АР - удалить.

         bool res = true;

         using (var t = _db.TransactionManager.StartTransaction())
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
                     if (MarkSbPanel.IsBlockNamePanelMarkAr(blRef.Name))
                     {
                        string markSb = MarkSbPanel.GetMarkSbName(blRef.Name);
                        string markSbBlName = MarkSbPanel.GetMarkSbBlockName(markSb);
                        if (!bt.Has(markSbBlName))
                        {
                           // Нет определения блока марки СБ.
                           // Такое возможно, если после покраски панелей, сделать очистку чертежа (блоки марки СБ удалятся).
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
                  if (MarkSbPanel.IsBlockNamePanelMarkAr(btr.Name))
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
      private bool CreatePanelsMarkAR()
      {
         bool res = true;
         foreach (var markSB in _marksSB)
         {
            foreach (var markAR in markSB.MarksAR)
            {
               res = markAR.CreateBlock(markSB);
            }
         }
         return res;
      }

      // Замена вхождений блоков панелей Марки СБ на панели Марки АР
      private void ReplaceBlocksMarkSbOnMarkAr()
      {
         foreach (var markSb in _marksSB)
         {
            markSb.ReplaceBlocksSbOnAr();
         }
      }

      // Добавление подписи имени марки панели в блоки панелей в чертеже
      public void CaptionPanels()
      {
         // Подпись в виде текста на слое АР_Марки
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
            bool hasMark = false;
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;               
               if (MarkSbPanel.IsBlockNamePanel(btr.Name))
               {
                  string panelMark = MarkSbPanel.GetPanelMarkFromBlockName(btr.Name);
                  foreach (ObjectId idEnt in btr)
                  {
                     if (idEnt.ObjectClass.Name == "AcDbText")
                     {
                        var text = t.GetObject(idEnt, OpenMode.ForRead) as DBText;
                        if (text.Layer == Album.Options.LayerForMarks)
                        {
                           if (text.TextString != panelMark)
                           {
                              text.UpgradeOpen();
                              text.TextString = panelMark;
                              //Марка найдена
                              hasMark = true;
                              break;
                           } 
                        }
                     }
                  }
                  // Если марки нет, то создаем ее.
                  if (!hasMark)
                  {
                     DBText text = new DBText();
                     text.TextString = panelMark;
                     text.Height = 250;
                     text.Annotative = AnnotativeStates.False;
                     text.Layer = GetLayerForMark();
                     text.Position = Point3d.Origin;
                     // Точка вставки и выравнивание ???
                     btr.UpgradeOpen();
                     btr.AppendEntity(text);
                     t.AddNewlyCreatedDBObject(text, true);
                  }
               }
            }
            t.Commit();
         }
      }

      // Получение слоя для марок (АР_Марки)
      private string GetLayerForMark()
      {         
         // Если уже был создан слой, то возвращаем его. Опасно, т.к. перед повторным запуском команды покраски, могут удалить/переименовать слой марок.
         if (_idLayerMarks == ObjectId.Null)
         {            
            using (var t = _db.TransactionManager.StartTransaction() )
            {
               var lt = t.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
               if (lt.Has(Album.Options.LayerForMarks))
               {
                  _idLayerMarks = lt[Album.Options.LayerForMarks];
               }
               else
               {
                  // Если слоя нет, то он создается.
                  var ltrMarks = new LayerTableRecord();
                  ltrMarks.Name = Album.Options.LayerForMarks;
                  ltrMarks.IsPlottable = false;
                  lt.UpgradeOpen();
                  _idLayerMarks = lt.Add(ltrMarks);
                  t.AddNewlyCreatedDBObject(ltrMarks, true);                  
               }
               t.Commit();
            }
         }
         return Album.Options.LayerForMarks;
      }
   }
}
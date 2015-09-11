using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Checks;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Lib;
using Vil.Acad.AR.AlbumPanelColorTiles.Model.Sheets;

namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   // Альбом колористических решений.
   public class Album
   {
      // Набор цветов используемых в альбоме.
      private static List<Paint> _colors;

      private static Options _options;

      // Сокращенное имя проеккта
      private string _abbreviateProject;

      private ColorAreaModel _colorAreaModel;
      private Database _db;
      private Document _doc;
      //private ObjectId _idLayerMarks = ObjectId.Null;
      private List<MarkSbPanel> _marksSB;
      private SheetsSet _sheetsSet;      

      public Album()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         if (!File.Exists(_doc.Name))
            throw new System.Exception("Нужно сохранить файл.");
         _db = _doc.Database;
         // Запрос сокращенного имени проекта для добавления к индексу маркок АР
         _abbreviateProject = AbbreviateNameProject();         
      }

      public static Options Options
      {
         get
         {
            if (_options == null)
               _options = new Options();
            return _options;
         }
      }

      public string AbbreviateProject { get { return _abbreviateProject; } }

      public string DwgFacade { get { return _doc.Name; } }

      public List<MarkSbPanel> MarksSB { get { return _marksSB; } }

      public SheetsSet SheetsSet { get { return _sheetsSet; } }

      public static Tolerance Tolerance
      {
         get
         {            
            return Tolerance.Global;
         }
      }      

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

      // Сброс блоков панелей в чертеже. Замена панелей марки АР на панели марки СБ
      public static void ResetBlocks()
      {
         // Для покраски панелей, нужно, чтобы в чертеже были расставлены блоки панелей Марки СБ.
         // Поэтому, при изменении зон покраски, перед повторным запуском команды покраски панелей и создания альбома,
         // нужно восстановить блоки Марки СБ (вместо Марок АР).
         // Блоки панелей Марки АР - удалить.         

         Document doc = Application.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;
         Editor ed = doc.Editor;         
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
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
                        string markSb = MarkSbPanel.GetMarkSbName(blRef.Name);// может быть с суффиксом торца _тп или _тл
                        string markSbBlName = MarkSbPanel.GetMarkSbBlockName(markSb);// может быть с суффиксом торца _тп или _тл
                        if (!bt.Has(markSbBlName))
                        {
                           // Нет определения блока марки СБ.
                           // Такое возможно, если после покраски панелей, сделать очистку чертежа (блоки марки СБ удалятся).
                           MarkSbPanel.CreateBlockMarkSbFromAr(blRef.BlockTableRecord, markSbBlName);
                           string errMsg = "\nНет определения блока для панели Марки СБ " + markSbBlName +
                                          ". Оно создано из панели Марки АР " + blRef.Name + ". Зоны покраски внутри блока не определены." +
                                          "Необходимо проверить блоки и заново запустить программу.";
                           ed.WriteMessage ("\n" + errMsg);
                           // Надо чтобы проектировщик проверил эти блоки, может в них нужно добавить зоны покраски (т.к. в блоках марки АР их нет).
                        }
                        var blRefMarkSb = new BlockReference(blRef.Position, bt[markSbBlName]);
                        blRefMarkSb.SetDatabaseDefaults();
                        blRefMarkSb.Layer = blRef.Layer;
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
                  else
                  {
                     // Подпись марки блока
                     string panelMark = btr.Name.Substring(Album.Options.BlockPanelPrefixName.Length);
                     AddMarkToPanelBtr(panelMark, t, btr);
                  }                  
               }
            }
            t.Commit();            
         }
      }

      // Добавление подписи имени марки панели в блоки панелей в чертеже
      public void CaptionPanels()
      {         
         // Подпись в виде текста на слое АР_Марки
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;            
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (MarkSbPanel.IsBlockNamePanel(btr.Name))
               {
                  string panelMark = MarkSbPanel.GetPanelMarkFromBlockName(btr.Name, _marksSB);
                  AddMarkToPanelBtr(panelMark, t, btr);
               }
            }
            t.Commit();
         }
      }

      public static void AddMarkToPanelBtr(string panelMark,Transaction t, BlockTableRecord btr)
      {
         // Найти панель марки СБ или АР по имени блока         
         foreach (ObjectId idEnt in btr)
         {
            if (idEnt.ObjectClass.Name == "AcDbText")
            {
               var textMark = t.GetObject(idEnt, OpenMode.ForRead, false) as DBText;
               if (textMark.Layer == Album.Options.LayerMarks)
               {
                  textMark.UpgradeOpen();
                  textMark.Erase(true);          
               }
            }
         }
         // Если марки нет, то создаем ее.         
         var text = new DBText();
         text.TextString = panelMark;
         text.Height = 200;
         text.Annotative = AnnotativeStates.False;
         text.Layer = GetLayerForMark();
         text.Position = Point3d.Origin;
         // Точка вставки и выравнивание ???
         btr.UpgradeOpen();
         btr.AppendEntity(text);         
         t.AddNewlyCreatedDBObject(text, true);
      }

      // Проверка панелей на чертеже и панелей в памяти (this)
      public void CheckPanelsInDrawingAndMemory()
      {
         // Проверка зон покраски
         var colorAreaModelCheck = new ColorAreaModel(SymbolUtilityServices.GetBlockModelSpaceId(_db));
         // сравнение фоновых зон
         if (!colorAreaModelCheck.ColorAreasBackground.SequenceEqual(_colorAreaModel.ColorAreasBackground))
         {
            throw new System.Exception("Фоновые зоны изменились. Рекомендуется выполнить повторную покраску панелей командой PaintPanels.");
         }
         // сравнение передних зон (пятен)
         if (!colorAreaModelCheck.ColorAreasForeground.SequenceEqual(_colorAreaModel.ColorAreasForeground))
         {
            throw new System.Exception("Пятна изменились (зоны покраски). Рекомендуется выполнить повторную покраску панелей командой PaintPanels.");
         }

         // Проверка панелей
         // Определение покраски панелей.
         var marksSbCheck = MarkSbPanel.GetMarksSB(_colorAreaModel);
         RenamePanelsToArchitectIndex(marksSbCheck, _abbreviateProject);
         if (!marksSbCheck.SequenceEqual(_marksSB))
         {
            throw new System.Exception("Панели изменились после последнего выполнения команды покраски. Рекомендуется выполнить повторную покраску панелей командой PaintPanels.");
         }
      }
      public void ChecksBeforeCreateAlbum()
      {
         if (_marksSB == null)
         {
            throw new System.Exception("Не определены панели марок АР.");
         }
         // Проверка есть ли панелеи марки АР
         bool hasMarkAR = false;
         foreach (var markSb in _marksSB)
         {
            if (markSb.MarksAR.Count > 0)
            {
               hasMarkAR = true;
               break;
            }
         }
         if (!hasMarkAR)
         {
            throw new System.Exception("Не определены панели марок АР.");
         }
      }

      // Создание альбома панелей
      public void CreateAlbum()
      {
         _sheetsSet = new SheetsSet(this);
         _sheetsSet.CreateAlbum();
      }

      // Покраска панелей в модели (по блокам зон покраски)
      public void PaintPanels()
      {
         // Определение марок покраски панелей (Марок АР).
         // Создание определениц блоков марок АР.
         // Покраска панелей в чертеже.

         // В Модели должны быть расставлены панели Марки СБ и зоны покраски.
         // сброс списка цветов.
         _colors = new List<Paint>();
         
         // Определение зон покраски в Модели
         _colorAreaModel = new ColorAreaModel(SymbolUtilityServices.GetBlockModelSpaceId(_db));
         
         // Сброс блоков панелей Марки АР на панели марки СБ.
         ResetBlocks();
         
         // Проверка чертежа
         Inspector inspector = new Inspector();
         if (!inspector.CheckDrawing())
         {
            throw new System.Exception("\nПокраска панелей не выполнена, т.к. в чертежа найдены ошибки в блоках панелей, см. выше.");
         }
         
         // Определение покраски панелей.
         _marksSB = MarkSbPanel.GetMarksSB(_colorAreaModel);
         
         // Проверить всели плитки покрашены. Если есть непокрашенные плитки, то выдать сообщение об ошибке.
         if (!inspector.CheckAllTileArePainted(_marksSB))
         {
            throw new System.Exception("\nПокраска не выполнена, т.е. не все плитки покрашены. См. подробности выше.");
         }
         
         // Переименование марок АР панелей в соответствии с индексами архитекторов (Э2_Яр1)
         RenamePanelsToArchitectIndex(_marksSB, _abbreviateProject);
         
         // Создание определений блоков панелей покраски МаркиАР
         CreatePanelsMarkAR();
         
         // Замена вхождений блоков панелей Марки СБ на блоки панелей Марки АР.
         ReplaceBlocksMarkSbOnMarkAr();
         
         // Добавление подписей к панелям
         CaptionPanels();         
      }

      // Сброс данных расчета панелей
      public void ResetData()
      {
         // Набор цветов используемых в альбоме.
         _colors = null;
         _colorAreaModel = null;
         ObjectId _idLayerMarks = ObjectId.Null;
         _marksSB = null;
         _sheetsSet = null;
      }
      private string AbbreviateNameProject()
      {
         string abbrName;
         string defName = "Н47Г";
         var opt = new PromptStringOptions("Введите сокращенное имя проекта для добавления к имени Марки АР:");
         opt.DefaultValue = defName;
         var res = _doc.Editor.GetString(opt);
         if (res.Status == PromptStatus.OK)
            abbrName = res.StringResult;
         else
            abbrName = defName;
         return abbrName;
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

      // Получение слоя для марок (АР_Марки)
      private static string GetLayerForMark()
      {
         Database db = HostApplicationServices.WorkingDatabase; 
         // Если уже был создан слой, то возвращаем его. Опасно, т.к. перед повторным запуском команды покраски, могут удалить/переименовать слой марок.         
         using (var t = db.TransactionManager.StartTransaction())
         {
            var lt = t.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            if (!lt.Has(Album.Options.LayerMarks))
            {
               // Если слоя нет, то он создается.
               var ltrMarks = new LayerTableRecord();
               ltrMarks.Name = Album.Options.LayerMarks;
               ltrMarks.IsPlottable = false;
               lt.UpgradeOpen();
               lt.Add(ltrMarks);
               t.AddNewlyCreatedDBObject(ltrMarks, true);
            }
            t.Commit();
         }
         return Album.Options.LayerMarks;
      }

      // определение этажей панелей
      private void IdentificationStoreys(List<MarkSbPanel> marksSB)
      {
         // Определение этажей панелей (точек вставки панелей по Y.) для всех панелей в чертеже, кроме панелей чердака.
         var comparerStorey = new DoubleEqualityComparer(2000);
         HashSet<double> panelsStorey = new HashSet<double>(comparerStorey);
         // Этажи
         List<Storey> storeys = new List<Storey>();
         foreach (var markSb in marksSB)
         {
            if (!markSb.IsUpperStoreyPanel)
            {
               foreach (var markAr in markSb.MarksAR)
               {
                  foreach (var panel in markAr.Panels)
                  {
                     Storey storey = null;
                     if (panelsStorey.Add(panel.InsPt.Y))
                     {
                        // Новый этаж
                        storey = new Storey(panel.InsPt.Y);
                        storeys.Add(storey);
                     }
                     else
                     {
                        storey = storeys.Single(s => comparerStorey.Equals(s.Y, panel.InsPt.Y));
                     }
                     panel.Storey = storey;
                  }
               }
            }
         }
         // Нумерация этажей
         int i = 2;
         var storeysOrders = storeys.OrderBy(s => s.Y).ToList();
         storeysOrders.ForEach((s) => s.Number = i++.ToString());
         storeysOrders.Last().Number = "П";
         // В итоге у всех панелей (Panel) проставлены этажи (Storey).
      }

      // Переименование марок АР панелей в соответствии с индексами архитекторов (Э2_Яр1)
      private void RenamePanelsToArchitectIndex(List<MarkSbPanel> marksSB, string abbreviateProject)
      {
         // Определение этажа панели.
         IdentificationStoreys(marksSB);         

         // Маркировка Марок АР по архитектурному индексу
         foreach (var markSB in marksSB)
         {
            markSB.DefineArchitectMarks(marksSB, abbreviateProject);
         }
      }

      // Замена вхождений блоков панелей Марки СБ на панели Марки АР
      private void ReplaceBlocksMarkSbOnMarkAr()
      {
         foreach (var markSb in _marksSB)
         {
            markSb.ReplaceBlocksSbOnAr();
         }
      }
   }
}
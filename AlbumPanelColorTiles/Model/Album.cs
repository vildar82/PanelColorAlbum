﻿using System.Collections.Generic;
using System.Linq;
using AcadLib.Comparers;
using AcadLib.Errors;
using AlbumPanelColorTiles.Checks;
using AlbumPanelColorTiles.Lib;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.Properties;
using AlbumPanelColorTiles.Sheets;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RTreeLib;

namespace AlbumPanelColorTiles
{
   // Альбом колористических решений.
   public class Album
   {
      private const string _regAppPath = @"Software\Vildar\AKR";      
      private List<Paint> _colors; // Набор цветов используемых в альбоме.
      private string _abbreviateProject;// Сокращенное имя проеккта
      private int _numberFirstFloor;
      private string _albumDir;      
      private List<ColorArea> _colorAreas;      
      private Database _db;
      private Document _doc;
      private List<MarkSbPanelAR> _marksSB;      
      private SheetsSet _sheetsSet;
      private List<Storey> _storeys;

      public Album()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _db = _doc.Database;
      }      

      public static string RegAppPath { get { return _regAppPath; } }
      public static Tolerance Tolerance { get { return Tolerance.Global; } }
      public string AbbreviateProject { get { return _abbreviateProject; } }
      public string AlbumDir { get { return _albumDir; } set { _albumDir = value; } }
      public List<Paint> Colors { get { return _colors; } }
      public string DwgFacade { get { return _doc.Name; } }
      public List<MarkSbPanelAR> MarksSB { get { return _marksSB; } }
      public SheetsSet SheetsSet { get { return _sheetsSet; } }
      public List<Storey> Storeys { get { return _storeys; } }      

      // Поиск цвета в списке цветов альбома
      public Paint GetPaint(string layerName)
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
                  if (MarkSbPanelAR.IsBlockNamePanel(blRef.Name))
                  {
                     // Если это панель марки АР, то заменяем на панель марки СБ.
                     if (MarkSbPanelAR.IsBlockNamePanelMarkAr(blRef.Name))
                     {
                        string markSb = MarkSbPanelAR.GetMarkSbName(blRef.Name);// может быть с суффиксом торца _тп или _тл
                        string markSbBlName = MarkSbPanelAR.GetMarkSbBlockName(markSb);// может быть с суффиксом торца _тп или _тл
                        if (!bt.Has(markSbBlName))
                        {
                           // Нет определения блока марки СБ.
                           // Такое возможно, если после покраски панелей, сделать очистку чертежа (блоки марки СБ удалятся).
                           MarkSbPanelAR.CreateBlockMarkSbFromAr(blRef.BlockTableRecord, markSbBlName);
                           string errMsg = "\nНет определения блока для панели Марки СБ " + markSbBlName +
                                          ". Оно создано из панели Марки АР " + blRef.Name + 
                                          ". Если внутри блока Марки СБ были зоны покраски, то в блоке Марки АР они были удалены." +
                                          "Необходимо проверить блоки и заново запустить программу.";
                           ed.WriteMessage("\n" + errMsg);
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
               if (MarkSbPanelAR.IsBlockNamePanel(btr.Name))
               {
                  // Если это блок панели Марки АР
                  if (MarkSbPanelAR.IsBlockNamePanelMarkAr(btr.Name))
                  {
                     // Удаление всех вхожденний бллока
                     var idsBlRef = btr.GetBlockReferenceIds(true, true);
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
                     string panelMark = btr.Name.Substring(Settings.Default.BlockPanelPrefixName.Length);
                     PanelAR.AddMarkToPanelBtr(panelMark, t, btr);
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
               if (HostApplicationServices.Current.UserBreak())
                  throw new System.Exception("Отменено пользователем.");

               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (MarkSbPanelAR.IsBlockNamePanel(btr.Name))
               {
                  string panelMark = MarkSbPanelAR.GetPanelMarkFromBlockName(btr.Name, _marksSB);
                  PanelAR.AddMarkToPanelBtr(panelMark, t, btr);
               }
            }
            t.Commit();
         }
      }

      // Проверка панелей на чертеже и панелей в памяти (this)
      public void CheckPanelsInDrawingAndMemory()
      {
         // Проверка зон покраски
         var colorAreasCheck = ColorArea.GetColorAreas(SymbolUtilityServices.GetBlockModelSpaceId(_db), this);
         // сравнение фоновых зон
         if (!colorAreasCheck.SequenceEqual(_colorAreas))
         {
            throw new System.Exception("Изменились зоны покраски. Рекомендуется выполнить повторную покраску панелей командой PaintPanels.");
         }

         // Проверка панелей
         // Определение покраски панелей.
         var rtreeColorAreas = ColorArea.GetRTree(colorAreasCheck);
         var marksSbCheck = MarkSbPanelAR.GetMarksSB(rtreeColorAreas, this, "Проверка панелей...");
         //RenamePanelsToArchitectIndex(marksSbCheck);
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
         // Запрос сокращенного имени проекта для добавления к индексу маркок АР
         _abbreviateProject = abbreviateNameProject();

         // Запрос номера первого этажа панелей
         _numberFirstFloor = promptNumberFirtsFloor();

         // Определение марок покраски панелей (Марок АР).
         // Создание определениц блоков марок АР.
         // Покраска панелей в чертеже.

         // В Модели должны быть расставлены панели Марки СБ и зоны покраски.
         // сброс списка цветов.
         _colors = new List<Paint>();

         // Определение зон покраски в Модели
         _colorAreas = ColorArea.GetColorAreas(SymbolUtilityServices.GetBlockModelSpaceId(_db), this);
         RTree<ColorArea> rtreeColorAreas = ColorArea.GetRTree(_colorAreas);

         // Бонус - покраска блоков плитки разложенных просто в Модели
         Tile.PaintTileInModel(rtreeColorAreas);

         // Сброс блоков панелей Марки АР на панели марки СБ.
         ResetBlocks();

         // Проверка чертежа
         Inspector.Clear();
         CheckDrawing checkDrawing = new CheckDrawing();
         checkDrawing.Check();                  
         if (Inspector.HasErrors)
         {
            throw new System.Exception("\nПокраска панелей не выполнена, в чертеже найдены ошибки в блоках панелей, см. выше.");
         }

         // Определение покраски панелей.
         _marksSB = MarkSbPanelAR.GetMarksSB(rtreeColorAreas, this, "Покраска панелей...");
         if (_marksSB?.Count == 0)
         {
            throw new System.Exception("Не найдены блоки панелей в чертеже. Выполните команду AKR-Help для просмотра справки к программе.");
         }

         // Проверить всели плитки покрашены. Если есть непокрашенные плитки, то выдать сообщение об ошибке.
         if (Inspector.HasErrors)
         {
            throw new System.Exception("\nПокраска не выполнена, не все плитки покрашены. См. список непокрашенных плиток в форме ошибок.");
         }

         // Переименование марок АР панелей в соответствии с индексами архитекторов (Э2_Яр1)
         RenamePanelsToArchitectIndex(_marksSB);

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
         Inspector.Clear();
         _colors = null;
         _colorAreas = null;
         ObjectId _idLayerMarks = ObjectId.Null;
         _marksSB = null;
         _sheetsSet = null;
      }      

      private string abbreviateNameProject()
      {
         string abbrName;
         string defName;
         if (string.IsNullOrEmpty(_abbreviateProject))
         {
            defName = loadAbbreviateName();// "Н47Г";
         }
         else
         {
            defName = _abbreviateProject;
         }
         var opt = new PromptStringOptions("\nВведите сокращенное имя проекта:");
         opt.DefaultValue = defName;
         var res = _doc.Editor.GetString(opt);
         if (res.Status == PromptStatus.OK)
         {
            abbrName = res.StringResult;
            saveAbbreviateName(abbrName);
         }
         else
         {
            throw new System.Exception("Прервано пользователем.");
         }
         return abbrName;
      }

      // Создание определений блоков панелей марки АР
      private void CreatePanelsMarkAR()
      {
         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.Start("Создание определений блоков панелей марки АР ");
         progressMeter.SetLimit(_marksSB.Count);
         progressMeter.Start();

         foreach (var markSB in _marksSB)
         {
            progressMeter.MeterProgress();
            if (HostApplicationServices.Current.UserBreak())
               throw new System.Exception("Отменено пользователем.");

            foreach (var markAR in markSB.MarksAR)
            {
               markAR.CreateBlock();
            }
         }
         progressMeter.Stop();
      }

      

      private string loadAbbreviateName()
      {
         string res = "Н47Г"; // default
         try
         {
            // из словаря чертежа
            res = DictNOD.LoadAbbr();
            if (string.IsNullOrEmpty(res))
            {
               var keyAKR = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegAppPath);
               res = (string)keyAKR.GetValue("Abbreviate", "Н47Г");
            }
         }
         catch { }
         return res;
      }

      private int loadNumberFirstFloor()
      {
         int res = 2;
         try
         {
            // из словаря чертежа
            res = DictNOD.LoadNumberFirstFloor();
            if (res == 0)
            {
               res = 2; // default
            }
         }
         catch { }
         return res;
      }

      private int promptNumberFirtsFloor()
      {
         int numberFirstFloor;
         int defaultNumber;
         if (_numberFirstFloor == 0)
         {
            defaultNumber = loadNumberFirstFloor();
         }
         else
         {
            defaultNumber = _numberFirstFloor;
         }
         var opt = new PromptStringOptions("\nВведите номер для первого этажа панелей:");
         opt.DefaultValue = defaultNumber.ToString();
         do
         {
            var res = _doc.Editor.GetString(opt);
            if (res.Status == PromptStatus.OK)
            {
               if (int.TryParse(res.StringResult, out numberFirstFloor) && numberFirstFloor != 0)
               {
                  saveNumberFirstFloor(numberFirstFloor);
               }
               else
               {
                  _doc.Editor.WriteMessage("\nНомер не определен. Повтортите.");
               }
            }
            else
            {
               throw new System.Exception("Прервано пользователем.");
            }
         } while (numberFirstFloor == 0);
         return numberFirstFloor;
      }

      // Переименование марок АР панелей в соответствии с индексами архитекторов (Э2_Яр1)
      private void RenamePanelsToArchitectIndex(List<MarkSbPanelAR> marksSB)
      {
         // Определение этажа панели.
         _storeys = Storey.IdentificationStoreys(marksSB, _numberFirstFloor);
         // Маркировка Марок АР по архитектурному индексу
         foreach (var markSB in marksSB)
         {
            markSB.DefineArchitectMarks(marksSB);
         }
      }

      // Замена вхождений блоков панелей Марки СБ на панели Марки АР
      private void ReplaceBlocksMarkSbOnMarkAr()
      {
         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.SetLimit(_marksSB.Count);
         progressMeter.Start("Замена вхождений блоков панелей Марки СБ на панели Марки АР ");

         using (var t = _db.TransactionManager.StartTransaction())
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(_db), OpenMode.ForWrite) as BlockTableRecord;
            foreach (var markSb in _marksSB)
            {
               if (HostApplicationServices.Current.UserBreak())
                  throw new System.Exception("Отменено пользователем.");

               markSb.ReplaceBlocksSbOnAr(t, ms);
               progressMeter.MeterProgress();
            }
            t.Commit();
         }
         progressMeter.Stop();
      }

      private void saveAbbreviateName(string abbr)
      {
         try
         {
            // в реестр
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(RegAppPath);
            keyAKR.SetValue("Abbreviate", abbr, Microsoft.Win32.RegistryValueKind.String);
            // в словарь чертежа
            DictNOD.SaveAbbr(abbr);
         }
         catch { }
      }

      private void saveNumberFirstFloor(int numberFirstFloor)
      {
         try
         {
            // в словарь чертежа
            DictNOD.SaveNumberFirstFloor(numberFirstFloor);
         }
         catch { }
      }
   }
}
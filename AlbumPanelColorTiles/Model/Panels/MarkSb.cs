using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Blocks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Base;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RTreeLib;

namespace AlbumPanelColorTiles.Panels
{
    // Панели марки СБ
    public class MarkSb : IEquatable<MarkSb>
    {
        private string _abbr;
        private Point2d _centerPanel;

        // зоны покраски внутри определения блока (приоритет выше чем у зон в модели).
        private List<ColorArea> _colorAreas;

        // Границы блока по плитке
        private Extents3d _extentsTiles;
        private double _heightPanelByTile;
        private ObjectId _idBtr;
        private bool _isEndLeftPanel;
        private bool _isEndRightPanel;
        private List<MarkAr> _marksAR;
        private string _markSb;
        // может быть с _тп или _тл
        private string _markSbBlockName;
        private string _markSbClean;
        // без _тп или _тл
        private List<Paint> _paints;
        private RTree<ColorArea> _rtreeColorArea;
        private EnumStorey _storeyTypePanel;
        // Список плиток в панели Марки СБ
        private List<Tile> _tiles;        

        // Конструктор. Скрытый.
        private MarkSb(BlockReference blRefPanel, ObjectId idBtrMarkSb, string markSbName, string markSbBlockName, Album album)
        {
            Album = album;
            _abbr = album.StartOptions.Abbr;
            _markSb = markSbName;
            _idBtr = idBtrMarkSb;
            _markSbBlockName = markSbBlockName;
            defineStoreyTypePanel(blRefPanel);
            checkPanelIndexes(markSbName);
            _marksAR = new List<MarkAr>();
            _colorAreas = ColorArea.GetColorAreas(_idBtr, album);
            _rtreeColorArea = ColorArea.GetRTree(_colorAreas);
            // Список плиток (в определении блока марки СБ)
            IterateBtrEnt();
            // Центр панели
            _centerPanel = GetCenterPanel(_tiles);
        }

        public string Abbr { get { return _abbr; } }
        public Album Album { get; private set; }
        public Point2d CenterPanel { get { return _centerPanel; } }
        public Extents3d ExtentsTiles { get { return _extentsTiles; } }
        public double HeightPanelByTile { get { return _heightPanelByTile; } }
        public ObjectId IdBtr { get { return _idBtr; } }
        public bool IsEndLeftPanel { get { return _isEndLeftPanel; } }
        public bool IsEndRightPanel { get { return _isEndRightPanel; } }
        public List<MarkAr> MarksAR { get { return _marksAR; } }
        public string MarkSbBlockName { get { return _markSbBlockName; } }

        public string MarkSbClean
        {
            get
            {
                if (_markSbClean == null)
                {
                    _markSbClean = GetMarkSbCleanName(_markSb);
                }
                return _markSbClean;
            }
        }

        public string MarkSbName { get { return _markSb; } }
        public List<Paint> Paints { get { return _paints; } }
        public EnumStorey StoreyTypePanel { get { return _storeyTypePanel; } }
        // Свойства
        public List<Tile> Tiles { get { return _tiles; } }
        // Окна
        public List<BlockWindow> Windows { get; private set; } = new List<BlockWindow>();

        // Суммарная площадь плитки на панель (расход м2 на панель).
        public double TotalAreaTiles
        {
            get
            {
                return Math.Round(_paints.Count * TileCalc.OneTileArea, 2);
            }
        }

        /// <summary>
        /// Индекс окна для добавления в марку покраски
        /// </summary>
        public int WindowIndex { get; set; }
        /// <summary>
        /// Название окна в блоке панели после приставки _ок - например "[ОП6]", "ОП6 ОП5" - что человек напишет.
        /// </summary>
        public string WindowName { get; set; }        
        /// <summary>
        /// Создание определения блока марки СБ из блока марки АР, и сброс покраски плитки (в слой 0)
        /// </summary>        
        public static void CreateBlockMarkSbFromAr(ObjectId idBtrMarkAr, string markSbBlName)
        {
            // Копирование блока
            var idBtrMarkSb = Lib.Block.CopyBtr(idBtrMarkAr, markSbBlName);            
        }        

        public static string GetMarkSbBlockName(string markSb)
        {
            return Settings.Default.BlockPanelAkrPrefixName + markSb;
        }

        /// <summary>
        /// Чистая марка СБ - без суффиксов _ТП, _ТЛ, _ОК
        /// </summary>
        /// <param name="markSbName"></param>
        /// <returns></returns>
        public static string GetMarkSbCleanName(string markSbName)
        {
            int indexEndLeftPanel = markSbName.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase);
            indexEndLeftPanel = indexEndLeftPanel > 0 ? indexEndLeftPanel : 0;
            int indexEndRightPanel = markSbName.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase);
            indexEndRightPanel = indexEndRightPanel > 0 ? indexEndRightPanel : 0;
            int indexWindow = markSbName.IndexOf(Settings.Default.WindowPanelSuffix, StringComparison.OrdinalIgnoreCase);
            indexWindow = indexWindow > 0 ? indexWindow : 0;
            int[] indexes = { indexEndLeftPanel, indexEndRightPanel, indexWindow };
            var indexesAboveZero = indexes.Where(i => i > 0);
            if (indexesAboveZero.Count() > 0)
            {
                int index = indexesAboveZero.Min();
                return markSbName.Substring(0, index);
            }
            return markSbName;
        }

        /// <summary>
        /// Определение марки СБ (может быть с суффиксом торца _тл или _тп, и индекс окна _ок1 и т.п.). Отбрасывается последняя часть имени в скобках (это марка АР).
        /// </summary>
        /// <param name="blName">Имя блока</param>
        /// <returns>Марка СБ - без приставки АКР_Панель_, и без () марки покраски</returns>
        public static string GetMarkSbName(string blName)
        {
            string markSb = string.Empty;
            if (IsBlockNamePanel(blName))
            {
                // Хвостовая часть
                markSb = blName.Substring(Settings.Default.BlockPanelAkrPrefixName.Length);
                if (markSb.EndsWith(")"))
                {
                    int lastDirectBracket = markSb.LastIndexOf('(');
                    if (lastDirectBracket > 0)
                    {
                        markSb = markSb.Substring(0, lastDirectBracket);
                    }
                }
            }
            return markSb;
        }

        // Определение покраски панелей текущего чертежа (в Модели)
        public static List<MarkSb> GetMarksSB(RTree<ColorArea> rtreeColorAreas, Album album, string progressMsg, List<ObjectId> idsBlRefPanels)
        {
            List<MarkSb> marksSb = new List<MarkSb>();
            Database db = HostApplicationServices.WorkingDatabase;

            using (var t = db.TransactionManager.StartTransaction())
            {
                // Перебор всех блоков в модели и составление списка блоков марок и панелей.
                var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                var ms = bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;

                ProgressMeter progressMeter = new ProgressMeter();
                progressMeter.SetLimit(idsBlRefPanels.Count);
                progressMeter.Start(progressMsg);

                // Перебор вхожденй блоков Марки СБ
                foreach (ObjectId idBlRefPanelMarkSb in idsBlRefPanels)
                {
                    if (HostApplicationServices.Current.UserBreak())
                    {
                        throw new System.Exception("Отменено пользователем.");
                    }
                    progressMeter.MeterProgress();

                    var blRefPanel = idBlRefPanelMarkSb.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    // Определение Марки СБ панели. Если ее еще нет, то она создается и добавляется в список _marks.
                    MarkSb markSb = GetMarkSb(blRefPanel, marksSb, bt, album);
                    if (markSb == null)
                    {
                        // Значит это не блок панели. Пропускаем.
                        continue;
                    }
                    //Определение покраски панели (Марки АР)
                    List<Paint> paintAR = MarkAr.GetPanelMarkAR(markSb, blRefPanel, rtreeColorAreas);
                    // Добавление панели АР в список панелей для Марки СБ
                    markSb.AddPanelAR(paintAR, blRefPanel, markSb);
                }
                progressMeter.Stop();
                t.Commit();
            }
            return marksSb;
        }

        /// <summary>
        /// Возвращает марку панели из имени блока панели (для панелей Марки СБ и Марок АР).
        /// </summary>
        /// <param name="blName">Имя блока панели</param>
        /// <returns>марка панели (СБ или СБ+АР)</returns>
        public static string GetPanelMarkFromBlockName(string blName, List<MarkSb> marksSB)
        {
            //return blName.Substring(Album.Options.BlockPanelPrefixName.Length);
            string panelMark = string.Empty;
            // Найти имя марки панели СБ или АР
            foreach (var markSB in marksSB)
            {
                if (markSB.MarkSbBlockName == blName)
                {
                    return markSB.MarkSbClean;
                }
                else
                {
                    foreach (var markAR in markSB.MarksAR)
                    {
                        if (markAR.MarkArBlockName == blName)
                        {
                            return markAR.MarkARPanelFullName;
                        }
                    }
                }
            }
            return panelMark;
        }

        // Проверка, это блок панели, по имени (Марки СБ или Марки АР)
        public static bool IsBlockNamePanel(string blName)
        {
            return blName.StartsWith(Settings.Default.BlockPanelAkrPrefixName);
        }

        // Проверка, это имя блоки марки АР, по имени блока.
        public static bool IsBlockNamePanelMarkAr(string blName)
        {
            bool res = false;
            if (IsBlockNamePanel(blName))
            {
                string markSb = GetMarkSbName(blName); // может быть с суффиксом торца _тп или _тл
                string markSbBlName = GetMarkSbBlockName(markSb);// может быть с суффиксом торца _тп или _тл
                res = blName != markSbBlName;
            }
            return res;
        }

        public bool Equals(MarkSb other)
        {
            return _markSb.Equals(other._markSb) &&
               _colorAreas.SequenceEqual(other._colorAreas) &&
               _idBtr.Equals(other._idBtr) &&
               _paints.SequenceEqual(other._paints) &&
               _tiles.SequenceEqual(other._tiles) &&
               _marksAR.SequenceEqual(other._marksAR);
        }

        // Замена вхождений блоков СБ на АР
        public void ReplaceBlocksSbOnAr(Transaction t, BlockTableRecord ms)
        {
            foreach (var markAr in _marksAR)
            {
                markAr.ReplaceBlocksSbOnAr(t, ms);
            }
        }

        // Определение марки СБ, если ее еще нет, то создание и добавление в список marks.
        private static MarkSb GetMarkSb(BlockReference blRefPanelSb, List<MarkSb> marksSb, BlockTable bt, Album album)
        {
            MarkSb markSb = null;
            string markSbName = GetMarkSbName(blRefPanelSb.Name);
            if (markSbName != string.Empty)
            {
                // Поиск панели Марки СБ в коллекции панелей по имени марки СБ.
                markSb = marksSb.Find(m => m._markSb == markSbName);
                if (markSb == null)
                {
                    // Блок Марки СБ
                    string markSbBlName = GetMarkSbBlockName(markSbName);
                    if (bt.Has(markSbBlName))
                    {
                        var idMarkSbBtr = bt[markSbBlName];
                        markSb = new MarkSb(blRefPanelSb, idMarkSbBtr, markSbName, markSbBlName, album);
                        marksSb.Add(markSb);
                    }
                    else
                    {
                        //TODO: Ошибка в чертеже. Блок с Маркой АР есть, а блока Марки СБ нет. Добавить в колекцию блоков с ошибками.
                        //???
                        Inspector.AddError($"Блок марки АР есть, а блока марки СБ нет - {blRefPanelSb.Name}", blRefPanelSb, icon: System.Drawing.SystemIcons.Error);
                    }
                }
            }
            return markSb;
        }

        // Добавление панели АР по списку ее покраски
        private void AddPanelAR(List<Paint> paintAR, BlockReference blRefPanel, MarkSb markSb)
        {
            // Проверка нет ли уже такой марки покраси АР
            MarkAr panelAR = HasPanelAR(paintAR);
            if (panelAR == null)
            {
                panelAR = new MarkAr(paintAR, markSb, blRefPanel);
                _marksAR.Add(panelAR);
            }
            panelAR.AddBlockRefPanel(blRefPanel);
        }

        // Проверка есть ли доп индексы в имени блока панели марки СБ - такие как ТП, ТЛ, ОК№
        private void checkPanelIndexes(string markSbName)
        {
            // markSbName - марка СБ (без приставки АКР_Панель_)
            // проверка торца
            if (markSbName.IndexOf(Settings.Default.EndLeftPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
            {
                _isEndLeftPanel = true; //markSbName.EndsWith(Album.Options.endLeftPanelSuffix); // Торец слева
            }
            if (markSbName.IndexOf(Settings.Default.EndRightPanelSuffix, StringComparison.OrdinalIgnoreCase) != -1)
            {
                _isEndRightPanel = true; //markSbName.EndsWith(Album.Options.endRightPanelSuffix); // Торец спрва
            }

            string winName;
            AkrHelper.GetMarkWithoutWindowsSuffix(markSbName, out winName);
            WindowName = winName;            
        }

        /// <summary>
        /// Определение архитектурных Марок АР (Э1_Яр1)        
        /// </summary>
        public void DefineArchitectMarks ()
        {
            Dictionary<string, MarkAr> marksArArchitectIndex = new Dictionary<string, MarkAr>();
            if (StoreyTypePanel == EnumStorey.Upper)
            {
                defUpperStoreyAndParapetArchMarks(marksArArchitectIndex, Settings.Default.PaintIndexUpperStorey);
            }
            else if (StoreyTypePanel == EnumStorey.Parapet)
            {
                defUpperStoreyAndParapetArchMarks(marksArArchitectIndex, Settings.Default.PaintIndexParapet);
            }
            //else if (Album.StartOptions.EndsInPainting && (IsEndRightPanel || IsEndLeftPanel))
            //{
            //    defEndsArchMarks(marksArArchitectIndex);
            //}
            else
            {
                defOtherArchMarks(marksArArchitectIndex);
            }
        }

        //private void defEndsArchMarks(Dictionary<string, MarkAr> marksArArchitectIndex)
        //{
        //    // Торцевые панели (Э1ТЛ_Яр1)
        //    string endIndex = GetEndSuffix();
        //    // Панели этажей
        //    //int i = 1;
        //    Dictionary<string, int> indexTypeColor = new Dictionary<string, int>();
        //    foreach (var markAR in MarksAR)
        //    {
        //        string markPaint;
        //        var floors = markAR.Panels.GroupBy(p => p.Storey.Number).Select(p => p.First().Storey.Number).OrderBy(f => f);
        //        string floor = GetFloorsSequence(floors);// String.Join(",", floors);
        //        markPaint = $"{Settings.Default.PaintIndexStorey}{floor}{endIndex}"; // Э2,3,4ТП
        //        if (marksArArchitectIndex.ContainsKey(markPaint))
        //        {
        //            int i = GetIndecTypePainting(indexTypeColor, markPaint);
        //            markPaint = $"{markPaint}{Album.StartOptions.SplitIndexPainting}{i}";
        //        }
        //        marksArArchitectIndex.Add(markPaint, markAR);
        //        if (markAR.MarkSB.WindowIndex > 0)
        //        {
        //            markPaint += Settings.Default.PaintIndexWindow + markAR.MarkSB.WindowIndex; // -ОК1
        //        }
        //        markAR.MarkPaintingCalulated = markPaint;
        //    }
        //}

        private void defOtherArchMarks(Dictionary<string, MarkAr> marksArArchitectIndex)
        {
            // Панели этажей
            Dictionary<string, int> indexTypeColor = new Dictionary<string, int>();
            //int i = 1;
            foreach (var markAR in MarksAR)
            {
                string markPaint;
                var floors = markAR.Panels.GroupBy(p => p.Storey.Number).Select(p => p.First().Storey.Number).OrderBy(f => f);
                string floor = GetFloorsSequence(floors); // String.Join(",", floors);
                markPaint = $"{Settings.Default.PaintIndexStorey}{floor}{GetEndSuffix()}";//Э2,5,8
                if (marksArArchitectIndex.ContainsKey(markPaint))
                {                    
                    int i = GetIndecTypePainting(indexTypeColor, markPaint);
                    markPaint = $"{markPaint}{Album.StartOptions.SplitIndexPainting}{i}";
                }
                marksArArchitectIndex.Add(markPaint, markAR);
                if (markAR.MarkSB.WindowIndex > 0)
                {
                    markPaint += Settings.Default.PaintIndexWindow + markAR.MarkSB.WindowIndex;// -ОК1
                }
                markAR.MarkPaintingCalulated = markPaint;
            }
        }

        private void defUpperStoreyAndParapetArchMarks(Dictionary<string, MarkAr> marksArArchitectIndex, string index)
        {
            // Панели чердака
            // (ЭЧ-#_Яр1)
            var markAr = MarksAR[0];
            var endSuff = GetEndSuffix();
            if (MarksAR.Count == 1)
            {
                // Если одна марка покраски
                
                string markPaint = $"{Settings.Default.PaintIndexStorey}{index}{endSuff}"; // "ЭЧ"
                marksArArchitectIndex.Add(markPaint, markAr);
                if (markAr.MarkSB.WindowIndex > 0)
                {
                    markPaint += Settings.Default.PaintIndexWindow + markAr.MarkSB.WindowIndex; // -ОК1
                }
                markAr.MarkPaintingCalulated = markPaint;
            }
            else
            {
                // Если несколько марок покраски
                int i = 1;
                foreach (var markAR in MarksAR)
                {
                    string markPaint = $"{Settings.Default.PaintIndexStorey}{index}{Album.StartOptions.SplitIndexPainting}{i++}{endSuff}"; // ЭЧ-1
                    marksArArchitectIndex.Add(markPaint, markAR);
                    if (markAR.MarkSB.WindowIndex > 0)
                    {
                        markPaint += Settings.Default.PaintIndexWindow + markAR.MarkSB.WindowIndex;//"-ОК1"
                    }
                    markAR.MarkPaintingCalulated = markPaint;
                }
            }
        }

        private string GetEndSuffix ()
        {            
            string suf = "";
            if (Album.StartOptions.EndsInPainting)
            {
                if (IsEndLeftPanel)
                {
                    suf = Settings.Default.PaintIndexEndLeftPanel;
                }
                else if (IsEndRightPanel)
                {
                    suf += Settings.Default.PaintIndexEndRightPanel;
                }
            }
            return suf;
        }

        // Определение центра панели по блокам плиток в ней
        private Point2d GetCenterPanel(List<Tile> _tiles)
        {
            _extentsTiles = new Extents3d();
            foreach (var tile in _tiles)
            {
                _extentsTiles.AddPoint(tile.CenterTile);
            }
            _heightPanelByTile = _extentsTiles.MaxPoint.Y - _extentsTiles.MinPoint.Y + Settings.Default.TileSeam + Settings.Default.TileHeight;
            return new Point2d((_extentsTiles.MinPoint.X + _extentsTiles.MaxPoint.X) * 0.5, (_extentsTiles.MinPoint.Y + _extentsTiles.MaxPoint.Y) * 0.5);
        }

        // Получение списка плиток в определении блока
        private void IterateBtrEnt()
        {
            _tiles = new List<Tile>();
            _paints = new List<Paint>();

            var btrMarkSb = _idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
                if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                {
                    var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    string blName = blRef.GetEffectiveName();
                    // Плитка
                    if (blName.Equals(Settings.Default.BlockTileName, StringComparison.OrdinalIgnoreCase))
                    {
                        Tile tile = new Tile(blRef, blName);
                        //Определение покраски плитки
                        Paint paint = ColorArea.GetPaint(tile.CenterTile, _rtreeColorArea);
                        _tiles.Add(tile);
                        _paints.Add(paint);
                    }
                    // Окна                  
                    else if (BlockWindow.IsblockWindow(blName))
                    {
                        BlockWindow window = new BlockWindow(blRef, blName, this);
                        Windows.Add(window);
                    }
                }
            }
        }

        // Поиск покраски марки АР в списке _marksAR
        private MarkAr HasPanelAR(List<Paint> paintAR)
        {
            //Поиск панели АР по покраске
            MarkAr resPanelAR = null;
            //Сравнение списков покраски
            foreach (MarkAr panelAR in _marksAR)
            {
                if (panelAR.EqualPaint(paintAR))
                {
                    resPanelAR = panelAR;
                    break;
                }
            }
            return resPanelAR;
        }

        private static string GetFloorsSequence (IEnumerable<int> floors)
        {
            string res = string.Empty;
            int firstFloor = floors.First();
            int lastFloor = floors.Last();
            int countFloors = floors.Count();
            var rightRange = Enumerable.Range(firstFloor, countFloors);
            if (countFloors>3 && rightRange.SequenceEqual(floors))
            {
                res = firstFloor + "-" + lastFloor;
            }
            else
            {
                res = string.Join(",", floors);
            }
            return res;
        }

        // индекс отличия панели по виду окна, 1,2,3 и т.д. по порядку.
        private void defineStoreyTypePanel (BlockReference blRefPanel)
        {
            // Определение типа этажа панели
            if (string.Equals(blRefPanel.Layer, Settings.Default.LayerUpperStoreyPanels, StringComparison.OrdinalIgnoreCase))
            {
                _storeyTypePanel = EnumStorey.Upper;
            }
            else if (string.Equals(blRefPanel.Layer, Settings.Default.LayerParapetPanels, StringComparison.OrdinalIgnoreCase))
            {
                _storeyTypePanel = EnumStorey.Parapet;
            }
        }

        private static int GetIndecTypePainting (Dictionary<string, int> indexTypeColor, string markPaint)
        {
            int i;
            if (indexTypeColor.ContainsKey(markPaint))
            {
                i = indexTypeColor[markPaint] + 1;
                indexTypeColor[markPaint] = i;
            }
            else
            {
                i = 1;
                indexTypeColor.Add(markPaint, i);
            }
            return i;
        }
    }
}
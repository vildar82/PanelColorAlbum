using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.Colors;

namespace AlbumPanelColorTiles.Panels
{
    // Колическтво плиток
    public class TileCalc : IComparable<TileCalc>
    {
        private static double _oneTileArea = Settings.Default.TileLenght * Settings.Default.TileHeight * 0.000001;

        public static double OneTileArea { get { return _oneTileArea; } }
        public Paint Paint { get; private set; }
        public int Count { get; set; }        
        public double TotalArea { get { return Math.Round(OneTileArea * Count, 2); } }

        public TileCalc(Paint paint, int count)
        {
            Paint = paint;
            Count = count;            
        }

        public static List<TileCalc> CalcAlbum(Album album)
        {
            Dictionary<string, TileCalc> totalTileCalcAlbum = new Dictionary<string, TileCalc>();
            // Подсчет общего кол плитки на альбом
            foreach (var markSb in album.MarksSB)
            {
                foreach (var markAr in markSb.MarksAR)
                {
                    // Кол плитки разных цветов в панели Марки АР
                    foreach (var tileCalcInOneMarkAr in markAr.TilesCalc)
                    {
                        // Кол плитки этого цвета суммарно во всех панелях этой марки
                        var tileCalcInAllMarksAr = tileCalcInOneMarkAr * markAr.Panels.Count;
                        // добавление плиток этого цвета в общий список плиток на альбом
                        TileCalc totalTileCalcColor;
                        if (totalTileCalcAlbum.TryGetValue(tileCalcInAllMarksAr.Paint.Layer, out totalTileCalcColor))
                        {
                            // суммирование плиток одного цвета
                            totalTileCalcColor.Count += tileCalcInAllMarksAr.Count;
                        }
                        else
                        {
                            totalTileCalcAlbum.Add(tileCalcInAllMarksAr.Paint.Layer, tileCalcInAllMarksAr);
                        }
                    }
                }
            }
            var res = totalTileCalcAlbum.Values.ToList();
            res.Sort();
            return res;
        }

        public static TileCalc operator *(TileCalc tileCalc, int countPanels)
        {
            return new TileCalc(tileCalc.Paint, tileCalc.Count * countPanels);
        }

        /// <summary>
        /// Сравнение по кол-ву плиток
        /// </summary>
        public int CompareTo(TileCalc other)
        {
            return other.Count.CompareTo(Count);
        }
    }
}
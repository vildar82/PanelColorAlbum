using System;
using AcadLib.Layers;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RTreeLib;
using AcadLib.Blocks;
using AcadLib;

namespace AlbumPanelColorTiles.Panels
{
    // Плитка
    public class Tile : BlockBase, IEquatable<Tile>
    {
        public const string PropArticle = "АРТИКУЛ";
        //private Extents3d _bounds;
        private Point3d _centerTile;
        private Paint _paint;
        //private ObjectId _idBlRef;

        // Id внутри определения блока панели (марки СБ).
        public Tile(BlockReference blRefTile, string blName) : base(blRefTile, blName)
        {
            //_idBlRef = blRefTile.ObjectId;
            //_bounds = blRefTile.GeometricExtents;
            _centerTile = new Point3d((Bounds.Value.MaxPoint.X + Bounds.Value.MinPoint.X) * 0.5,
                                      (Bounds.Value.MaxPoint.Y + Bounds.Value.MinPoint.Y) * 0.5, 0);
        }

        public Point3d CenterTile { get { return _centerTile; } }

        public static void GetColorNameFromLayer (string layerName, out string article, out string ncs)
        {
            var splitUnders = layerName.Split(new char[] { '_' }, 2);

            article = splitUnders[0];

            if (splitUnders.Length == 2)
            {
                ncs = splitUnders[1];
            }
            else
            {
                ncs = string.Empty;
            }
        }

        /// <summary>
        /// Покраска блоков плитки в Модели (без блоков АКР-Панелей)
        /// </summary>
        public static void PaintTileInModel(RTree<ColorArea> rtreeColorAreas)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var t = db.TransactionManager.StartTransaction())
            {
                PaintTileInBtr(SymbolUtilityServices.GetBlockModelSpaceId(db), rtreeColorAreas, Matrix3d.Identity);
                t.Commit();
            }
        }

        public static void PaintTileInBtr(ObjectId idBtr, RTree<ColorArea> rtreeColorAreas, Matrix3d transToModel)
        {
            var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btr)
            {
                if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                {
                    var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    string blName = blRef.GetEffectiveName();
                    if (blName.StartsWith(Settings.Default.BlockTileName, StringComparison.OrdinalIgnoreCase))
                    {
                        Tile tile = new Tile(blRef, blName);
                        //Определение покраски плитки
                        Paint paint = ColorArea.GetPaint(tile.CenterTile.TransformBy(transToModel), rtreeColorAreas);
                        if (paint != null)
                        {
                            blRef.UpgradeOpen();
                            blRef.Layer = paint.Layer;
                            FillTileArticle(blRef, paint.Article); // Так же поворачивает атрибут
                            //tile.SetPaint(paint);
                        }
                    }
                    else if (!MarkSb.IsBlockNamePanel(blName))
                    {
                        // Покраска во вложенных блоках, кроме АРК панелей
                        PaintTileInBtr(blRef.BlockTableRecord, rtreeColorAreas, blRef.BlockTransform * transToModel);
                    }
                }
            }
        }

        private void SetPaint(Paint paint)
        {
            _paint = paint;
            if (_paint != null)
            {
                FillPropValue(PropArticle, _paint.Article, isRequired:false);
            }
        }

        public bool Equals(Tile other)
        {
            return IdBlRef.Equals(other.IdBlRef) &&
               Bounds.Value.IsEqualTo(other.Bounds.Value, Album.Tolerance);
        }

        /// <summary>
        /// Нормализация плиток - слой, поворот артикула и установка пустого артикула
        /// </summary>        
        public static void TilesNormalize(ObjectId idBtrMarkSb)
        {
            // Перенос блоков плиток на слой                 
            var layerTile = LayerExt.CheckLayerState(new LayerInfo("АР_Плитка"));
            var btrMarkSb = idBtrMarkSb.GetObject(OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in btrMarkSb)
            {
                if (!idEnt.IsValidEx()) continue;
                var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                if (blRef == null) continue;
                if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                {
                    blRef.UpgradeOpen();
                    // Слой
                    blRef.LayerId = layerTile;
                    // Нормализация плитки (зеркальность, поворот)
                    //blRef.Normalize(); // Есть плитки с подрезкой - как ее нормализовать, пока непонтно!!!???
                    Tile.FillTileArticle(blRef, "");
                    blRef.DowngradeOpen();
                }
            }
        }

        /// <summary>
        /// Запись артикла цвета в атрибут блока плитки
        /// </summary>
        /// <param name="blRef">Блок плитки открытый на запись</param>
        /// <param name="article">Артикул</param>
        public static void FillTileArticle(BlockReference blRef, string article)
        {
            if (blRef.AttributeCollection != null)
            {
                foreach (ObjectId atrId in blRef.AttributeCollection)
                {
                    if (!atrId.IsValidEx()) continue;
                    using (var atr = atrId.Open(OpenMode.ForRead) as AttributeReference)
                    {
                        if (atr == null) continue;
                        if (atr.Tag.Equals(Tile.PropArticle, StringComparison.OrdinalIgnoreCase))
                        {
                            atr.UpgradeOpen();
                            atr.TextString = article;
                            if (atr.Rotation!=0)
                            {
                                atr.Rotation = 0;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
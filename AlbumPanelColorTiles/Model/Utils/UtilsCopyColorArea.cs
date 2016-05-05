using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.DB;
using AcadLib.Layers;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils
{
    public static class UtilsCopyColorArea
    {
        public static Document Doc { get; private set; }
        public static Editor Ed { get; private set; }
        public static Database Db { get; private set; }
        public static List<ColorArea> ToCopy { get; private set; }
        public static Point3d Base { get; private set; }
        public static Dictionary<string, LayerInfo> Layers { get; private set; }

        public static void Copy()
        {
            Init();
            ToCopy = new List<ColorArea>();
            // Выбор блоков зон покраски.

            var idsSelect = GetSelection();
            Base = Ed.GetPointWCS("Базовая точка");
            Layers = new Dictionary<string, LayerInfo>();
            // Фильтр зон покраски
            using (var t = Db.TransactionManager.StartTransaction())
            {                
                foreach (var idBlRef in idsSelect)
                {
                    var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;
                    var blName = blRef.GetEffectiveName();
                    if (blName.Equals(Settings.Default.BlockColorAreaName, StringComparison.OrdinalIgnoreCase))
                    {
                        ColorArea ca = new ColorArea(blRef);
                        ToCopy.Add(ca);
                        // добавление слоя в словарь
                        if(!Layers.ContainsKey(blRef.Layer))
                        {
                            LayerInfo li = new LayerInfo(blRef.LayerId);
                            Layers.Add(li.Name, li);
                        }
                    }
                }
                t.Commit();
            }
            Ed.WriteMessage($"\nСкопировано {ToCopy.Count} блоков зон покраски.");
        }

        private static List<ObjectId> GetSelection()
        {
            var sel = Ed.SelectImplied();
            if (sel.Status ==PromptStatus.OK)
            {
               return sel.Value.GetObjectIds().ToList();
            }
            return Ed.SelectBlRefs("Выбор блоков");
        }

        public static void Paste()
        {
            Init();
            if (ToCopy == null || ToCopy.Count == 0)
            {
                Ed.WriteMessage("\nБуфер пустой! Сначала нужно скопировать зоны покраски!");
                return;
            }

            Base = Ed.GetPointWCS("Базовая точка");

            // Группировка зон покраски по параметрам длины и высоты
            var sizeGroups = ToCopy.GroupBy(c => new { c.Length, c.Height });

            using (var t = Db.TransactionManager.StartTransaction())
            {
                // Получение определения блока зоны в текущем чертеже.
                ObjectId idBtrColorArea = GetColorAreaIdBtr();
                var cs = Db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                // Создание всех слоев для зон покраски
                checkLayers();

                // Создание блока шаблона для каждого типоразмера зон покраски
                foreach (var size in sizeGroups)
                {
                    var idBlRefTemplate = GetBlRefTemplate(size.Key.Length, size.Key.Height, idBtrColorArea, cs, t);
                    var idColToCopy = new ObjectIdCollection(new[] { idBlRefTemplate });
                    foreach (var item in size)
                    {
                        copyItem(item, idColToCopy, cs.Id, t);
                    }
                    // Удаление блока шаблона
                    var blRefTemplate = idBlRefTemplate.GetObject(OpenMode.ForWrite) as BlockReference;
                    blRefTemplate.Erase();
                }
                t.Commit();
            }
            Ed.WriteMessage($"\nВставлено {ToCopy.Count} блоков зон покраски.");
        }        

        private static void Init()
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            Ed = Doc.Editor;
            Db = Doc.Database;
        }

        private static ObjectId GetColorAreaIdBtr()
        {
            // Проверка есть ли блок зоны покраски в текущем чертеже.
            var bt = Db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            if (bt.Has(Settings.Default.BlockColorAreaName))
            {
                return bt[Settings.Default.BlockColorAreaName];
            }
            // Если нет, то копирование из шаблона блоков АКР
            return Lib.BlockInsert.CopyBlockFromTemplate(Settings.Default.BlockColorAreaName, Db);
        }

        private static ObjectId GetBlRefTemplate(double length, double height, ObjectId idBtr, 
                                            BlockTableRecord btrOwner, Transaction t)
        {
            ObjectId res = ObjectId.Null;
            using (BlockReference blRefTemplate = new BlockReference(Point3d.Origin, idBtr))
            {
                blRefTemplate.ColorIndex = 256;
                blRefTemplate.Linetype = SymbolUtilityServices.LinetypeByLayerName;
                blRefTemplate.LineWeight = LineWeight.ByLayer;
                btrOwner.AppendEntity(blRefTemplate);
                t.AddNewlyCreatedDBObject(blRefTemplate, true);
                foreach (DynamicBlockReferenceProperty prop in blRefTemplate.DynamicBlockReferencePropertyCollection)
                {
                    if (prop.PropertyName.Equals(Settings.Default.BlockColorAreaDynPropLength, StringComparison.OrdinalIgnoreCase))
                    {
                        prop.Value = length;
                    }
                    else if (prop.PropertyName.Equals(Settings.Default.BlockColorAreaDynPropHeight, StringComparison.OrdinalIgnoreCase))
                    {
                        prop.Value = height;
                    }
                }
                res = blRefTemplate.Id;
            }
            return res;
        }

        private static void copyItem(ColorArea item, ObjectIdCollection idColToCopy, ObjectId idBtrOwner, Transaction t)
        {
            using (IdMapping map = new IdMapping())
            {
                Db.DeepCloneObjects(idColToCopy, idBtrOwner, map, false);
                ObjectId idBlRefCopy = map[idColToCopy[0]].Value;

                if (idBlRefCopy.IsValid && !idBlRefCopy.IsNull)
                {
                    using (var blRefCopy = t.GetObject(idBlRefCopy, OpenMode.ForWrite, false, true) as BlockReference)
                    {
                        blRefCopy.Position = new Point3d(Base.X + item.Pos.X, Base.Y + item.Pos.Y, 0);
                        blRefCopy.Layer = item.EntInfo.Layer;                                                
                    }
                }
            }
        }

        private static void checkLayers()
        {
            AcadLib.Layers.LayerExt.CheckLayerState(Layers.Values.ToList());
        }
    }

    public class ColorArea
    {
        public double Length { get; private set; }
        public double Height { get; private set; }
        public EntityInfo EntInfo { get; private set; }
        public Point3d Pos { get; private set; }

        public ColorArea(BlockReference blRef)
        {
            EntInfo = new EntityInfo(blRef);
            Pos = new Point3d(blRef.Position.X - UtilsCopyColorArea.Base.X, 
                            blRef.Position.Y - UtilsCopyColorArea.Base.Y, 0);
            foreach (DynamicBlockReferenceProperty prop in blRef.DynamicBlockReferencePropertyCollection)
            {
                if (prop.PropertyName.Equals(Settings.Default.BlockColorAreaDynPropLength,
                                            StringComparison.OrdinalIgnoreCase                    ))
                {
                    Length = (double)prop.Value;
                }
                else if(prop.PropertyName.Equals(Settings.Default.BlockColorAreaDynPropHeight, 
                    StringComparison.OrdinalIgnoreCase))
                {
                    Height = (double)prop.Value;
                }
            }
        }
    }
}

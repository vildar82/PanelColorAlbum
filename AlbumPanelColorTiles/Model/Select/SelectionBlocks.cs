using System;
using System.Linq;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Select
{
    // Выбор панелей в чертеже
    public class SelectionBlocks
    {
        private Database _db;

        public SelectionBlocks (Database db)
        {
            _db = db;
        }

        public SelectionBlocks ()
        {
            _db = HostApplicationServices.WorkingDatabase;
        }

        /// <summary>
        /// Блоки Фасадов в Модели
        /// </summary>
        public List<ObjectId> FacadeBlRefs { get; private set; }

        /// <summary>
        /// Вхождения блоков панелей Марки АР в Модели
        /// </summary>
        public List<ObjectId> IdsBlRefPanelAr { get; private set; }

        /// <summary>
        /// Вхождения блоков панелей Марки СБ в Модели
        /// </summary>
        public List<ObjectId> IdsBlRefPanelSb { get; private set; }

        /// <summary>
        /// Определения блоков панелей Марки АР
        /// </summary>
        public List<ObjectId> IdsBtrPanelAr { get; private set; }

        /// <summary>
        /// Определения блоков панелей Марки СБ
        /// </summary>
        public List<ObjectId> IdsBtrPanelSb { get; private set; }

        /// <summary>
        /// Блоки Секций в Модели
        /// </summary>
        public List<ObjectId> SectionsBlRefs { get; private set; }

        /// <summary>
        /// Выбор определений блоков панелей Марки АР и Марки СБ
        /// </summary>
        public void SelectAKRPanelsBtr ()
        {
            IdsBtrPanelAr = new List<ObjectId>();
            IdsBtrPanelSb = new List<ObjectId>();
            using (var bt = _db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
            {
                foreach (ObjectId idEnt in bt)
                {
                    using (var btr = idEnt.Open(OpenMode.ForRead) as BlockTableRecord)
                    {
                        if (btr == null) continue;
                        if (MarkSb.IsBlockNamePanel(btr.Name))
                        {
                            if (MarkSb.IsBlockNamePanelMarkAr(btr.Name))
                            {
                                IdsBtrPanelAr.Add(idEnt);
                            }
                            else
                            {
                                IdsBtrPanelSb.Add(idEnt);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выбор вхождений блоков панелей Марки АР и Марки СБ в Модели
        /// + выбор блоков Секций.
        /// + выбор фасадов
        /// </summary>
        public void SelectBlRefsInModel (bool sortPanels)
        {
            SectionsBlRefs = new List<ObjectId>();
            FacadeBlRefs = new List<ObjectId>();
            List<KeyValuePair<Point3d, ObjectId>> listPtsIdsBlRefMarkAr = new List<KeyValuePair<Point3d, ObjectId>>();
            List<KeyValuePair<Point3d, ObjectId>> listPtsIdsBlRefMarkSb = new List<KeyValuePair<Point3d, ObjectId>>();

            using (var t = _db.TransactionManager.StartTransaction())
            {
                var lvs = new AcadLib.Layers.LayerVisibleState(_db);
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(_db).GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId idEnt in ms)
                {
                    var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null || !lvs.IsVisible(blRef)) continue;
                    // Блоки АКР-Панелей
                    if (MarkSb.IsBlockNamePanel(blRef.Name))
                    {
                        if (MarkSb.IsBlockNamePanelMarkAr(blRef.Name))
                        {
                            listPtsIdsBlRefMarkAr.Add(new KeyValuePair<Point3d, ObjectId>(blRef.Position, idEnt));
                        }
                        else
                        {
                            listPtsIdsBlRefMarkSb.Add(new KeyValuePair<Point3d, ObjectId>(blRef.Position, idEnt));
                        }
                        continue;
                    }
                    if (blRef.IsDynamicBlock)
                    {
                        // Блоки Секций
                        var blNameEff = blRef.GetEffectiveName();
                        if (string.Equals(blNameEff, Settings.Default.BlockSectionName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            SectionsBlRefs.Add(idEnt);
                        }
                        // Блоки Фасадов
                        else if (string.Equals(blNameEff, Settings.Default.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            FacadeBlRefs.Add(idEnt);
                        }
                    }
                }            
                t.Commit();
            }
            if (sortPanels)
            {
                // сортировка блоков панелей
                IdsBlRefPanelAr = getSortedIdBlrefPanel(listPtsIdsBlRefMarkAr);
                IdsBlRefPanelSb = getSortedIdBlrefPanel(listPtsIdsBlRefMarkSb);
            }
            else
            {
                // Без сортировки панелей
                IdsBlRefPanelAr = listPtsIdsBlRefMarkAr.Select(p => p.Value).ToList();
                IdsBlRefPanelSb = listPtsIdsBlRefMarkSb.Select(p => p.Value).ToList();
            }
        }

        private List<ObjectId> getSortedIdBlrefPanel (List<KeyValuePair<Point3d, ObjectId>> listPtsIdsBlRefMarkAr)
        {
            // группировка по Y с допуском 2000, потом сортировка по X в каждой группе.
            AcadLib.Comparers.DoubleEqualityComparer comparerY = new AcadLib.Comparers.DoubleEqualityComparer(2000);
            return listPtsIdsBlRefMarkAr.OrderBy(p => p.Key.X).GroupBy(p => p.Key.Y, comparerY)
                     .OrderBy(g => g.Key).SelectMany(g => g).Select(g => g.Value).ToList();
        }

        public void SelectSectionBlRefs ()
        {
            SectionsBlRefs = new List<ObjectId>();
            using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(_db).Open(OpenMode.ForRead) as BlockTableRecord)
            {
                foreach (ObjectId idEnt in ms)
                {
                    using (var blRef = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                    {
                        if (blRef == null) continue;
                        if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockSectionName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            SectionsBlRefs.Add(idEnt);
                        }
                    }
                }
            }
        }
    }
}
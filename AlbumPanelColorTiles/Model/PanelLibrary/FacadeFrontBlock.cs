using System;
using System.Linq;
using System.Collections.Generic;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib.RTree.SpatialIndex;

namespace AlbumPanelColorTiles.PanelLibrary
{
    // Блок обозначения стороны фасада на монтажке
    public class FacadeFrontBlock
    {   
        public string BlName { get; private set; }
        public Extents3d Extents { get; private set; }
        public ObjectId IdBlRef { get; private set; }
        public List<MountingPanel> Panels { get; private set; } = new List<MountingPanel>();
        public Point3d Position { get; private set; }
        public Rectangle RectangleRTree { get; private set; }
        public double XMax { get; private set; }
        public double XMin { get; private set; }

        public FacadeFrontBlock (BlockReference blRef)
        {
            Position = blRef.Position;
            BlName = blRef.GetEffectiveName();
            IdBlRef = blRef.Id;
            Extents = blRef.GeometricExtentsСlean();
            RectangleRTree = new Rectangle(Extents.MinPoint.X, Extents.MinPoint.Y, Extents.MaxPoint.X, Extents.MaxPoint.Y, 0, 0);
        }

        public static List<FacadeFrontBlock> GetFacadeFrontBlocks (BlockTableRecord ms, List<FloorMounting> floors)
        {
            List<FacadeFrontBlock> facadeFrontBlocks = new List<FacadeFrontBlock>();
            foreach (ObjectId idEnt in ms)
            {
                var blRef = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                if (blRef == null) { continue; }
                // Если это блок обозначения стороны фасада - по имени блока
                if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
                {
                    FacadeFrontBlock front = new FacadeFrontBlock(blRef);
                    facadeFrontBlocks.Add(front);
                }
            }
            // Найти все блоки монтажных панелей входящих в фасад
            RTree<FloorMounting> rtreeFloors = new RTree<FloorMounting>();
            foreach (var front in floors)
            {
                try
                {
                    rtreeFloors.Add(front.RectangleRTree, front);
                }
                catch { }
            }
            foreach (var front in facadeFrontBlocks)
            {
                // найти соотв обозн стороны фасада                              
                var frontsIntersects = rtreeFloors.Intersects(front.RectangleRTree);

                // если нет пересечений фасадов - пропускаем блок монтажки - он не входит в
                // фасады, просто так вставлен
                if (frontsIntersects.Count == 0)
                {
                    Inspector.AddError($"Для блока обозначения стороны фасада не найдены монтажные планы.", front.IdBlRef, System.Drawing.SystemIcons.Error);
                    continue;
                }                
                foreach (var item in frontsIntersects)
                {
                    front.AddPanels(item);                    
                }
                front.DefineMinMax();
            }
            return facadeFrontBlocks;
        }

        private void DefineMinMax ()
        {
            XMin = Panels.Min(p => p.ExtTransToModel.MinPoint.X);
            XMax = Panels.Max(p => p.ExtTransToModel.MaxPoint.X);
        }

        private void AddPanels (FloorMounting floor)
        {
            // найти блоки панелей-СБ входящих внутрь границ блока стороны фасада
            var panels = new List<MountingPanel>();
            foreach (var panelSb in floor.RemainingPanels)
            {
                if (Extents.IsPointInBounds(panelSb.ExtTransToModel.MinPoint) &&
                   Extents.IsPointInBounds(panelSb.ExtTransToModel.MaxPoint))
                {
                    panels.Add(panelSb);
                }
            }
            Panels.AddRange(panels);
            floor.RemainingPanels.RemoveAll(p=>panels.Contains(p));
        }
    }
}
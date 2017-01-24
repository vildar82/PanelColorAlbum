using AcadLib;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
using AcadLib.Statistic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles.MountingsPlans
{
    /// <summary>
    /// Автоматическая генерация блоков монтажных планов из рабочих областей
    /// </summary>
    public class AutoGeneratePlans
    {
        Document doc;
        Database db;
        Editor ed;        

        public void AKR_AutoGenerateMountPlans()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;

            PluginStatisticsHelper.AddStatistic();

            // Проверка - не должно быть определено блоков монтажек в файле
            CheckFileHasAnyMountBlock();

            List<Workspace> wsList;
            List<Axis> axisList;
            List<ObjectId> elemList;
            RTree<object> treeElementsInWs;

            // Определение блоков - раб областей, панелей, осей
            DefineBlocks(out wsList, out axisList, out elemList, out treeElementsInWs);

            // Распределение элементов по рабочим областям
            DefineElemInWS(ref wsList, treeElementsInWs);

            // Проверка уникальности рабочих областей
            CheckUniqueWS(wsList);

            // Создание блоков монтажных планов       
            var idsMountFloorsBlRefs = CreateMountingPlanBlocks(wsList);

            // Создание сборок монтажек
            UtilsPlanBlocksTogether.AKR_CollectMountPlansTogether(idsMountFloorsBlRefs);
        }        

        private List<ObjectId> CreateMountingPlanBlocks(List<Workspace> wsList)
        {
            var idsBlRefMount = new List<ObjectId>();
            foreach (var ws in wsList)
            {
                var floorBlockName = BlockPlans.GetFloorBlockName(ws.Floor, ws.Section);
                try
                {
                    var idBlRefMount = BlockPlans.CreateBlock(ws.IdsElementInWS, floorBlockName, ws.AxisPosition);
                    idsBlRefMount.Add(idBlRefMount);
                }
                catch(Exception ex)
                {
                    Inspector.AddError($"Ошибка создания блока монтажного плана '{ws}' : {ex.Message}", ws.Extents, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
                }                
            }
            return idsBlRefMount;
        }

        private void DefineElemInWS(ref List<Workspace> wsList, RTree<object> treeElementsInWs)
        {
            foreach (var ws in wsList)
            {
                ws.IdsElementInWS = new List<ObjectId>();                
                var elems = treeElementsInWs.Intersects(new Rectangle(ws.Extents));

                if (!elems.Any())
                {
                    Inspector.AddError($"Пустая рабочая область '{ws}'", System.Drawing.SystemIcons.Exclamation);
                }
                else
                {
                    bool hasAxis = false;
                    string errSameAxis = string.Empty;

                    foreach (var el in elems)
                    {
                        if (el is Axis)
                        {
                            var axis = el as Axis;
                            ws.AxisPosition = axis.Position;
                            if (hasAxis)
                            {
                                errSameAxis += $"'{axis.BlName}',";
                            }
                            ws.IdsElementInWS.Add(axis.IdBlRef);
                            hasAxis = true;
                        }
                        else
                        {
                            var id = (ObjectId)el;
                            ws.IdsElementInWS.Add(id);
                        }
                    }
                    if (!hasAxis)
                    {
                        Inspector.AddError($"Не найдены оси в рабочей области '{ws}'. Оси должны быть в блоке. : {errSameAxis}.");
                        ws.IdsElementInWS = null;
                    }
                    else if (!string.IsNullOrEmpty(errSameAxis))
                    {
                        Inspector.AddError($"Несколько блоков осей в рабочей области '{ws}' : {errSameAxis}.");
                        ws.IdsElementInWS = null;
                    }
                }
            }
        }

        private void DefineBlocks(out List<Workspace> wsList, out List<Axis> axisList, out List<ObjectId> elemList, 
            out RTree<object> treeElementsInWs) 
        {
            wsList = new List<Workspace>();
            axisList = new List<Axis>();
            elemList = new List<ObjectId>();
            treeElementsInWs = new RTree<object>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = db.CurrentSpaceId.GetObject(OpenMode.ForRead) as BlockTableRecord;

                foreach (var item in ms)
                {
                    if (!item.IsValidEx()) continue;                   

                    var ent = item.GetObject(OpenMode.ForRead);
                                        
                    if (ent is BlockReference)
                    {
                        var blRef = ent as BlockReference;                        
                        var blName = blRef.GetEffectiveName();
                        if (Workspace.IsWorkSpace(blName))
                        {
                            var ws = Workspace.Define(blRef);
                            if (ws != null)
                                wsList.Add(ws);

                        }
                        else if (Axis.IsBlockAxis(blName))
                        {
                            var axis = Axis.Define(blRef, blName);
                            if (axis != null)
                            {
                                axisList.Add(axis);
                                AddElementWsToTree(axis, axis.Bounds.Value, "Блок осей", ref treeElementsInWs);
                            }
                        }
                        else
                        {
                            List<AttributeRefDetail> attrsDet;
                            string mark;
                            string paint;
                            if (MountingPanel.IsMountingPanel(blRef, out mark, out paint, out attrsDet))
                            {
                                elemList.Add(item);
                                AddElementWsToTree(item, blRef.GeometricExtents, "Блок осей", ref treeElementsInWs);
                            }
                        }
                    }
                    else if (ent is DBText)
                    {
                        var text = ent as DBText;
                        if (IsTextScheme(text.TextString))
                        {
                            elemList.Add(item);
                            AddElementWsToTree(item, ent.Bounds.Value, "Блок осей", ref treeElementsInWs);
                        }
                    }
                    else if (ent is MText)
                    {
                        var mt = ent as MText;
                        if (IsTextScheme(mt.Text))
                        {
                            elemList.Add(item);
                            AddElementWsToTree(item, ent.Bounds.Value, "Блок осей", ref treeElementsInWs);
                        }
                    }
                }
                t.Commit();
            }
        }

        private bool IsTextScheme(string text)
        {
            return text.Contains("Схема расположения стеновых панелей", StringComparison.OrdinalIgnoreCase);
        }

        private void AddElementWsToTree(object obj, Extents3d ext, string elemName, ref RTree<object> treeElementsInWs)
        {
            try
            {
                treeElementsInWs.Add(new Rectangle(ext), obj);
            }
            catch
            {
                Inspector.AddError($"Дублирование элементов '{elemName}'", ext, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
            }
        }

        private void CheckFileHasAnyMountBlock()
        {
            using (var bt = db.BlockTableId.Open( OpenMode.ForRead) as BlockTable)
            {
                foreach (var item in bt)
                {
                    using (var btr = item.Open(OpenMode.ForRead) as BlockTableRecord)
                    {
                        if (btr.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName))
                        {
                            throw new Exception("В файле не должно быть определений блоков монтажных планов.");
                        }
                    }
                }
            }
        }

        private void CheckUniqueWS(List<Workspace> wsList)
        {
            var errGroupWs = wsList.GroupBy(g => new { s = g.Section, f = g.Floor }).Where(w=>w.Skip(1).Any());
            bool hasErr = false;
            foreach (var errWsGroup in errGroupWs)
            {
                foreach (var errWs in errWsGroup)
                {
                    Inspector.AddError($"Повторяется рабочая область '{errWs}'", errWs.Extents, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
                }
                hasErr = true;
            }
            if (hasErr)
            {
                throw new Exception("Невозможно создать блоки планов.");
            }
        }
    }
}

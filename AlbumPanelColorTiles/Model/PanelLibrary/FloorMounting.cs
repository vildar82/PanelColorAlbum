using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
    // Этаж - блоки АКР-панелей этажа и связаннный с ним блок монтажки с блоком обозначения стороны фасада
    public class FloorMounting : IComparable<FloorMounting>
    {
        public List<MountingPanel> AllPanelsSbInFloor { get; private set; }
        public string BlRefName { get; private set; }
        public FacadeFrontBlock FacadeFrontBlock { get; private set; }
        public double Height { get; set; }
        public ObjectId IdBlRefMounting { get; private set; }
        public ObjectId IdBtrMounting { get; private set; }
        public Point3d PosBlMounting { get; private set; }
        public Matrix3d Transform { get; private set; }
        public PanelLibraryLoadService LibLoadServ { get; private set; }
        public List<MountingPanel> PanelsSbInFront { get; private set; }
        public Storey Storey { get; private set; }
        public int Section { get; set; }
        public double XMax { get; private set; }
        public double XMin { get; private set; }
        public FacadeMounting Facade { get; set; }
        public Extents3d? Extents { get; set; }
        /// <summary>
        /// Ширина монтажного плана - по границам блока или 33000
        /// </summary>
        public double PlanExtentsHeight { get; set; }
        public double PlanExtentsLength { get; set; }

        public FloorMounting(BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
        {
            IdBtrMounting = blRefMounting.BlockTableRecord;
            PosBlMounting = blRefMounting.Position;
            LibLoadServ = libLoadServ;
            IdBlRefMounting = blRefMounting.Id;
            BlRefName = blRefMounting.Name;
            Transform = blRefMounting.BlockTransform;
            definePlanSection(blRefMounting);

            Extents = blRefMounting.Bounds;       
            if (Extents.HasValue)
            {
                var ext = Extents.Value;
                ext.TransformBy(blRefMounting.BlockTransform.Inverse());
                var h = Extents.Value.MaxPoint.Y - Extents.Value.MinPoint.Y;
                var l = Extents.Value.MaxPoint.X - Extents.Value.MinPoint.X;
                PlanExtentsHeight = ext.MaxPoint.Y - ext.MinPoint.Y;
                PlanExtentsLength = ext.MaxPoint.X - ext.MinPoint.X;
            }
            else
            {
                PlanExtentsHeight = 30000;
                PlanExtentsLength = 50000;
            }            

            //defFloorNameAndNumber(blRefMounting);

            //// добавление блоков паненлей в общий список панелей СБ
            //libLoadServ.AllPanelsSB.AddRange(_allPanelsSbInFloor);
        }

        // мин значение х среди всех границ блоков панелей внктри этажа

        /// <summary>
        /// Поиск всех блоков монтажек в модели в WorkingDatabase
        /// Запускается транзакция
        /// </summary>
        public static List<FloorMounting> GetMountingBlocks(PanelLibraryLoadService libLoadServ)
        {
            List<FloorMounting> floors = new List<FloorMounting>();
            var db = HostApplicationServices.WorkingDatabase;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
                // Найдем все блоки обозначения фасада
                List<FacadeFrontBlock> facadeFrontBlocks = FacadeFrontBlock.GetFacadeFrontBlocks(ms);
                // Дерево прямоугольников от блоков обозначений сторон фасада, для поиска пересечений с
                // блоками монтажек
                RTreeLib.RTree<FacadeFrontBlock> rtreeFront = new RTreeLib.RTree<FacadeFrontBlock>();
                foreach (var front in facadeFrontBlocks)
                {
                    try
                    {
                        rtreeFront.Add(front.RectangleRTree, front);
                    }
                    catch { }
                }

                // Найти блоки монтажек пересекающиеся с блоками обозначения стороны фасада                        
                //// Поиск панелейСБ в Модели и добавление в общий список панелей СБ.
                //libLoadServ.AllPanelsSB.AddRange(PanelSB.GetPanels(ms.Id, Point3d.Origin, Matrix3d.Identity));
                foreach (ObjectId idEnt in ms)
                {
                    var blRefMounting = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRefMounting == null) continue;

                    // Если это блок монтажного плана - имя блока начинается с АКР_Монтажка_
                    if (blRefMounting.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        FloorMounting floor = new FloorMounting(blRefMounting, libLoadServ);
                        floor.GetAllPanels();
                        // найти соотв обозн стороны фасада                              
                        var frontsIntersects = rtreeFront.Intersects(ColorArea.GetRectangleRTree(blRefMounting.GeometricExtents));

                        // если нет пересечений фасадов - пропускаем блок монтажки - он не входит в
                        // фасады, просто так вставлен
                        if (frontsIntersects.Count == 0)
                        {
                            continue;
                        }
                        // если больше одного пересечения с фасадами, то это ошибка, на одной монтажке
                        // должен быть один блок обозначения стороны фасада
                        else if (frontsIntersects.Count > 1)
                        {
                            Inspector.AddError("На монтажном плане не должно быть больше одного блока обозначения фасада.", blRefMounting,
                               icon: System.Drawing.SystemIcons.Error);
                        }
                        else
                        {
                            floor.SetFacadeFrontBlock(frontsIntersects[0]);
                            floors.Add(floor);
                        }
                    }
                }
                t.Commit();
            }
            return floors;
        }

        private void GetAllPanels()
        {
            // Получение всех блоков панелей СБ из блока монтажки
            using (var btr = this.IdBtrMounting.GetObject(OpenMode.ForRead) as BlockTableRecord)
            {
                AllPanelsSbInFloor = MountingPanel.GetPanels(btr, PosBlMounting, Transform, LibLoadServ, this);
            }
            XMin = getXMinFloor();
            XMax = getXMaxFloor();
        }

        public int CompareTo(FloorMounting other)
        {
            return Storey.CompareTo(other.Storey);
        }

        public void DefineStorey(List<Storey> storeysNumbersTypeInAllFacades)
        {
            var indexFloor = BlRefName.IndexOf("эт-");
            string nameStorey = string.Empty;
            if (indexFloor == -1)
                nameStorey = BlRefName.Substring(Settings.Default.BlockPlaneMountingPrefixName.Length);
            else
                nameStorey = BlRefName.Substring(indexFloor + "эт-".Length);
            try
            {
                var storey = new Storey(nameStorey);
                if (storey.Type == EnumStorey.Number)
                {
                    // поиск в общем списке этажей. Номерные этажи всех фасадов должны быть на одном уровне
                    var storeyAllFacades = storeysNumbersTypeInAllFacades.Find(s => s.Number == storey.Number);
                    if (storeyAllFacades == null)
                    {
                        storeysNumbersTypeInAllFacades.Add(storey);
                    }
                    else
                    {
                        storey = storeyAllFacades;
                    }
                    Height = Settings.Default.FacadeFloorHeight;
                }
                Storey = storey;
            }
            catch (Exception ex)
            {
                // ошибка определения номера этажа монтажки - это не чердак (Ч), не парапет (П), и не
                // просто число
                Inspector.AddError(ex.Message + BlRefName, IdBlRefMounting, icon: System.Drawing.SystemIcons.Error);
                Log.Error(ex, "Floor - DefineStorey()");
            }
        }

        private double getXMaxFloor()
        {
            if (AllPanelsSbInFloor.Count == 0) return 0;
            return AllPanelsSbInFloor.Max(p => p.ExtTransToModel.MaxPoint.X);
        }

        private double getXMinFloor()
        {
            if (AllPanelsSbInFloor.Count == 0) return 0;
            return AllPanelsSbInFloor.Min(p => p.ExtTransToModel.MinPoint.X);
        }

        // Добавление стороны фасада в этаж
        private void SetFacadeFrontBlock(FacadeFrontBlock facadeFrontBlock)
        {
            FacadeFrontBlock = facadeFrontBlock;
            PanelsSbInFront = new List<MountingPanel>();

            // найти блоки панелей-СБ входящих внутрь границ блока стороны фасада
            foreach (var panelSb in AllPanelsSbInFloor)
            {                
                if (facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MinPoint) &&
                   facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MaxPoint))
                {
                    PanelsSbInFront.Add(panelSb);
                }
            }
            if (PanelsSbInFront.Count == 0)
            {
                Inspector.AddError($"В блоке обозначения стороны фасада {facadeFrontBlock.BlName} не найдена ни одна панель.",
                   facadeFrontBlock.Extents, facadeFrontBlock.IdBlRef, icon: System.Drawing.SystemIcons.Error);
            }
            else
            {
                XMax = PanelsSbInFront.Max(p => p.ExtTransToModel.MaxPoint.X);
                XMin = PanelsSbInFront.Min(p => p.ExtTransToModel.MinPoint.X);
            }
        }

        private void definePlanSection(BlockReference blRefMountPlan)
        {
            // Номер плана и номер секции
            var val = blRefMountPlan.Name.Substring(Settings.Default.BlockPlaneMountingPrefixName.Length);
            var arrSplit = val.Split('_');
            string numberPart;
            if (arrSplit.Length > 1)
            {
                int section;
                string sectionPart = arrSplit[0].Substring(1);
                if (int.TryParse(sectionPart, out section))
                {
                    Section = section;
                }
                else
                {
                    Inspector.AddError($"Монтажный план {blRefMountPlan.Name}. Не определен номер секции {sectionPart}.", icon: System.Drawing.SystemIcons.Error);
                }
                numberPart = arrSplit[1];
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AcadLib.RTree.SpatialIndex;
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
        public List<FacadeFrontBlock> FacadeFrontBlocks { get; private set; } = new List<FacadeFrontBlock>();
        //public double Height { get; set; }
        public ObjectId IdBlRefMounting { get; private set; }
        public ObjectId IdBtrMounting { get; private set; }
        public Point3d PosBlMounting { get; private set; }
        public Matrix3d Transform { get; private set; }
        public PanelLibraryLoadService LibLoadServ { get; private set; }
        public List<MountingPanel> PanelsSbInFront { get; private set; } = new List<MountingPanel>();
        public Storey Storey { get; private set; }
        public int Section { get; set; }
        public double XMax { get; private set; }
        public double XMin { get; private set; }
        //public FacadeMounting Facade { get; set; }
        public Extents3d Extents { get; set; }
        /// <summary>
        /// Ширина монтажного плана - по границам блока или 33000
        /// </summary>
        public double PlanExtentsHeight { get; set; }
        public double PlanExtentsLength { get; set; }
        public Rectangle RectangleRTree { get; internal set; }
        public List<MountingPanel> RemainingPanels { get; set; }

        public FloorMounting (BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
        {
            IdBtrMounting = blRefMounting.BlockTableRecord;
            PosBlMounting = blRefMounting.Position;
            LibLoadServ = libLoadServ;
            IdBlRefMounting = blRefMounting.Id;
            BlRefName = blRefMounting.Name;
            Transform = blRefMounting.BlockTransform;
            definePlanSection(blRefMounting);

            try
            {
                Extents = blRefMounting.GeometricExtents;
            }
            catch
            {
                string err = $"Ошибка при определении границ монтажного плана - {BlRefName}";
                Inspector.AddError(err, IdBlRefMounting, System.Drawing.SystemIcons.Error);
                throw new Exception(err);
            }

            var ext = Extents;
            ext.TransformBy(blRefMounting.BlockTransform.Inverse());
            var h = Extents.MaxPoint.Y - Extents.MinPoint.Y;
            var l = Extents.MaxPoint.X - Extents.MinPoint.X;
            PlanExtentsHeight = ext.MaxPoint.Y - ext.MinPoint.Y;
            PlanExtentsLength = ext.MaxPoint.X - ext.MinPoint.X;

            RectangleRTree = new Rectangle(Extents.MinPoint.X, Extents.MinPoint.Y, Extents.MaxPoint.X, Extents.MaxPoint.Y, 0, 0);

            //defFloorNameAndNumber(blRefMounting);

            //// добавление блоков паненлей в общий список панелей СБ
            //libLoadServ.AllPanelsSB.AddRange(_allPanelsSbInFloor);
        }

        // мин значение х среди всех границ блоков панелей внктри этажа

        /// <summary>
        /// Поиск всех блоков монтажек в модели в WorkingDatabase
        /// Запускается транзакция
        /// </summary>
        public static List<FloorMounting> GetMountingBlocks(PanelLibraryLoadService libLoadServ, BlockTableRecord ms)
        {
            List<FloorMounting> floors = new List<FloorMounting>();            

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
                    floors.Add(floor);
                }
            }
            return floors;
        }

        private void GetAllPanels()
        {
            // Получение всех блоков панелей СБ из блока монтажки
            using (var btr = this.IdBtrMounting.GetObject(OpenMode.ForRead) as BlockTableRecord)
            {
                AllPanelsSbInFloor = MountingPanel.GetPanels(btr, PosBlMounting, Transform, LibLoadServ, this);
                RemainingPanels = AllPanelsSbInFloor.ToList();
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
                    //Height = Settings.Default.FacadeFloorHeight;
                }
                Storey = storey;
            }
            catch (Exception ex)
            {
                // ошибка определения номера этажа монтажки - это не чердак (Ч), не парапет (П), и не
                // просто число
                Inspector.AddError(ex.Message + BlRefName, IdBlRefMounting, icon: System.Drawing.SystemIcons.Error);
                Logger.Log.Error(ex, "Floor - DefineStorey()");
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
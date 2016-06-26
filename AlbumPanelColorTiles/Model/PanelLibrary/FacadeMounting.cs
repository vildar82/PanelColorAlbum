using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Comparers;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
    // Фасад - это ряд блоков монтажных планов этажей с блоками обозначения стороны плана фасада - составляющие один фасада дома
    public class FacadeMounting
    {
        public FacadeFrontBlock FrontBlock { get; set; }
        public List<MountingPanel> Panels { get; private set; }
        public List<FloorMounting> Floors { get; private set; }
        public Point3d PosPtFrontBlock { get; private set; }
        public double XMax { get; private set; }
        public double XMin { get; private set; }

        public FacadeMounting(FacadeFrontBlock front)
        {
            XMin = front.XMin;
            XMax = front.XMax;
            PosPtFrontBlock = front.Position;
            Panels = front.Panels;
            Floors = Panels.Select(p => p.Floor).GroupBy(g => g).Select(s => s.Key).ToList();
        }

        public static void CreateFacades(List<FacadeMounting> facades)
        {
            // Создание фасадов по монтажным планам
            if (facades.Count == 0) return;

            // Определение высот этажей по загруженным блокам панелей
            DefineStoreyHeight(facades);

            Database db = HostApplicationServices.WorkingDatabase;
            checkLayers(db);
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForWrite) as BlockTableRecord;
                //double yFirstFloor = getFirstFloorY(facades); // Y для первых этажей всех фасадов

                ProgressMeter progress = new ProgressMeter();

                progress.SetLimit(facades.SelectMany(f => f.Panels).Count());
                progress.Start("Создание фасадов");

                foreach (var facade in facades)
                {
                    //double yFloor = yFirstFloor;
                    foreach (var panelSb in facade.Panels)
                    {
                        double yFloor = panelSb.Y;
                        // Подпись номера этажа
                        //captionFloor(facade.XMin, yFloor, panelSb.Floor, ms, t);
                        if (panelSb.PanelAkr != null || panelSb.PanelBase != null)
                        {
                            Point3d ptPanelAkr = new Point3d(panelSb.ExtTransToModel.MinPoint.X, yFloor, 0);
                            //testGeom(panelSb, facade, floor, yFloor, t, ms);
                            ObjectId idBtrPanelAkr;
                            if (panelSb.PanelBase != null)
                            {
                                if (panelSb.PanelBase.IdBtrPanel.IsNull) continue;
                                idBtrPanelAkr = panelSb.PanelBase.IdBtrPanel;
                            }
                            else
                            {
                                idBtrPanelAkr = panelSb.PanelAkr.IdBtrPanelAkrInFacade;
                            }

                            var blRefPanelAkr = new BlockReference(ptPanelAkr, idBtrPanelAkr);
                            blRefPanelAkr.Layer = panelSb.Floor.Storey.Layer;
                            ms.AppendEntity(blRefPanelAkr);
                            t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
                            //blRefPanelAkr.RecordGraphicsModified(true);
                            //blRefPanelAkr.Draw();
                        }
                        progress.MeterProgress();
                    }
                }
                t.Commit();
                progress.Stop();
            }
        }

        private static void DefineStoreyHeight (List<FacadeMounting> facades)
        {
            var yFirstFloor = getFirstFloorY(facades);
            var storeyPanels = facades.SelectMany(s=>s.Panels).GroupBy(g=>g.Floor.Storey).OrderBy(o=>o.Key);
            double curY = yFirstFloor;
            foreach (var storeyPanel in storeyPanels.Where(p=>p.Key.Type == EnumStorey.Number))
            {
                var maxHeight = storeyPanel.Max(p=>p.PanelAkr?.HeightPanelByTile);
                if (maxHeight == null)
                {
                    maxHeight = Settings.Default.FacadeFloorHeight;
                }
                storeyPanel.Key.Height = maxHeight.Value;
                storeyPanel.Key.Y = curY;                
                foreach (var item in storeyPanel)
                {
                    item.Y = curY;
                }
                curY += maxHeight.Value;
            }
            // Определение высот и отметок для парампетных и чержачных панелей
            // группировка панелей по секциям
            var sectionPanels = facades.SelectMany(f=>f.Panels).GroupBy(g=>g.Floor.Section);
            foreach (var secPanel in sectionPanels)
            {
                // отметка высоты секции
                var yPeakPanel = secPanel.Where(w=>w.Floor.Storey.Type == EnumStorey.Number).
                    OrderByDescending(o=>o.Floor.Storey).First();
                curY = yPeakPanel.Y + yPeakPanel.Floor.Storey.Height;
                // панели чердака
                var upperPanels =secPanel.Where(p=>p.Floor.Storey.Type == EnumStorey.Upper);
                if (upperPanels.Any())
                {
                    var maxHeight = upperPanels.Max(p=>p.PanelAkr?.HeightPanelByTile);
                    if (maxHeight == null)
                    {
                        maxHeight = 1900;
                    }
                    foreach (var item in upperPanels)
                    {
                        item.Y = curY;
                    }
                    curY += maxHeight.Value;                        
                }
                // панели парапета
                var parapetPanels =secPanel.Where(p=>p.Floor.Storey.Type == EnumStorey.Parapet);
                if (parapetPanels.Any())
                {                    
                    foreach (var item in parapetPanels)
                    {
                        item.Y = curY;
                    }                    
                }
            }
        }

        public static void DeleteOldAkrPanels(List<FacadeMounting> facades)
        {
            // удаление старых АКР-Панелей фасадов
            Database db = HostApplicationServices.WorkingDatabase;
            // список всех акр панелей в модели
            List<ObjectId> idsBlRefPanelAkr = Panel.GetPanelsBlRefInModel(db);

            ProgressMeter progressMeter = new ProgressMeter();
            progressMeter.SetLimit(idsBlRefPanelAkr.Count);
            progressMeter.Start("Удаление старых фасадов");

            foreach (var idBlRefPanelAkr in idsBlRefPanelAkr)
            {
                using (var blRefPanelAkr = idBlRefPanelAkr.Open(OpenMode.ForRead, false, true) as BlockReference)
                {
                    var extentsAkr = blRefPanelAkr.GeometricExtents;
                    var ptCenterPanelAkr = extentsAkr.Center();
                    // если панель входит в границы любого фасада, то удаляем ее
                    //FacadeMounting facade = facades.Find(f => f.XMin < ptCenterPanelAkr.X && f.XMax > ptCenterPanelAkr.X);
                    //if (facade != null)
                    //{
                        blRefPanelAkr.UpgradeOpen();
                        blRefPanelAkr.Erase();
                    //}
                    progressMeter.MeterProgress();
                }
            }
            progressMeter.Stop();
        }

        /// <summary>
        /// Получение фасадов из блоков монтажных планов и обозначений стороны фасада в чертеже
        /// </summary>
        /// <returns></returns>
        public static List<FacadeMounting> GetFacadesFromMountingPlans(PanelLibraryLoadService libLoadServ = null)
        {
            List<FacadeMounting> facades = new List<FacadeMounting>();
            var db = HostApplicationServices.WorkingDatabase;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead) as BlockTableRecord;
                // Поиск всех блоков монтажных планов в Модели чертежа с соотв обозначением стороны фасада
                List<FloorMounting> floors = FloorMounting.GetMountingBlocks(libLoadServ, ms);
                // блоки сторон фасадов - Фасады
                List<FacadeFrontBlock> facadeFrontBlocks = FacadeFrontBlock.GetFacadeFrontBlocks(ms, floors);

                // Создание фасадов
                foreach (var front in facadeFrontBlocks)
                {
                    FacadeMounting facade = new FacadeMounting (front);
                    facades.Add(facade);
                }
                                
                // определение уровней этажей Storey
                defineFloorStoreys(facades);                
                t.Commit();
            }
            return facades;
        }

        //public void DefYForUpperAndParapetStorey()
        //{
        //    // определение уровней для Ч и П этажей в этом фасаде
        //    // уровеь последнего этажа в фасаде            
        //    var floorsNumberType = Floors.Where(f => f.Storey.Type == EnumStorey.Number);
        //    double yLastNumberFloor = 0;
        //    if (floorsNumberType.Count() > 0)
        //    {
        //        yLastNumberFloor = floorsNumberType.Max(f => f.Storey.Y);
        //    }
        //    // чердак
        //    // double yParapet = 0;
        //    var floorUpper = Floors.Where(f => f.Storey.Type == EnumStorey.Upper).FirstOrDefault();
        //    if (floorUpper != null)
        //    {
        //        //var maxHeightPanel = floorUpper.PanelsSbInFront.Where(p => p.PanelAkr != null)?.Max(p => p.PanelAkr?.HeightPanelByTile);
        //        //if (maxHeightPanel.HasValue)
        //        //{
        //            floorUpper.Storey.Y = yLastNumberFloor + Settings.Default.FacadeFloorHeight;
        //            //yParapet = floorUpper.Storey.Y + maxHeightPanel.Value;
        //            //floorUpper.Height = maxHeightPanel.Value;
        //        //}
        //    }
        //    var floorParapet = Floors.Where(f => f.Storey.Type == EnumStorey.Parapet).FirstOrDefault();
        //    if (floorParapet != null)
        //    {
        //        yParapet = yParapet != 0 ? yParapet : yLastNumberFloor + Settings.Default.FacadeFloorHeight;
        //        floorParapet.Storey.Y = yParapet;
        //        var maxHeightPanel = floorParapet.PanelsSbInFront.Where(p => p.PanelAkr != null)?.Max(p => p.PanelAkr?.HeightPanelByTile);
        //        if (maxHeightPanel.HasValue)
        //        {
        //            floorParapet.Height = maxHeightPanel.Value;
        //        }
        //    }
        //}

        private static void captionFloor(double x, double yFloor, FloorMounting floor, BlockTableRecord ms, Transaction t)
        {
            // Подпись номера этажа
            DBText textFloor = new DBText();
            textFloor.SetDatabaseDefaults(ms.Database);
            textFloor.Annotative = AnnotativeStates.False;
            textFloor.Height = Settings.Default.FacadeCaptionFloorTextHeight;// 250;// FacadeCaptionFloorTextHeight
            textFloor.TextString = floor.Storey.ToString();
            textFloor.Position = new Point3d(x - Settings.Default.FacadeCaptionFloorIndent, yFloor + (floor.Storey.Height * 0.5), 0);
            ms.AppendEntity(textFloor);
            t.AddNewlyCreatedDBObject(textFloor, true);
        }

        private static void checkLayers(Database db)
        {
            // проверкаслоев - если рабочие слои - заблокированны, то разблокировать
            List<string> layersCheck = new List<string>();
            layersCheck.Add(SymbolUtilityServices.LayerZeroName);
            layersCheck.Add(Settings.Default.LayerParapetPanels);
            layersCheck.Add(Settings.Default.LayerUpperStoreyPanels);
            AcadLib.Layers.LayerExt.CheckLayerState(layersCheck.ToArray());
        }

        private static void defineFloorStoreys(List<FacadeMounting> facades)
        {
            // определение уровней этажей Storey
            // этажи с одинаковыми номерами, должны быть на одном уровне во всех фасадах.
            // этажи Ч и П - должны быть последними в этажах одного фасада
            // Определение Storey в фасадах
            List<Storey> storeysAllFacades = new List<Storey>(); // общий список этажей
            facades.ForEach(f =>
            {
                f.DefineFloorStoreys(storeysAllFacades);
                //f.XMax = f.Floors.Max(l => l.XMax);
            });
            // назначение Y для нумеррованных этажей
            storeysAllFacades.Sort();            
        }

        private static double getFirstFloorY(List<FacadeMounting> facades)
        {
            // определение уровня по Y для первого этажа всех фасадов - отступить 10000 вверх от самого верхнего блока панели СБ.
            double maxYblRefPanelInModel = facades.SelectMany(f => f.Floors).Max(f => f.Extents.MaxPoint.Y);
            return maxYblRefPanelInModel + Settings.Default.FacadeIndentFromMountingPlanes;// 10000; // FacadeIndentFromMountingPlanes
        }

        private void checkStoreysFacade()
        {
            // проверка этажей в фасаде.
            // не должно быть одинаковых номеров этажей
            var storeysFacade = Floors.Select(f => f.Storey);
            var storeyNumbersType = storeysFacade.Where(s => s.Type == EnumStorey.Number).ToList();
            var dublicateNumbers = storeyNumbersType.GroupBy(s => s.Number).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dublicateNumbers.Count > 0)
            {
                string nums = string.Join(",", dublicateNumbers);
                Inspector.AddError($"Повторяющиеся номера этажей в фасаде. Координата фасада X = {XMin}. " +
                   $"Повторяющиеся номера этажей определенные по блокам монтажных планов этого фасада {nums}",
                   icon: System.Drawing.SystemIcons.Error);
            }
            // Ч и П могут быть только по одной штуке
            var storeyUpperType = storeysFacade.Where(s => s.Type == EnumStorey.Upper);
            if (storeyUpperType.Count() > 1)
            {
                Inspector.AddError(string.Format(
                   "Не должно быть больше одного этажа Чердака в одном фасаде. Для фасада найдено {0} блоков монтажных планов определенных как чердак. Координата фасада X = {1}.",
                   storeyUpperType.Count(), XMin), icon: System.Drawing.SystemIcons.Error);
            }
            var storeyParapetType = storeysFacade.Where(s => s.Type == EnumStorey.Parapet);
            if (storeyParapetType.Count() > 1)
            {
                Inspector.AddError(string.Format(
                   "Не должно быть больше одного этажа Парапета в одном фасаде. Для фасада найдено {0} блоков монтажных планов определенных как парапет. Координата фасада X = {1}.",
                   storeyParapetType.Count(), XMin), icon: System.Drawing.SystemIcons.Error);
            }
        }

        private void DefineFloorStoreys(List<Storey> storeysNumbersTypeInAllFacades)
        {
            // Определение этажей в этажах фасада.
            List<Storey> storeysFacade = new List<Storey>();            
            foreach (var floor in Floors)
            {
                floor.DefineStorey(storeysNumbersTypeInAllFacades);
            }                        
            // проверка этажей в фасаде.
            //checkStoreysFacade();
        }

        //private static void testGeom(MountingPanel panelSb, Facade facade, Floor floor, double yFloor, Transaction t, BlockTableRecord ms)
        //{
        //   // Точка центра панели СБ
        //   DBPoint ptPanelSbInModel = new DBPoint(panelSb.PtCenterPanelSbInModel);
        //   ms.AppendEntity(ptPanelSbInModel);
        //   t.AddNewlyCreatedDBObject(ptPanelSbInModel, true);
        //   // Точка вставки панели АКР
        //   DBPoint ptPanelArkInModel = new DBPoint(new Point3d(panelSb.GetPtInModel(panelSb.PanelAkrLib).X, yFloor, 0));
        //   ms.AppendEntity(ptPanelArkInModel);
        //   t.AddNewlyCreatedDBObject(ptPanelArkInModel, true);
        //}
    }
}
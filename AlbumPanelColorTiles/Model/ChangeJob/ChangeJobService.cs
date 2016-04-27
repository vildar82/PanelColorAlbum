using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.ChangeJob
{
    /// <summary>
    /// Выполнение задания на изменение покраски в панелях
    /// Для передачи конструкторам Задания на Изменение
    /// </summary>
    public static class ChangeJobService
    {
        public static List<ChangePanel> ChangePanels { get; private set; }
        private static int OffsetMountPlansY = 5000;
        private static int OffsetMountPlansX = 5000;
        public static ObjectId IdTextStylePik;
        public static List<SectionColumn> SecCols { get; set; }
        public static List<FloorRow> FloorRows { get; set; }

        public static void Init()
        {
            ChangePanels = new List<ChangePanel>();
            SecCols = new List<SectionColumn>();
            FloorRows = new List<FloorRow>();
        }

        public static void AddChangePanel (Panels.Panel panelAkr, PanelLibrary.MountingPanel panelMount)
        {
            var chPanel = new ChangePanel(panelAkr, panelMount);
            ChangePanels.Add(chPanel);
        }

        public static void CreateJob()
        {
            if (ChangePanels.Count == 0) return;

            // Показать список панелей с изменившейся маркой.
            // Исключение ошибочныйх панелей.
            ChangePanels.Sort();
            FormChangePanel formChange = new FormChangePanel();
            if (Application.ShowModalDialog(formChange) != System.Windows.Forms.DialogResult.OK)
            {
                // Прервать
                formChange.SetModaless();
                Application.ShowModelessDialog(formChange);
                throw new Exception(AcadLib.General.CanceledByUser);
            }
            else
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                IdTextStylePik = db.GetTextStylePIK();
                
                double totalLen = SecCols.Sum(s => s.LengthMax) + (SecCols.Count-1) * OffsetMountPlansX;
                double tableHeight = ChangePanels.Count * 800 + 2500;
                double totalHeight = FloorRows.Sum(f => f.HeightMax) + (FloorRows.Count-1) * OffsetMountPlansY +tableHeight; 

                // Запрос точки вставки изменений
                // Общая высота планов изменений                        
                var ptRes = ed.GetRectanglePoint(totalLen, totalHeight);// ed.GetPointWCS("Точка вставки планов задания на изменение марок покраски.");
                var ptStart = new Point3d(ptRes.X, ptRes.Y + totalHeight, 0);
                //AcadLib.Jigs.RectangleJig

                SecCols.Sort();
                FloorRows.Sort();

                // Определение X для столбцов секций
                double xPrev = ptStart.X;
                SecCols.First().X = xPrev;
                foreach (var secCol in SecCols.Skip(1))
                {
                    secCol.X = xPrev + OffsetMountPlansX + secCol.LengthMax;
                    xPrev = secCol.X;                 
                }
                // Определение Y для этажей
                double yPrev = ptStart.Y;
                FloorRows.First().Y = yPrev;
                foreach (var fr in FloorRows.Skip(1))
                {
                    fr.Y = yPrev - fr.HeightMax - OffsetMountPlansY;
                    yPrev = fr.Y;
                }

                // группировка измененных панелей по секциям и этажам
                var groupChPanels = ChangePanels.OrderBy(o=>o.FloorRow.Storey).ThenBy(o=>o.SecCol.Section)
                                                .GroupBy(g => new { g.FloorRow, g.SecCol });               

                // Формирование паланов изменений и общей таблицы изменений.
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var ptPlan = ptStart;
                    var ms = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;

                    // подпись секций
                    var heightFirstFloor = FloorRows.First().HeightMax;
                    double yTextSection = ptStart.Y + heightFirstFloor + 2500;
                    foreach (var secCol in SecCols)
                    {                        
                        var ptText = new Point3d(secCol.X + secCol.LengthMax * 0.5, yTextSection, 0);
                        string text = "Секция " + secCol.Section.ToString();
                        addText(ms, t, ptText, text, 1500, TextHorizontalMode.TextCenter);
                    }

                    var ptTextData = new Point3d(ptStart.X + totalLen*0.5,yTextSection+3500, 0);
                    string textData = "Изменения марок покраски от " + DateTime.Now;
                    addText(ms, t, ptTextData, textData, 1500, TextHorizontalMode.TextCenter);

                    var xFloorTexts = ptStart.X;
                    foreach (var group in groupChPanels)
                    {                        
                        ptPlan = new Point3d(group.Key.SecCol.X, group.Key.FloorRow.Y, 0);
                        // изменения для этого монтажного плана
                        var extPlan = createChangePlan(group.ToList(), ptPlan, ms, t);
                        if (extPlan.MinPoint.X < xFloorTexts) xFloorTexts = extPlan.MinPoint.X;
                    }

                    // подпись этажа                    
                    foreach (var fr in FloorRows)
                    {
                        var ptText = new Point3d(xFloorTexts-5000, fr.Y + fr.HeightMax * 0.5, 0);
                        string text = fr.Storey.ToString() + "-этаж";
                        addText(ms, t, ptText, text, 1500, TextHorizontalMode.TextCenter);
                    }

                    // Таблица изменений
                    ptPlan = new Point3d(ptPlan.X, ptPlan.Y - 10000, 0);
                    ChangeJobTable chTable = new ChangeJobTable(ChangePanels, db, ptPlan);
                    chTable.Create(ms, t);

                    t.Commit();
                }
            }
        }

        /// <summary>
        /// исключение панели из изменения
        /// </summary>
        /// <param name="chPanel"></param>
        public static void ExcludePanel(ChangePanel chPanel)
        {
            
        }

        private static Extents3d createChangePlan(List<ChangePanel> chPanels, Point3d ptPlan, BlockTableRecord btr, Transaction t)
        {
            // Вставить блок монтажки
            var blRefFloor = new BlockReference(ptPlan, chPanels.First().PanelMount.Floor.IdBtrMounting);
            btr.AppendEntity(blRefFloor);
            t.AddNewlyCreatedDBObject(blRefFloor, true);

            // Обвести облачком каждую панель с изменившейся покраской
            foreach (var chPanel in chPanels)
            {
                // Границы монт. панели на монт. плане в координатах Модели.
                var extMP = chPanel.ExtMountPanel;
                extMP.TransformBy(blRefFloor.BlockTransform);

                var ptCloudMin = new Point3d(extMP.MinPoint.X+150, extMP.MinPoint.Y - 300, 0);
                var ptCloudMax = new Point3d(extMP.MaxPoint.X-150, extMP.MaxPoint.Y + 300, 0);
                var extCloud = new Extents3d(ptCloudMin, ptCloudMax);

                // Полилиния облака изменения
                var pl = extCloud.GetPolyline();
                var plCloud = getCloudPolyline(pl);
                plCloud.SetDatabaseDefaults();
                plCloud.ColorIndex = 1;
                btr.AppendEntity(plCloud);
                t.AddNewlyCreatedDBObject(plCloud, true);

                // Текст изменения
                MText text = new MText();
                text.SetDatabaseDefaults();
                text.ColorIndex = 1;
                text.TextHeight = 250;
                text.Contents = $"Старая марка покраски: {chPanel.PaintOld}, \n\rНовая марка покраски: {chPanel.PaintNew}";
                text.Location = new Point3d(extCloud.MinPoint.X, extCloud.MinPoint.Y-100, 0);
                btr.AppendEntity(text);
                t.AddNewlyCreatedDBObject(text, true);

                chPanel.PanelMount.SetPaintingToAttr(chPanel.PanelAKR.MarkAr);
            }
            // Разбить
            //blRefFloor.ExplodeToOwnerSpace();
            return blRefFloor.GeometricExtents;
        }

        private static Polyline getCloudPolyline(Polyline pl)
        {
            Polyline plCloud = new Polyline();

            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                var segType = pl.GetSegmentType(i);
                switch (segType)
                {
                    case SegmentType.Line:
                        addCloudLineSegment(plCloud, pl.GetLineSegment2dAt(i));
                        break;
                    case SegmentType.Arc:
                        break;
                    case SegmentType.Coincident:
                        break;
                    case SegmentType.Point:
                        break;
                    case SegmentType.Empty:
                        break;
                    default:
                        break;
                }
            }
            return plCloud;
        }

        private static void addCloudLineSegment(Polyline plCloud, LineSegment2d lineSeg)
        {               
            var lenCur = 0d;
            var ptCur = lineSeg.StartPoint;
            while (lenCur< lineSeg.Length)
            {
                if ((lineSeg.Length-lenCur) <100)                
                    ptCur = ptCur + lineSeg.Direction * (lineSeg.Length-lenCur);
                else
                    ptCur = ptCur + lineSeg.Direction * 100;
                plCloud.AddVertexAt(plCloud.NumberOfVertices, ptCur, -1, 0, 0);
                lenCur +=100;
            }
        }

        private static DBText addText(BlockTableRecord btr, Transaction t, Point3d pt, string value, double height,
            TextHorizontalMode horMode = TextHorizontalMode.TextCenter)
        {
            // Подпись развертки - номер вида
            DBText text = new DBText();
            text.SetDatabaseDefaults();
            text.Height = height;
            text.TextStyleId = IdTextStylePik;
            text.TextString = value;
            if (horMode == TextHorizontalMode.TextLeft)
            {
                text.Position = pt;
            }
            else
            {
                text.HorizontalMode = horMode;
                text.AlignmentPoint = pt;
                text.AdjustAlignment(btr.Database);
            }
            btr.AppendEntity(text);
            t.AddNewlyCreatedDBObject(text, true);
            return text;
        }
    }
}

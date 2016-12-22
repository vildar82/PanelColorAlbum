using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;

namespace AlbumPanelColorTiles.Utils.ColorAreaTable
{
    public class UtilsColorAreasTible
    {
        public Document Doc { get; set; }
        public Database Db { get; set; }
        public Editor Ed { get; set; }

        public UtilsColorAreasTible (Document doc)
        {
            Doc = doc;
            Db = doc.Database;
            Ed = doc.Editor;
        }

        public void CreateTable ()
        {
            // Выбор блоков
            var sel = Ed.SelectBlRefs("\nВыбор блоков:");

            AcadLib.Blocks.Dublicate.CheckDublicateBlocks.Check(sel);           
            var areas = FindColorAreas(sel);
                
            if (areas.Count == 0)
            {
                Inspector.AddError($"Блоки зон покраски не найдены. Блок должен называться - {Settings.Default.BlockColorAreaName}");
                return;
            }

            var data = new DataColorAreas(areas);

            // Таблица
            var tableService = new ColorAreasTable(data, Db);
            tableService.CalcRows();
            var table = tableService.Create();
            tableService.Insert(table, Doc);
        }

        private List<ColorArea> FindColorAreas (IEnumerable sel)
        {
            List<ColorArea> areas = new List<ColorArea>();
            using (var t = Db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId idEnt in sel)
                {
                    if (!idEnt.IsValidEx()) continue;
                    var blRef = idEnt.GetObject(OpenMode.ForRead) as BlockReference;
                    if (blRef == null) continue;

                    string blName = blRef.GetEffectiveName();

                    if (blName == Settings.Default.BlockColorAreaName)
                    {
                        var area = new ColorArea(blRef, blName);
                        areas.Add(area);
                    }
                }
                t.Commit();
            }
            return areas;
        }
    }
}

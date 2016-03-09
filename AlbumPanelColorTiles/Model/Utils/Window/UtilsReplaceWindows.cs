using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Utils.Window;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils
{
    public class UtilsReplaceWindows
    {
        Document doc;
        Editor ed;
        public static Database db { get; private set; }
        ObjectId IdBtrWindow;
        string LayerWindow = "АР_Окна";

        public void Redefine()
        {
            // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
            doc = Application.DocumentManager.MdiActiveDocument;
            ed = doc.Editor;
            db = doc.Database;            

            using (var t = db.TransactionManager.StartTransaction())
            {
                // Блоки панелей            
                List<ObjectId> idsBtrPanels = getPanelsBtr();

                var translatorWindows = getTranslatorWindows();

                foreach (var idBtrPanel in idsBtrPanels)
                {
                    var btrPanel = idBtrPanel.GetObject(OpenMode.ForRead) as BlockTableRecord;
                    foreach (ObjectId idEnt in btrPanel)
                    {
                        var blRefWindowOld = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                        if (blRefWindowOld == null) continue;
                        WindowTranslator translatorW;
                        string blName = blRefWindowOld.GetEffectiveName();
                        
                        if (blName.Equals(Settings.Default.BlockWindowName, StringComparison.OrdinalIgnoreCase))
                        {
                            translatorW = WindowTranslator.GetAkrBlWinTranslator(blRefWindowOld);
                        }

                        if (translatorWindows.TryGetValue(blName, out translatorW))
                        {
                            //WindowRedefine winRedefine = new WindowRedefine(blRefWindowOld, translatorW);
                        }
                    }
                }
                t.Commit();
            }
        }

        private void replaceWindows(BlockTableRecord btrPanel, BlockReference blRefWindowOld, WindowTranslator translatorW)
        {
            var extOldWind = blRefWindowOld.GeometricExtentsСlean();
            var newBlRefW = new BlockReference(extOldWind.MinPoint, IdBtrWindow);
            newBlRefW.SetDatabaseDefaults(db);
            newBlRefW.Layer = LayerWindow;

            btrPanel.UpgradeOpen();
            btrPanel.AppendEntity(newBlRefW);
            btrPanel.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(newBlRefW, true);

            //setDynProp(newBlRefW, translatorW);

            blRefWindowOld.UpgradeOpen();
            blRefWindowOld.Erase();
        }

        

        private Dictionary<string, WindowTranslator> getTranslatorWindows()
        {
            Dictionary<string, WindowTranslator> translator = new Dictionary<string, WindowTranslator>();            
            
            // ОП-2
            translator.Add("Окно_ОП-2", new WindowTranslator("Окно_ОП-2", "ОП-2"));            
            translator.Add("ОП-2п 900х1800", new WindowTranslator("ОП-2п 900х1800", "ОП-2"));
            translator.Add("ОП-2 (КБЕ) 950-1790 ПО ВК", new WindowTranslator("ОП-2 (КБЕ) 950-1790 ПО ВК", "ОП-2"));
            translator.Add("ОП-2л (КБЕ) 950-1790 ПО ВК", new WindowTranslator("ОП-2л (КБЕ) 950-1790 ПО ВК", "ОП-2л"));
            translator.Add("ок2 внешний вид", new WindowTranslator("ок2 внешний вид", "ОП-2л"));
            translator.Add("Окно_ОП-2_Л", new WindowTranslator("Окно_ОП-2_Л", "ОП-2л"));
            translator.Add("ОП-2л 900х1800", new WindowTranslator("ОП-2л 900х1800", "ОП-2л"));            
            // ОП-3
            translator.Add("ОП-3 900х1800", new WindowTranslator("ОП-3 900х1800", "ОП-3"));
            translator.Add("ОП-3 (КБЕ) 950-1790 ГЛ", new WindowTranslator("ОП-3 (КБЕ) 950-1790 ГЛ", "ОП-3"));
            // ОП-4
            translator.Add("ок-4 внешний вид", new WindowTranslator("ок-4 внешний вид", "ОП-4"));
            translator.Add("Окно_ОП-4", new WindowTranslator("Окно_ОП-4", "ОП-4"));
            translator.Add("ОП-4(КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4(КБЕ) 1250-1790 П ВК", "ОП-4"));
            translator.Add("ОП-4 (КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4 (КБЕ) 1250-1790 П ВК", "ОП-4"));
            translator.Add("ОП-4 1200х1800", new WindowTranslator("ОП-4 1200х1800", "ОП-4"));
            translator.Add("ОП-4л(КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4л(КБЕ) 1250-1790 П ВК", "ОП-4л"));
            translator.Add("ОП-4л (КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4л (КБЕ) 1250-1790 П ВК", "ОП-4л"));
            // ОП-5
            translator.Add("ОП-5 (КБЕ) 650-1790 ПО ВК", new WindowTranslator("ОП-5 (КБЕ) 650-1790 ПО ВК", "ОП-5"));
            translator.Add("ок 5 внешний вид", new WindowTranslator("ок 5 внешний вид", "ОП-5л"));
            translator.Add("Окно_ОП-5_Л", new WindowTranslator("Окно_ОП-5_Л", "ОП-5л"));
            translator.Add("ОП-5л (КБЕ) 650-1790 ПО ВК", new WindowTranslator("ОП-5л (КБЕ) 650-1790 ПО ВК", "ОП-5л"));
            translator.Add("ОП-5л 600х1800", new WindowTranslator("ОП-5л 600х1800", "ОП-5л"));
            // ОП-6
            translator.Add("ОП-6п 1500х1800", new WindowTranslator("ОП-6п 1500х1800", "ОП-6"));
            translator.Add("ОП-6 (КБЕ) 1550-1790 ПО ВК", new WindowTranslator("ОП-6 (КБЕ) 1550-1790 ПО ВК", "ОП-6"));
            translator.Add("ОП-6л", new WindowTranslator("ОП-6л", "ОП-6л"));
            translator.Add("ОП-6л 1500х1800", new WindowTranslator("ОП-6л 1500х1800", "ОП-6л"));
            // ОП-7
            translator.Add("ОП-7 (КБЕ) 1850-1790 ГЛ", new WindowTranslator("ОП-7 (КБЕ) 1850-1790 ГЛ", "ОП-7"));                        
            // ОП-11
            translator.Add("АР_Окно_ОП-11", new WindowTranslator("АР_Окно_ОП-11", "ОП-11"));
            translator.Add("ОП-11_фасад", new WindowTranslator("ОП-11_фасад", "ОП-11"));
            // ОП-14
            translator.Add("Окно_ОП-14", new WindowTranslator("Окно_ОП-14", "ОП-14"));
            translator.Add("ОП-14 2100х1800", new WindowTranslator("ОП-14 2100х1800", "ОП-14"));            
            translator.Add("ок 1 вид с фасада", new WindowTranslator("ок 1 вид с фасада", "ОП-14"));
            // ОП-15
            translator.Add("ОП-15 (КБЕ) 1550-1790 ПО ВК", new WindowTranslator("ОП-15 (КБЕ) 1550-1790 ПО ВК", "ОП-15"));
            translator.Add("ОП-15 1500х1800", new WindowTranslator("ОП-15 1500х1800", "ОП-15"));
            // ОП-16
            translator.Add("ОП-16 1500х1800", new WindowTranslator("ОП-16 1500х1800", "ОП-16"));
            // ОП-17
            translator.Add("АР_Окно_ОП-17", new WindowTranslator("АР_Окно_ОП-17", "ОП-17"));
            translator.Add("ОП-17 600х1800", new WindowTranslator("ОП-17 600х1800", "ОП-17"));
            // ОП-24
            translator.Add("АР_Окно_ОП-24", new WindowTranslator("АР_Окно_ОП-24", "ОП-24"));
            // БП-1
            translator.Add("Окно_БП-1", new WindowTranslator("Окно_БП-1", "БП-1"));
            translator.Add("бп-1", new WindowTranslator("бп-1", "БП-1"));
            // ОБД-1
            translator.Add("Окно_ОБД-1", new WindowTranslator("Окно_ОБД-1", "ОБД-1"));            

            return translator;
        }

        private List<ObjectId> getPanelsBtr()
        {
            List<ObjectId> idsBtrPanels = new List<ObjectId>();
            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            IdBtrWindow = bt["АКР_Окно"];
            foreach (ObjectId idBtr in bt)
            {
                var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                if (btr.Name.StartsWith(Settings.Default.BlockPanelAkrPrefixName))
                {
                    idsBtrPanels.Add(idBtr);
                }
            }
            return idsBtrPanels;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Lib;
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
        public static Database Db { get; private set; }        
        public static Transaction Transaction { get; private set; }        

        Document doc;
        Editor ed;                
        
        private static Dictionary<string, WindowTranslator> translatorWindows;

        public void Redefine()
        {            
            doc = Application.DocumentManager.MdiActiveDocument;
            ed = doc.Editor;
            Db = doc.Database;            

            using (Transaction = Db.TransactionManager.StartTransaction())
            {
                WindowRedefine.Init();

                // Блоки панелей АКР            
                List<ObjectId> idsBtrPanels = getPanelsBtr();

                // Транслятор имен блоков окон в параметр марки блока АКР_Окно
                if (translatorWindows == null)
                {
                    translatorWindows = WindowTranslator.GetTranslatorWindows();
                }

                // Сбор блоков окон для замены.
                List<WindowRedefine> redefines = getRedefineBlocks(idsBtrPanels);

                // Переопределение блоков окон из библиотеки
                string fileAkrWinBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAkrWindows);
                AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(
                    n => n.StartsWith(Settings.Default.BlockWindowName), fileAkrWinBlocksTemplate, Db, DuplicateRecordCloning.Replace);

                // Проверка наличия слоя для вставки окон.
                AcadLib.Layers.LayerExt.CheckLayerState(Settings.Default.LayerWindows);                

                // Замена блоков окон
                foreach (var redefine in redefines)
                {
                    try
                    {
                        redefine.Replace();
                    }
                    catch (Exception ex)
                    {
                        Inspector.AddError($"Не удалось обновить блок {redefine.BlNameOld} - {ex.Message}", System.Drawing.SystemIcons.Error);
                    }                    
                }               
                Transaction.Commit();
            }
        }        

        private List<ObjectId> getPanelsBtr()
        {
            List<ObjectId> idsBtrPanels = new List<ObjectId>();
            var bt = Db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;            
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

        private List<WindowRedefine> getRedefineBlocks(List<ObjectId> idsBtrPanels)
        {
            List<WindowRedefine> redefines = new List<WindowRedefine>();
            foreach (var idBtrPanel in idsBtrPanels)
            {
                var btrPanel = idBtrPanel.GetObject(OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId idEnt in btrPanel)
                {
                    var blRefWindowOld = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRefWindowOld == null) continue;

                    WindowTranslator translatorW;
                    string blName = blRefWindowOld.GetEffectiveName();
                    bool isAkrBlWin = false;

                    if (blName.Equals(Settings.Default.BlockWindowName, StringComparison.OrdinalIgnoreCase))
                    {
                        translatorW = WindowTranslator.GetAkrBlWinTranslator(blRefWindowOld);
                        isAkrBlWin = true;
                    }
                    else
                    {
                        translatorWindows.TryGetValue(blName, out translatorW);
                    }

                    if (translatorW != null)
                    {
                        WindowRedefine winRedefine = new WindowRedefine(isAkrBlWin, blRefWindowOld, translatorW);
                        redefines.Add(winRedefine);
                    }
                }
            }
            return redefines;
        }
    }
}

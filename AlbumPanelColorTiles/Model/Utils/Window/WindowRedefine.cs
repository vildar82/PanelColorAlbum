using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Base;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils.Window
{
    public class WindowRedefine
    {
        private static Dictionary<string, ObjectIdCollection> dictIdBlRefAkrWindowMarks;

        public bool IsAkrBlockWindow { get; set; }
        public string BlNameOld { get; set; }
        public ObjectId IdBtrOwner { get; set; }
        public ObjectId IdBlRef { get; set; }
        public Point3d Position { get; set; }
        public WindowTranslator TranslatorW { get; set; }

        public WindowRedefine (bool isAkrBlWin, BlockReference blRefWinOld, WindowTranslator translatorW)
        {
            IsAkrBlockWindow = isAkrBlWin;
            IdBlRef = blRefWinOld.Id;
            TranslatorW = translatorW;
            IdBtrOwner = blRefWinOld.OwnerId;
            if (IsAkrBlockWindow)
            {
                Position = blRefWinOld.Position;
            }
            else
            {
                var extOldWind = blRefWinOld.GeometricExtentsСlean();
                Position = extOldWind.MinPoint;
            }
        }        

        /// <summary>
        /// Получение определений блоков для всех марок окон в динамическом блоке
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ObjectIdCollection> GetBlRefAkrWindowMarks()
        {
            var idBlRefAkrWindowMarks = new Dictionary<string, ObjectIdCollection>();
            // Получение определения блока для каждой марки
            Database db = UtilsReplaceWindows.Db;
            using (var bt = db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
            {
                // Исходный блок окна
                ObjectId idBtrWindow = bt["АКР_Окно"];                
                using (var ms = bt[BlockTableRecord.ModelSpace].Open(OpenMode.ForWrite) as BlockTableRecord)
                {
                    BlockReference blRefWin = new BlockReference(Point3d.Origin, idBtrWindow);
                    blRefWin.SetDatabaseDefaults();

                    List<string> marks = null;                    
                    foreach (DynamicBlockReferenceProperty param in blRefWin.DynamicBlockReferencePropertyCollection)
                    {
                        if (param.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                        {                            
                            marks = param.GetAllowedValues().Cast<string>().ToList();
                            break;
                        }
                    }

                    List<BlockReference> blRefWins = new List<BlockReference>();
                    if (marks != null)
                    {                        
                        foreach (string mark in marks)
                        {
                            using (blRefWin = new BlockReference(Point3d.Origin, idBtrWindow))
                            {
                                blRefWin.SetDatabaseDefaults();
                                blRefWin.Layer = UtilsReplaceWindows.LayerWindow;
                                blRefWins.Add(blRefWin);

                                ms.AppendEntity(blRefWin);

                                foreach (DynamicBlockReferenceProperty param in blRefWin.DynamicBlockReferencePropertyCollection)
                                {
                                    if (param.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        param.Value = mark;
                                        break;
                                    }
                                }

                                ObjectIdCollection idCol = new ObjectIdCollection();
                                idCol.Add(blRefWin.Id);
                                idBlRefAkrWindowMarks.Add(mark, idCol);
                            }
                        }                        
                    }
                }
            }
            return idBlRefAkrWindowMarks;
        }

        public static void EraseTemplateBlRefsAkrWin()
        {
            if (dictIdBlRefAkrWindowMarks != null)
            {
                foreach (var item in dictIdBlRefAkrWindowMarks)
                {
                    var blRefTemp = item.Value[0].GetObject(OpenMode.ForWrite, false, true) as BlockReference;
                    blRefTemp.Erase();
                }
            }
        }

        public void Replace()
        {
            var blRefOldWin = IdBlRef.GetObject(OpenMode.ForWrite, false, true) as BlockReference;
            
            //if (IsAkrBlockWindow)
            //{
            //    // Не заменять блок, а просто проверить видимость
            //    setDynProp(blRefOldWin, TranslatorW);
            //}
            //else
            //{
                ObjectIdCollection idColBlRefAkrWindow = getBlRefAkrWindow(TranslatorW.Mark);
                IdMapping map = new IdMapping();
                UtilsReplaceWindows.Db.DeepCloneObjects(idColBlRefAkrWindow,IdBtrOwner, map, false);
                ObjectId idBlRefCopy = map[idColBlRefAkrWindow[0]].Value;

                var blRefNew = idBlRefCopy.GetObject(OpenMode.ForWrite, false, true) as BlockReference;
                blRefNew.Position = Position;
                                
                blRefOldWin.Erase();
            //}
        }

        private ObjectIdCollection getBlRefAkrWindow(string mark)
        {
            ObjectIdCollection res;
            if (dictIdBlRefAkrWindowMarks == null)
            {
                dictIdBlRefAkrWindowMarks = GetBlRefAkrWindowMarks();
            }            
            if (!dictIdBlRefAkrWindowMarks.TryGetValue(mark, out res))
            {
                throw new Exception($"Не найдена марка {mark}");
            }            
            return res;
        }

        private static void setDynProp(BlockReference blRefW, WindowTranslator translatorW)
        {
            foreach (DynamicBlockReferenceProperty prop in blRefW.DynamicBlockReferencePropertyCollection)
            {
                if (prop.PropertyName.Equals("Видимость", StringComparison.OrdinalIgnoreCase) &&
                    !prop.Value.ToString().Equals(translatorW.Mark, StringComparison.OrdinalIgnoreCase))
                {
                    prop.Value = translatorW.Mark;
                    break;
                }
            }
        }
    }
}

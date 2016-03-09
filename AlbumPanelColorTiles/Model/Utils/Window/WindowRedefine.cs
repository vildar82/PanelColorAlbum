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
        public static Dictionary<string, ObjectId> _idBtrWindowMarks;
        public string BlNameOld { get; set; }
        public string Mark { get; set; }
        public ObjectId IdBtrOwner { get; set; }
        public ObjectId IdBlRef { get; set; }
        public Point3d Position { get; set; }

        public WindowRedefine(string oldBlName, string mark)
        {
            BlNameOld = oldBlName;
            Mark = mark;            
        }

        /// <summary>
        /// Получение определений блоков для всех марок окон в динамическом блоке
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ObjectId> GetBtrMarks()
        {
            var idBtrWindowMarks = new Dictionary<string, ObjectId>();
            // Получение определения блока для каждой марки
            Database db = UtilsReplaceWindows.db;
            using (var bt = db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
            {
                // Исходный блок окна
                ObjectId idBtrWindow = bt["АКР_Окно"];                
                using (var ms = bt[BlockTableRecord.ModelSpace].Open(OpenMode.ForWrite) as BlockTableRecord)
                {
                    BlockReference blRefWin = new BlockReference(Point3d.Origin, idBtrWindow);
                    blRefWin.SetDatabaseDefaults();

                    List<string> marks = null;
                    DynamicBlockReferenceProperty paramvisibility = null;
                    foreach (DynamicBlockReferenceProperty param in blRefWin.DynamicBlockReferencePropertyCollection)
                    {
                        if (param.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                        {
                            paramvisibility = param;
                            marks = param.GetAllowedValues().Cast<string>().ToList();
                            break;
                        }
                    }

                    if (marks != null && paramvisibility != null)
                    {
                        ms.AppendEntity(blRefWin);
                        foreach (string mark in marks)
                        {
                            paramvisibility.Value = mark;
                            idBtrWindowMarks.Add(mark, blRefWin.BlockTableRecord);
                        }
                        blRefWin.Erase();
                    }
                }
            }
            return idBtrWindowMarks;
        }

        private static void setDynProp(BlockReference newBlRefW, WindowTranslator translatorW)
        {
            foreach (DynamicBlockReferenceProperty prop in newBlRefW.DynamicBlockReferencePropertyCollection)
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

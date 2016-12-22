using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;

namespace AlbumPanelColorTiles.Utils
{
    /// <summary>
    /// Собрать блоки в одну точку
    /// </summary>
    public static class UtilsPlanBlocksTogether
    {
        /// <summary>
        /// Блоки для сборки вместе
        /// </summary>
        public static ObjectId[] IdBlRefs { get; set; }
        public static void Together()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Выбор блоков
            if(IdBlRefs == null)
            {
                IdBlRefs = ed.SelectBlRefs("\nВыбор блоков:").ToArray();
            }
            // Точка соединения всех блоков (точка вставки).
            var ptInsert = ed.GetPointWCS("\nТочка вставки:");

            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                foreach (var idBlRef in IdBlRefs)
                {
                    if (!idBlRef.IsValidEx()) continue;
                    var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;
                    if (blRef.Name.StartsWith(Settings.Default.BlockPlaneMountingPrefixName))
                    {
                        // вставка нового вхождения этого блока                    
                        var blRefNew = new BlockReference(ptInsert, blRef.BlockTableRecord);
                        cs.AppendEntity(blRefNew);
                        t.AddNewlyCreatedDBObject(blRefNew, true);
                    }                    
                }
                t.Commit();
            }
            IdBlRefs = null;                
        }
    }
}

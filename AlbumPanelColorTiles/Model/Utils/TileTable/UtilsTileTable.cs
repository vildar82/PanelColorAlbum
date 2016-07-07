using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Utils.TileTable
{
    /// <summary>
    /// Спецификация плитки - отдельный подсчет от создания альбома
    /// </summary>
    public class UtilsTileTable
    {
        public Document Doc { get; set; }
        public Database Db { get; set; }
        public Editor Ed { get; set; }

        public UtilsTileTable (Document doc)
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

            using (var t = Db.TransactionManager.StartTransaction())
            {
                foreach (var idEnt in sel)
                {
                    var blRef = idEnt.GetObject( OpenMode.ForRead) as BlockReference;
                    if (blRef == null) continue;

                    string blName = blRef.GetEffectiveName();

                    if (blName.StartsWith(Settings.Default.BlockTileName, StringComparison.OrdinalIgnoreCase))
                    {
                        
                    }
                }
            }
        }
    }
}

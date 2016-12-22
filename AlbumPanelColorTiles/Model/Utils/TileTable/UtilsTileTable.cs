using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadLib;

namespace AlbumPanelColorTiles.Utils.TileTable
{
    /// <summary>
    /// Спецификация плитки - отдельный подсчет от создания альбома
    /// </summary>
    public class UtilsTileTable
    {
        private Dictionary<ObjectId, List<Tile>> dictBtrTiles = new Dictionary<ObjectId, List<Tile>> ();
        private TileData data = new TileData ();

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
                data.Tiles = FindTiles(sel, null);
                t.Commit();
            }
            if (data.Tiles.Count == 0)
            {
                Ed.WriteMessage("\nБлоки плитки не найдены.");
                return;
            }

            // Группировка плиток
            data.Calc();

            // Таблица
            TileTable tableService = new TileTable (Db, data);
            tableService.CalcRows();
            var table = tableService.Create();
            tableService.Insert(table, Doc);
        }

        private List<Tile> FindTiles (IEnumerable sel, string ownerBtrName)
        {
            List<Tile> tiles = new List<Tile> ();
            List<ObjectId> idsBtr = new List<ObjectId> ();

            foreach (ObjectId idEnt in sel)
            {
                if (!idEnt.IsValidEx()) continue;
                var blRef = idEnt.GetObject( OpenMode.ForRead) as BlockReference;
                if (blRef == null) continue;

                string blName = blRef.GetEffectiveName();

                if (blName.StartsWith(Settings.Default.BlockTileName, StringComparison.OrdinalIgnoreCase))
                {
                    var color = data.GetColor(blRef.LayerId);
                    Tile tile = new Tile(blRef, blName, ownerBtrName, color);
                    tiles.Add(tile);
                }
                else
                {
                    idsBtr.Add(blRef.BlockTableRecord);
                }
            }

            foreach (var idBtr in idsBtr)
            {
                List<Tile> tilesInBtr;
                if (!dictBtrTiles.TryGetValue(idBtr, out tilesInBtr))
                {
                    var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;                    
                    tilesInBtr = FindTiles(btr, btr.Name);
                    dictBtrTiles.Add(btr.Id, tilesInBtr);
                }
                tiles.AddRange(tilesInBtr);
            }
            return tiles;
        }
    }
}

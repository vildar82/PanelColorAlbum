using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Base;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib;

namespace AlbumPanelColorTiles.Utils.Windows
{
    public class WindowRedefine
    {
        private static Dictionary<string, ObjectId> dictIdBtrAkrWindow;
        
        public string BlNameOld { get; set; }
        public ObjectId IdBtrOwner { get; set; }
        public ObjectId IdBlRef { get; set; }
        public Point3d Position { get; set; }
        public WindowTranslator TranslatorW { get; set; }

        public WindowRedefine (bool isAkrBlWin, BlockReference blRefWinOld, WindowTranslator translatorW)
        {            
            IdBlRef = blRefWinOld.Id;
            TranslatorW = translatorW;
            IdBtrOwner = blRefWinOld.OwnerId;
            if (isAkrBlWin)
            {
                Position = blRefWinOld.Position;
            }
            else
            {
                var extOldWind = blRefWinOld.GeometricExtentsСlean();
                Position = extOldWind.MinPoint;
            }
        }

        public static void Init()
        {
            dictIdBtrAkrWindow = null;
        }                

        public void Replace()
        {
            ObjectId idBtrWin;
            if (dictIdBtrAkrWindow == null)
            {
                dictIdBtrAkrWindow = getBtrAkrWin(UtilsReplaceWindows.Db);
            }
            dictIdBtrAkrWindow.TryGetValue(TranslatorW.BlNameNew, out idBtrWin);

            if (idBtrWin.IsNull)
            {
                throw new Exception($"Ошибка, не найден блок окна {TranslatorW.BlNameNew}.");
            }

            // Создание вхождения нового блока окнак
            var blRefNew = new BlockReference(Position, idBtrWin);
            blRefNew.SetDatabaseDefaults();
            blRefNew.Layer = Settings.Default.LayerWindows;
            // добавление его в определение блок
            var btrOwner = IdBtrOwner.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            btrOwner.AppendEntity(blRefNew);
            UtilsReplaceWindows.Transaction.AddNewlyCreatedDBObject(blRefNew, true);
            // Удаление старого блока.
            var blRefOldWin = IdBlRef.GetObject(OpenMode.ForWrite, false, true) as BlockReference;
            blRefOldWin.Erase();            
        }

        private Dictionary<string, ObjectId> getBtrAkrWin(Database db)
        {
            Dictionary<string, ObjectId> dictRes = new Dictionary<string, ObjectId>();
            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
            foreach (var idBtr in bt)
            {
                if (!idBtr.IsValidEx()) continue;
                var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
                if (btr.Name.StartsWith(Settings.Default.BlockWindowName))
                {
                    dictRes.Add(btr.Name, btr.Id);
                }
            }
            return dictRes;
        }
    }
}

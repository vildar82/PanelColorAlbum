using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
    /// <summary>
    /// Экспортная панель
    /// </summary>
    public class PanelBtrExport
    {
        private Extents3d _extentsByTile;

        public string BlName { get; private set; }
        public ObjectId CaptionLayerId { get; private set; }
        public string CaptionMarkSb { get; private set; }
        public string CaptionPaint { get; private set; }
        public ConvertPanelService CPS { get; private set; }

        // Объекты линий, полилиний в блоке панели (на одном уровне вложенности в блок)
        public List<EntityInfo> EntInfos { get; set; }

        public string ErrMsg { get; private set; }

        public Extents3d ExtentsByTile { get { return _extentsByTile; } }

        public Extents3d ExtentsNoEnd { get; set; }

        public double HeightByTile { get; private set; }

        /// <summary>
        /// Определение блока панели в файле АКР
        /// </summary>
        public ObjectId IdBtrAkr { get; set; }

        /// <summary>
        /// Определение блока панели в экспортированном файле
        /// </summary>
        public ObjectId IdBtrExport { get; set; }

        public ObjectId IdCaptionMarkSb { get; set; }
        public ObjectId IdCaptionPaint { get; set; }
        public List<ObjectId> IdsEndsBottomEntity { get; set; }

        // Объекты торцов панели
        public List<ObjectId> IdsEndsLeftEntity { get; set; }

        public List<ObjectId> IdsEndsRightEntity { get; set; }
        public List<ObjectId> IdsEndsTopEntity { get; set; }
        public List<PanelBlRefExport> Panels { get; private set; }
        public List<Extents3d> Tiles { get; private set; }

        public PanelBtrExport(ObjectId idBtrAkr, ConvertPanelService cps)
        {
            CPS = cps;
            IdBtrAkr = idBtrAkr;
            Panels = new List<PanelBlRefExport>();
            IdsEndsLeftEntity = new List<ObjectId>();
            IdsEndsRightEntity = new List<ObjectId>();
            IdsEndsTopEntity = new List<ObjectId>();
            IdsEndsBottomEntity = new List<ObjectId>();
            EntInfos = new List<EntityInfo>();
            Tiles = new List<Extents3d>();
        }

        public void ConvertBtr()
        {
            using (var btr = IdBtrExport.GetObject(OpenMode.ForWrite) as BlockTableRecord)
            {
                // Итерация по объектам в блоке и выполнение различных операций к элементам
                iterateEntInBlock(btr);

                // Контур панели (так же определяется граница панели без торцов)
                ContourPanel contourPanel = new ContourPanel(this);
                contourPanel.CreateContour2(btr);

                // Определение торцевых объектов (плитки и полилинии контура торца)
                defineEnds();

                // Удаление объектов торцов из блока панели, если это ОЛ
                if (CaptionMarkSb.StartsWith("ОЛ", StringComparison.CurrentCultureIgnoreCase))
                {
                    deleteEnds(IdsEndsTopEntity);
                    IdsEndsTopEntity = new List<ObjectId>();
                }

                // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
                ConvertCaption caption = new ConvertCaption(this);
                caption.Convert(btr);

                //Если есть ошибки при конвертации, то подпись заголовка этих ошибок
                if (!string.IsNullOrEmpty(ErrMsg))
                {
                    ErrMsg = string.Format("Ошибки в блоке панели {0}: {1}", BlName, ErrMsg);
                }
            }
        }

        public void Def()
        {
            using (var btr = IdBtrAkr.Open(OpenMode.ForRead) as BlockTableRecord)
            {
                BlName = btr.Name;
            }
        }

        public void DeleteEnd(bool isLeftSide)
        {
            if (isLeftSide)
            {
                deleteEnds(IdsEndsLeftEntity);
                IdsEndsLeftEntity = new List<ObjectId>();
            }
            else
            {
                deleteEnds(IdsEndsRightEntity);
                IdsEndsRightEntity = new List<ObjectId>();
            }
        }

        // Удаление мусора из блока
        private static bool deleteWaste(Entity ent)
        {
            if (string.Equals(ent.Layer, Settings.Default.LayerDimensionFacade, StringComparison.CurrentCultureIgnoreCase) ||
                              string.Equals(ent.Layer, Settings.Default.LayerDimensionForm, StringComparison.CurrentCultureIgnoreCase))
            {
                ent.UpgradeOpen();
                ent.Erase();
                return true;
            }
            return false;
        }

        private void defineEnds()
        {
            // условие наличия торцов
            if (ExtentsByTile.Diagonal() < 1000 || ExtentsNoEnd.Diagonal() < 1000 ||
               ExtentsByTile.Diagonal() - ExtentsNoEnd.Diagonal() < 100)
            {
                return;
            }

            // Определение торцевых объектов в блоке
            // Торец слева
            if ((ExtentsNoEnd.MinPoint.X - ExtentsByTile.MinPoint.X) > 400)
            {
                // поиск объектов с координатой близкой к ExtentsByTile.MinPoint.X
                var idsEndEntsTemp = getEndEntsInCoord(ExtentsByTile.MinPoint.X, true);
                if (idsEndEntsTemp.Count > 0)
                {
                    HashSet<ObjectId> idsEndLeftEntsHash = new HashSet<ObjectId>();
                    idsEndEntsTemp.ForEach(t => idsEndLeftEntsHash.Add(t));
                    IdsEndsLeftEntity = idsEndLeftEntsHash.ToList();
                }
            }
            // Торец справа
            if ((ExtentsByTile.MaxPoint.X - ExtentsNoEnd.MaxPoint.X) > 400)
            {
                var idsEndEntsTemp = getEndEntsInCoord(ExtentsByTile.MaxPoint.X, true);
                if (idsEndEntsTemp.Count > 0)
                {
                    HashSet<ObjectId> idsEndRightEntsHash = new HashSet<ObjectId>();
                    idsEndEntsTemp.ForEach(t => idsEndRightEntsHash.Add(t));
                    IdsEndsRightEntity = idsEndRightEntsHash.ToList();
                }
            }
            // Торец сверху
            if ((ExtentsByTile.MaxPoint.Y - ExtentsNoEnd.MaxPoint.Y) > 400)
            {
                var idsEndEntsTemp = getEndEntsInCoord(ExtentsByTile.MaxPoint.Y, false);
                if (idsEndEntsTemp.Count > 0)
                {
                    HashSet<ObjectId> idsEndTopEntsHash = new HashSet<ObjectId>();
                    idsEndEntsTemp.ForEach(t => idsEndTopEntsHash.Add(t));
                    IdsEndsTopEntity = idsEndTopEntsHash.ToList();
                }
            }
            // Торец снизу
            if ((ExtentsNoEnd.MinPoint.Y - ExtentsByTile.MinPoint.Y) > 400)
            {
                var idsEndEntsTemp = getEndEntsInCoord(ExtentsByTile.MinPoint.Y, false);
                if (idsEndEntsTemp.Count > 0)
                {
                    HashSet<ObjectId> idsEndBotEntsHash = new HashSet<ObjectId>();
                    idsEndEntsTemp.ForEach(t => idsEndBotEntsHash.Add(t));
                    IdsEndsBottomEntity = idsEndBotEntsHash.ToList();
                }
            }
        }

        private void deleteEnds(List<ObjectId> idsList)
        {
            idsList.ForEach(idEnt =>
            {
                using (var ent = idEnt.GetObject(OpenMode.ForWrite, false, true) as Entity)
                {
                    ent.Erase();
                }
            });
        }

        private List<ObjectId> getEndEntsInCoord(double coord, bool isX)
        {
            //coord - координата края торца панели по плитке
            List<ObjectId> resVal = new List<ObjectId>();
            // выбор объектов блока на нужной координате (+- толщина торца = ширине одной плитки - 300)
            foreach (var entInfo in EntInfos)
            {
                if (Math.Abs((isX ? entInfo.Extents.MinPoint.X : entInfo.Extents.MinPoint.Y) - coord) < 330 &&
                    Math.Abs((isX ? entInfo.Extents.MaxPoint.X : entInfo.Extents.MaxPoint.Y) - coord) < 330)
                {
                    resVal.Add(entInfo.Id);
                }
            }
            return resVal;
        }

        public void iterateEntInBlock(BlockTableRecord btr, bool _deleteWaste = true)
        {
            Dictionary<Extents3d, Extents3d> tilesDict = new Dictionary<Extents3d, Extents3d>();
            _extentsByTile = new Extents3d();
            foreach (ObjectId idEnt in btr)
            {
                using (var ent = idEnt.GetObject(OpenMode.ForRead) as Entity)
                {
                    EntInfos.Add(new EntityInfo(ent));

                    // Удаление лишних объектов (мусора)
                    if (_deleteWaste && deleteWaste(ent)) continue; // Если объект удален, то переход к новому объекту в блоке

                    // Если это плитка, то определение размеров панели по габаритам всех плиток
                    if (ent is BlockReference && string.Equals(((BlockReference)ent).GetEffectiveName(),
                               Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var ext = ent.GeometricExtents;
                        _extentsByTile.AddExtents(ext);

                        try
                        {
                            tilesDict.Add(ext, ext);
                        }
                        catch (ArgumentException)
                        {
                            // Ошибка - плитка с такими границами уже есть
                            ErrMsg += "Наложение плиток. ";
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error(ex, "iterateEntInBlock - tilesDict.Add(ent.GeometricExtents, ent.GeometricExtents);");
                        }
                        continue;
                    }

                    // Если это подпись Марки (на слое Марок)
                    if (ent is DBText && string.Equals(ent.Layer, Settings.Default.LayerMarks, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Как определить - это текст Марки или Покраски - сейчас Покраска в скобках (). Но вдруг будет без скобок.
                        var textCaption = (DBText)ent;
                        if (textCaption.TextString.StartsWith("("))
                        {
                            CaptionPaint = textCaption.TextString;
                            IdCaptionPaint = idEnt;
                        }
                        else
                        {
                            CaptionMarkSb = textCaption.TextString;
                            IdCaptionMarkSb = idEnt;
                            CaptionLayerId = textCaption.LayerId;
                        }
                        continue;
                    }
                }
            }

            Tiles = tilesDict.Values.ToList();
            // Проверка
            if (string.IsNullOrEmpty(CaptionMarkSb))
            {
                ErrMsg += "Не наден текст подписи марки панели. ";
            }
            if (string.IsNullOrEmpty(CaptionPaint))
            {
                ErrMsg += "Не наден текст подписи марки покраски панели. ";
            }
            if (ExtentsByTile.Diagonal() < 100)
            {
                ErrMsg += string.Format("Не определены габариты панели по плиткам - диагональ панели = {0}", ExtentsByTile.Diagonal());
            }

            // Определение высоты панели
            HeightByTile = ExtentsByTile.MaxPoint.Y - ExtentsByTile.MinPoint.Y;
        }
    }
}
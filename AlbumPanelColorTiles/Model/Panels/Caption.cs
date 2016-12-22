using AcadLib;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
namespace AlbumPanelColorTiles.Panels
{
    public class Caption
    {
        private string _captionLayer;
        private ObjectId _idTextstylePik;
        private Database _db;
        private List<MarkSb> _marksSB;
        public Caption(Database db)
        {
            _db = db;
            _captionLayer = Caption.GetLayerForMark(db);
            _idTextstylePik = DbExtensions.GetTextStylePIK(_db);
        }
        public Caption(List<MarkSb> marksSB)
        {
            _db = HostApplicationServices.WorkingDatabase;
            _marksSB = marksSB;
            _captionLayer = Caption.GetLayerForMark(_db);
            _idTextstylePik = DbExtensions.GetTextStylePIK(_db);
        }
        public static string GetLayerForMark(Database db)
        {
            using (LayerTable layerTable = db.LayerTableId.Open(0) as LayerTable)
            {
                if (!layerTable.Has(Settings.Default.LayerMarks))
                {
                    using (LayerTableRecord layerTableRecord = new LayerTableRecord())
                    {
                        layerTableRecord.Name = Settings.Default.LayerMarks;
                        layerTable.UpgradeOpen();
                        layerTable.Add(layerTableRecord);
                    }
                }
            }
            return Settings.Default.LayerMarks;
        }
        public void AddMarkToPanelBtr(string panelMark, ObjectId idBtr)
        {
            using (BlockTableRecord btr = idBtr.Open(OpenMode.ForWrite) as BlockTableRecord)
            {
                foreach (ObjectId idEnt in btr)
                {
                    if (idEnt.IsValidEx() && (idEnt.ObjectClass.Name == "AcDbText" || idEnt.ObjectClass.Name == "AcDbHatch"))
                    {
                        using (Entity entity = idEnt.Open(OpenMode.ForRead, false, true) as Entity)
                        {
                            if (string.Equals(entity.Layer, Settings.Default.LayerMarks, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    entity.UpgradeOpen();
                                    entity.Erase(true);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error(ex, "AddMarkToPanelBtr - entity.UpgradeOpen();");
                                }
                            }
                        }
                    }
                }
                if (panelMark.EndsWith(")"))
                {
                    CreateCaptionMarkAr(panelMark, btr);
                }
                else
                {
                    CreateCaptionMarkSb(panelMark, btr);
                }
            }
        }
        public void CaptionPanels()
        {
            _captionLayer = Caption.GetLayerForMark(_db);
            foreach (MarkSb markSb in _marksSB)
            {
                if (HostApplicationServices.Current.UserBreak())
                {
                    throw new Exception("Отменено пользователем.");
                }
                AddMarkToPanelBtr(markSb.MarkSbClean, markSb.IdBtr);
                foreach (MarkAr markAr in markSb.MarksAR)
                {
                    AddMarkToPanelBtr(markAr.MarkARPanelFullName, markAr.IdBtrAr);
                }
            }
        }
        private void CreateCaptionMarkAr(string panelMark, BlockTableRecord btr)
        {
            int num = panelMark.LastIndexOf('(');
            string textMarkSb = panelMark.Substring(0, num);
            string textMarkPaint = panelMark.Substring(num);
            using (DBText dBTextPaint = GetDBText(textMarkPaint))
            {
                btr.AppendEntity(dBTextPaint);
            }
            using (DBText dBTextMarkSb = GetDBText(textMarkSb))
            {
                dBTextMarkSb.Position = new Point3d(0.0, Settings.Default.CaptionPanelSecondTextShift, 0.0);
                btr.AppendEntity(dBTextMarkSb);
            }
        }
        private void CreateCaptionMarkSb(string panelMark, BlockTableRecord btr)
        {
            using (DBText dBText = GetDBText(panelMark))
            {
                btr.AppendEntity(dBText);
            }
        }
        private DBText GetDBText(string text)
        {
            DBText dBText = new DBText();
            dBText.SetDatabaseDefaults(_db);
            dBText.TextStyleId = _idTextstylePik;
            dBText.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
            dBText.Linetype = SymbolUtilityServices.LinetypeByLayerName;
            dBText.LineWeight = LineWeight.ByLayer;
            dBText.TextString = text;
            dBText.Height = Settings.Default.CaptionPanelTextHeight;
            dBText.Annotative = AnnotativeStates.False;
            dBText.Layer = _captionLayer;
            return dBText;
        }
    }
}

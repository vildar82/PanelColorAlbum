using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
namespace AlbumPanelColorTiles.Model.Panels
{
	public class Caption
	{
		private string _captionLayer;
		private ObjectId _idTextstylePik;
		private Database _db;
		private List<MarkSb> _marksSB;
		public Caption(Database db)
		{
			this._db = db;
			this._captionLayer = Caption.GetLayerForMark(db);
			this._idTextstylePik = DbExtensions.GetTextStylePIK(this._db);
		}
		public Caption(List<MarkSb> marksSB)
		{
			this._db = HostApplicationServices.get_WorkingDatabase();
			this._marksSB = marksSB;
			this._captionLayer = Caption.GetLayerForMark(this._db);
			this._idTextstylePik = DbExtensions.GetTextStylePIK(this._db);
		}
		public static string GetLayerForMark(Database db)
		{
			using (LayerTable layerTable = db.get_LayerTableId().Open(0) as LayerTable)
			{
				bool flag = !layerTable.Has(Settings.Default.LayerMarks);
				if (flag)
				{
					using (LayerTableRecord layerTableRecord = new LayerTableRecord())
					{
						layerTableRecord.set_Name(Settings.Default.LayerMarks);
						layerTable.UpgradeOpen();
						layerTable.Add(layerTableRecord);
					}
				}
			}
			return Settings.Default.LayerMarks;
		}
		public void AddMarkToPanelBtr(string panelMark, ObjectId idBtr)
		{
			using (BlockTableRecord blockTableRecord = idBtr.Open(1) as BlockTableRecord)
			{
				using (BlockTableRecordEnumerator enumerator = blockTableRecord.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ObjectId current = enumerator.get_Current();
						bool flag = current.get_ObjectClass().get_Name() == "AcDbText" || current.get_ObjectClass().get_Name() == "AcDbHatch";
						if (flag)
						{
							using (Entity entity = current.Open(0, false, true) as Entity)
							{
								bool flag2 = string.Equals(entity.get_Layer(), Settings.Default.LayerMarks, StringComparison.OrdinalIgnoreCase);
								if (flag2)
								{
									entity.UpgradeOpen();
									entity.Erase(true);
								}
							}
						}
					}
				}
				bool flag3 = panelMark.EndsWith(")");
				if (flag3)
				{
					this.CreateCaptionMarkAr(panelMark, blockTableRecord);
				}
				else
				{
					this.CreateCaptionMarkSb(panelMark, blockTableRecord);
				}
			}
		}
		public void CaptionPanels()
		{
			this._captionLayer = Caption.GetLayerForMark(this._db);
			foreach (MarkSb current in this._marksSB)
			{
				bool flag = HostApplicationServices.get_Current().UserBreak();
				if (flag)
				{
					throw new Exception("Отменено пользователем.");
				}
				this.AddMarkToPanelBtr(current.MarkSbClean, current.IdBtr);
				foreach (MarkAr current2 in current.MarksAR)
				{
					this.AddMarkToPanelBtr(current2.MarkARPanelFullName, current2.IdBtrAr);
				}
			}
		}
		private void CreateCaptionMarkAr(string panelMark, BlockTableRecord btr)
		{
			int num = panelMark.LastIndexOf('(');
			string text = panelMark.Substring(0, num);
			string text2 = panelMark.Substring(num);
			using (DBText dBText = this.GetDBText(text2))
			{
				btr.AppendEntity(dBText);
			}
			using (DBText dBText2 = this.GetDBText(text))
			{
				dBText2.set_Position(new Point3d(0.0, (double)Settings.Default.CaptionPanelSecondTextShift, 0.0));
				btr.AppendEntity(dBText2);
			}
		}
		private void CreateCaptionMarkSb(string panelMark, BlockTableRecord btr)
		{
			using (DBText dBText = this.GetDBText(panelMark))
			{
				btr.AppendEntity(dBText);
			}
		}
		private DBText GetDBText(string text)
		{
			DBText dBText = new DBText();
			dBText.SetDatabaseDefaults(this._db);
			dBText.set_TextStyleId(this._idTextstylePik);
			dBText.set_Color(Color.FromColorIndex(192, 256));
			dBText.set_Linetype(SymbolUtilityServices.get_LinetypeByLayerName());
			dBText.set_LineWeight(-1);
			dBText.set_TextString(text);
			dBText.set_Height((double)Settings.Default.CaptionPanelTextHeight);
			dBText.set_Annotative(1);
			dBText.set_Layer(this._captionLayer);
			return dBText;
		}
	}
}

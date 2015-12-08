using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public enum EnumReportStatus
   {
      Other, // 0 - default
      New,
      Changed,
      Force
   }

   public class PanelAkrFacade : PanelAKR
   {
      private ObjectId _idBlRefWhenCreateFacade;
      private ObjectId _idBlRefForShow;
      // блок панели АКР в файле фасада
      private MountingPanel _panelSb;      
      protected EnumReportStatus _reportStatus;

      public PanelAkrFacade(ObjectId idBtr, string blName) : base(idBtr, blName)
      {

      }      

      /// <summary>
      /// Вхождение блока в файле фасада
      /// </summary>
      public ObjectId IdBlRefWhenCreateFacade { get { return _idBlRefWhenCreateFacade; } set { _idBlRefWhenCreateFacade = value; } }
      public MountingPanel PanelSb { get { return _panelSb; } set { _panelSb = value; } }
      public EnumReportStatus ReportStatus { get { return _reportStatus; } set { _reportStatus = value; } }

      

      ///// <summary>
      ///// Простая расстановка АКР-панелей в точуи вставки панелей СБ
      ///// </summary>
      ///// <param name="_allPanelsSB"></param>
      //public static void SimpleInsert(List<PanelSB> _allPanelsSB)
      //{
      //   Database db = HostApplicationServices.WorkingDatabase;
      //   using (var t = db.TransactionManager.StartTransaction())
      //   {
      //      using (ProgressMeter progress = new ProgressMeter())
      //      {
      //         progress.SetLimit(_allPanelsSB.Count);
      //         progress.Start("Простая расстановка панелей");

      //         var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
      //         foreach (var panelSb in _allPanelsSB)
      //         {
      //            int countNull = 0;
      //            if (panelSb.PanelAKR != null)
      //            {
      //               var blRefPanelAkr = new BlockReference((panelSb.GetPtInModel(panelSb.PanelAKR)), panelSb.PanelAKR.IdBtrAkrPanel);
      //               panelSb.PanelAKR.IdBlRefWhenCreateFacade = ms.AppendEntity(blRefPanelAkr);
      //               t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
      //            }
      //            else
      //            {
      //               countNull++;
      //            }
      //            progress.MeterProgress();
      //         }
      //         t.Commit();
      //         progress.Stop();
      //      }
      //   }
      //}

      

      public void ShowPanelInFacade(Document doc)
      {
         if (_idBlRefForShow.IsNull || _idBlRefForShow.IsErased)
         {
            // поиск панели на чертеже
            if (!getFirstBlRefInFacade(out _idBlRefForShow))
            {
               // вставка блока
               _idBlRefForShow = AcadLib.Blocks.BlockInsert.Insert(_blName);
               if (_idBlRefForShow.IsNull)
               {
                  return;
               }
            }
         }
         Extents3d extents;
         using (var blRef = _idBlRefForShow.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            extents = blRef.GeometricExtents;
         }
         doc.Editor.Zoom(extents);         
      }

      private bool getFirstBlRefInFacade(out ObjectId _idBlRefForShow)
      {
         using (var btrPanel = IdBtrAkrPanel.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            var idsBlRef = btrPanel.GetBlockReferenceIds(true, false);
            if (idsBlRef.Count > 0)
            {
               _idBlRefForShow = idsBlRef[0];
               return true;
            }
         }
         _idBlRefForShow = ObjectId.Null;
         return false;
      }

      public static List<PanelAkrFacade> GetChangedAndNewPanels(List<PanelAkrFacade> panelsAkrInFacade, List<PanelAkrLib> panelsAkrInLib)
      {
         List<PanelAkrFacade> panelsChangedAndNew = new List<PanelAkrFacade>();
         foreach (var panelAkrInFacade in panelsAkrInFacade)
         {
            var panelAkrInLib = panelsAkrInLib.Find(p => string.Equals(p.BlName, panelAkrInFacade.BlName, StringComparison.OrdinalIgnoreCase));
            if (panelAkrInLib == null)
            {
               // panelAkrInFacade - новая панель, которой нет в библмиотеке
               panelAkrInFacade.ReportStatus = EnumReportStatus.New; // "Новая";               
               panelsChangedAndNew.Add(panelAkrInFacade);
            }
            else
            {
               // сравнить панели (по списку объектов в блоке)
               if (!panelAkrInFacade.EntInfos.SequenceEqual(panelAkrInLib.EntInfos))
               {
                  panelAkrInFacade.ReportStatus = EnumReportStatus.Changed; //"Изменившаяся";
                  panelsChangedAndNew.Add(panelAkrInFacade);
               }
            }
         }
         return panelsChangedAndNew;
      }

      public string ReportStatusString()
      {
         switch (_reportStatus)
         {
            case EnumReportStatus.New:
               return "Новая";
            case EnumReportStatus.Changed:
               return "Изменившаяся";
            case EnumReportStatus.Force:
               return "Принудительно";
            default:
               break;
         }
         return "";
      }
   }
}

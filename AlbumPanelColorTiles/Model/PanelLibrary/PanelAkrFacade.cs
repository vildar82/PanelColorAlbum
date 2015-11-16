using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class PanelAkrFacade : PanelAKR
   {
      private ObjectId _idBlRefWhenCreateFacade;

      /// <summary>
      /// Вхождение блока в файле фасада
      /// </summary>
      public ObjectId IdBlRefWhenCreateFacade { get { return _idBlRefWhenCreateFacade; } set { _idBlRefWhenCreateFacade = value; } }

      /// <summary>
      /// Простая расстановка АКР-панелей в точуи вставки панелей СБ
      /// </summary>
      /// <param name="_allPanelsSB"></param>
      public static void SimpleInsert(List<PanelSB> _allPanelsSB)
      {
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            using (ProgressMeter progress = new ProgressMeter())
            {
               progress.SetLimit(_allPanelsSB.Count);
               progress.Start("Простая расстановка панелей");

               var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;
               foreach (var panelSb in _allPanelsSB)
               {
                  int countNull = 0;
                  if (panelSb.PanelAKR != null)
                  {
                     var blRefPanelAkr = new BlockReference((panelSb.GetPtInModel(panelSb.PanelAKR)), panelSb.PanelAKR.IdBtrAkrPanelInFacade);
                     panelSb.PanelAKR.IdBlRefWhenCreateFacade = ms.AppendEntity(blRefPanelAkr);
                     t.AddNewlyCreatedDBObject(blRefPanelAkr, true);
                  }
                  else
                  {
                     countNull++;
                  }
                  progress.MeterProgress();
               }
               t.Commit();
               progress.Stop();
            }
         }
      }
   }
}

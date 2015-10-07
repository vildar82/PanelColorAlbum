using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;

namespace AlbumPanelColorTiles.Plot
{
   /// <summary>
   /// Хреново работает.
   /// </summary>
   public class PlotMultiPDFReadDwg
   {
      // Открытие и печать всех файлов в папке
      public void PlotDir(string dir)
      {
         var dirInfo = new DirectoryInfo(dir);
         var filesDwg = dirInfo.GetFiles("*.dwg", SearchOption.TopDirectoryOnly);

         Database dbOrig = HostApplicationServices.WorkingDatabase;

         foreach (var fileDwg in filesDwg)
         {
            // Открыть файл, сделать его текущим
            using (var db = new Database(false, true))
            {
               db.ReadDwgFile(fileDwg.FullName, FileShare.ReadWrite, false, "");
               HostApplicationServices.WorkingDatabase = db;
               db.CloseInput(true);
               multiSheetPlot(db);
               HostApplicationServices.WorkingDatabase = dbOrig;
            }
         }
      }

      // Печать всех листов в текущем документе
      private void multiSheetPlot(Database db)
      {
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = (BlockTable)t.GetObject(db.BlockTableId, OpenMode.ForRead);
            var pi = new PlotInfo();
            var piv = new PlotInfoValidator();
            piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;

            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
               using (var pe = PlotFactory.CreatePublishEngine())
               {
                  var layouts = new List<Layout>();
                  DBDictionary layoutDict = (DBDictionary)db.LayoutDictionaryId.GetObject(OpenMode.ForRead);
                  foreach (DBDictionaryEntry entry in layoutDict)
                  {
                     if (entry.Key != "Model")
                     {
                        layouts.Add((Layout)t.GetObject(entry.Value, OpenMode.ForRead));
                     }
                  }
                  layouts.Sort((l1, l2) => l1.LayoutName.CompareTo(l2.LayoutName));
                  var layoutsToPlot = new ObjectIdCollection(layouts.Select(l => l.BlockTableRecordId).ToArray());

                  int numSheet = 1;
                  foreach (ObjectId btrId in layoutsToPlot)
                  {
                     var btr = (BlockTableRecord)t.GetObject(btrId, OpenMode.ForRead);
                     var lo = (Layout)t.GetObject(btr.LayoutId, OpenMode.ForRead);
                     var psv = PlotSettingsValidator.Current;
                     pi.Layout = btr.LayoutId;
                     LayoutManager.Current.CurrentLayout = lo.LayoutName;
                     piv.Validate(pi);

                     if (numSheet == 1)
                     {
                        pe.BeginPlot(null, null);
                        string fileName = Path.Combine(Path.GetDirectoryName(db.Filename), Path.GetFileNameWithoutExtension(db.Filename) + ".pdf");
                        pe.BeginDocument(pi, db.Filename, null, 1, true, fileName);
                     }
                     var ppi = new PlotPageInfo();
                     pe.BeginPage(ppi, pi, (numSheet == layoutsToPlot.Count), null);
                     pe.BeginGenerateGraphics(null);
                     pe.EndGenerateGraphics(null);
                     // Finish the sheet
                     pe.EndPage(null);
                     numSheet++;
                  }
                  // Finish the document
                  pe.EndDocument(null);
               }
            }
            else
            {
               throw new System.Exception("Другое задание на печать уже выполняется.");
            }
            t.Commit();
         }
      }
   }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AlbumPanelColorTiles.Plot
{
   public class PlotMultiPDF
   {
      #region Private Fields

      private short _bgp;

      #endregion Private Fields

      #region Public Constructors

      public PlotMultiPDF()
      {
         _bgp = (short)AcAp.GetSystemVariable("BACKGROUNDPLOT");
         AcAp.SetSystemVariable("BACKGROUNDPLOT", 0);
      }

      #endregion Public Constructors

      #region Private Destructors

      ~PlotMultiPDF()
      {
         AcAp.SetSystemVariable("BACKGROUNDPLOT", _bgp);
      }

      #endregion Private Destructors

      #region Public Methods

      public void PlotCur()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         if (!File.Exists(doc.Name))
         {
            throw new System.Exception("Нужно сохранить текущий чертеж.");
         }
         HostApplicationServices.WorkingDatabase = doc.Database;
         MultiSheetPlot("Печать листов текущего чертежа");
      }

      // Открытие и печать всех файлов в папке
      public void PlotDir(string dir)
      {
         var dirInfo = new DirectoryInfo(dir);
         var filesDwg = dirInfo.GetFiles("*.dwg", SearchOption.TopDirectoryOnly);
         Database dbOrig = HostApplicationServices.WorkingDatabase;

         if (!Application.DocumentManager.DocumentActivationEnabled)
            Application.DocumentManager.DocumentActivationEnabled = true;

         int i = 0;
         foreach (var fileDwg in filesDwg)
         {
            i++;
            using (var docOpen = Application.DocumentManager.Open(fileDwg.FullName))
            {
               Application.DocumentManager.MdiActiveDocument = docOpen;
               HostApplicationServices.WorkingDatabase = docOpen.Database;
               try
               {
                  using (var lockDoc = docOpen.LockDocument())
                  {
                     MultiSheetPlot(string.Format("Печать {0} файла из {1} в папке {2}", i, filesDwg.Length, dirInfo.Name));
                  }
               }
               finally
               {
                  docOpen.CloseAndDiscard();
                  HostApplicationServices.WorkingDatabase = dbOrig;
               }
            }
         }
      }

      #endregion Public Methods

      #region Private Methods

      // Печать всех листов в текущем документе
      private void MultiSheetPlot(string title)
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;

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
                        layouts.Add((Layout)t.GetObject(entry.Value, OpenMode.ForRead));
                  }
                  layouts.Sort((l1, l2) => l1.LayoutName.CompareTo(l2.LayoutName));
                  var layoutsToPlot = new ObjectIdCollection(layouts.Select(l => l.BlockTableRecordId).ToArray());

                  using (var ppd = new PlotProgressDialog(false, layoutsToPlot.Count, false))
                  {
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
                           ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, title);
                           ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отмена");
                           ppd.set_PlotMsgString(PlotMessageIndex.MessageCanceling, "Отмена печати");
                           ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Печать листов");
                           ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Печать листа");
                           ppd.LowerPlotProgressRange = 0;
                           ppd.UpperPlotProgressRange = 100;
                           ppd.PlotProgressPos = 0;

                           ppd.OnBeginPlot();
                           ppd.IsVisible = true;
                           pe.BeginPlot(ppd, null);

                           string fileName = Path.Combine(Path.GetDirectoryName(doc.Name), Path.GetFileNameWithoutExtension(doc.Name) + ".pdf");
                           pe.BeginDocument(pi, doc.Name, null, 1, true, fileName);
                        }
                        ppd.OnBeginSheet();
                        ppd.SheetProgressPos = 0;

                        var ppi = new PlotPageInfo();
                        pe.BeginPage(ppi, pi, (numSheet == layoutsToPlot.Count), null);
                        pe.BeginGenerateGraphics(null);
                        ppd.SheetProgressPos = 50;
                        pe.EndGenerateGraphics(null);

                        pe.EndPage(null);
                        ppd.SheetProgressPos = 100;
                        ppd.OnEndSheet();
                        numSheet++;
                        ppd.PlotProgressPos += 100 / layoutsToPlot.Count;
                     }
                     pe.EndDocument(null);
                     ppd.PlotProgressPos = 100;
                     ppd.OnEndPlot();
                     pe.EndPlot(null);
                  }
               }
            }
            else
            {
               throw new System.Exception("Другое задание на печать уже выполняется.");
            }
            t.Commit();
         }
      }

      #endregion Private Methods
   }
}
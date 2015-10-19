using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Runtime;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AlbumPanelColorTiles.Plot
{
   public class PlotMultiPDF
   {
      private short _bgp;
      private bool _isCancelPublish;

      public PlotMultiPDF()
      {
         _bgp = (short)AcAp.GetSystemVariable("BACKGROUNDPLOT");
         AcAp.SetSystemVariable("BACKGROUNDPLOT", 0);
      }

      ~PlotMultiPDF()
      {
         AcAp.SetSystemVariable("BACKGROUNDPLOT", _bgp);
      }

      public void PlotCur()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         if (!File.Exists(doc.Name))
         {
            throw new System.Exception("Нужно сохранить текущий чертеж.");
         }
         HostApplicationServices.WorkingDatabase = doc.Database;
         MultiSheetPlot("Печать текущего чертежа");
         //MultiSheetPlot(Path.GetDirectoryName(doc.Name));
      }

      // Открытие и печать всех файлов в папке
      public void PlotDir(string dir)
      {
         var dirInfo = new DirectoryInfo(dir);
         var filesDwg = dirInfo.GetFiles("*.dwg", SearchOption.TopDirectoryOnly);
         Database dbOrig = HostApplicationServices.WorkingDatabase;

         if (!Application.DocumentManager.DocumentActivationEnabled)
            Application.DocumentManager.DocumentActivationEnabled = true;

         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.SetLimit(filesDwg.Count());
         progressMeter.Start("Печать всех файлов в папке " + dir);

         int i = 0;         
         foreach (var fileDwg in filesDwg)
         {            
            if (_isCancelPublish || HostApplicationServices.Current.UserBreak())
               break;            
                        
            Document docOpen;
            progressMeter.MeterProgress();
            if (!isAlreadyOpenDoc(fileDwg.FullName, out docOpen))
            { 
               docOpen = Application.DocumentManager.Open(fileDwg.FullName);
            }
            Application.DocumentManager.MdiActiveDocument = docOpen;
            HostApplicationServices.WorkingDatabase = docOpen.Database;
            try
            {
               using (var lockDoc = docOpen.LockDocument())
               {
                  //MultiSheetPlot(Path.GetDirectoryName(docOpen.Name));                  
                  MultiSheetPlot(string.Format("Печать {0} из {1} файлов в папке {2}", i++, filesDwg.Length, dirInfo.Name));
               }
            }
            catch (System.Exception ex)
            {
               Log.Error(ex, "MultiSheetPlot()");
            }
            finally
            {
               docOpen.CloseAndDiscard();
               HostApplicationServices.WorkingDatabase = dbOrig;
            }
         }
         progressMeter.Stop(); 
      }

      private bool isAlreadyOpenDoc(string fullName, out Document docOpen)
      {
         foreach (Document item in Application.DocumentManager)
         {
            if (string.Equals(item.Name, fullName, StringComparison.CurrentCultureIgnoreCase))
            {
               docOpen = item;
               return true;
            }
         }
         docOpen = null;
         return false;
      }

      //// Печать с использованием класса PlotToFileConfig
      //private void MultiSheetPlot(string dir)
      //{
      //   Document doc = Application.DocumentManager.MdiActiveDocument;
      //   Database db = doc.Database;
      //   Application.Publisher.CancelledOrFailedPublishing += Publisher_CancelledOrFailedPublishing; 

      //   using (var t = db.TransactionManager.StartTransaction())
      //   {
      //      var bt = (BlockTable)t.GetObject(db.BlockTableId, OpenMode.ForRead);

      //      if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
      //      {
      //         var layouts = new List<Layout>();                
      //         DBDictionary layoutDict = (DBDictionary)db.LayoutDictionaryId.GetObject(OpenMode.ForRead);
      //         foreach (DBDictionaryEntry entry in layoutDict)
      //         {
      //            if (entry.Key != "Model")
      //               layouts.Add((Layout)t.GetObject(entry.Value, OpenMode.ForRead));
      //         }
      //         layouts.Sort((l1, l2) => l1.LayoutName.CompareTo(l2.LayoutName));
      //         //var layoutsToPlot = new ObjectIdCollection(layouts.Select(l => l.BlockTableRecordId).ToArray());

      //         Gile.Publish.MultiSheetsPdf gileMultiPdfPublish = new Gile.Publish.MultiSheetsPdf(dir, layouts);
      //         gileMultiPdfPublish.Publish();
      //      }
      //      else
      //      {
      //         throw new System.Exception("Другое задание на печать уже выполняется.");
      //      }
      //      t.Commit();
      //   }
      //}

      private void Publisher_CancelledOrFailedPublishing(object sender, Autodesk.AutoCAD.Publishing.PublishEventArgs e)
      {
         _isCancelPublish = true;
      }

      //Печать всех листов в текущем документе
      private void MultiSheetPlot(string title)
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;

         var layoutsToPlot = GetLayouts(db);

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

      private static ObjectIdCollection GetLayouts(Database db)
      {
         List<KeyValuePair<string, ObjectId>> layouts = new List<KeyValuePair<string, ObjectId>>();
         using (DBDictionary layoutDict = (DBDictionary)db.LayoutDictionaryId.Open(OpenMode.ForRead))
         {
            foreach (DBDictionaryEntry entry in layoutDict)
            {
               if (entry.Key != "Model")
               {
                  using (var layout = entry.Value.Open(OpenMode.ForRead) as Layout)
                  {
                     layouts.Add(new KeyValuePair<string, ObjectId>(layout.LayoutName, layout.BlockTableRecordId));
                  }
               }
            }                        
         }
         layouts.Sort((l1, l2) => l1.Key.CompareTo(l2.Key));
         return new ObjectIdCollection(layouts.Select(l => l.Value).ToArray());         
      }
   }
}
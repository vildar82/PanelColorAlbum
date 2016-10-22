using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.PanelLibrary.LibEditor
{
    // редактор библиотеки панелей
    public class LibraryEditor
    {
        // Просмотр списка панелей в библиотеке. (поиск, просмотр, добавление примечания.)
        // Удаление блоков панелей из библиотеки.
        private Database dbLib;

        public void Edit ()
        {
            var panelsFile = GetPanelsFile();
            List<PanelAKR> panelsInLib;
            dbLib = new Database(false, true);
            dbLib.ReadDwgFile(panelsFile, FileShare.ReadWrite, true, "");
            dbLib.CloseInput(true);
            // список блоков АКР-Панелей в библиотеке (полные имена блоков).
            panelsInLib = PanelAKR.GetAkrPanelLib(dbLib, true);

            UI.PanelsAkrView panelsView = new UI.PanelsAkrView(panelsInLib, this);
            UI.PanelsWindow panelsWindow = new UI.PanelsWindow(panelsView);
            Application.ShowModelessWindow(panelsWindow);
        }

        private string GetPanelsFile ()
        {
            // Копирование библиотеки в темп
            var tempFile = Path.GetTempFileName();
            File.Copy(PanelLibrarySaveService.LibPanelsFilePath, tempFile, true);
            return tempFile;
        }

        public void CloseAndDelete (List<PanelAKR> panelsToDelete)
        {
            string file = dbLib.Filename;
            try
            {
                dbLib.Dispose();
            }
            catch { }
            File.Delete(file);

            // Удаление блоков
            if (panelsToDelete != null && panelsToDelete.Count > 0)
            {
                // Сделать копию библиотеки
                PanelLibrarySaveService.BackUpLibPanelsFile();

                using (ProgressMeter progress = new ProgressMeter())
                {
                    progress.SetLimit(panelsToDelete.Count);
                    progress.Start($"Удаление {panelsToDelete.Count} панелей из библиотеки...");

                    using (var db = new Database(false, true))
                    {
                        db.ReadDwgFile(PanelLibrarySaveService.LibPanelsFilePath, FileShare.None, true, "");
                        db.CloseInput(true);

                        using (var t = db.TransactionManager.StartTransaction())
                        {
                            var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;

                            foreach (var item in panelsToDelete)
                            {
                                if (bt.Has(item.BlName))
                                {
                                    var idPanelAkr = bt[item.BlName];
                                    var dbo = idPanelAkr.GetObject(OpenMode.ForWrite);
                                    try
                                    {
                                        dbo.Erase();
                                        progress.MeterProgress();
                                    }
                                    catch { };
                                }
                            }
                            t.Commit();
                        }
                        db.SaveAs(PanelLibrarySaveService.LibPanelsFilePath, DwgVersion.Current);                        
                    }
                    Logger.Log.Error ($"Удалены панели АКР из библиотеки - {panelsToDelete.Count}шт.: {string.Join(", ",panelsToDelete)}");
                    progress.Stop();
                }
            }
        }
    }
}
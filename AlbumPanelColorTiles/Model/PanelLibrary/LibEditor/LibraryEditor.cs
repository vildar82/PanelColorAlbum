using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

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
            dbLib = new Database(false, true);
            dbLib.ReadDwgFile(panelsFile, FileShare.ReadWrite, true, "");
            dbLib.CloseInput(true);
            // список блоков АКР-Панелей в библиотеке (полные имена блоков).
            var panelsInLib = PanelAKR.GetAkrPanelLib(dbLib, true);
            UI.PanelsAkrView panelsView = new UI.PanelsAkrView(panelsInLib);
            UI.PanelsWindow panelsWindow = new UI.PanelsWindow(panelsView);
            panelsWindow.Closed += PanelsWindow_Closed;
            Application.ShowModelessWindow(panelsWindow);
        }

        private void PanelsWindow_Closed (object sender, EventArgs e)
        {
            string file = dbLib.Filename;
            dbLib.Dispose();
            File.Delete(file);
        }

        private string GetPanelsFile ()
        {
            // Копирование библиотеки в темп
            var tempFile = Path.GetTempFileName();
            File.Copy(PanelLibrarySaveService.LibPanelsFilePath, tempFile, true);
            return tempFile;
        }
    }
}
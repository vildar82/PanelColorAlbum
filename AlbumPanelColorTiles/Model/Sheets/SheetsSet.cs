using System.Collections.Generic;
using System.IO;
using System.Windows;
using AlbumPanelColorTiles.Panels;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace AlbumPanelColorTiles.Sheets
{
    // Ведомость альбома панелей
    public class SheetsSet
    {
        private List<SheetMarkSB> _sheetsMarkSB;
        private string _sheetTemplateFileContent;
        private string _sheetTemplateFileMarkSB;

        public SheetsSet(Album album)
        {
            Album = album;
            _sheetsMarkSB = new List<SheetMarkSB>();
        }

        public Album Album { get; set; }
        public List<SheetMarkSB> SheetsMarkSB { get { return _sheetsMarkSB; } }
        public string SheetTemplateFileContent { get { return _sheetTemplateFileContent; } }
        public string SheetTemplateFileMarkSB { get { return _sheetTemplateFileMarkSB; } }

        // Создание альбома панелей
        public void CreateAlbum()
        {
            //Создание файлов марок СБ и листов марок АР в них.
            // Проверка наличия файла шаблона листов
            _sheetTemplateFileMarkSB = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateSheetMarkSBFileName);
            _sheetTemplateFileContent = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateSheetContentFileName);
            if (!File.Exists(_sheetTemplateFileMarkSB))
                throw new System.Exception("\nНе найден файл шаблона для листов панелей - " + _sheetTemplateFileMarkSB);
            if (!File.Exists(_sheetTemplateFileContent))
                throw new System.Exception("\nНе найден файл шаблона для содержания альбома - " + _sheetTemplateFileContent);

            // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
            _sheetsMarkSB = ProcessingSheets(Album.MarksSB);
            if (_sheetsMarkSB.Count == 0)
            {
                throw new System.Exception("Не определены панели марок АР");
            }

            // Создаение папки для альбома панелей
            CreateAlbumFolder();

            //Поиск блока рамки на текущем чертеже фасада
            Album.AlbumInfo = new AlbumInfo();
            Album.AlbumInfo.Search();

            // Титульные листы и обложеи в одном файле "Содержание".
            // Создание титульных листов
            // Листы содержания
            SheetsContent content = new SheetsContent(this);
            content.Contents();

            ProgressMeter progressMeter = new ProgressMeter();
            progressMeter.SetLimit(_sheetsMarkSB.Count);
            progressMeter.Start("Создание файлов панелей марок СБ с листами марок АР...");
            int countMarkSB = 1;
            foreach (var sheetMarkSB in _sheetsMarkSB)
            {
                if (HostApplicationServices.Current.UserBreak())
                    throw new System.Exception("Отменено пользователем.");
                progressMeter.MeterProgress();

                sheetMarkSB.CreateSheetMarkSB(this, countMarkSB++);
            }
            progressMeter.Stop();            
        }

        // Создание папки Альбома панелей
        private void CreateAlbumFolder(string dir = "")
        {
            // Папка альбома панелей
            if (string.IsNullOrEmpty(dir))
            {
                string albumFolderName = ("АКР_" + Path.GetFileNameWithoutExtension(Album.DwgFacade)).Trim();
                string curDwgFacadeFolder = Path.GetDirectoryName(Album.DwgFacade);
                dir = Path.Combine(curDwgFacadeFolder, albumFolderName);
            }
            Album.AlbumDir = dir;
            if (Directory.Exists(Album.AlbumDir))
            {
                try
                {
                    Directory.Delete(Album.AlbumDir, true);
                }
                catch (IOException ex)
                {
                    string msg = $"{ex.Message}\n\nЗакройте все файлы из папки {dir} и нажмите OK для продолжения.";
                    var res = MessageBox.Show(msg, "АКР. Ошибка.", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK);
                    if (res == MessageBoxResult.OK)
                    {
                        CreateAlbumFolder(dir);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            Directory.CreateDirectory(Album.AlbumDir);
        }

        // Обработка панелей. получение списка Марок СБ SheetMarkSB (без создания папок, файлов и листов автокада)
        private List<SheetMarkSB> ProcessingSheets(List<MarkSb> marksSB)
        {
            List<SheetMarkSB> sheetsMarkSb = new List<SheetMarkSB>();
            foreach (var markSB in marksSB)
            {
                // Создание листа марки СБ
                SheetMarkSB sheetMarkSb = new SheetMarkSB(markSB);
                sheetsMarkSb.Add(sheetMarkSb);
            }
            // Сортировка
            sheetsMarkSb.Sort();
            return sheetsMarkSb;
        }
    }
}
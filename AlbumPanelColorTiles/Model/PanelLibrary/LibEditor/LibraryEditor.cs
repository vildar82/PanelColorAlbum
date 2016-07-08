namespace AlbumPanelColorTiles.PanelLibrary.LibEditor
{
    // редактор библиотеки панелей
    public class LibraryEditor
    {
        // Просмотр списка панелей в библиотеке. (поиск, просмотр, добавление примечания.)
        // Удаление блоков панелей из библиотеки.

        public void Edit ()
        {
            var panelsInlib = PanelLibrarySaveService.GetPanelsInLib(true);
        }
    }
}
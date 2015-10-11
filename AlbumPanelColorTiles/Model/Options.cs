namespace AlbumPanelColorTiles
{
   public class Options
   {
      //
      // Имена блоков
      //
      public string BlockColorAreaName = "АКР_Зона-покраски";

      //public string BlockStampContent = "АКР_Рамака_Содержание";
      //public string BlockStampMarkAR = "АКР_Рамка_МаркаАР";
      public string BlockFrameName = "АКР_Рамка";

      public string BlockPanelPrefixName = "АКР_Панель_";

      public string BlockTileName = "АКР_Плитка";

      // Суффикс для торцевых панелей слева
      public string endLeftPanelSuffix = "_тл";

      // Суффикс для торцевых панелей справа
      public string endRightPanelSuffix = "_тп";

      public string LayerDimensionFacade = "АР_Размеры на фасаде";

      public string LayerDimensionForm = "АР_Размеры в форме";

      //
      // Слои
      //
      /// <summary>
      /// Слой для подписей марок панелей
      /// </summary>
      public string LayerMarks = "АР_Марки";

      /// <summary>
      /// Слой для панелей чердака. Панель на этом слое считается панелью чердака.
      /// </summary>
      public string LayerUpperStoreyPanels = "АР_Панели_Чердак";

      // Слой окон (отключать на листе панели для формы)
      public string LayerWindows = "АР_Окна";

      /// <summary>
      /// Имя листа шаблона содержания в файле шаблона листов.
      /// </summary>
      public string SheetTemplateLayoutNameForContent = "Содержание";

      /// <summary>
      /// Имя листа шаблона для Марки АР в файле шаблона листов.
      /// </summary>
      public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";

      // Файл шаблона с блоками АКР (зона покраски, плитка, панель Марки СБ).
      public string TemplateBlocksAKRFileName = "АКР_Блоки.dwg";

      //
      // Шаблоны
      //
      /// <summary>
      /// Имя файла шаблона содержания (АКР_Шаблон_Содержание.dwg)
      /// </summary>
      public string TemplateSheetContentFileName = "АКР_Шаблон_Содержание.dwg";

      /// <summary>
      /// Имя файла шаблона МаркиСБ с шаблоном листа для МАрки АР. АКР_Шаблон_МаркаСБ.dwg
      /// </summary>
      public string TemplateSheetMarkSBFileName = "АКР_Шаблон_МаркаСБ.dwg";

      public int TileHeight = 88;

      // Размеры плитки
      public int TileLenght = 288; // Длина плитки

      // Высота плитки
      public int TileSeam = 12; // Высота плитки
   }
}
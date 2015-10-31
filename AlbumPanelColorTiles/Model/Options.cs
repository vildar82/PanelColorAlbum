namespace AlbumPanelColorTiles
{
   public class Options
   {
      //
      // Имена блоков
      //
      public string BlockColorAreaName = "АКР_Зона-покраски";      
      public string BlockFrameName = "АКР_Рамка";
      public string BlockPanelPrefixName = "АКР_Панель_";      
      public string BlockTileName = "АКР_Плитка";
      public string BlockMountingPlanePrefixName = "АКР_Монтажка_";
      public string BlockFacadeName = "АКР_Фасад"; // Блок обозначения стороны фасада на монтажном плане      
      //
      // Атибуты блоков
      //
      public string AttributePanelSbPaint = "ПОКРАСКА";
      public string AttributePanelSbMark = "МАРКА";

      //
      // Суффиксы (приставки к именам блоков панелей)
      //
      public string EndLeftPanelSuffix = "_тл";// Суффикс для торцевых панелей слева      
      public string EndRightPanelSuffix = "_тп";// Суффикс для торцевых панелей справа
      public string WindowPanelSuffix = "_ок";// панель отличается формой окна. _ОК1, _ОК2 и т.д. - к марке покраске прибалять -ОК1, -ОК2 и т.д.      
      //
      // Слои
      //      
      public string LayerMarks = "АР_Марки";// Слой для подписей марок панелей      
      public string LayerUpperStoreyPanels = "АР_Панели_Чердак";// Слой для панелей чердака. Панель на этом слое считается панелью чердака.      
      public string LayerWindows = "АР_Окна";// Слой окон (отключать на листе панели для формы)      
      public string LayerDimensionFacade = "АР_Размеры на фасаде";
      public string LayerDimensionForm = "АР_Размеры в форме";
      //
      // Шаблоны
      //
      public string TemplateBlocksAKRFileName = "АКР_Блоки.dwg";// Файл шаблона с блоками АКР (зона покраски, плитка, панель Марки СБ).
      public string SheetTemplateLayoutNameForContent = "Содержание";// Имя листа шаблона содержания в файле шаблона листов.           
      public string TemplateSheetContentFileName = "АКР_Шаблон_Содержание.dwg";// Имя файла шаблона содержания (АКР_Шаблон_Содержание.dwg)      
      public string TemplateSheetMarkSBFileName = "АКР_Шаблон_МаркаСБ.dwg";// Имя файла шаблона МаркиСБ с шаблоном листа для МАрки АР. АКР_Шаблон_МаркаСБ.dwg
      public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";// Имя листа шаблона для Марки АР в файле шаблона листов.
      //
      // Плитка
      //
      public int TileHeight = 88; //Высота плитки
      public int TileLenght = 288; // Длина плитки      
      public int TileSeam = 12; // Ширина шва между плитками
   }
}
namespace AlbumPanelColorTiles.Options
{
   public class Settings
   {
      private static Settings _instance = loadSettings();

      private string _attributePanelSbMark;

      //
      // Атибуты блоков
      //
      private string _attributePanelSbPaint;

      private string _blockColorAreaDynPropHeight;

      private string _blockColorAreaDynPropLength;

      //
      // Имена блоков
      //
      private string _blockColorAreaName;

      private string _blockFacadeName;
      private string _blockFrameName;
      private string _blockMountingPlanePrefixName;
      private string _blockPanelPrefixName;
      private string _blockTileName;
      private int _captionPanelSecondTextShift;

      //
      // Построение фасадов из монтажек
      //
      private int _captionPanelTextHeight;

      //
      // Суффиксы (приставки к именам блоков панелей)
      //
      private string _endLeftPanelSuffix;

      private string _endRightPanelSuffix;
      private int _facadeCaptionFloorIndent;
      private int _facadeCaptionFloorTextHeight;
      private int _facadeEndsPanelIndent;
      private int _facadeFloorHeight;
      private int _facadeIndentFromMountingPlanes;
      private int _imagePaintFormHeight;
      private int _imagePaintFormWidth;
      private int _imagePaintSpotHeight;

      //
      // Покраска по картинке
      //
      private int _imagePaintSpotLength;

      private string _layerDimensionFacade;
      private string _layerDimensionForm;

      //
      // Слои
      //
      private string _layerMarks;

      private string _layerUpperStoreyPanels;
      private string _layerWindows;
      private string _paintIndexEndLeftPanel;
      private string _paintIndexEndRightPanel;
      private string _paintIndexLastStorey;
      private string _paintIndexStorey;

      //
      // Индексы покраски
      //
      private string _paintIndexUpperStorey;

      private string _paintIndexWindow;
      private int _sheetPanelEndShift;

      //
      // Листы Альбома АКР
      //
      private int _sheetPanelEndUp;

      //_settings.Add("PaintIndexLastStorey", "П");
      private int _sheetScale;

      private string _sheetTemplateLayoutNameForContent;
      private string _sheetTemplateLayoutNameForMarkAR;
      private int _storeyDefineDeviation;

      //
      // Шаблоны
      //
      private string _templateBlocksAKRFileName;

      private string _templateSheetContentFileName;
      private string _templateSheetMarkSBFileName;

      //
      // Плитка
      //
      private int _tileHeight;

      private int _tileLenght;
      private int _tileSeam;
      private string _windowPanelSuffix;

      private Settings()
      { }

      public static Settings Default
      {
         get
         {
            return _instance;
         }
      }

      public string AttributePanelSbMark { get { return _attributePanelSbMark; } }
      public string AttributePanelSbPaint { get { return _attributePanelSbPaint; } }
      public string BlockColorAreaDynPropHeight { get { return _blockColorAreaDynPropHeight; } }
      public string BlockColorAreaDynPropLength { get { return _blockColorAreaDynPropLength; } }
      public string BlockColorAreaName { get { return _blockColorAreaName; } }
      public string BlockFacadeName { get { return _blockFacadeName; } }
      public string BlockFrameName { get { return _blockFrameName; } }
      public string BlockMountingPlanePrefixName { get { return _blockMountingPlanePrefixName; } }
      public string BlockPanelPrefixName { get { return _blockPanelPrefixName; } }
      public string BlockTileName { get { return _blockTileName; } }
      public int CaptionPanelSecondTextShift { get { return _captionPanelSecondTextShift; } }
      public int CaptionPanelTextHeight { get { return _captionPanelTextHeight; } }
      public string EndLeftPanelSuffix { get { return _endLeftPanelSuffix; } }
      public string EndRightPanelSuffix { get { return _endRightPanelSuffix; } }
      public int FacadeCaptionFloorIndent { get { return _facadeCaptionFloorIndent; } }
      public int FacadeCaptionFloorTextHeight { get { return _facadeCaptionFloorTextHeight; } }
      public int FacadeEndsPanelIndent { get { return _facadeEndsPanelIndent; } }
      public int FacadeFloorHeight { get { return _facadeFloorHeight; } }
      public int FacadeIndentFromMountingPlanes { get { return _facadeIndentFromMountingPlanes; } }
      public int ImagePaintFormHeight { get { return _imagePaintFormHeight; } }
      public int ImagePaintFormWidth { get { return _imagePaintFormWidth; } }
      public int ImagePaintSpotHeight { get { return _imagePaintSpotHeight; } }
      public int ImagePaintSpotLength { get { return _imagePaintSpotLength; } }
      public string LayerDimensionFacade { get { return _layerDimensionFacade; } }
      public string LayerDimensionForm { get { return _layerDimensionForm; } }
      public string LayerMarks { get { return _layerMarks; } }
      public string LayerUpperStoreyPanels { get { return _layerUpperStoreyPanels; } }
      public string LayerWindows { get { return _layerWindows; } }
      public string PaintIndexEndLeftPanel { get { return _paintIndexEndLeftPanel; } }
      public string PaintIndexEndRightPanel { get { return _paintIndexEndRightPanel; } }
      public string PaintIndexLastStorey { get { return _paintIndexLastStorey; } }
      public string PaintIndexStorey { get { return _paintIndexStorey; } }
      public string PaintIndexUpperStorey { get { return _paintIndexUpperStorey; } }
      public string PaintIndexWindow { get { return _paintIndexWindow; } }
      public int SheetPanelEndShift { get { return _sheetPanelEndShift; } }
      public int SheetPanelEndUp { get { return _sheetPanelEndUp; } }
      public int SheetScale { get { return _sheetScale; } }
      public string SheetTemplateLayoutNameForContent { get { return _sheetTemplateLayoutNameForContent; } }
      public string SheetTemplateLayoutNameForMarkAR { get { return _sheetTemplateLayoutNameForMarkAR; } }
      public int StoreyDefineDeviation { get { return _storeyDefineDeviation; } }
      public string TemplateBlocksAKRFileName { get { return _templateBlocksAKRFileName; } }
      public string TemplateSheetContentFileName { get { return _templateSheetContentFileName; } }
      public string TemplateSheetMarkSBFileName { get { return _templateSheetMarkSBFileName; } }
      public int TileHeight { get { return _tileHeight; } }
      public int TileLenght { get { return _tileLenght; } }
      public int TileSeam { get { return _tileSeam; } }
      public string WindowPanelSuffix { get { return _windowPanelSuffix; } }

      private static Settings loadSettings()
      {
         Settings settings = new Settings();
         // Если есть файл настроек загрузка настроек из него

         // Если файла нет, установка дефолтных настроек
         settings.setDefault();

         return settings;
      }

      private void setDefault()
      {
         _blockColorAreaName = "АКР_Зона-покраски";
         _blockFrameName = "АКР_Рамка";
         _blockPanelPrefixName = "АКР_Панель_";
         _blockMountingPlanePrefixName = "АКР_Монтажка_";
         _blockFacadeName = "АКР_Фасад";
         _blockTileName = "АКР_Плитка";
         _attributePanelSbPaint = "ПОКРАСКА";
         _attributePanelSbMark = "МАРКА";
         _endLeftPanelSuffix = "_тл";
         _endRightPanelSuffix = "_тп";
         _windowPanelSuffix = "_ок";
         _layerMarks = "АР_Марки";
         _layerUpperStoreyPanels = "АР_Панели_Чердак";
         _layerWindows = "АР_Окна";
         _layerDimensionFacade = "АР_Размеры на фасаде";
         _layerDimensionForm = "АР_Размеры в форме";
         _templateBlocksAKRFileName = "АКР_Блоки.dwg";
         _sheetTemplateLayoutNameForContent = "Содержание";
         _templateSheetContentFileName = "АКР_Шаблон_Содержание.dwg";
         _templateSheetMarkSBFileName = "АКР_Шаблон_МаркаСБ.dwg";
         _sheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";
         _tileHeight = 88;
         _tileLenght = 288;
         _tileSeam = 12;
         _captionPanelTextHeight = 180;
         _captionPanelSecondTextShift = 250;
         _imagePaintSpotLength = 300;
         _imagePaintSpotHeight = 300;
         _imagePaintFormWidth = 1200;
         _imagePaintFormHeight = 900;
         _facadeFloorHeight = 2800;
         _facadeCaptionFloorTextHeight = 250;
         _facadeCaptionFloorIndent = 3000;
         _facadeIndentFromMountingPlanes = 10000;
         _facadeEndsPanelIndent = 890;
         _paintIndexUpperStorey = "Ч";
         _paintIndexWindow = "-ОК";
         _paintIndexEndLeftPanel = "ТЛ";
         _paintIndexEndRightPanel = "ТП";
         _paintIndexStorey = "Э";
         _paintIndexLastStorey = "П";
         _storeyDefineDeviation = 2000;
         _blockColorAreaDynPropLength = "Длина";
         _blockColorAreaDynPropHeight = "Высота";
         _sheetPanelEndUp = 1400;
         _sheetPanelEndShift = 700;
         _sheetScale = 25;
      }
   }
}

//namespace AlbumPanelColorTiles
//{
//public class Options
//{
//
// Имена блоков
//
//public string BlockColorAreaName = "АКР_Зона-покраски";
//public string BlockFrameName = "АКР_Рамка";
//public string BlockPanelPrefixName = "АКР_Панель_";
//public string BlockTileName = "АКР_Плитка";
//public string BlockMountingPlanePrefixName = "АКР_Монтажка_";
//public string BlockFacadeName = "АКР_Фасад"; // Блок обозначения стороны фасада на монтажном плане
//
// Атибуты блоков
//
//public string AttributePanelSbPaint = "ПОКРАСКА";
//public string AttributePanelSbMark = "МАРКА";

////
//// Суффиксы (приставки к именам блоков панелей)
////
//public string EndLeftPanelSuffix = "_тл";// Суффикс для торцевых панелей слева
//public string EndRightPanelSuffix = "_тп";// Суффикс для торцевых панелей справа
//public string WindowPanelSuffix = "_ок";// панель отличается формой окна. _ОК1, _ОК2 и т.д. - к марке покраске прибалять -ОК1, -ОК2 и т.д.
////
//// Слои
////
//public string LayerMarks = "АР_Марки";// Слой для подписей марок панелей
//public string LayerUpperStoreyPanels = "АР_Панели_Чердак";// Слой для панелей чердака. Панель на этом слое считается панелью чердака.
//public string LayerWindows = "АР_Окна";// Слой окон (отключать на листе панели для формы)
//public string LayerDimensionFacade = "АР_Размеры на фасаде";
//public string LayerDimensionForm = "АР_Размеры в форме";
////
//// Шаблоны
////
//public string TemplateBlocksAKRFileName = "АКР_Блоки.dwg";// Файл шаблона с блоками АКР (зона покраски, плитка, панель Марки СБ).
//public string SheetTemplateLayoutNameForContent = "Содержание";// Имя листа шаблона содержания в файле шаблона листов.
//public string TemplateSheetContentFileName = "АКР_Шаблон_Содержание.dwg";// Имя файла шаблона содержания (АКР_Шаблон_Содержание.dwg)
//public string TemplateSheetMarkSBFileName = "АКР_Шаблон_МаркаСБ.dwg";// Имя файла шаблона МаркиСБ с шаблоном листа для МАрки АР. АКР_Шаблон_МаркаСБ.dwg
//public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";// Имя листа шаблона для Марки АР в файле шаблона листов.
////
//// Плитка
////
//public int TileHeight = 88; //Высота плитки
//public int TileLenght = 288; // Длина плитки
//public int TileSeam = 12; // Ширина шва между плитками
//}
//}
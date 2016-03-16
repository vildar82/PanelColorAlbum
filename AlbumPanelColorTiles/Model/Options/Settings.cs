namespace AlbumPanelColorTiles.Options
{
    public class Settings
    {
        private static Settings _instance = loadSettings();

        private Settings()
        { }

        public static Settings Default
        {
            get
            {
                return _instance;
            }
        }

        public string AttributeFacadeAxis1 { get; set; }
        public string AttributeFacadeAxis2 { get; set; }
        public string AttributePanelSbMark { get; set; }
        public string AttributePanelSbPaint { get; set; }
        public string AttributeSectionName { get; set; }
        public string BlockColorAreaDynPropHeight { get; set; }
        public string BlockColorAreaDynPropLength { get; set; }
        public string BlockColorAreaName { get; set; }
        public string BlockFacadeName { get; set; }
        public string BlockFrameName { get; set; }
        public string BlockCoverName { get; set; }
        public string BlockTitleName { get; set; }
        public string BlockPlaneMountingPrefixName { get; set; }
        public string BlockPlaneArchitectPrefixName { get; set; }
        public string BlockPanelAkrPrefixName { get; set; }
        public string BlockPanelSectionVerticalPrefixName { get; set; }
        public string BlockPanelSectionHorizontalPrefixName { get; set; }
        public string BlockSectionName { get; set; }
        public string BlockTileName { get; set; }
        public string BlockWindowName { get; set; }
        public string BlockWindowVisibilityName { get; set; }
        public string BlockViewName { get; set; }
        public string BlockCrossName { get; set; }
        public string BlockProfileTile { get; set; }
        public string BlockArrow { get; set; }
        public string BlockWindowHorSection { get; set; }
        public int CaptionPanelSecondTextShift { get; set; }
        public int CaptionPanelTextHeight { get; set; }
        public string EndLeftPanelSuffix { get; set; }
        public string EndRightPanelSuffix { get; set; }
        public int FacadeCaptionFloorIndent { get; set; }
        public int FacadeCaptionFloorTextHeight { get; set; }
        public int FacadeEndsPanelIndent { get; set; }
        public int FacadeFloorHeight { get; set; }
        public int FacadeIndentFromMountingPlanes { get; set; }
        public int ImagePaintFormHeight { get; set; }
        public int ImagePaintFormWidth { get; set; }
        public int ImagePaintSpotHeight { get; set; }
        public int ImagePaintSpotLength { get; set; }
        public string LayerDimensionFacade { get; set; }
        public string LayerDimensionForm { get; set; }
        public string LayerMarks { get; set; }
        public string LayerParapetPanels { get; set; }
        public string LayerUpperStoreyPanels { get; set; }
        public string LayerWindows { get; set; }
        public string PaintIndexEndLeftPanel { get; set; }
        public string PaintIndexEndRightPanel { get; set; }
        public string PaintIndexLastStorey { get; set; }
        public string PaintIndexParapet { get; set; }
        public string PaintIndexStorey { get; set; }
        public string PaintIndexUpperStorey { get; set; }
        public string PaintIndexWindow { get; set; }
        public int SheetPanelEndShift { get; set; }
        public int SheetPanelEndUp { get; set; }
        public int SheetScale { get; set; }
        public string SheetTemplateLayoutNameForContent { get; set; }
        public string SheetTemplateLayoutNameForMarkAR { get; set; }
        public int StoreyDefineDeviation { get; set; }
        public string TemplateBlocksAKRExportFacadeFileName { get; set; }
        public string TemplateBlocksAKRFileName { get; set; }
        public string TemplateBlocksAkrWindows { get; set; }
        public string TemplateSheetContentFileName { get; set; }
        public string TemplateSheetMarkSBFileName { get; set; }
        public int TileHeight { get; set; }
        public int TileLenght { get; set; }
        public int TileSeam { get; set; }
        public int TileThickness { get; set; }
        public string WindowPanelSuffix { get; set; }

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
            BlockColorAreaName = "АКР_Зона-покраски";
            BlockFrameName = "АКР_Рамка";
            BlockCoverName = "АКР_Обложка";
            BlockTitleName = "АКР_Титульный";
            BlockPanelAkrPrefixName = "АКР_Панель_";
            BlockPanelSectionVerticalPrefixName = "АКР_СечениеПанелиВертик_";
            BlockPanelSectionHorizontalPrefixName = "АКР_СечениеПанелиГор";
            BlockPlaneMountingPrefixName = "АКР_Монтажка_";
            BlockPlaneArchitectPrefixName = "АКР_Архитектура_";
            BlockFacadeName = "АКР_Фасад";
            BlockTileName = "АКР_Плитка";
            BlockWindowName = "АКР_Окно";
            BlockWindowVisibilityName = "Видимость";
            BlockViewName = "АКР_Вид";
            BlockCrossName = "АКР_Разрез";
            BlockProfileTile = "АКР_Профиль_плитки";
            BlockArrow = "АКР_Стрелка";
            BlockWindowHorSection = "АКР_ОкноСеченияПанелиГор";
            BlockSectionName = "АКР_Секция";
            AttributePanelSbPaint = "ПОКРАСКА";
            AttributePanelSbMark = "МАРКА";
            AttributeSectionName = "СЕКЦИЯ";
            AttributeFacadeAxis1 = "ОСЬ1";
            AttributeFacadeAxis2 = "ОСЬ2";
            EndLeftPanelSuffix = "_тл";
            EndRightPanelSuffix = "_тп";
            WindowPanelSuffix = "_ок";
            LayerMarks = "АР_Марки";
            LayerUpperStoreyPanels = "АР_Панели_Чердак";
            LayerParapetPanels = "АР_Панели_Парапет";
            LayerWindows = "АР_Окна";
            LayerDimensionFacade = "АР_Размеры на фасаде";
            LayerDimensionForm = "АР_Размеры в форме";
            TemplateBlocksAKRFileName = "АКР_Блоки.dwg";
            TemplateBlocksAkrWindows = "АКР_Окна.dwg";
            TemplateBlocksAKRExportFacadeFileName = "АКР_Блоки-ЭкспортФасада.dwg";
            SheetTemplateLayoutNameForContent = "Содержание";
            TemplateSheetContentFileName = "АКР_Шаблон_Содержание.dwg";
            TemplateSheetMarkSBFileName = "АКР_Шаблон_МаркаСБ.dwg";
            SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";
            TileHeight = 88;
            TileLenght = 288;
            TileSeam = 12;
            TileThickness = 8;
            CaptionPanelTextHeight = 180;
            CaptionPanelSecondTextShift = 250;
            ImagePaintSpotLength = 300;
            ImagePaintSpotHeight = 300;
            ImagePaintFormWidth = 1200;
            ImagePaintFormHeight = 900;
            FacadeFloorHeight = 2800;
            FacadeCaptionFloorTextHeight = 250;
            FacadeCaptionFloorIndent = 3000;
            FacadeIndentFromMountingPlanes = 20000;
            FacadeEndsPanelIndent = 890;
            PaintIndexUpperStorey = "Ч";
            PaintIndexParapet = "П";
            PaintIndexWindow = "-ОК";
            PaintIndexEndLeftPanel = "ТЛ";
            PaintIndexEndRightPanel = "ТП";
            PaintIndexStorey = "Э";
            PaintIndexLastStorey = "П";
            StoreyDefineDeviation = 2000;
            BlockColorAreaDynPropLength = "Длина";
            BlockColorAreaDynPropHeight = "Высота";
            SheetPanelEndUp = 1400;
            SheetPanelEndShift = 700;
            SheetScale = 25;
        }
    }
}
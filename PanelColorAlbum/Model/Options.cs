namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   public class Options
   {
      public string BlockPanelPrefixName = "АКР_Панель_";
      public string BlockColorAreaName = "АКР_Зона-покраски";
      public string BlockTileName = "АКР_Плитка";
      /// <summary>
      /// Путь к файлу шаблона МаркиСБ с шаблоном листа для МАрки АР.
      /// </summary>
      public string SheetTemplateFileMarkSB = "root";
      public string SheetTemplateFileContent = "root";
      /// <summary>
      /// Имя листа шаблона для Марки АР в файле шаблона листов.
      /// </summary>
      public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";
      public string SheetTemplateLayoutNameForContent = "Содержание";
      public string BlockStampContent = "АКР_Рамака_Содержание";
      public string BlockStampMarkAR = "АКР_Рамка_МаркаАР";
      /// <summary>
      /// Слой для подписей марок панелей
      /// </summary>
      public string LayerMarks = "АР_Марки";
      /// <summary>
      /// Слой для панелей чердака. Панель на этом слое считается панелью чердака.
      /// </summary>
      public string LayerUpperStoreyPanels = "АР_Панели_Чердак";
      // Слой для панелей с торцом справа
      public string LayerPanelEndRight = "АР_Панели_Торец справа";
      // Слой для панелей с торцом слева
      public string LayerPanelEndLeft = "АР_Панели_Торец слева";
   }
}
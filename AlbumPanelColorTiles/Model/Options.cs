namespace Vil.Acad.AR.AlbumPanelColorTiles.Model
{
   public class Options
   {
      public string BlockColorAreaName = "АКР_Зона-покраски";
      public string BlockPanelPrefixName = "АКР_Панель_";
      public string BlockStampContent = "АКР_Рамака_Содержание";
      public string BlockStampMarkAR = "АКР_Рамка_МаркаАР";
      public string BlockTileName = "АКР_Плитка";

      // Суффикс для торцевых панелей слева
      public string endLeftPanelSuffix = "_тл";

      // Суффикс для торцевых панелей справа
      public string endRightPanelSuffix = "_тп";

      /// <summary>
      /// Слой для подписей марок панелей
      /// </summary>
      public string LayerMarks = "АР_Марки";

      /// <summary>
      /// Слой для панелей чердака. Панель на этом слое считается панелью чердака.
      /// </summary>
      public string LayerUpperStoreyPanels = "АР_Панели_Чердак";

      public string SheetTemplateFileContent = "root";

      /// <summary>
      /// Путь к файлу шаблона МаркиСБ с шаблоном листа для МАрки АР.
      /// </summary>
      public string SheetTemplateFileMarkSB = "root";
      public string SheetTemplateLayoutNameForContent = "Содержание";

      /// <summary>
      /// Имя листа шаблона для Марки АР в файле шаблона листов.
      /// </summary>
      public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";
   }
}
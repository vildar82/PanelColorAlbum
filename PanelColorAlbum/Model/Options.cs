﻿namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   public class Options
   {
      public string BlockPanelPrefixName = "АКР_Панель_";
      public string BlockColorAreaName = "АКР_Зона-покраски";
      public string BlockTileName = "АКР_Плитка";
      /// <summary>
      /// Путь к файлу шаблона листов.
      /// </summary>
      public string SheetTemplateFile = @"c:\temp\Sheet.dwg";
      /// <summary>
      /// Имя листа шаблона для Марки АР в файле шаблона листов.
      /// </summary>
      public string SheetTemplateLayoutNameForMarkAR = "TemplateMarkAR";
      /// <summary>
      /// Слой для подписей марок панелей
      /// </summary>
      public string LayerMarks = "АР_Марки";
      /// <summary>
      /// Слой для панелей чердака. Панель на этом слое считается панелью чердака.
      /// </summary>
      public string LayerUpperStoreyPanels = "АР_Чердак";
   }
}
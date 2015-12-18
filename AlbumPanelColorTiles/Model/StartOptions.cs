using System;
using System.ComponentModel;
using System.Globalization;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;

namespace AlbumPanelColorTiles.Model
{
   public class StartOptions
   {
      [Category()]
      [DisplayName("Индекс проекта")]
      [Description("Добавляется к марке покраски.")]
      [DefaultValue("Н47Г")]
      public string Abbr { get; set; }

      [Category()]
      [DisplayName("Проверка марок покраски")]
      [Description("При создании альбома марки покраски будут сверяться со значениями в блоках монтажных панелей. Необходимо включать эту опцию после выдачи задания по маркам покраски конструкторам.")]
      [DefaultValue(false)]
      [TypeConverter(typeof(BooleanTypeConverter))]
      public bool CheckMarkPainting { get; set; }

      [Category()]
      [DisplayName("Номер первого этажа")]
      [Description("Начальный номер для нумерации этажей.")]
      [DefaultValue(2)]
      public int NumberFirstFloor { get; set; }

      [Category()]
      [DisplayName("Номер первого листа")]
      [Description("Если 0, то этот параметр не учитывается.")]
      [DefaultValue(0)]
      public int NumberFirstSheet { get; set; }

      public void PromptStartOptions()
      {
         // Дефолтное значение аббревиатуры проекта
         if (Abbr == null)
         {
            Abbr = loadAbbreviateName();// "Н47Г";
            CheckMarkPainting = DictNOD.LoadBool(Album.KEYNAMECHECKMARKPAINTING);
         }
         // дефолтное значение номера первого этажа
         if (NumberFirstFloor == 0)
         {
            NumberFirstFloor = loadNumberFromDict(Album.KEYNAMENUMBERFIRSTFLOOR, 2);
         }
         // дефолтное значение номера первого листа
         if (NumberFirstSheet == 0)
         {
            NumberFirstSheet = loadNumberFromDict(Album.KEYNAMENUMBERFIRSTSHEET, 0);
         }

         // Запрос начальных значений
         FormStartOptions formStartOptions = new FormStartOptions(this);
         if (Application.ShowModalDialog(formStartOptions) != System.Windows.Forms.DialogResult.OK)
         {
            throw new System.Exception("Отменено пользователем.");
         }
         try
         {
            saveAbbreviateName(Abbr);
            saveNumberToDict(NumberFirstFloor, Album.KEYNAMENUMBERFIRSTFLOOR);
            saveNumberToDict(NumberFirstSheet, Album.KEYNAMENUMBERFIRSTSHEET);
            DictNOD.SaveBool(CheckMarkPainting, Album.KEYNAMECHECKMARKPAINTING);
         }
         catch (Exception ex)
         {
            Log.Error(ex, "Не удалось сохранить стартовые параметры.");
         }
      }

      private string loadAbbreviateName()
      {
         string res = "Н47Г"; // default
         try
         {
            // из словаря чертежа
            res = DictNOD.LoadAbbr();
            if (string.IsNullOrEmpty(res))
            {
               var keyAKR = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Album.REGAPPPATH);
               res = (string)keyAKR.GetValue(Album.REGKEYABBREVIATE, "Н47Г");
            }
         }
         catch { }
         return res;
      }

      private int loadNumberFromDict(string keyName, int defaulVal)
      {
         int res = defaulVal;
         try
         {
            // из словаря чертежа
            res = DictNOD.LoadNumber(keyName);
            if (res == 0)
            {
               res = defaulVal; // default
            }
         }
         catch { }
         return res;
      }

      private void saveAbbreviateName(string abbr)
      {
         try
         {
            // в реестр
            var keyAKR = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Album.REGAPPPATH);
            keyAKR.SetValue(Album.REGKEYABBREVIATE, abbr, Microsoft.Win32.RegistryValueKind.String);
            // в словарь чертежа
            DictNOD.SaveAbbr(abbr);
         }
         catch { }
      }

      private void saveNumberToDict(int number, string keyName)
      {
         try
         {
            // в словарь чертежа
            DictNOD.SaveNumber(number, keyName);
         }
         catch { }
      }
   }

   internal class BooleanTypeConverter : BooleanConverter
   {
      public override object ConvertFrom(ITypeDescriptorContext context,
        CultureInfo culture,
        object value)
      {
         return (string)value == "Да";
      }

      public override object ConvertTo(ITypeDescriptorContext context,
              CultureInfo culture,
        object value,
        Type destType)
      {
         return (bool)value ?
           "Да" : "Нет";
      }
   }
}
using System;
using System.ComponentModel;
using System.Globalization;
using AlbumPanelColorTiles.Lib;
using Autodesk.AutoCAD.ApplicationServices;

namespace AlbumPanelColorTiles.Model
{
    public class StartOption
    {
        [Category("Важно")]
        [DisplayName("Индекс проекта")]
        [Description("Добавляется к марке покраски.")]
        [DefaultValue("Н47Г")]
        public string Abbr { get; set; }

        [Category("Важно")]
        [DisplayName("Проверка марок покраски")]
        [Description("При создании альбома марки покраски будут сверяться со значениями в блоках монтажных панелей. Необходимо включать эту опцию после выдачи задания по маркам покраски конструкторам.")]
        [DefaultValue(false)]
        [TypeConverter(typeof(BooleanTypeConverter))]
        public bool CheckMarkPainting { get; set; }

        [Category("Важно")]
        [DisplayName("Номер первого этажа СБ")]
        [Description("Первый этаж для сборной части.")]
        [DefaultValue(2)]
        public int NumberFirstFloor { get; set; }

        [Browsable(false)]
        [Category("Важно")]
        [DisplayName("Сортировка перед покраской")]
        [Description("Сортировка блоков панелей перед покраской - слева-направо, снизу-вверх по этажам. Влияет на порядок присвоения марок панелям. Для старых проектов выбирайте Нет, для новых Да. После нового года эта опция будет скрыта и включена по-умолчанию.")]
        [DefaultValue(true)]
        [TypeConverter(typeof(BooleanTypeConverter))]
        public bool SortPanels { get; set; }

        [Category("Важно")]
        [DisplayName("Способ построения панелей")]
        [Description("Новый - это автоматически создаваемые панели. Старый - ручной и по библиотеке блоков панелей.")]
        [DefaultValue(false)]
        [TypeConverter(typeof(BooleanNewModeConverter))]
        public bool NewMode { get; set; }

        [Category("Важно")]
        [DisplayName("Торцы в марке покраски")]
        [Description("Добалять или нет индекс торцов в марку покраски. Например 'Э2ТП'.")]
        [DefaultValue(false)]
        [TypeConverter(typeof(BooleanTypeConverter))]
        public bool EndsInPainting { get; set; }

        [Category("Важно")]
        [DisplayName("Разделитель индекса покраски")]
        [Description("Разделитель для индекса покраски. Раньше был'-', сейчас принят '_'.")]
        [DefaultValue("_")]        
        public string SplitIndexPainting { get; set; }

        [Category("Не важно")]
        [DisplayName("Номер первого листа в альбоме")]
        [Description("Начальный номер для листов панелей в альбоме. Если 0, то этот параметр не учитывается.")]
        [DefaultValue(0)]
        public int NumberFirstSheet { get; set; }

        public void LoadDefault()
        {            
            // Дефолтное значение аббревиатуры проекта
            if (Abbr == null)
            {
                Abbr = loadAbbreviateName();// "Н47Г";                     
            }
            CheckMarkPainting = DictNOD.LoadBool(Album.KEYNAMECHECKMARKPAINTING, false);
            NewMode = DictNOD.LoadBool(Album.KEYNAMENEWMODE, false);
            NumberFirstFloor = loadNumberFromDict(Album.KEYNAMENUMBERFIRSTFLOOR, 2);
            NumberFirstSheet = loadNumberFromDict(Album.KEYNAMENUMBERFIRSTSHEET, 0);            
            SortPanels = DictNOD.LoadBool(Album.KEYNAMESORTPANELS, true);
            EndsInPainting = DictNOD.LoadBool(Album.KEYNAMEENDSINPAINTING, false);
            SplitIndexPainting = DictNOD.LoadString(Album.KEYNAMESPLITINDEXPAINTING, "_");
        }

        public StartOption PromptStartOptions()
        {
            StartOption resVal = this;
            //Запрос начальных значений
            FormStartOptions formStartOptions = new FormStartOptions((StartOption)resVal.MemberwiseClone());
            if (Application.ShowModalDialog(formStartOptions) != System.Windows.Forms.DialogResult.OK)
            {
                throw new System.Exception(AcadLib.General.CanceledByUser);
            }
            try
            {
                resVal = formStartOptions.StartOptions;
                saveAbbreviateName(resVal.Abbr);
                saveNumberToDict(resVal.NumberFirstFloor, Album.KEYNAMENUMBERFIRSTFLOOR);
                saveNumberToDict(resVal.NumberFirstSheet, Album.KEYNAMENUMBERFIRSTSHEET);
                DictNOD.SaveBool(resVal.CheckMarkPainting, Album.KEYNAMECHECKMARKPAINTING);
                DictNOD.SaveBool(resVal.SortPanels, Album.KEYNAMESORTPANELS);
                DictNOD.SaveBool(resVal.NewMode, Album.KEYNAMENEWMODE);
                DictNOD.SaveBool(resVal.EndsInPainting, Album.KEYNAMEENDSINPAINTING);
                DictNOD.SaveString(resVal.SplitIndexPainting, Album.KEYNAMESPLITINDEXPAINTING);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Не удалось сохранить стартовые параметры.");
            }
            return resVal;
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

    public class BooleanTypeConverter : BooleanConverter
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
    
    public class BooleanNewModeConverter : BooleanConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture,
          object value)
        {
            return (string)value == "Новый";
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                CultureInfo culture,
          object value,
          Type destType)
        {
            return (bool)value ?
              "Новый" : "Старый";
        }
    }    
}
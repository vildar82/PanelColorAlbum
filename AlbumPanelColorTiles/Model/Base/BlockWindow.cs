using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Base
{
    public class BlockWindow
    {
        //public const string PropNameVISIBILITY = "Видимость";

        public string Mark { get; private set; } = string.Empty;
        public string BlName { get; private set; }
        public MarkSb MarkSb { get; private set; }
        /// <summary>
        /// Окно противопожарное
        /// </summary>
        public bool IsFireBox { get; set; }
        public static List<string> FireBoxMarks { get; set; } = new List<string>() { "ОП-3.2" };

        public BlockWindow(BlockReference blRef, string blName, MarkSb markSb)
        {
            MarkSb = markSb;
            BlName = blName;
            defineWindow(blRef);
            checkWindow();
        }

        private void checkWindow()
        {
            if (string.IsNullOrEmpty(Mark))
            {
                Inspector.AddError($"Не определена марка окна '{BlName}' в блоке панели '{MarkSb.MarkSbBlockName}'", icon: System.Drawing.SystemIcons.Error);
            }
        }

        private void defineWindow(BlockReference blRef)
        {
            //if (blRef != null && blRef.DynamicBlockReferencePropertyCollection != null)
            //{
            //   foreach (DynamicBlockReferenceProperty prop in blRef.DynamicBlockReferencePropertyCollection)
            //   {
            //      if (prop.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
            //      {
            //         Mark = prop.Value.ToString();
            //         break;
            //      }
            //   }
            //}       
            try
            {
                var split = BlName.Split(new[] { '_' });
                Mark = split.Skip(2).First();   
                if (FireBoxMarks.Contains(Mark))
                {
                    IsFireBox = true;
                }
            }
            catch
            {
            }
        }

        public static bool SetDynBlWinMark(BlockReference blRefWin, string mark)
        {
            //return true;
            if (blRefWin == null)
            {
                return false;
            }
            bool findProp = false;
            var dynProps = blRefWin.DynamicBlockReferencePropertyCollection;
            foreach (DynamicBlockReferenceProperty item in dynProps)
            {
                if (item.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                {
                    findProp = true;
                    var allowedVals = item.GetAllowedValues();
                    // Найти марку без учетарегистра
                    var findMarkValue = allowedVals.FirstOrDefault(v => v.ToString().Equals(mark, StringComparison.OrdinalIgnoreCase));
                    if (findMarkValue != null)
                    {
                        item.Value = findMarkValue;
                        return true;
                    }
                    else
                    {
                        Inspector.AddError($"Блок окна. Отсутствует видимость для марки окна {mark}", icon: System.Drawing.SystemIcons.Error);
                        return false;
                    }
                }
            }
            if (!findProp)
            {
                Inspector.AddError("Блок окна. Не найдено динамическое свойтво блока окна Видимость", icon: System.Drawing.SystemIcons.Error);
            }
            return false;
        }

        public static List<string> GetMarks(ObjectId idBtrWindow)
        {
            List<string> marks = new List<string>();
            var blRef = new BlockReference(Point3d.Origin, idBtrWindow);
            var dynParams = blRef.DynamicBlockReferencePropertyCollection;
            if (dynParams != null)
            {
                foreach (DynamicBlockReferenceProperty param in dynParams)
                {
                    if (param.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                    {
                        marks = param.GetAllowedValues().Cast<string>().ToList();
                        break;
                    }
                }
            }
            return marks;
        }

        public static bool IsblockWindow(string blName)
        {
            return Regex.IsMatch(blName, Settings.Default.BlockWindowName, RegexOptions.IgnoreCase);
        }
    }
}
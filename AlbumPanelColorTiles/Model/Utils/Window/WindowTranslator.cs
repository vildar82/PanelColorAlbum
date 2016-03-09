using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Utils.Window
{
    public class WindowTranslator
    {
        public string BlNameOld { get; set; }
        public string Mark { get; set; }        

        public WindowTranslator(string oldBlName, string mark)
        {
            BlNameOld = oldBlName;
            Mark = mark;
        }

        public static WindowTranslator GetAkrBlWinTranslator(BlockReference blRefWindow)
        {
            WindowTranslator res = null;
            foreach (DynamicBlockReferenceProperty prop in blRefWindow.DynamicBlockReferencePropertyCollection)
            {
                if (prop.PropertyName.Equals(Settings.Default.BlockWindowVisibilityName, StringComparison.OrdinalIgnoreCase))
                {
                    //res = new WindowTranslator();
                }
            }
            return res;
        }
    }    
}

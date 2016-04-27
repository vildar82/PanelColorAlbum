using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace AlbumPanelColorTiles.Utils.CopyDict
{
    public class SelectObject
    {
        public string Name { get; set; }
        public object Object { get; set; }

        public SelectObject(object item, string name)
        {
            Name = name;
            Object = item;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

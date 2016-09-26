using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.ColorAreaTable
{
    class ColorArea : BlockBase, IEquatable<ColorArea>, IComparable<ColorArea>
    {
        public int Height { get; set; }
        public int Width { get; set; }        
        public double Area { get; set; }
        public string Size { get; set; }

        public ColorArea (BlockReference blRef, string blName) : base(blRef, blName)
        {
            var ext = Bounds.Value;
            Height = Convert.ToInt32(ext.MaxPoint.Y - ext.MinPoint.Y);
            Width = Convert.ToInt32(ext.MaxPoint.X - ext.MinPoint.X);
            Area = Math.Round( Height * Width * 0.000001, 2);
            Size = $"{Height}x{Width}";
        }

        public bool Equals (ColorArea other)
        {
            var res = BlLayer == other.BlLayer && Height == other.Height && Width == other.Width;
            return res;
        }        

        public int CompareTo (ColorArea other)
        {
            var res = Area.CompareTo(other.Area);
            return res;
        }

        public override int GetHashCode ()
        {
            return BlLayer.GetHashCode() ^ Height.GetHashCode() ^ Width.GetHashCode();
        }
    }
}

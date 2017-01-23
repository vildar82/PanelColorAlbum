using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib.Blocks;
using System.Text.RegularExpressions;

namespace AlbumPanelColorTiles.MountingsPlans
{
    public class Axis : BlockBase
    {
        public Axis(BlockReference blRef, string blName) : base(blRef, blName)
        {
        }

        public static bool IsBlockAxis(string blName)
        {
            // Содержит оси, вокруг которого стоят не буквы. 'Оси_', Но не 'Осина'
            return Regex.IsMatch(blName, @"(?i)(?<=^|[^a-z])оси(?=$|[^a-z])", RegexOptions.IgnoreCase);
        }

        public static Axis Define(BlockReference blRef, string blName)
        {
            var axis = new Axis(blRef, blName);
            return axis;
        }
    }
}

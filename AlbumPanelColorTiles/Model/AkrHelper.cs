using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;

namespace AlbumPanelColorTiles
{
    /// <summary>
    /// Общие вспомогательные функкции АКР
    /// </summary>
    public static class AkrHelper
    {
        public static string GetMarkWithoutWindowsSuffix(string markSB, out string windowSuffix)
        {
            windowSuffix = string.Empty;
            string res = markSB;
            var winIndex = markSB.IndexOf(Settings.Default.WindowPanelSuffix, StringComparison.OrdinalIgnoreCase);
            if (winIndex != -1)
            {
                var winVal = markSB.Substring(winIndex + Settings.Default.WindowPanelSuffix.Length);
                var splitDash = winVal.Split(new char[] { '_' }, 2);
                windowSuffix = splitDash[0];
                res = markSB.Substring(0, winIndex) +
                    markSB.Substring(winIndex + Settings.Default.WindowPanelSuffix.Length + windowSuffix.Length);

            }
            return res;
        }

        public static string GetMarkWithoutElectric(string markSB)
        {
            string res = markSB;
            // "-1э" или в конце строи или перед разделителем "_".
            var matchs = Regex.Matches(markSB, @"-\d{0,2}э($|_)", RegexOptions.IgnoreCase);
            if (matchs.Count == 1)
            {
                int indexAfterElectric = matchs[0].Index + (matchs[0].Value.EndsWith("_") ? matchs[0].Value.Length - 1 : matchs[0].Value.Length);
                res = markSB.Substring(0, matchs[0].Index) + markSB.Substring(indexAfterElectric);
            }
            return res;
        }
    }
}

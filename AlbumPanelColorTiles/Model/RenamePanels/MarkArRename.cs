using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;

namespace AlbumPanelColorTiles.RenamePanels
{
   public class MarkArRename : IComparable<MarkArRename>
   {
      private static string _abbr;
      private bool _isRenamed;
      private MarkArPanel _markAR;
      private string _markArCurFull;
      private string _markPainting;
      private string _markSB;

      public MarkArRename(MarkArPanel markAR)
      {
         _markAR = markAR;
         _markPainting = _markAR.MarkPainting;
         _markSB = _markAR.MarkSB.MarkSbClean;
         _abbr = _markAR.MarkSB.Abbr;
         _markArCurFull = GetMarkArPreview(_markPainting);
      }

      public static string Abbr
      {
         get { return _abbr; }
         set { _abbr = value; }
      }

      public bool IsRenamed { get { return _isRenamed; } }
      public MarkArPanel MarkAR { get { return _markAR; } }
      public string MarkArCurFull { get { return _markArCurFull; } }
      public string MarkPainting { get { return _markPainting; } }

      public static Dictionary<string, MarkArRename> GetMarks(Album album)
      {
         Dictionary<string, MarkArRename> markArRenames = new Dictionary<string, MarkArRename>();
         // Все панели марки АР.
         List<MarkArPanel> marksAR = new List<MarkArPanel>();
         album.MarksSB.ForEach(m => marksAR.AddRange(m.MarksAR));
         foreach (var markAr in marksAR)
         {
            MarkArRename markArRename = new MarkArRename(markAr);
            markArRenames.Add(markArRename.MarkArCurFull, markArRename);
         }
         return markArRenames;
      }

      public int CompareTo(MarkArRename other)
      {
         return _markArCurFull.CompareTo(other._markArCurFull);
      }

      public string GetMarkArPreview(string markPainting)
      {
         return string.Format("{0}({1}_{2})", _markSB, markPainting, _abbr);
      }

      public void RenamePainting(string markPainting)
      {
         _isRenamed = markPainting != _markPainting;
         _markPainting = markPainting;
         _markArCurFull = GetMarkArPreview(_markPainting);
      }
   }
}
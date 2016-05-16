using System;
using System.Collections.Generic;
using AcadLib.Errors;
using AlbumPanelColorTiles.Panels;

namespace AlbumPanelColorTiles.RenamePanels
{
   public class MarkArRename : IComparable<MarkArRename>
   {
      private static string _abbr;
      private bool _isRenamed;
      private MarkAr _markAR;
      private string _markArCurFull;
      private string _markPainting;
      private string _markSB;

      public MarkArRename(MarkAr markAR)
      {
         _markAR = markAR;
         _markPainting = _markAR.MarkPaintingCalulated;
         _markSB = _markAR.MarkSB.MarkSbClean;
         _abbr = _markAR.MarkSB.Abbr;
         _markArCurFull = _markAR.MarkARPanelFullNameCalculated;// GetMarkArPreview(_markPainting);
      }

      public static string Abbr
      {
         get { return _abbr; }
         set { _abbr = value; }
      }

      public bool IsRenamed { get { return _isRenamed; } }
      public MarkAr MarkAR { get { return _markAR; } }
      public string MarkArCurFull { get { return _markArCurFull; } }
      public string MarkPainting { get { return _markPainting; } }

      public static bool ContainsRenamePainting(MarkArRename markArForRename, string newPaintingMark, Dictionary<string, MarkArRename> renameDict)
      {
         string markArNew = markArForRename.GetMarkArPreview(newPaintingMark);
         return renameDict.ContainsKey(markArNew);
      }

      public static Dictionary<string, MarkArRename> GetMarks(Album album)
      {
         Dictionary<string, MarkArRename> markArRenames = new Dictionary<string, MarkArRename>();
         // Все панели марки АР.
         List<MarkAr> marksAR = new List<MarkAr>();
         album.MarksSB.ForEach(m => marksAR.AddRange(m.MarksAR));
         foreach (var markAr in marksAR)
         {
            MarkArRename markArRename = new MarkArRename(markAr);
            //markArRenames.Add(markArRename.MarkArCurFull, markArRename);
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
         if (string.IsNullOrEmpty(_abbr))
         {
            return string.Format("{0}({1})", _markSB, markPainting);
         }
         else
         {
            return string.Format("{0}({1}_{2})", _markSB, markPainting, _abbr);
         }
      }

      // Переименование марки покрааски
      public void RenameMark(string newPaintingMark, Dictionary<string, MarkArRename> marksArForRename)
      {
            try
            {
                marksArForRename.Remove(MarkArCurFull);
                RenamePainting(newPaintingMark);
                marksArForRename.Add(MarkArCurFull, this);
            }
            catch
            {
                Inspector.AddError($"Не удалось переименовать покраску для панели {_markArCurFull}, на покраску {newPaintingMark}");
            }            
      }

      private void RenamePainting(string markPainting)
      {
         _isRenamed = markPainting != _markPainting;
         _markPainting = markPainting;
         _markArCurFull = GetMarkArPreview(_markPainting);
      }
   }
}
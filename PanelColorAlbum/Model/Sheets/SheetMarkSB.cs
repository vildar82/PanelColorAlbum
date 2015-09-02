using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Листы Марки СБ
   public class SheetMarkSB
   {
      // Файл панели Марки СБ с листами Маркок АР.
      List<MarkArPanel> _sheetsMarkAR;
      FileInfo _fileMarkSB;
      FileInfo _fileSheetTemplate;

      public SheetMarkSB (MarkSbPanel markSB, DirectoryInfo albumFolder, FileInfo fileTemplateSheet)
      {
         _fileSheetTemplate = fileTemplateSheet;
         // Создание файла панели Марки СБ и создание в нем листов с панелями Марки АР
         _fileMarkSB = CreateSheetMarkSB(markSB, albumFolder);
      }

      // Создание файла Марки СБ
      private FileInfo CreateSheetMarkSB(MarkSbPanel markSB, DirectoryInfo albumFolder)
      {
         return _fileSheetTemplate.CopyTo(Path.Combine(albumFolder.FullName, markSB.MarkSb));
      }
   }
}

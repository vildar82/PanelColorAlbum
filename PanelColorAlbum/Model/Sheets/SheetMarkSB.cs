using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Листы Марки СБ
   public class SheetMarkSB
   {
      // Файл панели Марки СБ с листами Маркок АР.
      MarkSbPanel _markSB;
      List<SheetMarkAr> _sheetsMarkAR;
      string _fileMarkSB;
      string _fileSheetTemplate;      

      public SheetMarkSB(MarkSbPanel markSB, string albumFolder, string fileTemplateSheet)
      {
         _markSB = markSB;
         _sheetsMarkAR = new List<SheetMarkAr>();
         _fileSheetTemplate = fileTemplateSheet;
         // Создание файла панели Марки СБ и создание в нем листов с панелями Марки АР
         _fileMarkSB = CreateSheetMarkSB(_markSB, albumFolder);

         // Создание листов Марок АР         
         using (Database dbMarkSB = new Database(false, true))
         {
            dbMarkSB.ReadDwgFile(_fileMarkSB, FileShare.ReadWrite, false, "");
            dbMarkSB.CloseInput(true);

            // Копирование всех определений блоков марки АР в файл Марки СБ
            CopyBtrMarksARToSheetMarkSB(_markSB, dbMarkSB);

            // Создание листов Марок АР
            Point3d pt = Point3d.Origin;
            foreach (var markAR in markSB.MarksAR)
            {
               SheetMarkAr sheetMarkAR = new SheetMarkAr(markAR, dbMarkSB, pt);               
               _sheetsMarkAR.Add(sheetMarkAR);
               // Точка для вставки следующего блока Марки АР
               pt = new Point3d(pt.X + 15000, pt.Y, 0);
            }

            //// Удаление шаблона листа из фала Марки СБ
            //DeleteTemplateLayout(dbMarkSB);// FatalError

            dbMarkSB.SaveAs(_fileMarkSB, DwgVersion.Current);
         }
      }

      //private void DeleteTemplateLayout(Database dbMarkSB)
      //{         
      //   Database dbOrig = HostApplicationServices.WorkingDatabase;
      //   HostApplicationServices.WorkingDatabase = dbMarkSB;
      //   LayoutManager lm = LayoutManager.Current;
      //   lm.CurrentLayout = _markSB.MarksAR[0].MarkARPanelFullName;
      //   lm.DeleteLayout (Album.Options.SheetTemplateLayoutNameForMarkAR);         
      //   HostApplicationServices.WorkingDatabase = dbOrig;         
      //}

      // Копирование определений блоков Марок АР в чертеж листов Марки СБ.
      private void CopyBtrMarksARToSheetMarkSB(MarkSbPanel markSB, Database dbMarkSB)
      {
         Database dbSource = markSB.IdBtr.Database;
         var idsCopy = new ObjectIdCollection();
         foreach (var markAr in markSB.MarksAR)
         {
            idsCopy.Add(markAr.IdBtrAr);
         }
         IdMapping map = new IdMapping();
         dbSource.WblockCloneObjects(idsCopy, dbMarkSB.BlockTableId, map, DuplicateRecordCloning.Replace, false);
      }

      // Создание файла Марки СБ
      private string CreateSheetMarkSB(MarkSbPanel markSB, string  albumFolder)
      {
         string fileDest = Path.Combine(albumFolder, markSB.MarkSb + ".dwg");
         File.Copy(_fileSheetTemplate, fileDest);
         return fileDest;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Panels.AlbumInfos;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Panels
{
   // Инфо по альбому - заполнение рамки, обложки, титульного блока
   public class AlbumInfo
   {
      private Document doc;
      private Database db;      

      public FrameSheet Frame { get; set; } // Основная рамка
      public CoverAndTitle CoverTitle { get; set; } // Обложка и тит блок

      public AlbumInfo ()
      {
         Document doc = Application.DocumentManager.MdiActiveDocument;
         Database db = doc.Database;
      }

      public void Search()
      {
         // Поиск блока рамки на текущем чертеже фасада
         using (var bt = db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            // Определение блоков оформления
            ObjectId idBtrFrame = getBtr(Settings.Default.BlockFrameName, bt);
            ObjectId idBtrCover = getBtr(Settings.Default.BlockCoverName, bt);
            ObjectId idBtrTitle = getBtr(Settings.Default.BlockTitleName, bt);
            ObjectId msId = bt[BlockTableRecord.ModelSpace];

            // Рамка
            if (!idBtrFrame.IsNull)
            {
               var blRefFrame = getFirstBlRefInModel(idBtrFrame, msId);
               Frame = new FrameSheet();
               Frame.Check(blRefFrame);
            }
            // Титул
            if (!idBtrCover.IsNull)
            {
               var blRefCover = getFirstBlRefInModel(idBtrCover, msId);
               CoverTitle = new CoverAndTitle();
               CoverTitle.Check(blRefCover);
            }
         }
      }      

      private ObjectId getBtr(string name, BlockTable bt)
      {
         if (bt.Has(name))
         {
            return bt[name];
         }
         else
         {
            doc.Editor.WriteMessage("Не найден блок оформления {0}", name);
            return ObjectId.Null;
         }
      }

      private BlockReference getFirstBlRefInModel(ObjectId idBtrFrame, ObjectId msId)
      {
         using (var btr = idBtrFrame.Open( OpenMode.ForRead) as BlockTableRecord)
         {
            var idsBlRef = btr.GetBlockReferenceIds(true, false);
            foreach (ObjectId idBlRef in idsBlRef)
            {
               using (var blRef = idBlRef.Open( OpenMode.ForRead, false, true) as BlockReference)
               {
                  if (blRef.OwnerId == msId)
                  {
                     return blRef;
                  }
               }
            }
         }
         return null;
      }
   }
}

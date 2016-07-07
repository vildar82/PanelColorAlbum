using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Panels.AlbumInfos;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Panels
{
   // Инфо по альбому - заполнение рамки, обложки, титульного блока
   public class AlbumInfo
   {
      private Document doc;
      private Database db;      

      public FrameSheet Frame { get; set; } // Основная рамка
      public CoverAndTitle CoverTitle { get; set; } // Обложка и тит блок
      public ProfileTile ProfileTile { get; set;} // Профиль для плитки в торцах

      public AlbumInfo ()
      {
         doc = Application.DocumentManager.MdiActiveDocument;
         db = doc.Database;
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
            ObjectId idBtrProfileTile = getBtr(Settings.Default.BlockProfileTile, bt);
            ObjectId msId = bt[BlockTableRecord.ModelSpace];

            // Рамка
            Frame = new FrameSheet();
            if (!idBtrFrame.IsNull)
            {
               ObjectId idBlRefFrame = getFirstBlRefInModel(idBtrFrame, msId);               
               Frame.Check(idBlRefFrame);
            }
            // Обложка
            CoverTitle = new CoverAndTitle();
            if (!idBtrCover.IsNull)
            {
               ObjectId idBlRefCover = getFirstBlRefInModel(idBtrCover, msId);               
               CoverTitle.CheckCover(idBlRefCover);
            }
            // Титул            
            if (!idBtrTitle.IsNull)
            {
               ObjectId idBlRefTitle = getFirstBlRefInModel(idBtrTitle, msId);
               CoverTitle.CheckTitle(idBlRefTitle);
            }
            // Профиль для торцов плитки
            if (!idBtrProfileTile.IsNull)
            {
               ObjectId idBlRefProfileTile = getFirstBlRefInModel(idBtrProfileTile, msId);
               ProfileTile = new ProfileTile(idBlRefProfileTile);
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

      private ObjectId getFirstBlRefInModel(ObjectId idBtrFrame, ObjectId msId)
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
                     return idBlRef;
                  }
               }
            }
         }
         return ObjectId.Null;
      }
   }
}

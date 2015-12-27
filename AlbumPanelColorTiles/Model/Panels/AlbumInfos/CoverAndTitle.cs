using System;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Panels.AlbumInfos
{
   // Обложека и титульный лист
   public class CoverAndTitle
   {
      public ObjectId IdCoverBtr { get; private set; }
      public ObjectId IdTitleBtr { get; private set; }
      public bool IsOkCover { get; private set; }
      public bool IsOkTitle { get; private set; }

      public void ChangeCoverAndTitle(Database db)
      {
         // Замена блоков обложки и титульного лисчта
         if (!IsOkCover && !IsOkTitle)
         {
            return;
         }

         var ids = new ObjectIdCollection();
         if (IsOkCover)
         {
            ids.Add(IdCoverBtr);
         }
         if (IsOkTitle)
         {
            ids.Add(IdTitleBtr);
         }
         IdMapping iMap = new IdMapping();
         db.WblockCloneObjects(ids, db.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
      }

      public void CheckCover(ObjectId idBlRefCover)
      {
         bool isOk;
         IdCoverBtr = checkBlock(idBlRefCover, "Не найден блок {0}".f(Settings.Default.BlockCoverName), out isOk);
         IsOkCover = isOk;
      }

      public void CheckTitle(ObjectId idBlRefTitle)
      {
         bool isOk;
         IdTitleBtr = checkBlock(idBlRefTitle, "Не найден блок {0}".f(Settings.Default.BlockTitleName), out isOk);
         IsOkTitle = isOk;
      }

      private ObjectId checkBlock(ObjectId idBlRef, string errMsg, out bool isOk)
      {
         if (idBlRef.IsNull)
         {
            Inspector.AddError(errMsg);
            isOk = false;
            return ObjectId.Null;
         }
         using (var blRefCover = idBlRef.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            isOk = true;
            return blRefCover.BlockTableRecord;
         }
      }
   }
}
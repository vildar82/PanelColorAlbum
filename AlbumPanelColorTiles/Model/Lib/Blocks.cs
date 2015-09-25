using System;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Lib
{
   public static class Blocks
   {
      #region Public Methods

      /// <summary>
      /// Смена имен листов. Лист с с именем name1 станет name2, и наоборот.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="name1"></param>
      /// <param name="name2"></param>
      public static void ConvertLayoutNames(Database db, string name1, string name2)
      {
         Database dbOrig = HostApplicationServices.WorkingDatabase;
         HostApplicationServices.WorkingDatabase = db;
         LayoutManager lm = LayoutManager.Current;
         string tempName1 = name1 + name1;
         lm.RenameLayout(name1, tempName1);
         lm.RenameLayout(name2, name1);
         lm.RenameLayout(tempName1, name2);
         HostApplicationServices.WorkingDatabase = dbOrig;
      }

      /// <summary>
      /// Копирование определенич блока из внешнего чертежа
      /// </summary>
      /// <param name="blName">Имя блока</param>
      /// <param name="fileDrawing">Полный путь к чертежу из которого копируется блок</param>
      /// <param name="destDb">База чертежа в который копируетсяя блок</param>
      /// <exception cref="Exception">Если нет блока в файле fileDrawing.</exception>
      public static void CopyBlockFromExternalDrawing(string blName, string fileDrawing, Database destDb)
      {
         using (var extDb = new Database(false, true))
         {
            extDb.ReadDwgFile(fileDrawing, System.IO.FileShare.ReadWrite, true, "");

            ObjectIdCollection ids = new ObjectIdCollection();
            using (var t = extDb.TransactionManager.StartTransaction())
            {
               var bt = (BlockTable)t.GetObject(extDb.BlockTableId, OpenMode.ForRead);
               if (bt.Has(blName))
               {
                  ids.Add(bt[blName]);
               }
               else
               {
                  throw new Exception(string.Format("Не найдено определение блока {0} в файле {1}", blName, fileDrawing));
               }
               t.Commit();
            }
            // Если нашли – добавим блок
            if (ids.Count != 0)
            {
               // Получаем текущую базу чертежа
               IdMapping iMap = new IdMapping();
               destDb.WblockCloneObjects(ids, destDb.BlockTableId, iMap, DuplicateRecordCloning.Ignore, false);
            }
         }
      }

      /// <summary>
      /// Копирование определения блока и добавление его в таблицу блоков.
      /// </summary>
      /// <param name="idBtr">Копируемый блок</param>
      /// <param name="name">Имя для нового блока</param>
      /// <returns>ID скопированного блока, или существующего в чертеже с таким именем.</returns>
      public static ObjectId CopyBtr(ObjectId idBtr, string name)
      {
         ObjectId idBtrCopy = ObjectId.Null;
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            var btrSource = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            //проверка имени блока
            if (bt.Has(name))
            {
               idBtrCopy = bt[name];
            }
            else
            {
               var btrCopy = btrSource.Clone() as BlockTableRecord;
               btrCopy.Name = name;
               bt.UpgradeOpen();
               idBtrCopy = bt.Add(btrCopy);
               t.AddNewlyCreatedDBObject(btrCopy, true);
               // Копирование объектов блока
               ObjectIdCollection ids = new ObjectIdCollection();
               foreach (ObjectId idEnt in btrSource)
               {
                  ids.Add(idEnt);
               }
               IdMapping map = new IdMapping();
               db.DeepCloneObjects(ids, idBtrCopy, map, false);
            }
            t.Commit();
         }
         return idBtrCopy;
      }

      /// <summary>
      /// Копирование листа
      /// </summary>
      /// <returns>ID Layout</returns>
      public static ObjectId CopyLayout(Database db, string layerSource, string layerCopy)
      {
         ObjectId idLayoutCopy = ObjectId.Null;
         Database dbOrig = HostApplicationServices.WorkingDatabase;
         HostApplicationServices.WorkingDatabase = db;
         LayoutManager lm = LayoutManager.Current;
         // Нужно проверить имена. Вдруг нет листа источника, или уже есть копируемый лист.
         lm.CopyLayout(layerSource, layerCopy);
         idLayoutCopy = lm.GetLayoutId(layerCopy);
         HostApplicationServices.WorkingDatabase = dbOrig;
         return idLayoutCopy;
      }

      public static string EffectiveName(BlockReference blRef)
      {
         using (var btr = blRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            return btr.Name;
         }
      }

      /// <summary>
      /// Получение валидной строки для имени блока. С замоной всех ненужных символов на .
      /// </summary>
      /// <param name="name">Имя для блока</param>
      /// <returns>Валидная строка имени</returns>
      public static string GetValidNameForBlock(string name)
      {
         string res = name;
         //string testString = "<>/?\";:*|,='";
         Regex pattern = new Regex("[<>/?\";:*|,=']");
         res = pattern.Replace(name, ".");
         res = res.Replace('\\', '.');
         return res;
      }

      /// <summary>
      /// Проверка дублирования вхождений блоков
      /// </summary>
      /// <param name="blk1"></param>
      /// <param name="blk2"></param>
      /// <returns></returns>
      public static bool IsDuplicate(this BlockReference blk1, BlockReference blk2)
      {
         Tolerance tol = new Tolerance(1e-6, 1e-6);
         return
             blk1.OwnerId == blk2.OwnerId &&
             blk1.Name == blk2.Name &&
             blk1.Layer == blk2.Layer &&
             Math.Round(blk1.Rotation, 5) == Math.Round(blk2.Rotation, 5) &&
             blk1.Position.IsEqualTo(blk2.Position, tol) &&
             blk1.ScaleFactors.IsEqualTo(blk2.ScaleFactors, tol);
      }

      #endregion Public Methods
   }
}
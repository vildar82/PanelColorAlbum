using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.Lib
{
   public static class Blocks
   {
      public static string EffectiveName (BlockReference blRef)
      {
         using (var btr = blRef.DynamicBlockTableRecord.GetObject( OpenMode.ForRead) as BlockTableRecord)
         {
            return btr.Name;
         }
      }

      /// <summary>
      /// Копирование определения блока и добавление его в таблицу блоков.
      /// </summary>
      /// <param name="idBtr">Копируемый блок</param>
      /// <param name="name">Имя для нового блока</param>
      /// <returns>Новый блок</returns>
      public static ObjectId CopyBtr(ObjectId idBtr, string name)
      {
         ObjectId idBtrCopy = ObjectId.Null;
         Database db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {            
            var btrSource = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;            
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                        
            var btrCopy = btrSource.Clone() as BlockTableRecord;
            //TODO: проверка имени блока
            btrCopy.Name = name;
            idBtrCopy = bt.Add(btrCopy);
            t.AddNewlyCreatedDBObject(btrCopy, true);
            // Копирование объектов блока
            ObjectIdCollection ids = new ObjectIdCollection();
            foreach (ObjectId idEnt in btrSource)
            {
               ids.Add(idEnt);
            }
            IdMapping map = new IdMapping();
            db.DeepCloneObjects(ids, idBtrCopy, map, true);
            t.Commit();
         }
         return idBtrCopy;
      }

      /// <summary>
      /// Проверка дублирования вхождений блоков
      /// </summary>
      /// <param name="blk1"></param>
      /// <param name="blk2"></param>
      /// <returns></returns>
      public static bool Duplicate(this BlockReference blk1, BlockReference blk2)
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
   }   
}

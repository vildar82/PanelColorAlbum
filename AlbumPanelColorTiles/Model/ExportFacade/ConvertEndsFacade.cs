using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   /// <summary>
   /// преобразование торцов фасадов
   /// </summary>
   public class ConvertEndsFacade
   {
      // скопировать торцы из блоков панелей в блок торцов и вставить в тоже место на чертеже что они были в блоках панелей
      private IGrouping<double, PanelBlRefExport> itemLefEndsByY;
      private string sideName;
      private bool isLeftSide;
      public string BlNameEnd { get; private set; }
      public Point3d Position { get; private set; }
      public ConvertPanelService CPS;
      public ObjectId IdBtrEnd { get; set; }
      public bool IsExistsBlockEnd { get; private set; }

      public ConvertEndsFacade(IGrouping<double, PanelBlRefExport> itemLefEndsByY, bool isLeftSide, ConvertPanelService cps)
      {
         this.itemLefEndsByY = itemLefEndsByY;
         this.sideName = isLeftSide ? "левый" : "правый";
         this.isLeftSide = isLeftSide;
         CPS = cps;
      }

      public void Convert()
      {
         // Конвертирование торцов панелей в одном столбце - в отдельный блок торца
         // Определение имя для блока торца
         BlNameEnd = defBlNameEnd();

         // определение точки вставки для блока торца
         Position = defPosition();

         // копирование объектов торца в блок торца
         createBlock();         
      }

      private void deleteEndInPanels()
      {
         var panelsBtr = itemLefEndsByY.Select(p => p.PanelBtrExport);

         foreach (var panelBtr in panelsBtr)
         {
            panelBtr.DeleteEnd(isLeftSide);
         }         
      }

      private Point3d defPosition()
      {
         // Определение точки вставки блока торца
         Point3d resVal;
         // самая нижняя панель в столбце панелей с торцами
         var panelLower = this.itemLefEndsByY.OrderBy(p => p.Position.Y).First();
         if (isLeftSide)
         {
            resVal = panelLower.PanelBtrExport.ExtentsByTile.MinPoint.TransformBy(panelLower.Transform);
         }
         else
         {
            resVal = new Point3d(panelLower.PanelBtrExport.ExtentsByTile.MaxPoint.X - 300,
                                 panelLower.PanelBtrExport.ExtentsByTile.MinPoint.Y, 0).TransformBy(panelLower.Transform);
         }
         return resVal;
      }

      private string defBlNameEnd()
      {         
         string nameEnd;
         // если все панели имеют один фасад, то имя торца по имени осей фасада
         var facades = itemLefEndsByY.Select(p=>p.Facade);
         if (facades.Count()==1 && facades.First()!=null)
         {
            nameEnd = facades.First().Name;
         }
         else
         {
            // имя блока торца по координате Y
            nameEnd = itemLefEndsByY.Key.ToString("0.0");
         }
         return string.Format("АКР_Торец_{0}_{1}", nameEnd, sideName);
      }

      private void createBlock()
      {
         // создание определения блока
         using (var bt = CPS.DbExport.BlockTableId.GetObject( OpenMode.ForRead) as BlockTable)
         {
            IdBtrEnd = getIdBtrEnd(bt);
         }

         // для каждой панели - копирование объектв торца с преобразование в координаты модели  
         // список копируемых объектов торуа с привязкой к объекту блока панели для дальнейшего перемещения объектов в 0,0 в блоке торца                                  
         foreach (var panelBlRef in itemLefEndsByY)
         {
            Dictionary<ObjectId, PanelBlRefExport> dictIdsEndEnts = new Dictionary<ObjectId, PanelBlRefExport>();
            if (isLeftSide)
            {
               panelBlRef.PanelBtrExport.IdsEndsLeftEntity.ForEach(e => dictIdsEndEnts.Add(e, panelBlRef));
            }
            else
            {
               panelBlRef.PanelBtrExport.IdsEndsRightEntity.ForEach(e => dictIdsEndEnts.Add(e, panelBlRef));
            }
            ObjectIdCollection ids = new ObjectIdCollection(dictIdsEndEnts.Keys.ToArray());
            IdMapping mapping = new IdMapping();
            CPS.DbExport.DeepCloneObjects(ids, IdBtrEnd, mapping, false);

            //перемещение объектов в блоке
            var moveMatrix = Matrix3d.Displacement(new Vector3d(0, panelBlRef.Position.Y - Position.Y, 0));
            foreach (IdPair itemMap in mapping)
            {
               using (var ent = itemMap.Value.GetObject(OpenMode.ForWrite, false, true) as Entity)
               {
                  ent.TransformBy(moveMatrix);
               }
            }
         }

         // перемещение вех объектов торца в 0
         var btr = IdBtrEnd.GetObject(OpenMode.ForRead) as BlockTableRecord;
         Extents3d extFull = new Extents3d();
         extFull.AddBlockExtents(btr);
         foreach (ObjectId idEnt in btr)
         {
            using (var ent = idEnt.GetObject(OpenMode.ForWrite, false, true) as Entity)
            {
               ent.TransformBy(Matrix3d.Displacement(new Vector3d(-extFull.MinPoint.X, 0, 0)));
            }
         }

         ////сопоставление скопированных объектов с панелями
         //Dictionary<ObjectId, PanelBlRefExport> dictIdsCopyedEndEnts = new Dictionary<ObjectId, PanelBlRefExport>();
         //foreach (IdPair itemIdMap in mapping)
         //{
         //   var panelBlRef = dictIdsEndEnts[itemIdMap.Key];
         //   dictIdsCopyedEndEnts.Add(itemIdMap.Key, panelBlRef);
         //}


         //// удаление выбранных объектов
         //foreach (ObjectId idEnt in ids)
         //{
         //   var ent = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as Entity;
         //   ent.Erase();
         //}

         // вставка блока
         if (!IsExistsBlockEnd)
         {
            using (var blRef = new BlockReference(Position, IdBtrEnd))
            {
               blRef.SetDatabaseDefaults(CPS.DbExport);
               using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(CPS.DbExport).GetObject(OpenMode.ForWrite) as BlockTableRecord)
               {
                  ms.AppendEntity(blRef);
                  ms.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(blRef, true);
               }
            }
         }
      }

      public void DeleteEnds()
      {
         deleteEndInPanels();
      }

      private ObjectId getIdBtrEnd(BlockTable bt)
      {
         ObjectId idBtrEnd;
         if (bt.Has(BlNameEnd))
         {
            // отредактировать существующий блок торца - удалтить все старые объекты
            idBtrEnd = bt[BlNameEnd];
            eraseEntInBlock(idBtrEnd);
            IsExistsBlockEnd = true;
         }
         else
         {
            using (var btr = new BlockTableRecord())
            {
               btr.Name = BlNameEnd;
               bt.UpgradeOpen();
               idBtrEnd = bt.Add(btr);
               bt.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(btr, true);
            }
         }
         return idBtrEnd;
      }

      private void eraseEntInBlock(ObjectId idBtr)
      {
         using (var btr = idBtr.GetObject( OpenMode.ForWrite) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in btr)
            {
               using (var ent = idEnt.GetObject( OpenMode.ForWrite, false, true)as Entity)
               {
                  ent.Erase();
               }
            }
         }
      }
   }
}

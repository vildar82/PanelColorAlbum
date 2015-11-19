using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class MountingPlans
   {
      private Database _db;
      private Document _doc;
      private Editor _ed;
      private string _section;

      public MountingPlans()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _ed = _doc.Editor;
         _db = _doc.Database;
         _section = string.Empty;
      }

      // создание блоков монтажных планов из выбранных планов монтажек пользователем
      public void CreateMountingPlans()
      {
         int numberFloor = 2;
         _ed.WriteMessage("\nКоманда создания блоков монтажных планов вида АКР_Монтажка_2.");

         createFloor(numberFloor);
      }

      // проверка наличия блока монтажки этого этажа
      private bool checkBlock(string floorBlockName)
      {
         bool skipOrRedefine = false; // true - skip, false - нет такого блока, можно создавать
         using (var bt = _db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            if (bt.Has(floorBlockName))
            {
               var prOpt = new PromptKeywordOptions(string.Format("Блок монтажки {0} уже определен в чертеже. Что делать?", floorBlockName));
               prOpt.Keywords.Add("Выход");
               prOpt.Keywords.Add("Пропустить");
               prOpt.Keywords.Default = "Выход";

               var res = _ed.GetKeywords(prOpt);

               if (res.Status == PromptStatus.OK)
               {
                  switch (res.StringResult)
                  {
                     case "Выход":
                        throw new Exception("\nОтменено пользователем.");
                     case "Пропустить":
                        skipOrRedefine = true;
                        break;

                     default:
                        throw new Exception("\nОтменено пользователем.");
                  }
               }
               else
               {
                  throw new Exception("\nОтменено пользователем.");
               }
            }
         }
         return skipOrRedefine;
      }

      // создаение блока монтажки
      private void createBlock(List<ObjectId> idsFloor, string floorBlockName)
      {
         Point3d location = getPoint(string.Format("Точка вставки блока монтажного плана {0}", floorBlockName)).TransformBy(_ed.CurrentUserCoordinateSystem);
         using (var t = _db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(_db.BlockTableId, OpenMode.ForWrite) as BlockTable;
            ObjectId idBtr;
            BlockTableRecord btr;
            // создание определения блока
            using (btr = new BlockTableRecord())
            {
               btr.Name = floorBlockName;
               idBtr = bt.Add(btr);
               t.AddNewlyCreatedDBObject(btr, true);
            }
            // копирование выбранных объектов в блок
            ObjectIdCollection ids = new ObjectIdCollection(idsFloor.ToArray());
            IdMapping mapping = new IdMapping();
            _db.DeepCloneObjects(ids, idBtr, mapping, false);

            // перемещение объектов в блоке
            btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
            var moveMatrix = Matrix3d.Displacement(Point3d.Origin - location);
            foreach (ObjectId idEnt in btr)
            {
               var ent = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as Entity;
               ent.TransformBy(moveMatrix);
            }

            // удаление выбранных объектов
            foreach (ObjectId idEnt in ids)
            {
               var ent = t.GetObject(idEnt, OpenMode.ForWrite, false, true) as Entity;
               ent.Erase();
            }

            // вставка блока
            using (var blRef = new BlockReference(location, idBtr))
            {
               blRef.SetDatabaseDefaults(_db);
               var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
               ms.AppendEntity(blRef);
               t.AddNewlyCreatedDBObject(blRef, true);
            }
            t.Commit();
         }
      }

      private void createFloor(int numberFloor)
      {
         // запрос номера этажа
         numberFloor = getNumberFloor(numberFloor);
         // проверка наличия блока монтажки с этим номером
         string floorBlockName;
         if (string.IsNullOrEmpty(_section))
         {
            floorBlockName = string.Format("{0}эт-{1}", Settings.Default.BlockMountingPlanePrefixName, numberFloor);
         }
         else
         {
            floorBlockName = string.Format("{0}С{1}_эт-{2}", Settings.Default.BlockMountingPlanePrefixName, _section, numberFloor);
         }
         if (!checkBlock(floorBlockName))
         {
            // запрос объектов плана этажа
            var idsFloor = selectFloor(numberFloor);
            createBlock(idsFloor, floorBlockName);
         }
         // создание следующего этажа
         createFloor(++numberFloor);
      }

      // Запрос номера этажа
      private int getNumberFloor(int defaultNumber)
      {
         var opt = new PromptIntegerOptions("\nВведи номер этажа монтажного плана");
         opt.DefaultValue = defaultNumber;
         opt.Keywords.Add("Секция" + _section);
         var res = _ed.GetInteger(opt);
         if (res.Status == PromptStatus.OK)
         {
            return res.Value;
         }
         else if (res.Status == PromptStatus.Keyword)
         {
            _section = getSection();
            return getNumberFloor(defaultNumber);
         }
         else
         {
            throw new Exception("\nОтменено пользователем.");
         }
      }

      private Point3d getPoint(string msg)
      {
         var res = _ed.GetPoint(msg);
         if (res.Status == PromptStatus.OK)
         {
            return res.Value;
         }
         else
         {
            throw new Exception("\nОтменено пользователем.");
         }
      }

      private string getSection()
      {
         string resSection = string.Empty;
         var opt = new PromptStringOptions("Номер секции");
         opt.DefaultValue = string.IsNullOrEmpty(_section) ? "1" : _section;
         var res = _ed.GetString(opt);
         if (res.Status == PromptStatus.OK)
         {
            resSection = res.StringResult;
         }
         return resSection;
      }

      // запрос выбора объектов этажа
      private List<ObjectId> selectFloor(int numberFloor)
      {
         var selOpt = new PromptSelectionOptions();
         selOpt.MessageForAdding = string.Format("\nВыбор объектов монтажного плана {0} этажа", numberFloor);
         var selRes = _ed.GetSelection(selOpt);
         if (selRes.Status == PromptStatus.OK)
         {
            return selRes.Value.GetObjectIds().ToList();
         }
         else
         {
            throw new Exception("\nОтменено пользователем.");
         }
      }
   }
}
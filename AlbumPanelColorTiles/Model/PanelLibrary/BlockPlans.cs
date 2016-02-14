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
   public enum BlockPlanTypeEnum
   {
      Mounting,
      Architect
   }

   public class BlockPlans
   {
      private const string BLOCKMOUNTING = "Монтажного";
      private const string BLOCKARCHITECT = "Архитектурного";

      private Database _db;
      private Document _doc;
      private Editor _ed;
      private string _nameFloor;
      private int _numberFloor;
      private string _section;
      private BlockPlanTypeEnum _planType;
      private string _planTypeName;
      private string _prefixBlockName;

      public BlockPlans()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _ed = _doc.Editor;
         _db = _doc.Database;
         _section = string.Empty;
         setTypeBlock( BlockPlanTypeEnum.Mounting);
      }

      private void setTypeBlock(BlockPlanTypeEnum type)
      {
         _planType = type;
         if (type == BlockPlanTypeEnum.Mounting)
         {
            _planTypeName = BLOCKMOUNTING;
            _prefixBlockName = Settings.Default.BlockPlaneMountingPrefixName;
         }
         else
         {
            _planTypeName = BLOCKARCHITECT;
            _prefixBlockName = Settings.Default.BlockPlaneArchitectPrefixName;
         }         
      }

      // создание блоков монтажных планов из выбранных планов монтажек пользователем
      public void CreateBlockPlans()
      {
         _numberFloor = 2;
         _ed.WriteMessage($"\nКоманда создания блока блоков монтажных и архитектурных планов.");
         createFloor();
      }     

      // проверка наличия блока монтажки этого этажа
      private bool checkBlock(string floorBlockName)
      {
         bool skipOrRedefine = false; // true - skip, false - нет такого блока, можно создавать
         using (var bt = _db.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            if (bt.Has(floorBlockName))
            {
               var prOpt = new PromptKeywordOptions($"Блок плана {floorBlockName} уже определен в чертеже. Что делать?");
               prOpt.Keywords.Add("Выход");
               prOpt.Keywords.Add("Пропустить");
               prOpt.Keywords.Default = "Выход";

               var res = _ed.GetKeywords(prOpt);

               if (res.Status == PromptStatus.OK)
               {
                  switch (res.StringResult)
                  {
                     case "Выход":
                        throw new Exception("Отменено пользователем.");
                     case "Пропустить":
                        skipOrRedefine = true;
                        break;

                     default:
                        throw new Exception("Отменено пользователем.");
                  }
               }
               else
               {
                  throw new Exception("Отменено пользователем.");
               }
            }
         }
         return skipOrRedefine;
      }

      // создаение блока монтажки
      private void createBlock(List<ObjectId> idsFloor, string floorBlockName)
      {
         Point3d location = getPoint($"Точка вставки блока {_planTypeName} плана {floorBlockName}").TransformBy(_ed.CurrentUserCoordinateSystem);
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
            using (IdMapping mapping = new IdMapping())
            {
               _db.DeepCloneObjects(ids, idBtr, mapping, false);
            }
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

      private void createFloor()
      {
         // запрос номера этажа
         getNumberFloor();
         // проверка наличия блока монтажки с этим номером
         string indexFloor;
         if (string.IsNullOrEmpty(_nameFloor))
         {
            indexFloor = _numberFloor.ToString();
            _numberFloor++;
         }
         else
         {
            indexFloor = _nameFloor;
         }
         string floorBlockName;
         if (string.IsNullOrEmpty(_section))
         {
            floorBlockName = $"{_prefixBlockName}эт-{indexFloor}";
         }
         else
         {
            floorBlockName = $"{_prefixBlockName}С{_section}_эт-{indexFloor}";
         }
         if (!checkBlock(floorBlockName))
         {
            // запрос объектов плана этажа
            var idsFloor = selectFloor(indexFloor);
            createBlock(idsFloor, floorBlockName);
         }
         // создание следующего этажа
         createFloor();
      }

      // Запрос номера этажа
      private void getNumberFloor()
      {
         var opt = new PromptIntegerOptions($"\nВведи номер этажа {_planTypeName} плана");
         opt.DefaultValue = _numberFloor;
         opt.Keywords.Add(_planTypeName);
         string keySection = "Секция" + _section;
         opt.Keywords.Add(keySection);
         opt.Keywords.Add("Чердак");
         opt.Keywords.Add("Парапет");
         var res = _ed.GetInteger(opt);
         if (res.Status == PromptStatus.OK)
         {
            _numberFloor = res.Value;
            _nameFloor = null;
         }
         else if (res.Status == PromptStatus.Keyword)
         {
            if (res.StringResult == keySection)
            {
               _section = getSection();
               getNumberFloor();
            }
            else if (res.StringResult == "Чердак")
            {
               _nameFloor = Settings.Default.PaintIndexUpperStorey;
            }
            else if (res.StringResult == "Парапет")
            {
               _nameFloor = Settings.Default.PaintIndexParapet;
            }
            else if (res.StringResult == _planTypeName)
            {
               setTypeBlock(_planType == BlockPlanTypeEnum.Mounting ? BlockPlanTypeEnum.Architect : BlockPlanTypeEnum.Mounting);               
               getNumberFloor();
            }
            else
            {
               throw new Exception("Отменено пользователем.");
            }
         }
         else
         {
            throw new Exception("Отменено пользователем.");
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
            throw new Exception("Отменено пользователем.");
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
      private List<ObjectId> selectFloor(string indexFloor)
      {
         var selOpt = new PromptSelectionOptions();
         selOpt.MessageForAdding = $"\nВыбор объектов {_planTypeName} плана {indexFloor} этажа";
         var selRes = _ed.GetSelection(selOpt);
         if (selRes.Status == PromptStatus.OK)
         {
            return selRes.Value.GetObjectIds().ToList();
         }
         else
         {
            throw new Exception("Отменено пользователем.");
         }
      }
   }
}
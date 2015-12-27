﻿using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Этаж - блоки АКР-панелей этажа и связаннный с ним блок монтажки с блоком обозначения стороны фасада
   public class Floor : IComparable<Floor>
   {
      public List<MountingPanel> AllPanelsSbInFloor { get; private set; }
      public string BlRefName { get; private set; }
      public FacadeFrontBlock FacadeFrontBlock { get; private set; }
      public double Height { get; set; }
      public ObjectId IdBlRefMounting { get; private set; }
      public PanelLibraryLoadService LibLoadServ { get; private set; }
      public List<MountingPanel> PanelsSbInFront { get; private set; }
      public Storey Storey { get; private set; }
      public double XMax { get; private set; }
      public double XMin { get; private set; }

      public Floor(BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
      {
         LibLoadServ = libLoadServ;
         IdBlRefMounting = blRefMounting.Id;
         BlRefName = blRefMounting.Name;

         //defFloorNameAndNumber(blRefMounting);
         
         //// добавление блоков паненлей в общий список панелей СБ
         //libLoadServ.AllPanelsSB.AddRange(_allPanelsSbInFloor);
      }

      // мин значение х среди всех границ блоков панелей внктри этажа

      /// <summary>
      /// Поиск всех блоков монтажек в модели
      /// </summary>
      public static List<Floor> GetMountingBlocks(PanelLibraryLoadService libLoadServ)
      {
         List<Floor> floors = new List<Floor>();
         var db = HostApplicationServices.WorkingDatabase;
         using (var t = db.TransactionManager.StartTransaction())
         {
            // Найдем все блоки обозначения фасада
            List<FacadeFrontBlock> facadeFrontBlocks = FacadeFrontBlock.GetFacadeFrontBlocks();
            // Дерево прямоугольников от блоков обозначений сторон фасада, для поиска пересечений с
            // блоками монтажек
            RTreeLib.RTree<FacadeFrontBlock> rtreeFront = new RTreeLib.RTree<FacadeFrontBlock>();
            foreach (var front in facadeFrontBlocks)
            {
               rtreeFront.Add(front.RectangleRTree, front);
            }

            // Найти блоки монтажек пересекающиеся с блоками обозначения стороны фасада
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead) as BlockTableRecord;
            //// Поиск панелейСБ в Модели и добавление в общий список панелей СБ.
            //libLoadServ.AllPanelsSB.AddRange(PanelSB.GetPanels(ms.Id, Point3d.Origin, Matrix3d.Identity));
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead, false, true) as BlockReference;

                  // Если это блок монтажного плана - имя блока начинается с АКР_Монтажка_
                  if (blRefMounting.Name.StartsWith(Settings.Default.BlockMountingPlanePrefixName, StringComparison.CurrentCultureIgnoreCase))
                  {
                     Floor floor = new Floor(blRefMounting, libLoadServ);
                     floor.GetAllPanels();
                     // найти соотв обозн стороны фасада
                     var frontsIntersects = rtreeFront.Intersects(ColorArea.GetRectangleRTree(blRefMounting.GeometricExtents));

                     // если нет пересечений фасадов - пропускаем блок монтажки - он не входит в
                     // фасады, просто так вставлен
                     if (frontsIntersects.Count == 0)
                     {
                        continue;
                     }
                     // если больше одного пересечения с фасадами, то это ошибка, на одной монтажке
                     // должен быть один блок обозначения стороны фасада
                     else if (frontsIntersects.Count > 1)
                     {
                        Inspector.AddError("На монтажном плане не должно быть больше одного блока обозначения фасада.", blRefMounting);
                     }
                     else
                     {
                        floor.SetFacadeFrontBlock(frontsIntersects[0]);
                        floors.Add(floor);
                     }
                  }
               }
            }
            t.Commit();
         }
         return floors;
      }

      private void GetAllPanels()
      {
         // Получение всех блоков панелей СБ из блока монтажки
         using (var btr = this._idBtrMounting.GetObject(OpenMode.ForRead) as BlockTableRecord)
         {
            _allPanelsSbInFloor = MountingPanel.GetPanels(btr, _ptBlMounting, _transformMounting, LibLoadServ);
         }
         _xmin = getXMinFloor();
         _xmax = getXMaxFloor();
      }

      public int CompareTo(Floor other)
      {
         return Storey.CompareTo(other.Storey);
      }

      public void DefineStorey(List<Storey> storeysNumbersTypeInAllFacades)
      {
         var indexFloor = BlRefName.IndexOf("эт-");
         string nameStorey = string.Empty;
         if (indexFloor == -1)
            nameStorey = BlRefName.Substring(Settings.Default.BlockMountingPlanePrefixName.Length);
         else
            nameStorey = BlRefName.Substring(indexFloor + "эт-".Length);
         try
         {
            var storey = new Storey(nameStorey);
            if (storey.Type == EnumStorey.Number)
            {
               // поиск в общем списке этажей. Номерные этажи всех фасадов должны быть на одном уровне
               var storeyAllFacades = storeysNumbersTypeInAllFacades.Find(s => s.Number == storey.Number);
               if (storeyAllFacades == null)
               {
                  storeysNumbersTypeInAllFacades.Add(storey);
               }
               else
               {
                  storey = storeyAllFacades;
               }
               Height = Settings.Default.FacadeFloorHeight;
            }
            Storey = storey;
         }
         catch (Exception ex)
         {
            // ошибка определения номера этажа монтажки - это не чердак (Ч), не парапет (П), и не
            // просто число
            Inspector.AddError(ex.Message + BlRefName, IdBlRefMounting);
            Log.Error(ex, "Floor - DefineStorey()");
         }
      }

      private double getXMaxFloor()
      {
         if (AllPanelsSbInFloor.Count == 0) return 0;
         return AllPanelsSbInFloor.Max(p => p.ExtTransToModel.MaxPoint.X);
      }

      private double getXMinFloor()
      {
         if (AllPanelsSbInFloor.Count == 0) return 0;
         return AllPanelsSbInFloor.Min(p => p.ExtTransToModel.MinPoint.X);
      }

      // Добавление стороны фасада в этаж
      private void SetFacadeFrontBlock(FacadeFrontBlock facadeFrontBlock)
      {
         FacadeFrontBlock = facadeFrontBlock;
         PanelsSbInFront = new List<MountingPanel>();

         // найти блоки панелей-СБ входящих внутрь границ блока стороны фасада
         foreach (var panelSb in AllPanelsSbInFloor)
         {
            if (facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MinPoint) &&
               facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MaxPoint))
            {
               PanelsSbInFront.Add(panelSb);
            }
         }
         if (PanelsSbInFront.Count == 0)
         {
            Inspector.AddError(string.Format("В блоке обозначения стороны фасада {0} не найдена ни одна панель.", facadeFrontBlock.BlName),
               facadeFrontBlock.Extents, facadeFrontBlock.IdBlRef);
         }
         else
         {
            XMax = PanelsSbInFront.Max(p => p.ExtTransToModel.MaxPoint.X);
            XMin = PanelsSbInFront.Min(p => p.ExtTransToModel.MinPoint.X);
         }
      }
   }
}
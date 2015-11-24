using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Этаж - блоки АКР-панелей этажа и связаннный с ним блок монтажки с блоком обозначения стороны фасада
   public class Floor : IComparable<Floor>
   {
      // для сортировки этажей (строка имени этажа = номеру этажа)
      private static StoreyNumberComparer _comparer = new StoreyNumberComparer();

      // Панели СБ - все что есть внутри блока монтажки
      private List<PanelSB> _allPanelsSbInFloor;

      private Extents3d _extBlMounting;

      // обозначение стороны фасада на монтажном плане
      private FacadeFrontBlock _facadeFrontBlock;

      // Блок монтажного плана
      private ObjectId _idBlRefMounting;

      // Имя/номер этажа
      private string _name;
      private Storey _storey; // этаж.

      private List<PanelSB> _panelsSbInFront;

      // Точка вставки блока монтажки
      private Point3d _ptBlMounting;

      private double _xmax;
      private double _xmin; // мин значение х среди всех границ блоков панелей внктри этажа
                            // макс значение х среди всех границ блоков панелей внктри этажа
                            // блоки панелей СБ входящие внутрь блока стороны фасада

      public Floor(BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
      {
         _idBlRefMounting = blRefMounting.Id;
         _extBlMounting = blRefMounting.GeometricExtents;
         _ptBlMounting = blRefMounting.Position;
         _name = getFloorName(blRefMounting);
         // Получение всех блоков панелей СБ из блока монтажки
         _allPanelsSbInFloor = PanelSB.GetPanels(blRefMounting, blRefMounting.Position, blRefMounting.BlockTransform);
         _xmin = getXMinFloor();
         _xmax = getXMaxFloor();
         //// добавление блоков паненлей в общий список панелей СБ
         //libLoadServ.AllPanelsSB.AddRange(_allPanelsSbInFloor);
      }

      //public Point3d PtBlMounting { get { return _ptBlMounting; } }
      public List<PanelSB> AllPanelsSbInFloor { get { return _allPanelsSbInFloor; } }

      public FacadeFrontBlock FacadeFrontBlock { get { return _facadeFrontBlock; } }

      public string Name { get { return _name; } }
      public Storey Storey { get { return _storey; } set { _storey = value; } }

      public List<PanelSB> PanelsSbInFront { get { return _panelsSbInFront; } }

      public double XMax { get { return _xmax; } }

      public double XMin { get { return _xmin; } }

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
            // Дерево прямоугольников от блоков обозначений сторон фасада, для поиска пересечений с блоками монтажек
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
                     // найти соотв обозн стороны фасада
                     var frontsIntersects = rtreeFront.Intersects(ColorArea.GetRectangleRTree(blRefMounting.GeometricExtents));
                     // если нет пересечений фасадов - пропускаем блок монтажки - он не входит в фасады, просто так вставлен
                     if (frontsIntersects.Count == 0)
                     {
                        continue;
                     }
                     // если больше одного пересечения с фасадами, то это ошибка, на одной монтажке должен быть один блок обозначения стороны фасада
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

      public int CompareTo(Floor other)
      {
         // Сортировка этажей по именам
         return _comparer.Compare(_name, other._name);
      }

      // определение торцов панелей
      public void DefineEndsPanelSb()
      {
         if (_panelsSbInFront.Count == 0) return;
         // панель с самым меньшим X это торцевая панель слева
         var min = _panelsSbInFront.Aggregate((p1, p2) => p1.PtCenterPanelSbInModel.X < p2.PtCenterPanelSbInModel.X ? p1 : p2);
         min.IsEndLeftPanel = true;
         // панель с самым большим X это торцевая панель справа
         var max = _panelsSbInFront.Aggregate((p1, p2) => p1.PtCenterPanelSbInModel.X > p2.PtCenterPanelSbInModel.X ? p1 : p2);
         max.IsEndRightPanel = true;
      }

      private string getFloorName(BlockReference blRefMounting)
      {
         string name = string.Empty;
         var indexFloor = blRefMounting.Name.IndexOf("эт-");
         if (indexFloor == -1)
         {
            name = blRefMounting.Name.Substring(Settings.Default.BlockMountingPlanePrefixName.Length);
         }
         else
         {
            name = blRefMounting.Name.Substring(indexFloor + "эт-".Length);
         }
         return name;
      }

      private double getXMaxFloor()
      {
         if (_allPanelsSbInFloor.Count == 0) return 0;
         return _allPanelsSbInFloor.Max(p => p.ExtTransToModel.MaxPoint.X);
      }

      private double getXMinFloor()
      {
         if (_allPanelsSbInFloor.Count == 0) return 0;
         return _allPanelsSbInFloor.Min(p => p.ExtTransToModel.MinPoint.X);
      }

      // Добавление стороны фасада в этаж
      private void SetFacadeFrontBlock(FacadeFrontBlock facadeFrontBlock)
      {
         _facadeFrontBlock = facadeFrontBlock;
         _panelsSbInFront = new List<PanelSB>();
         // найти блоки панелей-СБ входящих внутрь границ блока стороны фасада
         foreach (var panelSb in _allPanelsSbInFloor)
         {
            if (facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MinPoint) &&
               facadeFrontBlock.Extents.IsPointInBounds(panelSb.ExtTransToModel.MaxPoint))
            {
               panelSb.IsInFloor = true;
               _panelsSbInFront.Add(panelSb);
            }
         }
         if(_panelsSbInFront.Count ==0)
         {
            Inspector.AddError(string.Format("В блоке обозначения стороны фасада {0} не найдена ни одна панель.", facadeFrontBlock.BlName),
               facadeFrontBlock.Extents, facadeFrontBlock.IdBlRef);
         }
      }
   }
}
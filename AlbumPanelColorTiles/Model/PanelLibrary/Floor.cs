using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Checks;
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

      // обозначение стороны фасада на монтажном плане
      private FacadeFrontBlock _facadeFrontBlock;
      // Блок монтажного плана
      private ObjectId _idBlRefMounting;
      // Точка вставки блока монтажки
      private Point3d _ptBlMounting;
      // Имя/номер этажа 
      private string _name;
      // Панели СБ
      private List<PanelSB> _panelsSB;

      public Floor(BlockReference blRefMounting, PanelLibraryLoadService libLoadServ)
      {         
         _idBlRefMounting = blRefMounting.Id;
         _ptBlMounting = blRefMounting.Position; 
         _name = getFloorName(blRefMounting);
         // Получение всех блоков панелей СБ из блока монтажки
         _panelsSB = PanelSB.GetPanels(blRefMounting.BlockTableRecord, blRefMounting.Position);
         // добавление блоков паненлей в общий список панелей СБ
         libLoadServ.AllPanelsSB.AddRange(_panelsSB);
      }

      public Point3d PtBlMounting { get { return _ptBlMounting; } }
      public List<PanelSB> PanelsSB { get { return _panelsSB; } }
      public FacadeFrontBlock FacadeFrontBlock { get { return _facadeFrontBlock; } private set { _facadeFrontBlock = value; } }

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
            // Поиск панелейСБ в Модели и добавление в общий список панелей СБ.
            libLoadServ.AllPanelsSB.AddRange(PanelSB.GetPanels(ms.Id, Point3d.Origin));
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  // Если это блок монтажного плана - имя блока начинается с АКР_Монтажка_
                  if (blRefMounting.Name.StartsWith(Album.Options.BlockMountingPlanePrefixName, StringComparison.CurrentCultureIgnoreCase))
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
                        floor.FacadeFrontBlock = frontsIntersects[0];
                        floors.Add(floor);
                     }                     
                  }
               }
            }
            t.Commit();
         }
         return floors;
      }

      private string getFloorName(BlockReference blRefMounting)
      {
         return blRefMounting.Name.Substring(Album.Options.BlockMountingPlanePrefixName.Length);
      }

      public int CompareTo(Floor other)
      {
         // Сортировка этажей по именам
         return _comparer.Compare(_name, other._name);
      }
   }
}

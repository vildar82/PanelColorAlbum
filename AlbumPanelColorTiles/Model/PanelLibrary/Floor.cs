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
   public class Floor
   {      
      // обозначение стороны фасада на монтажном плане
      private FacadeFrontBlock _facadeFrontBlock;
      // Блок монтажного плана
      private ObjectId _idBlRefMounting;
      // Точка вставки блока монтажки
      private Point3d _ptBlMounting;
      // Имя/номер этажа 
      private string _name;      

      public Floor(BlockReference blRefMounting, FacadeFrontBlock facadeFrontBlock)
      {
         _facadeFrontBlock = facadeFrontBlock;
         _idBlRefMounting = blRefMounting.Id;
         _ptBlMounting = blRefMounting.Position; 
         _name = getFloorName(blRefMounting);
      }

      public Point3d PtBlMounting { get { return _ptBlMounting; } }

      /// <summary>
      /// Поиск всех блоков монтажек в модели
      /// </summary>      
      public static List<Floor> GetMountingBlocks()
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
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefMounting = t.GetObject(idEnt, OpenMode.ForRead) as BlockReference;
                  // Если это блок монтажного плана - имя блока начинается с АКР_Монтажка_
                  if (blRefMounting.Name.StartsWith(Album.Options.BlockMountingPlanePrefixName, StringComparison.CurrentCultureIgnoreCase))
                  {
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
                        Floor floor = new Floor(blRefMounting, frontsIntersects[0]);
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
   }
}

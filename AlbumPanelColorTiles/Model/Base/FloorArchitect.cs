using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MoreLinq;

namespace AlbumPanelColorTiles.Model.Base
{
   // Архитектурный план с марками окнон
   public class FloorArchitect
   {
      public int Section { get; private set; }
      //public int Number { get; private set; }      
      public EnumStorey StoreyType { get; private set; }
      public IEnumerable<int> StoreyNumbers { get; private set; }
      public string BlName { get; private set; }
      public ObjectId IdBlRef { get; private set; }
      public ObjectId IdBtr { get; private set; }
      public Dictionary<Point3d, string> Windows { get; private set; }
      public BaseService Service { get; private set; }

      public FloorArchitect(BlockReference blRefArPlan, BaseService service)
      {
         Service = service;
         IdBlRef = blRefArPlan.Id;
         IdBtr = blRefArPlan.BlockTableRecord;
         BlName = blRefArPlan.Name;
         // определение параметров плана и окон
         try
         {
            definePlanNumberAndSection(blRefArPlan);
         }
         catch (Exception ex)
         {
            Inspector.AddError($"Ошибка при определении параметров арх.плана {BlName}.", blRefArPlan, icon: System.Drawing.SystemIcons.Error);
         }         
         // определение окон в плане
         Windows = defineWindows(blRefArPlan);
      }      

      /// <summary>
      /// Поиск архитектурных планов в Модели.
      /// Запускает транзакцию.
      /// </summary>            
      public static List<FloorArchitect> GetAllPlanes(Database db, BaseService service)
      {
         List<FloorArchitect> floorsAr = new List<FloorArchitect>();

         using (var t = db.TransactionManager.StartTransaction())
         {
            var ms = SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject( OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId idEnt in ms)
            {
               var blRefArPlan = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
               if (blRefArPlan == null) continue;               

               if (blRefArPlan.Name.StartsWith(Settings.Default.BlockPlaneArchitectPrefixName, StringComparison.CurrentCultureIgnoreCase))
               {
                  FloorArchitect floorAr = new FloorArchitect(blRefArPlan, service);
                  floorsAr.Add(floorAr);
               }
            }
            t.Commit();
         }
         return floorsAr;
      }

      private void definePlanNumberAndSection(BlockReference blRefArPlan)
      {
         // Номер плана и номер секции
         string blPlanArName = blRefArPlan.Name;
         var val = blPlanArName.Substring(Settings.Default.BlockPlaneArchitectPrefixName.Length);
         var arrSplit = val.Split('_');
         string numberPart;
         if (arrSplit.Length>1)
         {
            int section;
            string sectionPart = arrSplit[0].Substring(1);
            if (int.TryParse(sectionPart, out section))
            {
               Section = section;               
            }
            else
            {
               Inspector.AddError($"Архитектурный план '{blPlanArName}'. Не определен номер секции '{sectionPart}'.", blRefArPlan, 
                  icon: System.Drawing.SystemIcons.Error);
            }            
            numberPart = arrSplit[1];
         }
         else
         {
            numberPart = arrSplit[0];
         }
         int number;

         string valStorey = numberPart.Substring("эт-".Length);
         if (string.Equals(valStorey, Settings.Default.PaintIndexUpperStorey, StringComparison.OrdinalIgnoreCase))
         {
            StoreyType = EnumStorey.Upper;
         }
         else if (string.Equals(valStorey, Settings.Default.PaintIndexParapet, StringComparison.OrdinalIgnoreCase))
         {
            StoreyType = EnumStorey.Parapet;
         }
         else
         {
            // число
            StoreyType = EnumStorey.Number;
            var splitNumbers = valStorey.Split('-');
            if (splitNumbers.Count()>1)
            {
               int minNumber = int.Parse(splitNumbers[0]);
               int maxNumber = int.Parse(splitNumbers[1]);
               StoreyNumbers = Enumerable.Range(minNumber, maxNumber - minNumber);
            }
            else
            {
               StoreyNumbers = Enumerable.Range(int.Parse(splitNumbers[0]), 1);
            }
            if (this.StoreyNumbers.Count()==0)
            {
               Inspector.AddError($"Архитектурный план '{blPlanArName}'. Не определен номер этажа {numberPart}.", blRefArPlan,
               icon: System.Drawing.SystemIcons.Error);
            }
         }            
      }

      private Dictionary<Point3d, string> defineWindows(BlockReference blRefArPlan)
      {
         var windows = new Dictionary<Point3d, string>();
         var btrArPlan = blRefArPlan.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord;

         Dictionary<Point3d, string> marks = getAllMarks(btrArPlan);

         foreach (ObjectId idEnt in btrArPlan)
         {
            var blRefWindow = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
            if (blRefWindow == null) continue;

            // Если это блок окна - имя начинается с WNW, на слое A-GLAZ
            //if (blRefWindow.Name.StartsWith("WNW", StringComparison.OrdinalIgnoreCase) &&
            //   blRefWindow.Layer.Equals("A-GLAZ", StringComparison.OrdinalIgnoreCase))
            string blNameWindow = blRefWindow.GetEffectiveName();
            if (Regex.IsMatch(blNameWindow, "окно", RegexOptions.IgnoreCase))
            {
               // Найти рядом текст марки окна - в радиусе 1000 
               var markNearKey = marks.GroupBy(m => m.Key.DistanceTo(blRefWindow.Position))?.MinBy(m => m.Key);
               if (markNearKey == null || markNearKey.Key > 1000)
               {
                  Extents3d extBlRefWin = blRefWindow.GeometricExtents;
                  extBlRefWin.TransformBy(blRefArPlan.BlockTransform);
                  Inspector.AddError($"Архитектурный план '{blRefArPlan.Name}' - " + 
                     $"Не определена марка окна около блока окна '{blNameWindow}'.", extBlRefWin, blRefWindow.Id, 
                     icon: System.Drawing.SystemIcons.Error);
                  continue;
               }
               var markNear = markNearKey.First();
               windows.Add(blRefWindow.Position, markNear.Value);
#if Test
                        //Test Добавить точку окна и найденную марку окна в точку вставки блока окна
                        {
                           //DBPoint ptWin = new DBPoint(blRefWindow.Position);
                           //btrArPlan.AppendEntity(ptWin);
                           //btrArPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(ptWin, true);

                           using (DBText dbText = new DBText())
                           {
                              dbText.Position = blRefWindow.Position;
                              dbText.TextString = markNear.Value;
                              btrArPlan.AppendEntity(dbText);
                              //btrArPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(dbText, true);
                           }
                        }
#endif
            }
         }
         return windows;
      }

      private Dictionary<Point3d, string> getAllMarks(BlockTableRecord btrArPlan)
      {
         Dictionary<Point3d, string> marks = new Dictionary<Point3d, string>();
         foreach (ObjectId idEnt in btrArPlan)
         {
            bool isFound = false;

            Point3d ptTextPos = Point3d.Origin;
            string textMark = string.Empty;

            if (idEnt.ObjectClass.Name == "AcDbMText")
            {
               // МТекст на слое A-GLAZ - IDEN               
               var markText = idEnt.GetObject(OpenMode.ForRead, false, true) as MText;
               //if (markText.Layer.Equals("A-GLAZ-IDEN", StringComparison.OrdinalIgnoreCase))
               if (Service.Env.WindowMarks.Contains(markText.Text, StringComparer.OrdinalIgnoreCase))
               {
                  isFound = true;
                  ptTextPos = markText.Location;
                  textMark = markText.Text;
               }
            }
            if (idEnt.ObjectClass.Name == "AcDbText")
            {               
               var markText = idEnt.GetObject(OpenMode.ForRead, false, true) as DBText;             
               if (Service.Env.WindowMarks.Contains(markText.TextString, StringComparer.OrdinalIgnoreCase))
               {
                  isFound = true;
                  ptTextPos = markText.Position;
                  textMark = markText.TextString;
               }
            }
            if (isFound && !marks.ContainsKey(ptTextPos))
            {              
               marks.Add(ptTextPos, textMark);
            }
         }         
         return marks;
      }

      public bool IsEqualMountingStorey(Storey storey)
      {
         bool res = false;
         if (storey.Type == StoreyType)
         {
            if (storey.Type == EnumStorey.Number)
            {
               if (StoreyNumbers.Contains(storey.Number))
               {
                  res = true;
               }
            }
            else
            {
               res = true;
            }  
         }
         return res;
      }
   }
}

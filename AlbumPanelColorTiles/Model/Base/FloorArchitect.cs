﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MoreLinq;

namespace AlbumPanelColorTiles.Model.Base
{
   // Архитектурный план с марками окнон
   public class FloorArchitect
   {
      public int Section { get; private set; }
      public int Number { get; private set; }
      public Dictionary<Point3d, string> Windows { get; private set; }

      public FloorArchitect(BlockReference blRefArPlan)
      {
         // определение параметров плана и окон
         definePlanNumberAndSection(blRefArPlan.Name);
         // определение окон в плане
         Windows = defineWindows(blRefArPlan);
      }      

      public static List<FloorArchitect> GetAllPlanes(Database db)
      {
         List<FloorArchitect> floorsAr = new List<FloorArchitect>();

         using (var t = db.TransactionManager.StartTransaction())
         {
            var ms = t.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead) as BlockTableRecord;

            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefArPlan = idEnt.GetObject( OpenMode.ForRead, false, true) as BlockReference;                  
                  if (blRefArPlan.Name.StartsWith(Settings.Default.BlockPlaneArchitectPrefixName, StringComparison.CurrentCultureIgnoreCase))
                  {
                     FloorArchitect floorAr = new FloorArchitect(blRefArPlan);
                     floorsAr.Add(floorAr);
                  }
               }
            }
            t.Commit();
         }
         return floorsAr;
      }

      private void definePlanNumberAndSection(string blPlanArName)
      {
         // Номер плана и номер секции
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
               Inspector.AddError($"Архитектурный план {blPlanArName}. Не определен номер секции {sectionPart}.");
            }            
            numberPart = arrSplit[1];
         }
         else
         {
            numberPart = arrSplit[0];
         }
         int number;
         if (int.TryParse(numberPart.Substring("эт-".Length), out number))
         {
            Number = number;
         }
         else
         {
            Inspector.AddError($"Архитектурный план {blPlanArName}. Не определен номер этажа {numberPart}.");
         }
      }

      private Dictionary<Point3d, string> defineWindows(BlockReference blRefArPlan)
      {
         var windows = new Dictionary<Point3d, string>();
         using (var btrArPlan = blRefArPlan.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord)
         {
            Dictionary<Point3d, string> marks = getAllMarks(btrArPlan);            

            foreach (ObjectId idEnt in btrArPlan)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  var blRefWindow = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                  // Если это блок окна - имя начинается с WNW, на слое A-GLAZ
                  if (blRefWindow.Name.StartsWith("WNW", StringComparison.OrdinalIgnoreCase) &&
                      blRefWindow.Layer.Equals("A-GLAZ", StringComparison.OrdinalIgnoreCase))
                  {
                     // Найти рядом текст марки окна - в радиусе 1000 
                     var markNearKey = marks.GroupBy (m => m.Key.DistanceTo(blRefWindow.Position)).MinBy(m=>m.Key);                                          
                     if (markNearKey == null || markNearKey.Key > 1000)
                     {                        
                        Inspector.AddError($"Архитектурный план {blRefArPlan.Name}. Не определена марка окна около блока окна с координатами {blRefWindow.Position}.");
                        continue;
                     }
                     var markNear = markNearKey.First();
                     windows.Add(blRefWindow.Position, markNear.Value);
                     DBText dbText = new DBText();
                     dbText.Position = blRefWindow.Position;
                     dbText.TextString = markNear.Value;
                     btrArPlan.AppendEntity(dbText);
                     btrArPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(dbText, true);
                  }
               }
            }
         }
         return windows;
      }

      private Dictionary<Point3d, string> getAllMarks(BlockTableRecord btrArPlan)
      {
         Dictionary<Point3d, string> marks = new Dictionary<Point3d, string>();
         foreach (ObjectId idEnt in btrArPlan)
         {
            if (idEnt.ObjectClass.Name == "AcDbMText")
            {
               // МТекст на слое A-GLAZ - IDEN
               var markText = idEnt.GetObject(OpenMode.ForRead, false, true) as MText;
               if (markText.Layer.Equals("A-GLAZ-IDEN", StringComparison.OrdinalIgnoreCase))
               {
                  marks.Add(markText.Location, markText.Text);
               }
            }
         }         
         return marks;
      }
   }
}
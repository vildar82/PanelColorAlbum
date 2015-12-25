using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Tests
{
   public enum EnumOpening
   {
      None,
      Right,
      Left
   }

   public class WindowTranslator
   {
      public string BlNameOld { get; set; }
      public string Mark { get; set; }
      public EnumOpening Opening { get; set; }
      public short TableView { get; set; }
      public int Order { get; set; }

      public WindowTranslator(string oldBlName, string mark, EnumOpening opening, short view)
      {
         BlNameOld = oldBlName;
         Mark = mark;
         Opening = opening;
         TableView = view;
         switch (TableView)
         {
            case 0:
            case 1:
               Order = 1;
               break;
            case 2:
               Order = 2;
               break;
            case 3:
            case 4:
               Order = 3;
               break;
            case 5:
            case 6:
               Order = 4;
               break;
            case 7:
            case 8:
               Order = 5;
               break;                           
            default:
               Order = TableView - 3;
               break;
         }
      }
   }

   public class TestReplaceWindows
   {
      Document doc;
      Editor ed;
      Database db;
      ObjectId IdBtrWindow;
      string LayerWindow = "АР_Окна";

      public void Replace()
      {
         // Переименование блоков панелей с тире (3НСг-72.29.32 - на 3НСг 72.29.32)
         doc = Application.DocumentManager.MdiActiveDocument;
         ed = doc.Editor;
         db = doc.Database;

         var translatorWindows = getTranslatorWindows();

         using (var t = db.TransactionManager.StartTransaction())
         {  
            // Блоки панелей            
            List<ObjectId> idsBtrPanels = getPanelsBtr();

            foreach (var idBtrPanel in idsBtrPanels)
            {
               var btrPanel = idBtrPanel.GetObject(OpenMode.ForRead) as BlockTableRecord;
               foreach (ObjectId idEnt in btrPanel)
               {
                  if (idEnt.ObjectClass.Name == "AcDbBlockReference")
                  {
                     var blRefWindowOld = idEnt.GetObject(OpenMode.ForRead, false, true) as BlockReference;
                     WindowTranslator translatorW;
                     if (translatorWindows.TryGetValue(blRefWindowOld.Name,out translatorW))
                     {
                        replaceWindows(btrPanel, blRefWindowOld, translatorW);
                     }
                  }
               }
            }
            t.Commit();
         }
      }

      private void replaceWindows(BlockTableRecord btrPanel, BlockReference blRefWindowOld, WindowTranslator translatorW)
      {
         var extOldWind = blRefWindowOld.GeometricExtentsСlean();
         var newBlRefW = new BlockReference(extOldWind.MinPoint, IdBtrWindow);
         newBlRefW.SetDatabaseDefaults(db);
         newBlRefW.Layer = LayerWindow;         

         btrPanel.UpgradeOpen();
         btrPanel.AppendEntity(newBlRefW);
         btrPanel.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(newBlRefW, true);

         setDynProp(newBlRefW, translatorW);

         blRefWindowOld.UpgradeOpen();
         blRefWindowOld.Erase();         
      }

      private void setDynProp(BlockReference newBlRefW, WindowTranslator translatorW)
      {
         Dictionary<string, DynamicBlockReferenceProperty> dictProps = new Dictionary<string, DynamicBlockReferenceProperty>();
         foreach (DynamicBlockReferenceProperty prop in newBlRefW.DynamicBlockReferencePropertyCollection)
         {
            dictProps.Add(prop.PropertyName, prop);
         }
         // Таблица выбора
         dictProps["Видимость"].Value = translatorW.Mark;
         dictProps["Марка"].Value = translatorW.Mark;
         dictProps["Открывание"].Value = getPropOpening (translatorW.Opening);
         dictProps["Порядок"].Value = translatorW.Order;
         dictProps["Выбор"].Value = translatorW.TableView;
      }

      private string getPropOpening(EnumOpening opening)
      {
         switch (opening)
         {
            case EnumOpening.None:
               return "";               
            case EnumOpening.Right:
               return "Правое";
            case EnumOpening.Left:
               return "Левое";
            default:
               return "";
         }
      }

      private Dictionary<string, WindowTranslator> getTranslatorWindows()
      {
         Dictionary<string, WindowTranslator> translator = new Dictionary<string, WindowTranslator>();
         translator.Add("бп-1", new WindowTranslator ("бп-1", "БП-1",  EnumOpening.None, 15));
         translator.Add("Окно_БП-1",new WindowTranslator ("Окно_БП-1", "БП-1", EnumOpening.None, 15));
         translator.Add("Окно_ОБД-1", new WindowTranslator ("Окно_ОБД-1", "ОБД-1", EnumOpening.None, 16));
         translator.Add("АР_Окно_ОП-11", new WindowTranslator (  "АР_Окно_ОП-11", "ОП-11", EnumOpening.None, 9));
         translator.Add("ОП-11_фасад", new WindowTranslator ( "ОП-11_фасад", "ОП-11", EnumOpening.None, 9));
         translator.Add("ок 1 вид с фасада",new WindowTranslator ("ок 1 вид с фасада", "ОП-14", EnumOpening.None, 10));
         translator.Add("Окно_ОП-14", new WindowTranslator("Окно_ОП-14", "ОП-14", EnumOpening.None, 10));
         translator.Add("ОП-14 2100х1800", new WindowTranslator("ОП-14 2100х1800", "ОП-14", EnumOpening.None, 10));
         translator.Add("ОП-15 1500х1800", new WindowTranslator("ОП-15 1500х1800", "ОП-15", EnumOpening.None, 11));
         translator.Add("ОП-15 (КБЕ) 1550-1790 ПО ВК", new WindowTranslator("ОП-15 (КБЕ) 1550-1790 ПО ВК", "ОП-15", EnumOpening.None, 11));         
         translator.Add("ОП-16 1500х1800", new WindowTranslator("ОП-16 1500х1800", "ОП-16", EnumOpening.None, 12));
         translator.Add("АР_Окно_ОП-17",new WindowTranslator("АР_Окно_ОП-17", "ОП-17", EnumOpening.None,13));
         translator.Add("ОП-17 600х1800",new WindowTranslator("ОП-17 600х1800", "ОП-17", EnumOpening.None, 13));
         translator.Add("Окно_ОП-2", new WindowTranslator("Окно_ОП-2", "ОП-2", EnumOpening.Right, 0));
         translator.Add("ок2 внешний вид",new WindowTranslator("ок2 внешний вид", "ОП-2", EnumOpening.Left, 1));
         translator.Add("Окно_ОП-2_Л", new WindowTranslator("Окно_ОП-2_Л", "ОП-2", EnumOpening.Left, 1));
         translator.Add("АР_Окно_ОП-24", new WindowTranslator("АР_Окно_ОП-24", "ОП-24", EnumOpening.None, 14));
         translator.Add("ОП-2л 900х1800", new WindowTranslator("ОП-2л 900х1800", "ОП-2", EnumOpening.Left, 1));
         translator.Add("ОП-2п 900х1800", new WindowTranslator("ОП-2п 900х1800", "ОП-2", EnumOpening.Right, 0));
         translator.Add("ОП-3 900х1800", new WindowTranslator("ОП-3 900х1800", "ОП-3", EnumOpening.None, 2));
         translator.Add("ок-4 внешний вид", new WindowTranslator("ок-4 внешний вид", "ОП-4", EnumOpening.Right, 3));
         translator.Add("Окно_ОП-4", new WindowTranslator("Окно_ОП-4", "ОП-4", EnumOpening.Right, 3));
         translator.Add("ОП-4(КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4(КБЕ) 1250-1790 П ВК", "ОП-4", EnumOpening.Right, 3));
         translator.Add("ОП-4 1200х1800", new WindowTranslator("ОП-4 1200х1800", "ОП-4", EnumOpening.Right, 3));
         translator.Add("ОП-4л(КБЕ) 1250-1790 П ВК", new WindowTranslator("ОП-4л(КБЕ) 1250-1790 П ВК", "ОП-4", EnumOpening.Left, 4));
         translator.Add("ОП-5(КБЕ) 650-1790 ПО ВК", new WindowTranslator("ОП-5(КБЕ) 650-1790 ПО ВК", "ОП-5", EnumOpening.Right, 5));
         translator.Add("ок 5 внешний вид", new WindowTranslator("ок 5 внешний вид", "ОП-5", EnumOpening.Left, 6));
         translator.Add("Окно_ОП-5_Л", new WindowTranslator("Окно_ОП-5_Л", "ОП-5", EnumOpening.Left, 6));
         translator.Add("ОП-5л(КБЕ) 650-1790 ПО ВК", new WindowTranslator("ОП-5л(КБЕ) 650-1790 ПО ВК", "ОП-5", EnumOpening.Left, 6));
         translator.Add("ОП-5л 600х1800", new WindowTranslator("ОП-5л 600х1800", "ОП-5", EnumOpening.Left, 6));
         translator.Add("ОП-6л", new WindowTranslator("ОП-6л", "ОП-6", EnumOpening.Left, 7));
         translator.Add("ОП-6л 1500х1800", new WindowTranslator("ОП-6л 1500х1800", "ОП-6", EnumOpening.Left, 7));
         translator.Add("ОП-6п 1500х1800", new WindowTranslator("ОП-6п 1500х1800", "ОП-6", EnumOpening.Right, 8));         
         return translator;
      }

      private List<ObjectId> getPanelsBtr()
      {
         List<ObjectId> idsBtrPanels = new List<ObjectId>();
         var bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
         IdBtrWindow = bt["АКР_Окно"];
         foreach (ObjectId idBtr in bt)
         {
            var btr = idBtr.GetObject(OpenMode.ForRead) as BlockTableRecord;
            if (btr.Name.StartsWith(Settings.Default.BlockPanelPrefixName))
            {
               idsBtrPanels.Add(idBtr);
            }
         }
         return idsBtrPanels;
      }
   }
}

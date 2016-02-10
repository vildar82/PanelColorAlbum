using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Select;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using MoreLinq;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BaseService
   {  
      private Dictionary<string, Panel> _panelsXML;      
      public CreatePanelsBtrEnvironment Env { get; private set; }
      public string XmlBasePanelsFile { get; set; }
      public int CountPanelsInBase { get { return (_panelsXML == null) ? 0 : _panelsXML.Count; } }
      public Database Db { get; set; }

      public BaseService()
      {
         //XmlBasePanelsFile = @"c:\dev\АР\AlbumPanelColorTiles\PanelColorAlbum\AlbumPanelColorTiles\Model\Base\Panels.xml";
         XmlBasePanelsFile = @"\\dsk2.picompany.ru\project\CAD_Settings\Settings\dbs\outwalls_base.xml";         
      }

      public BaseService(string xmlBasePanelsFile)
      {
         XmlBasePanelsFile = xmlBasePanelsFile;
      }      

      /// <summary>
      /// Подготовка к построению блоков панелей - копирование вспом блоков.
      /// Транзакция не нужна!
      /// </summary>
      /// <param name="db"></param>
      public void InitToCreationPanels(Database db)
      {
         Db = db;
         Env = new CreatePanelsBtrEnvironment(this);
         Env.LoadAllEnv();
      }

      public void ReadPanelsFromBase()
      {
         if (!File.Exists(XmlBasePanelsFile))
         {
            throw new FileNotFoundException("XML файл базы панелей не найден.", XmlBasePanelsFile);
         }

         // TODO: Проверка валидности xml         

         // Чтение файла базы панелей
         _panelsXML = new Dictionary<string, Panel>();
         XmlSerializer ser = new XmlSerializer(typeof(Base.Panels));
         using (var fileStreamXml = new FileStream(XmlBasePanelsFile, FileMode.Open))
         {
            Panels panels = ser.Deserialize(fileStreamXml) as Panels;
            var panelsList = panels.Panel.ToList();
            foreach (var panel in panelsList)
            {
               try
               {
                  _panelsXML.Add(panel.mark.ToUpper(), panel);
               }
               catch (ArgumentException ex)
               {
                  Inspector.AddError($"Ошибка получения панели из базы xml - такая панель уже есть. {ex.Message}", 
                     icon: System.Drawing.SystemIcons.Error);
               }
            }
         }
      }
      
      public void ClearPanelsAkrFromDrawing(Database db)
      {
         SelectionBlocks sel = new SelectionBlocks(db);
         sel.SelectAKRPanelsBtr();

         List<ObjectId> idsBtrPanelsAkr = sel.IdsBtrPanelAr.ToList();
         idsBtrPanelsAkr.AddRange(sel.IdsBtrPanelSb);

         List<ObjectId> idsBtrOther = new List<ObjectId>();

         foreach (ObjectId idBtrPanel in idsBtrPanelsAkr)
         {
            using (var btrPanel = idBtrPanel.Open(OpenMode.ForRead) as BlockTableRecord)
            {
               foreach (ObjectId idBlRefPanel in btrPanel.GetBlockReferenceIds(false, true))
               {
                  using (var blRefPanel = idBlRefPanel.Open(OpenMode.ForWrite, false, true) as BlockReference)
                  {
                     blRefPanel.Erase();
                  }
               }
               foreach (ObjectId idEnt in btrPanel)
               {
                  using (var blRef = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     if (blRef == null) continue;
                     idsBtrOther.Add(blRef.BlockTableRecord);
                  }
               }
            }
         }

         eraseIdsDbo(idsBtrPanelsAkr);
         eraseIdsDbo(idsBtrOther);
      }      

      public Panel GetPanelXml(string markSb, MountingPanel panelMount = null)
      {
         Panel panel;
         if (!_panelsXML.TryGetValue(markSb.ToUpper(), out panel))
         {
            // Если это панель с классом бетона 2, то найти панель без индекса класса (3НСНг2 75.29.42-6 - 3НСНг 75.29.42-6 - геометрически одинаковые)            
            int indexConcreteClass = panelMount != null ? panelMount.IndexConcreteClass : MountingPanel.DefineIndexConcreteClass(markSb);
            if (indexConcreteClass > 0)
            {
               string markSbWoConcreteClass = MountingPanel.GetMarkWoConcreteClass(markSb);
               panel = GetPanelXml(markSbWoConcreteClass, panelMount);
               if (panel !=null)
               {
                  panel.mark = markSb;
               }
               return panel;           
            }        
         }
         if (panel == null)
         {
            // Ошибка - панели с такой маркой нет в базе                 
            if (panelMount == null)
            {
               Inspector.AddError($"Панели с такой маркой нет в базе - {markSb}", icon: System.Drawing.SystemIcons.Error);
            }
            else
            {
               Inspector.AddError($"Панели с такой маркой нет в базе - {markSb}", panelMount.ExtTransToModel, panelMount.IdBlRef,
                  icon: System.Drawing.SystemIcons.Error);
            }
         }
         return panel;
      }       

      /// <summary>
      /// Создание панелей
      /// Не нужна транзакция!!!
      /// </summary>      
      public void CreateBtrPanels(List<FacadeMounting> facadesMounting, List<FloorArchitect> floorsAr)
      {         
         var panelsBaseGroup = matchingPlans(facadesMounting, floorsAr).Values.GroupBy(p => p.Panel.mark);

         ProgressMeter progressMeter = new ProgressMeter();
         progressMeter.SetLimit(panelsBaseGroup.Sum(g=>g.Count()));
         progressMeter.Start("Создание блоков панелей");

         foreach (var itemGroupPanelByMark in panelsBaseGroup)
         {
            if (HostApplicationServices.Current.UserBreak())
            {
               throw new System.Exception("Отменено пользователем.");
            }

            // Нумерация индексов окон
            if (itemGroupPanelByMark.Count() > 1)
            {
               // Панели отличающиеся сочетанием окон - пронумеровать индекс окна
               int index = 1;
               itemGroupPanelByMark.ForEach(p => p.WindowsIndex = index++);
            }

            //using (var t = Db.TransactionManager.StartTransaction())
            //{           
            foreach (var panelBase in itemGroupPanelByMark)
            {
               try
               {
                  panelBase.CreateBlock();
                  progressMeter.MeterProgress();
               }
               catch (System.Exception ex)
               {
                  Inspector.AddError($"Не создана панель {panelBase.Panel.mark}. Ошибка - {ex.Message}",icon: System.Drawing.SystemIcons.Error);
               }
            }
            //   t.Commit();
            //}            
         }
         progressMeter.Stop();
      }

      private Dictionary<string, PanelBase> matchingPlans(List<FacadeMounting> facadesMounting, List<FloorArchitect> floorsAr)
      {
         // Определение окон в монтажных планах по архитектурным планам
         var panelsBase = new Dictionary<string, PanelBase>(); // string - ключ - маркаСБ + Марки Окон по порядку.
         
         // Список монтажных планов - уникальных
         var floorsMount = facadesMounting.SelectMany(f => f.Floors);
         foreach (var floorMount in floorsMount)
         {
            // Найти соотв арх план
            var floorAr = floorsAr.Find(f => (f.Section == floorMount.Section) && (f.Number == floorMount.Storey?.Number));

            //Test Добавить текст имени плана Ар в блок монтажного плана
#if Test
            {
               if (floorAr != null)
               {
                  using (var btrMount = floorMount.IdBtrMounting.Open(OpenMode.ForWrite) as BlockTableRecord)
                  {
                     DBText textFindPlanAr = new DBText();
                     textFindPlanAr.TextString = floorAr.BlName;
                     btrMount.AppendEntity(textFindPlanAr);
                     //btrMount.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(textFindPlanAr, true);
                  }
               }
            }
#endif

            if (floorAr == null)
            {
               Inspector.AddError($"Не найден блок архитектурного плана для соответствующего монтажного плана '{floorMount.BlRefName}'",
                  floorMount.IdBlRefMounting,
                  icon: System.Drawing.SystemIcons.Error);               
            }
            else if (floorAr.Windows.Count == 0)
            {
               Inspector.AddError($"Не найдено ни одного окна в блоке архитектурного плана '{floorMount.BlRefName}'.",
                  floorAr.IdBlRef,
                  icon: System.Drawing.SystemIcons.Error);               
            }

            foreach (var panelMount in floorMount.PanelsSbInFront)
            {
               Panel panelXml = GetPanelXml(panelMount.MarkSb, panelMount);
               if (panelXml == null) continue;
               PanelBase panelBase = new PanelBase(panelXml, this, panelMount);
               // Определение окон в панели по арх плану
               if (panelXml.windows?.window != null)
               {
                  foreach (var window in panelXml.windows.window)
                  {
                     if (floorAr == null || floorAr.Windows.Count==0)
                     {
                        break;
                     }

                     // Точка окна внутри панели по XML описанию
                     Point3d ptOpeningCenter = new Point3d(window.posi.X + window.width * 0.5, 0, 0);
                     // Точка окна внутри монтажного плана
                     Point3d ptWindInModel = panelMount.ExtTransToModel.MinPoint.Add(ptOpeningCenter.GetAsVector());
                     Point3d ptWindInArPlan = ptWindInModel.TransformBy(floorMount.Transform.Inverse());

                     var windowKey = floorAr?.Windows.GroupBy(w => w.Key.DistanceTo(ptWindInArPlan))?.MinBy(w => w.Key);
                     if (windowKey == null || windowKey.Key > 600)
                     {
                        Inspector.AddError(
                           $"Не найдено соответствующее окно в архитектурном плане. Блок монтажной панели {panelMount.MarkSb}",
                           panelMount.ExtTransToModel, panelMount.IdBlRef, icon: System.Drawing.SystemIcons.Error);
                        continue;
                     }
                     panelBase.WindowsBaseCenters.Add(ptOpeningCenter, windowKey.First().Value);

                     // Test Добавление точек окна в блоке монтажки
#if Test
                     {
                        using (var btrMountPlan = floorMount.IdBtrMounting.Open(OpenMode.ForWrite) as BlockTableRecord)
                        {
                           using (DBPoint ptWinInPlan = new DBPoint(ptWindInArPlan))
                           {
                              ptWinInPlan.ColorIndex = 2;
                              btrMountPlan.AppendEntity(ptWinInPlan);
                              //btrMountPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(ptWinInPlan, true);
                           }

                           using (DBText dbText = new DBText())
                           {
                              dbText.Position = ptWindInArPlan;
                              dbText.TextString = windowKey.First().Value;
                              btrMountPlan.AppendEntity(dbText);
                              //btrMountPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(dbText, true);
                           }
                        }
                        using (var btrArPlan = floorAr.IdBtr.Open(OpenMode.ForWrite) as BlockTableRecord)
                        {
                           using (DBPoint ptWinInArPlan = new DBPoint(ptWindInArPlan))
                           {
                              ptWinInArPlan.ColorIndex = 1;
                              btrArPlan.AppendEntity(ptWinInArPlan);
                              //btrArPlan.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(ptWinInArPlan, true);
                           }
                        }
                     }
#endif
                  }
               }
               // Уникальный ключ панели - МаркаСБ + Марки окон                    
               string key = panelBase.MarkWithoutElectric;
               if (panelBase.WindowsBaseCenters.Count > 0)
               {
                  string windowMarks = string.Join(";", panelBase.WindowsBaseCenters.Values);
                  key += windowMarks;
               }
               PanelBase panelBaseUniq;
               if (!panelsBase.TryGetValue(key, out panelBaseUniq))
               {
                  panelsBase.Add(key, panelBase);
                  panelBaseUniq = panelBase;
               }
               panelMount.PanelBase = panelBaseUniq;
            }
         }
         return panelsBase;
      }

      private static void eraseIdsDbo(List<ObjectId> idsDbobjects)
      {
         foreach (ObjectId idEnt in idsDbobjects)
         {
            if (!idEnt.IsNull && !idEnt.IsErased)
            {
               using (var dbo = idEnt.Open(OpenMode.ForWrite, false) as DBObject)
               {
                  if (dbo != null)
                  {
                     try
                     {
                        dbo.Erase();
                     }
                     catch { }
                  }
               }
            }
         }
      }

      /// <summary>
      /// Заморозка слоев образмеривания панелей
      /// </summary>
      public void FreezeDimLayers()
      {
         using (var layerDimFacade = Env.IdLayerDimFacade.Open(OpenMode.ForRead) as LayerTableRecord)
         {
            if (!layerDimFacade.IsFrozen)
            {
               layerDimFacade.UpgradeOpen();
               layerDimFacade.IsFrozen = true;
            }
         }
         using (var layerDimForm = Env.IdLayerDimForm.Open(OpenMode.ForRead) as LayerTableRecord)
         {
            if (!layerDimForm.IsFrozen)
            {
               layerDimForm.UpgradeOpen();
               layerDimForm.IsFrozen = true;
            }
         }
      }
   }
}

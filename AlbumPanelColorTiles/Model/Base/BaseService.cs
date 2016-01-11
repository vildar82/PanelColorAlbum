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

namespace AlbumPanelColorTiles.Model.Base
{
   public class BaseService
   {      
      private Dictionary<string,Panel> _panelsFromBase;
      public CreatePanelsBtrEnvironment Env { get; private set; }
      public string XmlBasePanelsFile { get; set; }
      public int CountPanelsInBase { get { return (_panelsFromBase == null) ? 0 : _panelsFromBase.Count; } }
      public Database Db { get; set; }

      public BaseService()
      {
         XmlBasePanelsFile = @"c:\dev\АР\AlbumPanelColorTiles\PanelColorAlbum\AlbumPanelColorTiles\Model\Base\Panels.xml";
      }

      public BaseService(string xmlBasePanelsFile)
      {
         XmlBasePanelsFile = xmlBasePanelsFile;
      }
      

      public void InitToCreationPanels(Database db)
      {
         Db = db;
         Env = new CreatePanelsBtrEnvironment(this); 
      }

      public void ReadPanelsFromBase()
      {
         if (!File.Exists(XmlBasePanelsFile))
         {
            throw new FileNotFoundException("XML файл базы панелей не найден.", XmlBasePanelsFile);
         }

         // TODO: Проверка валидности xml         

         // Чтение файла базы панелей
         _panelsFromBase = new Dictionary<string, Panel>();
         XmlSerializer ser = new XmlSerializer(typeof(Base.Panels));
         using (var fileStreamXml = new FileStream(XmlBasePanelsFile, FileMode.Open))
         {
            Panels panels = ser.Deserialize(fileStreamXml) as Panels;
            var panelsList = panels.Panel.ToList();
            foreach (var panel in panelsList)
            {
               try
               {
                  _panelsFromBase.Add(panel.mark.ToUpper(), panel);
               }
               catch (ArgumentException ex)
               {
                  Inspector.AddError("Ошибка получения панели из базы xml - такая панель уже есть. {0}", ex.Message);
               }
            }
         }
      }
      
      public void ClearPanelsAkrFromDrawing(Database db)
      {
         SelectionBlocks sel = new SelectionBlocks(db);
         sel.SelectAKRPanelsBtr();
         List<ObjectId> idsBtrPanel = sel.IdsBtrPanelAr;
         idsBtrPanel.AddRange(sel.IdsBtrPanelSb);
         foreach (ObjectId idBtrPanel in idsBtrPanel)
         {
            using (var btrPanel = idBtrPanel.GetObject(OpenMode.ForWrite) as BlockTableRecord)
            {
               foreach (ObjectId idBlRefPanel in btrPanel.GetBlockReferenceIds(false, true))
               {
                  using (var blRefPanel = idBlRefPanel.GetObject(OpenMode.ForWrite, false, true) as BlockReference)
                  {
                     blRefPanel.Erase();
            }
               }
               btrPanel.Erase();
            }
         }
      }     

      public Panel CreateBtrPanel(string markSb)
            {
         Panel panel;
         if (_panelsFromBase.TryGetValue(markSb.ToUpper(), out panel))
         {
            panel.CreateBlock(this);            
         }
         else
         {
            // Ошибка - панели с такой маркой нет в базе
            throw new ArgumentException("Панели с такой маркой нет в базе - {0}".f(markSb), "markSb");
         }
         return panel;
      }

      public void CreateBtrPanels(List<FacadeMounting> facadesMounting)
      {
         var panelsMountUnique = facadesMounting.SelectMany(f => f.Floors?.SelectMany(fl => fl.PanelsSbInFront)).
                                             GroupBy(p => p.MarkSb).Select(g=>g.First());
         foreach (var panelMount in panelsMountUnique)
         {
            try
            {
               Panel panelBase = CreateBtrPanel(panelMount.MarkSb);
               panelMount.PanelAkr = new PanelAKR(panelBase);
            }
            catch (Exception ex)
            {
               Inspector.AddError("Не создана панель {0}. Ошибка - {1}", panelMount.MarkSb, ex.Message);
            }
         }
      }
   }
}

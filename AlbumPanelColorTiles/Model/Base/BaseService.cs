using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib.Errors;
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

      public BaseService()
      {
         XmlBasePanelsFile = @"c:\dev\АР\AlbumPanelColorTiles\PanelColorAlbum\AlbumPanelColorTiles\Model\Base\Panels.xml";
      }

      public BaseService(string xmlBasePanelsFile)
      {
         XmlBasePanelsFile = xmlBasePanelsFile;
      }
      
      public ObjectId CreateBtrPanel (string markSb)
      {
         ObjectId resIdBtrPanel = ObjectId.Null;
         Panel panel;         
         if ( _panelsFromBase.TryGetValue(markSb.ToUpper(), out panel ))
         {
            resIdBtrPanel = panel.CreateBlock(this);
         }
         else
         {
            // Ошибка - панели с такой маркой нет в базе
            throw new ArgumentException("Панели с такой маркой нет в базе - {0}".f(markSb), "markSb");
         }
         return resIdBtrPanel;
      }

      public void InitToCreationPanels()
      {
         Env = new CreatePanelsBtrEnvironment(); 
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
   }
}

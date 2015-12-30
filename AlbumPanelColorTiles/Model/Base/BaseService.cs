using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib.Errors;
using AlbumPanelColorTiles.PanelLibrary;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BaseService
   {
      private Dictionary<string,Panel> _panelsFromBase;

      public string XmlBasePanelsFile { get; set; }

      public BaseService()
      {
         XmlBasePanelsFile = @"c:\dev\АР\AlbumPanelColorTiles\PanelColorAlbum\AlbumPanelColorTiles\Model\Base\Panels.xml";
      }
      public BaseService(string xmlBasePanelsFile)
      {
         XmlBasePanelsFile = xmlBasePanelsFile;
      }

      public void ReadPanelsFromBase()
      {
         if (File.Exists(XmlBasePanelsFile))
         {
            throw new FileNotFoundException("XML файл базы панелей не найден.", XmlBasePanelsFile);
         }

         // TODO: Проверка валидности xml

         // Чтение файла базы панелей
         _panelsFromBase = new Dictionary<string, Panel>();
         XmlSerializer ser = new XmlSerializer(typeof(Base.Panels));                  
         Panels panels = ser.Deserialize(new FileStream(XmlBasePanelsFile, FileMode.Open)) as Panels;
         var panelsList = panels.Panel.ToList();
         foreach (var panel in panelsList)
         {
            try
            {
               _panelsFromBase.Add(panel.mark, panel);
            }
            catch (Exception)
            {

               throw;
            }            
         }
      }
   }
}

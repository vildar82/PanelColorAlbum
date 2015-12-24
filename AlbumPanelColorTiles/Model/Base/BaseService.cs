using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AlbumPanelColorTiles.Model.Base
{
   public class BaseService
   {
      public void LoadPanels()
      {
         XmlSerializer ser = new XmlSerializer(typeof(Base.Panels));         
         string filename = Path.Combine(Commands.CurDllDir, @"Model\Base\Panels.xml");
         Panels panels = ser.Deserialize(new FileStream(filename, FileMode.Open)) as Panels;         
      }
   }
}

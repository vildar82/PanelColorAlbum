using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Краска
   public class Paint
   {
      // Имя слоя. (для каждой краски свой слой с именем марки краски)
      private string _layerName;  
      
      public Paint (string layerName)
      {
         _layerName = layerName;
      }

      public string LayerName
      {
         get { return _layerName; }         
      }
   }
}

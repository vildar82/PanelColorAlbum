using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Utils.AirConditioners
{
    public class AirConditioner
    {
        public ObjectId IdBlRef { get; set; }
        public string Mark { get; set; }
        public string ColorName { get; set; }
        public Color Color { get; set; }

        public AirConditioner(BlockReference blRef)
        {
            IdBlRef = blRef.Id;
            ParseLayer(blRef.Layer);
        }

        private void ParseLayer(string layer)
        {
            // пример имени слоя кондиционера - АР_Кондиционеры_1_NCS S 0300-N_белый
            if (layer.StartsWith("АР_Кондиционер", StringComparison.OrdinalIgnoreCase))
            {
                var splits = layer.Split('_');
            }                                    
        }
    }
}

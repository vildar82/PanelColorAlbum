using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Blocks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Utils.AirConditioners
{
    public class AirConditioner
    {
        public ObjectId IdBlRef { get; set; }
        public int Mark { get; set; }
        public string ColorName { get; set; } = string.Empty;      
        public Color Color { get; set; }
        public AcadLib.Blocks.AttributeInfo AtrRefMark { get; set; }
        public string Error { get; set; }

        public AirConditioner(BlockReference blRef)
        {
            IdBlRef = blRef.Id;
            Color = getLayerColor(blRef.LayerId);
            AtrRefMark = getAttRefMark(blRef);
            ParseLayer(blRef.Layer);
        }        

        private void ParseLayer(string layer)
        {
            // пример имени слоя кондиционера - АР_Кондиционеры_1_NCS S 0300-N_белый
            if (layer.StartsWith("АР_Кондиционер", StringComparison.OrdinalIgnoreCase))
            {
                var splits = layer.Split('_');
                if (splits.Length>3)
                {
                    Mark = getNum(splits[2]);
                    ColorName = splits[3].Trim();
                }
            }
            else
            {
                ColorName = layer;
                addErr("Имя слоя должно иметь вид, например: АР_Кондиционеры_1_NCS S 0300, где 1 - марка, NCS S 0300 цвет.");
            }                                    
        }       

        private Color getLayerColor(ObjectId layerId)
        {
            using (var layer = layerId.Open( OpenMode.ForRead) as LayerTableRecord)
            {
                return layer.Color;
            }
        }

        private AttributeInfo getAttRefMark(BlockReference blRef)
        {
            if (blRef.AttributeCollection != null)
            {
                foreach (ObjectId idAtrRef in blRef.AttributeCollection)
                {
                    using (var atrRef = idAtrRef.Open( OpenMode.ForRead, false, true)as AttributeReference)
                    {
                        if (atrRef.Tag.Equals("НОМЕР", StringComparison.OrdinalIgnoreCase))
                        {
                            return new AttributeInfo(atrRef);
                        }
                    }
                }
            }
            addErr("Не найден атрибут 'НОМЕР' для заполнения марки кондиционера.");
            return null;
        }

        private int getNum(string input)
        {
            try
            {
                return int.Parse(input);
            }
            catch
            {
                addErr($"Не определен номер кондиционера из имени слоя.");
                return 0;
            }
        }

        private void addErr(string err)
        {
            if (string.IsNullOrEmpty(Error))
            {
                Error = err;
            }
            else
            {
                Error += "; " + err;
            }
        }
    }
}

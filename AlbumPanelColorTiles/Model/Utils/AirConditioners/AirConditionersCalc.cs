using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Utils.AirConditioners
{
    public class AirConditionersCalc
    {
        private Document doc;
        private Database db;
        private Editor ed;

        public AirConditionersCalc()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
        }

        public void Calc()
        {
            // Запрос выбора блоков кондиционеров
            var selBls = ed.SelectBlRefs("Выбор кондициоенор для спецификации");

            // Фильтр блоков кондициоеров
            var airConds = Filter(selBls);
        }

        private List<AirConditioner> Filter(List<ObjectId> selBls)
        {
            Dictionary<Point3d, AirConditioner> dictAirConds = new Dictionary<Point3d, AirConditioner>();
            foreach (var idBlRef in selBls)
            {
                using (var blRef = idBlRef.Open( OpenMode.ForRead, false, true)as BlockReference)
                {
                    var blName = blRef.GetEffectiveName();
                    if (blName.Equals("АР_Решетка_Кондиционера", StringComparison.OrdinalIgnoreCase) )
                    {
                        var airCond = new AirConditioner(blRef);
                    }                    
                }
            }
            return dictAirConds.Values.ToList();
        }
    }
}

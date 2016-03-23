using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
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

            // подсчет кодиц
            var condRows = getCondRows(airConds);

            // заполнение номеров в атрбутах кондиционеров
            fillMarksAttr(condRows);

            AirCondTable airTable = new AirCondTable(condRows);
            airTable.CreateTable();
        }        

        private List<AirConditioner> Filter(List<ObjectId> selBls)
        {
            List<AirConditioner> airConds = new List<AirConditioner>();
            foreach (var idBlRef in selBls)
            {
                using (var blRef = idBlRef.Open( OpenMode.ForRead, false, true)as BlockReference)
                {
                    var blName = blRef.GetEffectiveName();
                    if (blName.Equals("АР_Корзина_Кондиционера", StringComparison.OrdinalIgnoreCase) )
                    {
                        var airCond = new AirConditioner(blRef);
                        airConds.Add(airCond);
                        if (!string.IsNullOrEmpty(airCond.Error))
                        {                         
                            Inspector.AddError($"Ошибки в блоке кондиционера - {airCond.Error}", blRef, System.Drawing.SystemIcons.Warning);
                        }
                    }                    
                }
            }
            return airConds;
        }

        private List<AirCondRow> getCondRows(List<AirConditioner> airConds)
        {
            List<AirCondRow> condRows = new List<AirCondRow>();
            var groupsCond = airConds.GroupBy(c => c.Color);
            foreach (var groupCond in groupsCond)
            {
                AirCondRow condRow = new AirCondRow(groupCond);
                condRows.Add(condRow);
            }

            // сортировка строк по номеру
            var maxMark = condRows.Max(c => c.Mark);
            var zeroMarks = condRows.Where(c => c.Mark == 0);
            foreach (var item in zeroMarks)
            {
                item.Mark = ++maxMark;
            }
            condRows = condRows.OrderBy(c => c.Mark).ToList();
            return condRows;
        }

        private void fillMarksAttr(List<AirCondRow> condRows)
        {
            foreach (var groupCond in condRows)
            {
                foreach (var cond in groupCond.AirConds)
                {
                    using (var atrRef = cond.AtrRefMark.IdAtr.Open(OpenMode.ForWrite, false, true) as AttributeReference)
                    {
                        atrRef.TextString = groupCond.Mark.ToString();
                    }
                }
            }
        }
    }
}

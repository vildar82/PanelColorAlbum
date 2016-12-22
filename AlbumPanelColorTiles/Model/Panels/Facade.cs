using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;

namespace AlbumPanelColorTiles.Panels
{
    // Фасад архитектурный
    // определяется блоком АКР_Фасад которым обозначены фасады с панелями.
    public class Facade
    {
        public ObjectId IdBlRefFacade; // Блок АКР_Фасад

        public Facade(ObjectId idBlRefFacade)
        {
            IdBlRefFacade = idBlRefFacade;
        }

        public AttributeRefDetail AttrAxis1 { get; private set; }
        public AttributeRefDetail AttrAxis2 { get; private set; }
        public string Axis1 { get; private set; }
        public string Axis2 { get; private set; }
        public Extents3d Extents { get; private set; }

        public string Name {
            get {
                if (string.IsNullOrEmpty(Axis1) || string.IsNullOrEmpty(Axis2))
                {
                    return Extents.MinPoint.X.ToString();
                }
                return string.Format("{0}_{1}", Axis1, Axis2);
            }
        }

        /// <summary>
        /// Фасады по всем блокам фасадов - могут быть одинаковые фасады, из-за блоков фасадов на монтажных планах
        /// </summary>
        /// <param name="facadeBlRefs">Блоки фасадов</param>
        public static List<Facade> GetFacades(List<ObjectId> facadeBlRefs)
        {
            List<Facade> facades = new List<Facade>();
            foreach (var idBlRefSection in facadeBlRefs)
            {
                var facade = new Facade(idBlRefSection);
                facade.define();
                facades.Add(facade);
            }
            return facades;
        }

        private void define()
        {
            // определение фасада - параметров блока фасада
            using (var blRef = IdBlRefFacade.Open(OpenMode.ForRead, false, true) as BlockReference)
            {
                Extents = blRef.GeometricExtents;
                defineAttr(blRef);
            }
        }

        private void defineAttr(BlockReference blRef)
        {
            if (blRef.AttributeCollection != null)
            {
                foreach (ObjectId idAtrRef in blRef.AttributeCollection)
                {
                    if (!idAtrRef.IsValidEx()) continue;
                    using (var atrRef = idAtrRef.Open(OpenMode.ForRead, false, true) as AttributeReference)
                    {
                        if (string.Equals(atrRef.Tag, Settings.Default.AttributeFacadeAxis1))
                        {
                            AttrAxis1 = new AttributeRefDetail(atrRef);
                            Axis1 = atrRef.TextString.Trim();
                        }
                        else if (string.Equals(atrRef.Tag, Settings.Default.AttributeFacadeAxis2))
                        {
                            AttrAxis2 = new AttributeRefDetail(atrRef);
                            Axis2 = atrRef.TextString.Trim();
                        }
                    }
                }
            }
        }
    }
}
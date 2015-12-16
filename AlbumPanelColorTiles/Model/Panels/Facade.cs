using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.PanelLibrary;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Panels
{
   // Фасад архитектурный
   // определяется блоком АКР_Фасад которым обозначены фасады с панелями.
   public class Facade
   {      
      public ObjectId IdBlRefFacade; // Блок АКР_Фасад
      public string Axis1 { get; private set; }
      public string Axis2 { get; private set; }
      public AttributeRefDetail AttrAxis1 { get; private set; }
      public AttributeRefDetail AttrAxis2 { get; private set; }
      public Extents3d Extents { get; private set; }
      public string Name
      {
         get
         {
            return string.Format("{0}_{1}", Axis1, Axis2);
         }
      }

      public Facade (ObjectId idBlRefFacade)
      {
         IdBlRefFacade = idBlRefFacade;         
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
         using (var blRef = IdBlRefFacade.Open( OpenMode.ForRead, false, true )as BlockReference)
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
               using (var atrRef = idAtrRef.Open( OpenMode.ForRead, false, true) as AttributeReference)
               {
                  if (string.Equals(atrRef.Tag, Settings.Default.AttributeFacadeAxis1))
                  {
                     AttrAxis1 = new AttributeRefDetail(atrRef);
                  }
                  else if (string.Equals(atrRef.Tag, Settings.Default.AttributeFacadeAxis2))
                  {
                     AttrAxis2 = new AttributeRefDetail(atrRef);
                  }
               }
            }
         }
      }      
   }
}

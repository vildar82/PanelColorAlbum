using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // описание AttributeReference для хранения
   public class AttributeRefDetail
   {
      public ObjectId IdAtrRef { get; private set; }
      public string Tag { get; private set; }
      public string Text { get; private set; }

      public AttributeRefDetail(AttributeReference attr)
      {
         Tag = attr.Tag;
         Text = attr.TextString;
         IdAtrRef = attr.Id;
      }      
   }
}
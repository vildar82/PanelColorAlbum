using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // описание AttributeReference для хранения
   public class AttributeRefDetail
   {
      private ObjectId _idAtrRef;
      private string _tag;
      private string _text;

      public AttributeRefDetail(AttributeReference attr)
      {
         _tag = attr.Tag;
         _text = attr.TextString;
         _idAtrRef = attr.Id;
      }

      public ObjectId IdAtrRef { get { return _idAtrRef; } }
      public string Tag { get { return _tag; } }
      public string Text { get { return _text; } }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // описание AttributeReference для хранения
   public class AttributeRefDetail
   {
      private string _tag;
      private string _text;
      private ObjectId _idAtrRef;      

      public AttributeRefDetail (AttributeReference attr)
      {
         _tag = attr.Tag;
         _text = attr.TextString;
         _idAtrRef = attr.Id;
      }

      public string Tag { get { return _tag; } }
      public string Text { get { return _text; } }
      public ObjectId IdAtrRef { get { return _idAtrRef; } }      
   }
}

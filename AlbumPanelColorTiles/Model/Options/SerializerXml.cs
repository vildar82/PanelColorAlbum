using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace AlbumPanelColorTiles.Options
{
   public class SerializerXml
   {
      private string _file;

      public SerializerXml(string file)
      {
         _file = file;
      }

      public T DeserializeXmlFile<T>()
      {
         XmlSerializer ser = new XmlSerializer(typeof(T));
         using (XmlReader reader = XmlReader.Create(_file))
         {
            try
            {
               return (T)ser.Deserialize(reader);
            }
            catch (Exception ex)
            {
               Logger.Log.Error(ex, "DeserializeXmlFile {0}", _file);
               throw;
            }
         }
      }

      public void SerializeList<T>(T settings)
      {
         using (FileStream fs = new FileStream(_file, FileMode.Create, FileAccess.Write))
         {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            ser.Serialize(fs, settings);
         }
      }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Checks
{
   public class Error
   {
      private string _msg;
      private string _shortMsg;
      private ObjectId _idEnt;
      private Extents3d _extents;
      private bool _hasEntity;      

      public string Message { get { return _msg; } }
      public string ShortMsg { get { return _shortMsg; } }
      public ObjectId IdEnt { get { return _idEnt; } }
      public Extents3d Extents { get { return _extents; } }
      public bool HasEntity { get { return _hasEntity; } }      

      public Error(string message)
      {
         _msg = message;
         _shortMsg = getShortMsg(_msg);
         _hasEntity = false;         
      }     

      public Error(string message, Entity ent)
      {
         _msg = message;
         _shortMsg = getShortMsg(_msg);
         _idEnt = ent.Id;
         _extents = ent.GeometricExtents;
         _hasEntity = true;         
      }

      public Error(string message, Extents3d ext, Entity ent)
      {
         _msg = message;
         _shortMsg = getShortMsg(_msg);
         _idEnt = ent.Id;
         _extents = ext;         
         _hasEntity = true;
      }

      private string getShortMsg(string msg)
      {
         if (msg.Length > 100)
         {
            return msg.Substring(0, 99);
         }
         else
         {
            return msg;
         }
      }
   }
}

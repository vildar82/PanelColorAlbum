using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class EntityInfo : IEquatable<EntityInfo>//, IComparable<EntityInfo>
   {
      private Guid _classId;
      private System.Drawing.Color _color;
      private Extents3d _extents;
      private string _layer;
      private string _linetype;
      private LineWeight _lineweight;

      public EntityInfo(Entity ent)
      {
         _extents = ent.GeometricExtents;
         _classId = ent.ClassID;
         _color = ent.Color.ColorValue;
         _layer = ent.Layer;
         _linetype = ent.Linetype;
         _lineweight = ent.LineWeight;
      }

      public static List<EntityInfo> GetEntInfoBtr(ObjectId idBtrTest)
      {
         List<EntityInfo> entsInfo = new List<EntityInfo>();
         using (var btr = idBtrTest.Open(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in btr)
            {
               using (var ent = idEnt.Open(OpenMode.ForRead) as Entity)
               {
                  entsInfo.Add(new EntityInfo(ent));
                  if (ent is BlockReference)
                  {
                     if (!string.Equals(((BlockReference)ent).Name, Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        entsInfo.AddRange(GetEntInfoBtr(((BlockReference)ent).BlockTableRecord));
                     }
                  }
               }
            }
         }
         return entsInfo;
      }

      public bool Equals(EntityInfo other)
      {
         if (Object.ReferenceEquals(other, null)) return false;
         if (Object.ReferenceEquals(this, other)) return true;
         return _extents.Equals(other._extents) &&
            _classId.Equals(other._classId) &&
            _color.Equals(other._color) &&
            _layer.Equals(other._layer) &&
            _linetype.Equals(other._linetype) &&
            _lineweight.Equals(other._lineweight);
      }

      //public override int GetHashCode()
      //{
      //   return _extents.GetHashCode() ^ _classId.GetHashCode() ^ _color?.GetHashCode() ?? 0 ^ _linetype?.GetHashCode() ?? 0 ^ _lineweight.GetHashCode();
      //}

      //public int CompareTo(EntityInfo other)
      //{
      //   int res = _extents.GetHashCode().CompareTo(other._extents.GetHashCode());
      //   if (res == 0) res = _classId.CompareTo(other._classId);
      //   if (res == 0) res = _color.CompareTo(other._color);
      //   if (res == 0) res = _layer.CompareTo(other._layer);
      //   if (res == 0) res = _linetype.CompareTo(other._linetype);
      //   if (res == 0) res = _lineweight.CompareTo(other._lineweight);
      //   return res;
      //}
   }
}
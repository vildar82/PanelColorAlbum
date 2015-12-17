using System;
using System.Collections.Generic;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class EntityInfo : IEquatable<EntityInfo>//, IComparable<EntityInfo>
   {
      public ObjectId Id { get; set; }
      public Guid ClassId { get; set; }
      public string ClassName { get; set; }
      public System.Drawing.Color Color { get; set; }
      public Extents3d Extents { get; set; }
      public string Layer;
      public string Linetype;
      public LineWeight Lineweight;

      public EntityInfo(Entity ent)
      {
         ClassName = ent.GetRXClass().Name;
         Id = ent.Id;
         Extents = ent.GeometricExtents;
         ClassId = ent.ClassID;
         Color = ent.Color.ColorValue;
         Layer = ent.Layer;
         Linetype = ent.Linetype;
         Lineweight = ent.LineWeight;
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
         return Extents.Equals(other.Extents) &&
            ClassId.Equals(other.ClassId) &&
            Color.Equals(other.Color) &&
            Layer.Equals(other.Layer) &&
            Linetype.Equals(other.Linetype) &&
            Lineweight.Equals(other.Lineweight);
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
using System;
using System.Collections.Generic;
using System.Linq;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Panels
{
   // Описание блока Секции
   public class Section : IComparable<Section>
   {
      private int _num;

      public Section(ObjectId idBlRef)
      {
         IdsBlRef = new List<ObjectId>();
         ExtentsList = new List<Extents3d>();
         IdsBlRef.Add(idBlRef);
         Panels = new List<AlbumPanelColorTiles.Panels.Panel>();
         getSectionParams(idBlRef);
      }

      public List<Extents3d> ExtentsList { get; private set; }
      public List<ObjectId> IdsBlRef { get; private set; }
      public string Name { get; private set; }
      public List<AlbumPanelColorTiles.Panels.Panel> Panels { get; private set; }

      public static void DefineSections(Album album)
      {
         var panelsAll = album.MarksSB.SelectMany(sb => sb.MarksAR.SelectMany(ar => ar.Panels));
         foreach (var panel in panelsAll)
         {
            foreach (var section in album.Sections)
            {
               if (section.IsPointInSection(panel.InsPt))
               {
                  section.AddPanel(panel);
               }
            }
         }
      }

      public static List<Section> GetSections(List<ObjectId> sectionsBlRefs)
      {
         Dictionary<string, Section> sections = new Dictionary<string, Section>();
         foreach (var idBlRefSection in sectionsBlRefs)
         {
            var sectionNew = new Section(idBlRefSection);
            string key = sectionNew.Name.ToUpper();
            Section section;
            if (sections.TryGetValue(key, out section))
            {
               section.AddSection(sectionNew);
            }
            else
            {
               sections.Add(key, sectionNew);
            }
         }
         var res = sections.Values.ToList();
         res.Sort();
         return res;
      }

      public int CompareTo(Section other)
      {
         if (_num == 0)
         {
            return Name.CompareTo(other.Name);
         }
         else
         {
            return _num.CompareTo(other._num);
         }
      }

      private void AddPanel(AlbumPanelColorTiles.Panels.Panel panel)
      {
         Panels.Add(panel);
         panel.Section = this;
      }

      private void AddSection(Section sectionNew)
      {
         this.IdsBlRef.Add(sectionNew.IdsBlRef.First());
         this.ExtentsList.Add(sectionNew.ExtentsList.First());
      }

      private void getSectionParams(ObjectId idBlRef)
      {
         using (var blRef = idBlRef.Open(OpenMode.ForRead, false, true) as BlockReference)
         {
            ExtentsList.Add(blRef.GeometricExtents);
            if (blRef.AttributeCollection != null)
            {
               foreach (ObjectId idAtrRef in blRef.AttributeCollection)
               {
                  using (var atrRef = idAtrRef.Open(OpenMode.ForRead, false, true) as AttributeReference)
                  {
                     if (string.Equals(atrRef.Tag, Settings.Default.AttributeSectionName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        Name = atrRef.TextString.Trim();
                        int.TryParse(Name, out _num);
                        break;
                     }
                  }
               }
            }
         }
      }

      private bool IsPointInSection(Point3d insPt)
      {
         foreach (var extents in this.ExtentsList)
         {
            if (extents.IsPointInBounds(insPt))
            {
               return true;
            }
         }
         return false;
      }
   }
}
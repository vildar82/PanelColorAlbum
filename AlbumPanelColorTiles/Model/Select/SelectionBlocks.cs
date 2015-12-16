using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.Select
{
   // Выбор панелей в чертеже
   public class SelectionBlocks
   {
      /// <summary>
      /// Вхождения блоков панелей Марки АР в Модели
      /// </summary>
      public List<ObjectId> IdsBlRefPanelAr { get; private set; }
      /// <summary>
      /// Вхождения блоков панелей Марки СБ в Модели
      /// </summary>
      public List<ObjectId> IdsBlRefPanelSb { get; private set; }

      /// <summary>
      /// Определения блоков панелей Марки СБ
      /// </summary>
      public List<ObjectId> IdsBtrPanelSb { get; private set; }
      /// <summary>
      /// Определения блоков панелей Марки АР
      /// </summary>
      public List<ObjectId> IdsBtrPanelAr { get; private set; }

      /// <summary>
      /// Блоки Секций в Модели
      /// </summary>
      public List<ObjectId> SectionsBlRefs { get; private set; }

      /// <summary>
      /// Блоки Фасадов в Модели
      /// </summary>
      public List<ObjectId> FacadeBlRefs { get; private set; }

      private Database _db;

      public SelectionBlocks(Database db)
      {
         _db = db;
      }

      public SelectionBlocks()
      {
         _db = HostApplicationServices.WorkingDatabase;
      }

      /// <summary>
      /// Выбор вхождений блоков панелей Марки АР и Марки СБ в Модели
      /// + выбор блоков Секций.
      /// + выбор фасадов
      /// </summary>
      public void SelectBlRefsInModel()
      {
         IdsBlRefPanelAr = new List<ObjectId>();
         IdsBlRefPanelSb = new List<ObjectId>();
         SectionsBlRefs = new List<ObjectId>();
         FacadeBlRefs = new List<ObjectId>();
         using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(_db).Open(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRef = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     // Блоки АКР-Панелей
                     if (MarkSb.IsBlockNamePanel(blRef.Name))
                     {
                        if (MarkSb.IsBlockNamePanelMarkAr(blRef.Name))
                        {
                           IdsBlRefPanelAr.Add(idEnt);
                        }
                        else
                        {
                           IdsBlRefPanelSb.Add(idEnt);
                        }
                     }
                     // Блоки Секций
                     else if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockSectionName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        SectionsBlRefs.Add(idEnt);
                     }
                     // Блоки Фасадов
                     else if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockFacadeName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        FacadeBlRefs.Add(idEnt);
                     }
                  }
               }
            }
         }               
      }

      /// <summary>
      /// Выбор определений блоков панелей Марки АР и Марки СБ
      /// </summary>
      public void SelectAKRPanelsBtr()
      {
         IdsBtrPanelAr = new List<ObjectId>();
         IdsBtrPanelSb = new List<ObjectId>();
         using (var bt = _db.BlockTableId.Open( OpenMode.ForRead) as BlockTable)
         {
            foreach (ObjectId idEnt in bt)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var btr = idEnt.Open(OpenMode.ForRead) as BlockTableRecord)
                  {
                     if (MarkSb.IsBlockNamePanel(btr.Name))
                     {
                        if (MarkSb.IsBlockNamePanelMarkAr(btr.Name))
                        {
                           IdsBtrPanelAr.Add(idEnt);
                        }
                        else
                        {
                           IdsBtrPanelSb.Add(idEnt);
                        }
                     }
                  }
               }
            }
         }
      }

      public void SelectSectionBlRefs()
      {
         SectionsBlRefs = new List<ObjectId>();
         using (var ms = SymbolUtilityServices.GetBlockModelSpaceId(_db).Open(OpenMode.ForRead) as BlockTableRecord)
         {
            foreach (ObjectId idEnt in ms)
            {
               if (idEnt.ObjectClass.Name == "AcDbBlockReference")
               {
                  using (var blRef = idEnt.Open(OpenMode.ForRead, false, true) as BlockReference)
                  {
                     if (string.Equals(blRef.GetEffectiveName(), Settings.Default.BlockSectionName, StringComparison.CurrentCultureIgnoreCase))
                     {
                        SectionsBlRefs.Add(idEnt);
                     }
                  }
               }
            }
         }
      }
   }
}

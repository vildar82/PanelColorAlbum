using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   public class ConvertPanelService
   {
      public ExportFacadeService Service { get; private set; }
      public Database DbExport { get; set; }  
      public List<PanelBtrExport> PanelsBtrExport { get; private set; }

      // Слой для контура панелей
      private ObjectId _idLayerContour;
      public ObjectId IdLayerContour
      {
         get
         {
            if (_idLayerContour.IsNull)
            {
               // Создание контура плитки
               var layer = new AcadLib.Layers.LayerInfo("АР_Швы");
               layer.LineWeight = LineWeight.LineWeight030;
               _idLayerContour = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(layer);
            }
            return _idLayerContour;
         }
      }

      public ConvertPanelService(ExportFacadeService service)
      {
         Service = service;         
      }      

      /// <summary>
      /// преобразование панеелей
      /// </summary>
      public void Convert()
      {         
         // Преобразования определений блоков панелей         
         foreach (var panelBtr in PanelsBtrExport)
         {            
            try
            {
               panelBtr.Convert();
               if (!string.IsNullOrEmpty(panelBtr.ErrMsg))
               {
                  Inspector.AddError(panelBtr.ErrMsg);
               }               
            }
            catch (Exception ex)
            {
               Inspector.AddError("Ошибка конвертации блока панели - {0}", ex.Message);
               Log.Error(ex, "Ошибка конвертиации экспортрированного блока панели");
            }            
         }         
      }

      private void purge()
      {
         // Очистка экспортированного чертежа от блоков образмеривания которые были удалены из панелей после копирования
         ObjectIdGraph graph = new ObjectIdGraph();
         foreach (var panelBtr in PanelsBtrExport)
         {
            ObjectIdGraphNode node = new ObjectIdGraphNode(panelBtr.IdBtrExport);
            graph.AddNode(node);
         }
         DbExport.Purge(graph);
      }

      public void DefinePanels(List<Facade> facades)
      {
         // определение экспортируемых панелей - в файле АКР
         Dictionary<ObjectId, PanelBtrExport> dictPanelsBtrExport = new Dictionary<ObjectId, PanelBtrExport>();

         RTreeLib.RTree<Facade> treeFacades = new RTreeLib.RTree<Facade>();
         facades.ForEach(f => treeFacades.Add(ColorArea.GetRectangleRTree(f.Extents), f));

         foreach (var idBlRefPanel in Service.SelectPanels.IdsBlRefPanelAr)
         {
            using (var blRef = idBlRefPanel.Open(OpenMode.ForRead, false, true) as BlockReference)
            {
               // панель определения блока
               PanelBtrExport panelBtrExport;
               if (!dictPanelsBtrExport.TryGetValue(blRef.BlockTableRecord, out panelBtrExport))
               {
                  panelBtrExport = new PanelBtrExport(blRef.BlockTableRecord);
                  dictPanelsBtrExport.Add(blRef.BlockTableRecord, panelBtrExport);
               }
               panelBtrExport.Def();

               // панель вхождения блока
               PanelBlRefExport panelBlRefExport = new PanelBlRefExport(blRef, panelBtrExport);
               panelBtrExport.Panels.Add(panelBlRefExport);

               // определение фасада панели
               RTreeLib.Point pt = new RTreeLib.Point(panelBlRefExport.Position.X, panelBlRefExport.Position.Y, 0);
               var facadesFind = treeFacades.Nearest(pt, 100);
               if (facadesFind.Count == 1)
               {
                  Facade facade = facadesFind.First();
                  panelBlRefExport.Facade = facade;
               }
               else if (facadesFind.Count == 0)
               {
                  Inspector.AddError(string.Format("Не определен фасад для панели {0}", panelBtrExport.BlName),
                                       blRef.GeometricExtents, idBlRefPanel);
               }
               else if (facadesFind.Count > 1)
               {
                  Inspector.AddError(string.Format("Найдено больше одного фасада для панели {0}", panelBtrExport.BlName),
                                      blRef.GeometricExtents, idBlRefPanel);
               }
            }
         }
         PanelsBtrExport = dictPanelsBtrExport.Values.ToList();
      }
   }
}

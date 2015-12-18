using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Comparers;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

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
      public void ConvertBtr()
      {
         ProgressMeter progress = new ProgressMeter();
         progress.SetLimit(PanelsBtrExport.Count);
         progress.Start("Преобразование блоков панелей в экспортированном файле");
         // Преобразования определений блоков панелей         
         foreach (var panelBtr in PanelsBtrExport)
         {
            progress.MeterProgress();
            try
            {
               panelBtr.ConvertBtr();
               if (!string.IsNullOrEmpty(panelBtr.ErrMsg))
               {
                  Inspector.AddError(panelBtr.ErrMsg, panelBtr.Panels.First().Extents, panelBtr.Panels.First().IdBlRefAkr);
               }               
            }
            catch (System.Exception ex)
            {
               Inspector.AddError("Ошибка конвертации блока панели - {0}", ex.Message);
               Log.Error(ex, "Ошибка конвертиации экспортрированного блока панели");
            }            
         }
         progress.Stop();
      }

      public void Purge()
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

         ProgressMeter progress = new ProgressMeter();
         progress.SetLimit(Service.SelectPanels.IdsBlRefPanelAr.Count);
         progress.Start("Определение панелей в файле АКР");

         foreach (var idBlRefPanel in Service.SelectPanels.IdsBlRefPanelAr)
         {
            progress.MeterProgress();
            using (var blRef = idBlRefPanel.Open(OpenMode.ForRead, false, true) as BlockReference)
            {
               // панель определения блока
               PanelBtrExport panelBtrExport;
               if (!dictPanelsBtrExport.TryGetValue(blRef.BlockTableRecord, out panelBtrExport))
               {
                  panelBtrExport = new PanelBtrExport(blRef.BlockTableRecord, this);
                  dictPanelsBtrExport.Add(blRef.BlockTableRecord, panelBtrExport);
               }
               panelBtrExport.Def();

               // панель вхождения блока
               PanelBlRefExport panelBlRefExport = new PanelBlRefExport(blRef, panelBtrExport);
               panelBtrExport.Panels.Add(panelBlRefExport);

               // определение фасада панели
               panelBlRefExport.Facade = defFacadeForPanel(treeFacades, blRef, panelBtrExport, panelBlRefExport);               
            }
         }
         PanelsBtrExport = dictPanelsBtrExport.Values.ToList();
         progress.Stop();
      }

      public void ConvertEnds()
      {
         // Преобразование торцов фасада
         List<ConvertEndsFacade> convertsEnds = new List<ConvertEndsFacade>();
         DoubleEqualityComparer comparer = new DoubleEqualityComparer(500);
         // Все вхождения блоков панелей с торцами слева         
         var panelsWithLeftEndsByX = PanelsBtrExport.SelectMany(pBtr => pBtr.Panels).
                     Where(pBlRef => pBlRef.PanelBtrExport.IdsEndsLeftEntity.Count > 0).
                     GroupBy(pBlRef => pBlRef.Position.X, comparer);
         foreach (var itemLefEndsByY in panelsWithLeftEndsByX)
         {
            ConvertEndsFacade convertEndsFacade = new ConvertEndsFacade(itemLefEndsByY, true, this);
            convertEndsFacade.Convert();
            convertsEnds.Add(convertEndsFacade);
         }

         // Все вхождения блоков панелей с торцами справа         
         var panelsWithRightEndsByX = PanelsBtrExport.SelectMany(pBtr => pBtr.Panels).
                     Where(pBlRef => pBlRef.PanelBtrExport.IdsEndsRightEntity.Count > 0).
                     GroupBy(pBlRef => pBlRef.Position.X, comparer);
         foreach (var itemRightEndsByY in panelsWithRightEndsByX)
         {
            ConvertEndsFacade convertEndsFacade = new ConvertEndsFacade(itemRightEndsByY, false, this);
            convertEndsFacade.Convert();
            convertsEnds.Add(convertEndsFacade);
         }

         // удаление торцов
         convertsEnds.ForEach(c => c.DeleteEnds());
      }

      private Facade defFacadeForPanel(RTreeLib.RTree<Facade> treeFacades,BlockReference blRef, 
                                          PanelBtrExport panelBtrExport, PanelBlRefExport panelBlRefExport)
      {
         Facade resVal = null;
         RTreeLib.Point pt = new RTreeLib.Point(panelBlRefExport.Position.X, panelBlRefExport.Position.Y, 0);
         var facadesFind = treeFacades.Nearest(pt, 100);
         if (facadesFind.Count == 1)
         {
            resVal = facadesFind.First();            
         }
         else if (facadesFind.Count == 0)
         {
            Inspector.AddError(string.Format("Не определен фасад для панели {0}", panelBtrExport.BlName),
                                 blRef.GeometricExtents, panelBlRefExport.IdBlRefAkr);
         }
         else if (facadesFind.Count > 1)
         {
            Inspector.AddError(string.Format("Найдено больше одного фасада для панели {0}", panelBtrExport.BlName),
                                blRef.GeometricExtents, panelBlRefExport.IdBlRefAkr);
         }
         return resVal;
      }
   }
}

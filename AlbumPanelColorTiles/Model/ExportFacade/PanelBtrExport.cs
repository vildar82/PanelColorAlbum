using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Model.Select;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   /// <summary>
   /// Экспортная панель
   /// </summary>
   public class PanelBtrExport
   {      
      private Extents3d _extentsByTile;

      public ConvertPanelService CPS { get; private set; }
      public string BlName { get; private set; }
      /// <summary>
      /// Определение блока панели в экспортированном файле
      /// </summary>
      public ObjectId IdBtrExport { get; set; }

      /// <summary>
      /// Определение блока панели в файле АКР
      /// </summary>
      public ObjectId IdBtrAkr { get; set; }

      public Extents3d ExtentsByTile { get { return _extentsByTile; } }
      public Extents3d ExtentsNoEnd { get; set; }
      public double HeightByTile { get; private set; }
      public string CaptionMarkSb { get; private set; }
      public string CaptionPaint { get; private set; }
      public ObjectId CaptionLayerId { get; private set; }
      public ObjectId IdCaptionMarkSb{ get; set; }
      public ObjectId IdCaptionPaint { get; set; }
      public List<Extents3d> Tiles { get; private set; }    
      public List<PanelBlRefExport> Panels { get; private set; }

      public string ErrMsg { get; private set; }

      public PanelBtrExport(ObjectId idBtrAkr )
      {
         IdBtrAkr = idBtrAkr;
         Panels = new List<PanelBlRefExport>();
      }           
      

      public void Def()
      {
         using (var btr = IdBtrAkr.Open( OpenMode.ForRead )as BlockTableRecord)
         {
            BlName = btr.Name;
         }
      }

      public void Convert()
      {
         using (var btr = IdBtrExport.Open(OpenMode.ForWrite) as BlockTableRecord)
         {
            // Итерация по объектам в блоке и выполнение различных операций к элементам
            iterateEntInBlock(btr);

            // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
            ConvertCaption caption = new ConvertCaption(this);
            caption.Convert(btr);

            // Контур панели (так же определяется граница панели без торцов)
            ContourPanel contourPanel = new ContourPanel(this);
            contourPanel.CreateContour(btr);

            // Определение торцевых объектов (плитки и полилинии контура торца)


            // если это ОЛ, то удаление торца
            deleteEnds();
         }
      }

      private void deleteEnds()
      {
         if (CaptionMarkSb.StartsWith("ОЛ", StringComparison.CurrentCultureIgnoreCase))
         {

         }
      }

      private void iterateEntInBlock(BlockTableRecord btr)
      {
         foreach (ObjectId idEnt in btr)
         {
            using (var ent = idEnt.Open(OpenMode.ForRead) as Entity)
            {
               // Удаление лишних объектов (мусора)
               if (deleteWaste(ent)) continue; // Если объект удален, то переход к новому объекту в блоке

               // Если это плитка, то определение размеров панели по габаритам всех плиток
               if (ent is BlockReference && string.Equals(((BlockReference)ent).GetEffectiveName(),
                          Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
               {
                  _extentsByTile.AddExtents(ent.GeometricExtents);
                  Tiles.Add(ent.GeometricExtents);
                  continue;
               }

               // Если это подпись Марки (на слое Марок)
               if (ent is DBText && string.Equals(ent.Layer, Settings.Default.LayerMarks, StringComparison.CurrentCultureIgnoreCase))
               {
                  // Как определить - это текст Марки или Покраски - сейчас Покраска в скобках (). Но вдруг будет без скобок.
                  var textCaption = (DBText)ent;
                  if (textCaption.TextString.StartsWith("("))
                  {
                     CaptionPaint = textCaption.TextString;
                     IdCaptionPaint = idEnt;
                  }
                  else
                  {
                     CaptionMarkSb = textCaption.TextString;
                     IdCaptionMarkSb = idEnt;
                     CaptionLayerId = textCaption.LayerId;
                  }
               }
            }
         }
         // Определение высоты панели
         HeightByTile = ExtentsByTile.MaxPoint.Y - ExtentsByTile.MinPoint.Y;
      }

      // Удаление мусора из блока
      private static bool deleteWaste(Entity ent)
      {
         if (string.Equals(ent.Layer, Settings.Default.LayerDimensionFacade, StringComparison.CurrentCultureIgnoreCase) ||
                           string.Equals(ent.Layer, Settings.Default.LayerDimensionForm, StringComparison.CurrentCultureIgnoreCase))
         {
            ent.UpgradeOpen();
            ent.Erase();
            return true;
         }
         return false;
      }
   }
}

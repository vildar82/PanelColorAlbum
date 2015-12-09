using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Model.ExportFacade
{
   // преобразование определения блока панели
   public class ConvertPanelBtr
   {
      private Database _db;

      public ObjectId IdBtr { get; private set; }
      public Extents3d ExtentsByTile { get; private set; }
      public double HeightByTile { get; private set; }
      public string CaptionMarkSb { get; private set; }
      public string CaptionPaint { get; private set; }
      private ObjectId _idCaptionMarkSb;
      private ObjectId _idCaptionPaint;

      public ConvertPanelBtr(ObjectId idBtr)
      {
         IdBtr = idBtr;
         _db = idBtr.Database;
      }

      public void Convert()
      {
         using (var btr = IdBtr.Open(OpenMode.ForWrite) as BlockTableRecord)
         {
            // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
            redefineBlockTile();

            // Итерация по объектам в блоке и выполнение различных операций к элементам
            iterateEntInBlock(btr);

            // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
            convertCaption();
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
               if (ent is BlockReference && string.Equals( ((BlockReference)ent).GetEffectiveName(),
                          Settings.Default.BlockTileName, StringComparison.CurrentCultureIgnoreCase))
               {
                  ExtentsByTile.AddExtents(ent.GeometricExtents);
                  continue;
               }

               // Если это подпись Марки (на слое Марок)
               if (ent is DBText && string.Equals(ent.Layer, Settings.Default.LayerMarks, StringComparison.CurrentCultureIgnoreCase))
               {
                  // Как определить - это текст Марки или Покраски - сейчас Покраска в скобках (). Но вдруг будет без скобок.
                  var textCaption = (DBText)ent;
                  if (textCaption.TextString.StartsWith("("))                  
                     CaptionPaint = textCaption.TextString;
                  else
                     CaptionMarkSb = textCaption.TextString;
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

      // Переопределение блока плитки из файла шаблона блоков для Экспорта фасадов.
      private void redefineBlockTile()
      {
         string fileBlocksTemplate = Path.Combine(Commands.CurDllDir, Settings.Default.TemplateBlocksAKRExportFacadeFileName);
         if (File.Exists(fileBlocksTemplate))
         {
            try
            {
               AcadLib.Blocks.Block.CopyBlockFromExternalDrawing(Settings.Default.BlockTileName, fileBlocksTemplate,
                              _db, DuplicateRecordCloning.Replace);
            }
            catch
            {
            }
         }
      }

      // Повортот подписи марки (Марки СБ и Марки Покраски) и добавление фоновой штриховки
      private void convertCaption()
      {
         // Найти тексты на слое АР_Марки - нижний текст это Марка СБ, верхний - Покраска

         //// Проверка можно ли повернуть тексты вертикально - по длине текста и высоте панели
         //// Если высота панели больше 2500 мм, то поворачиваем текст вертикально
         //if (markAr.MarkSB.HeightPanelByTile > 2500)
         //{
         //   // Поворот текстов вертикально
         //   var angle = Math.PI * 90 / 180.0;
         //   textMarkSb.Rotation = angle;
         //   textMarkSb.Position = new Point3d(230, 20, 0);
         //   textMarkAr.Rotation = angle;
         //   textMarkAr.Position = new Point3d(230 + Settings.Default.CaptionPanelSecondTextShift, 20, 0);
         //   // Штриховка фона
         //   Extents3d extTexts = new Extents3d();
         //   extTexts.AddExtents(textMarkAr.GeometricExtents);
         //   extTexts.AddExtents(textMarkSb.GeometricExtents);
         //   Hatch h = GetHatch(extTexts, btr);
         //}
      }
   }
}

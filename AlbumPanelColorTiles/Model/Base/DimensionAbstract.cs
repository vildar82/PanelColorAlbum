using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   // Общее образмеривание для Фасада и Формы
   public abstract class DimensionAbstract
   {
      protected BlockTableRecord btrDim;
      protected BlockTableRecord btrPanel;
      protected PanelBase panelBase;
      protected Transaction t;
      protected ObjectId idBlRefDim;

      public DimensionAbstract(BlockTableRecord btrPanel, Transaction t, PanelBase panel)
      {
         this.btrPanel = btrPanel;
         this.t = t;
         this.panelBase = panel;
      }

      protected BlockTableRecord createBtrDim(string prefix, ObjectId idLayer)
      {
         // Создание определения блока образмеривания
         BlockTableRecord btrDim;
         string blNameDim = panelBase.BlNameAkr.Replace(Settings.Default.BlockPanelAkrPrefixName, prefix);
         using (var bt = btrPanel.OwnerId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            if (bt.Has(blNameDim))
            {
               btrDim = bt[blNameDim].GetObject(OpenMode.ForWrite) as BlockTableRecord;
               btrDim.ClearEntity();
            }
            else
            {
               btrDim = new BlockTableRecord();
               btrDim.Name = blNameDim;
               bt.UpgradeOpen();
               bt.Add(btrDim);
               t.AddNewlyCreatedDBObject(btrDim, true);
            }
         }
         // Добавление ссылки блока в блок панели
         BlockReference blRefDim = new BlockReference(Point3d.Origin, btrDim.Id);
         blRefDim.LayerId = idLayer;
         idBlRefDim = btrPanel.AppendEntity(blRefDim);
         t.AddNewlyCreatedDBObject(blRefDim, true);
         return btrDim;
      }

      protected void sizesBot()
      {
         Point3d ptBotLeft = new Point3d(0, 0, 0);
         Point3d ptBotRight = new Point3d(panelBase.Panel.gab.length, 0, 0);
         double yTotal = -250 - 200;
         Point3d ptDimLineTotal = new Point3d(0, yTotal, 0);
         createDim(ptBotLeft, ptBotRight, ptDimLineTotal, false, Matrix3d.Identity);

         Point3d ptDimLineInter = new Point3d(0, yTotal + 200, 0);
         var ptNext = new Point3d(ptBotRight.X - 288, 0, 0);
         createDim(ptBotLeft, ptNext, ptDimLineInter, false, Matrix3d.Identity);
         createDim(ptNext, ptBotRight, ptDimLineInter, false, Matrix3d.Identity);
      }

      protected void sizesTop(bool doTrans, Matrix3d trans)
      {
         bool hasInterDim = panelBase.PtsForTopDim.Count > 0;      

         // Общий размер
         Point3d ptTopLeft = new Point3d(0, panelBase.Panel.gab.height, 0);
         Point3d ptTopRight = new Point3d(panelBase.Panel.gab.length, panelBase.Panel.gab.height, 0);
         double yTotal = hasInterDim ? panelBase.Panel.gab.height + 250 + 200 : panelBase.Panel.gab.height + 250;
         Point3d ptDimLineTotal = new Point3d(0, yTotal, 0);
         createDim(ptTopLeft, ptTopRight, ptDimLineTotal, doTrans, trans);

         // добавление промежуточных размеров
         if (hasInterDim)
         {
            panelBase.PtsForTopDim.Sort();
            AcadLib.Comparers.DoubleEqualityComparer comparer = new AcadLib.Comparers.DoubleEqualityComparer(4);
            var ptsX = panelBase.PtsForTopDim.GroupBy(p=>p, comparer).Select(g=>g.First());

            Point3d ptPrev = ptTopLeft;
            Point3d ptDimLineInter = new Point3d(0, yTotal - 200, 0);
            foreach (var x in ptsX)
            {
               Point3d ptNext = new Point3d(x, ptPrev.Y, 0);
               createDim(ptPrev, ptNext, ptDimLineInter, doTrans, trans);
               ptPrev = ptNext;               
            }
            // Замыкающий размер
            createDim(ptPrev, ptTopRight, ptDimLineInter, doTrans, trans);
         }
      }

      protected RotatedDimension createDim(Point3d ptPrev, Point3d ptNext, Point3d ptDimLine, bool doTrans, Matrix3d trans)
      {
         if (doTrans)
         {
            ptPrev = ptPrev.TransformBy(trans);
            ptNext = ptNext.TransformBy(trans);
            ptDimLine = ptDimLine.TransformBy(trans);
         }         

         var dim = new RotatedDimension(0, ptPrev, ptNext, ptDimLine, "", panelBase.Service.Env.IdDimStyle);
         dim.Dimscale = Settings.Default.SheetScale;
         btrDim.AppendEntity(dim);
         t.AddNewlyCreatedDBObject(dim, true);
         return dim;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib.Blocks;
using AcadLib;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   // Образмеривание панели (на Фасаде)
   public class DimensionFacade : DimensionAbstract
   {
      public DimensionFacade(BlockTableRecord btrPanel, Transaction t, PanelBase panel) : base(btrPanel, t, panel)
      {

      }

      public void Create()
      {
         // Создание определения блока образмеривыания - пустого
         btrDim = CreateBtrDim("ОБР_", panelBase.Service.Env.IdLayerDimFacade);         
         // Размеры сверху
         SizesTop(false, Matrix3d.Identity);
         // Размеры снизу 
         SizesBot(false, Matrix3d.Identity);
         // Размеры слева
         SizesLeft(false, Matrix3d.Identity);
         // Размеры справа
         SizesRight(false, Matrix3d.Identity);
         // Сечение боковое
         verticalSection();
      }

      private void verticalSection()
      {         
         // Вертикальное сечение
         // вставка блока сечения панели
         addVerticalPanelSection();

         // вставка обозначения вертикального сечения
         addVerticalSectionMark();
         // вставка обозначения горизонтального сечения
         addHorizontalSectionMark();
      }

      private void addVerticalPanelSection()
      {
         // Вставка вертикального сечения панели
         // Ширина блока сечения панели
         double secThickness = panelBase.Thickness == 320 ? 700 : 800;
         double xPt = panelBase.IsCheekLeft ? xDimLineRightMax + 700 : xDimLineLeftMin - secThickness;

         Point3d ptPos = new Point3d();
      }

      private void addVerticalSectionMark()
      {
         // Определение X сечения в панели
         // Если есть окно, то сечение по первому окну                           
         var win = panelBase.Panel.windows?.window?.First();
         double xSec = (win == null) ? 750 : win.posi.X + 250;

         double yTop = yDimLineTopMax + 230;
         double yBot = yDimLineBotMin -145;         
         
         Point3d ptTopCross = new Point3d(xSec,yTop,0);
         Point3d ptBotCross = new Point3d(xSec, yBot, 0);
         BlockReference blRefCrossTop = CreateBlRef(ptTopCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         var attrRefTop = addAttrToBlockCross(blRefCrossTop, "2");

         BlockReference blRefCrossBot = CreateBlRef(ptBotCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         Matrix3d matrixMirrBotCross = Matrix3d.Mirroring(new Line3d(ptBotCross,
                                    new Point3d(ptBotCross.X + 1, ptBotCross.Y, ptBotCross.Z)));
         blRefCrossBot.TransformBy(matrixMirrBotCross);
         var attrRefBot = addAttrToBlockCross(blRefCrossBot, "2");
         attrRefBot.TransformBy(Matrix3d.Mirroring(new Line3d(attrRefBot.AlignmentPoint,
            new Point3d(attrRefBot.AlignmentPoint.X + 1, attrRefBot.AlignmentPoint.Y, attrRefBot.AlignmentPoint.Z))));
      }

      private void addHorizontalSectionMark()
      {
         double ySec = panelBase.Height * 70 / 100; // сечение на высоте 70% от общей высоты панели
         double xLeft = xDimLineLeftMin - 160;
         double xRight = xDimLineRightMax + 140;

         Point3d ptLeftCross = new Point3d(xLeft, ySec, 0);
         Point3d ptRightCross = new Point3d(xRight, ySec, 0);

         BlockReference blRefCrossLeft = CreateBlRef(ptLeftCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         Matrix3d matrixMirrLeftCross = Matrix3d.Mirroring(new Line3d(ptLeftCross, ptRightCross));
         blRefCrossLeft.Rotation = 90d.ToRadians();
         blRefCrossLeft.TransformBy(matrixMirrLeftCross);         
         var attrRefTop = addAttrToBlockCross(blRefCrossLeft, "1");                  
         attrRefTop.TransformBy(Matrix3d.Mirroring(new Line3d(attrRefTop.AlignmentPoint,
               new Point3d(attrRefTop.AlignmentPoint.X+1, attrRefTop.AlignmentPoint.Y, attrRefTop.AlignmentPoint.Z))));
         attrRefTop.Rotation = 0;

         BlockReference blRefCrossRight = CreateBlRef(ptRightCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         blRefCrossRight.Rotation = 270d.ToRadians();
         var attrRefBot = addAttrToBlockCross(blRefCrossRight, "1");
         attrRefBot.Rotation = 0; ;
      }

      private void rotateHorCross(BlockReference blRefCross, AttributeReference attrRef)
      {
         blRefCross.Rotation = 90d.ToRadians();         
      }

      private AttributeReference addAttrToBlockCross(BlockReference blRefCross, string num)
      {
         AttributeReference attrRefCross = null;
         if (!panelBase.Service.Env.IdAttrDefCross.IsNull)
         {
            using (var attrDefCross = panelBase.Service.Env.IdAttrDefCross.GetObject(OpenMode.ForRead, false, true) as AttributeDefinition)
            {
               attrRefCross = new AttributeReference();
               attrRefCross.SetAttributeFromBlock(attrDefCross, blRefCross.BlockTransform);
               attrRefCross.TextString = num;

               blRefCross.AttributeCollection.AppendAttribute(attrRefCross);
               t.AddNewlyCreatedDBObject(attrRefCross, true);               
            }
         }
         return attrRefCross;
      }
   }
}

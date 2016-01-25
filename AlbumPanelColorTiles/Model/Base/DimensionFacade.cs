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
using AcadLib.Errors;

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

         // вставка блока вертикального сечения панели
         addVerticalPanelSection();
         // вставка блока горизонтального сечения панели
         addHorizontalPanelSection();

         // вставка обозначения вертикального сечения
         addVerticalSectionMark();
         // вставка обозначения горизонтального сечения
         addHorizontalSectionMark();
      }     

      private void addHorizontalPanelSection()
      {
         var secBlocks = panelBase.Service.Env.BlPanelSections.OfType<BlockSectionHorizontal>().Where(s =>
                           s.IsCheekLeft == panelBase.IsCheekLeft &&
                           s.IsCheekRight == panelBase.IsCheekRight &&
                           s.IsOutsideLeft == panelBase.IsOutsideLeft &&
                           s.IsOutsideRight == panelBase.IsOutsideRight
                           );
         if (secBlocks.Count() == 0)
         {
            Inspector.AddError($"Не определено горизонтальное сечение для панели {panelBase.Panel.mark}");
            return;
         }

         ObjectId idBtrSec = secBlocks.First().IdBtr;
         var file = idBtrSec.Database.Filename;
         double yPt = yDimLineBotMin - 700;

         // Тескт сечения
         AddText(@"%%U2-2", new Point3d(panelBase.Length * 0.5, yPt+500, 0), 2.5*Settings.Default.SheetScale);

         Point3d ptPos = new Point3d(0, yPt, 0);
         var blRefSecHor = CreateBlRefInBtrDim(ptPos, idBtrSec, 1);
         // установить дин параметры длины и ширины блока сечения панели         
         var res = setDynParam(blRefSecHor, "Толщина", panelBase.Thickness);
         res = setDynParam(blRefSecHor, "Длина", panelBase.Length);

         // расставить окна и размеры
         Point3d ptDimLine = new Point3d(0, yPt - indentDimLineFromDraw, 0);
         if (panelBase.Panel.windows?.window?.Count() > 0)
         {
            // Первая точка - левая контура ? наверно нужно всей панели c outside
            Point3d ptPrev = new Point3d(panelBase.XMinContour, ptPos.Y, 0);
            
            // Первое окно
            var winFirst = panelBase.Panel.windows.window.First();

            // Если есть место - то добавление размера одной плитеки и шва
            if ((winFirst.posi.X - panelBase.XMinContour)>300)
            {
               Point3d pt288 = new Point3d(ptPrev.X + 288, ptPrev.Y, 0);
               CreateDim(ptPrev, pt288, ptDimLine, false, Matrix3d.Identity);
               ptPrev = pt288;
               Point3d pt12 = new Point3d(ptPrev.X + Settings.Default.TileSeam, ptPrev.Y, 0);
               CreateDim(ptPrev, pt12, ptDimLine, false, Matrix3d.Identity);
               ptPrev = pt12;
            }            

            foreach (var window in panelBase.Panel.windows.window)
            {
               Point3d ptWin = new Point3d(window.posi.X, ptPos.Y, 0);
               var blRefWin = CreateBlRefInBtrDim(ptWin, panelBase.Service.Env.IdBtrWindowHorSection, 1);
               // Уст дин парам окна
               res = setDynParam(blRefWin, "Толщина", panelBase.Thickness);
               res = setDynParam(blRefWin, "Длина", window.width);

               // размер до зазора от плитки до окна 6 мм
               Point3d pt6 = new Point3d(ptWin.X - 6, ptWin.Y, 0);
               CreateDim(ptPrev, pt6, ptDimLine, false, Matrix3d.Identity);
               ptPrev = pt6;
               // Зазор от конца плитки до начала окна
               pt6 = new Point3d(ptPrev.X + 6, ptPrev.Y, 0);
               CreateDim(ptPrev, pt6, ptDimLine, false, Matrix3d.Identity);
               ptPrev = pt6;
               // Размер окна
               Point3d ptNext = new Point3d(ptWin.X+ window.width, ptWin.Y, 0);
               CreateDim(ptPrev, ptNext, ptDimLine, false, Matrix3d.Identity);
               ptPrev = ptNext;
               // Зазор от конца плитки до начала окна
               pt6 = new Point3d(ptPrev.X + 6, ptPrev.Y, 0);
               CreateDim(ptPrev, pt6, ptDimLine, false, Matrix3d.Identity);
               ptPrev = pt6;
            }

            // Последний размер от последнего окна
            var winLast = panelBase.Panel.windows.window.Last();
            Point3d ptWinLast = new Point3d(winLast.posi.X+winLast.width, ptPos.Y, 0);
            Point3d ptLeftPanel = new Point3d(panelBase.XMaxContour, ptPos.Y, 0);
            CreateDim(ptWinLast, ptLeftPanel, ptDimLine, false, Matrix3d.Identity);
         }
         else
         {

         }
      }

      private Result setDynParam(BlockReference blRefSecHor, string propName, double value)
      {         
         foreach (DynamicBlockReferenceProperty prop in blRefSecHor.DynamicBlockReferencePropertyCollection)
         {
            if (!prop.ReadOnly && prop.PropertyName.Equals(propName, StringComparison.OrdinalIgnoreCase))
            {
               prop.Value = value;
               return Result.Ok();
            }            
         }
         return Result.Fail($"Не найден динамический параметр {propName}");
      }      

      private void addVerticalPanelSection()
      {
         double secThickness = getThicknessVerticalSectionBlock();
         double xPt = panelBase.IsCheekLeft ? xDimLineRightMax + 250 : xDimLineLeftMin - secThickness -250;

         // определение блока сечения панели         
         var secBlocks = panelBase.Service.Env.BlPanelSections.OfType<BlockSectionVertical>().Where(s =>                            
                           s.Thickness == panelBase.Thickness &&
                           Math.Abs(s.Length - panelBase.Height) < 300);
         if (secBlocks.Count()==0)
         {
            Inspector.AddError($"Не определено вертикальное сечение для панели {panelBase.Panel.mark}");
            return;
         }
         ObjectId idBtrSec = secBlocks.First().IdBtr;
         Point3d ptPos = new Point3d(xPt, 0,0);
         CreateBlRefInBtrDim(ptPos, idBtrSec, 1);
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
         BlockReference blRefCrossTop = CreateBlRefInBtrDim(ptTopCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         var attrRefTop = addAttrToBlockCross(blRefCrossTop, "2");

         BlockReference blRefCrossBot = CreateBlRefInBtrDim(ptBotCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
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

         BlockReference blRefCrossLeft = CreateBlRefInBtrDim(ptLeftCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
         Matrix3d matrixMirrLeftCross = Matrix3d.Mirroring(new Line3d(ptLeftCross, ptRightCross));
         blRefCrossLeft.Rotation = 90d.ToRadians();
         blRefCrossLeft.TransformBy(matrixMirrLeftCross);         
         var attrRefTop = addAttrToBlockCross(blRefCrossLeft, "1");                  
         attrRefTop.TransformBy(Matrix3d.Mirroring(new Line3d(attrRefTop.AlignmentPoint,
               new Point3d(attrRefTop.AlignmentPoint.X+1, attrRefTop.AlignmentPoint.Y, attrRefTop.AlignmentPoint.Z))));
         attrRefTop.Rotation = 0;

         BlockReference blRefCrossRight = CreateBlRefInBtrDim(ptRightCross, panelBase.Service.Env.IdBtrCross, Settings.Default.SheetScale);
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

      private double getThicknessVerticalSectionBlock()
      {         
         switch (panelBase.Thickness)
         {
            case 320:
               return 700;
            case 420:
               return 800;
            default:
               return 700;              
         }         
      }

      private void AddText(string val, Point3d pt, double height)
      {
         try
         {
            DBText text = new DBText();
            text.Position = pt;
            text.TextStyleId = panelBase.Service.Db.GetTextStylePIK();
            text.TextString = val;
            text.Height = height;            
            btrDim.AppendEntity(text);
            t.AddNewlyCreatedDBObject(text, true);
         }
         catch { }
      }
   }
}

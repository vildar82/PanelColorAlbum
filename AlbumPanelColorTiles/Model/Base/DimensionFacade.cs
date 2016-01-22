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
         double yPt = yDimLineBotMin -200;
         Point3d ptPos = new Point3d(0, yPt, 0);
         var blRefSecHor =  CreateBlRef(ptPos, idBtrSec, 1);
         // установить дин параметры длины и ширины блока сечения панели
         var resSetDynParam = setDynParam(blRefSecHor, new List<KeyValuePair<string, object>>
                                 {
                                    new KeyValuePair<string, object> ("Толщина", panelBase.Thickness),
                                    new KeyValuePair<string, object> ("Длина", panelBase.Length)
                                 });
         if (resSetDynParam.Success)
         {
            using (var btrSecHor = idBtrSec.GetObject( OpenMode.ForRead) as BlockTableRecord)
            {
               btrSecHor.UpdateAnonymousBlocks();
            }
         }
         else
         {
            Inspector.AddError(resSetDynParam.Error);
         }
         // расставить окна.
         if (panelBase.Panel.windows?.window?.Count()>0)
         {
            foreach (var window in panelBase.Panel.windows.window)
            {
               var blRefWin = CreateBlRef(new Point3d(window.posi.X, ptPos.Y, 0), panelBase.Service.Env.IdBtrWindowHorSection, 1);
               // Уст дин парам окна
               var resSetDynParamWin = setDynParam(blRefWin, new List<KeyValuePair<string, object>>
                                 {
                                    new KeyValuePair<string, object> ("Толщина", panelBase.Thickness),
                                    new KeyValuePair<string, object> ("Длина", window.width)
                                 });
               if (resSetDynParamWin.Failure)
               {
                  Inspector.AddError(resSetDynParamWin.Error);
               }               
            }                        
         }
      }

      private Result setDynParam(BlockReference blRefSecHor, List<KeyValuePair<string, object>> values)
      {
         string err = string.Empty;             
         Dictionary<string, DynamicBlockReferenceProperty> dictProps = new Dictionary<string, DynamicBlockReferenceProperty>();
         foreach (DynamicBlockReferenceProperty prop in blRefSecHor.DynamicBlockReferencePropertyCollection)
         {
            if (!prop.ReadOnly)
            {
               dictProps.Add(prop.PropertyName.ToLower(), prop);
            }            
         }
         foreach (var value in values)
         {
            DynamicBlockReferenceProperty prop;
            if (dictProps.TryGetValue(value.Key.ToLower(), out prop))
            {
               try
               {
                  var valueDwg = TryCastValueDynType(prop.UnitsType, value.Value);
                  prop.Value = valueDwg.Value;

               }
               catch (Exception ex)
               {
                  err += $"Ошибка при установке значения дин параметра {value.Key} = {value.Value} в блоке сечения {blRefSecHor.Name}: {ex.Message}. ";
               }
            }
            else
            {
               err += $"В блоке сечения {blRefSecHor.Name} не найден динамический параметр {value.Key}. ";
            }
         }
         if (string.IsNullOrEmpty(err))
         {
            return Result.Ok();
         }
         else
         {
            return Result.Fail(err);
         }
      }

      private Result<object> TryCastValueDynType(DynamicBlockReferencePropertyUnitsType unitsType, object value)
      {         
         try
         {
            switch (unitsType)
            {
               case DynamicBlockReferencePropertyUnitsType.NoUnits:
                  return Result.Ok<object>(Convert.ToString(value));
               case DynamicBlockReferencePropertyUnitsType.Angular:                  
               case DynamicBlockReferencePropertyUnitsType.Distance:                  
               case DynamicBlockReferencePropertyUnitsType.Area:
                  return Result.Ok<object>(Convert.ToDouble(value));               
            }
         }
         catch (Exception ex)
         {
            return Result.Fail<object>(ex.Message);
         }
         return Result.Fail<object>("Не найдено соответствие типа параметра.");
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
         CreateBlRef(ptPos, idBtrSec, 1);
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
   }
}

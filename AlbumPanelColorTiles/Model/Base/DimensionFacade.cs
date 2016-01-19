using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib.Blocks;
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
         bool leftSection = !panelBase.IsCheekLeft;
         // Вертикальное сечение
         // вставка блока сечения панели

         // вставка обозначения вертикального сечения
         addVerticalSectionMark(leftSection);
      }

      private void addVerticalSectionMark(bool leftSection)
      {
         // Определение X сечения в панели
         // Если есть окно, то сечение по первому окну                  
         double xSec = 750;
         double yTop = yTopDimLineMax + 120;
         double yBot = yBotDimLineMin -40;
         var win = panelBase.Panel.windows?.window?.First();
         if (win!=null)
         {
            xSec = win.posi.X + 250;
         }
         
         Point3d ptTopCross = new Point3d(xSec,yTop,0);
         Point3d ptBotCross = new Point3d(xSec, yBot, 0);
         BlockReference blRefCrossTop = CreateBlRef(ptTopCross, panelBase.Service.Env.IdBtrCross);
         var attrRefTop = addAttrToBlockCross(blRefCrossTop, "2");
         BlockReference blRefCrossBot = CreateBlRef(ptBotCross, panelBase.Service.Env.IdBtrCross);
         var attrRefBot = addAttrToBlockCross(blRefCrossBot, "2");         
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

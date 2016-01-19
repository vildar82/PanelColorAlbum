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
      }                  
   }
}

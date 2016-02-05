using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.Base
{
   public class DimensionForm : DimensionAbstract
   {
      public DimensionForm(BlockTableRecord btrPanel, Transaction t, PanelBase panel) : base(btrPanel, t, panel)
      {

      }

      public void Create()
      {
         double xCenter = panelBase.Length * 0.5;
         Matrix3d matrixMirr = Matrix3d.Mirroring(new Line3d(new Point3d(xCenter, 0, 0), new Point3d(xCenter, 1000, 0)));

         // Создание определения блока образмеривыания - пустого
         btrDim = CreateBtrDim("ОБРФ_", panelBase.Service.Env.IdLayerDimForm);
         // Размеры сверху
         SizesTop(true, matrixMirr);
         // Размеры снизу 
         SizesBot(true, matrixMirr);
         // Размеры слева
         SizesLeft(true, matrixMirr);
         // Размеры справа
         SizesRight(true, matrixMirr);

         // общая для фасада и формы - блок и стрелка к углу панели
         addProfileTile(true, matrixMirr);

         // Отзеркалить блок размеров в форме
         using (var blRefDim = this.idBlRefDim.GetObject(OpenMode.ForWrite, false, true) as BlockReference)
         {
            blRefDim.TransformBy(matrixMirr);
         }
      }
   }
}

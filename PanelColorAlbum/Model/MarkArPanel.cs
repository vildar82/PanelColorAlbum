using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Vil.Acad.AR.PanelColorAlbum.Model
{
   // Марка АР покраски панели
   public class MarkArPanel
   {
      private List<Paint> _paints;
      private List<Panel> _panels;
      private string _markAR;      
      

      public MarkArPanel(List<Paint> paintAR, string markAr)
      {
         _paints = paintAR;
         _markAR = markAr;
         _panels = new List<Panel>();                  
      }

      // Определение покраски панели (список цветов по порядку списка плитов в блоке СБ)
      public static List<Paint> GetPanelMarkAR(MarkSbPanel markSb, BlockReference blRefPanel, List<ColorArea> _colorAreas)
      {
         List<Paint> paintsAR = new List<Paint>();

         foreach (Tile tileMarSb in markSb.Tiles)
         {
            Paint paintAR = null;
            if (tileMarSb.Paint == null)
            {
               // Опрделение покраски по зонам
               Point3d pt = GetPointTileInBlockRef(blRefPanel, tileMarSb.InsPoint);
               paintAR = ColorArea.GetPaint(_colorAreas, pt);
            }
            else
            {
               paintAR = tileMarSb.Paint;
            }
            paintsAR.Add(paintAR);
         }
         return paintsAR;
      }

      private static Point3d GetPointTileInBlockRef(BlockReference blRefPanel, Point3d ptTile)
      {
         Point3d ptBlRef = blRefPanel.Position;
         Point3d ptRes = new Point3d(ptBlRef.X + ptTile.X, ptBlRef.Y + ptTile.Y, 0);
         return ptRes;
      }
      
      public void AddBlockRefPanel(BlockReference blRefPanel)
      {
         //TODO: Добавление ссылки на блок этой марки покраски
         throw new NotImplementedException();
      }

      public bool EqualPaint(List<Paint> paintAR)
      {
         // ??? сработает такое сравнение списков покраски?
         return paintAR.SequenceEqual(_paints);
      }
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Model.Panels;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Model.ExportFacade
{  
   /// <summary>
   /// Панель экспортированная - вхождение блока панели
   /// </summary>
   public class PanelBlRefExport
   {
      public Extents3d Extents { get; private set; }
      public Point3d Position { get; private set; }
      public Facade Facade { get; set; }
      public PanelBtrExport PanelBtrExport { get; private set; }
      public Matrix3d Transform { get; private set; }

      /// <summary>
      /// Блок панели в файле АКР
      /// </summary>
      public ObjectId IdBlRefAkr { get; private set; }
      /// <summary>
      /// Блок панели в экспортиованном файле фасада
      /// </summary>
      public ObjectId IdBlRefExport { get; set; }      

      public PanelBlRefExport(BlockReference blRef, PanelBtrExport panelBtrExport)
      {
         IdBlRefAkr = blRef.Id;
         Position = blRef.Position;
         PanelBtrExport = panelBtrExport;
         Transform = blRef.BlockTransform;
         Extents = blRef.GeometricExtents;
      }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Lib
{
   public class TableJig : EntityJig
   {
      Point3d _position;
      Table _table;
      int _scale;
      string _msg;

      public TableJig(Table table, int scale, string msg) : base(table)
      {
         _msg = msg;
         _scale = scale;
         _table = table;
         _position = _table.Position;
         _table.ScaleFactors = new Scale3d(scale);
      }

      protected override SamplerStatus Sampler(JigPrompts prompts)
      {
         var jigOpts = new JigPromptPointOptions();
         jigOpts.Message = _msg;
         var res = prompts.AcquirePoint(jigOpts);
         if (res.Status == PromptStatus.OK)
         {
            Point3d curPoint = res.Value;
            if (_position.DistanceTo(curPoint) > 1.0e-2)
               _position = curPoint;
            else
               return SamplerStatus.NoChange;
         }
         if (res.Status == PromptStatus.Cancel)
            return SamplerStatus.Cancel;
         else
            return SamplerStatus.OK;
      }

      protected override bool Update()
      {
         Table table = Entity as Table;
         if (table.Position.DistanceTo(_position) > 1.0e-2)
         {
            table.Position = _position;
            return true;
         }
         return false;
      }

      public Entity GetEntity()
      {
         return Entity;
      }
   }
}

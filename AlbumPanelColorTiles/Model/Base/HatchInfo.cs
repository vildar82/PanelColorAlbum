using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Base
{
   /// <summary>
   /// Описание штриховки - патерн, слой, цвет, тип и т.д.
   /// </summary>
   public class HatchInfo
   {
      /// <summary>
      /// Объекты с которыми штриховка ассоциирована
      /// </summary>
      public ObjectIdCollection IdsAssociate { get; set; }
      public ObjectId Id { get; set; }
      public string PatternName { get; private set; }
      public double PatternScale { get; private set; }
      public double PatternAngle { get; private set; }
      public string Layer { get; private set; }
      public Color Color { get; private set; }
      public LineWeight Lineweight { get; private set; }
      public HatchPatternType PatternType { get; private set; }      

      public HatchInfo(Hatch h)
      {
         Id = h.Id;
         if (h.Associative)
         {
            IdsAssociate = h.GetAssociatedObjectIds();
         }
         PatternName = h.PatternName;
         PatternScale = h.PatternScale;
         PatternAngle = h.PatternAngle;
         PatternType = h.PatternType;
         Layer = h.Layer;
         Color = h.Color;
         Lineweight = h.LineWeight;         
      }

      public void CreateNewHatch(BlockTableRecord btr)
      {
         var h = new Hatch();

         h.Annotative = AnnotativeStates.False;
         h.Layer = Layer;
         h.Color = Color;
         h.LineWeight = Lineweight;
         h.SetHatchPattern(PatternType, PatternName);
         h.PatternAngle = PatternAngle;
         h.PatternScale = PatternScale;
         h.SetHatchPattern(PatternType, PatternName);         

         Id = btr.AppendEntity(h);
         btr.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(h, true);         

         if (IdsAssociate != null && IdsAssociate.Count > 0)
         {
            h.Associative = true;
            h.AppendLoop(HatchLoopTypes.Default, IdsAssociate);
            h.EvaluateHatch(false);
         }
      }
   }
}

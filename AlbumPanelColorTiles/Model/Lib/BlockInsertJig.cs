using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Lib
{
   public class BlockInsertJig : EntityJig
   {
      private Point3d mCenterPt, mActualPoint;

      public BlockInsertJig(BlockReference br)
        : base(br)
      {
         mCenterPt = br.Position;
      }

      public Entity GetEntity()
      {
         return Entity;
      }

      protected override SamplerStatus Sampler(JigPrompts prompts)
      {
         JigPromptPointOptions jigOpts =
           new JigPromptPointOptions();
         jigOpts.UserInputControls =
           (UserInputControls.Accept3dCoordinates
           | UserInputControls.NoZeroResponseAccepted
           | UserInputControls.NoNegativeResponseAccepted);

         jigOpts.Message =
           "\nУкажите точку вставки: ";

         PromptPointResult dres =
           prompts.AcquirePoint(jigOpts);

         if (mActualPoint == dres.Value)
         {
            return SamplerStatus.NoChange;
         }
         else
         {
            mActualPoint = dres.Value;
         }
         return SamplerStatus.OK;
      }

      protected override bool Update()
      {
         mCenterPt = mActualPoint;
         try
         {
            ((BlockReference)Entity).Position = mCenterPt;
         }
         catch (System.Exception)
         {
            return false;
         }
         return true;
      }
   }
}
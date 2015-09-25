using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.Lib
{
   public class BlockInsertJig : EntityJig
   {
      #region Private Fields

      private Point3d mCenterPt, mActualPoint;

      #endregion Private Fields

      #region Public Constructors

      public BlockInsertJig(BlockReference br)
        : base(br)
      {
         mCenterPt = br.Position;
      }

      #endregion Public Constructors

      #region Public Methods

      public Entity GetEntity()
      {
         return Entity;
      }

      #endregion Public Methods

      #region Protected Methods

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

      #endregion Protected Methods
   }
}
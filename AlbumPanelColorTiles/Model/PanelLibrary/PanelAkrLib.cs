using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class PanelAkrLib : PanelAKR
   {
      protected ObjectId _idBtrPanelAkrInFacade;
      private bool _isElectricCopy;

      public PanelAkrLib(ObjectId idBtr, string blName) : base(idBtr, blName)
      {
      }

      public ObjectId IdBtrPanelAkrInFacade
      {
         get { return _idBtrPanelAkrInFacade; }
         set { _idBtrPanelAkrInFacade = value; }
      }

      public bool IsElectricCopy { get { return _isElectricCopy; } set { _isElectricCopy = value; } }

      public static List<PanelAkrLib> GetAkrPanelLib(Database dbLib)
      {
         List<PanelAkrLib> panelsAkrLIb = new List<PanelAkrLib>();
         using (var bt = dbLib.BlockTableId.Open(OpenMode.ForRead) as BlockTable)
         {
            foreach (ObjectId idBtr in bt)
            {
               using (var btr = idBtr.Open(OpenMode.ForRead) as BlockTableRecord)
               {
                  if (MarkSb.IsBlockNamePanel(btr.Name))
                  {
                     PanelAkrLib panelAkr = new PanelAkrLib(idBtr, btr.Name);
                     panelsAkrLIb.Add(panelAkr);
                  }
               }
            }
         }
         return panelsAkrLIb;
      }

      public PanelAkrLib CopyLibBlockElectricInTempFile(MountingPanel panelSb)
      {
         PanelAkrLib panelAkrLib = null;
         try
         {
            string markAkr = panelSb.MarkSbBlockName;
            SymbolUtilityServices.ValidateSymbolName(markAkr, false);
            // копирование блока с новым именем с электрикой
            ObjectId idBtrAkeElectricInTempLib = Lib.Block.CopyBtr(_idBtrAkrPanel, markAkr);
            panelAkrLib = new PanelAkrLib(idBtrAkeElectricInTempLib, markAkr);
            panelAkrLib.IsElectricCopy = true;
         }
         catch
         {
            // неудалось создать копию блока
         }
         return panelAkrLib;
      }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.Options;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class PanelAkrLib : PanelAKR
   {
      private  bool _isElectricCopy;
      protected ObjectId _idBtrPanelAkrInFacade;

      public PanelAkrLib(ObjectId idBtr, string blName) : base(idBtr, blName)
      {

      }

      public bool IsElectricCopy { get { return _isElectricCopy; } set { _isElectricCopy = value; } }
      public ObjectId IdBtrPanelAkrInFacade
      {
         get { return _idBtrPanelAkrInFacade; }
         set { _idBtrPanelAkrInFacade = value; }
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
   }
}

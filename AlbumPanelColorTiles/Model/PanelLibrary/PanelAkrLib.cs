using System.Collections.Generic;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class PanelAkrLib : PanelAKR
   {
      protected ObjectId _idBtrPanelAkrInFacade;      

      public PanelAkrLib(ObjectId idBtr, string blName) : base(idBtr, blName)
      {
      }

      public ObjectId IdBtrPanelAkrInFacade
      {
         get { return _idBtrPanelAkrInFacade; }
         set { _idBtrPanelAkrInFacade = value; }
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
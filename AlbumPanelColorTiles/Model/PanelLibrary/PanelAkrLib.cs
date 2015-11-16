using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles.PanelLibrary
{
   public class PanelAkrLib : PanelAKR
   {


      public PanelAKR CopyLibBlockElectricInTempFile(PanelSB panelSb)
      {
         PanelAKR panelAkr = null;
         try
         {
            string markAkr = panelSb.MarkSb;
            if (panelSb.IsEndLeftPanel)
            {
               markAkr += Settings.Default.EndLeftPanelSuffix;
            }
            else if (panelSb.IsEndRightPanel)
            {
               markAkr += Settings.Default.EndRightPanelSuffix;
            }
            SymbolUtilityServices.ValidateSymbolName(markAkr, false);
            // копирование блока с новым именем с электрикой
            ObjectId idBtrAkeElectricInTempLib = Lib.Block.CopyBtr(_idBtrAkrPanelInLib, markAkr);
            panelAkr = new PanelAKR(idBtrAkeElectricInTempLib, markAkr);
            panelAkr.IsElectricCopy = true;
         }
         catch
         {
         }
         return panelAkr;
      }
   }
}

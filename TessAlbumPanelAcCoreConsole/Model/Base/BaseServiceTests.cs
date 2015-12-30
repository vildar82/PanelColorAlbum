using NUnit.Framework;
using AlbumPanelColorTiles.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles.Model.Base.Tests
{
   [TestFixture()]
   public class BaseServiceTests
   {
      [Test()]
      public void ReadPanelsFromXmlTest()
      {
         // Проверка считывания Xml базы панелей
         BaseService baseService = new BaseService();
         baseService.LoadPanels();         
      }
   }
}
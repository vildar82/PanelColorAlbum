using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Blocks;
using AlbumPanelColorTiles.Options;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib;

namespace AlbumPanelColorTiles.Model.Base
{
   // Общее образмеривание для Фасада и Формы
   public abstract class DimensionAbstract
   {
      protected BlockTableRecord btrDim;
      protected BlockTableRecord btrPanel;
      protected PanelBase panelBase;
      protected Transaction t;
      protected ObjectId idBlRefDim;
      protected double yDimLineTopMax;
      protected double yDimLineBotMin;
      protected double xDimLineLeftMin;
      protected double xDimLineRightMax;

      public DimensionAbstract(BlockTableRecord btrPanel, Transaction t, PanelBase panel)
      {
         this.btrPanel = btrPanel;
         this.t = t;
         this.panelBase = panel;
      }

      protected BlockTableRecord CreateBtrDim(string prefix, ObjectId idLayer)
      {
         // Создание определения блока образмеривания
         BlockTableRecord btrDim;
         string blNameDim = panelBase.BlNameAkr.Replace(Settings.Default.BlockPanelAkrPrefixName, prefix);
         using (var bt = btrPanel.OwnerId.GetObject(OpenMode.ForRead) as BlockTable)
         {
            if (bt.Has(blNameDim))
            {
               btrDim = bt[blNameDim].GetObject(OpenMode.ForWrite) as BlockTableRecord;
               btrDim.ClearEntity();
            }
            else
            {
               btrDim = new BlockTableRecord();
               btrDim.Name = blNameDim;
               bt.UpgradeOpen();
               bt.Add(btrDim);
               t.AddNewlyCreatedDBObject(btrDim, true);
            }
         }
         // Добавление ссылки блока в блок панели
         BlockReference blRefDim = new BlockReference(Point3d.Origin, btrDim.Id);
         blRefDim.LayerId = idLayer;         
         idBlRefDim = btrPanel.AppendEntity(blRefDim);
         t.AddNewlyCreatedDBObject(blRefDim, true);
         return btrDim;
      }

      protected void SizesTop(bool doTrans, Matrix3d trans)
      {
         bool hasInterDim = panelBase.PtsForTopDim.Count > 0 || panelBase.XMinPanel < 0 || panelBase.XMaxPanel > panelBase.Length;

         // Общий размер
         Point3d ptTopLeft = new Point3d(panelBase.XMinContour, panelBase.Height, 0);
         Point3d ptTopRight = new Point3d(panelBase.XMaxContour, panelBase.Height, 0);
         yDimLineTopMax = hasInterDim ? panelBase.Height + 185 + 250 : panelBase.Height + 250;
         Point3d ptDimLineTotal = new Point3d(0, yDimLineTopMax, 0);
         CreateDim(ptTopLeft, ptTopRight, ptDimLineTotal, doTrans, trans, addTextRangeTile: true);
         // добавление промежуточных размеров
         if (hasInterDim)
         {
            panelBase.PtsForTopDim.Sort();
            AcadLib.Comparers.DoubleEqualityComparer comparer = new AcadLib.Comparers.DoubleEqualityComparer(4);
            var ptsX = panelBase.PtsForTopDim.GroupBy(p => p, comparer).Select(g => g.First());

            Point3d ptPrev = ptTopLeft;
            Point3d ptDimLineInter = new Point3d(0, yDimLineTopMax - 185, 0);
            foreach (var x in ptsX)
            {
               Point3d ptNext = new Point3d(x, ptPrev.Y, 0);
               CreateDim(ptPrev, ptNext, ptDimLineInter, doTrans, trans, addTextRangeTile: true);
               ptPrev = ptNext;
            }
            // Замыкающий размер
            CreateDim(ptPrev, ptTopRight, ptDimLineInter, doTrans, trans, addTextRangeTile: true);


            //если есть пустые области Outsides, то добавление промежеточных размеров.
            if (panelBase.XMinPanel < 0)
            {
               CreateDim(new Point3d(panelBase.XMinPanel, ptPrev.Y, 0),
                         new Point3d(panelBase.XMinContour, ptPrev.Y, 0), ptDimLineInter, doTrans, trans);
            }
            if (panelBase.XMaxPanel > panelBase.Length)
            {
               CreateDim(new Point3d(panelBase.XMaxContour, ptPrev.Y, 0),
                         new Point3d(panelBase.XMaxPanel, ptPrev.Y, 0), ptDimLineInter, doTrans, trans);
            }
         }
      }

      protected void SizesBot(bool doTrans, Matrix3d trans)
      {
         // Общий размер
         Point3d ptBotLeft = new Point3d(panelBase.XMinContour, 0, 0);
         Point3d ptBotRight = new Point3d(panelBase.XMaxContour, 0, 0);
         yDimLineBotMin = -215 - 215;
         Point3d ptDimLineTotal = new Point3d(0, yDimLineBotMin, 0);
         CreateDim(ptBotLeft, ptBotRight, ptDimLineTotal, doTrans, trans);
         // Промежуточный размер
         Point3d ptDimLineInter = new Point3d(0, yDimLineBotMin + 215, 0);
         var ptNext = new Point3d(ptBotRight.X - 288, 0, 0);
         var dim = CreateDim(ptBotLeft, ptNext, ptDimLineInter, doTrans, trans);
         var lenTile = Settings.Default.TileLenght + Settings.Default.TileSeam;
         var count = Convert.ToInt32(dim.Measurement / lenTile);
         dim.Prefix = $"({Settings.Default.TileLenght}+{Settings.Default.TileSeam})x{count}=";

         CreateDim(ptNext, ptBotRight, ptDimLineInter, doTrans, trans);        

         // Торец
         if (panelBase.IsCheekRight || panelBase.IsCheekLeft)
         {
            sizesCheek(doTrans, trans);
         }
      }

      protected void SizesLeft(bool doTrans, Matrix3d trans)
      {
         var heightTile = Settings.Default.TileHeight + Settings.Default.TileSeam;
         // y последней плитки
         var countTile = Convert.ToInt32((panelBase.Height - heightTile) / heightTile);
         var yLastTile = countTile * heightTile;

         Point3d ptBotLeft = new Point3d(panelBase.XMinPanel, 0, 0);
         Point3d ptTopLeft = new Point3d(ptBotLeft.X, yLastTile, 0);

         xDimLineLeftMin = ptBotLeft.X-175;
         Point3d ptDimLine = new Point3d(xDimLineLeftMin, 0, 0);

         var dim= CreateDim(ptBotLeft, ptTopLeft, ptDimLine, doTrans, trans, rotation: 90);
         dim.Prefix = $"({Settings.Default.TileHeight}+{Settings.Default.TileSeam})x{countTile}=";

         Point3d ptLastTile = new Point3d(ptTopLeft.X, ptTopLeft.Y + Settings.Default.TileHeight, 0);
         dim = CreateDim(ptTopLeft, ptLastTile, ptDimLine, doTrans, trans, rotation:90);
         Point3d ptText = new Point3d(ptDimLine.X, ptTopLeft.Y - 65, 0);
         dim.TextPosition = doTrans? ptText.TransformBy(trans): ptText;
      }

      protected void SizesRight(bool doTrans, Matrix3d trans)
      {
         double yWinMax=0;
         double yWinMin=0;
         var win = panelBase.Panel.windows?.window?.First();
         if (win == null)
         {
            var balc = panelBase.Panel.balconys?.balcony?.First();
            if (balc!=null)
            {
               yWinMax = balc.posi.Y + balc.height;
               yWinMin = balc.posi.Y;
            }
         }
         else
         {
            yWinMax = win.posi.Y + win.height;
            yWinMin = win.posi.Y;
         }

         var heightTile = Settings.Default.TileHeight + Settings.Default.TileSeam;
         // y последней плитки
         var countTile = Convert.ToInt32(panelBase.Height / heightTile);
         var yLastTile = (countTile * heightTile)-12;

         Point3d ptBotRight = new Point3d(panelBase.XMaxPanel, 0, 0);
         Point3d ptTopRight = new Point3d(ptBotRight.X, yLastTile, 0);

         bool hasIndentDim = yWinMax > 0;
         xDimLineRightMax = hasIndentDim ? ptBotRight.X+250+185 : ptBotRight.X + 250;

         // Общий размер         
         Point3d ptDimLineTotal = new Point3d(xDimLineRightMax, 0, 0);
         CreateDim(ptBotRight, ptTopRight, ptDimLineTotal, doTrans, trans, addTextRangeTile:true, rotation: 90);
         // Промежуточные размеры
         if (hasIndentDim)
         {
            Point3d ptDimLineIndent = new Point3d(xDimLineRightMax-185, 0, 0);
            Point3d ptMinWin= ptBotRight;
            if (yWinMin>0)
            {
               var countTileMinWin = Convert.ToInt32(yWinMin / heightTile);
               var yTilesMinWin = (countTileMinWin * heightTile) - 12;
               ptMinWin = new Point3d(ptBotRight.X, yTilesMinWin, 0);
               CreateDim(ptBotRight, ptMinWin, ptDimLineIndent, doTrans, trans, addTextRangeTile: true, rotation: 90);
               var ptMinWinSeam = new Point3d(ptMinWin.X, ptMinWin.Y+12, 0);
               var dimSeamMin= CreateDim(ptMinWin, ptMinWinSeam, ptDimLineIndent, doTrans, trans, addTextRangeTile: true, rotation: 90);
               Point3d ptTextSeamMin = new Point3d(ptDimLineIndent.X, ptMinWinSeam.Y+65,0);
               dimSeamMin.TextPosition = doTrans? ptTextSeamMin.TransformBy(trans): ptTextSeamMin;
               ptMinWin = ptMinWinSeam;
            }
            var countTileMaxWin = Convert.ToInt32(yWinMax / heightTile);
            var yTilesMaxWin = (countTileMaxWin * heightTile) - 12;
            Point3d ptMaxWin = new Point3d(ptMinWin.X, yTilesMaxWin, 0);
            CreateDim(ptMinWin, ptMaxWin, ptDimLineIndent, doTrans, trans, addTextRangeTile: true, rotation: 90);
            Point3d ptMaxWinSeam = new Point3d(ptMaxWin.X, ptMaxWin.Y+12, 0);
            var dimSeamMax = CreateDim(ptMaxWin, ptMaxWinSeam, ptDimLineIndent, doTrans, trans, addTextRangeTile: true, rotation: 90);
            Point3d ptTextSeamMax = new Point3d(ptDimLineIndent.X, ptMaxWin.Y - 65, 0);
            dimSeamMax.TextPosition = doTrans ? ptTextSeamMax.TransformBy(trans) : ptTextSeamMax;
            // размер до верха плиток
            CreateDim(ptMaxWinSeam, ptTopRight, ptDimLineIndent, doTrans, trans, addTextRangeTile: true, rotation: 90);
         }
      }

      private void sizesCheek(bool doTrans, Matrix3d trans)
      {
         double xMinCheek = panelBase.PtsForBotDimCheek.First();
         Point3d ptDimLineCheek = new Point3d(xMinCheek, -165, 0);
         Point3d ptFirstCheek = new Point3d(xMinCheek, 0, 0);
         Point3d ptLastCheek = new Point3d(panelBase.PtsForBotDimCheek.Last(), 0, 0);         

         if (panelBase.IsCheekLeft)
         {
            addCheekViewText(doTrans, trans, xMinCheek);
            addCheekViewBlock(doTrans, trans, xMinCheek+650, true);
         }
         if (panelBase.IsCheekRight)
         {
            addCheekViewText(doTrans, trans, xMinCheek);
            addCheekViewBlock(doTrans, trans, xMinCheek - 365, false);
         }

         Point3d ptPrevCheek = ptFirstCheek;
         foreach (var ptX in panelBase.PtsForBotDimCheek.Skip(1))
         {
            Point3d ptNextCheek = new Point3d(ptX, 0, 0);
            var dimCheek = CreateDim(ptPrevCheek, ptNextCheek, ptDimLineCheek, doTrans, trans);
            // Если размер маленький, то перемещение текста размера
            if (dimCheek.Measurement < 90)
            {               
               double deltaX = panelBase.IsCheekLeft ? 70 : -70;
               dimCheek.TextPosition = new Point3d(ptPrevCheek.X + deltaX, ptDimLineCheek.Y - 100, 0).TransformBy(trans);
            }
            ptPrevCheek = ptNextCheek;
         }
         // Общий размер торца
         CreateDim(ptFirstCheek, ptLastCheek, new Point3d(0, ptDimLineCheek.Y - 150, 0), doTrans, trans);
      }

      private void addCheekViewBlock(bool doTrans, Matrix3d trans, double xPosView, bool isLeft)
      {
         // Добавление блока вида.
         // Если блока нет, то выход.
         if (panelBase.Service.Env.IdBtrView.IsNull)
         {
            return;
         }

         Point3d ptBlView = new Point3d(xPosView, 860, 0);
         if (doTrans)
         {
            ptBlView = ptBlView.TransformBy(trans);
         }
         BlockReference blRefView = CreateBlRef(ptBlView, panelBase.Service.Env.IdBtrView);

         // атрибут Вида
         if (!panelBase.Service.Env.IdAttrDefView.IsNull)
         {
            using (var attrDefView = panelBase.Service.Env.IdAttrDefView.GetObject(OpenMode.ForRead, false, true) as AttributeDefinition)
            {
               var attrRefView = new AttributeReference();
               attrRefView.SetAttributeFromBlock(attrDefView, blRefView.BlockTransform);
               attrRefView.TextString = "А";

               blRefView.AttributeCollection.AppendAttribute(attrRefView);
               t.AddNewlyCreatedDBObject(attrRefView, true);

               if ((!isLeft || doTrans) && !(!isLeft && doTrans))
               {
                  attrRefView.TransformBy(Matrix3d.Mirroring(
                     new Line3d(attrRefView.AlignmentPoint, new Point3d(attrRefView.AlignmentPoint.X, 0, 0))));
               }
            }
         }

         if ((!isLeft || doTrans) && !(!isLeft && doTrans))
         {
            blRefView.TransformBy(Matrix3d.Mirroring(new Line3d(ptBlView, new Point3d(ptBlView.X, 0, 0))));
         }
      }      

      private void addCheekViewText(bool doTrans, Matrix3d trans, double xMinCheek)
      {
         // Текст с именем вида
         DBText textView = new DBText();         
         textView.TextString = "Вид А";
         textView.Height = 75;
         Point3d ptTextPos = new Point3d(xMinCheek, panelBase.Height + 170, 0);
         if (doTrans)
         {
            ptTextPos = ptTextPos.TransformBy(trans);
            ptTextPos = new Point3d(ptTextPos.X - 290, ptTextPos.Y, 0);
         }
         textView.Position = ptTextPos;         
         btrDim.AppendEntity(textView);
         t.AddNewlyCreatedDBObject(textView, true);
      }      

      private string getTextRange(double measurement)
      {
         var len = Settings.Default.TileLenght + Settings.Default.TileSeam;
         var count = Convert.ToInt32(measurement / len);
         string row = string.Empty;
         if (count <=1)
         {
            return "";
         }
         else if (count <5)
         {
            row = "ряда";
         }
         else if (count < 20)
         {
            row = "рядов";
         }
         else
         {
            var last = count % 10;
            if (last ==1)
            {
               return "ряд";
            }
            else if (last<5)
            {
               row = "ряда";
            }
            else
            {
               row = "рядов";
            }
         }
         return $" ({count} {row})";
      }

      protected RotatedDimension CreateDim(Point3d ptPrev, Point3d ptNext, Point3d ptDimLine, 
                    bool doTrans, Matrix3d trans, bool addTextRangeTile = false, double rotation = 0)
      {
         if (doTrans)
         {
            ptPrev = ptPrev.TransformBy(trans);
            ptNext = ptNext.TransformBy(trans);
            ptDimLine = ptDimLine.TransformBy(trans);
         }         

         var dim = new RotatedDimension(rotation.ToRadians(), ptPrev, ptNext, ptDimLine, "", panelBase.Service.Env.IdDimStyle);
         if (addTextRangeTile)
         {
            dim.Suffix = getTextRange(dim.Measurement);
         }
         dim.Dimscale = Settings.Default.SheetScale;
         btrDim.AppendEntity(dim);
         t.AddNewlyCreatedDBObject(dim, true);
         return dim;
      }

      protected BlockReference CreateBlRef(Point3d ptPos, ObjectId idBtr)
      {
         BlockReference blRef = new BlockReference(ptPos, idBtr);
         //var mScale = Matrix3d.Scaling(Settings.Default.SheetScale, ptBlView);
         //blRefView.TransformBy(mScale);
         blRef.ScaleFactors = new Scale3d(Settings.Default.SheetScale);
         blRef.Layer = "0";
         blRef.ColorIndex = 256; // ByLayer                 

         btrDim.AppendEntity(blRef);
         t.AddNewlyCreatedDBObject(blRef, true);
         return blRef;
      }
   }
}

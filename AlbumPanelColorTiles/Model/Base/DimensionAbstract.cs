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
      protected int rangeSizeVertic = Settings.Default.TileHeight + Settings.Default.TileSeam;
      protected int rangeSizeHor = Settings.Default.TileLenght + Settings.Default.TileSeam;
      protected BlockTableRecord btrDim;
      protected BlockTableRecord btrPanel;
      protected PanelBase panelBase;
      protected Transaction t;
      protected ObjectId idBlRefDim;
      // Максимальная отметка линии размера в верхней части панели
      protected double yDimLineTopMax;
      // Минимальная отметка линии размера в нижней части панели
      protected double yDimLineBotMin;
      // Минимальная координата x линии размера слева
      protected double xDimLineLeftMin;
      // Максимальная координата x линии размера справа
      protected double xDimLineRightMax;
      protected double indentBetweenDimLine = 160;
      protected double indentDimLineFromDraw = 180;

      protected Point3d ptPosHorizontalPanelSection;
      protected Point3d ptPosProfile;
      protected Point3d ptPosArrowToHorSec;

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

         using (var bt = panelBase.Service.Db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
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
         // Добавление ссылки блока обр в блок панели
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
         yDimLineTopMax = hasInterDim ? panelBase.Height + indentBetweenDimLine + indentDimLineFromDraw : panelBase.Height + indentDimLineFromDraw;
         Point3d ptDimLineTotal = new Point3d(0, yDimLineTopMax, 0);
         CreateDim(ptTopLeft, ptTopRight, ptDimLineTotal, doTrans, trans, rangeSize: rangeSizeHor);
         // добавление промежуточных размеров
         if (hasInterDim)
         {
            panelBase.PtsForTopDim.Sort();
            AcadLib.Comparers.DoubleEqualityComparer comparer = new AcadLib.Comparers.DoubleEqualityComparer(4);
            var ptsX = panelBase.PtsForTopDim.GroupBy(p => p, comparer).Select(g => g.First());

            Point3d ptPrev = ptTopLeft;
            Point3d ptDimLineInter = new Point3d(0, yDimLineTopMax - indentBetweenDimLine, 0);
            foreach (var x in ptsX)
            {
               Point3d ptNext = new Point3d(x, ptPrev.Y, 0);
               CreateDim(ptPrev, ptNext, ptDimLineInter, doTrans, trans, rangeSize: rangeSizeHor);
               ptPrev = ptNext;
            }
            // Замыкающий размер
            CreateDim(ptPrev, ptTopRight, ptDimLineInter, doTrans, trans, rangeSize: rangeSizeHor);


            //если есть пустые области Outsides, то добавление промежеточных размеров.
            if (Math.Abs(panelBase.XMinPanel - panelBase.XMinContour)>100)
            {
               CreateDim(new Point3d(panelBase.XMinPanel, ptPrev.Y, 0),
                         new Point3d(panelBase.XMinContour, ptPrev.Y, 0), ptDimLineInter, doTrans, trans);
            }
            if (Math.Abs(panelBase.XMaxPanel - panelBase.XMaxContour) > 100)
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
         yDimLineBotMin = -indentDimLineFromDraw -indentBetweenDimLine;
         Point3d ptDimLineTotal = new Point3d(0, yDimLineBotMin, 0);
         CreateDim(ptBotLeft, ptBotRight, ptDimLineTotal, doTrans, trans);
         // Промежуточный размер
         Point3d ptDimLineInter = new Point3d(0, yDimLineBotMin + indentBetweenDimLine, 0);
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

         xDimLineLeftMin = ptBotLeft.X-indentDimLineFromDraw;
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
         xDimLineRightMax = hasIndentDim ? ptBotRight.X+indentDimLineFromDraw+indentBetweenDimLine : ptBotRight.X + indentDimLineFromDraw;

         // Общий размер                
         Point3d ptDimLineTotal = new Point3d(xDimLineRightMax, 0, 0);
         CreateDim(ptBotRight, ptTopRight, ptDimLineTotal, doTrans, trans, rangeSize: rangeSizeVertic, rotation: 90);
         // Промежуточные размеры
         if (hasIndentDim)
         {
            Point3d ptDimLineIndent = new Point3d(xDimLineRightMax - indentBetweenDimLine, 0, 0);
            Point3d ptMinWin= ptBotRight;
            if (yWinMin>0)
            {
               var countTileMinWin = Convert.ToInt32(yWinMin / heightTile);
               var yTilesMinWin = (countTileMinWin * heightTile) - 12;
               ptMinWin = new Point3d(ptBotRight.X, yTilesMinWin, 0);
               CreateDim(ptBotRight, ptMinWin, ptDimLineIndent, doTrans, trans, rangeSize: rangeSizeVertic, rotation: 90, interlineText:true);
               var ptMinWinSeam = new Point3d(ptMinWin.X, ptMinWin.Y+12, 0);
               var dimSeamMin= CreateDim(ptMinWin, ptMinWinSeam, ptDimLineIndent, doTrans, trans, rotation: 90);
               Point3d ptTextSeamMin = new Point3d(ptDimLineIndent.X, ptMinWinSeam.Y+65,0);
               dimSeamMin.TextPosition = doTrans? ptTextSeamMin.TransformBy(trans): ptTextSeamMin;
               ptMinWin = ptMinWinSeam;
            }
            var countTileMaxWin = Convert.ToInt32(yWinMax / heightTile);
            var yTilesMaxWin = (countTileMaxWin * heightTile) - 12;
            Point3d ptMaxWin = new Point3d(ptMinWin.X, yTilesMaxWin, 0);
            CreateDim(ptMinWin, ptMaxWin, ptDimLineIndent, doTrans, trans, rangeSize: rangeSizeVertic, rotation: 90, interlineText: true);
            Point3d ptMaxWinSeam = new Point3d(ptMaxWin.X, ptMaxWin.Y+12, 0);
            var dimSeamMax = CreateDim(ptMaxWin, ptMaxWinSeam, ptDimLineIndent, doTrans, trans, rotation: 90);
            Point3d ptTextSeamMax = new Point3d(ptDimLineIndent.X, ptMaxWin.Y - 65, 0);
            dimSeamMax.TextPosition = doTrans ? ptTextSeamMax.TransformBy(trans) : ptTextSeamMax;
            // размер до верха плиток
            CreateDim(ptMaxWinSeam, ptTopRight, ptDimLineIndent, doTrans, trans, rangeSize: rangeSizeVertic, rotation: 90, interlineText: true);
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
               Point3d ptText = new Point3d(ptPrevCheek.X + deltaX, ptDimLineCheek.Y - 100, 0);
               dimCheek.TextPosition = doTrans? ptText.TransformBy(trans) : ptText;
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
         BlockReference blRefView = CreateBlRefInBtrDim(ptBlView, panelBase.Service.Env.IdBtrView, Settings.Default.SheetScale);

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

      /// <summary>
      /// Вставка блока профиля и стрелок к местам установки профиля на стыке плиток в торце панели
      /// </summary>
      protected void addProfileTile(bool doTrans, Matrix3d trans)
      {
         double xPtProfile = 0;
         double yPtProfile = 0;
         double xPtArrowDirect = 0;
         if (panelBase.IsCheekLeft)
         {
            // Для торца слева - профиль вставляется в одну и туже точку (-400,-600)
            xPtProfile = -400;
            yPtProfile = -600;
            ptPosArrowToHorSec = ptPosHorizontalPanelSection;
         }
         else if (panelBase.IsCheekRight)
         {
            // Для торца справа - на раст (400,600) от нижнего правого края последней плитки
            xPtProfile = panelBase.XMaxContour + 400;
            yPtProfile = -600;
            xPtArrowDirect = panelBase.XMaxContour;
            ptPosArrowToHorSec = new Point3d(panelBase.XMaxContour, ptPosHorizontalPanelSection.Y, 0);
         }
         else
         {
            // Не нужен профиль для панелей без торцов (Cheek)
            return;
         }

         // Поиск блока профиля в инвенторе
         BlockInfo biProfile = panelBase.Service.Env.BlocksInFacade.FirstOrDefault(b => b.BlName.Equals(Settings.Default.BlockProfileTile));
         if (biProfile == null)
         {
            return;
         }

         ObjectId idBtrProfile = biProfile.IdBtr;
         // Точка вставки блока профиля
         ptPosProfile = new Point3d(xPtProfile, yPtProfile, 0);
         Point3d ptBlrefProfile = ptPosProfile;
         if (doTrans)
         {
            ptBlrefProfile = ptPosProfile.TransformBy(trans);
         }

         // Вставка блока профиля - название и марка
         var blRefProfile = CreateBlRefInBtrDim(ptBlrefProfile, idBtrProfile, Settings.Default.SheetScale);
         // Добавление атрибутов Названия и марки
         // Атрибут Названия - из вхождения атрибута
         var atrRefName = biProfile.AttrsRef.FirstOrDefault(a => a.Tag.Equals("НАЗВАНИЕ"));
         if (atrRefName != null)
         {
            // определение атрибута
            var atrDefName = biProfile.AttrsDef.FirstOrDefault(a => a.Tag.Equals("НАЗВАНИЕ"));
            if (atrDefName != null)
            {
               AddAttrToBlockRef(blRefProfile, atrDefName.IdAtr, atrRefName.Text);
            }
         }
         // Атрибут Марки - из вхождения атрибута
         var atrRefMark = biProfile.AttrsRef.FirstOrDefault(a => a.Tag.Equals("МАРКА"));
         if (atrRefMark != null)
         {
            // определение атрибута
            var atrDefMark = biProfile.AttrsDef.FirstOrDefault(a => a.Tag.Equals("МАРКА"));
            if (atrDefMark != null)
            {
               AddAttrToBlockRef(blRefProfile, atrDefMark.IdAtr, atrRefMark.Text);
            }
         }

         // Вставка стрелки до угла панели
         if (!panelBase.Service.Env.IdBtrArrow.IsNull)
         {  
            Point3d ptArrowPos = new Point3d(xPtProfile, yPtProfile+4*Settings.Default.SheetScale, 0);
            Point3d ptArrowDirect = new Point3d(xPtArrowDirect, 0, 0);
            if (doTrans)
            {
               ptArrowPos = ptArrowPos.TransformBy(trans);
               ptArrowDirect = ptArrowDirect.TransformBy(trans);
            }
            // вставка блока стрелки
            var blRefArrow = CreateBlRefInBtrDim(ptArrowPos, panelBase.Service.Env.IdBtrArrow, Settings.Default.SheetScale);
            // поворот стрелки и установка длины
            Vector2d vecArrow = ptArrowDirect.Convert2d() - ptArrowPos.Convert2d();
            blRefArrow.Rotation = vecArrow.Angle;
            // длина стрелки
            setDynParam(blRefArrow, "Длина", vecArrow.Length);
         }
      }

      private string getTextRange(double measurement, int rangeSize)
      {
         //var len = Settings.Default.TileLenght + Settings.Default.TileSeam;
         var count = Convert.ToInt32(measurement / rangeSize);
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
                    bool doTrans, Matrix3d trans, int rangeSize = 0, double rotation = 0, bool interlineText = false)
      {
         if (doTrans)
         {
            ptPrev = ptPrev.TransformBy(trans);
            ptNext = ptNext.TransformBy(trans);
            ptDimLine = ptDimLine.TransformBy(trans);
         }         

         var dim = new RotatedDimension(rotation.ToRadians(), ptPrev, ptNext, ptDimLine, "", panelBase.Service.Env.IdDimStyle);
         if (rangeSize>0)
         {
            dim.Suffix = getTextRange(dim.Measurement, rangeSize);            
         }
         if (interlineText)
         {
            dim.Dimtmove = 0;
            dim.Dimtix = true;
         }
         dim.Dimscale = Settings.Default.SheetScale;
         btrDim.AppendEntity(dim);
         t.AddNewlyCreatedDBObject(dim, true);
         return dim;
      }

      protected BlockReference CreateBlRefInBtrDim(Point3d ptPos, ObjectId idBtr, double scale)
      {
         BlockReference blRef = new BlockReference(ptPos, idBtr);
         //var mScale = Matrix3d.Scaling(Settings.Default.SheetScale, ptBlView);
         //blRefView.TransformBy(mScale);
         if (scale != 1)
         {
            blRef.ScaleFactors = new Scale3d(scale);
         }         
         blRef.Layer = "0";
         blRef.ColorIndex = 256; // ByLayer                 

         btrDim.AppendEntity(blRef);
         t.AddNewlyCreatedDBObject(blRef, true);
         return blRef;
      }

      protected AttributeReference AddAttrToBlockRef(BlockReference blRef, ObjectId idAttrDef, string textString)
      {
         using (var attrDef = idAttrDef.GetObject(OpenMode.ForRead, false, true) as AttributeDefinition)
         {
            var attrRef = new AttributeReference();
            attrRef.SetAttributeFromBlock(attrDef, blRef.BlockTransform);
            attrRef.TextString = textString;

            blRef.AttributeCollection.AppendAttribute(attrRef);
            t.AddNewlyCreatedDBObject(attrRef, true);
            return attrRef;            
         }
      }

      protected Result setDynParam(BlockReference blRef, string propName, double value)
      {
         foreach (DynamicBlockReferenceProperty prop in blRef.DynamicBlockReferencePropertyCollection)
         {
            if (!prop.ReadOnly && prop.PropertyName.Equals(propName, StringComparison.OrdinalIgnoreCase))
            {
               prop.Value = value;
               return Result.Ok();
            }
         }
         return Result.Fail($"Не найден динамический параметр {propName}");
      }
   }
}

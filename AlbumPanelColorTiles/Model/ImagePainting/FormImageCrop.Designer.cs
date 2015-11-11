namespace AlbumPanelColorTiles.ImagePainting
{
   partial class FormImageCrop
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImageCrop));
         this.pictureBoxImage = new System.Windows.Forms.PictureBox();
         this.buttonBrowse = new System.Windows.Forms.Button();
         this.buttonFire = new System.Windows.Forms.Button();
         this.buttonSelect = new System.Windows.Forms.Button();
         this.buttonSpotSize = new System.Windows.Forms.Button();
         this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).BeginInit();
         this.SuspendLayout();
         // 
         // pictureBoxImage
         // 
         this.pictureBoxImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.pictureBoxImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.pictureBoxImage.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pictureBoxImage.Location = new System.Drawing.Point(0, 0);
         this.pictureBoxImage.Name = "pictureBoxImage";
         this.pictureBoxImage.Size = new System.Drawing.Size(974, 764);
         this.pictureBoxImage.TabIndex = 0;
         this.pictureBoxImage.TabStop = false;
         // 
         // buttonBrowse
         // 
         this.buttonBrowse.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.image;
         this.buttonBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.buttonBrowse.Location = new System.Drawing.Point(12, 12);
         this.buttonBrowse.Name = "buttonBrowse";
         this.buttonBrowse.Size = new System.Drawing.Size(45, 40);
         this.buttonBrowse.TabIndex = 1;
         this.toolTip1.SetToolTip(this.buttonBrowse, "Выбор файла картинки на компьютере");
         this.buttonBrowse.UseVisualStyleBackColor = true;
         this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
         // 
         // buttonFire
         // 
         this.buttonFire.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonFire.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonFire.BackgroundImage")));
         this.buttonFire.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.buttonFire.Location = new System.Drawing.Point(912, 702);
         this.buttonFire.Name = "buttonFire";
         this.buttonFire.Size = new System.Drawing.Size(50, 50);
         this.buttonFire.TabIndex = 3;
         this.toolTip1.SetToolTip(this.buttonFire, "Покраска области чертежа по картинке");
         this.buttonFire.UseVisualStyleBackColor = true;
         this.buttonFire.Click += new System.EventHandler(this.buttonFire_Click);
         // 
         // buttonSelect
         // 
         this.buttonSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonSelect.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.select;
         this.buttonSelect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.buttonSelect.Location = new System.Drawing.Point(0, 739);
         this.buttonSelect.Name = "buttonSelect";
         this.buttonSelect.Size = new System.Drawing.Size(25, 25);
         this.buttonSelect.TabIndex = 8;
         this.toolTip1.SetToolTip(this.buttonSelect, "Задание области покраски на чертеже");
         this.buttonSelect.UseVisualStyleBackColor = true;
         this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
         // 
         // buttonSpotSize
         // 
         this.buttonSpotSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonSpotSize.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.size;
         this.buttonSpotSize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
         this.buttonSpotSize.Location = new System.Drawing.Point(31, 739);
         this.buttonSpotSize.Name = "buttonSpotSize";
         this.buttonSpotSize.Size = new System.Drawing.Size(25, 25);
         this.buttonSpotSize.TabIndex = 9;
         this.toolTip1.SetToolTip(this.buttonSpotSize, "Задание размера блока зоны покраски для одного пикселя на чертеже. Рекомендуется " +
        "квадратное соотношение размеров зоны.");
         this.buttonSpotSize.UseVisualStyleBackColor = true;
         this.buttonSpotSize.Click += new System.EventHandler(this.buttonSpotSize_Click);
         // 
         // FormImageCrop
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(974, 764);
         this.Controls.Add(this.buttonSpotSize);
         this.Controls.Add(this.buttonSelect);
         this.Controls.Add(this.buttonFire);
         this.Controls.Add(this.buttonBrowse);
         this.Controls.Add(this.pictureBoxImage);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormImageCrop";
         this.ShowIcon = false;
         this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
         this.Text = "Выбор картинки";
         this.Activated += new System.EventHandler(this.FormImageCrop_Activated);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBoxImage)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.PictureBox pictureBoxImage;
      private System.Windows.Forms.Button buttonBrowse;
      private System.Windows.Forms.Button buttonFire;
      private System.Windows.Forms.Button buttonSelect;
      private System.Windows.Forms.Button buttonSpotSize;
      private System.Windows.Forms.ToolTip toolTip1;
   }
}
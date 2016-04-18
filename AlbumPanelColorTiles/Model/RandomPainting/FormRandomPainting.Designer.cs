namespace AlbumPanelColorTiles.RandomPainting
{
   partial class FormRandomPainting
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
            this.comboBoxColor = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonDraw = new System.Windows.Forms.Button();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonSpotSize = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // comboBoxColor
            // 
            this.comboBoxColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoxColor.FormattingEnabled = true;
            this.comboBoxColor.Location = new System.Drawing.Point(12, 33);
            this.comboBoxColor.Name = "comboBoxColor";
            this.comboBoxColor.Size = new System.Drawing.Size(369, 27);
            this.comboBoxColor.TabIndex = 2;
            this.toolTip1.SetToolTip(this.comboBoxColor, "Выбор цвета из слоев в чертеже");
            this.comboBoxColor.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxColor_DrawItem);
            this.comboBoxColor.SelectedIndexChanged += new System.EventHandler(this.comboBoxColor_SelectedIndexChanged);
            this.comboBoxColor.SelectionChangeCommitted += new System.EventHandler(this.comboBoxColor_SelectionChangeCommitted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Выбор цвета";
            // 
            // buttonDraw
            // 
            this.buttonDraw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDraw.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.Fire;
            this.buttonDraw.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonDraw.Location = new System.Drawing.Point(393, 127);
            this.buttonDraw.Name = "buttonDraw";
            this.buttonDraw.Size = new System.Drawing.Size(35, 35);
            this.buttonDraw.TabIndex = 4;
            this.toolTip1.SetToolTip(this.buttonDraw, "Распределение зон покраски в области чертежа");
            this.buttonDraw.UseVisualStyleBackColor = true;
            this.buttonDraw.Click += new System.EventHandler(this.buttonDraw_Click);
            // 
            // buttonSelect
            // 
            this.buttonSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSelect.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.select;
            this.buttonSelect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonSelect.Location = new System.Drawing.Point(3, 146);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(25, 25);
            this.buttonSelect.TabIndex = 7;
            this.toolTip1.SetToolTip(this.buttonSelect, "Указание области на чертеже в которой будут распределятся зоны покраски");
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.close;
            this.buttonClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonClose.Location = new System.Drawing.Point(416, -1);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(25, 25);
            this.buttonClose.TabIndex = 6;
            this.toolTip1.SetToolTip(this.buttonClose, "Закрыть окно");
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.add;
            this.buttonAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonAdd.Location = new System.Drawing.Point(391, 30);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(30, 30);
            this.buttonAdd.TabIndex = 5;
            this.toolTip1.SetToolTip(this.buttonAdd, "Добавить выбранный слой в распределение");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Visible = false;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonSpotSize
            // 
            this.buttonSpotSize.BackgroundImage = global::AlbumPanelColorTiles.Properties.Resources.size;
            this.buttonSpotSize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonSpotSize.Location = new System.Drawing.Point(43, 146);
            this.buttonSpotSize.Name = "buttonSpotSize";
            this.buttonSpotSize.Size = new System.Drawing.Size(25, 25);
            this.buttonSpotSize.TabIndex = 10;
            this.toolTip1.SetToolTip(this.buttonSpotSize, "Задание размера блока зоны покраски");
            this.buttonSpotSize.UseVisualStyleBackColor = true;
            this.buttonSpotSize.Click += new System.EventHandler(this.buttonSpotSize_Click);
            // 
            // FormRandomPainting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(440, 174);
            this.Controls.Add(this.buttonSpotSize);
            this.Controls.Add(this.buttonSelect);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonDraw);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxColor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRandomPainting";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Произвольная покраска";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

      }

      #endregion
      private System.Windows.Forms.ComboBox comboBoxColor;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button buttonDraw;
      private System.Windows.Forms.Button buttonAdd;
      private System.Windows.Forms.Button buttonClose;
      private System.Windows.Forms.Button buttonSelect;
      private System.Windows.Forms.Button buttonSpotSize;
      private System.Windows.Forms.ToolTip toolTip1;
   }
}
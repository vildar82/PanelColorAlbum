namespace AlbumPanelColorTiles.Model
{
   partial class FormStartOptions
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
         this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOk = new System.Windows.Forms.Button();
         this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
         this.buttonDefault = new System.Windows.Forms.Button();
         this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
         this.SuspendLayout();
         // 
         // propertyGrid1
         // 
         this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.propertyGrid1.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
         this.propertyGrid1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.propertyGrid1.Location = new System.Drawing.Point(16, 15);
         this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
         this.propertyGrid1.Name = "propertyGrid1";
         this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
         this.propertyGrid1.Size = new System.Drawing.Size(443, 232);
         this.propertyGrid1.TabIndex = 0;
         this.propertyGrid1.ToolbarVisible = false;
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(359, 267);
         this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(100, 28);
         this.buttonCancel.TabIndex = 1;
         this.buttonCancel.Text = "Отмена";
         this.toolTip1.SetToolTip(this.buttonCancel, "Отмена");
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonOk
         // 
         this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonOk.Location = new System.Drawing.Point(251, 267);
         this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
         this.buttonOk.Name = "buttonOk";
         this.buttonOk.Size = new System.Drawing.Size(100, 28);
         this.buttonOk.TabIndex = 1;
         this.buttonOk.Text = "ОК";
         this.buttonOk.UseVisualStyleBackColor = true;
         this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
         // 
         // buttonDefault
         // 
         this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonDefault.Location = new System.Drawing.Point(16, 269);
         this.buttonDefault.Margin = new System.Windows.Forms.Padding(4);
         this.buttonDefault.Name = "buttonDefault";
         this.buttonDefault.Size = new System.Drawing.Size(72, 28);
         this.buttonDefault.TabIndex = 2;
         this.buttonDefault.Text = "Сброс";
         this.toolTip1.SetToolTip(this.buttonDefault, "Установка выбранному параметру значения по умолчанию.");
         this.buttonDefault.UseVisualStyleBackColor = true;
         this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
         // 
         // errorProvider1
         // 
         this.errorProvider1.ContainerControl = this;
         // 
         // FormStartOptions
         // 
         this.AcceptButton = this.buttonOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(475, 310);
         this.Controls.Add(this.buttonDefault);
         this.Controls.Add(this.buttonOk);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.propertyGrid1);
         this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.Margin = new System.Windows.Forms.Padding(4);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormStartOptions";
         this.ShowIcon = false;
         this.ShowInTaskbar = false;
         this.Text = "АКР. Стартовые параметры";
         this.TopMost = true;
         ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.PropertyGrid propertyGrid1;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.ToolTip toolTip1;
      private System.Windows.Forms.Button buttonOk;
      private System.Windows.Forms.Button buttonDefault;
      private System.Windows.Forms.ErrorProvider errorProvider1;
   }
}
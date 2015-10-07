namespace AlbumPanelColorTiles.Model.Forms
{
   partial class FormRenameMarkAR
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRenameMarkAR));
         this.listBoxMarksAR = new System.Windows.Forms.ListBox();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOk = new System.Windows.Forms.Button();
         this.textBoxNewMark = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.textBoxOldMarkAR = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.buttonRename = new System.Windows.Forms.Button();
         this.labelPreview = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.buttonShow = new System.Windows.Forms.Button();
         this.errorProviderError = new System.Windows.Forms.ErrorProvider(this.components);
         this.errorProviderOk = new System.Windows.Forms.ErrorProvider(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderError)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderOk)).BeginInit();
         this.SuspendLayout();
         // 
         // listBoxMarksAR
         // 
         this.listBoxMarksAR.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.listBoxMarksAR.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.listBoxMarksAR.FormattingEnabled = true;
         this.listBoxMarksAR.ItemHeight = 16;
         this.listBoxMarksAR.Location = new System.Drawing.Point(12, 12);
         this.listBoxMarksAR.Name = "listBoxMarksAR";
         this.listBoxMarksAR.Size = new System.Drawing.Size(565, 452);
         this.listBoxMarksAR.TabIndex = 0;
         this.listBoxMarksAR.SelectedIndexChanged += new System.EventHandler(this.listBoxMarksAR_SelectedIndexChanged);
         this.listBoxMarksAR.DoubleClick += new System.EventHandler(this.listBoxMarksAR_DoubleClick);
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.buttonCancel.Location = new System.Drawing.Point(502, 613);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(75, 23);
         this.buttonCancel.TabIndex = 1;
         this.buttonCancel.Text = "Отмена";
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonOk
         // 
         this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.buttonOk.Location = new System.Drawing.Point(389, 613);
         this.buttonOk.Name = "buttonOk";
         this.buttonOk.Size = new System.Drawing.Size(107, 23);
         this.buttonOk.TabIndex = 1;
         this.buttonOk.Text = "Продолжить";
         this.buttonOk.UseVisualStyleBackColor = true;
         // 
         // textBoxNewMark
         // 
         this.textBoxNewMark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textBoxNewMark.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.textBoxNewMark.Location = new System.Drawing.Point(20, 554);
         this.textBoxNewMark.Name = "textBoxNewMark";
         this.textBoxNewMark.Size = new System.Drawing.Size(125, 22);
         this.textBoxNewMark.TabIndex = 3;
         this.textBoxNewMark.TextChanged += new System.EventHandler(this.textBoxNewMark_TextChanged);
         // 
         // label2
         // 
         this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label2.AutoSize = true;
         this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.label2.Location = new System.Drawing.Point(17, 488);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(162, 16);
         this.label2.TabIndex = 2;
         this.label2.Text = "Старая марка покраски";
         // 
         // textBoxOldMarkAR
         // 
         this.textBoxOldMarkAR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textBoxOldMarkAR.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.textBoxOldMarkAR.Location = new System.Drawing.Point(20, 504);
         this.textBoxOldMarkAR.Name = "textBoxOldMarkAR";
         this.textBoxOldMarkAR.ReadOnly = true;
         this.textBoxOldMarkAR.Size = new System.Drawing.Size(274, 22);
         this.textBoxOldMarkAR.TabIndex = 3;
         // 
         // label1
         // 
         this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label1.AutoSize = true;
         this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.label1.Location = new System.Drawing.Point(17, 535);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(156, 16);
         this.label1.TabIndex = 2;
         this.label1.Text = "Новая марка покраски";
         // 
         // buttonRename
         // 
         this.buttonRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonRename.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.buttonRename.Location = new System.Drawing.Point(169, 554);
         this.buttonRename.Name = "buttonRename";
         this.buttonRename.Size = new System.Drawing.Size(125, 23);
         this.buttonRename.TabIndex = 4;
         this.buttonRename.Text = "Переименовать";
         this.buttonRename.UseVisualStyleBackColor = true;
         this.buttonRename.Click += new System.EventHandler(this.buttonRename_Click);
         // 
         // labelPreview
         // 
         this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.labelPreview.AutoSize = true;
         this.labelPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.labelPreview.Location = new System.Drawing.Point(17, 616);
         this.labelPreview.Name = "labelPreview";
         this.labelPreview.Size = new System.Drawing.Size(29, 16);
         this.labelPreview.TabIndex = 2;
         this.labelPreview.Text = "test";
         // 
         // label3
         // 
         this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label3.AutoSize = true;
         this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.label3.Location = new System.Drawing.Point(17, 592);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(50, 16);
         this.label3.TabIndex = 2;
         this.label3.Text = "Марка";
         // 
         // buttonShow
         // 
         this.buttonShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
         this.buttonShow.Location = new System.Drawing.Point(425, 488);
         this.buttonShow.Name = "buttonShow";
         this.buttonShow.Size = new System.Drawing.Size(140, 26);
         this.buttonShow.TabIndex = 5;
         this.buttonShow.Text = "Показать панель";
         this.buttonShow.UseVisualStyleBackColor = true;
         this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
         // 
         // errorProviderError
         // 
         this.errorProviderError.ContainerControl = this;
         this.errorProviderError.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProviderError.Icon")));
         // 
         // errorProviderOk
         // 
         this.errorProviderOk.ContainerControl = this;
         this.errorProviderOk.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProviderOk.Icon")));
         // 
         // FormRenameMarkAR
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(589, 648);
         this.Controls.Add(this.buttonShow);
         this.Controls.Add(this.buttonRename);
         this.Controls.Add(this.textBoxOldMarkAR);
         this.Controls.Add(this.textBoxNewMark);
         this.Controls.Add(this.labelPreview);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.buttonOk);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.listBoxMarksAR);
         this.Name = "FormRenameMarkAR";
         this.Text = "Переименование марок АР";
         this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormRenameMarkAR_KeyUp);
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderError)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderOk)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ListBox listBoxMarksAR;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonOk;
      private System.Windows.Forms.TextBox textBoxNewMark;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBoxOldMarkAR;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button buttonRename;
      private System.Windows.Forms.Label labelPreview;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Button buttonShow;
      private System.Windows.Forms.ErrorProvider errorProviderError;
      private System.Windows.Forms.ErrorProvider errorProviderOk;
   }
}
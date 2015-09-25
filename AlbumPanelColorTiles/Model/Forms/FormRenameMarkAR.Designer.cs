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
         this.SuspendLayout();
         // 
         // listBoxMarksAR
         // 
         this.listBoxMarksAR.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.listBoxMarksAR.FormattingEnabled = true;
         this.listBoxMarksAR.Location = new System.Drawing.Point(12, 12);
         this.listBoxMarksAR.Name = "listBoxMarksAR";
         this.listBoxMarksAR.Size = new System.Drawing.Size(559, 381);
         this.listBoxMarksAR.TabIndex = 0;
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(496, 555);
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
         this.buttonOk.Location = new System.Drawing.Point(408, 555);
         this.buttonOk.Name = "buttonOk";
         this.buttonOk.Size = new System.Drawing.Size(82, 23);
         this.buttonOk.TabIndex = 1;
         this.buttonOk.Text = "Продолжить";
         this.buttonOk.UseVisualStyleBackColor = true;
         // 
         // textBoxNewMark
         // 
         this.textBoxNewMark.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textBoxNewMark.Location = new System.Drawing.Point(21, 479);
         this.textBoxNewMark.Name = "textBoxNewMark";
         this.textBoxNewMark.Size = new System.Drawing.Size(125, 20);
         this.textBoxNewMark.TabIndex = 3;
         this.textBoxNewMark.TextChanged += new System.EventHandler(this.textBoxNewMark_TextChanged);
         // 
         // label2
         // 
         this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(18, 416);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(129, 13);
         this.label2.TabIndex = 2;
         this.label2.Text = "Старая марка покраски";
         // 
         // textBoxOldMarkAR
         // 
         this.textBoxOldMarkAR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textBoxOldMarkAR.Location = new System.Drawing.Point(21, 432);
         this.textBoxOldMarkAR.Name = "textBoxOldMarkAR";
         this.textBoxOldMarkAR.Size = new System.Drawing.Size(256, 20);
         this.textBoxOldMarkAR.TabIndex = 3;
         // 
         // label1
         // 
         this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(18, 463);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(125, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "Новая марка покраски";
         // 
         // buttonRename
         // 
         this.buttonRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.buttonRename.Location = new System.Drawing.Point(170, 477);
         this.buttonRename.Name = "buttonRename";
         this.buttonRename.Size = new System.Drawing.Size(107, 23);
         this.buttonRename.TabIndex = 4;
         this.buttonRename.Text = "Переименовать";
         this.buttonRename.UseVisualStyleBackColor = true;
         this.buttonRename.Click += new System.EventHandler(this.buttonRename_Click);
         // 
         // labelPreview
         // 
         this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.labelPreview.AutoSize = true;
         this.labelPreview.Location = new System.Drawing.Point(18, 533);
         this.labelPreview.Name = "labelPreview";
         this.labelPreview.Size = new System.Drawing.Size(74, 13);
         this.labelPreview.TabIndex = 2;
         this.labelPreview.Text = "Новая марка";
         // 
         // label3
         // 
         this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(18, 520);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(74, 13);
         this.label3.TabIndex = 2;
         this.label3.Text = "Новая марка";
         // 
         // FormRenameMarkAR
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(583, 588);
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
   }
}
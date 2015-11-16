namespace AlbumPanelColorTiles.PanelLibrary
{
   partial class FormPanelDesc
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
         this.textBoxDesc = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOk = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // textBoxDesc
         // 
         this.textBoxDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxDesc.Location = new System.Drawing.Point(12, 29);
         this.textBoxDesc.Multiline = true;
         this.textBoxDesc.Name = "textBoxDesc";
         this.textBoxDesc.Size = new System.Drawing.Size(364, 139);
         this.textBoxDesc.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 13);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(70, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Примечание";
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(301, 192);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(75, 23);
         this.buttonCancel.TabIndex = 2;
         this.buttonCancel.Text = "Отмена";
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonOk
         // 
         this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonOk.Location = new System.Drawing.Point(220, 192);
         this.buttonOk.Name = "buttonOk";
         this.buttonOk.Size = new System.Drawing.Size(75, 23);
         this.buttonOk.TabIndex = 2;
         this.buttonOk.Text = "ОК";
         this.buttonOk.UseVisualStyleBackColor = true;
         this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
         // 
         // FormPanelDesc
         // 
         this.AcceptButton = this.buttonOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(388, 229);
         this.Controls.Add(this.buttonOk);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.textBoxDesc);
         this.Name = "FormPanelDesc";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "FormPanelDesc";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox textBoxDesc;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonOk;
   }
}
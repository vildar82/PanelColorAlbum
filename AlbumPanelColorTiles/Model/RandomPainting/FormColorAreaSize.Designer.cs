namespace AlbumPanelColorTiles.RandomPainting
{
   partial class FormColorAreaSize
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
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonOk = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.textBoxLenght = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.textBoxHeight = new System.Windows.Forms.TextBox();
         this.errorProviderError = new System.Windows.Forms.ErrorProvider(this.components);
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderError)).BeginInit();
         this.SuspendLayout();
         // 
         // buttonCancel
         // 
         this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(187, 105);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(75, 23);
         this.buttonCancel.TabIndex = 0;
         this.buttonCancel.Text = "Отмена";
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonOk
         // 
         this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonOk.Location = new System.Drawing.Point(106, 105);
         this.buttonOk.Name = "buttonOk";
         this.buttonOk.Size = new System.Drawing.Size(75, 23);
         this.buttonOk.TabIndex = 0;
         this.buttonOk.Text = "ОК";
         this.buttonOk.UseVisualStyleBackColor = true;
         this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 24);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(40, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Длина";
         // 
         // textBoxLenght
         // 
         this.textBoxLenght.Location = new System.Drawing.Point(12, 49);
         this.textBoxLenght.Name = "textBoxLenght";
         this.textBoxLenght.Size = new System.Drawing.Size(100, 20);
         this.textBoxLenght.TabIndex = 2;
         this.textBoxLenght.Leave += new System.EventHandler(this.textBoxLenght_Leave);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(140, 24);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(45, 13);
         this.label2.TabIndex = 1;
         this.label2.Text = "Высота";
         // 
         // textBoxHeight
         // 
         this.textBoxHeight.Location = new System.Drawing.Point(140, 49);
         this.textBoxHeight.Name = "textBoxHeight";
         this.textBoxHeight.Size = new System.Drawing.Size(100, 20);
         this.textBoxHeight.TabIndex = 2;
         this.textBoxHeight.Leave += new System.EventHandler(this.textBoxHeight_Leave);
         // 
         // errorProviderError
         // 
         this.errorProviderError.ContainerControl = this;
         // 
         // FormColorAreaSize
         // 
         this.AcceptButton = this.buttonOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(274, 140);
         this.Controls.Add(this.textBoxHeight);
         this.Controls.Add(this.textBoxLenght);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.buttonOk);
         this.Controls.Add(this.buttonCancel);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "FormColorAreaSize";
         this.ShowIcon = false;
         this.Text = "Размер зоны покраски";
         ((System.ComponentModel.ISupportInitialize)(this.errorProviderError)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonOk;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox textBoxLenght;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBoxHeight;
      private System.Windows.Forms.ErrorProvider errorProviderError;
   }
}
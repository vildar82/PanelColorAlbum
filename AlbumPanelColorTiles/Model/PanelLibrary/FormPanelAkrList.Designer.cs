namespace AlbumPanelColorTiles.PanelLibrary
{
   partial class FormPanelAkrList
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
         this.listBoxPanels = new System.Windows.Forms.ListBox();
         this.label1 = new System.Windows.Forms.Label();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonAdd = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // listBoxPanels
         // 
         this.listBoxPanels.FormattingEnabled = true;
         this.listBoxPanels.Location = new System.Drawing.Point(12, 26);
         this.listBoxPanels.Name = "listBoxPanels";
         this.listBoxPanels.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.listBoxPanels.Size = new System.Drawing.Size(423, 303);
         this.listBoxPanels.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 9);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(231, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Остальные АКР-Панели в текущем чертеже";
         // 
         // buttonCancel
         // 
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(362, 364);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(75, 23);
         this.buttonCancel.TabIndex = 2;
         this.buttonCancel.Text = "Отмена";
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonAdd
         // 
         this.buttonAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonAdd.Location = new System.Drawing.Point(281, 364);
         this.buttonAdd.Name = "buttonAdd";
         this.buttonAdd.Size = new System.Drawing.Size(75, 23);
         this.buttonAdd.TabIndex = 2;
         this.buttonAdd.Text = "Добавить";
         this.buttonAdd.UseVisualStyleBackColor = true;
         this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
         // 
         // FormPanelAkrList
         // 
         this.AcceptButton = this.buttonAdd;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(449, 399);
         this.Controls.Add(this.buttonAdd);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.listBoxPanels);
         this.Name = "FormPanelAkrList";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "FormPanelAkrList";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ListBox listBoxPanels;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonAdd;
   }
}
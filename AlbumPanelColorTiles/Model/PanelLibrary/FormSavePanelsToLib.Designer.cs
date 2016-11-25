namespace AlbumPanelColorTiles.PanelLibrary
{
   partial class FormSavePanelsToLib
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
            this.listBoxNew = new System.Windows.Forms.ListBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageNew = new System.Windows.Forms.TabPage();
            this.lbCountNew = new System.Windows.Forms.Label();
            this.tabPageChanged = new System.Windows.Forms.TabPage();
            this.lbCountChenged = new System.Windows.Forms.Label();
            this.listBoxChanged = new System.Windows.Forms.ListBox();
            this.tabPageForce = new System.Windows.Forms.TabPage();
            this.listBoxForce = new System.Windows.Forms.ListBox();
            this.buttonDel = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonShow = new System.Windows.Forms.Button();
            this.buttonShowInLib = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonDesc = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageNew.SuspendLayout();
            this.tabPageChanged.SuspendLayout();
            this.tabPageForce.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxNew
            // 
            this.listBoxNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxNew.FormattingEnabled = true;
            this.listBoxNew.Location = new System.Drawing.Point(3, 3);
            this.listBoxNew.Name = "listBoxNew";
            this.listBoxNew.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxNew.Size = new System.Drawing.Size(376, 295);
            this.listBoxNew.TabIndex = 0;
            this.toolTip1.SetToolTip(this.listBoxNew, "Панели которых нет в библиотеке");
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageNew);
            this.tabControl.Controls.Add(this.tabPageChanged);
            this.tabControl.Controls.Add(this.tabPageForce);
            this.tabControl.Location = new System.Drawing.Point(19, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(390, 327);
            this.tabControl.TabIndex = 2;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPageNew
            // 
            this.tabPageNew.Controls.Add(this.lbCountNew);
            this.tabPageNew.Controls.Add(this.listBoxNew);
            this.tabPageNew.Location = new System.Drawing.Point(4, 22);
            this.tabPageNew.Name = "tabPageNew";
            this.tabPageNew.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNew.Size = new System.Drawing.Size(382, 301);
            this.tabPageNew.TabIndex = 0;
            this.tabPageNew.Text = "Новые";
            this.tabPageNew.UseVisualStyleBackColor = true;
            // 
            // lbCountNew
            // 
            this.lbCountNew.AutoSize = true;
            this.lbCountNew.Location = new System.Drawing.Point(330, 285);
            this.lbCountNew.Name = "lbCountNew";
            this.lbCountNew.Size = new System.Drawing.Size(13, 13);
            this.lbCountNew.TabIndex = 6;
            this.lbCountNew.Text = "1";
            // 
            // tabPageChanged
            // 
            this.tabPageChanged.Controls.Add(this.lbCountChenged);
            this.tabPageChanged.Controls.Add(this.listBoxChanged);
            this.tabPageChanged.Location = new System.Drawing.Point(4, 22);
            this.tabPageChanged.Name = "tabPageChanged";
            this.tabPageChanged.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChanged.Size = new System.Drawing.Size(382, 301);
            this.tabPageChanged.TabIndex = 1;
            this.tabPageChanged.Text = "Измененные";
            this.tabPageChanged.UseVisualStyleBackColor = true;
            // 
            // lbCountChenged
            // 
            this.lbCountChenged.AutoSize = true;
            this.lbCountChenged.Location = new System.Drawing.Point(330, 285);
            this.lbCountChenged.Name = "lbCountChenged";
            this.lbCountChenged.Size = new System.Drawing.Size(13, 13);
            this.lbCountChenged.TabIndex = 5;
            this.lbCountChenged.Text = "1";
            // 
            // listBoxChanged
            // 
            this.listBoxChanged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxChanged.FormattingEnabled = true;
            this.listBoxChanged.Location = new System.Drawing.Point(3, 3);
            this.listBoxChanged.Name = "listBoxChanged";
            this.listBoxChanged.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxChanged.Size = new System.Drawing.Size(376, 295);
            this.listBoxChanged.TabIndex = 1;
            this.toolTip1.SetToolTip(this.listBoxChanged, "Панели отличаются от библиотечных");
            // 
            // tabPageForce
            // 
            this.tabPageForce.Controls.Add(this.listBoxForce);
            this.tabPageForce.Location = new System.Drawing.Point(4, 22);
            this.tabPageForce.Name = "tabPageForce";
            this.tabPageForce.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageForce.Size = new System.Drawing.Size(382, 301);
            this.tabPageForce.TabIndex = 2;
            this.tabPageForce.Text = "Принудительно";
            this.tabPageForce.UseVisualStyleBackColor = true;
            // 
            // listBoxForce
            // 
            this.listBoxForce.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxForce.FormattingEnabled = true;
            this.listBoxForce.Location = new System.Drawing.Point(3, 3);
            this.listBoxForce.Name = "listBoxForce";
            this.listBoxForce.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxForce.Size = new System.Drawing.Size(376, 295);
            this.listBoxForce.TabIndex = 1;
            this.toolTip1.SetToolTip(this.listBoxForce, "Если нужно сохранить панель, но она не попала в список Новых или Измененных панел" +
        "ей, можно принудительно сохранить добавив в этот список.");
            // 
            // buttonDel
            // 
            this.buttonDel.Location = new System.Drawing.Point(19, 345);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(75, 23);
            this.buttonDel.TabIndex = 3;
            this.buttonDel.Text = "Исключить";
            this.toolTip1.SetToolTip(this.buttonDel, "Исключить из списка");
            this.buttonDel.UseVisualStyleBackColor = true;
            this.buttonDel.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(100, 345);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = "Добавить";
            this.toolTip1.SetToolTip(this.buttonAdd, "Добавление блока панели принудително (не новой и не изменившейся)");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Visible = false;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonShow
            // 
            this.buttonShow.Location = new System.Drawing.Point(249, 345);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(75, 23);
            this.buttonShow.TabIndex = 3;
            this.buttonShow.Text = "Показать";
            this.toolTip1.SetToolTip(this.buttonShow, "Показать выбранную панель на чертеже");
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
            // 
            // buttonShowInLib
            // 
            this.buttonShowInLib.Location = new System.Drawing.Point(330, 345);
            this.buttonShowInLib.Name = "buttonShowInLib";
            this.buttonShowInLib.Size = new System.Drawing.Size(75, 23);
            this.buttonShowInLib.TabIndex = 3;
            this.buttonShowInLib.Text = "Показать в библиотеке";
            this.toolTip1.SetToolTip(this.buttonShowInLib, "Показать панель в библиотеке");
            this.buttonShowInLib.UseVisualStyleBackColor = true;
            this.buttonShowInLib.Visible = false;
            this.buttonShowInLib.Click += new System.EventHandler(this.buttonShowInLib_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(333, 397);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Отмена";
            this.toolTip1.SetToolTip(this.buttonCancel, "Выход без сохранения");
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSave.Location = new System.Drawing.Point(252, 397);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 3;
            this.buttonSave.Text = "Сохранить";
            this.toolTip1.SetToolTip(this.buttonSave, "Сохранение блоков панелей в файл библиотеки");
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonDesc
            // 
            this.buttonDesc.Location = new System.Drawing.Point(12, 397);
            this.buttonDesc.Name = "buttonDesc";
            this.buttonDesc.Size = new System.Drawing.Size(82, 23);
            this.buttonDesc.TabIndex = 4;
            this.buttonDesc.Text = "Примечание";
            this.toolTip1.SetToolTip(this.buttonDesc, "Примечание к панели. Примечание сохраниться при сохранении панелей в библиотеку.");
            this.buttonDesc.UseVisualStyleBackColor = true;
            this.buttonDesc.Click += new System.EventHandler(this.buttonDesc_Click);
            // 
            // FormSavePanelsToLib
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(420, 432);
            this.Controls.Add(this.buttonDesc);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonShowInLib);
            this.Controls.Add(this.buttonShow);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonDel);
            this.Controls.Add(this.tabControl);
            this.Name = "FormSavePanelsToLib";
            this.RightToLeftLayout = true;
            this.Text = "Сохранение блоков АКР-панелей в библиотеку";
            this.tabControl.ResumeLayout(false);
            this.tabPageNew.ResumeLayout(false);
            this.tabPageNew.PerformLayout();
            this.tabPageChanged.ResumeLayout(false);
            this.tabPageChanged.PerformLayout();
            this.tabPageForce.ResumeLayout(false);
            this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ListBox listBoxNew;
      private System.Windows.Forms.TabControl tabControl;
      private System.Windows.Forms.TabPage tabPageNew;
      private System.Windows.Forms.TabPage tabPageChanged;
      private System.Windows.Forms.TabPage tabPageForce;
      private System.Windows.Forms.Button buttonDel;
      private System.Windows.Forms.ToolTip toolTip1;
      private System.Windows.Forms.Button buttonAdd;
      private System.Windows.Forms.Button buttonShow;
      private System.Windows.Forms.Button buttonShowInLib;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonSave;
      private System.Windows.Forms.ListBox listBoxChanged;
      private System.Windows.Forms.ListBox listBoxForce;
      private System.Windows.Forms.Button buttonDesc;
        private System.Windows.Forms.Label lbCountChenged;
        private System.Windows.Forms.Label lbCountNew;
    }
}
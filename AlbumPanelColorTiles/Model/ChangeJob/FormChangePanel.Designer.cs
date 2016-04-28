namespace AlbumPanelColorTiles.ChangeJob
{
    partial class FormChangePanel
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
            this.listBoxChangePanels = new System.Windows.Forms.ListBox();
            this.groupBoxPanelAkr = new System.Windows.Forms.GroupBox();
            this.buttonAkrShow = new System.Windows.Forms.Button();
            this.textBoxAkrPaintNew = new System.Windows.Forms.TextBox();
            this.textBoxAkrMark = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonMountShow = new System.Windows.Forms.Button();
            this.textBoxMountPaintOld = new System.Windows.Forms.TextBox();
            this.textBoxMountMark = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonExeption = new System.Windows.Forms.Button();
            this.buttonExceptions = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxPanelAkr.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxChangePanels
            // 
            this.listBoxChangePanels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxChangePanels.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBoxChangePanels.FormattingEnabled = true;
            this.listBoxChangePanels.ItemHeight = 16;
            this.listBoxChangePanels.Location = new System.Drawing.Point(12, 12);
            this.listBoxChangePanels.Name = "listBoxChangePanels";
            this.listBoxChangePanels.Size = new System.Drawing.Size(462, 196);
            this.listBoxChangePanels.TabIndex = 0;
            this.listBoxChangePanels.SelectedIndexChanged += new System.EventHandler(this.listBoxChangePanels_SelectedIndexChanged);
            this.listBoxChangePanels.DoubleClick += new System.EventHandler(this.listBoxChangePanels_DoubleClick);
            // 
            // groupBoxPanelAkr
            // 
            this.groupBoxPanelAkr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxPanelAkr.Controls.Add(this.buttonAkrShow);
            this.groupBoxPanelAkr.Controls.Add(this.textBoxAkrPaintNew);
            this.groupBoxPanelAkr.Controls.Add(this.textBoxAkrMark);
            this.groupBoxPanelAkr.Controls.Add(this.label2);
            this.groupBoxPanelAkr.Controls.Add(this.label1);
            this.groupBoxPanelAkr.Location = new System.Drawing.Point(12, 226);
            this.groupBoxPanelAkr.Name = "groupBoxPanelAkr";
            this.groupBoxPanelAkr.Size = new System.Drawing.Size(272, 145);
            this.groupBoxPanelAkr.TabIndex = 2;
            this.groupBoxPanelAkr.TabStop = false;
            this.groupBoxPanelAkr.Text = "Панель АКР";
            // 
            // buttonAkrShow
            // 
            this.buttonAkrShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAkrShow.Location = new System.Drawing.Point(9, 116);
            this.buttonAkrShow.Name = "buttonAkrShow";
            this.buttonAkrShow.Size = new System.Drawing.Size(75, 23);
            this.buttonAkrShow.TabIndex = 0;
            this.buttonAkrShow.Text = "Показать";
            this.buttonAkrShow.UseVisualStyleBackColor = true;
            this.buttonAkrShow.Click += new System.EventHandler(this.buttonAkrShow_Click);
            // 
            // textBoxAkrPaintNew
            // 
            this.textBoxAkrPaintNew.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAkrPaintNew.Location = new System.Drawing.Point(9, 80);
            this.textBoxAkrPaintNew.Name = "textBoxAkrPaintNew";
            this.textBoxAkrPaintNew.ReadOnly = true;
            this.textBoxAkrPaintNew.Size = new System.Drawing.Size(257, 20);
            this.textBoxAkrPaintNew.TabIndex = 0;
            // 
            // textBoxAkrMark
            // 
            this.textBoxAkrMark.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAkrMark.Location = new System.Drawing.Point(9, 41);
            this.textBoxAkrMark.Name = "textBoxAkrMark";
            this.textBoxAkrMark.ReadOnly = true;
            this.textBoxAkrMark.Size = new System.Drawing.Size(257, 20);
            this.textBoxAkrMark.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Покраска новая";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Марка";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.buttonMountShow);
            this.groupBox1.Controls.Add(this.textBoxMountPaintOld);
            this.groupBox1.Controls.Add(this.textBoxMountMark);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(297, 226);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 145);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Монтажная панель";
            // 
            // buttonMountShow
            // 
            this.buttonMountShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonMountShow.Location = new System.Drawing.Point(9, 116);
            this.buttonMountShow.Name = "buttonMountShow";
            this.buttonMountShow.Size = new System.Drawing.Size(75, 23);
            this.buttonMountShow.TabIndex = 0;
            this.buttonMountShow.Text = "Показать";
            this.buttonMountShow.UseVisualStyleBackColor = true;
            this.buttonMountShow.Click += new System.EventHandler(this.buttonMountShow_Click);
            // 
            // textBoxMountPaintOld
            // 
            this.textBoxMountPaintOld.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMountPaintOld.Location = new System.Drawing.Point(9, 80);
            this.textBoxMountPaintOld.Name = "textBoxMountPaintOld";
            this.textBoxMountPaintOld.ReadOnly = true;
            this.textBoxMountPaintOld.Size = new System.Drawing.Size(257, 20);
            this.textBoxMountPaintOld.TabIndex = 0;
            // 
            // textBoxMountMark
            // 
            this.textBoxMountMark.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMountMark.Location = new System.Drawing.Point(9, 41);
            this.textBoxMountMark.Name = "textBoxMountMark";
            this.textBoxMountMark.ReadOnly = true;
            this.textBoxMountMark.Size = new System.Drawing.Size(257, 20);
            this.textBoxMountMark.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Покраска старая";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Марка";
            // 
            // buttonExeption
            // 
            this.buttonExeption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExeption.Enabled = false;
            this.buttonExeption.Location = new System.Drawing.Point(480, 12);
            this.buttonExeption.Name = "buttonExeption";
            this.buttonExeption.Size = new System.Drawing.Size(83, 23);
            this.buttonExeption.TabIndex = 3;
            this.buttonExeption.Text = "Исключить";
            this.buttonExeption.UseVisualStyleBackColor = true;
            this.buttonExeption.Click += new System.EventHandler(this.buttonExeption_Click);
            // 
            // buttonExceptions
            // 
            this.buttonExceptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExceptions.Enabled = false;
            this.buttonExceptions.Location = new System.Drawing.Point(481, 185);
            this.buttonExceptions.Name = "buttonExceptions";
            this.buttonExceptions.Size = new System.Drawing.Size(88, 23);
            this.buttonExceptions.TabIndex = 3;
            this.buttonExceptions.Text = "Исключенные";
            this.buttonExceptions.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(399, 396);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(490, 396);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Прервать";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // FormChangePanel
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(577, 431);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonExceptions);
            this.Controls.Add(this.buttonExeption);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxPanelAkr);
            this.Controls.Add(this.listBoxChangePanels);
            this.Name = "FormChangePanel";
            this.Text = "Измененные марки покраски";
            this.groupBoxPanelAkr.ResumeLayout(false);
            this.groupBoxPanelAkr.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxChangePanels;
        private System.Windows.Forms.GroupBox groupBoxPanelAkr;
        private System.Windows.Forms.Button buttonAkrShow;
        private System.Windows.Forms.TextBox textBoxAkrPaintNew;
        private System.Windows.Forms.TextBox textBoxAkrMark;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonMountShow;
        private System.Windows.Forms.TextBox textBoxMountPaintOld;
        private System.Windows.Forms.TextBox textBoxMountMark;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonExeption;
        private System.Windows.Forms.Button buttonExceptions;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
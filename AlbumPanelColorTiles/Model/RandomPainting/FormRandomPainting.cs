using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.EditorInput;

namespace AlbumPanelColorTiles.Model
{
   public partial class FormRandomPainting : Form
   {
      private const int DISTANCE_BETWEEN_GROUP = 78;
      private Dictionary<string, RandomPaint> _allPropers;
      private Dictionary<string, GroupBox> _groupBoxs;
      private Point _location;
      private RandomPainting _randomPaintingService;
      private Dictionary<string, RandomPaint> _trackPropers;
      private bool isMouseDown = false;
      private Point mouseOffset;

      public FormRandomPainting(Dictionary<string, RandomPaint> propers,
         RandomPainting randomPaintingService, Dictionary<string, RandomPaint> trackPropers)
      {
         InitializeComponent();

         _randomPaintingService = randomPaintingService;
         _location = new Point(12, 64);
         _allPropers = propers;
         _groupBoxs = new Dictionary<string, GroupBox>();
         // Ключ у всех - имя слоя.
         // Восстановление набора распределяемых цветов
         restoreTracks(trackPropers);
         comboBoxColor.DataSource = _allPropers.Values.ToList();
         comboBoxColor.DisplayMember = "LayerName";
      }

      public event EventHandler Fire = delegate { };

      public Dictionary<string, RandomPaint> TrackPropers { get { return _trackPropers; } }

      // Esc - сclose
      protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
      {
         if (keyData == Keys.Escape)
         {
            this.Close();
            return true;
         }
         return base.ProcessCmdKey(ref msg, keyData);
      }

      private void AddControls(RandomPaint proper)
      {
         string tag = proper.LayerName;
         GroupBox groupBox = new GroupBox();
         //CheckBox checkBox = new CheckBox(); // Lock/Ublock
         TextBox textBox = new TextBox(); // %
         Button buttonDel = new Button();
         TrackBar trackBar = new TrackBar();
         Label label = new Label();
         groupBox.SuspendLayout();
         ((ISupportInitialize)(trackBar)).BeginInit();
         SuspendLayout();
         // groupBox
         groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         groupBox.Tag = tag;
         groupBox.Name = "groupBox" + tag;
         groupBox.Size = new Size(378, 72);
         groupBox.TabStop = false;
         groupBox.Text = tag;
         groupBox.Controls.Add(textBox);
         groupBox.Controls.Add(buttonDel);
         groupBox.Controls.Add(trackBar);
         groupBox.Controls.Add(label);
         groupBox.Enabled = true;
         groupBox.Visible = true;
         groupBox.Location = _location;

         shiftLocation(DISTANCE_BETWEEN_GROUP);

         // textbox %
         textBox.Anchor = AnchorStyles.Right;
         textBox.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
         textBox.Location = new Point(291, 28);
         textBox.Name = "textBox" + tag;
         textBox.Size = new Size(43, 24);
         textBox.Tag = tag;
         textBox.Text = proper.Percent.ToString();
         // buttonDel
         buttonDel.Anchor = AnchorStyles.Right;
         buttonDel.BackgroundImage = Properties.Resources.delete;
         buttonDel.BackgroundImageLayout = ImageLayout.Stretch;
         buttonDel.Location = new Point(344, 27);
         buttonDel.Name = "buttonDel" + tag;
         buttonDel.Size = new Size(28, 28);
         buttonDel.Text = "-";
         buttonDel.UseVisualStyleBackColor = true;
         buttonDel.Tag = tag;
         // trackBar
         trackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
         trackBar.BackColor = proper.Color;
         trackBar.Location = new Point(12, 19);
         trackBar.Name = "trackBar" + tag;
         trackBar.Size = new Size(273, 45);
         trackBar.TickStyle = TickStyle.Both;
         trackBar.Tag = tag;
         trackBar.Maximum = 100;
         trackBar.Value = proper.Percent;
         // Label
         label.AutoSize = true;
         label.Location = new Point(301, 12);
         label.Name = "labelTemp2";
         label.Size = new Size(15, 13);
         label.TabIndex = 9;
         label.Text = "%";

         this.Controls.Add(groupBox);

         // События
         textBox.TextChanged += TextBox_TextChanged;
         trackBar.ValueChanged += trackBar_ValueChanged;
         buttonDel.Click += buttonDel_Click;

         _groupBoxs.Add(proper.LayerName, groupBox);
      }

      private void addTrackProper(RandomPaint proper)
      {
         // Проверка нет ли уже такого слоя в распределении
         if (!_trackPropers.ContainsKey(proper.LayerName))
         {
            _trackPropers.Add(proper.LayerName, proper);
            AddControls(proper);
         }
      }

      private void buttonAdd_Click(object sender, EventArgs e)
      {
         RandomPaint proper = comboBoxColor.SelectedItem as RandomPaint;
         // Добавление набора с ползунком
         addTrackProper(proper);
      }

      // Удаление трека
      private void buttonDel_Click(object sender, EventArgs e)
      {
         GroupBox groupBox = _groupBoxs[(string)((Button)sender).Tag];
         Point removeLocation = groupBox.Location;
         _groupBoxs.Remove((string)groupBox.Tag);
         foreach (var itemGroupBox in _groupBoxs.Values)
         {
            if (itemGroupBox.Location.Y > removeLocation.Y)
            {
               itemGroupBox.Location = new Point(itemGroupBox.Location.X, itemGroupBox.Location.Y - DISTANCE_BETWEEN_GROUP);
            }
         }
         this.Controls.Remove(groupBox);
         _trackPropers.Remove((string)groupBox.Tag);
         shiftLocation(-DISTANCE_BETWEEN_GROUP);
      }

      private void buttonDraw_Click(object sender, EventArgs e)
      {
         Fire(_trackPropers, e);
      }

      private void buttonSelect_Click(object sender, EventArgs e)
      {
         using (EditorUserInteraction UI = _randomPaintingService.Ed.StartUserInteraction(this))
         {
            _randomPaintingService.PromptExtents();
         }
      }

      private void comboBoxColor_DrawItem(object sender, DrawItemEventArgs e)
      {
         e.DrawBackground();
         RandomPaint proper = ((ComboBox)sender).Items[e.Index] as RandomPaint;
         // Покраска
         e.Graphics.FillRectangle(new SolidBrush(proper.Color), e.Bounds);
         // Текст
         e.Graphics.DrawString(proper.LayerName, ((Control)sender).Font, Brushes.Black, e.Bounds.X, e.Bounds.Y);
      }

      private void comboBoxColor_SelectedIndexChanged(object sender, EventArgs e)
      {
         RandomPaint proper = comboBoxColor.SelectedItem as RandomPaint;
         comboBoxColor.BackColor = proper.Color;
      }

      /// <summary>
      /// Распределено процентов на данный момент
      /// </summary>
      /// <returns></returns>
      private int distributedPercent()
      {
         // распределено на данный момент
         return _trackPropers.Values.Sum(p => p.Percent);
      }
      
      private void Form_KeyUp(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Escape) this.Close();
      }

      //
      // Перемещение формвы
      //
      private void Form_MouseDown(object sender, MouseEventArgs e)
      {
         int xOffset;
         int yOffset;

         if (e.Button == MouseButtons.Left)
         {
            xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
            yOffset = -e.Y - SystemInformation.CaptionHeight -
                SystemInformation.FrameBorderSize.Height;
            mouseOffset = new Point(xOffset, yOffset);
            isMouseDown = true;
         }
      }

      private void Form_MouseMove(object sender, MouseEventArgs e)
      {
         if (isMouseDown)
         {
            Point mousePos = Control.MousePosition;
            mousePos.Offset(mouseOffset.X, mouseOffset.Y);
            Location = mousePos;
         }
      }

      private void Form_MouseUp(object sender, MouseEventArgs e)
      {
         // Changes the isMouseDown field so that the form does
         // not move unless the user is pressing the left mouse button.
         if (e.Button == MouseButtons.Left)
         {
            isMouseDown = false;
         }
      }

      // Получение корректного нового значения процента (чтобы не превышалось 100% распределения)
      private int getCorrectValue(RandomPaint proper, int newValue)
      {
         int delta = newValue - proper.Percent;
         int value = newValue;
         // Нельзя распределить больше 100%
         if (delta > 0)
         {
            int oldDistributedPercent = distributedPercent();
            int newDistributedPercent = oldDistributedPercent + delta;
            if (newDistributedPercent >= 100)
            {
               value = 100 - oldDistributedPercent + proper.Percent;
               return value;
            }
         }
         return value;
      }

      private void restoreTracks(Dictionary<string, RandomPaint> trackPropers)
      {
         _trackPropers = new Dictionary<string, RandomPaint>();
         if (trackPropers != null)
         {
            foreach (var item in trackPropers)
            {
               if (_allPropers.ContainsKey(item.Key))
               {
                  addTrackProper(item.Value);
               }
            }
         }
      }

      // Запись текущего значения процента в текстбокс
      private void setPercentToTextBox(RandomPaint proper)
      {
         GroupBox groupBox = _groupBoxs[proper.LayerName];
         string tag = (string)groupBox.Tag;
         TextBox textBox = groupBox.Controls.Find("textBox" + tag, false).First() as TextBox;
         textBox.Text = proper.Percent.ToString();
      }

      // Сдвиг точки вставуки трэка на следующую позицию
      private void shiftLocation(int distance)
      {
         _location.Y += distance;
         // Проверка размера формы
         if (this.Height < (_location.Y + 40))
         {
            this.Height = _location.Y + 40;
            buttonDraw.Location = new Point(buttonDraw.Location.X, this.Height - 38);
            buttonSelect.Location = new Point(buttonSelect.Location.X, this.Height - 28);
         }
      }

      private void TextBox_TextChanged(object sender, EventArgs e)
      {
         TextBox textBox = (TextBox)sender;
         string tag = (string)textBox.Tag;
         RandomPaint proper = _trackPropers[tag];
         int val;
         if (int.TryParse(textBox.Text, out val))
         {
            val = getCorrectValue(proper, val);
            proper.Percent = val;
            GroupBox groupBox = _groupBoxs[tag];
            TrackBar trackBar = groupBox.Controls.Find("trackBar" + tag, false).First() as TrackBar;
            trackBar.Value = val;
            setPercentToTextBox(proper);
         }
      }

      private void trackBar_ValueChanged(object sender, EventArgs e)
      {
         // Распределяемый трек
         TrackBar trackBar = (TrackBar)sender;
         RandomPaint proper = _trackPropers[(string)trackBar.Tag];
         int value = getCorrectValue(proper, trackBar.Value);
         proper.Percent = value;
         trackBar.Value = value;
         // Подпись процента в TextBox
         setPercentToTextBox(proper);
      }
   }
}
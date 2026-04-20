namespace WinFormsApp6
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private void InitializeComponent()
        {
            tabPage4 = new TabPage();
            DeistvPlot = new ScottPlot.WinForms.FormsPlot();
            menuStrip4 = new MenuStrip();
            действующиеЗначенияToolStripMenuItem = new ToolStripMenuItem();
            tabPage3 = new TabPage();
            harmPlot = new ScottPlot.WinForms.FormsPlot();
            tabPage2 = new TabPage();
            FreqPlot = new ScottPlot.WinForms.FormsPlot();
            menuStrip2 = new MenuStrip();
            частотаToolStripMenuItem = new ToolStripMenuItem();
            результатыToolStripMenuItem = new ToolStripMenuItem();
            Осциллограммы = new TabPage();
            statusStrip1 = new StatusStrip();
            InfoToolStripStatusLabel = new ToolStripStatusLabel();
            OscPlot = new ScottPlot.WinForms.FormsPlot();
            menuStrip1 = new MenuStrip();
            файлToolStripMenuItem = new ToolStripMenuItem();
            осциллограммаToolStripMenuItem = new ToolStripMenuItem();
            harmPlot1212 = new TabControl();
            tabPage1 = new TabPage();
            PlotUotkl = new ScottPlot.WinForms.FormsPlot();
            menuStrip3 = new MenuStrip();
            показатьГрафикToolStripMenuItem1 = new ToolStripMenuItem();
            tabPage5 = new TabPage();
            menuStrip5 = new MenuStrip();
            показатьГрафикToolStripMenuItem = new ToolStripMenuItem();
            Plotfotkl = new ScottPlot.WinForms.FormsPlot();
            tabPage4.SuspendLayout();
            menuStrip4.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage2.SuspendLayout();
            menuStrip2.SuspendLayout();
            Осциллограммы.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            harmPlot1212.SuspendLayout();
            tabPage1.SuspendLayout();
            menuStrip3.SuspendLayout();
            tabPage5.SuspendLayout();
            menuStrip5.SuspendLayout();
            SuspendLayout();
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(DeistvPlot);
            tabPage4.Controls.Add(menuStrip4);
            tabPage4.Location = new Point(4, 29);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(678, 344);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Действующие значения U";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // DeistvPlot
            // 
            DeistvPlot.DisplayScale = 1.25F;
            DeistvPlot.Dock = DockStyle.Fill;
            DeistvPlot.Location = new Point(3, 31);
            DeistvPlot.Name = "DeistvPlot";
            DeistvPlot.Size = new Size(672, 310);
            DeistvPlot.TabIndex = 0;
            // 
            // menuStrip4
            // 
            menuStrip4.ImageScalingSize = new Size(20, 20);
            menuStrip4.Items.AddRange(new ToolStripItem[] { действующиеЗначенияToolStripMenuItem });
            menuStrip4.Location = new Point(3, 3);
            menuStrip4.Name = "menuStrip4";
            menuStrip4.Size = new Size(672, 28);
            menuStrip4.TabIndex = 1;
            menuStrip4.Text = "menuStrip4";
            // 
            // действующиеЗначенияToolStripMenuItem
            // 
            действующиеЗначенияToolStripMenuItem.Name = "действующиеЗначенияToolStripMenuItem";
            действующиеЗначенияToolStripMenuItem.Size = new Size(255, 24);
            действующиеЗначенияToolStripMenuItem.Text = "Показать действующие значения";
            действующиеЗначенияToolStripMenuItem.Click += действующиеЗначенияToolStripMenuItem_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(harmPlot);
            tabPage3.Location = new Point(4, 29);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(678, 344);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Спектр гармоник";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // harmPlot
            // 
            harmPlot.DisplayScale = 1.25F;
            harmPlot.Dock = DockStyle.Fill;
            harmPlot.Location = new Point(3, 3);
            harmPlot.Name = "harmPlot";
            harmPlot.Size = new Size(672, 338);
            harmPlot.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(FreqPlot);
            tabPage2.Controls.Add(menuStrip2);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(678, 344);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Частоты";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // FreqPlot
            // 
            FreqPlot.DisplayScale = 1.25F;
            FreqPlot.Dock = DockStyle.Fill;
            FreqPlot.Location = new Point(3, 31);
            FreqPlot.Name = "FreqPlot";
            FreqPlot.Size = new Size(672, 310);
            FreqPlot.TabIndex = 0;
            // 
            // menuStrip2
            // 
            menuStrip2.ImageScalingSize = new Size(20, 20);
            menuStrip2.Items.AddRange(new ToolStripItem[] { частотаToolStripMenuItem, результатыToolStripMenuItem });
            menuStrip2.Location = new Point(3, 3);
            menuStrip2.Name = "menuStrip2";
            menuStrip2.Size = new Size(672, 28);
            menuStrip2.TabIndex = 1;
            menuStrip2.Text = "Показать среднюю частоту";
            // 
            // частотаToolStripMenuItem
            // 
            частотаToolStripMenuItem.Name = "частотаToolStripMenuItem";
            частотаToolStripMenuItem.Size = new Size(206, 24);
            частотаToolStripMenuItem.Text = "Показать осциллограммы";
            частотаToolStripMenuItem.Click += частотаToolStripMenuItem_Click;
            // 
            // результатыToolStripMenuItem
            // 
            результатыToolStripMenuItem.Name = "результатыToolStripMenuItem";
            результатыToolStripMenuItem.Size = new Size(211, 24);
            результатыToolStripMenuItem.Text = "Показать среднюю частоту";
            результатыToolStripMenuItem.Click += результатыToolStripMenuItem_Click;
            // 
            // Осциллограммы
            // 
            Осциллограммы.Controls.Add(statusStrip1);
            Осциллограммы.Controls.Add(OscPlot);
            Осциллограммы.Controls.Add(menuStrip1);
            Осциллограммы.Location = new Point(4, 29);
            Осциллограммы.Name = "Осциллограммы";
            Осциллограммы.Padding = new Padding(3);
            Осциллограммы.Size = new Size(678, 344);
            Осциллограммы.TabIndex = 0;
            Осциллограммы.Text = "Осциллограммы";
            Осциллограммы.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { InfoToolStripStatusLabel });
            statusStrip1.Location = new Point(3, 315);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(672, 26);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // InfoToolStripStatusLabel
            // 
            InfoToolStripStatusLabel.Name = "InfoToolStripStatusLabel";
            InfoToolStripStatusLabel.Size = new Size(151, 20);
            InfoToolStripStatusLabel.Text = "toolStripStatusLabel1";
            // 
            // OscPlot
            // 
            OscPlot.DisplayScale = 1.25F;
            OscPlot.Dock = DockStyle.Fill;
            OscPlot.Location = new Point(3, 31);
            OscPlot.Name = "OscPlot";
            OscPlot.Size = new Size(672, 310);
            OscPlot.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { файлToolStripMenuItem });
            menuStrip1.Location = new Point(3, 3);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(672, 28);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { осциллограммаToolStripMenuItem });
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new Size(59, 24);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // осциллограммаToolStripMenuItem
            // 
            осциллограммаToolStripMenuItem.Name = "осциллограммаToolStripMenuItem";
            осциллограммаToolStripMenuItem.Size = new Size(206, 26);
            осциллограммаToolStripMenuItem.Text = "Осциллограмма";
            осциллограммаToolStripMenuItem.Click += осциллограммаToolStripMenuItem_Click;
            // 
            // harmPlot1212
            // 
            harmPlot1212.Controls.Add(Осциллограммы);
            harmPlot1212.Controls.Add(tabPage2);
            harmPlot1212.Controls.Add(tabPage3);
            harmPlot1212.Controls.Add(tabPage4);
            harmPlot1212.Controls.Add(tabPage1);
            harmPlot1212.Controls.Add(tabPage5);
            harmPlot1212.Dock = DockStyle.Fill;
            harmPlot1212.Location = new Point(0, 0);
            harmPlot1212.Name = "harmPlot1212";
            harmPlot1212.SelectedIndex = 0;
            harmPlot1212.Size = new Size(686, 377);
            harmPlot1212.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(PlotUotkl);
            tabPage1.Controls.Add(menuStrip3);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(678, 344);
            tabPage1.TabIndex = 4;
            tabPage1.Text = "График Отклонения U";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // PlotUotkl
            // 
            PlotUotkl.DisplayScale = 1.25F;
            PlotUotkl.Dock = DockStyle.Fill;
            PlotUotkl.Location = new Point(3, 31);
            PlotUotkl.Name = "PlotUotkl";
            PlotUotkl.Size = new Size(672, 310);
            PlotUotkl.TabIndex = 0;
            // 
            // menuStrip3
            // 
            menuStrip3.ImageScalingSize = new Size(20, 20);
            menuStrip3.Items.AddRange(new ToolStripItem[] { показатьГрафикToolStripMenuItem1 });
            menuStrip3.Location = new Point(3, 3);
            menuStrip3.Name = "menuStrip3";
            menuStrip3.Size = new Size(672, 28);
            menuStrip3.TabIndex = 1;
            menuStrip3.Text = "menuStrip3";
            // 
            // показатьГрафикToolStripMenuItem1
            // 
            показатьГрафикToolStripMenuItem1.Name = "показатьГрафикToolStripMenuItem1";
            показатьГрафикToolStripMenuItem1.Size = new Size(138, 24);
            показатьГрафикToolStripMenuItem1.Text = "показать график";
            показатьГрафикToolStripMenuItem1.Click += показатьГрафикToolStripMenuItem1_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(Plotfotkl);
            tabPage5.Controls.Add(menuStrip5);
            tabPage5.Location = new Point(4, 29);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(678, 344);
            tabPage5.TabIndex = 5;
            tabPage5.Text = "Отклонение частоты";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // menuStrip5
            // 
            menuStrip5.ImageScalingSize = new Size(20, 20);
            menuStrip5.Items.AddRange(new ToolStripItem[] { показатьГрафикToolStripMenuItem });
            menuStrip5.Location = new Point(3, 3);
            menuStrip5.Name = "menuStrip5";
            menuStrip5.Size = new Size(672, 28);
            menuStrip5.TabIndex = 0;
            menuStrip5.Text = "menuStrip5";
            // 
            // показатьГрафикToolStripMenuItem
            // 
            показатьГрафикToolStripMenuItem.Name = "показатьГрафикToolStripMenuItem";
            показатьГрафикToolStripMenuItem.Size = new Size(140, 24);
            показатьГрафикToolStripMenuItem.Text = "Показать график";
            показатьГрафикToolStripMenuItem.Click += показатьГрафикToolStripMenuItem_Click;
            // 
            // Plotfotkl
            // 
            Plotfotkl.DisplayScale = 1.25F;
            Plotfotkl.Dock = DockStyle.Fill;
            Plotfotkl.Location = new Point(3, 31);
            Plotfotkl.Name = "Plotfotkl";
            Plotfotkl.Size = new Size(672, 310);
            Plotfotkl.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 377);
            Controls.Add(harmPlot1212);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            menuStrip4.ResumeLayout(false);
            menuStrip4.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            menuStrip2.ResumeLayout(false);
            menuStrip2.PerformLayout();
            Осциллограммы.ResumeLayout(false);
            Осциллограммы.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            harmPlot1212.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            menuStrip3.ResumeLayout(false);
            menuStrip3.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            menuStrip5.ResumeLayout(false);
            menuStrip5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabPage tabPage4;
        private ScottPlot.WinForms.FormsPlot DeistvPlot;
        private MenuStrip menuStrip4;
        private ToolStripMenuItem действующиеЗначенияToolStripMenuItem;
        private TabPage tabPage3;
        private ScottPlot.WinForms.FormsPlot harmPlot;
        private TabPage tabPage2;
        private ScottPlot.WinForms.FormsPlot FreqPlot;
        private MenuStrip menuStrip2;
        private ToolStripMenuItem частотаToolStripMenuItem;
        private ToolStripMenuItem результатыToolStripMenuItem;
        private TabPage Осциллограммы;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel InfoToolStripStatusLabel;
        private ScottPlot.WinForms.FormsPlot OscPlot;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem осциллограммаToolStripMenuItem;
        private TabControl harmPlot1212;
        private TabPage tabPage1;
        private ScottPlot.WinForms.FormsPlot PlotUotkl;
        private MenuStrip menuStrip3;
        private ToolStripMenuItem показатьГрафикToolStripMenuItem1;
        private TabPage tabPage5;
        private ScottPlot.WinForms.FormsPlot Plotfotkl;
        private MenuStrip menuStrip5;
        private ToolStripMenuItem показатьГрафикToolStripMenuItem;
    }
}

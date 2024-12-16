namespace WinApp
{
    partial class FormHMI
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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewItem listViewItem1 = new ListViewItem("f1");
            ListViewItem listViewItem2 = new ListViewItem("f2");
            ListViewItem listViewItem3 = new ListViewItem("f3");
            ListViewItem listViewItem4 = new ListViewItem("f4");
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            btnSetup = new Button();
            label2 = new Label();
            label1 = new Label();
            pgConfig = new PropertyGrid();
            pgDataGen = new PropertyGrid();
            btnGetData = new Button();
            btnStop = new Button();
            btnStart = new Button();
            lvFrames = new ListView();
            chID = new ColumnHeader();
            chTimestamp = new ColumnHeader();
            chState = new ColumnHeader();
            chData = new ColumnHeader();
            splitContainer1 = new SplitContainer();
            lbLog = new ListBox();
            timerRefresh = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 596F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(lvFrames, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 436F));
            tableLayoutPanel1.Size = new Size(1132, 485);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSetup);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(pgConfig);
            panel1.Controls.Add(pgDataGen);
            panel1.Controls.Add(btnGetData);
            panel1.Controls.Add(btnStop);
            panel1.Controls.Add(btnStart);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(590, 479);
            panel1.TabIndex = 1;
            // 
            // btnSetup
            // 
            btnSetup.Location = new Point(9, 3);
            btnSetup.Name = "btnSetup";
            btnSetup.Size = new Size(135, 33);
            btnSetup.TabIndex = 7;
            btnSetup.Text = "Setup New Settings";
            btnSetup.UseVisualStyleBackColor = true;
            btnSetup.Click += btnSetup_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(296, 39);
            label2.Name = "label2";
            label2.Size = new Size(85, 15);
            label2.TabIndex = 6;
            label2.Text = "Data Processor";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 39);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 5;
            label1.Text = "Config";
            // 
            // pgConfig
            // 
            pgConfig.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pgConfig.Location = new Point(9, 57);
            pgConfig.Name = "pgConfig";
            pgConfig.Size = new Size(281, 419);
            pgConfig.TabIndex = 4;
            // 
            // pgDataGen
            // 
            pgDataGen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pgDataGen.Location = new Point(300, 57);
            pgDataGen.Name = "pgDataGen";
            pgDataGen.Size = new Size(272, 419);
            pgDataGen.TabIndex = 3;
            // 
            // btnGetData
            // 
            btnGetData.Location = new Point(155, 3);
            btnGetData.Name = "btnGetData";
            btnGetData.Size = new Size(135, 33);
            btnGetData.TabIndex = 2;
            btnGetData.Text = "Get Data (Consumer)";
            btnGetData.UseVisualStyleBackColor = true;
            btnGetData.Click += btnGetData_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(437, 3);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(135, 33);
            btnStop.TabIndex = 1;
            btnStop.Text = "Stop All";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(296, 3);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(135, 33);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start Producer";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // lvFrames
            // 
            lvFrames.Columns.AddRange(new ColumnHeader[] { chID, chTimestamp, chState, chData });
            lvFrames.Dock = DockStyle.Fill;
            lvFrames.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4 });
            lvFrames.Location = new Point(599, 3);
            lvFrames.Name = "lvFrames";
            lvFrames.Size = new Size(530, 479);
            lvFrames.TabIndex = 0;
            lvFrames.UseCompatibleStateImageBehavior = false;
            lvFrames.View = View.Details;
            // 
            // chID
            // 
            chID.DisplayIndex = 1;
            chID.Text = "ID";
            // 
            // chTimestamp
            // 
            chTimestamp.DisplayIndex = 0;
            chTimestamp.Text = "Timestamp";
            // 
            // chState
            // 
            chState.DisplayIndex = 3;
            chState.Text = "State";
            // 
            // chData
            // 
            chData.DisplayIndex = 2;
            chData.Text = "Payload";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lbLog);
            splitContainer1.Size = new Size(1132, 707);
            splitContainer1.SplitterDistance = 485;
            splitContainer1.TabIndex = 1;
            // 
            // lbLog
            // 
            lbLog.Dock = DockStyle.Fill;
            lbLog.FormattingEnabled = true;
            lbLog.ItemHeight = 15;
            lbLog.Items.AddRange(new object[] { "Log Message" });
            lbLog.Location = new Point(0, 0);
            lbLog.Name = "lbLog";
            lbLog.Size = new Size(1132, 218);
            lbLog.TabIndex = 0;
            // 
            // timerRefresh
            // 
            timerRefresh.Enabled = true;
            timerRefresh.Tick += timerRefresh_Tick;
            // 
            // FormHMI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1132, 707);
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Name = "FormHMI";
            Text = "Form1";
            FormClosed += FormHMI_FormClosed;
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer1;
        private ListView lvFrames;
        private Panel panel1;
        private ListBox lbLog;
        private System.Windows.Forms.Timer timerRefresh;
        private Button btnGetData;
        private Button btnStop;
        private Button btnStart;
        private PropertyGrid pgConfig;
        private PropertyGrid pgDataGen;
        private Label label2;
        private Label label1;
        private Button btnSetup;
        private ColumnHeader chID;
        private ColumnHeader chTimestamp;
        private ColumnHeader chState;
        private ColumnHeader chData;
    }
}

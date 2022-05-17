
namespace WindowsFormsAppCamera
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictCamera = new System.Windows.Forms.PictureBox();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblDrones = new System.Windows.Forms.Label();
            this.btnSaveBmp = new System.Windows.Forms.Button();
            this.btnToggleBlankOrLiveScreen = new System.Windows.Forms.Button();
            this.btnRecalLeftLess = new System.Windows.Forms.Button();
            this.btnRecalLeftMore = new System.Windows.Forms.Button();
            this.btnRecalRightMore = new System.Windows.Forms.Button();
            this.btnRecalRightLess = new System.Windows.Forms.Button();
            this.btnAllUp = new System.Windows.Forms.Button();
            this.cmbCamera = new System.Windows.Forms.ComboBox();
            this.cmbCameraFormat = new System.Windows.Forms.ComboBox();
            this.cmbComPorts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnPressLB = new System.Windows.Forms.Button();
            this.btnPressRB = new System.Windows.Forms.Button();
            this.numTrigger = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radLBNoPress = new System.Windows.Forms.RadioButton();
            this.radLBShortPress = new System.Windows.Forms.RadioButton();
            this.radLBLongPress = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radRBNoPress = new System.Windows.Forms.RadioButton();
            this.radRBShortPress = new System.Windows.Forms.RadioButton();
            this.radRBLongPress = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnTestComPort = new System.Windows.Forms.Button();
            this.btnTestSms = new System.Windows.Forms.Button();
            this.numDroneDelay = new System.Windows.Forms.NumericUpDown();
            this.lblDroneDelay = new System.Windows.Forms.Label();
            this.pictR = new System.Windows.Forms.PictureBox();
            this.pictG = new System.Windows.Forms.PictureBox();
            this.pictB = new System.Windows.Forms.PictureBox();
            this.lblVersionInfo = new System.Windows.Forms.Label();
            this.btnResetOffsets = new System.Windows.Forms.Button();
            this.chkSmsAlerts = new System.Windows.Forms.CheckBox();
            this.tpTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.numLongDelayOffset = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.chkLaunchDelay = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTrigger)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDroneDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongDelayOffset)).BeginInit();
            this.SuspendLayout();
            // 
            // pictCamera
            // 
            this.pictCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictCamera.Location = new System.Drawing.Point(13, 116);
            this.pictCamera.Margin = new System.Windows.Forms.Padding(2);
            this.pictCamera.Name = "pictCamera";
            this.pictCamera.Size = new System.Drawing.Size(427, 313);
            this.pictCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictCamera.TabIndex = 0;
            this.pictCamera.TabStop = false;
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Location = new System.Drawing.Point(13, 489);
            this.btnCalibrate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(58, 26);
            this.btnCalibrate.TabIndex = 1;
            this.btnCalibrate.Text = "Calibrate";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(74, 489);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(58, 26);
            this.btnStart.TabIndex = 13;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(136, 489);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(58, 26);
            this.btnStop.TabIndex = 14;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblDrones
            // 
            this.lblDrones.AutoSize = true;
            this.lblDrones.Location = new System.Drawing.Point(559, 145);
            this.lblDrones.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDrones.Name = "lblDrones";
            this.lblDrones.Size = new System.Drawing.Size(0, 13);
            this.lblDrones.TabIndex = 15;
            // 
            // btnSaveBmp
            // 
            this.btnSaveBmp.Location = new System.Drawing.Point(261, 490);
            this.btnSaveBmp.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveBmp.Name = "btnSaveBmp";
            this.btnSaveBmp.Size = new System.Drawing.Size(58, 26);
            this.btnSaveBmp.TabIndex = 16;
            this.btnSaveBmp.Text = "Save BMP";
            this.btnSaveBmp.UseVisualStyleBackColor = true;
            this.btnSaveBmp.Click += new System.EventHandler(this.btnSaveBmp_Click);
            // 
            // btnToggleBlankOrLiveScreen
            // 
            this.btnToggleBlankOrLiveScreen.Location = new System.Drawing.Point(198, 490);
            this.btnToggleBlankOrLiveScreen.Margin = new System.Windows.Forms.Padding(2);
            this.btnToggleBlankOrLiveScreen.Name = "btnToggleBlankOrLiveScreen";
            this.btnToggleBlankOrLiveScreen.Size = new System.Drawing.Size(58, 26);
            this.btnToggleBlankOrLiveScreen.TabIndex = 17;
            this.btnToggleBlankOrLiveScreen.Text = "To Blank";
            this.btnToggleBlankOrLiveScreen.UseVisualStyleBackColor = true;
            this.btnToggleBlankOrLiveScreen.Click += new System.EventHandler(this.btnToggleBlankOrLiveScreen_Click);
            // 
            // btnRecalLeftLess
            // 
            this.btnRecalLeftLess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalLeftLess.Location = new System.Drawing.Point(65, 16);
            this.btnRecalLeftLess.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecalLeftLess.Name = "btnRecalLeftLess";
            this.btnRecalLeftLess.Size = new System.Drawing.Size(45, 27);
            this.btnRecalLeftLess.TabIndex = 18;
            this.btnRecalLeftLess.Text = "Less";
            this.btnRecalLeftLess.UseVisualStyleBackColor = true;
            this.btnRecalLeftLess.Click += new System.EventHandler(this.btnRecalLeftLess_Click);
            // 
            // btnRecalLeftMore
            // 
            this.btnRecalLeftMore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalLeftMore.Location = new System.Drawing.Point(116, 16);
            this.btnRecalLeftMore.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecalLeftMore.Name = "btnRecalLeftMore";
            this.btnRecalLeftMore.Size = new System.Drawing.Size(45, 27);
            this.btnRecalLeftMore.TabIndex = 19;
            this.btnRecalLeftMore.Text = "More";
            this.btnRecalLeftMore.UseMnemonic = false;
            this.btnRecalLeftMore.UseVisualStyleBackColor = true;
            this.btnRecalLeftMore.Click += new System.EventHandler(this.btnRecalLeftMore_Click);
            // 
            // btnRecalRightMore
            // 
            this.btnRecalRightMore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalRightMore.Location = new System.Drawing.Point(113, 16);
            this.btnRecalRightMore.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecalRightMore.Name = "btnRecalRightMore";
            this.btnRecalRightMore.Size = new System.Drawing.Size(45, 27);
            this.btnRecalRightMore.TabIndex = 21;
            this.btnRecalRightMore.Text = "More";
            this.tpTooltip.SetToolTip(this.btnRecalRightMore, "Hello!");
            this.btnRecalRightMore.UseVisualStyleBackColor = true;
            this.btnRecalRightMore.Click += new System.EventHandler(this.btnRecalRightMore_Click);
            // 
            // btnRecalRightLess
            // 
            this.btnRecalRightLess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalRightLess.Location = new System.Drawing.Point(64, 16);
            this.btnRecalRightLess.Margin = new System.Windows.Forms.Padding(2);
            this.btnRecalRightLess.Name = "btnRecalRightLess";
            this.btnRecalRightLess.Size = new System.Drawing.Size(45, 27);
            this.btnRecalRightLess.TabIndex = 20;
            this.btnRecalRightLess.Text = "Less";
            this.btnRecalRightLess.UseVisualStyleBackColor = true;
            this.btnRecalRightLess.Click += new System.EventHandler(this.btnRecalRightLess_Click);
            // 
            // btnAllUp
            // 
            this.btnAllUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAllUp.Location = new System.Drawing.Point(188, 541);
            this.btnAllUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnAllUp.Name = "btnAllUp";
            this.btnAllUp.Size = new System.Drawing.Size(40, 20);
            this.btnAllUp.TabIndex = 24;
            this.btnAllUp.Text = "Up";
            this.btnAllUp.UseVisualStyleBackColor = true;
            this.btnAllUp.Click += new System.EventHandler(this.btnAllUp_Click);
            // 
            // cmbCamera
            // 
            this.cmbCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCamera.FormattingEnabled = true;
            this.cmbCamera.Location = new System.Drawing.Point(92, 8);
            this.cmbCamera.Margin = new System.Windows.Forms.Padding(2);
            this.cmbCamera.Name = "cmbCamera";
            this.cmbCamera.Size = new System.Drawing.Size(349, 21);
            this.cmbCamera.TabIndex = 25;
            this.cmbCamera.SelectedIndexChanged += new System.EventHandler(this.cmbCamera_SelectedIndexChanged);
            // 
            // cmbCameraFormat
            // 
            this.cmbCameraFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCameraFormat.FormattingEnabled = true;
            this.cmbCameraFormat.Location = new System.Drawing.Point(92, 33);
            this.cmbCameraFormat.Margin = new System.Windows.Forms.Padding(2);
            this.cmbCameraFormat.Name = "cmbCameraFormat";
            this.cmbCameraFormat.Size = new System.Drawing.Size(349, 21);
            this.cmbCameraFormat.TabIndex = 26;
            this.cmbCameraFormat.SelectedIndexChanged += new System.EventHandler(this.cmbCameraFormat_SelectedIndexChanged);
            // 
            // cmbComPorts
            // 
            this.cmbComPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbComPorts.FormattingEnabled = true;
            this.cmbComPorts.Location = new System.Drawing.Point(92, 58);
            this.cmbComPorts.Margin = new System.Windows.Forms.Padding(2);
            this.cmbComPorts.Name = "cmbComPorts";
            this.cmbComPorts.Size = new System.Drawing.Size(102, 21);
            this.cmbComPorts.TabIndex = 27;
            this.cmbComPorts.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Camera:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 38);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Video Format:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 64);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 30;
            this.label6.Text = "COM Port:";
            // 
            // btnPressLB
            // 
            this.btnPressLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnPressLB.Location = new System.Drawing.Point(15, 16);
            this.btnPressLB.Margin = new System.Windows.Forms.Padding(2);
            this.btnPressLB.Name = "btnPressLB";
            this.btnPressLB.Size = new System.Drawing.Size(45, 27);
            this.btnPressLB.TabIndex = 32;
            this.btnPressLB.Text = "Press";
            this.btnPressLB.UseVisualStyleBackColor = true;
            this.btnPressLB.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // btnPressRB
            // 
            this.btnPressRB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnPressRB.Location = new System.Drawing.Point(15, 16);
            this.btnPressRB.Margin = new System.Windows.Forms.Padding(2);
            this.btnPressRB.Name = "btnPressRB";
            this.btnPressRB.Size = new System.Drawing.Size(45, 27);
            this.btnPressRB.TabIndex = 33;
            this.btnPressRB.Text = "Press";
            this.btnPressRB.UseVisualStyleBackColor = true;
            this.btnPressRB.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // numTrigger
            // 
            this.numTrigger.Location = new System.Drawing.Point(265, 88);
            this.numTrigger.Margin = new System.Windows.Forms.Padding(2);
            this.numTrigger.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numTrigger.Name = "numTrigger";
            this.numTrigger.Size = new System.Drawing.Size(48, 20);
            this.numTrigger.TabIndex = 39;
            this.numTrigger.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numTrigger.ValueChanged += new System.EventHandler(this.numTrigger_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(209, 90);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 40;
            this.label8.Text = "Trigger %:";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radLBNoPress);
            this.groupBox1.Controls.Add(this.radLBShortPress);
            this.groupBox1.Controls.Add(this.radLBLongPress);
            this.groupBox1.Controls.Add(this.btnPressLB);
            this.groupBox1.Controls.Add(this.btnRecalLeftLess);
            this.groupBox1.Controls.Add(this.btnRecalLeftMore);
            this.groupBox1.Location = new System.Drawing.Point(13, 530);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(173, 84);
            this.groupBox1.TabIndex = 41;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "LB";
            // 
            // radLBNoPress
            // 
            this.radLBNoPress.AutoSize = true;
            this.radLBNoPress.Location = new System.Drawing.Point(119, 57);
            this.radLBNoPress.Margin = new System.Windows.Forms.Padding(2);
            this.radLBNoPress.Name = "radLBNoPress";
            this.radLBNoPress.Size = new System.Drawing.Size(51, 17);
            this.radLBNoPress.TabIndex = 59;
            this.radLBNoPress.TabStop = true;
            this.radLBNoPress.Text = "None";
            this.radLBNoPress.UseVisualStyleBackColor = true;
            this.radLBNoPress.CheckedChanged += new System.EventHandler(this.radLBNoPress_CheckedChanged);
            // 
            // radLBShortPress
            // 
            this.radLBShortPress.AutoSize = true;
            this.radLBShortPress.Location = new System.Drawing.Point(65, 57);
            this.radLBShortPress.Margin = new System.Windows.Forms.Padding(2);
            this.radLBShortPress.Name = "radLBShortPress";
            this.radLBShortPress.Size = new System.Drawing.Size(50, 17);
            this.radLBShortPress.TabIndex = 58;
            this.radLBShortPress.TabStop = true;
            this.radLBShortPress.Text = "Short";
            this.radLBShortPress.UseVisualStyleBackColor = true;
            this.radLBShortPress.CheckedChanged += new System.EventHandler(this.radLBShortPress_CheckedChanged);
            // 
            // radLBLongPress
            // 
            this.radLBLongPress.AutoSize = true;
            this.radLBLongPress.Location = new System.Drawing.Point(13, 57);
            this.radLBLongPress.Margin = new System.Windows.Forms.Padding(2);
            this.radLBLongPress.Name = "radLBLongPress";
            this.radLBLongPress.Size = new System.Drawing.Size(49, 17);
            this.radLBLongPress.TabIndex = 57;
            this.radLBLongPress.TabStop = true;
            this.radLBLongPress.Text = "Long";
            this.radLBLongPress.UseVisualStyleBackColor = true;
            this.radLBLongPress.CheckedChanged += new System.EventHandler(this.radLBLongPress_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radRBNoPress);
            this.groupBox2.Controls.Add(this.radRBShortPress);
            this.groupBox2.Controls.Add(this.radRBLongPress);
            this.groupBox2.Controls.Add(this.btnPressRB);
            this.groupBox2.Controls.Add(this.btnRecalRightLess);
            this.groupBox2.Controls.Add(this.btnRecalRightMore);
            this.groupBox2.Location = new System.Drawing.Point(270, 530);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(173, 84);
            this.groupBox2.TabIndex = 42;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "RB";
            // 
            // radRBNoPress
            // 
            this.radRBNoPress.AutoSize = true;
            this.radRBNoPress.Location = new System.Drawing.Point(117, 57);
            this.radRBNoPress.Margin = new System.Windows.Forms.Padding(2);
            this.radRBNoPress.Name = "radRBNoPress";
            this.radRBNoPress.Size = new System.Drawing.Size(51, 17);
            this.radRBNoPress.TabIndex = 61;
            this.radRBNoPress.TabStop = true;
            this.radRBNoPress.Text = "None";
            this.radRBNoPress.UseVisualStyleBackColor = true;
            this.radRBNoPress.CheckedChanged += new System.EventHandler(this.radRBNoPress_CheckedChanged);
            // 
            // radRBShortPress
            // 
            this.radRBShortPress.AutoSize = true;
            this.radRBShortPress.Location = new System.Drawing.Point(64, 57);
            this.radRBShortPress.Margin = new System.Windows.Forms.Padding(2);
            this.radRBShortPress.Name = "radRBShortPress";
            this.radRBShortPress.Size = new System.Drawing.Size(50, 17);
            this.radRBShortPress.TabIndex = 60;
            this.radRBShortPress.TabStop = true;
            this.radRBShortPress.Text = "Short";
            this.radRBShortPress.UseVisualStyleBackColor = true;
            this.radRBShortPress.CheckedChanged += new System.EventHandler(this.rabRBShortPress_CheckedChanged);
            // 
            // radRBLongPress
            // 
            this.radRBLongPress.AutoSize = true;
            this.radRBLongPress.Location = new System.Drawing.Point(13, 57);
            this.radRBLongPress.Margin = new System.Windows.Forms.Padding(2);
            this.radRBLongPress.Name = "radRBLongPress";
            this.radRBLongPress.Size = new System.Drawing.Size(49, 17);
            this.radRBLongPress.TabIndex = 59;
            this.radRBLongPress.TabStop = true;
            this.radRBLongPress.Text = "Long";
            this.radRBLongPress.UseVisualStyleBackColor = true;
            this.radRBLongPress.CheckedChanged += new System.EventHandler(this.radRBLongPress_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 92);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 43;
            this.label4.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(92, 88);
            this.txtName.Margin = new System.Windows.Forms.Padding(2);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(115, 20);
            this.txtName.TabIndex = 44;
            this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
            // 
            // btnTestComPort
            // 
            this.btnTestComPort.Enabled = false;
            this.btnTestComPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestComPort.Location = new System.Drawing.Point(199, 57);
            this.btnTestComPort.Margin = new System.Windows.Forms.Padding(2);
            this.btnTestComPort.Name = "btnTestComPort";
            this.btnTestComPort.Size = new System.Drawing.Size(45, 20);
            this.btnTestComPort.TabIndex = 31;
            this.btnTestComPort.Text = "Test";
            this.btnTestComPort.UseVisualStyleBackColor = true;
            this.btnTestComPort.Click += new System.EventHandler(this.btnTestComPort_Click);
            // 
            // btnTestSms
            // 
            this.btnTestSms.Enabled = false;
            this.btnTestSms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestSms.Location = new System.Drawing.Point(394, 57);
            this.btnTestSms.Margin = new System.Windows.Forms.Padding(2);
            this.btnTestSms.Name = "btnTestSms";
            this.btnTestSms.Size = new System.Drawing.Size(45, 20);
            this.btnTestSms.TabIndex = 47;
            this.btnTestSms.Text = "Test";
            this.btnTestSms.UseVisualStyleBackColor = true;
            this.btnTestSms.Click += new System.EventHandler(this.btnTestSms_Click);
            // 
            // numDroneDelay
            // 
            this.numDroneDelay.Location = new System.Drawing.Point(390, 89);
            this.numDroneDelay.Margin = new System.Windows.Forms.Padding(2);
            this.numDroneDelay.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numDroneDelay.Minimum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numDroneDelay.Name = "numDroneDelay";
            this.numDroneDelay.Size = new System.Drawing.Size(48, 20);
            this.numDroneDelay.TabIndex = 49;
            this.numDroneDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDroneDelay.ValueChanged += new System.EventHandler(this.numDroneDelay_ValueChanged);
            // 
            // lblDroneDelay
            // 
            this.lblDroneDelay.AutoSize = true;
            this.lblDroneDelay.Location = new System.Drawing.Point(320, 90);
            this.lblDroneDelay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDroneDelay.Name = "lblDroneDelay";
            this.lblDroneDelay.Size = new System.Drawing.Size(69, 13);
            this.lblDroneDelay.TabIndex = 50;
            this.lblDroneDelay.Text = "Drone Delay:";
            // 
            // pictR
            // 
            this.pictR.Location = new System.Drawing.Point(13, 432);
            this.pictR.Margin = new System.Windows.Forms.Padding(2);
            this.pictR.Name = "pictR";
            this.pictR.Size = new System.Drawing.Size(140, 42);
            this.pictR.TabIndex = 52;
            this.pictR.TabStop = false;
            // 
            // pictG
            // 
            this.pictG.Location = new System.Drawing.Point(156, 432);
            this.pictG.Margin = new System.Windows.Forms.Padding(2);
            this.pictG.Name = "pictG";
            this.pictG.Size = new System.Drawing.Size(140, 42);
            this.pictG.TabIndex = 53;
            this.pictG.TabStop = false;
            // 
            // pictB
            // 
            this.pictB.Location = new System.Drawing.Point(299, 432);
            this.pictB.Margin = new System.Windows.Forms.Padding(2);
            this.pictB.Name = "pictB";
            this.pictB.Size = new System.Drawing.Size(140, 42);
            this.pictB.TabIndex = 54;
            this.pictB.TabStop = false;
            // 
            // lblVersionInfo
            // 
            this.lblVersionInfo.AutoSize = true;
            this.lblVersionInfo.Location = new System.Drawing.Point(10, 630);
            this.lblVersionInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblVersionInfo.Name = "lblVersionInfo";
            this.lblVersionInfo.Size = new System.Drawing.Size(58, 13);
            this.lblVersionInfo.TabIndex = 55;
            this.lblVersionInfo.Text = "versioninfo";
            this.lblVersionInfo.Click += new System.EventHandler(this.lblVersionInfo_Click);
            // 
            // btnResetOffsets
            // 
            this.btnResetOffsets.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetOffsets.Location = new System.Drawing.Point(226, 541);
            this.btnResetOffsets.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetOffsets.Name = "btnResetOffsets";
            this.btnResetOffsets.Size = new System.Drawing.Size(44, 20);
            this.btnResetOffsets.TabIndex = 56;
            this.btnResetOffsets.Text = "Reset";
            this.btnResetOffsets.UseVisualStyleBackColor = true;
            this.btnResetOffsets.Click += new System.EventHandler(this.btnResetOffsets_Click);
            // 
            // chkSmsAlerts
            // 
            this.chkSmsAlerts.Location = new System.Drawing.Point(306, 58);
            this.chkSmsAlerts.Margin = new System.Windows.Forms.Padding(2);
            this.chkSmsAlerts.Name = "chkSmsAlerts";
            this.chkSmsAlerts.Size = new System.Drawing.Size(81, 20);
            this.chkSmsAlerts.TabIndex = 57;
            this.chkSmsAlerts.Text = "SMS alerts";
            this.chkSmsAlerts.UseVisualStyleBackColor = true;
            // 
            // tpTooltip
            // 
            this.tpTooltip.ShowAlways = true;
            // 
            // numLongDelayOffset
            // 
            this.numLongDelayOffset.Location = new System.Drawing.Point(193, 593);
            this.numLongDelayOffset.Margin = new System.Windows.Forms.Padding(2);
            this.numLongDelayOffset.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numLongDelayOffset.Name = "numLongDelayOffset";
            this.numLongDelayOffset.Size = new System.Drawing.Size(69, 20);
            this.numLongDelayOffset.TabIndex = 58;
            this.numLongDelayOffset.ValueChanged += new System.EventHandler(this.numLongDelayOffset_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 575);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 59;
            this.label2.Text = "Long offset";
            // 
            // chkLaunchDelay
            // 
            this.chkLaunchDelay.AutoSize = true;
            this.chkLaunchDelay.Location = new System.Drawing.Point(334, 493);
            this.chkLaunchDelay.Name = "chkLaunchDelay";
            this.chkLaunchDelay.Size = new System.Drawing.Size(92, 17);
            this.chkLaunchDelay.TabIndex = 60;
            this.chkLaunchDelay.Text = "Launch Delay";
            this.chkLaunchDelay.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 654);
            this.Controls.Add(this.chkLaunchDelay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numLongDelayOffset);
            this.Controls.Add(this.chkSmsAlerts);
            this.Controls.Add(this.btnResetOffsets);
            this.Controls.Add(this.lblVersionInfo);
            this.Controls.Add(this.pictB);
            this.Controls.Add(this.pictG);
            this.Controls.Add(this.pictR);
            this.Controls.Add(this.lblDroneDelay);
            this.Controls.Add(this.numDroneDelay);
            this.Controls.Add(this.btnTestSms);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.numTrigger);
            this.Controls.Add(this.btnTestComPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbComPorts);
            this.Controls.Add(this.cmbCameraFormat);
            this.Controls.Add(this.cmbCamera);
            this.Controls.Add(this.btnAllUp);
            this.Controls.Add(this.btnToggleBlankOrLiveScreen);
            this.Controls.Add(this.btnSaveBmp);
            this.Controls.Add(this.lblDrones);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnCalibrate);
            this.Controls.Add(this.pictCamera);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "DivGrind Gen 2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictCamera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTrigger)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDroneDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongDelayOffset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictCamera;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblDrones;
        private System.Windows.Forms.Button btnSaveBmp;
        private System.Windows.Forms.Button btnToggleBlankOrLiveScreen;
        private System.Windows.Forms.Button btnRecalLeftLess;
        private System.Windows.Forms.Button btnRecalLeftMore;
        private System.Windows.Forms.Button btnRecalRightMore;
        private System.Windows.Forms.Button btnRecalRightLess;
        private System.Windows.Forms.Button btnAllUp;
        private System.Windows.Forms.ComboBox cmbCamera;
        private System.Windows.Forms.ComboBox cmbCameraFormat;
        private System.Windows.Forms.ComboBox cmbComPorts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnPressLB;
        private System.Windows.Forms.Button btnPressRB;
        private System.Windows.Forms.NumericUpDown numTrigger;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnTestComPort;
        private System.Windows.Forms.Button btnTestSms;
        private System.Windows.Forms.NumericUpDown numDroneDelay;
        private System.Windows.Forms.Label lblDroneDelay;
        private System.Windows.Forms.PictureBox pictR;
        private System.Windows.Forms.PictureBox pictG;
        private System.Windows.Forms.PictureBox pictB;
        private System.Windows.Forms.Label lblVersionInfo;
        private System.Windows.Forms.Button btnResetOffsets;
        private System.Windows.Forms.RadioButton radLBShortPress;
        private System.Windows.Forms.RadioButton radLBLongPress;
        private System.Windows.Forms.RadioButton radRBShortPress;
        private System.Windows.Forms.RadioButton radRBLongPress;
        private System.Windows.Forms.CheckBox chkSmsAlerts;
        private System.Windows.Forms.RadioButton radLBNoPress;
        private System.Windows.Forms.RadioButton radRBNoPress;
        private System.Windows.Forms.ToolTip tpTooltip;
        private System.Windows.Forms.NumericUpDown numLongDelayOffset;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkLaunchDelay;
    }
}


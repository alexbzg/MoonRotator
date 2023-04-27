﻿namespace EncRotator
{
    partial class fMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fMain));
            this.ofdMap = new System.Windows.Forms.OpenFileDialog();
            this.miConnections = new System.Windows.Forms.ToolStripMenuItem();
            this.miHConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.miVConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.miCamURL = new System.Windows.Forms.ToolStripMenuItem();
            this.miMaps = new System.Windows.Forms.ToolStripMenuItem();
            this.miMapAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.miMapRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.miSetValues = new System.Windows.Forms.ToolStripMenuItem();
            this.miSetAzimuth = new System.Windows.Forms.ToolStripMenuItem();
            this.miSetHorizon = new System.Windows.Forms.ToolStripMenuItem();
            this.miSetZenith = new System.Windows.Forms.ToolStripMenuItem();
            this.miSetCoordinates = new System.Windows.Forms.ToolStripMenuItem();
            this.miModuleSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.pMap = new System.Windows.Forms.PictureBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.slMvt = new System.Windows.Forms.ToolStripLabel();
            this.ddSettings = new System.Windows.Forms.ToolStripDropDownButton();
            this.miConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.miDisconnect = new System.Windows.Forms.ToolStripMenuItem();
            this.miReset = new System.Windows.Forms.ToolStripMenuItem();
            this.miResetAzimuth = new System.Windows.Forms.ToolStripMenuItem();
            this.miResetElevation = new System.Windows.Forms.ToolStripMenuItem();
            this.miFollow = new System.Windows.Forms.ToolStripMenuItem();
            this.bStop = new System.Windows.Forms.Button();
            this.cbCam = new System.Windows.Forms.CheckBox();
            this.rotatorPanelV = new EncRotator.RotatorPanel();
            this.rotatorPanelH = new EncRotator.RotatorPanel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pMap)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // miConnections
            // 
            this.miConnections.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHConnection,
            this.miVConnection,
            this.miCamURL});
            this.miConnections.Name = "miConnections";
            this.miConnections.Size = new System.Drawing.Size(291, 34);
            this.miConnections.Text = "Соединения";
            // 
            // miHConnection
            // 
            this.miHConnection.Name = "miHConnection";
            this.miHConnection.Size = new System.Drawing.Size(179, 34);
            this.miHConnection.Text = "Азимут";
            this.miHConnection.Click += new System.EventHandler(this.miEditConnection_Click);
            // 
            // miVConnection
            // 
            this.miVConnection.Name = "miVConnection";
            this.miVConnection.Size = new System.Drawing.Size(179, 34);
            this.miVConnection.Text = "Элевация";
            this.miVConnection.Click += new System.EventHandler(this.miEditConnection_Click);
            // 
            // miCamURL
            // 
            this.miCamURL.Name = "miCamURL";
            this.miCamURL.Size = new System.Drawing.Size(179, 34);
            this.miCamURL.Text = "Камера";
            this.miCamURL.Click += new System.EventHandler(this.miCamURL_Click);
            // 
            // miMaps
            // 
            this.miMaps.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miMapAdd,
            this.miMapRemove});
            this.miMaps.Name = "miMaps";
            this.miMaps.Size = new System.Drawing.Size(291, 34);
            this.miMaps.Text = "Карты";
            // 
            // miMapAdd
            // 
            this.miMapAdd.Name = "miMapAdd";
            this.miMapAdd.Size = new System.Drawing.Size(179, 34);
            this.miMapAdd.Text = "Добавить";
            this.miMapAdd.Click += new System.EventHandler(this.miMapAdd_Click);
            // 
            // miMapRemove
            // 
            this.miMapRemove.Name = "miMapRemove";
            this.miMapRemove.Size = new System.Drawing.Size(179, 34);
            this.miMapRemove.Text = "Удалить";
            this.miMapRemove.Click += new System.EventHandler(this.miMapRemove_Click);
            // 
            // miSetValues
            // 
            this.miSetValues.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSetAzimuth,
            this.miSetHorizon,
            this.miSetZenith,
            this.miSetCoordinates});
            this.miSetValues.Name = "miSetValues";
            this.miSetValues.Size = new System.Drawing.Size(291, 34);
            this.miSetValues.Text = "Установить значения";
            // 
            // miSetAzimuth
            // 
            this.miSetAzimuth.Name = "miSetAzimuth";
            this.miSetAzimuth.Size = new System.Drawing.Size(204, 34);
            this.miSetAzimuth.Text = "Азимут";
            this.miSetAzimuth.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetHorizon
            // 
            this.miSetHorizon.Name = "miSetHorizon";
            this.miSetHorizon.Size = new System.Drawing.Size(204, 34);
            this.miSetHorizon.Text = "Горизонт";
            this.miSetHorizon.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetZenith
            // 
            this.miSetZenith.Name = "miSetZenith";
            this.miSetZenith.Size = new System.Drawing.Size(204, 34);
            this.miSetZenith.Text = "Зенит";
            this.miSetZenith.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetCoordinates
            // 
            this.miSetCoordinates.Name = "miSetCoordinates";
            this.miSetCoordinates.Size = new System.Drawing.Size(204, 34);
            this.miSetCoordinates.Text = "Координаты";
            this.miSetCoordinates.Click += new System.EventHandler(this.miSetCoordinates_Click);
            // 
            // miModuleSettings
            // 
            this.miModuleSettings.Name = "miModuleSettings";
            this.miModuleSettings.Size = new System.Drawing.Size(291, 34);
            this.miModuleSettings.Text = "Настройки модуля";
            this.miModuleSettings.Click += new System.EventHandler(this.miModuleSettings_Click);
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // pMap
            // 
            this.pMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pMap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pMap.Location = new System.Drawing.Point(0, 29);
            this.pMap.Name = "pMap";
            this.pMap.Size = new System.Drawing.Size(385, 324);
            this.pMap.TabIndex = 16;
            this.pMap.TabStop = false;
            this.pMap.Paint += new System.Windows.Forms.PaintEventHandler(this.pMap_Paint);
            this.pMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pMap_MouseClick);
            this.pMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pMap_MouseMove);
            this.pMap.Resize += new System.EventHandler(this.pMap_Resize);
            // 
            // toolStrip
            // 
            this.toolStrip.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slMvt,
            this.ddSettings});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(385, 26);
            this.toolStrip.TabIndex = 17;
            this.toolStrip.Text = "toolStrip1";
            // 
            // slMvt
            // 
            this.slMvt.AutoSize = false;
            this.slMvt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.slMvt.Image = ((System.Drawing.Image)(resources.GetObject("slMvt.Image")));
            this.slMvt.ImageTransparentColor = System.Drawing.Color.White;
            this.slMvt.IsLink = true;
            this.slMvt.Name = "slMvt";
            this.slMvt.Size = new System.Drawing.Size(16, 23);
            // 
            // ddSettings
            // 
            this.ddSettings.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ddSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ddSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miConnect,
            this.miDisconnect,
            this.miConnections,
            this.miMaps,
            this.miSetValues,
            this.miModuleSettings,
            this.miReset,
            this.miFollow});
            this.ddSettings.ForeColor = System.Drawing.Color.Transparent;
            this.ddSettings.Image = ((System.Drawing.Image)(resources.GetObject("ddSettings.Image")));
            this.ddSettings.ImageTransparentColor = System.Drawing.Color.Black;
            this.ddSettings.Name = "ddSettings";
            this.ddSettings.Size = new System.Drawing.Size(29, 23);
            this.ddSettings.Text = "Настройки";
            // 
            // miConnect
            // 
            this.miConnect.Name = "miConnect";
            this.miConnect.Size = new System.Drawing.Size(291, 34);
            this.miConnect.Text = "Подключиться";
            this.miConnect.Click += new System.EventHandler(this.miConnect_Click);
            // 
            // miDisconnect
            // 
            this.miDisconnect.Name = "miDisconnect";
            this.miDisconnect.Size = new System.Drawing.Size(291, 34);
            this.miDisconnect.Text = "Отключиться";
            this.miDisconnect.Visible = false;
            this.miDisconnect.Click += new System.EventHandler(this.miDisconnect_Click);
            // 
            // miReset
            // 
            this.miReset.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miResetAzimuth,
            this.miResetElevation});
            this.miReset.Name = "miReset";
            this.miReset.Size = new System.Drawing.Size(291, 34);
            this.miReset.Text = "Перезапуск";
            // 
            // miResetAzimuth
            // 
            this.miResetAzimuth.Name = "miResetAzimuth";
            this.miResetAzimuth.Size = new System.Drawing.Size(179, 34);
            this.miResetAzimuth.Text = "Азимут";
            this.miResetAzimuth.Click += new System.EventHandler(this.miReset_Click);
            // 
            // miResetElevation
            // 
            this.miResetElevation.Name = "miResetElevation";
            this.miResetElevation.Size = new System.Drawing.Size(179, 34);
            this.miResetElevation.Text = "Элевация";
            this.miResetElevation.Click += new System.EventHandler(this.miReset_Click);
            // 
            // miFollow
            // 
            this.miFollow.CheckOnClick = true;
            this.miFollow.Name = "miFollow";
            this.miFollow.Size = new System.Drawing.Size(291, 34);
            this.miFollow.Text = "Следовать за Луной";
            this.miFollow.Click += new System.EventHandler(this.miFollow_Click);
            // 
            // bStop
            // 
            this.bStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bStop.Image = global::EncRotator.Properties.Resources.stop;
            this.bStop.Location = new System.Drawing.Point(324, 480);
            this.bStop.Name = "bStop";
            this.bStop.Size = new System.Drawing.Size(60, 60);
            this.bStop.TabIndex = 22;
            this.bStop.UseVisualStyleBackColor = true;
            this.bStop.Click += new System.EventHandler(this.bStop_Click);
            // 
            // cbCam
            // 
            this.cbCam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCam.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbCam.Image = global::EncRotator.Properties.Resources.moon;
            this.cbCam.Location = new System.Drawing.Point(0, 480);
            this.cbCam.Name = "cbCam";
            this.cbCam.Size = new System.Drawing.Size(60, 60);
            this.cbCam.TabIndex = 25;
            this.cbCam.UseVisualStyleBackColor = true;
            this.cbCam.CheckedChanged += new System.EventHandler(this.cbCam_CheckedChanged);
            // 
            // rotatorPanelV
            // 
            this.rotatorPanelV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rotatorPanelV.Location = new System.Drawing.Point(194, 371);
            this.rotatorPanelV.Name = "rotatorPanelV";
            this.rotatorPanelV.Size = new System.Drawing.Size(129, 175);
            this.rotatorPanelV.TabIndex = 24;
            // 
            // rotatorPanelH
            // 
            this.rotatorPanelH.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rotatorPanelH.Location = new System.Drawing.Point(59, 371);
            this.rotatorPanelH.Name = "rotatorPanelH";
            this.rotatorPanelH.Size = new System.Drawing.Size(129, 175);
            this.rotatorPanelH.TabIndex = 23;
            // 
            // fMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 550);
            this.Controls.Add(this.cbCam);
            this.Controls.Add(this.rotatorPanelV);
            this.Controls.Add(this.rotatorPanelH);
            this.Controls.Add(this.bStop);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.pMap);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "fMain";
            this.Text = "MoonRotator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fMain_FormClosing);
            this.Load += new System.EventHandler(this.fMain_Load);
            this.ResizeEnd += new System.EventHandler(this.fMain_ResizeEnd);
            this.LocationChanged += new System.EventHandler(this.fMain_ResizeEnd);
            this.Resize += new System.EventHandler(this.fMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pMap)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog ofdMap;
        private System.Windows.Forms.ToolStripMenuItem miModuleSettings;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripMenuItem miConnections;
        private System.Windows.Forms.ToolStripMenuItem miSetValues;
        private System.Windows.Forms.PictureBox pMap;
        private System.Windows.Forms.ToolStripMenuItem miMaps;
        private System.Windows.Forms.ToolStripMenuItem miMapAdd;
        private System.Windows.Forms.ToolStripMenuItem miMapRemove;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton ddSettings;
        private System.Windows.Forms.ToolStripLabel slMvt;
        private System.Windows.Forms.ToolStripMenuItem miHConnection;
        private System.Windows.Forms.ToolStripMenuItem miVConnection;
        private System.Windows.Forms.ToolStripMenuItem miConnect;
        private System.Windows.Forms.Button bStop;
        private System.Windows.Forms.ToolStripMenuItem miSetAzimuth;
        private System.Windows.Forms.ToolStripMenuItem miSetHorizon;
        private System.Windows.Forms.ToolStripMenuItem miSetZenith;
        private RotatorPanel rotatorPanelH;
        private RotatorPanel rotatorPanelV;
        private System.Windows.Forms.ToolStripMenuItem miDisconnect;
        private System.Windows.Forms.ToolStripMenuItem miCamURL;
        private System.Windows.Forms.CheckBox cbCam;
        private System.Windows.Forms.ToolStripMenuItem miReset;
        private System.Windows.Forms.ToolStripMenuItem miResetAzimuth;
        private System.Windows.Forms.ToolStripMenuItem miResetElevation;
        private System.Windows.Forms.ToolStripMenuItem miSetCoordinates;
        private System.Windows.Forms.ToolStripMenuItem miFollow;
        private System.Windows.Forms.ToolTip toolTip;
    }
}


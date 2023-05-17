namespace EncRotator
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
            this.miConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.miDisconnect = new System.Windows.Forms.ToolStripMenuItem();
            this.miReset = new System.Windows.Forms.ToolStripMenuItem();
            this.miResetAzimuth = new System.Windows.Forms.ToolStripMenuItem();
            this.miResetElevation = new System.Windows.Forms.ToolStripMenuItem();
            this.miCam = new System.Windows.Forms.ToolStripMenuItem();
            this.bStop = new System.Windows.Forms.Button();
            this.cbFollow = new System.Windows.Forms.CheckBox();
            this.rotatorPanelV = new EncRotator.RotatorPanel();
            this.rotatorPanelH = new EncRotator.RotatorPanel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lCurrent = new System.Windows.Forms.Label();
            this.lTarget = new System.Windows.Forms.Label();
            this.lMode = new System.Windows.Forms.Label();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bMenu = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pMap)).BeginInit();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // miConnections
            // 
            this.miConnections.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHConnection,
            this.miVConnection,
            this.miCamURL});
            this.miConnections.Name = "miConnections";
            this.miConnections.Size = new System.Drawing.Size(270, 30);
            this.miConnections.Text = "Соединения";
            // 
            // miHConnection
            // 
            this.miHConnection.Name = "miHConnection";
            this.miHConnection.Size = new System.Drawing.Size(168, 30);
            this.miHConnection.Text = "Азимут";
            this.miHConnection.Click += new System.EventHandler(this.miEditConnection_Click);
            // 
            // miVConnection
            // 
            this.miVConnection.Name = "miVConnection";
            this.miVConnection.Size = new System.Drawing.Size(168, 30);
            this.miVConnection.Text = "Элевация";
            this.miVConnection.Click += new System.EventHandler(this.miEditConnection_Click);
            // 
            // miCamURL
            // 
            this.miCamURL.Name = "miCamURL";
            this.miCamURL.Size = new System.Drawing.Size(168, 30);
            this.miCamURL.Text = "Камера";
            this.miCamURL.Click += new System.EventHandler(this.miCamURL_Click);
            // 
            // miMaps
            // 
            this.miMaps.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miMapAdd,
            this.miMapRemove});
            this.miMaps.Name = "miMaps";
            this.miMaps.Size = new System.Drawing.Size(270, 30);
            this.miMaps.Text = "Карты";
            // 
            // miMapAdd
            // 
            this.miMapAdd.Name = "miMapAdd";
            this.miMapAdd.Size = new System.Drawing.Size(168, 30);
            this.miMapAdd.Text = "Добавить";
            this.miMapAdd.Click += new System.EventHandler(this.miMapAdd_Click);
            // 
            // miMapRemove
            // 
            this.miMapRemove.Name = "miMapRemove";
            this.miMapRemove.Size = new System.Drawing.Size(168, 30);
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
            this.miSetValues.Size = new System.Drawing.Size(270, 30);
            this.miSetValues.Text = "Установить значения";
            // 
            // miSetAzimuth
            // 
            this.miSetAzimuth.Name = "miSetAzimuth";
            this.miSetAzimuth.Size = new System.Drawing.Size(191, 30);
            this.miSetAzimuth.Text = "Азимут";
            this.miSetAzimuth.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetHorizon
            // 
            this.miSetHorizon.Name = "miSetHorizon";
            this.miSetHorizon.Size = new System.Drawing.Size(191, 30);
            this.miSetHorizon.Text = "Горизонт";
            this.miSetHorizon.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetZenith
            // 
            this.miSetZenith.Name = "miSetZenith";
            this.miSetZenith.Size = new System.Drawing.Size(191, 30);
            this.miSetZenith.Text = "Зенит";
            this.miSetZenith.Click += new System.EventHandler(this.miSetValueClick);
            // 
            // miSetCoordinates
            // 
            this.miSetCoordinates.Name = "miSetCoordinates";
            this.miSetCoordinates.Size = new System.Drawing.Size(191, 30);
            this.miSetCoordinates.Text = "Координаты";
            this.miSetCoordinates.Click += new System.EventHandler(this.miSetCoordinates_Click);
            // 
            // miModuleSettings
            // 
            this.miModuleSettings.Name = "miModuleSettings";
            this.miModuleSettings.Size = new System.Drawing.Size(270, 30);
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
            this.pMap.Location = new System.Drawing.Point(0, 0);
            this.pMap.Name = "pMap";
            this.pMap.Size = new System.Drawing.Size(386, 353);
            this.pMap.TabIndex = 16;
            this.pMap.TabStop = false;
            this.pMap.Paint += new System.Windows.Forms.PaintEventHandler(this.pMap_Paint);
            this.pMap.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pMap_MouseClick);
            this.pMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pMap_MouseMove);
            this.pMap.Resize += new System.EventHandler(this.pMap_Resize);
            // 
            // miConnect
            // 
            this.miConnect.Name = "miConnect";
            this.miConnect.Size = new System.Drawing.Size(270, 30);
            this.miConnect.Text = "Подключиться";
            this.miConnect.Click += new System.EventHandler(this.miConnect_Click);
            // 
            // miDisconnect
            // 
            this.miDisconnect.Name = "miDisconnect";
            this.miDisconnect.Size = new System.Drawing.Size(270, 30);
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
            this.miReset.Size = new System.Drawing.Size(270, 30);
            this.miReset.Text = "Перезапуск";
            // 
            // miResetAzimuth
            // 
            this.miResetAzimuth.Name = "miResetAzimuth";
            this.miResetAzimuth.Size = new System.Drawing.Size(168, 30);
            this.miResetAzimuth.Text = "Азимут";
            this.miResetAzimuth.Click += new System.EventHandler(this.miReset_Click);
            // 
            // miResetElevation
            // 
            this.miResetElevation.Name = "miResetElevation";
            this.miResetElevation.Size = new System.Drawing.Size(168, 30);
            this.miResetElevation.Text = "Элевация";
            this.miResetElevation.Click += new System.EventHandler(this.miReset_Click);
            // 
            // miCam
            // 
            this.miCam.CheckOnClick = true;
            this.miCam.Name = "miCam";
            this.miCam.Size = new System.Drawing.Size(270, 30);
            this.miCam.Text = "Камера";
            this.miCam.Click += new System.EventHandler(this.miCam_Click);
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
            // cbFollow
            // 
            this.cbFollow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFollow.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbFollow.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbFollow.Location = new System.Drawing.Point(0, 480);
            this.cbFollow.Name = "cbFollow";
            this.cbFollow.Size = new System.Drawing.Size(60, 60);
            this.cbFollow.TabIndex = 25;
            this.cbFollow.Text = "Az";
            this.cbFollow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbFollow.UseVisualStyleBackColor = true;
            this.cbFollow.CheckedChanged += new System.EventHandler(this.cbFollow_CheckedChanged);
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
            // lCurrent
            // 
            this.lCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lCurrent.Location = new System.Drawing.Point(-3, 371);
            this.lCurrent.Name = "lCurrent";
            this.lCurrent.Size = new System.Drawing.Size(65, 34);
            this.lCurrent.TabIndex = 26;
            this.lCurrent.Text = "Текущее положение";
            this.lCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lTarget
            // 
            this.lTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lTarget.Location = new System.Drawing.Point(-3, 409);
            this.lTarget.Name = "lTarget";
            this.lTarget.Size = new System.Drawing.Size(65, 34);
            this.lTarget.TabIndex = 27;
            this.lTarget.Text = "Цель движения";
            this.lTarget.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lMode
            // 
            this.lMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lMode.Location = new System.Drawing.Point(-3, 445);
            this.lMode.Name = "lMode";
            this.lMode.Size = new System.Drawing.Size(65, 34);
            this.lMode.TabIndex = 28;
            this.lMode.Text = "Включен режим";
            this.lMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenu
            // 
            this.contextMenu.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miConnect,
            this.miDisconnect,
            this.miConnections,
            this.miMaps,
            this.miSetValues,
            this.miModuleSettings,
            this.miReset,
            this.miCam});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(271, 244);
            // 
            // bMenu
            // 
            this.bMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bMenu.Image = ((System.Drawing.Image)(resources.GetObject("bMenu.Image")));
            this.bMenu.Location = new System.Drawing.Point(350, 6);
            this.bMenu.Name = "bMenu";
            this.bMenu.Size = new System.Drawing.Size(28, 30);
            this.bMenu.TabIndex = 30;
            this.bMenu.UseVisualStyleBackColor = true;
            this.bMenu.Click += new System.EventHandler(this.bMenu_Click);
            // 
            // fMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 550);
            this.Controls.Add(this.bMenu);
            this.Controls.Add(this.lMode);
            this.Controls.Add(this.lTarget);
            this.Controls.Add(this.lCurrent);
            this.Controls.Add(this.cbFollow);
            this.Controls.Add(this.rotatorPanelV);
            this.Controls.Add(this.rotatorPanelH);
            this.Controls.Add(this.bStop);
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
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.CheckBox cbFollow;
        private System.Windows.Forms.ToolStripMenuItem miReset;
        private System.Windows.Forms.ToolStripMenuItem miResetAzimuth;
        private System.Windows.Forms.ToolStripMenuItem miResetElevation;
        private System.Windows.Forms.ToolStripMenuItem miSetCoordinates;
        private System.Windows.Forms.ToolStripMenuItem miCam;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label lCurrent;
        private System.Windows.Forms.Label lTarget;
        private System.Windows.Forms.Label lMode;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.Button bMenu;
    }
}


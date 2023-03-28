namespace EncRotator
{
    partial class RotatorPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lDisplayAngle = new System.Windows.Forms.Label();
            this.nTargetAngle = new System.Windows.Forms.NumericUpDown();
            this.bRotateToTarget = new System.Windows.Forms.Button();
            this.bRotateCCW = new System.Windows.Forms.Button();
            this.bRotateCW = new System.Windows.Forms.Button();
            this.lWarning = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nTargetAngle)).BeginInit();
            this.SuspendLayout();
            // 
            // lDisplayAngle
            // 
            this.lDisplayAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lDisplayAngle.Location = new System.Drawing.Point(14, 0);
            this.lDisplayAngle.Name = "lDisplayAngle";
            this.lDisplayAngle.Size = new System.Drawing.Size(100, 37);
            this.lDisplayAngle.TabIndex = 0;
            this.lDisplayAngle.Text = "359.9";
            this.lDisplayAngle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // nTargetAngle
            // 
            this.nTargetAngle.DecimalPlaces = 1;
            this.nTargetAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nTargetAngle.Location = new System.Drawing.Point(3, 40);
            this.nTargetAngle.Maximum = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
            this.nTargetAngle.Name = "nTargetAngle";
            this.nTargetAngle.Size = new System.Drawing.Size(80, 31);
            this.nTargetAngle.TabIndex = 1;
            this.nTargetAngle.Value = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
            // 
            // bRotateToTarget
            // 
            this.bRotateToTarget.Location = new System.Drawing.Point(85, 40);
            this.bRotateToTarget.Name = "bRotateToTarget";
            this.bRotateToTarget.Size = new System.Drawing.Size(41, 29);
            this.bRotateToTarget.TabIndex = 2;
            this.bRotateToTarget.Text = "OK";
            this.bRotateToTarget.UseVisualStyleBackColor = true;
            this.bRotateToTarget.Click += new System.EventHandler(this._rotateToTargetClick);
            // 
            // bRotateCCW
            // 
            this.bRotateCCW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bRotateCCW.Location = new System.Drawing.Point(3, 109);
            this.bRotateCCW.Name = "bRotateCCW";
            this.bRotateCCW.Size = new System.Drawing.Size(60, 60);
            this.bRotateCCW.TabIndex = 3;
            this.bRotateCCW.UseVisualStyleBackColor = true;
            this.bRotateCCW.MouseDown += new System.Windows.Forms.MouseEventHandler(this._rotateButtonMouseDown);
            this.bRotateCCW.MouseLeave += new System.EventHandler(this._rotateButtonMouseLeave);
            this.bRotateCCW.MouseUp += new System.Windows.Forms.MouseEventHandler(this._rotateButtonMouseUp);
            // 
            // bRotateCW
            // 
            this.bRotateCW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bRotateCW.Location = new System.Drawing.Point(66, 109);
            this.bRotateCW.Name = "bRotateCW";
            this.bRotateCW.Size = new System.Drawing.Size(60, 60);
            this.bRotateCW.TabIndex = 4;
            this.bRotateCW.UseVisualStyleBackColor = true;
            this.bRotateCW.MouseDown += new System.Windows.Forms.MouseEventHandler(this._rotateButtonMouseDown);
            this.bRotateCW.MouseLeave += new System.EventHandler(this._rotateButtonMouseLeave);
            this.bRotateCW.MouseUp += new System.Windows.Forms.MouseEventHandler(this._rotateButtonMouseUp);
            // 
            // lWarning
            // 
            this.lWarning.BackColor = System.Drawing.Color.Red;
            this.lWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lWarning.ForeColor = System.Drawing.Color.White;
            this.lWarning.Location = new System.Drawing.Point(7, 74);
            this.lWarning.Name = "lWarning";
            this.lWarning.Size = new System.Drawing.Size(119, 31);
            this.lWarning.TabIndex = 5;
            this.lWarning.Text = "Нет связи";
            this.lWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RotatorPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lWarning);
            this.Controls.Add(this.bRotateCW);
            this.Controls.Add(this.bRotateCCW);
            this.Controls.Add(this.bRotateToTarget);
            this.Controls.Add(this.nTargetAngle);
            this.Controls.Add(this.lDisplayAngle);
            this.Name = "RotatorPanel";
            this.Size = new System.Drawing.Size(129, 175);
            ((System.ComponentModel.ISupportInitialize)(this.nTargetAngle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lDisplayAngle;
        private System.Windows.Forms.NumericUpDown nTargetAngle;
        private System.Windows.Forms.Button bRotateToTarget;
        private System.Windows.Forms.Button bRotateCCW;
        private System.Windows.Forms.Button bRotateCW;
        private System.Windows.Forms.Label lWarning;
    }
}

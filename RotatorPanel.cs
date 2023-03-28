using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using EncRotator.Properties;
using Jerome;
using AsyncConnectionNS;

namespace EncRotator
{
    public partial class RotatorPanel : UserControl
    {
        static double angleToDegrees(int angle)
        {
            return ((double)angle) * 0.3515625;
        }

        int _rotatorIdx;
        internal int rotatorIdx { 
            get { return _rotatorIdx; }
            set { 
                _rotatorIdx = value; 
                if  (value == fMain.ROTATOR_V)
                {
                    if (targetAngle > 90 || targetAngle < 0)
                    {
                        targetAngle = 0;
                    }
                    nTargetAngle.Minimum = 0;
                    nTargetAngle.Maximum = 90;
                }
                updateRotateButtonsImages();
            }
        }
        internal EventHandler<RotateToTargetClickEventArgs> rotateToTargetClick;
        internal EventHandler<RotateButtonMouseDownEventArgs> rotateButtonMouseDown;
        internal EventHandler<RotatorPanelEventArgs> rotateButtonMouseUp;
        private bool _rotatorConnected;
        internal bool rotatorConnected
        {
            get { return _rotatorConnected; }
            set
            {
                _rotatorConnected = value;
                warning = value ? "" : "Нет связи";
                if (!value)
                    displayAngle = 0;
                updateRotateButtonsImages();
            }
        }
        internal int targetAngle
        {
            get { return (int)((double)nTargetAngle.Value * 2.84166); }
            set { nTargetAngle.Value = (decimal)angleToDegrees(value); }
        }

        internal string warning
        {
            set
            {
                Invoke((MethodInvoker)delegate
                {
                    if (string.IsNullOrEmpty(value))
                        lWarning.Visible = false;
                    else
                    {
                        lWarning.Text = value;
                        lWarning.Visible = true;
                    }
                });
            }
        }
        internal int displayAngle
        {
            set 
            {
                Invoke((MethodInvoker)delegate
                {
                    lDisplayAngle.Text = angleToDegrees(value).ToString("0.0");
                });
            }
        }
        private Dictionary<int, Button> rotateButtons;
        internal void blink(int dir)
        {
            updateRotateButtonsImages(dir);
        }

        private Image getRotateButtonImage(int dir, int blinkDir)
        {
            if (blinkDir == dir && rotateButtons[dir].Image != null)
                return null;
            if (dir == 1)
            {
                switch (rotatorIdx)
                {
                    case (fMain.ROTATOR_H): return rotatorConnected ? Resources.right_green : Resources.right_red; 
                    case (fMain.ROTATOR_V): return rotatorConnected ? Resources.up_green : Resources.up_red;
                }
            } else
            {
                switch (rotatorIdx)
                {
                    case (fMain.ROTATOR_H): return rotatorConnected ? Resources.left_green : Resources.left_red;
                    case (fMain.ROTATOR_V): return rotatorConnected ? Resources.down_green : Resources.down_red;
                }
            }
            return null;
        }

        private void updateRotateButtonsImages(int blinkDir = 0)
        {
            if (InvokeRequired)
                Invoke((MethodInvoker)delegate
                {
                    _updateRotateButtonsImages(blinkDir);
                });
            else
                _updateRotateButtonsImages(blinkDir);
        }

        private void _updateRotateButtonsImages(int blinkDir = 0)
        {
            foreach (int dir in rotateButtons.Keys)
                rotateButtons[dir].Image = getRotateButtonImage(dir, blinkDir);
        }


        public RotatorPanel()
        {
            InitializeComponent();
            rotateButtons = new Dictionary<int, Button> {
                { -1, bRotateCCW }, { 1, bRotateCW }
            };
            lDisplayAngle.Text = "0.0";
            nTargetAngle.Value = 0;
        }

        private void _rotateButtonMouseDown(object sender, MouseEventArgs e)
        {
            rotateButtonMouseDown?.Invoke(this, new RotateButtonMouseDownEventArgs { 
                dir = (Button)sender == bRotateCW ? 1 : -1,
                rotatorIdx = rotatorIdx
            });
        }

        private void _rotateButtonMouseUp(object sender, MouseEventArgs e)
        {
            rotateButtonMouseUp?.Invoke(this, new RotatorPanelEventArgs { rotatorIdx = rotatorIdx });
        }

        private void _rotateButtonMouseLeave(object sender, EventArgs e)
        {
            rotateButtonMouseUp?.Invoke(this, new RotatorPanelEventArgs { rotatorIdx = rotatorIdx });
        }

        private void _rotateToTargetClick(object sender, EventArgs e)
        {
            rotateToTargetClick?.Invoke(this, new RotateToTargetClickEventArgs { 
                target = targetAngle,
                rotatorIdx = rotatorIdx
            });
        }
    }

    internal class RotatorPanelEventArgs : EventArgs
    {
        internal int rotatorIdx;
    }


    internal class RotateButtonMouseDownEventArgs : RotatorPanelEventArgs
    {
        internal int dir;
    }


    internal class RotateToTargetClickEventArgs : RotatorPanelEventArgs
    {
        internal int target;
    }

}

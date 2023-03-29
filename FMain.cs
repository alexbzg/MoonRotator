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
using System.Threading;

namespace EncRotator
{

    public partial class fMain : Form, IMessageFilter
    {
        static DeviceTemplate[] templates = {
                    new DeviceTemplate { engineLines = new Dictionary<int, int>{ { 1, 15 }, { -1, 14 } },
                                            encoderLines = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                                            limitsLines = new Dictionary<int, int> {  { 1, 21 }, { -1, 20 } },
                                            ledLine = 22
                                        } //0 MoonRotator 1.0
                    };

        internal const int ROTATOR_H = 0;
        internal const int ROTATOR_V = 1;

        internal bool editConnectionGroup(ConnectionSettings connectionSettings)
        {
            throw new NotImplementedException();
        }

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;

        FormState formState = new FormState();
        int[] currentAngles = new int[] { -1, -1 };
        int[] targetAngles = new int[] { -1, -1 };
        volatile int rotateKeyDown = -1;
        int[] anglesChange = new int[] { 0, 0 };
        List<Bitmap> maps = new List<Bitmap>();
        volatile bool waitCursor;
        internal RotatorEngine[] rotators = new RotatorEngine[] { null, null };
        internal System.Threading.Timer[] readAngleTimers = new System.Threading.Timer[] { null, null };
        Dictionary<Keys, int>[] rotateKeys;
        Dictionary<int, RotatorPanel> rotatorPanels;


        double mapRatio = 0;
        bool closingFl = false;
        bool loaded = false;

        private bool angleValuesSet(int rotatorIdx)
        {
            return rotatorIdx == ROTATOR_H ? formState.northAngle != -1 : formState.zenithAngle != -1 && formState.horizonAngle != -1;
        }

        public fMain(string[] args)
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            rotateKeys = new Dictionary<Keys, int>[]
                {
                    new Dictionary<Keys, int>
                    {
                        { Keys.Left, -1 }, { Keys.Right, 1 }
                    },
                    new Dictionary<Keys, int>
                    {
                        { Keys.Down, -1 }, { Keys.Up, 1 }
                    }

                };
            rotatorPanels = new Dictionary<int, RotatorPanel>
            {
                { ROTATOR_H, rotatorPanelH },
                { ROTATOR_V, rotatorPanelV }
            };
            foreach (int rotatorIdx in rotatorPanels.Keys)
            {
                RotatorPanel rotatorPanel = rotatorPanels[rotatorIdx];
                rotatorPanel.rotatorIdx = rotatorIdx;
                rotatorPanel.rotateButtonMouseDown += rotateButtonMouseDown;
                rotatorPanel.rotateButtonMouseUp += rotateButtonMouseUp;
                rotatorPanel.rotateToTargetClick += rotateToTargetClick;
            }

            if (File.Exists(Application.StartupPath + "\\config.xml"))
            {
                loadConfig();

                string currentMapStr = "";
                if (formState.currentMap != -1 && formState.currentMap < formState.maps.Count)
                    currentMapStr = formState.maps[formState.currentMap];
                formState.maps.RemoveAll(item => !File.Exists(item));
                if (!currentMapStr.Equals(string.Empty))
                    formState.currentMap = formState.maps.IndexOf(currentMapStr);
                else
                    formState.currentMap = -1;
                formState.maps.ForEach(item => loadMap(item));
                if (formState.currentMap != -1)
                    setCurrentMap(formState.currentMap);
                else if (formState.maps.Count > 0)
                    setCurrentMap(0);
            }

            for (int idx = 0; idx < 2; idx++)
            {
                rotators[idx] = new RotatorEngine(formState.connections[idx]);
                rotators[idx].onConnected += rotatorConnected;
            }
        }

        private void rotateToTargetClick(object sender, RotateToTargetClickEventArgs e)
        {
            rotateToAngle(e.rotatorIdx, e.target);
        }

        private void rotateButtonMouseUp(object sender, RotatorPanelEventArgs e)
        {
            rotators[e.rotatorIdx].on(0);
        }

        private void rotateButtonMouseDown(object sender, RotateButtonMouseDownEventArgs e)
        {
            rotators[e.rotatorIdx].on(e.dir);
        }

        private void setCurrentMap(int val)
        {
            formState.currentMap = val;
            pMap.BackgroundImage = maps[val];
            writeConfig();
            mapRatio = (double)maps[val].Width / (double)maps[val].Height;
            adjustToMap();
        }


        private void rotatorConnected(object sender, ConnectionEventArgs e)
        {
            RotatorEngine rotator = (RotatorEngine)sender;
            int rotatorIdx = getRotatorIndex(rotator);

            if (e.success)
            {
                rotator.onDisconnected += rotatorDisconnected;
                rotator.onAngleRead += rotatorAngleRead;
                readAngleTimers[rotatorIdx] = new System.Threading.Timer(delegate { rotator.readAngle(); }, null, 100, 1000);

                timer.Enabled = true;

                miSetValues.Visible = true;
                miSetValues.Enabled = true;

                rotatorPanels[rotatorIdx].rotatorConnected = true;
                if (rotatorIdx == ROTATOR_H)
                    setOvercoilCaption();
            }
            else
            {
                showMessage($"Подключение не удалось: {getConnectionByRotator(rotator).name}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            updateMenu();
        }

        private void updateMenu()
        {
            Invoke((MethodInvoker)delegate
            {
                int connected = rotators.Count(item => item.connected);
                miConnect.Visible = connected != 2;
                miDisconnect.Visible = connected != 0;
                miSetValues.Visible = connected != 0;
                miSetNorth.Visible = rotators[ROTATOR_H].connected;
                miSetHorizon.Visible = rotators[ROTATOR_V].connected;
                miSetZenith.Visible = rotators[ROTATOR_V].connected;
            });
        }

        public DialogResult showMessage(string text, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return (DialogResult)this.Invoke((Func<DialogResult>)delegate
            {
                return MessageBox.Show(fMain.ActiveForm, text, "MoonRotator", buttons, icon);
            });
        }


        private void miModuleSettings_Click(object sender, EventArgs e)
        {
            (new fModuleSettings()).ShowDialog();
        }

        private ConnectionSettings getConnectionByRotator(RotatorEngine rotator)
        {
            return formState.connections[getRotatorIndex(rotator)];
        }

        private int getRotatorIndex(RotatorEngine rotator)
        {
            return Array.IndexOf(rotators, rotator);
        }

        private void rotatorDisconnected(object sender, DisconnectEventArgs e)
        {
            if (!closingFl) {

                RotatorEngine rotator = (RotatorEngine)sender;
                int rotatorIdx = getRotatorIndex(rotator);

                readAngleTimers[rotatorIdx]?.Change(Timeout.Infinite, Timeout.Infinite);
                readAngleTimers[rotatorIdx]?.Dispose(); 
                readAngleTimers[rotatorIdx] = null;

                rotatorPanels[rotatorIdx].rotatorConnected = false;
                currentAngles[rotatorIdx] = -1;
                anglesChange[rotatorIdx] = 0;
                updateMenu();
                this.Invoke((MethodInvoker)delegate
                {
                    miSetValues.Visible = false;
                    timer.Enabled = false;
                    if (rotatorIdx == ROTATOR_H)
                        pMap.Invalidate();
                });
            }
        }



        public void writeConfig()
        {
            if (loaded)
            {
                System.Drawing.Rectangle bounds = this.WindowState != FormWindowState.Normal ? this.RestoreBounds : this.DesktopBounds;
                formState.formLocation = bounds.Location;
                formState.formSize = bounds.Size;
            }
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\config.xml"))
            {
                XmlSerializer ser = new XmlSerializer(typeof(FormState));
                ser.Serialize(sw, formState);
            }
        }


        private void loadConfig()
        {
            XmlSerializer ser = new XmlSerializer(typeof(FormState));
            using (FileStream fs = File.OpenRead(Application.StartupPath + "\\config.xml"))
            {
                try
                {
                    formState = (FormState)ser.Deserialize(fs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading config: " + ex.ToString());
                }
            }
        }

        /*
        private void onLimit(int dir)
        {
            if (limitReached == dir)
                return;
            if (engineStatus == dir)
                engine(0);
            if (currentConnection.hwLimits)
                currentConnection.limits[dir] = currentAngle;
            writeConfig();
            this.Invoke((MethodInvoker)delegate
            {
                if (!slCalibration.Visible || slCalibration.Text != "Концевик")
                {
                    slCalibration.Text = "Концевик";
                    slCalibration.Visible = true;
                    string sDir = dir == 1 ? "по часовой стрелке" : "против часовой стрелки";
                    showMessage("Достигнут концевик. Дальнейшее движение антенны " + sDir + " невозможно", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            });
        }
        */



        private int aD(int a, int b)
        {
            int r = a - b;
            if (r > 512)
                r -= 512;
            else if (r < -256)
                r += 512;
            return r;
        }

        private void rotateToAngle(int rotatorIdx, int angle)
        {
            if (!angleValuesSet(rotatorIdx) || !rotators[rotatorIdx].connected)
                return;

            targetAngles[rotatorIdx] = absoluteAngle(rotatorIdx, angle);
            if (rotatorIdx == ROTATOR_H)
                pMap.Invalidate();
            if (targetAngles[rotatorIdx] != currentAngles[rotatorIdx])
            {
                double d = aD(targetAngles[rotatorIdx], currentAngles[rotatorIdx]);
                int dir = Math.Sign(d);
                if (rotatorIdx == ROTATOR_H)
                {
                    double nd = aD(formState.northAngle, currentAngles[rotatorIdx]);
                    if (dir == Math.Sign(nd) && Math.Abs(nd) <= Math.Abs(d) && Math.Abs(formState.coils + dir) > 1)
                    {
                        dir = -dir;
                    }
                }
                rotators[rotatorIdx].on(dir);
            }
        }

        private int absoluteAngle(int rotatorIdx, int relativeValue)
        {
            return rotatorIdx == ROTATOR_H?
                    formState.northAngle + relativeValue - (formState.northAngle + relativeValue > 1023 ? 1023 : 0) :
                    formState.horizonAngle + (int)((relativeValue - formState.horizonAngle) * (formState.zenithAngle - formState.horizonAngle) / (double)256);
        }

        private int relativeAngle(int rotatorIdx, int absoluteValue)
        {
            return rotatorIdx == ROTATOR_H ?
                    absoluteValue - formState.northAngle + (absoluteValue < formState.northAngle ? 1023 : 0) :
                    (int)((double)(absoluteValue - formState.horizonAngle) / (formState.zenithAngle - formState.horizonAngle) * 256);

        }

        private void setOvercoilCaption()
        {
            rotatorPanelH.warning = Math.Abs(formState.coils) > 1 ? "Перехлест" : "";
        }

        private void rotatorAngleRead(Object sender, AngleReadEventArgs e)
        {
            RotatorEngine rotator = (RotatorEngine) sender;
            int rotatorIdx = getRotatorIndex(rotator);
            if (e.angle != currentAngles[rotatorIdx])
            {
                if (rotatorIdx == ROTATOR_H && rotator.engineStatus != 0 && formState.northAngle != -1 && currentAngles[rotatorIdx] != -1 &&
                    Math.Sign(aD(formState.northAngle, currentAngles[rotatorIdx])) != Math.Sign(aD(formState.northAngle, e.angle)))
                {
                    formState.coils += rotator.engineStatus;
                    writeConfig();

                    this.Invoke((MethodInvoker)delegate
                    {
                        setOvercoilCaption();
                    });
                }

                anglesChange[rotatorIdx] += e.angle - currentAngles[rotatorIdx];
                currentAngles[rotatorIdx] = e.angle;
                
                if (angleValuesSet(rotatorIdx) && rotator.engineStatus != 0 && targetAngles[rotatorIdx] != -1)
                {
                    int tD = aD(targetAngles[rotatorIdx], currentAngles[rotatorIdx]);
                    if (Math.Abs(tD) < 2)
                    {
                        rotator.on(0);
                        targetAngles[rotatorIdx] = -1;
                        if (rotatorIdx == ROTATOR_H)
                            pMap.Invalidate();
                    }
                }
                int displayAngle = currentAngles[rotatorIdx];
                if (angleValuesSet(rotatorIdx))
                    displayAngle = relativeAngle(rotatorIdx, displayAngle);

                rotatorPanels[rotatorIdx].displayAngle = displayAngle;
            }
        }

        private void loadMap(string fMap)
        {
            if (File.Exists(fMap))
            {
                maps.Add(new Bitmap(fMap));
                if (formState.maps.IndexOf(fMap) == -1)
                    formState.maps.Add(fMap);
            }
        }

        private void adjustToMap()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                int tmpHeight = Height;
                WindowState = FormWindowState.Normal;
                Height = tmpHeight;
                Top = 0;
                Left = 0;
            }
            if (Math.Abs(mapRatio - (double)pMap.Width / (double)pMap.Height) > 0.01)
            {
                Width = (int)(mapRatio * pMap.Height) + Width - pMap.Width;
                //pMap.Refresh();
            }
        }

        private void pMap_Paint(object sender, PaintEventArgs e)
        {
            if (formState.currentMap != -1 && rotators[ROTATOR_H].connected && angleValuesSet(ROTATOR_H))
            {
                Action<int, Color> drawAngle = (int angle, Color color) =>
                {
                    if (angle == -1)
                        return;
                    double rAngle = (((double)(angle - formState.northAngle)) / 512) * Math.PI;
                    e.Graphics.DrawLine(new Pen(color, 2), pMap.Width / 2, pMap.Height / 2,
                        pMap.Width / 2 + (int)(Math.Sin(rAngle) * (pMap.Height / 2)),
                        pMap.Height / 2 - (int)(Math.Cos(rAngle) * (pMap.Height / 2)));

                };
                drawAngle(currentAngles[ROTATOR_H], Color.Red);
                drawAngle(targetAngles[ROTATOR_H], Color.Green);
            }
        }

        private int mouse2Angle(int mx, int my)
        {
            int angle;
            if (mx == pMap.Width / 2)
            {
                if (my < pMap.Height / 2)
                {
                    angle = 256;
                }
                else
                {
                    angle = 768;
                }
            }
            else
            {
                double y = pMap.Height / 2 - my;
                double x = mx - pMap.Width / 2;
                angle = (int)((Math.Atan(y / x) / Math.PI) * 512);
                if (x < 0)
                {
                    if (y > 0)
                    {
                        angle = 512 + angle;
                    }
                    else
                    {
                        angle = angle - 512;
                    }
                }
            }
            angle = 256 - angle;
            if (angle < 0) { angle += 1024; }
            return angle;
        }

        private void pMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (maps.Count > 1)
                    if (formState.currentMap < maps.Count - 1)
                        setCurrentMap(++formState.currentMap);
                    else
                        setCurrentMap(0);
            }
            else if (rotators[ROTATOR_H].connected && angleValuesSet(ROTATOR_H) && currentAngles[ROTATOR_H] != -1)            
                rotateToAngle(ROTATOR_H, mouse2Angle(e.X, e.Y));            
        }


        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            closingFl = true;
            disconnect();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            for (int rotatorIdx = 0; rotatorIdx < 2; rotatorIdx++)
            {
                if (anglesChange[rotatorIdx] != 0)
                {
                    if (rotatorIdx == ROTATOR_H)
                        pMap.Invalidate();
                    int dir = Math.Sign(anglesChange[rotatorIdx]);
                    if (rotatorIdx == ROTATOR_H) {
                        dir *= Math.Sign(512 - anglesChange[rotatorIdx]);
                    } else if (angleValuesSet(ROTATOR_V))
                    {
                        dir *= Math.Sign(formState.zenithAngle - formState.horizonAngle);
                    }
                    anglesChange[rotatorIdx] = 0;
                    rotatorPanels[rotatorIdx].blink(dir);
                }
                else
                {
                    rotatorPanels[rotatorIdx].blink(0);
                }
            }
        }

        private void miMapAdd_Click(object sender, EventArgs e)
        {
            if (ofdMap.ShowDialog() == DialogResult.OK)
            {
                loadMap(ofdMap.FileName);
                setCurrentMap(maps.Count - 1);
                writeConfig();
            }
        }

        private void pMap_Resize(object sender, EventArgs e)
        {
            //pMap.Refresh();
        }


        private void disconnect()
        {
            foreach (RotatorEngine rotator in rotators)
                rotator.disconnect();
        }

        public bool editConnection(ConnectionSettings conn)
        {
            fConnectionParams fParams = new fConnectionParams(conn);
            fParams.ShowDialog(this);
            bool result = fParams.DialogResult == DialogResult.OK;
            if (result)
            {
                conn.jeromeParams.host = fParams.tbHost.Text.Trim();
                conn.jeromeParams.port = Convert.ToInt16(fParams.tbPort.Text.Trim());
                writeConfig();
            }
            return result;
        }

        private void miEditConnection_Click(object sender, EventArgs e)
        {
            editConnection(formState.connections[sender == miHConnection ? ROTATOR_H : ROTATOR_V]);
        }

        private void fMain_Resize(object sender, EventArgs e)
        {
            if (mapRatio != 0)
                adjustToMap();
        }


        private void lSizeM_Click(object sender, EventArgs e)
        {
            Height = 300;
        }

        private void lSizeP_Click(object sender, EventArgs e)
        {
            Height = 800;
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            foreach (RotatorEngine rotator in rotators)
                rotator.on(0);
        }


        private void miMapRemove_Click(object sender, EventArgs e)
        {
            maps.RemoveAt(formState.currentMap);
            formState.maps.RemoveAt(formState.currentMap);
            if (maps.Count > 0)
            {
                if (formState.currentMap > 0)
                    setCurrentMap(--formState.currentMap);
                else
                    setCurrentMap(1);
            }
            else
            {
                formState.currentMap = -1;
                pMap.BackgroundImage = null;
                pMap.Refresh();
                writeConfig();
            }
        }


        private void fMain_Load(object sender, EventArgs e)
        {
            if (formState.formLocation != null && formState.formSize != null)
                this.DesktopBounds =
                    new Rectangle((Point)formState.formLocation, (Size)formState.formSize);
            loaded = true;
//            AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
//            AutoUpdater.Start("http://73.ru/apps/AntennaNetRotatorRemote/update.xml");
        }

        private void fMain_ResizeEnd(object sender, EventArgs e)
        {
            if (loaded)
                writeConfig();
        }

        private void pMap_MouseMove(object sender, MouseEventArgs e)
        {
            rotatorPanelH.targetAngle = mouse2Angle(e.X, e.Y);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Left || keyData == Keys.Right) && msg.Msg == WM_KEYDOWN)
            {
                if (rotateKeyDown == -1 && !waitCursor)
                {
                    int rotatorIdx = getRotatorIndex(keyData);
                    rotators[rotatorIdx].on(rotateKeys[rotatorIdx][keyData]);
                    rotateKeyDown = rotatorIdx;
                }
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        private void switchWaitCursor(bool val)
        {
            if (val != waitCursor)
            {
                waitCursor = val;
                Cursor = val ? Cursors.WaitCursor : Cursors.Default;
            }
        }



        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYUP && rotateKeyDown != -1)
            {
                rotators[rotateKeyDown].on(0);
                rotateKeyDown = -1;
            }
            return false;
        }

        private void miConnect_Click(object sender, EventArgs e)
        {
            connect();
        }

        private void connect()
        {
            switchWaitCursor(true);
            for (int idx = 0; idx < 2; idx++)
            {
                if (!rotators[idx].connected)
                    rotators[idx].connect();
            }
            switchWaitCursor(false);
        }

        private int getRotatorIndex(Keys keys)
        {
            return rotateKeys[ROTATOR_H].ContainsKey(keys) ? ROTATOR_H : ROTATOR_V;
        }

        private void bStop_Click_1(object sender, EventArgs e)
        {
            foreach (RotatorEngine rotator in rotators)
                rotator.on(0);
        }

        private void miSetValueClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            if (mi == miSetNorth)
            {
                formState.northAngle = currentAngles[ROTATOR_H];
                pMap.Invalidate();
                rotatorPanelH.displayAngle = relativeAngle(ROTATOR_H, currentAngles[ROTATOR_H]);
            } else
            {
                int prevValue;
                if (mi == miSetHorizon)
                {
                    prevValue = formState.horizonAngle;
                    formState.horizonAngle = currentAngles[ROTATOR_V];
                }
                else
                {
                    prevValue = formState.zenithAngle;
                    formState.zenithAngle = currentAngles[ROTATOR_V];
                }
                if (formState.zenithAngle == formState.horizonAngle)
                {
                    showMessage("Горизонт не может совпадать с зенитом!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (mi == miSetHorizon)
                        formState.horizonAngle = prevValue;
                    else 
                        formState.zenithAngle = prevValue;
                    return;
                }
                rotatorPanelV.displayAngle = relativeAngle(ROTATOR_V, currentAngles[ROTATOR_V]);
            }
            writeConfig();
        }

        private void miDisconnect_Click(object sender, EventArgs e)
        {
            disconnect();
        }
    }




    public class ConnectionSettings
    {
        public int deviceType = 0;
        public string name = "";
        public JeromeConnectionParams jeromeParams = new JeromeConnectionParams();

        public override string ToString()
        {
            return name;
        }

    }



    public class FormState
    {
        public List<string> maps = new List<string>();
        public int currentMap = -1;
        public System.Drawing.Point? formLocation = null;
        public System.Drawing.Size? formSize = null;
        public ConnectionSettings[] connections = new ConnectionSettings[] { 
            new ConnectionSettings() { name = "Азимут", jeromeParams = new JeromeConnectionParams { usartPort = -1 } },
            new ConnectionSettings() { name = "Элевация", jeromeParams = new JeromeConnectionParams { usartPort = -1 } }
            };
        public int coils = 1;
        public int northAngle = -1;
        public int zenithAngle = -1;
        public int horizonAngle = -1;
    }


}

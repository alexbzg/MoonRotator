using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using Jerome;
using TcpConnectionNS;
using System.Threading;
using FFMpegUtils;
using InputBox;
using System.Globalization;
using System.Threading.Tasks;
using NLog;
using SunCalcNet;
using NLog.Fluent;
using SunCalcNet.Model;

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
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        Dictionary<Keys, int>[] rotateKeys;
        Dictionary<int, RotatorPanel> rotatorPanels;
        FFPlayWindow camWindow;

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

#if DEBUG
            logger.Info("start");
            LogManager.Flush();
#endif           

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
                rotators[idx].onAngleRead += rotatorAngleRead;
            }


        }

        private async void rotateToTargetClick(object sender, RotateToTargetClickEventArgs e)
        {
            await rotateToAngle(e.rotatorIdx, e.target);
        }

        private async void rotateButtonMouseUp(object sender, RotatorPanelEventArgs e)
        {
            await rotators[e.rotatorIdx].on(0);
        }

        private async void rotateButtonMouseDown(object sender, RotateButtonMouseDownEventArgs e)
        {
            await rotators[e.rotatorIdx].on(e.dir);
        }

        private void setCurrentMap(int val)
        {
            formState.currentMap = val;
            pMap.BackgroundImage = maps[val];
            writeConfig();
            mapRatio = (double)maps[val].Width / (double)maps[val].Height;
            adjustToMap();
        }


        private void rotatorConnected(object sender, ConnectEventArgs e)
        {
            RotatorEngine rotator = (RotatorEngine)sender;
            int rotatorIdx = getRotatorIndex(rotator);

            if (e.success)
            {
                rotator.onDisconnected += rotatorDisconnected;
                rotator.onAngleRead += rotatorAngleRead;

                Invoke((MethodInvoker)delegate
                {
                    timer.Enabled = true;
                    if (rotatorIdx == ROTATOR_H)
                    {
                        setOvercoilCaption();
                        pMap.Invalidate();
                    }
                });

                rotatorPanels[rotatorIdx].rotatorConnected = true;
            }
            else
            {
                showMessage($"Подключение не удалось: {getConnectionByRotator(rotator).name}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            updateMenu();
        }

        private void updateMenu()
        {
            Invoke((MethodInvoker) delegate
            {
                int connected = rotators.Count(item => item.connected);
                miConnect.Visible = connected != 2;
                miFollow.Visible = connected == 2 && formState.lat != 256;
                if (miFollow.Checked && connected != 2)
                    updateFollow();
                miDisconnect.Visible = connected != 0;
                miSetAzimuth.Visible = rotators[ROTATOR_H].connected;
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
            RotatorEngine rotator = (RotatorEngine)sender;
            int rotatorIdx = getRotatorIndex(rotator);
    
            if (!closingFl) {
                rotatorPanels[rotatorIdx].rotatorConnected = false;
                currentAngles[rotatorIdx] = -1;
                anglesChange[rotatorIdx] = 0;
                updateMenu();
                Invoke((MethodInvoker)delegate
                {
                    timer.Enabled = rotators[1 - rotatorIdx].connected;
                    if (rotatorIdx == ROTATOR_H)
                        pMap.Invalidate();
                    if (miFollow.Checked)
                    {
                        miFollow.Checked = false;
                        updateFollow();
                    }
                });
            }
        }



        public void writeConfig()
        {
            if (loaded)
            {
                Rectangle bounds = this.WindowState != FormWindowState.Normal ? this.RestoreBounds : this.DesktopBounds;
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
                    logger.Error("Error loading config: " + ex.ToString());
                }
            }
        }

        private int aD(int target, int start)
        {
            int r = target - start;
            if (r > 512)
                r -= 1024;
            else if (r < -512)
                r += 1024;
            return r;
        }

        private async Task rotateToAngle(int rotatorIdx, int angle)
        {
            if (!angleValuesSet(rotatorIdx) || !rotators[rotatorIdx].connected)
                return;

            targetAngles[rotatorIdx] = absoluteAngle(rotatorIdx, angle);
            if (rotatorIdx == ROTATOR_H)
                pMap.Invalidate();
            if (targetAngles[rotatorIdx] != currentAngles[rotatorIdx])
            {
                int d = aD(targetAngles[rotatorIdx], currentAngles[rotatorIdx]);
                int dir = Math.Sign(d);
                if (rotatorIdx == ROTATOR_H)
                {
                    double nd = aD(formState.northAngle, currentAngles[rotatorIdx]);
                    if (dir == Math.Sign(nd) && Math.Abs(nd) <= Math.Abs(d) && Math.Abs(formState.coils + dir) > 1)
                    {
                        dir = -dir;
                    }
                }
                await rotators[rotatorIdx].on(dir);
            }
        }

        private int normAngle(int value)
        {
            if (value < 0)
                value += 1023;
            if (value > 1023)
                value -= 1023;
            return value;
        }

        private int absoluteAngle(int rotatorIdx, int relativeValue)
        {
            return normAngle(rotatorIdx == ROTATOR_H?
                    formState.northAngle + relativeValue :
                    formState.horizonAngle + (int)(((double)relativeValue / 256) * aD(formState.zenithAngle, formState.horizonAngle)));
        }

        private int relativeAngle(int rotatorIdx, int absoluteValue)
        {
            return rotatorIdx == ROTATOR_H ?
                    normAngle(absoluteValue - formState.northAngle) :
                    (int)((double)aD(absoluteValue, formState.horizonAngle) / aD(formState.zenithAngle, formState.horizonAngle) * 256);
        }

        private int rotatorVSign()
        {
            return Math.Sign(aD(formState.zenithAngle, formState.horizonAngle));
        }

        private void setOvercoilCaption()
        {
            rotatorPanelH.warning = Math.Abs(formState.coils) > 1 ? "Перехлест" : "";
        }

        private async void rotatorAngleRead(Object sender, AngleReadEventArgs e)
        {
            RotatorEngine rotator = (RotatorEngine) sender;
            int rotatorIdx = getRotatorIndex(rotator);
            if (e.angle != currentAngles[rotatorIdx])
            {
                logger.Debug($"{formState.connections[rotatorIdx].name} angle {e.angle}");
                //warn if horizontal overcoil
                if (rotatorIdx == ROTATOR_H && rotator.engineStatus != 0 && formState.northAngle != -1 && currentAngles[rotatorIdx] != -1 &&
                    Math.Sign(aD(formState.northAngle, currentAngles[rotatorIdx])) != Math.Sign(aD(formState.northAngle, e.angle)))
                {
                    formState.coils += rotator.engineStatus;
                    writeConfig();

                    Invoke((MethodInvoker)delegate
                    {
                        setOvercoilCaption();
                    });
                }
                if (rotatorIdx == ROTATOR_H)
                    Invoke((MethodInvoker)delegate
                    {
                        pMap.Invalidate();
                    });

                //set current angle
                anglesChange[rotatorIdx] += e.angle - currentAngles[rotatorIdx];
                currentAngles[rotatorIdx] = e.angle;

                //stop if target is reached
                if (angleValuesSet(rotatorIdx) && rotator.engineStatus != 0 && targetAngles[rotatorIdx] != -1)
                {
                    int tD = aD(targetAngles[rotatorIdx], currentAngles[rotatorIdx]);
                    if (Math.Abs(tD) < 2)
                    {
                        await rotator.on(0);
                        targetAngles[rotatorIdx] = -1;
                        if (rotatorIdx == ROTATOR_H)
                            pMap.Invalidate();
                    }
                }

                //stop if vertical and getting out of bounds
                if (rotatorIdx == ROTATOR_V && angleValuesSet(ROTATOR_V) && ((Math.Abs(aD(formState.zenithAngle, currentAngles[ROTATOR_V])) < 2 && rotator.engineStatus == rotatorVSign()) ||
                        (Math.Abs(aD(formState.horizonAngle, currentAngles[ROTATOR_V])) < 2 && rotator.engineStatus == -rotatorVSign())))
                    await rotator.on(0);
                
                //show current angle
                int displayAngle = currentAngles[rotatorIdx];
                if (angleValuesSet(rotatorIdx))
                    displayAngle = relativeAngle(rotatorIdx, displayAngle);
                rotatorPanels[rotatorIdx].displayAngle = displayAngle;

            }
        }


        private async Task rotatorsParallelHelper(Func<RotatorEngine, Task> action)
        {
            await Task.WhenAll(rotators.Select(rotator => Task.Run(async () => await action(rotator))));
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

        private async void pMap_MouseClick(object sender, MouseEventArgs e)
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
                await rotateToAngle(ROTATOR_H, mouse2Angle(e.X, e.Y));            
        }


        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            closingFl = true;
            disconnect();
            camWindow?.kill();
        }

        private async void timer_Tick(object sender, EventArgs e)
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
            if (miFollow.Checked)
                await followMoon();
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


        private async void disconnect()
        {
            await rotatorsParallelHelper(async rotator => await rotator.disconnect());
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
            cbCam.Enabled = !string.IsNullOrEmpty(formState.camURL);
            cbCam.Checked = formState.showCam;
            loaded = true;
            if (formState.showCam)
                showCam();
            updateMenu();

         //   AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");
         //   AutoUpdater.Start("http://tnxqso.com/static/files/apps/MoonRotator/update.xml");
        }

        private void showCam()
        {
            //rtsp://admin:admin123@192.168.1.10:554/mode=real&idc=1&ids=2
            if (!string.IsNullOrEmpty(formState.camURL) && camWindow == null)
            {
                try
                {
                    camWindow = new FFPlayWindow(formState.camURL, this.Handle);
                    if (formState.camWindowPos.left != 0)
                        camWindow.moveWindow(formState.camWindowPos);
                    camWindow.windowResizeMove += camWindowResizeMove;
                    camWindow.windowClose += camWindowClose;
                }
                catch (TimeoutException)
                {
                    showMessage("Не удалось подключиться к камере. Проверьте состояние камеры и настройки подключения.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cbCam.Checked = false;
                    formState.showCam = false;
                    writeConfig();
                }
            }
        }

        private void unsubscribeCamEvents()
        {
            if (camWindow != null)
            {
                camWindow.windowClose -= camWindowClose;
                camWindow.windowResizeMove -= camWindowResizeMove;
            }
        }
        private void camWindowClose(object sender, EventArgs e)
        {
            formState.showCam = false;
            cbCam.Checked = false;
            writeConfig();
            unsubscribeCamEvents();
            camWindow?.Dispose();
            camWindow = null;
        }

        private void camWindowResizeMove(object sender, WindowResizeMoveEventArgs e)
        {
            formState.camWindowPos = e.newWindowRect;
            writeConfig();
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
                    Task.Run(async () => await rotators[rotatorIdx].on(rotateKeys[rotatorIdx][keyData]) );
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
                Task.Run(async () => await rotators[rotateKeyDown].on(0) );
                rotateKeyDown = -1;
            }
            return false;
        }

        private async void miConnect_Click(object sender, EventArgs e)
        {
            await connect();
        }

        private async Task connect()
        {
            /*   await Task.WhenAll(
                   rotators.Select(rotator => Task.Run(async () =>
                   {
                       if (!rotator.connected)
                           await rotator.connect();
                   })));
            */
            await rotatorsParallelHelper(async rotator =>
            {
                if (!rotator.connected)
                    await rotator.connect();
            });
        }

        private int getRotatorIndex(Keys keys)
        {
            return rotateKeys[ROTATOR_H].ContainsKey(keys) ? ROTATOR_H : ROTATOR_V;
        }

        private async void stopEngines()
        {
            await rotatorsParallelHelper(async rotator => await rotator.on(0));
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            if (miFollow.Checked)
            {
                miFollow.Checked = false;
                updateFollow();
            } else
                stopEngines();
        }

        private void miSetValueClick(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            if (mi == miSetAzimuth)
            {
                FInputBox urlInputBox = new FInputBox("Азимут", "0.0");
                if (urlInputBox.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        float currentAngleDeg = float.Parse(urlInputBox.value, CultureInfo.InvariantCulture);
                        if (currentAngleDeg < 0 || currentAngleDeg > 360)
                            throw new OverflowException();
                        int northAngle = (int)(currentAngles[ROTATOR_H] - currentAngleDeg * 2.84166);
                        if (northAngle < 0)
                            northAngle += 1023;
                        if (northAngle != formState.northAngle)
                        {
                            formState.northAngle = northAngle;
                            pMap.Invalidate();
                            rotatorPanelH.displayAngle = relativeAngle(ROTATOR_H, currentAngles[ROTATOR_H]);
                        }
                    }
                    catch
                    {
                        showMessage("Введите число от 0 до 360.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
                    showMessage("Горизонт не может совпадать с зенитом.", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void miCamURL_Click(object sender, EventArgs e)
        {
            FInputBox urlInputBox = new FInputBox("Камера", formState.camURL);
            if (urlInputBox.ShowDialog() == DialogResult.OK && urlInputBox.value != formState.camURL)
            {
                formState.camURL = urlInputBox.value;
                writeConfig();
                cbCam.Enabled = !string.IsNullOrEmpty(formState.camURL);
                if (camWindow != null)
                {
                    closeCam();
                    showCam();
                }
            }
        }

        private void closeCam()
        {
            unsubscribeCamEvents();
            camWindow?.kill();
            camWindow?.Dispose();
            camWindow = null;
        }

        private void cbCam_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCam.Checked)
                showCam();
            else
                closeCam();
            formState.showCam = cbCam.Checked;
            writeConfig();
        }

        private async void miReset_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            int rotatorIdx = mi == miResetAzimuth ? ROTATOR_H : ROTATOR_V;
            ConnectionSettings connection = formState.connections[rotatorIdx];
            if (showMessage($"Перегрузить контроллер: {connection.name}?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RotatorEngine rotator = rotators[rotatorIdx];
                bool connected = rotator.connected;
                if (connected)
                    await rotator.disconnect();
                await connection.jeromeParams.reset();
                if (connected)
                {
                    await Task.Delay(1000);
                    await rotator.connect();
                }
            }
        }

        private void miSetCoordinates_Click(object sender, EventArgs e)
        {
            string coo = formState.lat != 256 && formState.lng != 256 ? $"{formState.lat}, {formState.lng}" : "";
            FInputBox cooInputBox = new FInputBox("Широта, долгота", coo);
            if (cooInputBox.ShowDialog() == DialogResult.OK && cooInputBox.value != coo)
            {
                bool success = false;
                coo = cooInputBox.value;
                if (string.IsNullOrEmpty(coo))
                {
                    formState.lat = 256;
                    formState.lng = 256;
                }
                else
                {
                    string[] parts = coo.Split(new char[] { ' ', ',', ';' }).Where(item => !string.IsNullOrEmpty(item)).ToArray();
                    if (parts.Length == 2)
                    {
                        try
                        {
                            double lat = Convert.ToDouble(parts[0], CultureInfo.InvariantCulture);
                            double lng = Convert.ToDouble(parts[1], CultureInfo.InvariantCulture);
                            if (Math.Abs(lat) <= 180 && Math.Abs(lng) <= 180)
                            {
                                formState.lat = lat;
                                formState.lng = lng;
                                success = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "coordinates parse exception");
                        }
                    }
                }
                if (success)
                {
                    if (miFollow.Checked && formState.lat == 256)
                    {
                        miFollow.Checked = false;
                        updateFollow();
                    }
                    updateMenu();
                    writeConfig();
                }
                else
                {
                    showMessage($"Некорректные координаты.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task followMoon()
        {
            MoonPosition moonPosition = MoonCalc.GetMoonPosition(DateTime.UtcNow, formState.lat, formState.lng);
            if (moonPosition.Altitude > 0)
            {
                int azimuth = RotatorEngine.radToEncoder(moonPosition.Azimuth + Math.PI);
                if (azimuth < 0)
                    azimuth += 1024;
                int elevation = RotatorEngine.radToEncoder(moonPosition.Altitude);
                await rotateToAngle(ROTATOR_V, elevation);
                await rotateToAngle(ROTATOR_H, azimuth);
                Invoke((MethodInvoker)delegate
                {
                    rotatorPanelH.targetAngle = azimuth;
                    rotatorPanelV.targetAngle = elevation;
                });
            } else
            {
                Invoke((MethodInvoker)delegate
                {
                    miFollow.Checked = false;
                    updateFollow();
                    showMessage("Луна ниже горизонта!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
        }

        private void updateFollow()
        {
            Invoke((MethodInvoker) async delegate
            {
                string toolTipText = miFollow.Checked ? "Режим автоматического следования за Луной" : "";
                pMap.Enabled = !miFollow.Checked;
                foreach (RotatorPanel panel in rotatorPanels.Values)
                    panel.Enabled = !miFollow.Checked;
                if (miFollow.Checked)
                {
                    toolTip.SetToolTip(bStop, "Режим автоматического следования за Луной");
                    await followMoon();
                }
                else
                {
                    toolTip.RemoveAll();
                    stopEngines();
                }
            });
        }

        private void miFollow_Click(object sender, EventArgs e)
        {
            updateFollow();
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
        public RECT camWindowPos = new RECT();
        public bool showCam = false;
        public string camURL = "rtsp://admin:admin123@192.168.1.10:554/mode=real&idc=1&ids=2";
        public ConnectionSettings[] connections = new ConnectionSettings[] { 
            new ConnectionSettings() { name = "Азимут", jeromeParams = new JeromeConnectionParams { usartPort = -1 } },
            new ConnectionSettings() { name = "Элевация", jeromeParams = new JeromeConnectionParams { usartPort = -1 } }
            };
        public int coils = 1;
        public int northAngle = -1;
        public int zenithAngle = -1;
        public int horizonAngle = -1;
        public double lat = 256;
        public double lng = 256;
    }


}

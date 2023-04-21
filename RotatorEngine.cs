using Jerome;
using System;
using System.Collections.Generic;
using System.Linq;
using TcpConnectionNS;
using System.Threading.Tasks;

namespace EncRotator
{

    internal class DeviceTemplate
    {
        internal Dictionary<int, int> engineLines;
        internal Dictionary<int, int> limitsLines;
        internal int[] encoderLines;
        internal int ledLine;
    }

    internal class ConnectionEventArgs : EventArgs
    {
        public bool success;
    }

    internal class AngleReadEventArgs : EventArgs
    {
        public int angle;
    }


    internal class RotatorEngine
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        internal static int angleDistance(int a, int b)
        {
            int r = a - b;
            if (r > 512)
                r -= 512;
            else if (r < -256)
                r += 512;
            return r;
        }

        static internal DeviceTemplate[] TEMPLATES = {
                    new DeviceTemplate { engineLines = new Dictionary<int, int>{ { 1, 15 }, { -1, 14 } },
                                            encoderLines = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                                            limitsLines = new Dictionary<int, int> {  { 1, 21 }, { -1, 20 } },
                                            ledLine = 22
                                        } //0 MoonRotator 1.0
                    };

        DeviceTemplate template;
        private object controllerStateTimer;
        ConnectionSettings connectionSettings;
        JeromeController controller;
        internal int engineStatus = 0;
        internal volatile int limitReached = 0;
        int encGrayVal = -1;
        int currentAngle = -1;
        internal event EventHandler<DisconnectEventArgs> onDisconnected;
        internal event EventHandler<ConnectionEventArgs> onConnected;
        internal event EventHandler<AngleReadEventArgs> onAngleRead;
        internal bool connected
        {
            get { return controller?.connected ?? false; }
        }


        internal RotatorEngine(ConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
            template = TEMPLATES[connectionSettings.deviceType];
            //controllerStateTimer = new System.Threading.Timer(delegate { getControllerState(); }, null, 100, 1000);
        }

        internal async Task getControllerState()
        {
            JeromeControllerState controllerState = await connectionSettings.jeromeParams.getState();
            if (controllerState != null)
            {
                encGrayVal = 0;
                for (int lineNo = 0; lineNo < template.encoderLines.Length; lineNo++)
                {
                    if (!controllerState.linesStates[template.encoderLines[lineNo] - 1])
                    {
                        encGrayVal |= 1 << lineNo;
                    }
                }
                int val = encGrayVal;
                for (int mask = val >> 1; mask != 0; mask = mask >> 1)
                {
                    val ^= mask;
                }
                currentAngle = val;
                onAngleRead?.Invoke(this, new AngleReadEventArgs { angle = currentAngle });
            }
        }

        private async Task setLine(int line, int mode)
        {
            if (controller != null && controller.connected)
                await controller.setLineMode(line, mode);
        }

        private async Task toggleLine(int line, int state)
        {
            if (controller != null && controller.connected)
                await controller.switchLine(line, state);
        }

        public async Task on(int val)
        {
            logger.Debug($"Engine: {val}");
            if (controller != null && controller.connected && val != engineStatus && (limitReached == 0 || limitReached != val))
            {
                if (val == 0 || engineStatus != 0)
                {
                    int prevDir = engineStatus;
                    await toggleLine(template.engineLines[prevDir], 0);
                }
                if (val != 0)
                {
                    await toggleLine(template.engineLines[val], 1);
                }
                engineStatus = val;
            }
        }


        internal async Task disconnect()
        {
            if (controller != null && controller.connected)
            {
                if (engineStatus != 0)
                {
                    await on(0);
                }
                await toggleLine(template.ledLine, 0);
                controller.disconnect();
            }
        }


        private void lineStateChanged(object sender, LineStateChangedEventArgs e)
        {
            if (template.limitsLines.Values.Contains(e.line))
            {
                if (e.state == 0)
                {
                    int dir = template.limitsLines.SingleOrDefault(x => x.Value == e.line).Key;
                    //onLimit(dir);
                }
            }
        }

        internal async Task connect()
        {
            controller = JeromeController.create(connectionSettings.jeromeParams);
            if (controller != null)
            {
                if (await controller.connect())
                {
                    controller.lineStateChanged += lineStateChanged;
                    controller.onDisconnected += controllerDisconnected;

                    await setLine(template.ledLine, JeromeController.LINE_OUT);
                    foreach (int line in template.engineLines.Values)
                    {
                        await setLine(line, JeromeController.LINE_OUT);
                        await toggleLine(line, 0);
                    }
                    foreach (int line in template.encoderLines)
                        await setLine(line, JeromeController.LINE_IN);

                    onConnected?.Invoke(this, new ConnectionEventArgs { success = true });

                    _ = readAngle();
                }
                else
                {
                    onConnected?.Invoke(this, new ConnectionEventArgs { success = false });
                    controller = null;
                }
            }
        }

        private void controllerDisconnected(object sender, DisconnectEventArgs e)
        {
            controller.onDisconnected -= controllerDisconnected;
            controller = null;
            onDisconnected?.Invoke(this, e);
        }

        internal async Task readAngle()
        {
            logger.Debug($"{connectionSettings.name} Sending readlines query");
            string lines = await controller?.readlines();
            if (lines?.Length == 22)
            {
                encGrayVal = 0;
                for (int lineNo = 0; lineNo < template.encoderLines.Length; lineNo++)
                {
                    if (lines[template.encoderLines[lineNo] - 1] == '0')
                    {
                        encGrayVal |= 1 << lineNo;
                    }
                }
                int val = encGrayVal;
                for (int mask = val >> 1; mask != 0; mask = mask >> 1)
                {
                    val ^= mask;
                }
                currentAngle = val;
                onAngleRead?.Invoke(this, new AngleReadEventArgs { angle = currentAngle });
            }
        }




    }
}


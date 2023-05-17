using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.Threading;
using TcpConnectionNS;


namespace Jerome
{

    public class LineStateChangedEventArgs : EventArgs
    {
        public int line;
        public int state;
    }

    public class ConnectEventArgs : EventArgs
    {
        public bool success;
    }

    public class JeromeController : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const int LINE_IN = 1;
        public const int LINE_OUT = 2;
        class CmdEntry
        {
            public string cmd;
            public Action<string> cb;

            public CmdEntry(string cmdE, Action<string> cbE)
            {
                cmd = cmdE;
                cb = cbE;
            }
        }


        private volatile CmdEntry currentCmd = null;
        private object cmdQueeLock = new object();
        private List<CmdEntry> cmdQuee = new List<CmdEntry>();

        private const int TIMEOUT = 3000;
        private static Regex rEVT = new Regex(@"#EVT,IN,\d+,(\d+),(\d)");
        private Timer replyTimer;
        private bool isReplyTimeout;

        // ManualResetEvent instances signal completion.

        public event EventHandler<DisconnectEventArgs> onDisconnected;
        public event EventHandler<ConnectEventArgs> onConnected;
        public event EventHandler<LineStateChangedEventArgs> lineStateChanged;
       

        public JeromeConnectionParams connectionParams;
        private TcpConnection _connection;
        public bool resetControllerOnDisconnect { get; set; }
        public int reconnectInterval { get; set; }

        public bool connected
        {
            get
            {
                return _connection?.connected ?? false;
            }
        }

        
        public static JeromeController create( JeromeConnectionParams p )
        {
            IPAddress hostIP;
            if (IPAddress.TryParse(p.host, out hostIP))
            {
                JeromeController jc = new JeromeController();
                jc.connectionParams = p;
                return jc;
            }
            else
            {
                return null;
            }
        }

        private void newCmd(string cmd, Action<string> cb)
        {
            CmdEntry ce = new CmdEntry(cmd, cb);
            lock (cmdQueeLock)
            {
                cmdQuee.Add(ce);
            }
            if (currentCmd == null)
                processQuee();
        }

        private void processQuee()
        {
            if (cmdQuee.Count > 0 && currentCmd == null)
            {
                lock (cmdQueeLock)
                {
                    currentCmd = cmdQuee[0];
                    cmdQuee.RemoveAt(0);
                }
                string cmd = "$KE";
                if (!string.IsNullOrEmpty(currentCmd.cmd)) {
                    cmd += $",{currentCmd.cmd}";
                }
                _sendCommand(cmd);
                replyTimer = new Timer( obj => replyTimeout(), null, TIMEOUT, Timeout.Infinite);
            }
        }

        private void _sendCommand(string cmd)
        {
            _ = Task.Run(async () => {
                try
                {
                    await _connection.sendCommand(cmd);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"{connectionParams.name} command {cmd} send exception");
                }
            });
        }

        private async void ping()
        {
            if (cmdQuee.Count > 0 && currentCmd == null)
                await sendCommand("");
        }

        public async Task<string> sendCommand(string cmd)
        {
            string result = null;
            AsyncManualResetEvent reDone = new AsyncManualResetEvent(false);
            newCmd(cmd, delegate (string r)
            {
                result = r;
                reDone.Set();
            });
            await reDone.WaitAsync();
            return result;
        }


        public async Task connect()
        {
            bool result = false;
            _connection = new TcpConnection(connectionParams.host, connectionParams.port);            
            if (await _connection.connect())
            {
                _connection.lineReceived += lineReceived;
                //_connection.disconnected += _disconnected;
                await sendCommand("PSW,SET," + connectionParams.password);
                await sendCommand("EVT,ON");
                logger.Debug($"{connectionParams.name} connected");
                result = true;
            }
            else
            {
                logger.Debug($"{connectionParams.name} connection failed");
            }
            _ = Task.Run(() => {
                try
                {
                    onConnected?.Invoke(this, new ConnectEventArgs() { success = result });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"{connectionParams.name} connect event handler exception");
                }
            });
        }

        private void _disconnected(object sender, DisconnectEventArgs e)
        {
            bool requested = !isReplyTimeout && e.requested;
            isReplyTimeout = false;
            onDisconnected?.Invoke(this, new DisconnectEventArgs() { requested = requested });
            if (resetControllerOnDisconnect)
                Task.Run(async () => {
                    try
                    {
                        await connectionParams.reset();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"{connectionParams.name} reset controller exception");
                    }
                });
            if (reconnectInterval != 0 && !requested)
            {
                Task.Run(async () => {
                    try
                    {
                        await Task.Delay(reconnectInterval);
                        await connect();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"{connectionParams.name} reconnect exception");
                    }
                });
            }
        }

        public void disconnect(bool requested = true)
        {
            replyTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            currentCmd = null;
            cmdQuee.Clear();
            if (_connection != null)
            {
                _connection.disconnected -= _disconnected;
                try
                {
                    _connection.Close();
                    logger.Debug($"{connectionParams.name} connection closed");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"{connectionParams.name} connection close exception\n");
                }
                _connection = null;
                _disconnected(this, new DisconnectEventArgs() { requested = requested });
            }
        }

        private void lineReceived(object sender, LineReceivedEventArgs e )
        {
            string reply = e.line;
            Match match = rEVT.Match(reply);
            if (match.Success)
            {
                int line = Convert.ToInt16( match.Groups[1].Value );
                int lineState = match.Groups[2].Value == "0" ? 0 : 1;
                logger.Debug($"{connectionParams.name} event notification: {reply}");
                //Task.Factory.StartNew( () =>
                //    lineStateChanged?.Invoke(this, new LineStateChangedEventArgs { line = line, state = lineState } ) );
            }
            else if ( !reply.StartsWith( "#SLINF" ) && !reply.Contains( "FLAGS" ) && !reply.Contains( "JConfig" ) )
            {
                replyTimer?.Change(Timeout.Infinite, Timeout.Infinite);                
                if (currentCmd?.cb != null)
                {
                    Action<string> callback = currentCmd.cb;
                    _ = Task.Run(() => {
                        try
                        {
                            callback.Invoke(reply);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"{connectionParams.name} command {currentCmd.cmd} reply {reply} callback exception");
                        }
                    });
                }
                lock (cmdQuee)
                {
                    currentCmd = null;
                }
                processQuee();
            }
        }

        public async Task setLineMode(int line, int mode)
        {
            await sendCommand($"IO,SET,{line},{mode}");
        }

        public async Task switchLine(int line, int state)
        {
            await sendCommand($"WR,{line},{state}");
        }

        public async Task<string> readlines()
        {
            string reply = await sendCommand("RID,ALL");
            int split = reply.LastIndexOf( ',' );
            return reply.Substring(split + 1).TrimEnd('\r', '\n');
        }

        private void replyTimeout()
        {
            if (connected)
            {
                logger.Debug($"{connectionParams.name} reply timeout");
                isReplyTimeout = true;
                disconnect();
            }
        }

        public void Dispose()
        {
            disconnect();
            replyTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public class JeromeConnectionParams
    {


        public string name;
        public string host;
        public int port = 2424;
        public int usartPort = 2525;
        public string password = "Jerome";
        public int httpPort = 80;

        public async Task<HttpWebResponse> request(string path, string query = "", bool discard = true)
        {
            if (string.IsNullOrEmpty(host) || httpPort == 0)
                return null;

            string authHeader = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("admin:Jerome"));
            string url = $"http://{host}:{httpPort}/{path}";
            if (!string.IsNullOrEmpty(query)) { 
                url += "?" + query;
            }
            try { 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().WithTimeout(new TimeSpan(10000));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!discard)
                        return response;
                }
                else
                {
                    using (StreamReader stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        Debug.WriteLine($"Query to {url} failed with code {response.StatusCode}.");
                        Debug.WriteLine(stream.ReadToEnd());
                    }
                }
                response.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Query to {url} raised exception {e}.");
            }
            return null;
        }

        public async Task toggleLineType(int lineNo)
        {
            await request("server.cgi", $"data=SIO,{lineNo}");
        }

        public async Task toggleLineState(int lineNo)
        {
            await request("server.cgi", $"data=OUT,{lineNo},undefined");
        }

        public async Task reset()
        {
            await request("server.cgi", "data=RST");
        }

        public async Task<JeromeControllerState> getState()
        {
            HttpWebResponse response = await request("state.xml", discard: false);
            if (response != null) {
                XElement xr;
                using (StreamReader stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    xr = XElement.Parse(stream.ReadToEnd());
                }
                JeromeControllerState result = new JeromeControllerState();
                string linesModes = xr.XPathSelectElement("iotable").Value;
                string linesStates = xr.XPathSelectElement("iovalue").Value;
                for (int co = 0; co < 22; co++)
                {
                    result.linesModes[co] = linesModes[co] == '1';
                    result.linesStates[co] = linesStates[co] == '1';
                }
                for (int co = 0; co < 4; co++)
                    result.adcsValues[co] = (int)xr.XPathSelectElement("adc" + (co + 1).ToString());
                return result;
            }
            return null;
        }
    }

    public class JeromeControllerState {
        public bool[] linesModes = new bool[22];
        public bool[] linesStates = new bool[22];
        public int[] adcsValues = new int[4];
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using AsyncConnectionNS;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using Microsoft.VisualStudio.Threading;

namespace Jerome
{

    public class LineStateChangedEventArgs : EventArgs
    {
        public int line;
        public int state;
    }

    public class JeromeController
    {
        const int LINE_IN = 1;
        const int LINE_OUT = 2;
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



        private const int TIMEOUT = 10000;
        private static TimeSpan TIMEOUT_SPAN = new TimeSpan(TIMEOUT);
        private const int PING_INTERVAL = 20000;
        private static Regex rEVT = new Regex(@"#EVT,IN,\d+,(\d+),(\d)");
        private System.Threading.Timer replyTimer;

        // ManualResetEvent instances signal completion.

        private IPEndPoint remoteEP;
        private string password;
        private AsyncConnection connection;
        private AsyncConnection usartConnection;

        public event EventHandler<DisconnectEventArgs> onDisconnected {
            add { connection.disconnected += value; }
            remove { connection.disconnected -= value; }
        }
        public event EventHandler<LineStateChangedEventArgs> lineStateChanged;
        public event EventHandler<LineReceivedEventArgs> usartLineReceived
        {
            add { usartConnection.lineReceived += value; }
            remove { usartConnection.lineReceived -= value; }
        }
        public event EventHandler<BytesReceivedEventArgs> usartBytesReceived
        {
            add { usartConnection.bytesReceived += value; }
            remove { usartConnection.bytesReceived -= value; }
        }

        public JeromeConnectionParams connectionParams;

        public bool connected
        {
            get
            {
                return connection != null && connection.connected;
            }
        }

        public bool usartConnected {
            get
            {
                return usartConnection != null && usartConnection.connected;
            }
        }

        public bool usartBinaryMode
        {
            get { return usartConnection.binaryMode; }
            set { usartConnection.binaryMode = value; }
        }

        
        public static JeromeController create( JeromeConnectionParams p )
        {
            IPAddress hostIP;
            if (IPAddress.TryParse(p.host, out hostIP))
            {
                JeromeController jc = new JeromeController();
                jc.remoteEP = new IPEndPoint(hostIP, p.port);
                jc.password = p.password;
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
                connection.sendCommand(cmd);
                replyTimer = new Timer( obj => replyTimeout(), null, TIMEOUT, Timeout.Infinite);
            }
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
            await reDone.WaitAsync().WithTimeout(TIMEOUT_SPAN);
            return result;
        }



        public async Task<bool> connect()
        {
            bool result = false;
            AsyncManualResetEvent reConnect = new AsyncManualResetEvent();

            await Task.Run(() =>
            {
                connection = new AsyncConnection();
                connection.connect(connectionParams.host, connectionParams.port);
                if (connection.connected)
                {
                    connection.lineReceived += processReply;
                    _ = sendCommand("PSW,SET," + connectionParams.password);
                    _ = sendCommand("EVT,ON");
                    if (connectionParams.usartPort != -1)
                    {
                        usartConnection = new AsyncConnection();
                        usartConnection.connect(connectionParams.host, connectionParams.usartPort);
                    }
                    result = true;
                }
                reConnect.Set();
            });
            await reConnect.WaitAsync().WithTimeout(TIMEOUT_SPAN);
            return result;
        }

        private void UsartConnection_lineReceived(object sender, LineReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void disconnect()
        {
            currentCmd = null;
            cmdQuee.Clear();
            _disconnect(true);
        }

        private void _disconnect(bool requested)
        {
            System.Diagnostics.Debug.WriteLine("disconnect");
            if (connected)
                connection.disconnect();
            if (usartConnected)
                usartConnection.disconnect();
        }


        private void processReply(object sender, LineReceivedEventArgs e )
        {
            string reply = e.line;
            System.Diagnostics.Debug.WriteLine(reply);
            Match match = rEVT.Match(reply);
            if (match.Success)
            {
                int line = Convert.ToInt16( match.Groups[1].Value );
                int lineState = match.Groups[2].Value == "0" ? 0 : 1;
                Task.Factory.StartNew( () =>
                    lineStateChanged?.Invoke(this, new LineStateChangedEventArgs { line = line, state = lineState } ) );
            }
            else if ( !reply.StartsWith( "#SLINF" ) && !reply.Contains( "FLAGS" ) && !reply.Contains( "JConfig" ) )
            {
                replyTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                if (currentCmd != null && currentCmd.cb != null)
                {
                    Action<string> cb = currentCmd.cb;
                    Task.Run( () => cb.Invoke(reply) );
                }
                lock (cmdQuee)
                {
                    currentCmd = null;
                }
                processQuee();

            }
        }

        public void setLineMode(int line, int mode)
        {
            sendCommand("IO,SET," + line.ToString() + "," + mode.ToString());
        }

        public void switchLine(int line, int state)
        {
            sendCommand("WR," + line.ToString() + "," + state.ToString());
        }

        public async Task<string> readlines()
        {
            string reply = await sendCommand("RID,ALL");
            int split = reply.LastIndexOf( ',' );
            return reply.Substring(split + 1).TrimEnd('\r', '\n');
        }



        private void replyTimeout()
        {
            System.Diagnostics.Debug.WriteLine("Reply timeout");
            _disconnect(false);
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

        public async Task<HttpWebResponse> request(string path, string query = "", bool discard = true, bool reset = false)
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
                request.Headers.Add("Authorization", "Basic " + authHeader);
                if (reset)
                {
                    _ = request.GetResponseAsync();
                    return null;
                }
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
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

        public void toggleLineType(int lineNo)
        {
            request("server.cgi", $"data=SIO,{lineNo}");
        }

        public void toggleLineState(int lineNo)
        {
            request("server.cgi", $"data=OUT,{lineNo},undefined");
        }

        public void reset()
        {
            request("server.cgi", "data=RST", reset: true);
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

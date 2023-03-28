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

namespace Jerome
{

    public class LineStateChangedEventArgs : EventArgs
    {
        public int line;
        public int state;
    }

    public class JeromeController
    {

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
        private Object cmdQueeLock = new Object();
        private List<CmdEntry> cmdQuee = new List<CmdEntry>();



        private static int timeout = 10000;
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
                connection.sendCommand("$KE," + currentCmd.cmd);
                replyTimer = new Timer( obj => replyTimeout(), null, timeout, Timeout.Infinite);
            }
        }

        public string sendCommand(string cmd)
        {
            string result = "";
            ManualResetEvent reDone = new ManualResetEvent(false);
            newCmd(cmd, delegate (string r)
            {
                result = r;
                reDone.Set();
            });
            reDone.WaitOne(timeout);
            return result;
        }



        public bool connect()
        {
            connection = new AsyncConnection();
            connection.connect(connectionParams.host, connectionParams.port);
            if (connection.connected)
            {
                connection.lineReceived += processReply;
                sendCommand("PSW,SET," + connectionParams.password);
                sendCommand("EVT,ON");
                if (connectionParams.usartPort != -1)
                {
                    usartConnection = new AsyncConnection();
                    usartConnection.connect(connectionParams.host, connectionParams.usartPort);
                }
                return true;
            } else
                return false;
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
                replyTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if (currentCmd != null && currentCmd.cb != null)
                {
                    Action<string> cb = currentCmd.cb;
                    Task.Factory.StartNew( () => cb.Invoke(reply) );
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

        public string readlines()
        {
            string reply = sendCommand("RID,ALL");
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

        public JeromeControllerState getState()
        {
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create("http://" + host + ":" + httpPort + "/state.xml");
            try
            {
                HttpWebResponse resp = (HttpWebResponse)rq.GetResponse();
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    XElement xr;
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
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
                else
                {
                    System.Diagnostics.Debug.WriteLine("Query to " + host + ":" + httpPort.ToString() +
                        " returned status code" + resp.StatusCode.ToString());
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Query to " + host + ":" + httpPort.ToString() +
                    " error: " + e.ToString());
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

using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenOcdTraceUtil
{
    public partial class OpenOcdTraceUtilMain : Form
    {
        private OpenOcdTclClient.OpenOcdTclClient client;
        private SwoDec.SwoDecoder decoder;
        private ItmPortsForm itmPorts;

        public OpenOcdTraceUtilMain()
        {
            InitializeComponent();

            // setup decoder
            decoder = new SwoDec.SwoDecoder();
            decoder.PacketAvailable += DecoderOnPacketAvailable;

            // setup client
            client = new OpenOcdTclClient.OpenOcdTclClient(this);
            client.Trace = true;
            client.Notifications = true;
            client.ConnectionChanged += ClientOnConnectionChanged;
            client.TargetTrace += ClientOnTargetTrace;
            client.TargetStateChanged += ClientOnTargetStateChanged;

            // setup ports form
            itmPorts = new ItmPortsForm();
            itmPorts.ItmPortEnableChange += OnItmPortsItmPortEnableChange;
            itmPorts.ItmPortsEnableChange += OnItmPortsItmPortsEnableChange;
        }

        private void DecoderOnPacketAvailable(object sender, SwoDec.SwoDecoder.PacketAvailableArgs args)
        {
            switch (args.Packet.Type)
            {
                case SwoDec.SwoPacketType.Instrumentation:
                    DecoderOnInstrumentationPacketAvailable(args.Packet as SwoDec.Packets.InstrumentationPacket);
                    break;
                case SwoDec.SwoPacketType.Sync:
                    break;
                default:
                    richTextBoxLog.AppendText(String.Format("Unhandled Packet: {0}\n", args.Packet.Type));
                    richTextBoxLog.ScrollToEnd();
                    break;
            }
        }

        private void DecoderOnInstrumentationPacketAvailable(SwoDec.Packets.InstrumentationPacket packet)
        {
            var port = itmPorts.Ports[packet.Address];

            switch (port.Type)
            {
                case ItmPortType.Ascii:
                    if (packet.PayloadSize > 1)
                    {
                        richTextBoxLog.AppendText(String.Format("{0}: Incorrect Payload Size for ASCII mode {1}\n", packet.Address, packet.Value));
                        return;
                    }

                    port.StringBuffer += Encoding.UTF8.GetString(new byte[] { (byte)packet.Value });
                    if (port.StringBuffer.EndsWith("\n"))
                    {
                        richTextBoxLog.AppendText(String.Format("{0}: {1}\n", packet.Address, port.StringBuffer.Trim()), port.Color);
                        richTextBoxLog.ScrollToEnd();
                        port.StringBuffer = "";
                    }
                    break;
                case ItmPortType.Binary:
                    if (port.BinaryBufferInProgress)
                    {
                        if (port.BinaryBuffer.Count < port.BinaryBufferLength)
                        {
                            if (packet.PayloadSize == 4)
                                port.BinaryBuffer.AddRange(BitConverter.GetBytes(packet.Value));
                            else if (packet.PayloadSize == 2)
                                port.BinaryBuffer.AddRange(BitConverter.GetBytes((ushort)packet.Value));
                            else if (packet.PayloadSize == 1)
                                port.BinaryBuffer.Add((byte)packet.Value);
                        }

                        if (port.BinaryBuffer.Count == port.BinaryBufferLength)
                        {
                            port.StringBuffer = String.Concat(port.BinaryBuffer.Select(b => b.ToString("X2")));
                            richTextBoxLog.AppendText(String.Format("{0}: {1}\n", packet.Address, port.StringBuffer.Trim()), port.Color);
                            richTextBoxLog.ScrollToEnd();
                            port.StringBuffer = "";
                            port.BinaryBuffer = null;
                            port.BinaryBufferInProgress = false;
                        }
                        else if (port.BinaryBuffer.Count > port.BinaryBufferLength)
                        {
                            var diff = port.BinaryBuffer.Count - port.BinaryBufferLength;
                            port.StringBuffer = String.Concat(port.BinaryBuffer.Select(b => b.ToString("X2")));
                            richTextBoxLog.AppendText(String.Format("{0}: Buffer overflow for Binary mode {1}\n", packet.Address, diff));
                            richTextBoxLog.AppendText(String.Format("{0}: {1}\n", packet.Address, port.StringBuffer.Trim()), port.Color);
                            richTextBoxLog.ScrollToEnd();
                            port.StringBuffer = "";
                            port.BinaryBuffer = null;
                            port.BinaryBufferInProgress = false;
                            return;
                        }
                    }
                    else
                    {
                        if (packet.PayloadSize != 4)
                        {
                            richTextBoxLog.AppendText(String.Format("{0}: Incorrect Payload Size for Binary mode {1}, {2}\n", packet.Address, packet.PayloadSize, packet.Value.ToString("X8")));
                            richTextBoxLog.ScrollToEnd();
                            return;
                        }

                        if ((packet.Value & 0xFFFF0000) == 0xBAEF0000)
                        {
                            port.BinaryBufferLength = packet.Value & 0xFFFF;
                            port.BinaryBuffer = new System.Collections.Generic.List<byte>();
                            port.BinaryBufferInProgress = true;
                        }
                        else
                        {
                            richTextBoxLog.AppendText(String.Format("{0}: Incorrect Value for Binary mode {1}\n", packet.Address, packet.Value.ToString("X8")));
                            richTextBoxLog.ScrollToEnd();
                            return;
                        }
                    }
                    break;
            }
        }

        #region ItmPorts Event Handlers

        private void OnItmPortsItmPortsEnableChange(object sender, ItmPortsForm.ItmPortsEnableChangeArgs args)
        {
            if (client.Connected)
                client.ItmPorts(args.Enabled);
        }

        private void OnItmPortsItmPortEnableChange(object sender, ItmPortsForm.ItmPortEnableChangeArgs args)
        {
            if (client.Connected)
                client.ItmPort(args.Port.Channel, args.Port.Enabled);
        }

        #endregion

        #region Client Event Handlers

        private void ClientOnTargetTrace(object sender, OpenOcdTclClient.OpenOcdTclClient.TargetTraceArgs args)
        {
            decoder.Feed(args.Data);
            decoder.Decode();
        }

        private void ClientOnConnectionChanged(object sender, EventArgs e)
        {
            if (client.Connected)
            {
                toolStripStatusLabelConnected.Text = "Connected";
                if (itmPorts.Ports.All(p => p.Enabled))
                    client.ItmPorts(true);
                else
                {
                    if (!itmPorts.Ports[0].Enabled)
                        client.ItmPort(0, false);
                    foreach (var port in itmPorts.Ports.Where(p => p.Enabled))
                        client.ItmPort(port.Channel, true);
                }
            }
            else
                toolStripStatusLabelConnected.Text = "Disconnected";
        }

        private void ClientOnTargetStateChanged(object sender, OpenOcdTclClient.OpenOcdTclClient.TargetStateArgs args)
        {
            toolStripStatusLabelTargetState.Text = args.State.ToString();
        }

        #endregion

        #region Form Event Handlers

        private void OpenOcdTraceUtilMainLoad(object sender, EventArgs e)
        {
            client.Start();
        }

        private void ItmPortsToolStripMenuItemClick(object sender, EventArgs e)
        {
            itmPorts.Show();
        }

        #endregion
    }
}

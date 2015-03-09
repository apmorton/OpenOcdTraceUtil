using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenOcdTraceUtil
{
    public partial class OpenOcdTraceUtilMain : Form
    {
        private OpenOcdTclClient.OpenOcdTclClient client;
        private SwoDec.SwoDecoder decoder;

        public OpenOcdTraceUtilMain()
        {
            InitializeComponent();
            decoder = new SwoDec.SwoDecoder();
            decoder.PacketAvailable += DecoderOnPacketAvailable;
            client = new OpenOcdTclClient.OpenOcdTclClient(this);
            client.Trace = true;
            client.Notifications = true;
            client.ConnectionChanged += ClientOnConnectionChanged;
            client.TargetTrace += ClientOnTargetTrace;
            client.TargetStateChanged += ClientOnTargetStateChanged;
        }

        private void ClientOnTargetStateChanged(object sender, OpenOcdTclClient.OpenOcdTclClient.TargetStateArgs args)
        {
            toolStripStatusLabelTargetState.Text = args.State.ToString();
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
                    textBox1.AppendText(String.Format("Unhandled Packet: {0}\n", args.Packet.Type));
                    break;
            }
        }

        private Dictionary<int, List<byte>> instBuffer = new Dictionary<int, List<byte>>();
        private void DecoderOnInstrumentationPacketAvailable(SwoDec.Packets.InstrumentationPacket packet)
        {
            // create buffer if it doesn't exist
            if (!instBuffer.ContainsKey(packet.Address))
                instBuffer[packet.Address] = new List<byte>();

            if (packet.PayloadSize > 1)
            {
                textBox1.AppendText(String.Format("{0}: {1}\n", packet.Address, packet.Value));
                return;
            }

            if (packet.Value == 10)
            {
                var str = Encoding.UTF8.GetString(instBuffer[packet.Address].ToArray()).Trim();
                textBox1.AppendText(String.Format("{0}: {1}\n", packet.Address, str));
                instBuffer[packet.Address].Clear();
            }
            else if (packet.Value != 13)
            {
                instBuffer[packet.Address].Add((byte)packet.Value);
            }
        }

        private void ClientOnTargetTrace(object sender, OpenOcdTclClient.OpenOcdTclClient.TargetTraceArgs args)
        {
            decoder.Feed(args.Data);
            decoder.Decode();
        }

        private void ClientOnConnectionChanged(object sender, EventArgs e)
        {
            if (client.Connected)
                toolStripStatusLabelConnected.Text = "Connected";
            else
                toolStripStatusLabelConnected.Text = "Disconnected";
        }

        private void OpenOcdTraceUtilMain_Load(object sender, EventArgs e)
        {
            client.Start();
        }
    }
}

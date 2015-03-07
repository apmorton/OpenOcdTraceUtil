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

        public OpenOcdTraceUtilMain()
        {
            InitializeComponent();
            client = new OpenOcdTclClient.OpenOcdTclClient(this);
            client.ConnectionChanged += ClientOnConnectionChanged;
            client.TargetTrace += ClientOnTargetTrace;
        }

        private void ClientOnTargetTrace(object sender, OpenOcdTclClient.OpenOcdTclClient.TargetTraceArgs args)
        {
        }

        private void ClientOnConnectionChanged(object sender, EventArgs e)
        {
            if (client.Connected)
                toolStripStatusLabelConnected.Text = "Connected";
            else
                toolStripStatusLabelConnected.Text = "Disconnected";
        }
    }
}

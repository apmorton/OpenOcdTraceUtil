using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenOcdTraceUtil
{
    public partial class ItmPortsForm : Form
    {
        private const int DefaultPortCount = 32;
        private List<Color> defaultColors = new List<Color>()
        {
            Color.Blue, Color.Brown, Color.Cyan, Color.DarkGreen, Color.DarkOrange,
            Color.DarkViolet, Color.DeepPink, Color.ForestGreen, Color.Fuchsia, Color.Lime,
            Color.Navy, Color.Olive, Color.Red, Color.Sienna, Color.Teal, Color.Tomato,
            Color.Violet, Color.DarkSlateBlue, Color.DarkKhaki, Color.DarkGoldenrod, Color.DeepSkyBlue,
            Color.DimGray, Color.DodgerBlue, Color.GreenYellow, Color.MediumVioletRed, Color.OliveDrab,
            Color.SaddleBrown, Color.SteelBlue, Color.DarkSeaGreen, Color.Indigo, Color.IndianRed,
            Color.Firebrick, Color.Chartreuse
        };

        private readonly List<CheckBox> portCheckBoxes;
        private readonly List<ComboBox> portComboBoxes;
        private readonly ItmPortTypeMap[] itmPortTypeMap = new ItmPortTypeMap[]
        {
            new ItmPortTypeMap() { Name = "ASCII", Value = ItmPortType.Ascii },
            new ItmPortTypeMap() { Name = "Binary", Value = ItmPortType.Binary },
            new ItmPortTypeMap() { Name = "Interval", Value = ItmPortType.Interval },
            new ItmPortTypeMap() { Name = "Event", Value = ItmPortType.Event },
        };

        public readonly ItmPort[] Ports = new ItmPort[DefaultPortCount];

        public ItmPortsForm()
        {
            InitializeComponent();

            portCheckBoxes = new List<CheckBox>() {
                checkBoxPort0,  checkBoxPort1,  checkBoxPort2,  checkBoxPort3,  checkBoxPort4,  checkBoxPort5,  checkBoxPort6,  checkBoxPort7,
                checkBoxPort8,  checkBoxPort9,  checkBoxPort10, checkBoxPort11, checkBoxPort12, checkBoxPort13, checkBoxPort14, checkBoxPort15,
                checkBoxPort16, checkBoxPort17, checkBoxPort18, checkBoxPort19, checkBoxPort20, checkBoxPort21, checkBoxPort22, checkBoxPort23,
                checkBoxPort24, checkBoxPort25, checkBoxPort26, checkBoxPort27, checkBoxPort28, checkBoxPort29, checkBoxPort30, checkBoxPort31
            };

            portComboBoxes = new List<ComboBox>() {
                comboBoxPort0,  comboBoxPort1,  comboBoxPort2,  comboBoxPort3,  comboBoxPort4,  comboBoxPort5,  comboBoxPort6,  comboBoxPort7,
                comboBoxPort8,  comboBoxPort9,  comboBoxPort10, comboBoxPort11, comboBoxPort12, comboBoxPort13, comboBoxPort14, comboBoxPort15,
                comboBoxPort16, comboBoxPort17, comboBoxPort18, comboBoxPort19, comboBoxPort20, comboBoxPort21, comboBoxPort22, comboBoxPort23,
                comboBoxPort24, comboBoxPort25, comboBoxPort26, comboBoxPort27, comboBoxPort28, comboBoxPort29, comboBoxPort30, comboBoxPort31
            };

            for (var i = 0; i < DefaultPortCount; i++)
            {
                Ports[i] = new ItmPort() { Channel = i, Color = defaultColors[i], Type = ItmPortType.Ascii };
                Ports[i].PropertyChanged += ItmPortsFormPropertyChanged;

                portComboBoxes[i].DataSource = itmPortTypeMap.ToArray();
                portComboBoxes[i].DisplayMember = "Name";
                portComboBoxes[i].ValueMember = "Value";

                portCheckBoxes[i].DataBindings.Add("Checked", Ports[i], "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
                portCheckBoxes[i].DataBindings.Add("ForeColor", Ports[i], "Color", false, DataSourceUpdateMode.OnPropertyChanged);
                portComboBoxes[i].DataBindings.Add("SelectedValue", Ports[i], "Type", false, DataSourceUpdateMode.OnPropertyChanged);
            }

            toolStripComboBoxAll.ComboBox.DataSource = itmPortTypeMap.ToArray();
            toolStripComboBoxAll.ComboBox.DisplayMember = "Name";
            toolStripComboBoxAll.ComboBox.ValueMember = "Value";
        }
  
        private void ItmPortsFormPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
                OnItmPortEnableChange(sender as ItmPort);
        }

        private void ItmPortsFormFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void AllOnToolStripMenuItemClick(object sender, EventArgs e)
        {
            itmPortEnableChangeSuspended = true;
            foreach (var port in Ports)
                port.Enabled = true;
            itmPortEnableChangeSuspended = false;
            OnItmPortsEnableChange(true);
        }

        private void AllOffToolStripMenuItemClick(object sender, EventArgs e)
        {
            itmPortEnableChangeSuspended = true;
            foreach (var port in Ports)
                port.Enabled = false;
            itmPortEnableChangeSuspended = false;
            OnItmPortsEnableChange(false);
        }

        private void ToolStripComboBoxAllSelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var port in Ports)
                port.Type = itmPortTypeMap[toolStripComboBoxAll.SelectedIndex].Value;
        }

        #region Events

        public class ItmPortEnableChangeArgs : EventArgs
        {
            public ItmPort Port;

            public ItmPortEnableChangeArgs(ItmPort e)
            {
                Port = e;
            }
        }

        private bool itmPortEnableChangeSuspended = false;

        public delegate void ItmPortEnableChangeHandler(object sender, ItmPortEnableChangeArgs args);
        public ItmPortEnableChangeHandler ItmPortEnableChange;
        public void OnItmPortEnableChange(ItmPort e)
        {
            if (ItmPortEnableChange != null && !itmPortEnableChangeSuspended)
            {
                ItmPortEnableChange(this, new ItmPortEnableChangeArgs(e));
            }
        }

        public class ItmPortsEnableChangeArgs : EventArgs
        {
            public bool Enabled;

            public ItmPortsEnableChangeArgs(bool e)
            {
                Enabled = e;
            }
        }

        public delegate void ItmPortsEnableChangeHandler(object sender, ItmPortsEnableChangeArgs args);
        public ItmPortsEnableChangeHandler ItmPortsEnableChange;
        public void OnItmPortsEnableChange(bool e)
        {
            if (ItmPortsEnableChange != null)
            {
                ItmPortsEnableChange(this, new ItmPortsEnableChangeArgs(e));
            }
        }

        #endregion
    }
}

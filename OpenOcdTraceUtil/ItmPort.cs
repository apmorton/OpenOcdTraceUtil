using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenOcdTraceUtil
{
    public enum ItmPortType
    {
        Ascii,
        Binary,
        Interval,
        Event
    }

    public class ItmPortTypeMap
    {
        public string Name { get; set; }
        public ItmPortType Value { get; set; }
    }

    public class ItmPort : INotifyPropertyChanged
    {
        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                SetProperty(ref enabled, value);
            }
        }

        private int channel;
        public int Channel
        {
            get { return channel; }
            set
            {
                SetProperty(ref channel, value);
            }
        }

        private Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                SetProperty(ref color, value);
            }
        }

        private ItmPortType type;
        public ItmPortType Type
        {
            get { return type; }
            set
            {
                SetProperty(ref type, value);
            }
        }

        private string stringBuffer;
        public string StringBuffer
        {
            get { return stringBuffer; }
            set
            {
                SetProperty(ref stringBuffer, value);
            }
        }

        private List<byte> binaryBuffer;
        public List<byte> BinaryBuffer
        {
            get { return binaryBuffer; }
            set
            {
                SetProperty(ref binaryBuffer, value);
            }
        }

        private bool binaryBufferInProgress;
        public bool BinaryBufferInProgress
        {
            get { return binaryBufferInProgress; }
            set
            {
                SetProperty(ref binaryBufferInProgress, value);
            }
        }

        private int binaryBufferLength;
        public int BinaryBufferLength
        {
            get { return binaryBufferLength; }
            set
            {
                SetProperty(ref binaryBufferLength, value);
            }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

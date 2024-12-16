using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// DataProcessor eventargs to handle frame data
    /// </summary>
    public class DataProcessorFrameEventArgs : EventArgs
    {
        public Frame Frame { get; internal set; }
    }
}

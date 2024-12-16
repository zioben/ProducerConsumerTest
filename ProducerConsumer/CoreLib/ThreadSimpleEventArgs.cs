using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// Not used.
    /// </summary>
    public class ThreadSimpleEventArgs : EventArgs
    {
        public Thread Thread { get; internal set; }
        public CancellationToken? Token { get; internal set; }
    }
}

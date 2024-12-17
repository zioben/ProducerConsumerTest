using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// EventArgs that contains ThreadSimple execution context
    /// </summary>
    public class ThreadSimpleEventArgs : EventArgs
    {
        /// <summary>
        /// Running thread
        /// </summary>
        public Thread? thread { get; internal set; }

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken? token { get; internal set; }
    }
}

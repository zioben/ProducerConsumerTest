using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// Event message event
    /// </summary>
    public class LoggerEventArgs : EventArgs
    {
        /// <summary>
        /// Event timestamp
        /// </summary>
        public DateTime Timestamp { get; internal set; } = DateTime.Now;

        /// <summary>
        /// Log level
        /// </summary>
        public LogLevel Level { get; internal set; }

        /// <summary>
        /// Log message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Optional exception raised
        /// </summary>
        public Exception Except { get; internal set; }


        public override string ToString()
        {
            return $"{Message}";
        }
    }
}

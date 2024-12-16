using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// State of information 
    /// </summary>
    public enum FrameState
    {
        /// <summary>
        /// Construntor default state
        /// </summary>
        unknown,

        /// <summary>
        /// Data is created by a producer
        /// </summary>
        created,

        /// <summary>
        /// Consumer starts information pricessing 
        /// </summary>
        processing,

        /// <summary>
        /// Consumer completes information processing
        /// </summary>
        processed,

        /// <summary>
        /// Producer drops the information
        /// </summary>
        dropped,

        /// <summary>
        /// Consumer can't process the information
        /// </summary>
        skipped,
        /// <summary>
        /// Consumer can't process the information
        /// </summary>
        rejected
    }

    /// <summary>
    /// Class that represents a data frame for producer-consumer
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// Frame timestamp
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.Now;

        /// <summary>
        /// Frame number
        /// </summary>
        public int FrameID { get; internal set; } = 0;

        /// <summary>
        /// Data payload
        /// </summary>
        public object Payload { get; internal set; }

        /// <summary>
        /// State processing state
        /// </summary>
        public FrameState ProcessingState { get; internal set; } = FrameState.unknown;

        public bool Processed => ProcessingState == FrameState.processed;

        public override string ToString()
        {
            return $"Frame {FrameID} : {Timestamp:HH:mm:ss} : {ProcessingState} : {Payload?.ToString()}";
        }
    }
}

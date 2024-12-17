using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// Class that processes data, simulating random cpu load
    /// </summary>
    public class DataProcessor
    {
        static string sClassName = nameof(DataProcessor);
        Random rand { get; set; } = new Random();

        /// <summary>
        /// Minimum task sleep duration
        /// </summary>
        public int ProcessingMinimumSleep { get; set; } = 500;

        /// <summary>
        /// Random task sleep duration
        /// </summary>
        public int ProcessingRandomSleep { get; set; } = 1000;

        /// <summary>
        /// Processor Identificator
        /// </summary>
        public long ProcessingID { get; set; } = 0;

        /// <summary>
        /// Event called at beginning of processing
        /// </summary>
        public event EventHandler<DataProcessorFrameEventArgs> OnProcessingStart;

        /// <summary>
        /// Event called after processing
        /// </summary>
        public event EventHandler<DataProcessorFrameEventArgs> OnProcessingEnd;

        /// <summary>
        /// Process data frame asynchronously simulating heavy cpu load 
        /// </summary>
        /// <param name="frame">Processing frame</param>
        /// <returns></returns>
        public async Task<Frame> ProcessDataAsync(Frame frame, CancellationToken? token )
        {
            string sMethod = nameof(ProcessDataAsync);
            return await Task.Run( () =>
            {
                try
                {
                    Logger.LogMessage(sClassName, sMethod, $"{ProcessingID} : Start processing {frame}");
                    OnProcessingStart?.Invoke(this, new DataProcessorFrameEventArgs
                    {
                        Frame = frame,
                    });
                    frame.ProcessingState = FrameState.processing;
                    var sleep = (int)(ProcessingMinimumSleep + rand.NextDouble() * ProcessingRandomSleep);
                    Logger.LogMessage(sClassName, sMethod, $"{ProcessingID} : Sleep for {sleep}");
                    int iLoop = sleep / 100;
                    int iLast = sleep % 100;
                    for (int i = 0; i < iLoop; i++)
                    {
                        token?.ThrowIfCancellationRequested();
                        Thread.Sleep(100);
                    }
                    if (iLast > 0)
                    {
                        Thread.Sleep(iLast);
                    }
                    frame.ProcessingState = FrameState.processed;
                    Logger.LogMessage(sClassName, sMethod, $"{ProcessingID} : Frame {frame.FrameID} processing completed");
                    return frame;
                }
                catch (Exception ex)
                {
                    frame.ProcessingState = FrameState.aborted;
                    Logger.LogException(sClassName, sMethod, $"{ProcessingID} : Frame {frame.FrameID} : Exception", ex);
                    return frame;
                }
                finally
                {
                    OnProcessingEnd?.Invoke(this, new DataProcessorFrameEventArgs
                    {
                        Frame = frame,
                    });
                }
            });
        }

        /// <summary>
        /// Process data frame simulating heavy cpu load 
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Frame ProcessData(Frame frame) => ProcessDataAsync(frame, null).Result; 

    }
}

using NLog.LayoutRenderers.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// Class that produce and consume frame data.
    /// <para>
    /// This is a classic producer consumer competition model.<br/>
    /// Producer starts generating frame data after start() call.<br/>
    /// Data is consumed after a getDataAsync() call.<br/>
    /// A call to stop() method blocks the entire producer consumer process.<br/>
    /// </para>
    /// </summary>
    public class DataGenerator
    {
        static string sClassName = nameof(DataGenerator);

        #region properties

        /// <summary>
        /// Producer data creation time rate [ms]
        /// </summary>
        [Browsable(true), Category("Producer"), ReadOnly(true)]
        [Description("Producer data creation time rate [ms]")]
        public uint ProducerTimeout { get; set; } = 500;

        /// <summary>
        /// Processing simulation fixed sleep time [ms]
        /// </summary>
        [Browsable(true), Category("Processing")]
        [Description("Processing simulation fixed sleep time [ms]")]
        public int ProcessorMinimumSleep { get; set; } = 2000;

        /// <summary>
        /// Processing simulation random sleep time [ms]
        /// </summary>
        [Browsable(true), Category("Processing")]
        [Description("Processing simulation random sleep time [ms]")]
        public int ProcessorMaxRandomSleep { get; set; } = 4000;

        /// <summary>
        /// Consumer max parallelism degree
        /// </summary>
        [Browsable(true), Category("Consumer")]
        [Description("Consumer max parallelism degree")]
        public int MaxParallelism { get; set; } = 4;

        /// <summary>
        /// Producer data creation time rate [ms]
        /// </summary>
        [Browsable(true), Category("MVP")]
        [Description("Processin simulation fixed sleep time [ms]")]
        public int MaxQueueViewSize { get; set; } = 5;

        /// <summary>
        /// Total frames produced
        /// </summary>
        [Browsable(true), Category("Producer"), ReadOnly(true)]
        [Description("Total frames produced")]
        public int Produced { get; private set; }

        /// <summary>
        /// Total frames dropped by producer
        /// </summary>
        [Browsable(true), Category("Producer"), ReadOnly(true)]
        [Description("Total frames dropped by producer")]
        public int ProducedDropped { get; private set; }

        /// <summary>
        /// Total frames fetched by consumer 
        /// </summary>
        [Browsable(true), Category("Producer"), ReadOnly(true)]
        [Description("Total frames fetched by consumer")]
        public int ProducedValid { get; private set; }

        /// <summary>
        /// Total frames consumed
        /// </summary>
        [Browsable(true), Category("Consumer"), ReadOnly(true)]
        [Description("Total frames consumed")]
        public int Consumed { get; private set; }

        /// <summary>
        /// Total frames correctly processed
        /// </summary>
        [Browsable(true), Category("Consumer"), ReadOnly(true)]
        [Description("Total frames correctly processed")]
        public int ConsumedValid { get; private set; }

        /// <summary>
        /// Total frames skipped by consumer
        /// </summary>
        [Browsable(true), Category("Consumer"), ReadOnly(true)]
        [Description("Total frames skipped or error")]
        public int ConsumedRejected { get; private set; }


        /// <summary>
        /// Total frames rejected by consumer
        /// </summary>
        [Browsable(true), Category("Consumer"), ReadOnly(true)]
        [Description("Total frames skipped or error")]
        public int ConsumedSkipped { get; private set; }


        /// <summary>
        /// Event called after frame creation to allow data payload population
        /// </summary>
        public event EventHandler<DataProcessorFrameEventArgs> OnEventFrameCreated;

        ThreadSimple? threadProducer;
        ThreadSimple? threadConsumer;

        Stack<Frame>? frameStack;

        ConcurrentQueue<Frame>? viewFrameQueue;
        List<Task<Frame>>? taskProcessingList;
        Random rand = new Random();

        int frameID = 0;
        object lockerProducerConsumer = new object();
        object lockerCounters = new object();

        #endregion

        #region MVP

        /// <summary>
        /// Get the list of lat n processed frame for visualization purposes
        /// </summary>
        /// <returns></returns>
        public List<Frame> GetViewFrameList() => viewFrameQueue?.ToList() ?? new List<Frame>();

        #endregion

        #region resource allcations

        /// <summary>
        /// Allocates resourcers for producer.<br/>
        /// Create the producer thread.<br/>
        /// </summary>
        /// <returns></returns>
        public bool CreateProducer()
        {
            string sMethod = nameof(CreateProducer);
            lock (lockerProducerConsumer)
            {
                DestroyProducer();
                frameID = 0;
                Produced = ProducedDropped = ProducedValid = 0;
                threadProducer = new ThreadSimple()
                {
                    Name = "Producer",
                    WakeupTime = ProducerTimeout
                };
                threadProducer.OnTimeout -= ThreadProducer_OnTimeout;
                threadProducer.OnTimeout += ThreadProducer_OnTimeout;
                frameStack = new Stack<Frame>();
                viewFrameQueue = new ConcurrentQueue<Frame>();
            }
            return true;
        }

        /// <summary>
        /// Abort producer thread.<br/>
        /// Dispose producer resources<br/>
        /// </summary>
        /// <returns></returns>
        bool DestroyProducer()
        {
            string sMethod = nameof(DestroyProducer);
            lock (lockerProducerConsumer)
            {
                threadProducer?.Destroy();
                threadProducer = null;
                frameStack = new Stack<Frame>();
                viewFrameQueue = new ConcurrentQueue<Frame>();
            }
            return true;
        }

        /// <summary>
        /// Allocates resourcers for consumer.<br/>
        /// Create the producer thread.<br/>
        /// </summary>
        bool CreateConsumer()
        {
            string sMethod = nameof(CreateConsumer);
            lock (lockerProducerConsumer)
            {
                DestroyConsumer();
                Consumed = ConsumedValid = ConsumedSkipped = ConsumedRejected = 0;
                taskProcessingList = new List<Task<Frame>>();
                threadConsumer = new ThreadSimple()
                {
                    Name = $"Consumer",
                };
                threadConsumer.OnTrigger -= ThreadConsumer_OnSignal;
                threadConsumer.OnTrigger += ThreadConsumer_OnSignal;
                threadConsumer.Create().Start(true);
            }
            return true;
        }

        /// <summary>
        /// Abort consumer thread.<br/>
        /// Dispose consumer resources<br/>
        /// </summary>
        bool DestroyConsumer()
        {
            string sMethod = nameof(DestroyConsumer);
            lock (lockerProducerConsumer)
            {
                threadConsumer?.Destroy();
                threadConsumer = null;
                // wait for processing list comleted
                if (taskProcessingList != null)
                {
                    Task.WaitAll(taskProcessingList.ToArray());
                }
            }
            return true;
        }

        /// <summary>
        /// Dispose producer and consumer resources
        /// </summary>
        /// <returns></returns>
        public bool Destroy()
        {
            DestroyProducer();
            DestroyConsumer();
            return true;
        }

        #endregion

        #region producer consumer thread job

        private void ThreadProducer_OnTimeout(object? sender, ThreadSimpleEventArgs e)
        {
            string sMethod = nameof(ThreadProducer_OnTimeout);
            var frame = new Frame()
            {
                FrameID = ++frameID,
                ProcessingState = FrameState.created,
            };

            Logger.LogMessage(sClassName, sMethod, $"{threadProducer?.Name} : producing data '{frame}'");
            try
            {
                OnEventFrameCreated?.Invoke(this, new DataProcessorFrameEventArgs() { Frame = frame });
            }
            catch (Exception ex)
            {
                Logger.LogException(sClassName, sMethod, "Exception during payload creation", ex);
                return;
            }

            Produced++;
            while (viewFrameQueue.Count >= MaxQueueViewSize)
            {
                viewFrameQueue.TryDequeue(out var frameOld);
            }
            viewFrameQueue.Enqueue(frame);

            // only one can handle this
            lock (lockerProducerConsumer)
            {
                if (threadConsumer?.Initialized ?? false)
                {
                    frameStack.Push(frame);
                    ProducedValid++;
                    threadConsumer.ThreadSignal();
                }
                else
                {
                    while (frameStack.TryPop(out var oldFrame))
                    {
                        oldFrame.ProcessingState = FrameState.dropped;
                        Logger.LogWarning(sClassName, sMethod, $"{threadProducer?.Name} : missing consumer : dropping frame {oldFrame.FrameID}");
                        ProducedValid--;
                        ProducedDropped++;
                    }
                    frameStack.Push(frame);
                    ProducedValid++;
                }
            }
        }


        Frame? ThreadConsumer_GetLastFrameProduced()
        {
            string sMethod = nameof(ThreadConsumer_GetLastFrameProduced);
            if (!frameStack.TryPop(out var frame))
            {
                if (threadProducer?.Initialized ?? false)
                {
                    Logger.LogWarning(sClassName, sMethod, $"{threadConsumer?.Name} : THREAD SIGNALING RATE TOO HIGH!, but no frames are lost");
                }
                return null;
            }
            while (frameStack.TryPop(out var oldFrame))
            {
                oldFrame.ProcessingState = FrameState.skipped;
                Logger.LogWarning(sClassName, sMethod, $"{threadProducer?.Name} : frame too old : dropping frame {oldFrame.FrameID}");
                lock (lockerCounters)
                {
                    Consumed++;
                    ConsumedSkipped++;
                }
            }
            return frame;
        }

        private Task<Frame> ThreadConsumer_CreateProcessingTaskAsync(Frame frame, CancellationToken? token)
        {
            var dataProcessor = new DataProcessor()
            {
                ProcessingMinimumSleep = ProcessorMinimumSleep,
                ProcessingRandomSleep = ProcessorMaxRandomSleep,
                ProcessingID = DateTime.Now.Ticks
            };
            dataProcessor.OnProcessingEnd +=
            (sender, args) =>
            {
                Consumed++;
                if (token?.IsCancellationRequested ?? true)
                {
                    ConsumedRejected++;
                    return;
                }
                lock (lockerCounters)
                {
                    if (frame.Processed)
                    {
                        ConsumedValid++;
                    }
                    else
                    {
                        ConsumedRejected++;
                    }
                }
            };
            return dataProcessor.ProcessDataAsync(frame, token);
        }

        private void ThreadConsumer_OnSignal(object? sender, ThreadSimpleEventArgs e)
        {
            string sMethod = nameof(ThreadConsumer_OnSignal);
            try
            {
                var lTaskCompleted = taskProcessingList.Where(X => X.IsCompleted).ToList();
                foreach (var task in lTaskCompleted)
                {
                    taskProcessingList.Remove(task);
                }
                Logger.LogMessage(sClassName, sMethod, $"{threadConsumer.Name} : task count = task {taskProcessingList.Count}");

                lock (lockerProducerConsumer)
                {
                    var frame = ThreadConsumer_GetLastFrameProduced();

                    if (frame == null)
                    {
                        return;
                    }

                    if (taskProcessingList.Count >= Math.Max(1, MaxParallelism))
                    {
                        Logger.LogError(sClassName, sMethod, $"{threadConsumer.Name} : can't process frame {frame.FrameID}");
                        frame.ProcessingState = FrameState.rejected;
                        lock (lockerCounters)
                        {
                            Consumed++;
                            ConsumedRejected++;
                        }
                    }
                    else
                    {
                        taskProcessingList.Add(ThreadConsumer_CreateProcessingTaskAsync(frame, e.token));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(sClassName, sMethod, "Exception", ex);
            }
        }


        #endregion

        #region interface


        /// <summary>
        /// Start the producer
        /// </summary>
        /// <returns>true</returns>
        public bool start()
        {
            CreateProducer();
            threadProducer.Create().Start(false);
            return true;
        }

        /// <summary>
        /// Stop the producer/consumer interface
        /// </summary>
        /// <returns>true</returns>
        public bool stop()
        {
            DestroyProducer();
            DestroyConsumer();
            return true;
        }

        /// <summary>
        /// Start consuming data
        /// </summary>
        /// <returns>true</returns>
        public bool getData()
        {
            CreateConsumer();
            return true;    
        }

        #endregion
    }
}
        
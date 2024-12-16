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
    /// Generates and consume data
    /// <para>
    /// Producer starts generating data after start() call.<br/>
    /// </para>
    /// <para>
    /// Data consuming is enabled after a getDataAsync() call.<br/>
    /// </para>
    /// <para>
    /// A call to stop() method blocks entire process.<br/>
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


        ThreadSimple threadProducer;
        ThreadSimple threadConsumer;

        Stack<Frame> frameStack;

        ConcurrentQueue<Frame> viewFrameQueue;
        List<Task<Frame>> taskProcessingList;
        Random rand = new Random();

        int frameID = 0;
        object lockerProducerConsumer = new object();
        object lockerCounters = new object();

        #endregion

        public List<Frame> GetViewFrameList() => viewFrameQueue?.ToList();



        /// <summary>
        /// Allocate resources and producer thread
        /// </summary>
        /// <returns></returns>
        public bool CreateProducer()
        {
            string sMethod = nameof(CreateProducer);
            frameID = 0;
            Produced = ProducedDropped = ProducedValid = 0;
            threadProducer = new ThreadSimple()
            {
                Name = "Producer",
                WakeupTime = ProducerTimeout
            };
            threadProducer.OnTimeout -= ThreadProducer_OnTimeout;
            threadProducer.OnTimeout += ThreadProducer_OnTimeout;
            lock (lockerProducerConsumer)
            {
                frameStack = new Stack<Frame>();
            }
            viewFrameQueue = new ConcurrentQueue<Frame>();
            taskProcessingList = new List<Task<Frame>>();

            return true;
        }

        /// <summary>
        /// Dispose producer resources
        /// </summary>
        /// <returns></returns>
        bool DestroyProducer()
        {
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
        /// Initialize the consumer thread that must operate independently
        /// </summary>
        bool CreateConsumer()
        {
            string sMethod = nameof(CreateConsumer);
            Consumed = ConsumedValid = ConsumedSkipped = ConsumedRejected = 0;
            threadConsumer = new ThreadSimple()
            {
                Name = $"Consumer",
            };
            threadConsumer.OnSignal -= ThreadConsumer_OnSignal;
            threadConsumer.OnSignal += ThreadConsumer_OnSignal;
            threadConsumer.Create();
            return true;
        }

        /// <summary>
        /// Dispose consumer reosurces
        /// </summary>
        bool DestroyConsumer()
        {
            lock (lockerProducerConsumer)
            {
                string sMethod = nameof(DestroyConsumer);
                threadConsumer?.Destroy();
                threadConsumer = null;
                Task.WaitAll(taskProcessingList.ToArray());
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



        #region Task producer consumer 

        private void ThreadProducer_OnTimeout(object? sender, ThreadSimpleEventArgs e)
        {
            string sMethod = nameof(ThreadProducer_OnTimeout);
            var frame = new Frame()
            {
                FrameID = ++frameID,
                ProcessingState = FrameState.created,
                Payload = $"Hello {frameID}",
            };
            Logger.LogMessage(sClassName, sMethod, $"{threadProducer.Name} : producing data '{frame}'");

            Produced++;
            while (viewFrameQueue.Count >= MaxQueueViewSize)
            {
                viewFrameQueue.TryDequeue(out var frameOld);
            }
            viewFrameQueue.Enqueue(frame);

            // only one can handle this
            lock (lockerProducerConsumer)
            {
                if ( threadConsumer?.Initialized ?? false )
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
                        Logger.LogWarning(sClassName, sMethod, $"{threadProducer.Name} : missing consumer : dropping frame {oldFrame.FrameID}");
                        ProducedValid--;
                        ProducedDropped++;
                    }
                    frameStack.Push(frame);
                    ProducedValid++;
                }
            }
        }


        Frame ThreadConsumer_GetLastFrameProduced()
        {
            string sMethod = nameof(ThreadConsumer_GetLastFrameProduced);
            if (!frameStack.TryPop(out var frame))
            {
                if (threadProducer?.Initialized ?? false)
                {
                    Logger.LogWarning(sClassName, sMethod, $"{threadConsumer.Name} : THREAD SIGNALING RATE TOO HIGH!, but no frames are lost");
                }
                return null;
            }
            while (frameStack.TryPop(out var oldFrame))
            {
                oldFrame.ProcessingState = FrameState.skipped;
                Logger.LogWarning(sClassName, sMethod, $"{threadProducer.Name} : frame too old : dropping frame {oldFrame.FrameID}");
                lock (lockerCounters)
                {
                    Consumed++;
                    ConsumedSkipped++;
                }
            }
            return frame;
        }

        private Task<Frame> ThreadConsumer_CreateProcessingTaskAsync(Frame frame)
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
                if (threadConsumer?.cancellationTokenSource?.IsCancellationRequested ?? true)
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
            return dataProcessor.ProcessDataAsync(frame, threadConsumer.cancellationTokenSource);
        }

        private async void ThreadConsumer_OnSignal(object? sender, ThreadSimpleEventArgs e)
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
                        taskProcessingList.Add(ThreadConsumer_CreateProcessingTaskAsync(frame));
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
        /// <returns></returns>
        public bool start()
        {
            CreateProducer();
            threadProducer.Create().Start();
            return true;
        }

        /// <summary>
        /// Stop the interface
        /// </summary>
        /// <returns></returns>
        public bool stop()
        {
            DestroyProducer();
            DestroyConsumer();
            return true;
        }

        /// <summary>
        /// Acquire only last frame
        /// </summary>
        /// <returns></returns>
        public async Task<bool> getDataAsync()
        {
            return await Task.Run(() =>
            {
                string sMethod = nameof(getDataAsync);
                try
                {
                    CreateConsumer();
                    while (true)
                    {
                        switch (threadConsumer?.WaitForSignal(0) ?? ThreadSimple.EnumSignalType.Quit)
                        {
                            case ThreadSimple.EnumSignalType.Exception:
                                {
                                    Logger.LogError(sClassName, sMethod, "Detected exception on consumer");
                                    return false;
                                }
                            case ThreadSimple.EnumSignalType.Quit:
                                {
                                    Logger.LogMessage(sClassName, sMethod, "Detected quit signal on consumer");
                                    return true;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(sClassName, sMethod, "Exception raised", ex);
                    return false;
                }
                finally
                {
                    DestroyConsumer();
                }
            });

            #endregion
        }
    }
}


        /*
                /// <summary>
                /// Acquire ucontinuosly using producer consumer mechanism
                /// </summary>
                /// <returns></returns>
                public async Task<bool> getDataAsync()
                {
                    string sMethod = nameof(getDataAsync);
                    return await Task<bool>.Run(async () =>
                    {
                        try
                        {
                            CreateConsumer();
                            Frame frame = null;
                            lock (lockerProducerConsumer)
                            {
                                frame = ThreadConsumer_GetLastFrameProduced();
                            }
                            if (frame == null)
                            {
                                if (threadConsumer.WaitForSignal(0) == ThreadSimple.EnumSignalType.Trigger)
                                {
                                    lock (lockerProducerConsumer)
                                    {
                                        frame = ThreadConsumer_GetLastFrameProduced();
                                    }
                                }
                            }
                            if (frame != null)
                            {
                                await Task<Frame>.Run(() => ThreadConsumer_CreateProcessingTaskAsync(frame));
                                DestroyConsumer();
                                return true;
                            }
                            return false;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(sClassName, sMethod, "Exception raised", ex);
                            return false;
                        }
                    });
                }

                #endregion
            }
            }*/
    
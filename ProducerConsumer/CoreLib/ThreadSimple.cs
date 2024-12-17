using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CoreLib
{
    /// <summary>
    /// Simple Thread Handler<br/> 
    /// <para>
    /// This class provides a synchronization event mechanism for simple operations.<br/>
    /// Only three AutoResetEvent are provided:<br/>
    /// <list type="bullet">Trigger Signal - raised by a source (task,thread, another async caller method) to communicate that a condition occours</list>
    /// <list type="bullet">Quit Signal - raised by another task od by the destructor to inform about the abort of the source</list>
    /// <list type="bullet">Tiemout Signal (when used) - raised periodically</list>
    /// </para>
    /// <para>
    /// This class can also manage automatically a Thread for long time operations<br/>
    /// The created task runs and wait for any signal that can raise a .net event accordingly<br/>
    /// <seealso cref="ThreadSimple.OnTrigger"/> <seealso cref="ThreadSimple.OnQuit"/> <seealso cref="ThreadSimple.OnTimeout"/><br/>
    /// This is the core class for the producer consumer implementation<br/>
    public class ThreadSimple
    {
        static readonly string sClassName = nameof(ThreadSimple);

        /// <summary>
        /// Thread signal maps
        /// </summary>
        public enum EnumSignalType
        {
            /// <summary>
            /// Unknown or unmapped signal
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Operation aborted due to exception during AutoresetEvent handling
            /// </summary>
            Exception = 1,
            /// <summary>
            /// Quit AutoresetEvent is set
            /// </summary>
            Quit = 2,
            /// <summary>
            /// Trigger AutoresetEvent is set
            /// </summary>
            Trigger = 3,
            /// <summary>
            /// Timer polling AutoresetEvent is set
            /// </summary>
            Timeout = 4,    
        }
     
        // Microsoft specifications : for Long time running operations is suggested to use Thread instead of Task
        Thread? oThread;

        /// <summary>
        /// Thread Identifier
        /// </summary>
        public string Name { get; set; } = "*Thread*";
     
        readonly AutoResetEvent oSignalExecute = new AutoResetEvent(false);
        readonly AutoResetEvent oSignalQuit = new AutoResetEvent(false);
        readonly AutoResetEvent oSignalWakeupRestart = new AutoResetEvent(false);

        /// <summary>
        /// Cancellation oken to notify chain processing that the Thread is about to abort
        /// </summary>
        CancellationTokenSource cancellationTokenSource= new CancellationTokenSource();
        
        uint iWakeupTime = 1000;

        /// <summary>
        /// Setup polling time. Set to Zero to disable (Infinite wait time)
        /// </summary>
        public uint WakeupTime
        {
            get
            {
                return iWakeupTime;
            }
            set
            {
                iWakeupTime = value;
                // Set the dummy event to wake-up the thread and restart polling timer
                oSignalWakeupRestart.Set();
            }
        }

        /// <summary>
        /// Notify that resources are allocated
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Notify that the thread is running
        /// </summary>
        public bool Running => oThread?.IsAlive ?? false;


        /// <summary>
        /// Raised when external trigger event rises 
        /// </summary>
        public event EventHandler<ThreadSimpleEventArgs> OnTrigger;

        /// <summary>
        /// Raised on timeout
        /// </summary>
        public event EventHandler<ThreadSimpleEventArgs> OnTimeout;

        /// <summary>
        /// Raised when quit event rises 
        /// </summary>
        public event EventHandler OnQuit;

        /// <summary>
        /// Initialize thread infrastructure and start working thread
        /// </summary>
        public ThreadSimple Create()
        {
            string sMethod = nameof(Create);
            Destroy();
            cancellationTokenSource = new CancellationTokenSource();   
            oSignalExecute.Reset();
            oSignalQuit.Reset();
            oSignalWakeupRestart.Reset();
            Initialized = true; 
            return this;
        }

        /// <summary>
        /// Start automatic thread job
        /// </summary>
        /// <para>
        /// Thread is created and runs, looping and waiting for one AutoResetEvent signal
        /// </para>
        /// <param name="triggerSiganled">Raise trigger signal immediately</param>
        /// <returns></returns>
        public ThreadSimple? Start(bool triggerSignaled)
        {
            string sMethod = nameof (Start);
            if (!Initialized)
            {
                Logger.LogError(sClassName, sMethod, $"{Name} : class not initialized");
                return null;
            }
            if (triggerSignaled)
            {
                ThreadSignal();
            }
            oThread = new Thread(ThreadJob);
            oThread.IsBackground = true;
            oThread.Start();
            return this;
        }

        /// <summary>
        /// Kill working thread
        /// </summary>
        public ThreadSimple Destroy()
        {
            string sMethod = nameof(Destroy);
            ThreadQuit();
            if (oThread != null && oThread.IsAlive)
            {
                if (!oThread.Join(5000))
                {
                    Logger.LogError(sClassName, sMethod, $"Task {Name} Doesn't stop");
                }
            }
            oThread = null;
            Initialized = false;
            return this;
        }

        /// <summary>
        /// Signal trigger on working thread
        /// </summary>
        public void ThreadSignal()
        {
            oSignalExecute?.Set();
        }

        /// <summary>
        /// Raise quit trigger on working thread
        /// </summary>
        public void ThreadQuit()
        {
            cancellationTokenSource?.Cancel();
            oSignalQuit?.Set();
        }

        #region Working thread code

      

        /// <summary>
        /// The thread loop job
        /// </summary>
        void ThreadJob()
        {
            string sMethod = nameof(ThreadJob);
            bool bExit = false;
            Logger.LogMessage(sClassName, sMethod, $"{Name} : Starting thread");
            try
            {
                AutoResetEvent[] oEvents = { oSignalQuit, oSignalExecute, oSignalWakeupRestart };
                while (!bExit)
                {
                    int iTime = iWakeupTime == 0 ? Timeout.Infinite : (int)iWakeupTime;
                    int iEventID = WaitHandle.WaitAny(oEvents, iTime);
                    {
                        bExit = iEventID == 0;
                        if (!Running)
                        {
                            continue;
                        }
                        ProcessEvents(iEventID);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(sClassName, sMethod, $"{Name} : Exception", ex);
            }
            Logger.LogMessage(sClassName, sMethod, $"{Name} : Thread end");
        }

        /// <summary>
        /// Processes AutoReseEvent ID and raises events accordingly
        /// </summary>
        /// <param name="iEventID"></param>
        /// <returns></returns>
        EnumSignalType ProcessEvents( int iEventID )
        {
            string sMethod = nameof(ProcessEvents); 
            switch (iEventID)
            {
                case 0:
                    {
                        Logger.LogMessage(sClassName, sMethod, $"{Name} : Detected quit signal");
                        try
                        {
                            OnQuit?.Invoke(this, EventArgs.Empty);
                            return  EnumSignalType.Quit;    
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(sClassName, sMethod, $"{Name} : Exception on QuitSignal", ex);
                            return  EnumSignalType.Exception;   
                        }
                    }
                case 1:
                    {
                        try
                        {
                            OnTrigger?.Invoke(this, new ThreadSimpleEventArgs()
                            {
                                thread = oThread,
                                token = cancellationTokenSource.Token,
                            });
                            return  EnumSignalType.Trigger;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(sClassName, sMethod, $"{Name} : Exception on Trigger", ex);
                            return  EnumSignalType.Exception;
                        }
                    }
                case 2:
                default:
                    {
                        try
                        {
                            OnTimeout?.Invoke(this, new ThreadSimpleEventArgs()
                            {
                                thread = oThread,
                                token = cancellationTokenSource.Token,
                            });
                            return  EnumSignalType.Timeout;    
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(sClassName, sMethod, $"{Name} : Exception on Timeout", ex);
                            return  EnumSignalType.Exception;
                        }
                    }
            }
        }
        #endregion

        /// <summary>
        /// Wait for an AutoResetEvent signal.
        /// <para>Raises events accordingly </para> 
        /// <para>Any method caller will block until a signal comes</para> 
        /// </summary>
        /// <param name="iWakeupTime">maximum timeout delay: 0 for infinite timeout</param>
        /// <returns></returns>
        public EnumSignalType WaitForSignal(int iWakeupTime)
        {
            string sMethod = nameof(WaitForSignal);
            Logger.LogMessage(sClassName, sMethod, $"{Name} : Waiting for event");
            try
            {
                AutoResetEvent[] oEvents = { oSignalQuit, oSignalExecute, oSignalWakeupRestart };
                int iTime = iWakeupTime == 0 ? Timeout.Infinite : (int)iWakeupTime;
                return ProcessEvents(WaitHandle.WaitAny(oEvents, iTime));
            }
            catch (Exception ex)
            {
                Logger.LogException(sClassName, sMethod, $"{Name} : Exception", ex);
                return  EnumSignalType.Exception;
            }
        }

    }
}

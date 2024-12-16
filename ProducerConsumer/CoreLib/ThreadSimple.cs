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
    /// Once started can raise by a timeout or a trigger event
    /// </summary>
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
            /// Operation aborted due to exception
            /// </summary>
            Exception = 1,
            /// <summary>
            /// Quit signal
            /// </summary>
            Quit = 2,
            /// <summary>
            /// Trigger signal
            /// </summary>
            Trigger = 3,
            /// <summary>
            /// Timing signal
            /// </summary>
            Timeout = 4,    
        }
     
        // Microsoft specifications : for Long time running operations is suggested to use Thread instead of Task
        Thread oThread;
        public string Name { get; set; } = "*Thread*";
     
        readonly AutoResetEvent oSignalExecute = new AutoResetEvent(false);
        readonly AutoResetEvent oSignalQuit = new AutoResetEvent(false);
        readonly AutoResetEvent oSignalWakeupRestart = new AutoResetEvent(false);

        public CancellationTokenSource cancellationTokenSource { get; private set; }
        
        public uint iWakeupTime = 1000;
        public uint WakeupTime
        {
            get
            {
                return iWakeupTime;
            }
            set
            {
                iWakeupTime = value;
                oSignalWakeupRestart.Set();
            }
        }

        public bool Initialized { get; private set; }

        public bool Running
        {
            get
            {
                if (oThread == null)
                    return false;
                return oThread.IsAlive;
            }
        }

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
        /// Start thread job
        /// </summary>
        /// <returns></returns>
        public ThreadSimple Start()
        {
            string sMethod = nameof (Start);
            if (!Initialized)
            {
                Logger.LogError(sClassName, sMethod, $"{Name} : class not initialized");
                return null;
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

        public event EventHandler<ThreadSimpleEventArgs> OnSignal;
        public event EventHandler<ThreadSimpleEventArgs> OnTimeout;
        public event EventHandler OnQuit;

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
                            OnSignal?.Invoke(this, new ThreadSimpleEventArgs()
                            {
                                Thread = oThread,
                                Token = cancellationTokenSource.Token,
                            });
                            return  EnumSignalType.Trigger;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(sClassName, sMethod, $"{Name} : Exception on Trigger", ex);
                            return  EnumSignalType.Exception;
                        }
                    }
                default:
                    {
                        try
                        {
                            OnTimeout?.Invoke(this, new ThreadSimpleEventArgs()
                            {
                                Thread = oThread,
                                Token = cancellationTokenSource.Token,
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
        /// Wait for a notification
        /// </summary>
        /// <param name="iWakeupTime"></param>
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

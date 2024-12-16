using CoreLib;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace WinApp
{
    public partial class FormHMI : Form
    {
        /// <summary>
        /// Application context
        /// </summary>
        App oApp;

        /// <summary>
        /// Log message queue
        /// </summary>
        ConcurrentQueue<LoggerEventArgs> logQueue = new ConcurrentQueue<LoggerEventArgs>();

        public FormHMI()
        {
            InitializeComponent();
            Initialize();
        }


        private void Logger_OnLogMessage(object? sender, LoggerEventArgs e)
        {
            logQueue.Enqueue(e);
        }

        /// <summary>
        /// Create application and initialize HMI
        /// </summary>
        public void Initialize()
        {
            Logger.Create();
            Logger.OnLogMessage += Logger_OnLogMessage;
            oApp = new App();
            oApp.Create();
            pgConfig.SelectedObject = oApp.Config;
            lvFrames.Columns.Clear();
            lvFrames.Columns.Add(nameof(Frame.FrameID), 100);
            lvFrames.Columns.Add(nameof(Frame.Timestamp), 100);
            lvFrames.Columns.Add(nameof(Frame.ProcessingState), 100);
            lvFrames.Columns.Add(nameof(Frame.Payload), 200);
            pgDataGen.Enabled = false;
        }

        /// <summary>
        /// Updates Listbox with log messages
        /// </summary>
        void RefreshLogMessages()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { RefreshLogMessages(); }));
            }
            else
            {
                while (logQueue.TryDequeue(out var eventMessage))
                {
                    lbLog.Items.Insert(0, eventMessage.Message.ToString());
                }
                while (lbLog.Items.Count > 200)
                {
                    lbLog.Items.RemoveAt(199);
                }
            }
        }

        /// <summary>
        /// Updates Listview with ProducerConsumer items
        /// </summary>
        void RefreshProcessingView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { RefreshProcessingView(); }));
            }
            else
            {
                try
                {
                    lvFrames.Items.Clear();
                    var lData = oApp.DataGenerator.GetViewFrameList();
                    if (lData == null)
                    {
                        return;
                    }
                    foreach (var frame in lData)
                    {
                        var lvItem = new ListViewItem(new string[]
                            {
                            $"{frame.FrameID}",
                            $"{frame.Timestamp:HH:mm:ss}",
                            $"{frame.ProcessingState}",
                            $"{frame.Payload}"
                            });
                        switch (frame.ProcessingState)
                        {
                            case FrameState.created:
                                {
                                    lvItem.BackColor = Color.Orange;
                                    lvItem.ForeColor = Color.Black;
                                    break;
                                }
                            case FrameState.dropped:
                                {
                                    lvItem.BackColor = Color.DarkRed;
                                    lvItem.ForeColor = Color.Black;
                                    break;
                                }
                            case FrameState.processed:
                                {
                                    lvItem.BackColor = Color.Green;
                                    lvItem.ForeColor = Color.White;
                                    break;
                                }
                            case FrameState.processing:
                                {
                                    lvItem.BackColor = Color.DarkGreen;
                                    lvItem.ForeColor = Color.LightGray;
                                    break;
                                }
                            case FrameState.rejected:
                                {
                                    lvItem.BackColor = Color.Red;
                                    lvItem.ForeColor = Color.Yellow;
                                    break;
                                }
                            case FrameState.skipped:
                                {
                                    lvItem.BackColor = Color.DarkRed;
                                    lvItem.ForeColor = Color.Yellow;
                                    break;
                                }
                            case FrameState.unknown:
                                {
                                    lvItem.BackColor = Color.Gray;
                                    lvItem.ForeColor = Color.Black;
                                    break;
                                }
                        }
                        lvFrames.Items.Add(lvItem);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Refresh ProducerConsumer data and counters
        /// </summary>
        void RefreshProcessorGrid()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { RefreshProcessorGrid(); }));
            }
            else
            {
                pgDataGen.SelectedObject = oApp.DataGenerator;
            }
        }

        /// <summary>
        /// Timer event to refresh HMI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            try
            {
                SuspendLayout();
                timerRefresh.Stop();
                RefreshLogMessages();
                RefreshProcessingView();
                RefreshProcessorGrid();
            }
            catch
            {
            }
            finally
            {
                ResumeLayout();
                timerRefresh.Start();
            }
        }

        

        private void btnStart_Click(object sender, EventArgs e)
        {
            oApp.start();
            pgConfig.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            oApp.stop();
            pgConfig.Enabled = true;
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            oApp.getData();
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            oApp.Destroy();
            oApp.Create();
        }

        private void FormHMI_FormClosed(object sender, FormClosedEventArgs e)
        {
            oApp.stop();
        }
    }
}

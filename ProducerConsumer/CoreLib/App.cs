using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    /// <summary>
    /// Main application interface
    /// </summary>
    public class App
    {
        static string  sClassName = nameof(App);

        /// <summary>
        /// Application configuration class parameters
        /// </summary>
        public class ApplicationConfig
        {
            /// <summary>
            /// Producer data creation time rate [ms]
            /// </summary>
            [Browsable(true), Category("Producer")]
            [Description("Producer data creation time rate [ms]")]
            public uint ProducerTimeout { get; set; } = 1000;

            /// <summary>
            /// Processing simulation fixed sleep time [ms]
            /// </summary>
            [Browsable(true), Category("Processing")]
            [Description("Processing simulation fixed sleep time [ms]")]
            public int ProcessorMinimumSleep { get; set; } = 500;

            /// <summary>
            /// Processing simulation random sleep time [ms]
            /// </summary>
            [Browsable(true), Category("Processing")]
            [Description("Processing simulation random sleep time [ms]")]
            public int ProcessorMaxRandomSleep { get; set; } = 1000;

            /// <summary>
            /// Consumer max parallelism degree
            /// </summary>
            [Browsable(true), Category("Consumer")]
            [Description("Consumer max parallelism degree")]
            public int MaxParallelism { get; set; } = 1;

            /// <summary>
            /// Producer data creation time rate [ms]
            /// </summary>
            [Browsable(true), Category("MVP")]
            [Description("Processin simulation fixed sleep time [ms]")]
            public int MaxQueueViewSize { get; set; } = 25;

            
        }

        /// <summary>
        /// Application configuration
        /// </summary>
        public ApplicationConfig Config { get; set; }=new ApplicationConfig();

        /// <summary>
        /// The data generator instance
        /// </summary>
        public DataGenerator DataGenerator { get; private set; }

        /// <summary>
        /// Allocate resources
        /// </summary>
        /// <returns></returns>
        public void Create()
        {
            string sMethod = nameof(Create);
            Destroy();
            ThreadPool.SetMinThreads(200,200);
            ThreadPool.SetMaxThreads(300,300);
            Logger.LogMessage(sClassName, sMethod, "Starting main App");
            DataGenerator = new DataGenerator()
            {
                MaxParallelism = Config.MaxParallelism,
                MaxQueueViewSize = Config.MaxQueueViewSize,
                ProcessorMaxRandomSleep = Config.ProcessorMaxRandomSleep,
                ProcessorMinimumSleep = Config.ProcessorMinimumSleep,
                ProducerTimeout = Config.ProducerTimeout,
            };
            DataGenerator.CreateProducer();
            Logger.LogMessage(sClassName, sMethod, "App Started");
        }

        /// <summary>
        /// Free reosurces
        /// </summary>
        public void Destroy()
        {
            string sMethod = nameof(Destroy);
            if (DataGenerator != null)
            {
                Logger.LogMessage(sClassName, sMethod, "Stopping App");
                DataGenerator.stop();
                DataGenerator.Destroy();
                Logger.LogMessage(sClassName, sMethod, "App stopped");
            }
        }

        /// <summary>
        /// Start producer
        /// </summary>
        /// <returns></returns>
        public bool start() => DataGenerator.start();   

        /// <summary>
        /// Stop producer/consumer
        /// </summary>
        /// <returns></returns>
        public bool stop() => DataGenerator.stop();

        /// <summary>
        /// Start consumer
        /// </summary>
        public void getData() => DataGenerator.getDataAsync();
    }
}

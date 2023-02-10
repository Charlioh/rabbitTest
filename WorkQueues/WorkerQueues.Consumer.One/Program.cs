using System.ServiceProcess;

namespace WorkerQueues.Consumer.One
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ConsumerOne()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

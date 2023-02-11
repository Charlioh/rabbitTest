using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace Consum.Service
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
                new Consumer()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

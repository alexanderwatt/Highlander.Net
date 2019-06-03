using System.ServiceProcess;

namespace Orion.MDAS.WinSvc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            var servicesToRun = new ServiceBase[] { new MarketDataService() };
            ServiceBase.Run(servicesToRun);
        }
    }
}
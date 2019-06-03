﻿using System.ServiceProcess;

namespace CurveGenWinSvc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
                { 
                    new CurveGenService() 
                };
            ServiceBase.Run(servicesToRun);
        }
    }
}
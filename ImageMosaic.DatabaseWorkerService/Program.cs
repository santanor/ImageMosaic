using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using ImageMosaic.ImageProcessing;
using System.Threading.Tasks;
using ImageMosaic.Domain.Model;
using ImageMosaic.DatabaseWorkerService;

namespace ImageMosaic.DatabaseWorker
{
    static class Worker
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var service = new WorkerService();
            service.Run();
        }
    }
}

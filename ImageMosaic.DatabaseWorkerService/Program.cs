using System;
using ImageMosaic.DatabaseWorkerService;

namespace ImageMosaic.DatabaseWorker
{
    internal static class Worker
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            try
            {
                var service = new WorkerService();
                service.Run();
            }
            catch (Exception)
            {
            }
        }
    }
}
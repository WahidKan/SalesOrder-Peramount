using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.NetworkInformation;
using System.Timers;
using Timer = System.Threading.Timer;
using System.Net.Http;
using SalesOrder_Paramount.Controllers;

namespace SalesOrder_Paramount.Service
{
    public class DIService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly ILogger<DIService> logger;
        private const int generalDelay = 1 * 10 * 1000; // 10 seconds
        public DIService(ILogger<DIService> logger)
        {
            this.logger = logger;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Background Service Stoped");
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Delay(300000);
            //var client = new HttpClient();
            //client.Timeout = TimeSpan.FromMinutes(30);
            //client.GetAsync("https://localhost:5001/SalesOrder");

            logger.LogInformation($"Background Service Started");
            timer = new Timer(async o =>
            {
                //var client = new HttpClient();
                //client.Timeout = TimeSpan.FromMinutes(60);
                //var resp = await client.GetAsync("http://localhost:8080/salesorder");
                //if (resp.StatusCode.Equals(200))
                //{
                //    logger.LogInformation($"API Process completed for 100 records");
                //}

                //logger.LogInformation($"Background Service");
            },
      null,
      TimeSpan.Zero,
      TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}


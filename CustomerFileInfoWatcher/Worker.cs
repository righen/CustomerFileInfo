using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerFileInfoWatcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IChangeToken _fileChangeToken;
        private PhysicalFileProvider _fileProvider;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Customer File Info Watcher running at: {time}", DateTimeOffset.Now);
                
                int delay = 5000;

                try
                {
                    string username = Environment.UserName;

                    _fileProvider = new PhysicalFileProvider(@$"C:\print\{username}");

                    WatchForFileChanges();
                }
                catch (Exception)
                {
                    delay = delay * 2;
                }

                await Task.Delay(delay, stoppingToken);
            }
        }

        private void WatchForFileChanges()
        {
            _fileChangeToken = _fileProvider.Watch("*.pdf*");
            _fileChangeToken.RegisterChangeCallback(Notify, default);
        }


        private void Notify(object state)
        {
            _logger.LogInformation("File change detected at: {time}", DateTimeOffset.Now);

            _logger.LogInformation("Sending email to user: {time}", DateTimeOffset.Now);

            WatchForFileChanges();
        }
    }
}

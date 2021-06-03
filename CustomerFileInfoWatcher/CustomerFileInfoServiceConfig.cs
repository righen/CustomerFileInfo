using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerFileInfoWatcher
{
    public class CustomerFileInfoServiceConfig : BackgroundService
    {
        private readonly ILogger<CustomerFileInfoServiceConfig> _logger;
        private IChangeToken _fileChangeToken;
        private PhysicalFileProvider _fileProvider;
        private readonly IConfiguration _configuration;

        public CustomerFileInfoServiceConfig(ILogger<CustomerFileInfoServiceConfig> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Customer File Info Service running at: {time}", DateTimeOffset.Now);

                int delay = Convert.ToInt32(_configuration["CustomerFileInfoServiceConfig:Delay"]);

                try
                {
                    string username = Environment.UserName;

                    _fileProvider = new PhysicalFileProvider(@$"{_configuration["CustomerFileInfoServiceConfig:DirectoryPath"]}{username}");

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
            //create email service
            _logger.LogInformation("File change detected at: {time}", DateTimeOffset.Now);

            _logger.LogInformation("Sending email to user: {time}", DateTimeOffset.Now);

            WatchForFileChanges();
        }
    }
}

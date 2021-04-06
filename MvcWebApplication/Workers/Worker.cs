using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UrlChecker.Web.Models;

namespace UrlChecker.Web
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient httpClient;
        private List<Task> workerList;

        public Worker(ILogger<Worker> logger)
        {
            workerList = new List<Task>();
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            httpClient = new HttpClient();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
                _logger.LogInformation("ExecuteAsync: {time}", DateTimeOffset.Now);
                await Task.WhenAll(workerList.ToArray());
            //}
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            var result = await httpClient.GetAsync(_URI);

        //            if (result.IsSuccessStatusCode)
        //            {
        //                _logger.LogInformation("The {0} is Up. Status Code {1}", _URI, result.StatusCode);
        //            }
        //            else
        //            {
        //                _logger.LogError("The {0} is Down. Status Code {1}", _URI, result.StatusCode);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("The Website is Down {0}.", ex.Message);

        //            MailGonderAsync();
        //        }
        //        finally
        //        {
        //            await Task.Delay(_settings.Interval, stoppingToken);
        //        }

        //        //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //    }
        //}

        public void AddNewTask(string uri, int interval)
        {
            workerList.Add(
                CheckUrlAsync(interval, uri, new CancellationToken()));

            _logger.LogInformation("{0} task added", uri);
        }

        public void RemoveTasks()
        {
            workerList.Clear();

            _logger.LogInformation("{0} tasks restarted");
        }

        public async Task CheckUrlAsync(int interval, string uri, CancellationToken stoppingToken)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("CheckUrlAsync: {time}", DateTimeOffset.Now);

                try
                {
                    var result = await httpClient.GetAsync(uri);

                    if (result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("The {0} is Up. Status Code {1}", uri, result.StatusCode);
                    }
                    else
                    {
                        _logger.LogError("The {0} is Down. Status Code {1}", uri, result.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("The Website is Down {0}.", ex.Message);

                    MailGonderAsync();
                }
                finally
                {
                    await Task.Delay(interval, stoppingToken);
                }
            }

            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }


        public void MailGonderAsync()
        {
            //
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            httpClient.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}

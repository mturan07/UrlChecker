using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UrlChecker.Web.Data;
using UrlChecker.Web.Models;
using UrlChecker.Web.Models.Abstract;
using UrlChecker.Web.Models.Concrete;

namespace UrlChecker.Web.Controllers
{
    public class TargetAppsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Worker> _logger;
        private ISenderService _senderService;

        //private IBackgroundQueue _queue;
        //private readonly Worker _worker;

        public TargetAppsController(ApplicationDbContext context, ILogger<Worker> logger)
        {
            _context = context;
            _logger = logger;
            _senderService = new EmailService();
        }

        private async Task ListeyiYenile()
        {
            List<TargetApp> targetApps = await _context.TargetApps.ToListAsync();

            foreach (var app in targetApps)
            {                                                              // app.Interval
                RecurringJob.AddOrUpdate(app.AppName, () => PingUrl(app.AppUrl), Cron.Minutely);

                //_worker.AddNewTask(app.AppUrl, app.Interval);
            }
        }

        private void AddTargetAppToHangfire(TargetApp app)
        {
            //TODO: app interval -> cron

            RecurringJob.AddOrUpdate(app.AppName, () => PingUrl(app.AppUrl), app.Interval);
        }

        private void RemoveTargetAppFromHangfire(string appName)
        {
            RecurringJob.RemoveIfExists(appName);
        }

        // GET: TargetApps
        [Authorize]
        public async Task<IActionResult> Index()
        {            
            _senderService.Send("info@ethereal.email", "alverta72@ethereal.email", "Mail Test", "Test mail content");

            List<TargetApp> targetApps = await _context.TargetApps.ToListAsync();

            foreach (var app in targetApps)
            {
                AddTargetAppToHangfire(app);

                //_worker.AddNewTask(app.AppUrl, app.Interval);
            }

            return View(targetApps);
        }

        // GET: TargetApps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetApp = await _context.TargetApps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (targetApp == null)
            {
                return NotFound();
            }

            return View(targetApp);
        }

        // GET: TargetApps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TargetApps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppName,AppUrl,Interval")] TargetApp targetApp)
        {
            if (ModelState.IsValid)
            {
                // Target App adding to db
                _context.Add(targetApp);
                await _context.SaveChangesAsync();

                //_queue.QueueTask(async token =>
                //{
                //    await CheckUrlAsync(targetApp.Interval, targetApp.AppUrl, token);
                //});

                //_worker.AddNewTask(targetApp.AppUrl, targetApp.Interval);

                AddTargetAppToHangfire(targetApp);

                return RedirectToAction(nameof(Index));
            }
            return View(targetApp);
        }

        public async Task CheckUrlAsync(int interval, string uri, CancellationToken stoppingToken)
        {
            HttpClient httpClient = new HttpClient();
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

                httpClient.Dispose();
            }

            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        // GET: TargetApps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetApp = await _context.TargetApps.FindAsync(id);
            if (targetApp == null)
            {
                return NotFound();
            }
            return View(targetApp);
        }

        // POST: TargetApps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppName,AppUrl,Interval")] TargetApp targetApp)
        {
            if (id != targetApp.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(targetApp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TargetAppExists(targetApp.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(targetApp);
        }

        // GET: TargetApps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var targetApp = await _context.TargetApps
                .FirstOrDefaultAsync(m => m.Id == id);
            if (targetApp == null)
            {
                return NotFound();
            }

            return View(targetApp);
        }

        // POST: TargetApps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var targetApp = await _context.TargetApps.FindAsync(id);

            RemoveTargetAppFromHangfire(targetApp.AppName);

            _context.TargetApps.Remove(targetApp);
            await _context.SaveChangesAsync();

            //_worker.RemoveTasks();

            return RedirectToAction(nameof(Index));
        }

        private bool TargetAppExists(int id)
        {
            return _context.TargetApps.Any(e => e.Id == id);
        }

        // TODO: Move these methods to one class
        public void PingUrl(string url)
        {
            var png = new Ping();
            try
            {
                Task.Run(() => {

                    var png = new Ping();

                    var reply = png.Send(url);
                    if (reply.Status == IPStatus.Success)
                    {
                        _logger.LogInformation("{url} is up : {time}", url, DateTimeOffset.Now);
                    }
                    else
                    if (reply.Status == IPStatus.DestinationHostUnreachable)
                    {
                        _logger.LogInformation("{url} is down : {time}", url, DateTimeOffset.Now);

                        MailGonderAsync();
                    }
                    else
                    if (!(reply.Status == IPStatus.Success))
                    {
                        _logger.LogInformation("{url} has problems {code} : {time}", url, DateTimeOffset.Now, reply.Status.ToString());
                    }
                });
            }
            catch { }
        }

        public void MailGonderAsync()
        {
            //
        }
    }
}

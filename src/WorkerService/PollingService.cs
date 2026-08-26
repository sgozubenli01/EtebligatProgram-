using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtNotif.Libs.ApiClient;
using EtNotif.Libs.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtNotif.Worker
{
    public class PollingOptions
    {
        public System.Collections.Generic.List<string> Times { get; set; } = new();
        public bool RunImmediateOnStart { get; set; } = true;
    }

    public class PollingService : BackgroundService
    {
        private readonly ILogger<PollingService> _logger;
        private readonly IServiceProvider _services;
        private readonly PollingOptions _options;
        private readonly IGibClient _gibClient;

        public PollingService(ILogger<PollingService> logger, IServiceProvider services, IOptions<PollingOptions> options, IGibClient gibClient)
        {
            _logger = logger;
            _services = services;
            _options = options.Value;
            _gibClient = gibClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PollingService started.");
            var schedule = _options.Times
                .Select(t => TimeSpan.Parse(t))
                .OrderBy(t => t)
                .ToArray();

            if (_options.RunImmediateOnStart)
            {
                await RunPollingCycle(stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now.TimeOfDay;
                var deltas = schedule.Select(s => (s - now + TimeSpan.FromDays(1)) % TimeSpan.FromDays(1)).OrderBy(d => d).ToArray();
                var next = deltas.First();
                _logger.LogInformation("Next polling in {delay}", next);
                try
                {
                    await Task.Delay(next, stoppingToken);
                }
                catch (TaskCanceledException) { break; }

                await RunPollingCycle(stoppingToken);
            }
        }

        private async Task RunPollingCycle(CancellationToken ct)
        {
            _logger.LogInformation("Running polling cycle at {time}", DateTime.Now);
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var taxpayers = await db.Taxpayers.Where(t => t.Enabled).ToListAsync(ct);

                foreach (var t in taxpayers)
                {
                    try
                    {
                        var pwd = EtNotif.Libs.Security.CryptoHelper.UnprotectFromBase64(t.EncryptedPassword);
                        var ok = await _gibClient.AuthenticateAsync(t.Vkn, pwd);
                        if (!ok)
                        {
                            _logger.LogWarning("Auth failed for {vkn}", t.Vkn);
                            continue;
                        }

                        var notes = await _gibClient.GetNotificationsAsync(t.Vkn);
                        foreach (var n in notes)
                        {
                            _logger.LogInformation("Found notification for {vkn}: {title} at {date}", t.Vkn, n.Title, n.Date);
                        }

                        t.LastCheckedAt = DateTime.Now;
                        db.Update(t);
                        await db.SaveChangesAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing taxpayer {vkn}", t.Vkn);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polling cycle failed");
            }
        }
    }
}

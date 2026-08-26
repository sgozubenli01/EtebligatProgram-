using System;
using EtNotif.Libs.ApiClient;
using EtNotif.Libs.Data;
using EtNotif.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<PollingOptions>(ctx.Configuration.GetSection("Polling"));
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(ctx.Configuration.GetConnectionString("Default")));
        services.AddSingleton<IGibClient, MockGibClient>();
        services.AddHostedService<PollingService>();
    })
    .ConfigureLogging((ctx, logging) =>
    {
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();

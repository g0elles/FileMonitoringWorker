using GTFileMonitorService;
using GTFileMonitorService.Configuration;
using Microsoft.Extensions.Logging.EventLog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddEventLog(new EventLogSettings
        {
            SourceName = "GTFolderMonitorService",
            LogName = "GTFolderMonitorService"
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.AddHostedService<Worker>();
        services.Configure<MonitoredPath>(configuration.GetSection(nameof(MonitoredPath)));
        services.Configure<TimeSettings>(configuration.GetSection(nameof(TimeSettings)));

    })
    .UseWindowsService()
    .Build();

await host.RunAsync();

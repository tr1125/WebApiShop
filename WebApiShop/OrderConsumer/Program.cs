using NLog;
using NLog.Extensions.Logging;
using OrderConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddNLog("nlog.config");
builder.Services.AddHostedService<Worker>();

var logger = LogManager.GetCurrentClassLogger();

try
{
    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.Fatal(ex, "OrderConsumer terminated unexpectedly");
    throw;
}
finally
{
    LogManager.Shutdown();
}

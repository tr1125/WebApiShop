using NLog.Web;
using OrderConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddHostedService<Worker>();

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

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
    NLog.LogManager.Shutdown();
}

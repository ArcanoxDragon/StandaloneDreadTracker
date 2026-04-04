using System.Net;
using DreadRemoteConnector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = InitServices();
var logger = services.GetRequiredService<ILogger<DreadSocket>>();
await using var connector = new DreadConnector(IPAddress.Loopback);

connector.Logger = logger;
connector.ConnectionInterests = ConnectionInterests.Logging | ConnectionInterests.Multiworld;
connector.SleepTimeBeforeReconnect = TimeSpan.FromSeconds(2);
connector.CurrentInventory.PropertyChanged += (sender, eventArgs) => {
	if (eventArgs.PropertyName is null)
		return;

	var property = sender?.GetType().GetProperty(eventArgs.PropertyName);

	if (property is null)
		return;

	var value = property.GetValue(sender);

	logger.LogInformation("Inventory updated! {PropertyName} = {Value}", eventArgs.PropertyName, value);
};

await connector.StartAsync();

var shutdownSemaphore = new SemaphoreSlim(0, 1);
var shutdownRequested = false;

Console.CancelKeyPress += (_, e) => {
	if (e.SpecialKey != ConsoleSpecialKey.ControlC)
		return;

	e.Cancel = true;

	if (!Interlocked.Exchange(ref shutdownRequested, true))
		shutdownSemaphore.Release();
};

shutdownSemaphore.Wait();
logger.LogInformation("Shutting down...");
await connector.StopAsync();

static IServiceProvider InitServices()
{
	var services = new ServiceCollection();

	services.AddLogging(logging => {
		logging.SetMinimumLevel(LogLevel.Debug);
		logging.AddConsole();
	});

	return services.BuildServiceProvider(new ServiceProviderOptions {
		ValidateOnBuild = true,
	});
}
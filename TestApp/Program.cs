using System.Net;
using DreadRemoteConnector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = InitServices();
var logger = services.GetRequiredService<ILogger<DreadSocket>>();
using var socket = new DreadSocket(IPAddress.Loopback);

socket.Logger = logger;
await socket.ConnectAsync();

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
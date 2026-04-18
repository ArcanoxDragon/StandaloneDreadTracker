using System.Reactive.Disposables;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.Extensions;

internal static class SerialSubscriptionExtensions
{
	public static IDisposable SubscribeWith<T>(this IObservable<T?> observable, SerialSubscription<T> subscription)
	where T : class
	{
		var subscriptionDisposable = observable.Subscribe(subscription.Subscribe);

		return Disposable.Create(() => {
			subscriptionDisposable.Dispose();
			subscription.Unsubscribe();
		});
	}
}
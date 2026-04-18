using System.Reactive.Disposables;

namespace StandaloneDreadTracker.App.Utility;

internal delegate void SubscribeDelegate<in T>(T subject, CompositeDisposable disposables);

internal class SerialSubscription<T>(SubscribeDelegate<T> subscribe)
where T : class
{
	private volatile CompositeDisposable? disposables;

	public void Subscribe(T? newSubject)
	{
		var newDisposables = newSubject is null ? null : new CompositeDisposable();
		var previousDisposables = Interlocked.Exchange(ref this.disposables, newDisposables);

		previousDisposables?.Dispose();

		if (newSubject is null)
			return;

		subscribe(newSubject, newDisposables!);
	}

	public void Unsubscribe()
	{
		var previousDisposables = Interlocked.Exchange(ref this.disposables, null);

		previousDisposables?.Dispose();
	}
}
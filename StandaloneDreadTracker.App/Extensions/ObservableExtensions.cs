using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace StandaloneDreadTracker.App.Extensions;

internal static class ObservableExtensions
{
	extension<TParam, TResult>(IReactiveCommand<TParam, TResult> command)
	{
		public IObservable<TResult> HandleExceptions(Func<IObservable<Exception?>, IDisposable> subscribeToExceptions)
			=> Observable.Using(
				() => subscribeToExceptions(command.ThrownExceptions),
				_ => command.Catch(Observable.Empty<TResult>())
			);

		public IObservable<TResult> HandleExceptionsWith(Func<Exception, IObservable<Unit>> onException)
			=> command.HandleExceptions(exceptions => exceptions.WhereNotNull().SelectMany(onException).Subscribe());
	}
}
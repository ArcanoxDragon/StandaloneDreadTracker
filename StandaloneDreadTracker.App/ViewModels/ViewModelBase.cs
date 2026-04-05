using System.Reactive.Concurrency;
using ReactiveUI;

namespace StandaloneDreadTracker.App.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
	protected static IScheduler MainThreadScheduler => RxSchedulers.MainThreadScheduler;
	protected static IScheduler TaskPoolScheduler   => RxSchedulers.TaskpoolScheduler;

	public ViewModelActivator Activator { get; } = new();
}
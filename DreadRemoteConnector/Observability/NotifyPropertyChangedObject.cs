using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DreadRemoteConnector.Observability;

public abstract class NotifyPropertyChangedObject : INotifyPropertyChanged
{
	private readonly ConcurrentDictionary<string, PropertyChangedEventArgs> cachedEventArgs = [];

	private readonly Func<string, PropertyChangedEventArgs> eventArgsFactory =
		static propertyName => new PropertyChangedEventArgs(propertyName);

	public event PropertyChangedEventHandler? PropertyChanged;

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(field, value))
			return false;

		field = value;
		RaisePropertyChanged(propertyName);
		return true;
	}

	protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
	{
		if (propertyName == null || PropertyChanged is not { } propertyChanged)
			return;

		var eventArgs = this.cachedEventArgs.GetOrAdd(propertyName, this.eventArgsFactory);

		propertyChanged(this, eventArgs);
	}
}
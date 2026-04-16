using DreadRemoteConnector.Observability;
using JetBrains.Annotations;

namespace DreadRemoteConnector.Inventory;

[PublicAPI]
public class DreadBosses : NotifyPropertyChangedObject
{
	#region EMMIs

	public bool WhiteEmmi  { get; set => SetField(ref field, value); }
	public bool GreenEmmi  { get; set => SetField(ref field, value); }
	public bool YellowEmmi { get; set => SetField(ref field, value); }
	public bool BlueEmmi   { get; set => SetField(ref field, value); }
	public bool PurpleEmmi { get; set => SetField(ref field, value); }
	public bool OrangeEmmi { get; set => SetField(ref field, value); }

	#endregion

	#region Other Bosses

	public bool Corpius       { get; set => SetField(ref field, value); }
	public bool Kraid         { get; set => SetField(ref field, value); }
	public bool Drogyga       { get; set => SetField(ref field, value); }
	public bool ExperimentZ57 { get; set => SetField(ref field, value); }
	public bool Golzuna       { get; set => SetField(ref field, value); }
	public bool Escue         { get; set => SetField(ref field, value); }

	#endregion
}
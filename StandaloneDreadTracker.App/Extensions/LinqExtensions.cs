namespace StandaloneDreadTracker.App.Extensions;

internal static class LinqExtensions
{
	extension<T>(IEnumerable<T> source)
	{
		public IEnumerable<(int, T)> Pairs()
		{
			var index = 0;

			foreach (var item in source)
				yield return ( index++, item );
		}
	}
}
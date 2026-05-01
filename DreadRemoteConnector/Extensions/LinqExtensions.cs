namespace DreadRemoteConnector.Extensions;

internal static class LinqExtensions
{
	extension<T>(IEnumerable<T> source)
	{
		public int IndexOf(T item, IEqualityComparer<T> comparer)
		{
			foreach (var (index, candidate) in source.Index())
			{
				if (comparer.Equals(candidate, item))
					return index;
			}

			return -1;
		}
	}
}
using System.Collections.ObjectModel;

namespace Microsoft.Dynamics.Nav.Types;

public class ReadOnlyCollectionConstructor<T>
{
	public ReadOnlyCollection<T> ReadOnlyCollection { get; }

	public ReadOnlyCollectionConstructor(object readOnlyCollection)
	{
		ReadOnlyCollection = (ReadOnlyCollection<T>)new ReadOnlyCollectionConverter().ConvertFrom(readOnlyCollection);
	}

	public ReadOnlyCollectionConstructor(ReadOnlyCollection<T> readOnlyCollection)
	{
		ReadOnlyCollection = readOnlyCollection;
	}
}

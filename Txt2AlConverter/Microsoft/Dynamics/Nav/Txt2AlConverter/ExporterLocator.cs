using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class ExporterLocator : IExporterLocator
{
	private readonly Func<IElement, string> getElementKey;

	private readonly Dictionary<string, object> servicecontainer = new Dictionary<string, object>();

	public ExporterLocator(Func<IElement, string> getElementKey)
	{
		this.getElementKey = getElementKey;
	}

	public string GetElementKey(IElement element)
	{
		return getElementKey(element);
	}

	public T GetService<T>(string serviceKey)
	{
		return (T)servicecontainer[serviceKey];
	}

	public void AddService<T>(string serviceKey, T value)
	{
		servicecontainer.Add(serviceKey, value);
	}
}

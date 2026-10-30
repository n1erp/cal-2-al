using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

[DebuggerDisplay("{DebugDisplay()}")]
public class ApplicationObject
{
	private class ApplicationObjectEqualityComparer : IEqualityComparer<ApplicationObject>
	{
		public bool Equals(ApplicationObject x, ApplicationObject y)
		{
			if (x.ElementTypeInfo.ElementType == ElementType.Modification && y.ElementTypeInfo.ElementType == ElementType.Modification)
			{
				return x.RootElement.GetStringProperty(PropertyType.Target) == y.RootElement.GetStringProperty(PropertyType.Target);
			}
			if (x.ElementTypeInfo.ElementType == y.ElementTypeInfo.ElementType)
			{
				return x.Id == y.Id;
			}
			return false;
		}

		public int GetHashCode(ApplicationObject obj)
		{
			return obj.ElementTypeInfo.ElementType.GetHashCode() ^ obj.Id.ObjectNumber.GetHashCode();
		}
	}

	private readonly IElement rootElement;

	private ApplicationObjectId? id;

	private readonly ApplicationObjectProperties objectProperties = new ApplicationObjectProperties();

	public static readonly IEqualityComparer<ApplicationObject> EqualityComparer = new ApplicationObjectEqualityComparer();

	public ApplicationObjectProperties ObjectProperties => objectProperties;

	public bool ShouldEncode { get; set; }

	public virtual bool IsValid => true;

	public ElementTypeInfo ElementTypeInfo => rootElement.ElementTypeInfo;

	public ApplicationObjectId Id
	{
		get
		{
			if (!id.HasValue)
			{
				id = RootElement.GetApplicationObjectId();
			}
			return id.Value;
		}
	}

	public IElement RootElement => rootElement;

	public ApplicationObject(IElement rootElement)
	{
		this.rootElement = rootElement;
	}

	public ApplicationObject(IElement rootElement, ApplicationObjectProperties properties)
	{
		this.rootElement = rootElement;
		ObjectProperties.CopyValuesFrom(properties);
	}

	public IEnumerable<IElement> GetElements()
	{
		return rootElement.GetElements();
	}

	internal string DebugDisplay()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0} {1}", ElementTypeInfo.ElementType, (ElementTypeInfo.ElementType == ElementType.Modification) ? RootElement.GetStringProperty(PropertyType.Target) : Id.ObjectNumber.ToString(CultureInfo.CurrentCulture));
	}
}

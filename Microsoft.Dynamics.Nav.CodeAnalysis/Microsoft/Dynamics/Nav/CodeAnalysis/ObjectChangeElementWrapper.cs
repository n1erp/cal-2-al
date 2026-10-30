using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ObjectChangeElementWrapper : IEquatable<ObjectChangeElementWrapper>
{
	public ObjectChangeElement ObjectChangeElement { get; }

	internal ObjectChangeElementWrapper(ObjectChangeElement objectChangeElement)
	{
		ObjectChangeElement = objectChangeElement;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ObjectChangeElementWrapper);
	}

	public bool Equals(ObjectChangeElementWrapper other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (ObjectChangeElement == null)
		{
			return other.ObjectChangeElement != null;
		}
		if (other.ObjectChangeElement == null)
		{
			return ObjectChangeElement != null;
		}
		if (!ObjectChangeElement.Id.HasValue && !other.ObjectChangeElement.Id.HasValue)
		{
			if (SemanticFacts.IsSameName(ObjectChangeElement.Name, other.ObjectChangeElement.Name))
			{
				return ObjectChangeElement.Kind == other.ObjectChangeElement.Kind;
			}
			return false;
		}
		if (ObjectChangeElement.Id == other.ObjectChangeElement.Id)
		{
			return ObjectChangeElement.Kind == other.ObjectChangeElement.Kind;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (ObjectChangeElement == null)
		{
			return 0;
		}
		if (!ObjectChangeElement.Id.HasValue)
		{
			return Hash.Combine(ObjectChangeElement.Name.GetHashCode(), ObjectChangeElement.Kind.GetHashCode());
		}
		return Hash.Combine(ObjectChangeElement.Id.GetHashCode(), ObjectChangeElement.Kind.GetHashCode());
	}

	public static bool operator ==(ObjectChangeElementWrapper left, ObjectChangeElementWrapper right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ObjectChangeElementWrapper left, ObjectChangeElementWrapper right)
	{
		return !object.Equals(left, right);
	}
}

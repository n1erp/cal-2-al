using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class ObjectChangeContext : IEquatable<ObjectChangeContext>
{
	internal ObjectChangeElementWrapper ObjectChangeElementWrapper { get; }

	public ObjectChangeElement ObjectChangeElement { get; }

	public ObjectChangeState State { get; }

	internal ObjectChangeContext(ObjectChangeElementWrapper element, ObjectChangeState state)
	{
		ObjectChangeElement = element?.ObjectChangeElement;
		State = state;
		ObjectChangeElementWrapper = element;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ObjectChangeContext);
	}

	public bool Equals(ObjectChangeContext other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (ObjectChangeElementWrapper == other.ObjectChangeElementWrapper)
		{
			return State == other.State;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (ObjectChangeElementWrapper == null)
		{
			return State.GetHashCode();
		}
		return Hash.Combine(ObjectChangeElementWrapper.GetHashCode(), State.GetHashCode());
	}

	public static bool operator ==(ObjectChangeContext left, ObjectChangeContext right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ObjectChangeContext left, ObjectChangeContext right)
	{
		return !object.Equals(left, right);
	}
}

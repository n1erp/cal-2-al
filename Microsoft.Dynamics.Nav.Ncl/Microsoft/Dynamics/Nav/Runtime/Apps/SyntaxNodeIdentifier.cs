using System;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class SyntaxNodeIdentifier
{
	public string Id { get; }

	public PersonalizationV1DeltaType DeltaType { get; }

	public SyntaxNodeIdentifier(string id, PersonalizationV1DeltaType deltaType)
	{
		Id = id ?? throw new ArgumentNullException("id");
		DeltaType = deltaType;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SyntaxNodeIdentifier syntaxNodeIdentifier && string.Equals(Id, syntaxNodeIdentifier.Id, StringComparison.OrdinalIgnoreCase))
		{
			return DeltaType == syntaxNodeIdentifier.DeltaType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode() ^ DeltaType.GetHashCode();
	}
}

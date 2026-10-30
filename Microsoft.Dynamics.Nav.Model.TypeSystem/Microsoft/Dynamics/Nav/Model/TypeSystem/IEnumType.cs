using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public interface IEnumType : IModelType
{
	Type TypeOfEnum { get; }

	Type TypeOfEnumType { get; }

	string ValueAsString();

	int ValueAsInt32();
}

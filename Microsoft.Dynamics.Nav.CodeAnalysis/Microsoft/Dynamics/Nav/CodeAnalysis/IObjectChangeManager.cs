using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IObjectChangeManager
{
	IReadOnlyCollection<ObjectChangeContext> Changes { get; }

	bool ContainsSymbol(IObjectTypeSymbol symbol);

	void Update(IObjectTypeSymbol symbol, string declaringObjectPath, ObjectChangeState state);

	void Clear();

	void Serialize(string path, IFileSystem fileSystem);

	void Load(string path, IFileSystem fileSystem);
}

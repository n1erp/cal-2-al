using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Emit;

internal interface IPEDeltaAssemblyBuilder
{
	void OnCreatedIndices(DiagnosticBag diagnostics);

	IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> GetAnonymousTypeMap();
}

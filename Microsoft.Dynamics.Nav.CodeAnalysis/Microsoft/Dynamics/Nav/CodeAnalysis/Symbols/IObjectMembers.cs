using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal interface IObjectMembers
{
	CompletionPart NextIncompletePart { get; }

	bool HasComplete(CompletionPart part);

	bool NotePartComplete(CompletionPart part);

	void SpinWaitComplete(CompletionPart part, CancellationToken cancellationToken);

	ImmutableArray<VariableSymbol> GetGlobalVariables();

	ImmutableArray<Symbol> GetMembers();

	ImmutableArray<Symbol> GetMembers(string name);

	ImmutableArray<PropertySymbol> GetProperties();
}

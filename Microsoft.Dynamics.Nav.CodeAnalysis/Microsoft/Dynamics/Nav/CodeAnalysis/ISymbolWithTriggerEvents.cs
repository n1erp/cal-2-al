using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface ISymbolWithTriggerEvents
{
	ImmutableArray<BuiltInTriggerEventSymbol> TriggerEvents { get; }
}

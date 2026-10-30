using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedAttributeData : SourceAttributeData
{
	internal SynthesizedAttributeData(MethodSymbol wellKnownMember, ImmutableArray<TypedConstant> arguments, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
		: base(null, wellKnownMember.ContainingType, wellKnownMember, arguments, default, namedArguments, hasErrors: false, isConditionallyOmitted: false)
	{
	}
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ICustomAttribute
{
	int ArgumentCount { get; }

	ushort NamedArgumentCount { get; }

	bool AllowMultiple { get; }

	ImmutableArray<IMetadataExpression> GetArguments(EmitContext context);

	IMethodReference Constructor(EmitContext context, bool reportDiagnostics);

	ImmutableArray<IMetadataNamedArgument> GetNamedArguments(EmitContext context);

	ITypeReference GetType(EmitContext context);
}

using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class ParameterTypeInformation : IParameterTypeInformation, IParameterListEntry
{
	private readonly ParameterSymbol _underlyingParameter;

	ImmutableArray<ICustomModifier> IParameterTypeInformation.CustomModifiers => _underlyingParameter.CustomModifiers.As<ICustomModifier>();

	bool IParameterTypeInformation.IsByReference => _underlyingParameter.RefKind != RefKind.None;

	ImmutableArray<ICustomModifier> IParameterTypeInformation.RefCustomModifiers => _underlyingParameter.RefCustomModifiers.As<ICustomModifier>();

	ushort IParameterListEntry.Index => (ushort)_underlyingParameter.Ordinal;

	public ParameterTypeInformation(ParameterSymbol underlyingParameter)
	{
		_underlyingParameter = underlyingParameter;
	}

	ITypeReference IParameterTypeInformation.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(_underlyingParameter.Type, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	public override string ToString()
	{
		return _underlyingParameter.ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
	}
}

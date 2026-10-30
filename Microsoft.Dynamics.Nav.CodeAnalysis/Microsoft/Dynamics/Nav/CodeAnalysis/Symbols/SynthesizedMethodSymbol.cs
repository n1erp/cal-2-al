using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedMethodSymbol : MethodSymbol
{
	public override int Id { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override ReturnValueSymbol ReturnValue { get; }

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override ImmutableArray<AttributeSymbol> Attributes { get; }

	public override ImmutableArray<VariableSymbol> LocalVariables { get; }

	public override bool IsStatic => false;

	public override bool IsLocal { get; }

	public override MethodKind MethodKind { get; }

	public override Symbol ContainingSymbol { get; }

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (!IsLocal)
			{
				return Accessibility.Public;
			}
			return Accessibility.Private;
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedMethodSymbol(Symbol containingSymbol, string name, string metadataName = null, TypeSymbol returnType = null, ImmutableArray<ParameterSymbol> parameters = default(ImmutableArray<ParameterSymbol>), ImmutableArray<AttributeSymbol> attributes = default(ImmutableArray<AttributeSymbol>), ImmutableArray<VariableSymbol> localVariables = default(ImmutableArray<VariableSymbol>), bool isLocal = false, MethodKind methodKind = MethodKind.Method)
	{
		Name = name;
		MetadataName = metadataName ?? name.MangleUnquotedIdentifierName();
		ReturnValue = ((returnType != null) ? new SynthesizedReturnValueSymbol(this, returnType) : null);
		Parameters = (parameters.IsDefault ? ImmutableArray<ParameterSymbol>.Empty : ((IEnumerable<ParameterSymbol>)parameters.Select((ParameterSymbol x) => new SynthesizedParameterSymbol(this, x.Type, x.Ordinal, x.IsVar, x.Name))).ToImmutableArray());
		Attributes = (attributes.IsDefault ? ImmutableArray<AttributeSymbol>.Empty : attributes);
		LocalVariables = (localVariables.IsDefault ? ImmutableArray<VariableSymbol>.Empty : localVariables);
		IsLocal = isLocal;
		MethodKind = methodKind;
		ContainingSymbol = containingSymbol;
		Id = CalculateMethodId();
	}
}

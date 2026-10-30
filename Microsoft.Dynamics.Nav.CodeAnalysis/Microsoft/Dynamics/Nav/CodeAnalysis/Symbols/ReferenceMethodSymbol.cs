using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceMethodSymbol : MethodSymbol
{
	private readonly Lazy<ImmutableArray<ParameterSymbol>> lazyParameters;

	private readonly Lazy<ImmutableArray<AttributeSymbol>> lazyAttributes;

	private readonly Lazy<ReturnValueSymbol> lazyReturnValueType;

	public override Symbol ContainingSymbol { get; }

	public override int Id { get; }

	public override string Name { get; }

	public override bool IsTryFunction => Attributes.Any((AttributeSymbol a) =>
	{
		AttributeTypeInfo attributeInfo = a.AttributeInfo;
		return attributeInfo != null && attributeInfo.Category == AttributeCategory.TryFunction;
	});

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ReturnValueSymbol ReturnValue => lazyReturnValueType.Value;

	public override ImmutableArray<ParameterSymbol> Parameters => lazyParameters.Value;

	public override ImmutableArray<AttributeSymbol> Attributes => lazyAttributes.Value;

	public override ImmutableArray<VariableSymbol> LocalVariables { get; } = ImmutableArray<VariableSymbol>.Empty;

	public override bool IsStatic { get; }

	public override bool IsLocal { get; }

	public override MethodKind MethodKind { get; }

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

	internal ReferenceMethodSymbol(ContainerSymbol containingSymbol, MethodDefinition method)
	{
		ReferenceMethodSymbol referenceMethodSymbol = this;
		ContainingSymbol = containingSymbol;
		Id = method.Id ?? (-1);
		Name = method.Name();
		IsStatic = false;
		IsLocal = method.IsLocal ?? false;
		MethodKind = method.MethodKind ?? MethodKind.Method;
		lazyAttributes = ReferenceSymbolHelper.BuildAttributesLazy(this, method);
		lazyReturnValueType = ReferenceSymbolHelper.CreateLazy(() => referenceMethodSymbol.GetReturnValueLazy(containingSymbol, method));
		lazyParameters = ReferenceSymbolHelper.BuildParametersLazy(this, method);
	}

	private ReturnValueSymbol GetReturnValueLazy(ContainerSymbol containingSymbol, MethodDefinition method)
	{
		TypeSymbol typeSymbol = SerializableSymbolModelConverter.ResolveType(containingSymbol.ContainingModule, this, method.GetReturnTypeDefinition());
		ReturnValueSymbol result = (((object)typeSymbol == MissingTypeSymbol.Instance) ? Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.None : new BuiltInReturnValueSymbol(typeSymbol, isOptional: true));
		if (IsTryFunction)
		{
			return Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.Boolean;
		}
		return result;
	}
}

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceCustomEventAccessorSymbol : SourceEventAccessorSymbol
{
	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	private readonly string _name;

	public override Accessibility DeclaredAccessibility => AssociatedSymbol.DeclaredAccessibility;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override string Name => _name;

	public override bool IsImplicitlyDeclared => false;

	internal override bool GenerateDebugInfo => true;

	internal override bool IsExpressionBodied
	{
		get
		{
			AccessorDeclarationSyntax syntax = GetSyntax();
			bool flag = syntax.Body != null;
			bool flag2 = syntax.ExpressionBody != null;
			return !flag & flag2;
		}
	}

	internal SourceCustomEventAccessorSymbol(SourceEventSymbol @event, AccessorDeclarationSyntax syntax, EventSymbol explicitlyImplementedEventOpt, string aliasQualifierOpt, DiagnosticBag diagnostics)
		: base(@event, syntax.GetReference(), ImmutableArray.Create(syntax.Keyword.GetLocation()))
	{
		bool flag = syntax.Kind() == SyntaxKind.AddAccessorDeclaration;
		string name;
		ImmutableArray<MethodSymbol> immutableArray;
		if ((object)explicitlyImplementedEventOpt == null)
		{
			name = SourceEventSymbol.GetAccessorName(@event.Name, flag);
			immutableArray = ImmutableArray<MethodSymbol>.Empty;
		}
		else
		{
			MethodSymbol methodSymbol = (flag ? explicitlyImplementedEventOpt.AddMethod : explicitlyImplementedEventOpt.RemoveMethod);
			name = ExplicitInterfaceHelpers.GetMemberName(((object)methodSymbol != null) ? methodSymbol.Name : SourceEventSymbol.GetAccessorName(explicitlyImplementedEventOpt.Name, flag), explicitlyImplementedEventOpt.ContainingType, aliasQualifierOpt);
			immutableArray = (((object)methodSymbol == null) ? ImmutableArray<MethodSymbol>.Empty : ImmutableArray.Create(methodSymbol));
		}
		_explicitInterfaceImplementations = immutableArray;
		_name = name;
		MakeFlags(flag ? MethodKind.EventAdd : MethodKind.EventRemove, @event.Modifiers, returnsVoid: false, isExtensionMethod: false, immutableArray.Any());
		if (@event.ContainingType.IsInterface)
		{
			diagnostics.Add(ErrorCode.ERR_EventPropertyInInterface, Location);
		}
		else if (syntax.Body != null || syntax.ExpressionBody != null)
		{
			if (IsExtern && !IsAbstract)
			{
				diagnostics.Add(ErrorCode.ERR_ExternHasBody, Location, this);
			}
			else if (IsAbstract && !IsExtern)
			{
				diagnostics.Add(ErrorCode.ERR_AbstractHasBody, Location, this);
			}
		}
		_name = GetOverriddenAccessorName(@event, flag) ?? _name;
		if (syntax.Modifiers.Count > 0)
		{
			diagnostics.Add(ErrorCode.ERR_NoModifiersOnAccessor, syntax.Modifiers[0].GetLocation());
		}
		Symbol.CheckForBlockAndExpressionBody(syntax.Body, syntax.ExpressionBody, syntax, diagnostics);
	}

	internal AccessorDeclarationSyntax GetSyntax()
	{
		return (AccessorDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(GetSyntax().AttributeLists);
	}
}

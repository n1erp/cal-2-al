using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceCustomEventSymbol : SourceEventSymbol
{
	private readonly TypeSymbol _type;

	private readonly string _name;

	private readonly SourceCustomEventAccessorSymbol _addMethod;

	private readonly SourceCustomEventAccessorSymbol _removeMethod;

	private readonly TypeSymbol _explicitInterfaceType;

	private readonly ImmutableArray<EventSymbol> _explicitInterfaceImplementations;

	public override TypeSymbol Type => _type;

	public override string Name => _name;

	public override MethodSymbol AddMethod => _addMethod;

	public override MethodSymbol RemoveMethod => _removeMethod;

	protected override AttributeLocation AllowedAttributeLocations => AttributeLocation.Event;

	private ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => ((EventDeclarationSyntax)CSharpSyntaxNode).ExplicitInterfaceSpecifier;

	internal override bool IsExplicitInterfaceImplementation => ExplicitInterfaceSpecifier != null;

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	internal SourceCustomEventSymbol(SourceMemberContainerTypeSymbol containingType, Binder binder, EventDeclarationSyntax syntax, DiagnosticBag diagnostics)
		: base(containingType, syntax, syntax.Modifiers, syntax.ExplicitInterfaceSpecifier, syntax.Identifier, diagnostics)
	{
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = syntax.ExplicitInterfaceSpecifier;
		SyntaxToken identifier = syntax.Identifier;
		bool flag = explicitInterfaceSpecifier != null;
		_name = ExplicitInterfaceHelpers.GetMemberNameAndInterfaceSymbol(binder, explicitInterfaceSpecifier, identifier.ValueText, diagnostics, out _explicitInterfaceType, out var aliasQualifierOpt);
		_type = BindEventType(binder, syntax.Type, diagnostics);
		EventSymbol eventSymbol = this.FindExplicitlyImplementedEvent(_explicitInterfaceType, identifier.ValueText, explicitInterfaceSpecifier, diagnostics);
		if (!flag)
		{
			if (IsOverride)
			{
				EventSymbol overriddenEvent = OverriddenEvent;
				if ((object)overriddenEvent != null)
				{
					SourceEventSymbol.CopyEventCustomModifiers(overriddenEvent, ref _type, ContainingAssembly);
				}
			}
		}
		else if ((object)eventSymbol != null)
		{
			SourceEventSymbol.CopyEventCustomModifiers(eventSymbol, ref _type, ContainingAssembly);
		}
		AccessorDeclarationSyntax accessorDeclarationSyntax = null;
		AccessorDeclarationSyntax accessorDeclarationSyntax2 = null;
		foreach (AccessorDeclarationSyntax accessor in syntax.AccessorList.Accessors)
		{
			switch (accessor.Kind())
			{
			case SyntaxKind.AddAccessorDeclaration:
				if (accessorDeclarationSyntax == null)
				{
					accessorDeclarationSyntax = accessor;
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
				}
				break;
			case SyntaxKind.RemoveAccessorDeclaration:
				if (accessorDeclarationSyntax2 == null)
				{
					accessorDeclarationSyntax2 = accessor;
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateAccessor, accessor.Keyword.GetLocation());
				}
				break;
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
				diagnostics.Add(ErrorCode.ERR_AddOrRemoveExpected, accessor.Keyword.GetLocation());
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(accessor.Kind());
			case SyntaxKind.UnknownAccessorDeclaration:
				break;
			}
		}
		_addMethod = CreateAccessorSymbol(accessorDeclarationSyntax, eventSymbol, aliasQualifierOpt, diagnostics);
		_removeMethod = CreateAccessorSymbol(accessorDeclarationSyntax2, eventSymbol, aliasQualifierOpt, diagnostics);
		if (containingType.IsInterfaceType())
		{
			if (accessorDeclarationSyntax == null && accessorDeclarationSyntax2 == null)
			{
				diagnostics.Add(ErrorCode.ERR_EventNeedsBothAccessors, Locations[0], this);
			}
		}
		else if (accessorDeclarationSyntax == null || accessorDeclarationSyntax2 == null)
		{
			diagnostics.Add(ErrorCode.ERR_EventNeedsBothAccessors, Locations[0], this);
		}
		_explicitInterfaceImplementations = (((object)eventSymbol == null) ? ImmutableArray<EventSymbol>.Empty : ImmutableArray.Create(eventSymbol));
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		if ((object)_explicitInterfaceType != null)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier = ExplicitInterfaceSpecifier;
			_explicitInterfaceType.CheckAllConstraints(conversions, new SourceLocation(explicitInterfaceSpecifier.Name), diagnostics);
		}
	}

	private SourceCustomEventAccessorSymbol CreateAccessorSymbol(AccessorDeclarationSyntax syntaxOpt, EventSymbol explicitlyImplementedEventOpt, string aliasQualifierOpt, DiagnosticBag diagnostics)
	{
		if (syntaxOpt == null)
		{
			return null;
		}
		return new SourceCustomEventAccessorSymbol(this, syntaxOpt, explicitlyImplementedEventOpt, aliasQualifierOpt, diagnostics);
	}
}

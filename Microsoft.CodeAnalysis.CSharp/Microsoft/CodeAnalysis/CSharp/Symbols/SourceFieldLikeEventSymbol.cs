using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceFieldLikeEventSymbol : SourceEventSymbol
{
	private readonly string _name;

	private readonly TypeSymbol _type;

	private readonly SourceEventFieldSymbol _associatedField;

	private readonly SynthesizedFieldLikeEventAccessorSymbol _addMethod;

	private readonly SynthesizedFieldLikeEventAccessorSymbol _removeMethod;

	internal override FieldSymbol AssociatedField => _associatedField;

	public override string Name => _name;

	public override TypeSymbol Type => _type;

	public override MethodSymbol AddMethod => _addMethod;

	public override MethodSymbol RemoveMethod => _removeMethod;

	internal override bool IsExplicitInterfaceImplementation => false;

	protected override AttributeLocation AllowedAttributeLocations
	{
		get
		{
			if ((object)_associatedField == null)
			{
				return AttributeLocation.Method | AttributeLocation.Event;
			}
			return AttributeLocation.Method | AttributeLocation.Field | AttributeLocation.Event;
		}
	}

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations => ImmutableArray<EventSymbol>.Empty;

	internal SourceFieldLikeEventSymbol(SourceMemberContainerTypeSymbol containingType, Binder binder, SyntaxTokenList modifiers, VariableDeclaratorSyntax declaratorSyntax, DiagnosticBag diagnostics)
		: base(containingType, declaratorSyntax, modifiers, null, declaratorSyntax.Identifier, diagnostics)
	{
		_name = declaratorSyntax.Identifier.ValueText;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		_type = BindEventType(binder, variableDeclarationSyntax.Type, instance);
		if (IsOverride)
		{
			EventSymbol overriddenEvent = OverriddenEvent;
			if ((object)overriddenEvent != null)
			{
				SourceEventSymbol.CopyEventCustomModifiers(overriddenEvent, ref _type, ContainingAssembly);
			}
		}
		bool flag = declaratorSyntax.Initializer != null;
		bool flag2 = containingType.IsInterfaceType();
		if (flag)
		{
			if (flag2)
			{
				diagnostics.Add(ErrorCode.ERR_InterfaceEventInitializer, Locations[0], this);
			}
			else if (IsAbstract)
			{
				diagnostics.Add(ErrorCode.ERR_AbstractEventInitializer, Locations[0], this);
			}
		}
		if (flag || (!flag2 && !IsExtern && !IsAbstract))
		{
			_associatedField = MakeAssociatedField(declaratorSyntax);
		}
		if (!IsStatic && ContainingType.IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_FieldlikeEventsInRoStruct, Locations[0]);
		}
		_addMethod = new SynthesizedFieldLikeEventAccessorSymbol(this, isAdder: true);
		_removeMethod = new SynthesizedFieldLikeEventAccessorSymbol(this, isAdder: false);
		if (variableDeclarationSyntax.Variables[0] == declaratorSyntax)
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
	}

	private SourceEventFieldSymbol MakeAssociatedField(VariableDeclaratorSyntax declaratorSyntax)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		SourceEventFieldSymbol result = new SourceEventFieldSymbol(this, declaratorSyntax, instance);
		instance.Free();
		return result;
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		if ((object)AssociatedField != null)
		{
			AssociatedField.ForceComplete(locationOpt, cancellationToken);
		}
		base.ForceComplete(locationOpt, cancellationToken);
	}
}

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedFieldLikeEventAccessorSymbol : SourceEventAccessorSymbol
{
	private readonly object _methodChecksLockObject = new object();

	private readonly string _name;

	public override string Name => _name;

	public override bool IsImplicitlyDeclared => true;

	internal override bool GenerateDebugInfo => false;

	protected override SourceMemberMethodSymbol BoundAttributesSource
	{
		get
		{
			if (MethodKind != MethodKind.EventAdd)
			{
				return null;
			}
			return (SourceMemberMethodSymbol)AssociatedEvent.RemoveMethod;
		}
	}

	protected override IAttributeTargetSymbol AttributeOwner => AssociatedEvent;

	protected override object MethodChecksLockObject => _methodChecksLockObject;

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			MethodImplAttributes methodImplAttributes = base.ImplementationAttributes;
			if (!IsAbstract && !AssociatedEvent.IsWindowsRuntimeEvent && !ContainingType.IsStructType() && (object)DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Threading_Interlocked__CompareExchange_T) == null)
			{
				methodImplAttributes |= MethodImplAttributes.Synchronized;
			}
			return methodImplAttributes;
		}
	}

	internal SynthesizedFieldLikeEventAccessorSymbol(SourceFieldLikeEventSymbol @event, bool isAdder)
		: base(@event, null, @event.Locations)
	{
		MakeFlags(isAdder ? MethodKind.EventAdd : MethodKind.EventRemove, @event.Modifiers, returnsVoid: false, isExtensionMethod: false);
		_name = GetOverriddenAccessorName(@event, isAdder) ?? SourceEventSymbol.GetAccessorName(@event.Name, isAdder);
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(AssociatedEvent.AttributeDeclarationSyntaxList);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
	}
}

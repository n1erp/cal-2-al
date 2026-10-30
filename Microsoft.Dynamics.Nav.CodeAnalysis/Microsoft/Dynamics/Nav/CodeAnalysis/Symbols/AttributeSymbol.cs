using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class AttributeSymbol : Symbol, IAttributeSymbol, ISymbol, IEquatable<ISymbol>
{
	private ImmutableArray<AttributeArgumentSymbol> signatureArguments;

	public override SymbolKind Kind => SymbolKind.Attribute;

	internal abstract BoundAttribute Attribute { get; }

	internal abstract ImmutableArray<AttributeArgumentSymbol> Arguments { get; }

	internal ImmutableArray<AttributeArgumentSymbol> SignatureArguments
	{
		get
		{
			if (AttributeInfo == null)
			{
				return ImmutableArray<AttributeArgumentSymbol>.Empty;
			}
			LazyGetSignatureArguments();
			return signatureArguments;
		}
	}

	internal abstract AttributeTypeInfo AttributeInfo { get; }

	public override bool IsDeprecated => AttributeInfo?.IsObsolete ?? false;

	public override SymbolCompilationScope CompilationScope => AttributeInfo?.CompilationScope ?? SymbolCompilationScope.Personalization;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override VersionCompatibility Compatibility => AttributeInfo?.Compatibility;

	AttributeCategory IAttributeSymbol.AttributeCategory => AttributeInfo?.Category ?? AttributeCategory.None;

	AttributeKind IAttributeSymbol.AttributeKind => AttributeInfo?.Kind ?? AttributeKind.None;

	ImmutableArray<IAttributeArgumentSymbol> IAttributeSymbol.SignatureArguments => StaticCast<IAttributeArgumentSymbol>.From(SignatureArguments);

	ImmutableArray<IAttributeArgumentSymbol> IAttributeSymbol.Arguments => StaticCast<IAttributeArgumentSymbol>.From(Arguments);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAttribute(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAttribute(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitAttribute(this, a);
	}

	private void LazyGetSignatureArguments()
	{
		if (!(signatureArguments != null) && AttributeInfo != null && !(AttributeInfo.Parameters == null))
		{
			ArrayBuilder<AttributeArgumentSymbol> instance = ArrayBuilder<AttributeArgumentSymbol>.GetInstance();
			foreach (AttributeParameter parameter in AttributeInfo.Parameters)
			{
				instance.Add(new BuiltInAttributeArgumentSymbol(this, parameter.Type, parameter.Name));
			}
			ImmutableArray<AttributeArgumentSymbol> value = instance.ToImmutableAndFree();
			ImmutableInterlocked.InterlockedCompareExchange(ref signatureArguments, value, default);
		}
	}

	public override string GetDocumentation()
	{
		return DocumentationFacts.GetDocumentation(this);
	}
}

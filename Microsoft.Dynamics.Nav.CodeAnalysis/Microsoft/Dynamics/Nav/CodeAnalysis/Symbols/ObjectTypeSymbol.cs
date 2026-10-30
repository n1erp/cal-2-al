using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ObjectTypeSymbol : TypeSymbol, IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers
{
	private readonly ContainerSymbol containingSymbol;

	protected SymbolCompilationScope? Scope;

	internal virtual int GlobalVariableCount => GlobalVariables.Length;

	internal abstract ImmutableArray<VariableSymbol> GlobalVariables { get; }

	public override Symbol ContainingSymbol => containingSymbol;

	internal override ModuleSymbol ContainingModule => this.GetContainingSymbolOfKind<ModuleSymbol>(SymbolKind.Module);

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override SymbolCompilationScope CompilationScope
	{
		get
		{
			if (!Scope.HasValue)
			{
				Scope = this.GetEnumPropertyValue<SymbolCompilationScope>(PropertyKind.Scope) ?? SymbolCompilationScope.Personalization;
			}
			return Scope.Value;
		}
	}

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitObjectType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitObjectType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitObjectType(this, a);
	}

	protected ObjectTypeSymbol(ContainerSymbol containingSymbol)
	{
		this.containingSymbol = containingSymbol;
	}

	public abstract TriggerTypeInfo GetTriggerTypeInfo(string name);

	internal virtual ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}

	internal virtual ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}

	internal virtual PreMetadataEmitter GetPreMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}
}

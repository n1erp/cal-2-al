using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class ErrorMethodSymbol : MethodSymbol
{
	internal class ErrorReturnValueSymbol : ReturnValueSymbol
	{
		private readonly TypeSymbol containingType;

		private readonly TypeSymbol returnType;

		public override SymbolKind Kind => SymbolKind.Method;

		public override bool IsNamed => false;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		public override Symbol ContainingSymbol => containingType;

		public override TypeSymbol Type => returnType;

		public ErrorReturnValueSymbol(TypeSymbol containingType, TypeSymbol returnType)
		{
			this.containingType = containingType;
			this.returnType = returnType;
		}
	}

	private readonly TypeSymbol containingType;

	private readonly ReturnValueSymbol returnValue;

	private readonly string name;

	public override MethodKind MethodKind => MethodKind.Method;

	public override bool IsLocal => false;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override bool IsStatic => false;

	public override SymbolKind Kind => SymbolKind.Method;

	public override string Name => name;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol => containingType;

	internal override int ParameterCount => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	internal override int LocalVariableCount => 0;

	public override ImmutableArray<VariableSymbol> LocalVariables => ImmutableArray<VariableSymbol>.Empty;

	public override ReturnValueSymbol ReturnValue => returnValue;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitErrorMethod(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitErrorMethod(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitErrorMethod(this, a);
	}

	public ErrorMethodSymbol(TypeSymbol containingType, TypeSymbol returnType, string name)
	{
		this.containingType = containingType;
		returnValue = new ErrorReturnValueSymbol(containingType, returnType);
		this.name = name;
	}
}

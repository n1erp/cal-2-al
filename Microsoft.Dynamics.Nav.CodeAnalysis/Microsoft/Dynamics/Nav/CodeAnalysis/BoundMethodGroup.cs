using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundMethodGroup : BoundMethodOrPropertyGroup
{
	public MemberAccessExpressionSyntax MemberAccessExpressionSyntax => Syntax as MemberAccessExpressionSyntax;

	public SyntaxNode NameSyntax
	{
		get
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = MemberAccessExpressionSyntax;
			if (memberAccessExpressionSyntax != null)
			{
				return memberAccessExpressionSyntax.Name;
			}
			return Syntax;
		}
	}

	public string Name { get; private set; }

	public ImmutableArray<MethodSymbol> Methods { get; private set; }

	public Symbol LookupSymbolOpt { get; private set; }

	public DiagnosticInfo LookupError { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundMethodGroup(SyntaxNode syntax, BoundExpression receiverOpt, string name, ImmutableArray<MethodSymbol> methods, LookupResult lookupResult, bool hasErrors = false)
		: this(syntax, name, methods, lookupResult.SingleSymbolOrDefault, lookupResult.Error, receiverOpt, lookupResult.Kind, hasErrors)
	{
	}

	public BoundMethodGroup(SyntaxNode syntax, string name, ImmutableArray<MethodSymbol> methods, Symbol lookupSymbolOpt, DiagnosticInfo lookupError, BoundExpression receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(BoundKind.MethodGroup, syntax, receiverOpt, resultKind, hasErrors || receiverOpt.HasErrors())
	{
		Name = name;
		Methods = methods;
		LookupSymbolOpt = lookupSymbolOpt;
		LookupError = lookupError;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMethodGroup(this);
	}

	public BoundMethodGroup Update(string name, ImmutableArray<MethodSymbol> methods, Symbol lookupSymbolOpt, DiagnosticInfo lookupError, BoundExpression receiverOpt, LookupResultKind resultKind)
	{
		if (name != Name || methods != Methods || lookupSymbolOpt != LookupSymbolOpt || lookupError != LookupError || receiverOpt != ReceiverOpt || resultKind != ResultKind)
		{
			return new BoundMethodGroup(Syntax, name, methods, lookupSymbolOpt, lookupError, receiverOpt, resultKind, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}

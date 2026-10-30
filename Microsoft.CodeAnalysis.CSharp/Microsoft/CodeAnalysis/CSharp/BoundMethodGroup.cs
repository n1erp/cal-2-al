using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

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

	public BoundExpression InstanceOpt
	{
		get
		{
			if (ReceiverOpt == null || ReceiverOpt.Kind == BoundKind.TypeExpression)
			{
				return null;
			}
			return ReceiverOpt;
		}
	}

	public bool SearchExtensionMethods => (Flags & BoundMethodGroupFlags.SearchExtensionMethods) != 0;

	public override object Display => MessageID.IDS_MethodGroup.Localize();

	public ImmutableArray<TypeSymbol> TypeArgumentsOpt { get; }

	public string Name { get; }

	public ImmutableArray<MethodSymbol> Methods { get; }

	public Symbol LookupSymbolOpt { get; }

	public DiagnosticInfo LookupError { get; }

	public BoundMethodGroupFlags Flags { get; }

	public BoundMethodGroup(SyntaxNode syntax, ImmutableArray<TypeSymbol> typeArgumentsOpt, BoundExpression receiverOpt, string name, ImmutableArray<MethodSymbol> methods, LookupResult lookupResult, BoundMethodGroupFlags flags, bool hasErrors = false)
		: this(syntax, typeArgumentsOpt, name, methods, lookupResult.SingleSymbolOrDefault, lookupResult.Error, flags, receiverOpt, lookupResult.Kind, hasErrors)
	{
	}

	public BoundMethodGroup(SyntaxNode syntax, ImmutableArray<TypeSymbol> typeArgumentsOpt, string name, ImmutableArray<MethodSymbol> methods, Symbol lookupSymbolOpt, DiagnosticInfo lookupError, BoundMethodGroupFlags flags, BoundExpression receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(BoundKind.MethodGroup, syntax, receiverOpt, resultKind, hasErrors || receiverOpt.HasErrors())
	{
		TypeArgumentsOpt = typeArgumentsOpt;
		Name = name;
		Methods = methods;
		LookupSymbolOpt = lookupSymbolOpt;
		LookupError = lookupError;
		Flags = flags;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMethodGroup(this);
	}

	public BoundMethodGroup Update(ImmutableArray<TypeSymbol> typeArgumentsOpt, string name, ImmutableArray<MethodSymbol> methods, Symbol lookupSymbolOpt, DiagnosticInfo lookupError, BoundMethodGroupFlags flags, BoundExpression receiverOpt, LookupResultKind resultKind)
	{
		if (typeArgumentsOpt != TypeArgumentsOpt || name != Name || methods != Methods || lookupSymbolOpt != LookupSymbolOpt || lookupError != LookupError || flags != Flags || receiverOpt != ReceiverOpt || resultKind != ResultKind)
		{
			return new BoundMethodGroup(Syntax, typeArgumentsOpt, name, methods, lookupSymbolOpt, lookupError, flags, receiverOpt, resultKind, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundIndexerAccess : BoundExpression
{
	public override Symbol ExpressionSymbol => Indexer;

	public ImmutableArray<PropertySymbol> OriginalIndexersOpt { get; private set; }

	public override LookupResultKind ResultKind
	{
		get
		{
			if (OriginalIndexersOpt.IsDefault)
			{
				return base.ResultKind;
			}
			return LookupResultKind.OverloadResolutionFailure;
		}
	}

	public BoundExpression ReceiverOpt { get; }

	public PropertySymbol Indexer { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public bool Expanded { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public Binder BinderOpt { get; }

	public bool UseSetterForDefaultArgumentGeneration { get; }

	public BoundIndexerAccess Update(bool useSetterForDefaultArgumentGeneration)
	{
		if (useSetterForDefaultArgumentGeneration != UseSetterForDefaultArgumentGeneration)
		{
			return new BoundIndexerAccess(Syntax, ReceiverOpt, Indexer, Arguments, ArgumentNamesOpt, ArgumentRefKindsOpt, Expanded, ArgsToParamsOpt, BinderOpt, useSetterForDefaultArgumentGeneration, Type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated,
				OriginalIndexersOpt = OriginalIndexersOpt
			};
		}
		return this;
	}

	public static BoundIndexerAccess ErrorAccess(SyntaxNode node, BoundExpression receiverOpt, PropertySymbol indexer, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> namedArguments, ImmutableArray<RefKind> refKinds, ImmutableArray<PropertySymbol> originalIndexers)
	{
		return new BoundIndexerAccess(node, receiverOpt, indexer, arguments, namedArguments, refKinds, expanded: false, default, null, useSetterForDefaultArgumentGeneration: false, indexer.Type, hasErrors: true)
		{
			OriginalIndexersOpt = originalIndexers
		};
	}

	public BoundIndexerAccess(SyntaxNode syntax, BoundExpression receiverOpt, PropertySymbol indexer, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, Binder binderOpt, bool useSetterForDefaultArgumentGeneration, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IndexerAccess, syntax, type, hasErrors || receiverOpt.HasErrors() || arguments.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Indexer = indexer;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		BinderOpt = binderOpt;
		UseSetterForDefaultArgumentGeneration = useSetterForDefaultArgumentGeneration;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIndexerAccess(this);
	}

	public BoundIndexerAccess Update(BoundExpression receiverOpt, PropertySymbol indexer, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, Binder binderOpt, bool useSetterForDefaultArgumentGeneration, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || indexer != Indexer || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || binderOpt != BinderOpt || useSetterForDefaultArgumentGeneration != UseSetterForDefaultArgumentGeneration || type != Type)
		{
			return new BoundIndexerAccess(Syntax, receiverOpt, indexer, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, binderOpt, useSetterForDefaultArgumentGeneration, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

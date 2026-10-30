using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicIndexerAccess : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(Arguments.Insert(0, ReceiverOpt));

	public BoundExpression ReceiverOpt { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public ImmutableArray<PropertySymbol> ApplicableIndexers { get; }

	internal string TryGetIndexedPropertyName()
	{
		foreach (PropertySymbol applicableIndexer in ApplicableIndexers)
		{
			if (!applicableIndexer.IsIndexer && applicableIndexer.IsIndexedProperty)
			{
				return applicableIndexer.Name;
			}
		}
		return null;
	}

	public BoundDynamicIndexerAccess(SyntaxNode syntax, BoundExpression receiverOpt, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<PropertySymbol> applicableIndexers, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicIndexerAccess, syntax, type, hasErrors || receiverOpt.HasErrors() || arguments.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		ApplicableIndexers = applicableIndexers;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicIndexerAccess(this);
	}

	public BoundDynamicIndexerAccess Update(BoundExpression receiverOpt, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<PropertySymbol> applicableIndexers, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || applicableIndexers != ApplicableIndexers || type != Type)
		{
			return new BoundDynamicIndexerAccess(Syntax, receiverOpt, arguments, argumentNamesOpt, argumentRefKindsOpt, applicableIndexers, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

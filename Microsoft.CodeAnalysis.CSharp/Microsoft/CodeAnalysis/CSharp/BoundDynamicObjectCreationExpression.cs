using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicObjectCreationExpression : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(Arguments.AddRange(BoundObjectCreationExpression.GetChildInitializers(InitializerExpressionOpt)));

	public string Name { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public BoundObjectInitializerExpressionBase InitializerExpressionOpt { get; }

	public ImmutableArray<MethodSymbol> ApplicableMethods { get; }

	public BoundDynamicObjectCreationExpression(SyntaxNode syntax, string name, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicObjectCreationExpression, syntax, type, hasErrors || arguments.HasErrors() || initializerExpressionOpt.HasErrors())
	{
		Name = name;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		InitializerExpressionOpt = initializerExpressionOpt;
		ApplicableMethods = applicableMethods;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicObjectCreationExpression(this);
	}

	public BoundDynamicObjectCreationExpression Update(string name, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type)
	{
		if (name != Name || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || initializerExpressionOpt != InitializerExpressionOpt || applicableMethods != ApplicableMethods || type != Type)
		{
			return new BoundDynamicObjectCreationExpression(Syntax, name, arguments, argumentNamesOpt, argumentRefKindsOpt, initializerExpressionOpt, applicableMethods, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

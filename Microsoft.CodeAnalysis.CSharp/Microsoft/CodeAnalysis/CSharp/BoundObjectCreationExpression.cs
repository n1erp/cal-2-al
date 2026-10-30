using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundObjectCreationExpression : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => Constructor;

	public MethodSymbol Constructor { get; }

	public ImmutableArray<MethodSymbol> ConstructorsGroup { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public bool Expanded { get; }

	public ImmutableArray<int> ArgsToParamsOpt { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundObjectInitializerExpressionBase InitializerExpressionOpt { get; }

	public Binder BinderOpt { get; }

	internal BoundObjectCreationExpression UpdateArgumentsAndInitializer(ImmutableArray<BoundExpression> newArguments, ImmutableArray<RefKind> newRefKinds, BoundObjectInitializerExpressionBase newInitializerExpression, TypeSymbol changeTypeOpt = null)
	{
		return Update(Constructor, newArguments, default, newRefKinds, expanded: false, default, ConstantValueOpt, newInitializerExpression, BinderOpt, changeTypeOpt ?? Type);
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, ConstantValue constantValueOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, Binder binderOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, constructor, ImmutableArray<MethodSymbol>.Empty, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, constantValueOpt, initializerExpressionOpt, binderOpt, type, hasErrors)
	{
	}

	public BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, ConstantValue constantValueOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, Binder binderOpt, TypeSymbol type)
	{
		return Update(constructor, ImmutableArray<MethodSymbol>.Empty, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, constantValueOpt, initializerExpressionOpt, binderOpt, type);
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, Binder binderOpt, params BoundExpression[] arguments)
		: this(syntax, constructor, ImmutableArray.Create(arguments), default, default, expanded: false, default, null, null, binderOpt, constructor.ContainingType)
	{
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, Binder binderOpt, ImmutableArray<BoundExpression> arguments)
		: this(syntax, constructor, arguments, default, default, expanded: false, default, null, null, binderOpt, constructor.ContainingType)
	{
	}

	internal static ImmutableArray<BoundExpression> GetChildInitializers(BoundExpression objectOrCollectionInitializer)
	{
		if (objectOrCollectionInitializer is BoundObjectInitializerExpression boundObjectInitializerExpression)
		{
			return boundObjectInitializerExpression.Initializers;
		}
		if (objectOrCollectionInitializer is BoundCollectionInitializerExpression boundCollectionInitializerExpression)
		{
			return boundCollectionInitializerExpression.Initializers;
		}
		return ImmutableArray<BoundExpression>.Empty;
	}

	public BoundObjectCreationExpression(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<MethodSymbol> constructorsGroup, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, ConstantValue constantValueOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, Binder binderOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectCreationExpression, syntax, type, hasErrors || arguments.HasErrors() || initializerExpressionOpt.HasErrors())
	{
		Constructor = constructor;
		ConstructorsGroup = constructorsGroup;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		Expanded = expanded;
		ArgsToParamsOpt = argsToParamsOpt;
		ConstantValueOpt = constantValueOpt;
		InitializerExpressionOpt = initializerExpressionOpt;
		BinderOpt = binderOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectCreationExpression(this);
	}

	public BoundObjectCreationExpression Update(MethodSymbol constructor, ImmutableArray<MethodSymbol> constructorsGroup, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, ConstantValue constantValueOpt, BoundObjectInitializerExpressionBase initializerExpressionOpt, Binder binderOpt, TypeSymbol type)
	{
		if (constructor != Constructor || constructorsGroup != ConstructorsGroup || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || expanded != Expanded || argsToParamsOpt != ArgsToParamsOpt || constantValueOpt != ConstantValueOpt || initializerExpressionOpt != InitializerExpressionOpt || binderOpt != BinderOpt || type != Type)
		{
			return new BoundObjectCreationExpression(Syntax, constructor, constructorsGroup, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, constantValueOpt, initializerExpressionOpt, binderOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

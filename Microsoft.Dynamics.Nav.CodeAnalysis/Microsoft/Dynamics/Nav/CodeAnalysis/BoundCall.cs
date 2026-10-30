using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundCall : BoundExpression, IInvocationExpression, IOperation
{
	private class Argument : IArgument, IOperation
	{
		public IParameterSymbol Parameter { get; }

		public IOperation Value { get; }

		bool IOperation.IsInvalid
		{
			get
			{
				if (Parameter != null)
				{
					return Value.IsInvalid;
				}
				return true;
			}
		}

		OperationKind IOperation.Kind => OperationKind.Argument;

		SyntaxNode IOperation.Syntax => Value?.Syntax;

		public ITypeSymbol Type => null;

		public Optional<object> ConstantValue => default;

		public Argument(IParameterSymbol parameter, IOperation value)
		{
			DebugAssertHelper.Assert(value != null);
			Value = value;
			Parameter = parameter;
		}

		void IOperation.Accept(OperationVisitor visitor)
		{
			visitor.VisitArgument(this);
		}

		TResult IOperation.Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
		{
			return visitor.VisitArgument(this, argument);
		}
	}

	private readonly LookupResultKind _ResultKind;

	private static readonly ConditionalWeakTable<BoundCall, object> ArgumentsMappings = new ConditionalWeakTable<BoundCall, object>();

	public override Symbol ExpressionSymbol => Method;

	public ImmutableArray<MethodSymbol> OriginalMethodsOpt { get; private set; }

	public BoundExpression ReceiverOpt { get; private set; }

	public MethodSymbol Method { get; private set; }

	public ImmutableArray<BoundExpression> Arguments { get; private set; }

	public EffectiveParameters EffectiveParameters { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	public bool IsStatement { get; private set; }

	IMethodSymbol IInvocationExpression.TargetMethod => Method;

	IOperation IInvocationExpression.Instance
	{
		get
		{
			if ((object)Method != null && !Method.IsStatic)
			{
				return ReceiverOpt;
			}
			return null;
		}
	}

	ImmutableArray<IArgument> IInvocationExpression.Arguments => (ImmutableArray<IArgument>)ArgumentsMappings.GetValue(this, (BoundCall call) =>
	{
		ArrayBuilder<IArgument> instance = ArrayBuilder<IArgument>.GetInstance(Arguments.Length);
		for (int i = 0; i < Arguments.Length; i++)
		{
			instance.Add(new Argument(EffectiveParameters?.Parameters[i], Arguments[i]));
		}
		return instance.ToImmutableAndFree();
	});

	protected override OperationKind ExpressionKind => OperationKind.InvocationExpression;

	public static BoundCall ErrorCall(SyntaxNode node, BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ImmutableArray<MethodSymbol> originalMethods, LookupResultKind resultKind)
	{
		if (!originalMethods.IsEmpty)
		{
			resultKind = resultKind.WorseResultKind(LookupResultKind.OverloadResolutionFailure);
		}
		DebugAssertHelper.Assert(arguments.IsDefaultOrEmpty || receiverOpt != arguments[0]);
		return new BoundCall(node, receiverOpt, method, arguments, null, resultKind, isStatement: false, method.ReturnType, hasErrors: true)
		{
			OriginalMethodsOpt = originalMethods
		};
	}

	public BoundCall(SyntaxNode syntax, BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, EffectiveParameters effectiveParameters, LookupResultKind resultKind, bool isStatement, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Call, syntax, type, hasErrors || receiverOpt.HasErrors() || arguments.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Method = method;
		Arguments = arguments;
		EffectiveParameters = effectiveParameters;
		_ResultKind = resultKind;
		IsStatement = isStatement;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCall(this);
	}

	public BoundCall Update(BoundExpression receiverOpt, MethodSymbol method, ImmutableArray<BoundExpression> arguments, EffectiveParameters effectiveParameters, LookupResultKind resultKind, bool isStatement, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || method != Method || arguments != Arguments || effectiveParameters != EffectiveParameters || resultKind != ResultKind || isStatement != IsStatement || type != Type)
		{
			return new BoundCall(Syntax, receiverOpt, method, arguments, effectiveParameters, resultKind, isStatement, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInvocationExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInvocationExpression(this, argument);
	}
}

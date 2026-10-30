using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTupleExpression : BoundExpression
{
	public override object Display
	{
		get
		{
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			StringBuilder builder = instance.Builder;
			ImmutableArray<BoundExpression> arguments = Arguments;
			object[] array = new object[arguments.Length];
			builder.Append('(');
			builder.Append("{0}");
			array[0] = arguments[0].Display;
			for (int i = 1; i < arguments.Length; i++)
			{
				builder.Append(", {" + i + "}");
				array[i] = arguments[i].Display;
			}
			builder.Append(')');
			return FormattableStringFactory.Create(instance.ToStringAndFree(), array);
		}
	}

	public ImmutableArray<BoundExpression> Arguments { get; }

	internal void VisitAllElements<T>(Action<BoundExpression, T> action, T args)
	{
		foreach (BoundExpression argument in Arguments)
		{
			if (argument.Kind == BoundKind.TupleLiteral)
			{
				((BoundTupleExpression)argument).VisitAllElements(action, args);
			}
			else
			{
				action(argument, args);
			}
		}
	}

	protected BoundTupleExpression(BoundKind kind, SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, TypeSymbol type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Arguments = arguments;
	}
}

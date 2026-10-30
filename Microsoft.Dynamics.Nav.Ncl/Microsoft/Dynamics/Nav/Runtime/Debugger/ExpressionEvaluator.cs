using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class ExpressionEvaluator
{
	private class CacheEntry
	{
		private readonly Breakpoint breakpoint;

		private readonly Func<NavMethodScope, bool> compiledFunction;

		public Breakpoint Breakpoint
		{
			[DebuggerStepThrough]
			get
			{
				return breakpoint;
			}
		}

		public Func<NavMethodScope, bool> CompiledFunction
		{
			[DebuggerStepThrough]
			get
			{
				return compiledFunction;
			}
		}

		public CacheEntry(Breakpoint breakpoint, Func<NavMethodScope, bool> compiledFunction)
		{
			this.breakpoint = breakpoint;
			this.compiledFunction = compiledFunction;
		}
	}

	private static readonly Regex expressionRegex = BuildExpressionRegex();

	private static readonly Regex hasDecimalRegex = new Regex("\\.", RegexOptions.Compiled);

	private static readonly Expression invariantCultureCall = Expression.Property(null, typeof(CultureInfo).GetProperty("InvariantCulture"));

	private static readonly Regex isNumberRegex = new Regex("-?\\d+\\.?\\d*|-?\\.\\d+", RegexOptions.Compiled);

	private static readonly ParameterExpression methodScopeParameterExpression = Expression.Parameter(typeof(NavMethodScope), "methodScope");

	private static readonly MethodInfo stringCompareMethod = typeof(string).GetMethod("Compare", new Type[3]
	{
		typeof(string),
		typeof(string),
		typeof(StringComparison)
	});

	private static readonly Type[] typeHierarchy = new Type[6]
	{
		typeof(bool),
		typeof(int),
		typeof(long),
		typeof(decimal),
		typeof(Decimal18),
		typeof(string)
	};

	private readonly ConcurrentDictionary<long, CacheEntry> compiledExpressions = new ConcurrentDictionary<long, CacheEntry>();

	public bool EvaluateCondition(Breakpoint breakpoint, NavMethodScope methodScope)
	{
		if (string.IsNullOrEmpty(breakpoint.ConditionExpression))
		{
			return true;
		}
		if (compiledExpressions.TryGetValue(breakpoint.Id, out var value) && value.Breakpoint == breakpoint && TryInvokeCompiledExpression(value.CompiledFunction, methodScope, out var result))
		{
			return result;
		}
		value = new CacheEntry(breakpoint, ParseAndCompileCondition(breakpoint.ConditionExpression, methodScope));
		compiledExpressions[breakpoint.Id] = value;
		if (!TryInvokeCompiledExpression(value.CompiledFunction, methodScope, out var result2))
		{
			throw new InvalidConditionExpressionException(string.Format(CultureInfo.CurrentCulture, "Could not evaluate expression {0}", breakpoint.ConditionExpression));
		}
		return result2;
	}

	private static Expression BuildExpression(NavMethodScope methodScope, Match match, string pathGroupName, string literalGroupName, string stringLiteralGroupName)
	{
		Group obj = match.Groups[literalGroupName];
		if (obj.Success)
		{
			bool result;
			if (isNumberRegex.IsMatch(obj.Value))
			{
				try
				{
					if (hasDecimalRegex.IsMatch(obj.Value))
					{
						return Expression.Constant(decimal.Parse(obj.Value, CultureInfo.InvariantCulture));
					}
					try
					{
						return Expression.Constant(int.Parse(obj.Value, CultureInfo.InvariantCulture));
					}
					catch (OverflowException)
					{
						return Expression.Constant(long.Parse(obj.Value, CultureInfo.InvariantCulture));
					}
				}
				catch (OverflowException)
				{
				}
			}
			else if (bool.TryParse(obj.Value, out result))
			{
				return Expression.Constant(result);
			}
			return Expression.Constant(obj.Value);
		}
		obj = match.Groups[stringLiteralGroupName];
		if (obj.Success)
		{
			return Expression.Constant(obj.Value);
		}
		obj = match.Groups[pathGroupName];
		return BuildPathExpression(methodScope, obj.Value);
	}

	private static Regex BuildExpressionRegex()
	{
		string format = string.Join("|", "(?<{0}LITERAL>-?\\d+\\.?\\d*|-?\\.\\d+|true|false)", "'(?<{0}STRINGLITERAL>[^']*(?:''[^']*)*)'", "(?<{0}PATH>((([^.\\\"\\s=\\<\\>]+)|(\\\"[^\\\"]+\\\"))(\\.(?=[\\w\\\"\\[])|(?![\\w\\\"])))+)");
		string arg = string.Format(CultureInfo.InvariantCulture, format, "LEFT");
		string arg2 = string.Format(CultureInfo.InvariantCulture, format, "RIGHT");
		string pattern = string.Format(CultureInfo.InvariantCulture, "^(?<LEFT>{0})\\s*(?<OPANDRIGHT>(?<OP>{1})\\s*(?<RIGHT>{2})?)?$", arg, "[=<>]|\\<\\=|\\>\\=|\\<\\>", arg2);
		return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
	}

	private static Expression BuildInnerValueFromNavValueExpression(Expression input)
	{
		if (!input.Type.IsSubclassOf(typeof(NavValue)))
		{
			return input;
		}
		Expression expression = Expression.Property(input, "ValueAsObject");
		if (input.Type == typeof(NavDecimal))
		{
			return Expression.Convert(Expression.Convert(expression, typeof(Decimal18)), typeof(decimal));
		}
		return Expression.Convert(expression, GetInnerTypeOfNavValueType(input.Type));
	}

	private static Expression BuildLeftExpression(NavMethodScope methodScope, Match m)
	{
		return BuildExpression(methodScope, m, "LEFTPATH", "LEFTLITERAL", "LEFTSTRINGLITERAL");
	}

	private static Expression BuildPathExpression(NavMethodScope methodScope, string pathExpression)
	{
		try
		{
			MethodScopeScaffolding methodScopeScaffolding = new MethodScopeScaffolding();
			methodScopeScaffolding.Update(methodScope, getNodes: false);
			methodScopeScaffolding.GetDescendantScaffolding(ref pathExpression, methodScope, out var currentInstance, out var childSequence);
			Expression expression = methodScopeParameterExpression;
			foreach (Scaffolding item in childSequence)
			{
				expression = item.ValueBinder.BuildGetValueExpression(expression);
				for (int num = item.ValueCoercers.Count - 1; num >= 0; num--)
				{
					ScaffoldingValueCoercerBase scaffoldingValueCoercerBase = item.ValueCoercers[num];
					expression = scaffoldingValueCoercerBase.BuildCoerceValueExpression(expression);
				}
			}
			if (currentInstance is NavVariant { IsInitialized: not false } navVariant)
			{
				expression = Expression.Convert(Expression.Condition(Expression.Property(expression, "IsInitialized"), Expression.Property(expression, "InnerValue"), expression, typeof(NavValue)), navVariant.InnerValue.GetType());
			}
			return expression;
		}
		catch (ScaffoldingPathNotFoundException ex)
		{
			throw new InvalidConditionExpressionException(ex.Message, ex);
		}
	}

	private static Expression BuildRightExpression(NavMethodScope methodScope, Match m)
	{
		if (!m.Groups["OPANDRIGHT"].Success)
		{
			return null;
		}
		return BuildExpression(methodScope, m, "RIGHTPATH", "RIGHTLITERAL", "RIGHTSTRINGLITERAL");
	}

	private static Expression CombineExpressions(Expression leftExpression, Expression rightExpression, Match match)
	{
		Group obj = match.Groups["OP"];
		if (!obj.Success)
		{
			return ConvertExpressionType(leftExpression, typeof(bool));
		}
		Type type = FindCommonType(leftExpression.Type, rightExpression.Type);
		leftExpression = ConvertExpressionType(leftExpression, type);
		rightExpression = ConvertExpressionType(rightExpression, type);
		ExpressionType operatorType = GetOperatorType(obj.Value);
		ConstantExpression right = Expression.Constant(0);
		if (type == typeof(string))
		{
			MethodCallExpression left = Expression.Call(stringCompareMethod, leftExpression, rightExpression, Expression.Constant(StringComparison.OrdinalIgnoreCase));
			switch (operatorType)
			{
			case ExpressionType.Equal:
				return Expression.Equal(left, right);
			case ExpressionType.NotEqual:
				return Expression.NotEqual(left, right);
			case ExpressionType.GreaterThan:
				return Expression.GreaterThan(left, right);
			case ExpressionType.LessThan:
				return Expression.LessThan(left, right);
			case ExpressionType.GreaterThanOrEqual:
				return Expression.GreaterThanOrEqual(left, right);
			case ExpressionType.LessThanOrEqual:
				return Expression.LessThanOrEqual(left, right);
			}
		}
		else if (typeof(NavValue).IsAssignableFrom(type))
		{
			MethodInfo method = typeof(NavValue).GetMethod("Equals", new Type[1] { typeof(NavValue) });
			MethodInfo method2 = typeof(NavValue).GetMethod("CompareTo", new Type[1] { typeof(NavValue) });
			switch (operatorType)
			{
			case ExpressionType.Equal:
				return Expression.Call(leftExpression, method, rightExpression);
			case ExpressionType.NotEqual:
				return Expression.Not(Expression.Call(leftExpression, method, rightExpression));
			case ExpressionType.GreaterThan:
				return Expression.GreaterThan(Expression.Call(leftExpression, method2, rightExpression), right);
			case ExpressionType.LessThan:
				return Expression.LessThan(Expression.Call(leftExpression, method2, rightExpression), right);
			case ExpressionType.GreaterThanOrEqual:
				return Expression.GreaterThanOrEqual(Expression.Call(leftExpression, method2, rightExpression), right);
			case ExpressionType.LessThanOrEqual:
				return Expression.LessThanOrEqual(Expression.Call(leftExpression, method2, rightExpression), right);
			}
		}
		else
		{
			switch (operatorType)
			{
			case ExpressionType.Equal:
				return Expression.Equal(leftExpression, rightExpression);
			case ExpressionType.NotEqual:
				return Expression.NotEqual(leftExpression, rightExpression);
			case ExpressionType.GreaterThan:
				return Expression.GreaterThan(leftExpression, rightExpression);
			case ExpressionType.LessThan:
				return Expression.LessThan(leftExpression, rightExpression);
			case ExpressionType.GreaterThanOrEqual:
				return Expression.GreaterThanOrEqual(leftExpression, rightExpression);
			case ExpressionType.LessThanOrEqual:
				return Expression.LessThanOrEqual(leftExpression, rightExpression);
			}
		}
		throw new DebuggerFatalException(string.Format(CultureInfo.CurrentCulture, "Unrecognized operator {0}.", operatorType));
	}

	private static Expression ConvertExpressionType(Expression expression, Type targetType)
	{
		if (expression.Type == targetType)
		{
			return expression;
		}
		if (expression.NodeType == ExpressionType.Constant)
		{
			ConstantExpression constantExpression = (ConstantExpression)expression;
			if (targetType == typeof(Decimal18))
			{
				return Expression.Constant((Decimal18)(decimal)Convert.ChangeType(constantExpression.Value, typeof(decimal), CultureInfo.InvariantCulture));
			}
			return Expression.Constant(Convert.ChangeType(constantExpression.Value, targetType, CultureInfo.InvariantCulture));
		}
		if (expression.Type.IsSubclassOf(typeof(NavValue)))
		{
			if (targetType == typeof(NavValue))
			{
				if (expression.NodeType == ExpressionType.Convert)
				{
					UnaryExpression unaryExpression = (UnaryExpression)expression;
					if (unaryExpression.Method == null && typeof(NavValue).IsAssignableFrom(unaryExpression.Operand.Type))
					{
						return unaryExpression.Operand;
					}
				}
				return expression;
			}
			return ConvertExpressionType(BuildInnerValueFromNavValueExpression(expression), targetType);
		}
		if (targetType == typeof(Decimal18))
		{
			return Expression.Convert(ConvertExpressionType(expression, typeof(decimal)), typeof(Decimal18));
		}
		MethodInfo method = typeof(Convert).GetMethod("To" + targetType.Name, new Type[2]
		{
			expression.Type,
			typeof(IFormatProvider)
		});
		if (method != null && method.GetParameters()[0].ParameterType == expression.Type)
		{
			return Expression.Call(method, expression, invariantCultureCall);
		}
		method = typeof(Convert).GetMethod("To" + targetType.Name, new Type[1] { expression.Type });
		if (method != null)
		{
			return Expression.Call(method, (method.GetParameters()[0].ParameterType == expression.Type) ? expression : Expression.Convert(expression, method.GetParameters()[0].ParameterType));
		}
		method = typeof(Convert).GetMethod("ChangeType", new Type[3]
		{
			typeof(object),
			typeof(Type),
			typeof(IFormatProvider)
		});
		return Expression.Convert(Expression.Call(method, Expression.Convert(expression, typeof(object)), Expression.Constant(targetType, typeof(Type)), invariantCultureCall), targetType);
	}

	private static Type FindCommonType(Type typeA, Type typeB)
	{
		if (typeof(NavValue).IsAssignableFrom(typeA) && typeof(NavValue).IsAssignableFrom(typeB))
		{
			return typeof(NavValue);
		}
		if (typeof(NavValue).IsAssignableFrom(typeA))
		{
			return FindCommonType(GetInnerTypeOfNavValueType(typeA), typeB);
		}
		if (typeof(NavValue).IsAssignableFrom(typeB))
		{
			return FindCommonType(typeA, GetInnerTypeOfNavValueType(typeB));
		}
		int num = Array.IndexOf(typeHierarchy, typeA);
		if (num < 0)
		{
			throw new InvalidConditionExpressionException(string.Format(CultureInfo.CurrentCulture, "Type {0} is not supported for comparisons.", typeA.Name));
		}
		int num2 = Array.IndexOf(typeHierarchy, typeB);
		if (num2 < 0)
		{
			throw new InvalidConditionExpressionException(string.Format(CultureInfo.CurrentCulture, "Type {0} is not supported for comparisons.", typeB.Name));
		}
		return typeHierarchy[Math.Max(num, num2)];
	}

	private static ExpressionType GetOperatorType(string operatorString)
	{
		return operatorString switch
		{
			"=" => ExpressionType.Equal, 
			"<>" => ExpressionType.NotEqual, 
			">" => ExpressionType.GreaterThan, 
			"<" => ExpressionType.LessThan, 
			">=" => ExpressionType.GreaterThanOrEqual, 
			"<=" => ExpressionType.LessThanOrEqual, 
			_ => throw new DebuggerFatalException(string.Format(CultureInfo.CurrentCulture, "Unrecognized operator {0}.", operatorString)), 
		};
	}

	private static Func<NavMethodScope, bool> ParseAndCompileCondition(string expression, NavMethodScope methodScope)
	{
		Match match = expressionRegex.Match(expression);
		if (!match.Success)
		{
			throw new InvalidConditionExpressionException(string.Format(CultureInfo.CurrentCulture, "Could not parse expression {0}", expression));
		}
		Expression leftExpression = BuildLeftExpression(methodScope, match);
		Expression rightExpression = BuildRightExpression(methodScope, match);
		Expression body = CombineExpressions(leftExpression, rightExpression, match);
		return Expression.Lambda<Func<NavMethodScope, bool>>(body, new ParameterExpression[1] { methodScopeParameterExpression }).Compile();
	}

	private static bool TryInvokeCompiledExpression(Func<NavMethodScope, bool> compiledExpression, NavMethodScope methodScope, out bool result)
	{
		try
		{
			result = compiledExpression(methodScope);
			return true;
		}
		catch (InvalidCastException)
		{
		}
		catch (IndexOutOfRangeException)
		{
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		result = false;
		return false;
	}

	private static Type GetInnerTypeOfNavValueType(Type navValueType)
	{
		NavType navTypeFromType = NavNclTypeHelper.GetNavTypeFromType(navValueType);
		return CommonTypeInformation.ResolveClrType(navTypeFromType);
	}
}

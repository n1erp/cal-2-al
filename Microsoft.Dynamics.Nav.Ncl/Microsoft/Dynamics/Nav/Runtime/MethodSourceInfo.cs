using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class MethodSourceInfo : IEquatable<MethodSourceInfo>
{
	private readonly ObjectSourceInfo applicationObject;

	private readonly Type localsType;

	private readonly ReadOnlyCollection<SourceSpan> sourceSpans;

	internal ushort SignatureLineNumber { get; set; }

	public ObjectSourceInfo ApplicationObject => applicationObject;

	public SourcePosition HighestSourcePosition => sourceSpans[sourceSpans.Count - 1].To;

	public int LastStatementNumber => sourceSpans.Count - 1;

	public Type LocalsType
	{
		[DebuggerStepThrough]
		get
		{
			return localsType;
		}
	}

	public SourcePosition LowestSourcePosition => sourceSpans[0].From;

	public string Name => NavMethodScope.MethodName(localsType);

	public bool IsTrigger
	{
		get
		{
			Type baseType = LocalsType;
			while (baseType != typeof(NavMethodScope))
			{
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(NavTriggerMethodScope<>))
				{
					return true;
				}
				baseType = baseType.BaseType;
			}
			return false;
		}
	}

	public ReadOnlyCollection<SourceSpan> SourceSpans
	{
		[DebuggerStepThrough]
		get
		{
			return sourceSpans;
		}
	}

	public IEnumerable<StatementSourceInfo> Statements
	{
		get
		{
			for (int i = 0; i < sourceSpans.Count - 1; i++)
			{
				yield return new StatementSourceInfo(this, i, sourceSpans[i]);
			}
			yield return new StatementSourceInfo(this, int.MaxValue, sourceSpans[sourceSpans.Count - 1]);
		}
	}

	public MethodSourceInfo(Type localsType, ObjectSourceInfo applicationObject)
	{
		this.applicationObject = applicationObject;
		this.localsType = localsType;
		sourceSpans = new ReadOnlyCollection<SourceSpan>(GetSourceSpanAttribute(this.localsType).Spans);
	}

	public static SourceSpan GetCurrentStatementSourceSpan(NavMethodScope scope)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		SourceSpansAttribute sourceSpanAttribute = GetSourceSpanAttribute(scope.GetType());
		return (SourceSpan)sourceSpanAttribute.EncodedSpans[scope.IsAtExitStatement ? (sourceSpanAttribute.EncodedSpans.Length - 1) : scope.StatementNumber];
	}

	public static int GetMethodStatementCount(Type navMethodScopeType)
	{
		return GetSourceSpanAttribute(navMethodScopeType).EncodedSpans.Length;
	}

	public override bool Equals(object obj)
	{
		if (obj is MethodSourceInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return applicationObject.GetHashCode() ^ localsType.FullName.GetHashCode();
	}

	public StatementSourceInfo GetStatement(int statementNumber)
	{
		int index = ((statementNumber == int.MaxValue) ? LastStatementNumber : statementNumber);
		if (statementNumber == LastStatementNumber)
		{
			statementNumber = int.MaxValue;
		}
		return new StatementSourceInfo(this, statementNumber, SourceSpans[index]);
	}

	public SourceSpan GetSourceSpan(int statementNumber)
	{
		return sourceSpans[(statementNumber == int.MaxValue) ? (sourceSpans.Count - 1) : statementNumber];
	}

	public bool Equals(MethodSourceInfo other)
	{
		if (applicationObject.Equals(other))
		{
			return localsType.FullName == other.localsType.FullName;
		}
		return false;
	}

	private static SourceSpansAttribute GetSourceSpanAttribute(Type navMethodScopeType)
	{
		if (navMethodScopeType == null)
		{
			throw new ArgumentNullException("navMethodScopeType");
		}
		object[] customAttributes = navMethodScopeType.GetCustomAttributes(typeof(SourceSpansAttribute), inherit: false);
		int num = 0;
		if (num < customAttributes.Length)
		{
			return (SourceSpansAttribute)customAttributes[num];
		}
		throw new InvalidOperationException("Not an application method.");
	}
}

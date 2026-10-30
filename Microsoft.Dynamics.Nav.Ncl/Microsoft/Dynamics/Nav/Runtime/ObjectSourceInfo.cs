using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ObjectSourceInfo : IEquatable<ObjectSourceInfo>
{
	private readonly Type clrType;

	private readonly SourceSpan[] methodRanges;

	private readonly MethodSourceInfo[] methods;

	private readonly AppQualifiedApplicationObjectId appQualifiedObjectId;

	public ReadOnlyCollection<MethodSourceInfo> Methods => new ReadOnlyCollection<MethodSourceInfo>(methods);

	public AppQualifiedApplicationObjectId AppQualifiedObjectId
	{
		[DebuggerStepThrough]
		get
		{
			return appQualifiedObjectId;
		}
	}

	public Type ClrType
	{
		[DebuggerStepThrough]
		get
		{
			return clrType;
		}
	}

	public ObjectSourceInfo(AppQualifiedApplicationObjectId appQualifiedObjectId, Type clrType)
	{
		this.appQualifiedObjectId = appQualifiedObjectId;
		this.clrType = clrType;
		methods = GetMethodsSorted();
		methodRanges = Array.ConvertAll(methods, (MethodSourceInfo method) => new SourceSpan(method.LowestSourcePosition, method.HighestSourcePosition));
	}

	public override bool Equals(object obj)
	{
		if (obj is ObjectSourceInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public StatementSourceInfo FindStatementForPosition(SourcePosition position)
	{
		int num = FindIndexOfPositionInRanges(position, methodRanges);
		if (num == -1)
		{
			return null;
		}
		MethodSourceInfo method = Methods[num];
		return FindStatementForPositionInMethod(position, method);
	}

	public StatementSourceInfo FindStatementFromRelativeLineNumberInMethod(string methodName, int relativeLineNumber)
	{
		MethodSourceInfo methodSourceInfo = methods.FirstOrDefault((MethodSourceInfo m) => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
		if (methodSourceInfo == null)
		{
			return null;
		}
		int signatureLineNumber = methodSourceInfo.SignatureLineNumber;
		SourcePosition position = new SourcePosition((ushort)(signatureLineNumber + relativeLineNumber), 0);
		return FindStatementForPositionInMethod(position, methodSourceInfo);
	}

	public override int GetHashCode()
	{
		return appQualifiedObjectId.GetHashCode();
	}

	public bool Equals(ObjectSourceInfo other)
	{
		return AppQualifiedObjectId == other.AppQualifiedObjectId;
	}

	private static StatementSourceInfo FindStatementForPositionInMethod(SourcePosition position, MethodSourceInfo method)
	{
		int num = FindIndexOfPositionInRanges(position, method.SourceSpans);
		if (num == -1)
		{
			return null;
		}
		SourceSpan sourceSpan = method.SourceSpans[num];
		if (num == method.LastStatementNumber)
		{
			num = int.MaxValue;
		}
		return new StatementSourceInfo(method, num, sourceSpan);
	}

	private static int FindIndexOfPositionInRanges(SourcePosition position, IList<SourceSpan> ranges)
	{
		if (ranges.Count == 0)
		{
			return -1;
		}
		int num = 0;
		int num2 = ranges.Count - 1;
		while (num != num2)
		{
			int num3 = (num + num2) / 2;
			SourceSpan sourceSpan = ranges[num3];
			if (position > sourceSpan.To)
			{
				num = num3 + 1;
				continue;
			}
			if (position >= sourceSpan.From)
			{
				return num3;
			}
			num2 = num3;
		}
		return num2;
	}

	private IEnumerable<Type> GetAllNestedMethodScopeTypes(Type rootType)
	{
		Type[] nestedTypes = rootType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
		foreach (Type type in nestedTypes)
		{
			if (type.IsSubclassOf(typeof(NavApplicationObjectBase)))
			{
				foreach (Type allNestedMethodScopeType in GetAllNestedMethodScopeTypes(type))
				{
					yield return allNestedMethodScopeType;
				}
			}
			else if (type.IsSubclassOf(typeof(NavMethodScope)))
			{
				yield return type;
			}
		}
	}

	private MethodSourceInfo[] GetMethodsSorted()
	{
		MethodSourceInfo[] array = ((clrType == null) ? new MethodSourceInfo[0] : (from methodScopeType in GetAllNestedMethodScopeTypes(clrType)
			where methodScopeType.GetCustomAttribute<DebuggerNonUserCodeAttribute>() == null
			let methodDebugInfo = new MethodSourceInfo(methodScopeType, this)
			orderby methodDebugInfo.LowestSourcePosition
			select methodDebugInfo).ToArray());
		for (int num = 0; num < array.Length - 1; num++)
		{
			array[num + 1].SignatureLineNumber = (ushort)(array[num].HighestSourcePosition.Line + 1);
		}
		return array;
	}
}

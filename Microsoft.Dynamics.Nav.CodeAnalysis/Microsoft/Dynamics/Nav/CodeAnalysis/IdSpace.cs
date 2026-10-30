using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public sealed class IdSpace : IEquatable<IdSpace>, IComparable<IdSpace>
{
	internal const int MinInternalObjectId = 1;

	internal const int MaxInternalObjectId = int.MaxValue;

	internal const int MinExtensionObjectId = 70000000;

	internal const int MaxExtensionObjectId = 74999999;

	public string AllowedRange { get; }

	public int ApplicationObjectIdRangeStart { get; }

	public int ApplicationObjectIdRangeEnd { get; }

	public static IdSpace BaseAppDevelopmentIdSpace { get; } = new IdSpace(1, int.MaxValue);

	public static IdSpace ExtensionDevelopmentIdSpace { get; } = new IdSpace(70000000, 74999999);

	public static int GetMemberId(string memberName)
	{
		int fNVHashCode = Hash.GetFNVHashCode(memberName);
		if (fNVHashCode == int.MinValue)
		{
			return int.MaxValue;
		}
		return Math.Abs(fNVHashCode);
	}

	public static int GetMemberId(int ancestorObjectId, string memberName)
	{
		return GetMemberId(ancestorObjectId.ToString(CultureInfo.InvariantCulture) + memberName);
	}

	public IdSpace(int applicationObjectIdRangeStart, int applicationObjectIdRangeEnd)
	{
		ApplicationObjectIdRangeStart = applicationObjectIdRangeStart;
		ApplicationObjectIdRangeEnd = applicationObjectIdRangeEnd;
		AllowedRange = "[" + ApplicationObjectIdRangeStart + ".." + ApplicationObjectIdRangeEnd + "]";
	}

	public void ValidateRange(IList<Diagnostic> diagnostics, Location location)
	{
		if (ApplicationObjectIdRangeStart <= 0 || ApplicationObjectIdRangeEnd <= 0 || ApplicationObjectIdRangeEnd < ApplicationObjectIdRangeStart)
		{
			diagnostics.Add(new DiagnosticWithInfo(new DiagnosticInfo(MessageProvider.Instance, 1047, AllowedRange), location));
		}
	}

	public bool IsObjectIdAllowedForApplicationObject(int applicationObjectId, SymbolKind kind)
	{
		if (kind != SymbolKind.PageCustomization)
		{
			if (applicationObjectId >= ApplicationObjectIdRangeStart)
			{
				return applicationObjectId <= ApplicationObjectIdRangeEnd;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		IdSpace idSpace = obj as IdSpace;
		if (idSpace == null)
		{
			return false;
		}
		return Equals(idSpace);
	}

	public bool Equals(IdSpace other)
	{
		if (ApplicationObjectIdRangeStart == other?.ApplicationObjectIdRangeStart)
		{
			return ApplicationObjectIdRangeEnd == other?.ApplicationObjectIdRangeEnd;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ApplicationObjectIdRangeStart, ApplicationObjectIdRangeEnd);
	}

	public static bool operator ==(IdSpace first, IdSpace second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(IdSpace first, IdSpace second)
	{
		return !(first == second);
	}

	public static bool operator >(IdSpace operand1, IdSpace operand2)
	{
		return operand1.CompareTo(operand2) == 1;
	}

	public static bool operator <(IdSpace operand1, IdSpace operand2)
	{
		return operand1.CompareTo(operand2) == -1;
	}

	public static bool operator >=(IdSpace operand1, IdSpace operand2)
	{
		return operand1.CompareTo(operand2) >= 0;
	}

	public static bool operator <=(IdSpace operand1, IdSpace operand2)
	{
		return operand1.CompareTo(operand2) <= 0;
	}

	public bool IsWithinSpace(int value)
	{
		if (value >= ApplicationObjectIdRangeStart)
		{
			return value <= ApplicationObjectIdRangeEnd;
		}
		return false;
	}

	public int CompareTo(IdSpace other)
	{
		if (other == null)
		{
			return 1;
		}
		return ApplicationObjectIdRangeStart.CompareTo(other.ApplicationObjectIdRangeStart);
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public sealed class IdSpaces : IEquatable<IdSpaces>
{
	public ImmutableArray<IdSpace> ApplicationObjectIdRanges { get; }

	public string AllowedRanges { get; }

	public static IdSpaces BaseAppDevelopmentIdSpaces => new IdSpaces(new IdSpace[1] { IdSpace.BaseAppDevelopmentIdSpace });

	public static IdSpaces ExtensionDevelopmentIdSpace => new IdSpaces(new IdSpace[1] { IdSpace.ExtensionDevelopmentIdSpace });

	public IdSpaces(IdSpace[] applicationObjectIdRanges)
	{
		ApplicationObjectIdRanges = ImmutableArray<IdSpace>.Empty;
		if (applicationObjectIdRanges != null)
		{
			Array.Sort(applicationObjectIdRanges);
			ApplicationObjectIdRanges = applicationObjectIdRanges.AsImmutableOrEmpty();
		}
		AllowedRanges = GetRangesAsString(ApplicationObjectIdRanges);
	}

	public void ValidateRanges(IList<Diagnostic> diagnostics, Location location)
	{
		int num = 0;
		for (int i = 0; i < ApplicationObjectIdRanges.Length; i++)
		{
			ApplicationObjectIdRanges[i].ValidateRange(diagnostics, location);
			if (ApplicationObjectIdRanges[i].ApplicationObjectIdRangeStart <= num)
			{
				diagnostics.Add(new DiagnosticWithInfo(new DiagnosticInfo(MessageProvider.Instance, 1046, (i == 0) ? ApplicationObjectIdRanges[i].AllowedRange : (ApplicationObjectIdRanges[i - 1].AllowedRange + ", " + ApplicationObjectIdRanges[i].AllowedRange)), location));
			}
			num = ApplicationObjectIdRanges[i].ApplicationObjectIdRangeEnd;
		}
	}

	public bool IsObjectIdAllowedForApplicationObject(int applicationObjectId, SymbolKind kind)
	{
		foreach (IdSpace applicationObjectIdRange in ApplicationObjectIdRanges)
		{
			if (applicationObjectIdRange.IsObjectIdAllowedForApplicationObject(applicationObjectId, kind))
			{
				return true;
			}
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		IdSpaces idSpaces = obj as IdSpaces;
		if (idSpaces == null)
		{
			return false;
		}
		return Equals(idSpaces);
	}

	public bool Equals(IdSpaces other)
	{
		return ApplicationObjectIdRanges.SequenceEqual(other.ApplicationObjectIdRanges, (IdSpace t, IdSpace o) => t == o);
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(ApplicationObjectIdRanges);
	}

	public static bool operator ==(IdSpaces first, IdSpaces second)
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

	public static bool operator !=(IdSpaces first, IdSpaces second)
	{
		return !(first == second);
	}

	private static string GetRangesAsString(IEnumerable<IdSpace> ranges)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			foreach (IdSpace range in ranges)
			{
				builder.Append(range.AllowedRange);
				builder.Append(", ");
			}
			return builder.ToString().TrimEnd(", ".ToCharArray());
		}
		finally
		{
			instance.Free();
		}
	}

	public bool IsWithinSpaces(int value)
	{
		foreach (IdSpace applicationObjectIdRange in ApplicationObjectIdRanges)
		{
			if (applicationObjectIdRange.IsWithinSpace(value))
			{
				return true;
			}
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class DiagnosticWithInfo : Diagnostic
{
	private readonly DiagnosticInfo diagnosticInfo;

	private readonly Location location;

	private readonly bool isSuppressed;

	public override Location Location => location;

	public override IReadOnlyList<Location> AdditionalLocations => Info.AdditionalLocations;

	internal override IReadOnlyList<string> CustomTags => Info.CustomTags;

	public override DiagnosticDescriptor Descriptor => Info.Descriptor;

	public override string Id => Info.MessageIdentifier;

	internal override string Category => Info.Category;

	internal sealed override int Code => Info.Code;

	public sealed override DiagnosticSeverity Severity => Info.Severity;

	public sealed override DiagnosticSeverity DefaultSeverity => Info.DefaultSeverity;

	internal sealed override bool IsEnabledByDefault => true;

	public override bool IsSuppressed => isSuppressed;

	public sealed override int WarningLevel => Info.WarningLevel;

	internal override IReadOnlyList<object> Arguments => Info.Arguments;

	public DiagnosticInfo Info
	{
		get
		{
			if (diagnosticInfo.Severity == (DiagnosticSeverity)(-1))
			{
				return diagnosticInfo.GetResolvedInfo();
			}
			return diagnosticInfo;
		}
	}

	internal bool HasLazyInfo
	{
		get
		{
			if (diagnosticInfo.Severity != (DiagnosticSeverity)(-1))
			{
				return diagnosticInfo.Severity == (DiagnosticSeverity)(-2);
			}
			return true;
		}
	}

	internal DiagnosticWithInfo(DiagnosticInfo info, Location location, bool isSuppressed = false)
	{
		DebugAssertHelper.Assert(info != null);
		DebugAssertHelper.Assert(location != null);
		diagnosticInfo = info;
		this.location = location;
		this.isSuppressed = isSuppressed;
	}

	public override string GetMessage(IFormatProvider formatProvider = null)
	{
		return Info.GetMessage(formatProvider);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Location.GetHashCode(), Info.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Diagnostic);
	}

	public override bool Equals(Diagnostic obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is DiagnosticWithInfo diagnosticWithInfo) || GetType() != diagnosticWithInfo.GetType())
		{
			return false;
		}
		if (Location.Equals(diagnosticWithInfo.location) && Info.Equals(diagnosticWithInfo.Info))
		{
			return AdditionalLocations.SequenceEqual(diagnosticWithInfo.AdditionalLocations);
		}
		return false;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return diagnosticInfo.Severity switch
		{
			(DiagnosticSeverity)(-1) => "Unresolved diagnostic at " + Location, 
			(DiagnosticSeverity)(-2) => "Void diagnostic at " + Location, 
			_ => ToString(), 
		};
	}

	internal override Diagnostic WithLocation(Location locationParameter)
	{
		if (locationParameter == null)
		{
			throw new ArgumentNullException("locationParameter");
		}
		if (locationParameter != location)
		{
			return new DiagnosticWithInfo(diagnosticInfo, locationParameter);
		}
		return this;
	}

	internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
	{
		if (Severity != severity)
		{
			return new DiagnosticWithInfo(Info.GetInstanceWithSeverity(severity), location);
		}
		return this;
	}

	internal override Diagnostic WithIsSuppressed(bool isSuppressedParameter)
	{
		if (IsSuppressed != isSuppressedParameter)
		{
			return new DiagnosticWithInfo(Info, location, isSuppressedParameter);
		}
		return this;
	}
}

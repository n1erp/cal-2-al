using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public sealed class EmitOptions : IEquatable<EmitOptions>
{
	public const int CurrentMetadataVersion = 130000;

	internal static readonly EmitOptions Default = new EmitOptions();

	public int RuntimeMetadataVersion { get; }

	public string RuntimeMetadataSuffix { get; }

	internal bool SkipStmtHit { get; }

	public bool EmitCodeAsAst { get; }

	public bool ConcurrentEmit { get; }

	public bool NonDebuggableEmit { get; }

	public ExtensionEmitValues ExtensionEmitValues { get; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Intended spelling")]
	public EmitOptions(int runtimeMetadataVersion = 130000, string runtimeMetadataSuffix = null, bool skipStmtHit = false, ExtensionEmitValues extensionEmitValues = null, bool emitCodeAsAst = false, bool concurrentEmit = false, bool nonDebuggableEmit = false)
	{
		RuntimeMetadataVersion = runtimeMetadataVersion;
		RuntimeMetadataSuffix = runtimeMetadataSuffix;
		SkipStmtHit = skipStmtHit;
		ExtensionEmitValues = extensionEmitValues;
		EmitCodeAsAst = emitCodeAsAst;
		ConcurrentEmit = concurrentEmit;
		NonDebuggableEmit = nonDebuggableEmit;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as EmitOptions);
	}

	public bool Equals(EmitOptions other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (RuntimeMetadataVersion == other.RuntimeMetadataVersion && SkipStmtHit == other.SkipStmtHit && RuntimeMetadataSuffix == other.RuntimeMetadataSuffix && ConcurrentEmit == other.ConcurrentEmit && EmitCodeAsAst == other.EmitCodeAsAst)
		{
			return NonDebuggableEmit == other.NonDebuggableEmit;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(RuntimeMetadataVersion.GetHashCode(), SkipStmtHit.GetHashCode(), RuntimeMetadataSuffix.GetHashCode(), ConcurrentEmit.GetHashCode(), EmitCodeAsAst.GetHashCode(), NonDebuggableEmit.GetHashCode());
	}

	public static bool operator ==(EmitOptions left, EmitOptions right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(EmitOptions left, EmitOptions right)
	{
		return !object.Equals(left, right);
	}
}

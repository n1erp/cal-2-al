using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class AttributeMethodSignatureResolutionResult
{
	private int Score;

	internal static AttributeMethodSignatureResolutionResult Default = new AttributeMethodSignatureResolutionResult(null, versionCompatible: false, int.MaxValue);

	internal AttributeRequiredMethodSignature Signature { get; }

	internal bool IsVersionCompatible { get; }

	internal bool IsExactMatch => Score == 0;

	internal AttributeMethodSignatureResolutionResult(AttributeRequiredMethodSignature signature, bool versionCompatible)
		: this(signature, versionCompatible, 0)
	{
	}

	private AttributeMethodSignatureResolutionResult(AttributeRequiredMethodSignature signature, bool versionCompatible, int score)
	{
		Signature = signature;
		IsVersionCompatible = versionCompatible;
		Score = score;
	}

	internal bool IsBetterThan(AttributeMethodSignatureResolutionResult other)
	{
		if (other == null || other.Equals(Default))
		{
			return true;
		}
		if (IsVersionCompatible && !other.IsVersionCompatible)
		{
			return true;
		}
		if (other.IsVersionCompatible && !IsVersionCompatible)
		{
			return false;
		}
		return Score < other.Score;
	}

	internal NavDiagnosticInfo ScoreCalculator(ErrorCode code, Location location, params object[] args)
	{
		Interlocked.Increment(ref Score);
		return null;
	}
}

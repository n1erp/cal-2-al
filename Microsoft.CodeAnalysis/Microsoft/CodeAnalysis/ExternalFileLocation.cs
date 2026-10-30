using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class ExternalFileLocation : Location, IEquatable<ExternalFileLocation>
{
	private readonly TextSpan _sourceSpan;

	private readonly FileLinePositionSpan _lineSpan;

	public override TextSpan SourceSpan => _sourceSpan;

	public override LocationKind Kind => LocationKind.ExternalFile;

	internal ExternalFileLocation(string filePath, TextSpan sourceSpan, LinePositionSpan lineSpan)
	{
		_sourceSpan = sourceSpan;
		_lineSpan = new FileLinePositionSpan(filePath, lineSpan);
	}

	public override FileLinePositionSpan GetLineSpan()
	{
		return _lineSpan;
	}

	public override FileLinePositionSpan GetMappedLineSpan()
	{
		return _lineSpan;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ExternalFileLocation);
	}

	public bool Equals(ExternalFileLocation obj)
	{
		if ((object)obj == this)
		{
			return true;
		}
		if (obj != null && _sourceSpan == obj._sourceSpan)
		{
			return _lineSpan.Equals(obj._lineSpan);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_lineSpan.GetHashCode(), _sourceSpan.GetHashCode());
	}
}

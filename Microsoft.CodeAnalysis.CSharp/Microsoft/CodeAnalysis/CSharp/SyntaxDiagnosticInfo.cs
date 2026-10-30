using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SyntaxDiagnosticInfo : DiagnosticInfo
{
	internal readonly int Offset;

	internal readonly int Width;

	static SyntaxDiagnosticInfo()
	{
		ObjectBinder.RegisterTypeReader(typeof(SyntaxDiagnosticInfo), (ObjectReader r) => new SyntaxDiagnosticInfo(r));
	}

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args)
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, (int)code, args)
	{
		Offset = offset;
		Width = width;
	}

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code)
		: this(offset, width, code, Array.Empty<object>())
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code, params object[] args)
		: this(0, 0, code, args)
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code)
		: this(0, 0, code)
	{
	}

	public SyntaxDiagnosticInfo WithOffset(int offset)
	{
		return new SyntaxDiagnosticInfo(offset, Width, (ErrorCode)Code, Arguments);
	}

	protected override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteInt32(Offset);
		writer.WriteInt32(Width);
	}

	protected SyntaxDiagnosticInfo(ObjectReader reader)
		: base(reader)
	{
		Offset = reader.ReadInt32();
		Width = reader.ReadInt32();
	}
}

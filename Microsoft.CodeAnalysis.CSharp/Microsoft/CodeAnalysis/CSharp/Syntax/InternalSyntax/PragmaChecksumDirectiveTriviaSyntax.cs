using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PragmaChecksumDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken pragmaKeyword;

	internal readonly SyntaxToken checksumKeyword;

	internal readonly SyntaxToken file;

	internal readonly SyntaxToken guid;

	internal readonly SyntaxToken bytes;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken PragmaKeyword => pragmaKeyword;

	public SyntaxToken ChecksumKeyword => checksumKeyword;

	public SyntaxToken File => file;

	public SyntaxToken Guid => guid;

	public SyntaxToken Bytes => bytes;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal PragmaChecksumDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(checksumKeyword);
		this.checksumKeyword = checksumKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(guid);
		this.guid = guid;
		AdjustFlagsAndWidth(bytes);
		this.bytes = bytes;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal PragmaChecksumDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 7;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(checksumKeyword);
		this.checksumKeyword = checksumKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(guid);
		this.guid = guid;
		AdjustFlagsAndWidth(bytes);
		this.bytes = bytes;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal PragmaChecksumDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(checksumKeyword);
		this.checksumKeyword = checksumKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(guid);
		this.guid = guid;
		AdjustFlagsAndWidth(bytes);
		this.bytes = bytes;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)hashToken, 
			1 => pragmaKeyword, 
			2 => checksumKeyword, 
			3 => file, 
			4 => guid, 
			5 => bytes, 
			6 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PragmaChecksumDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPragmaChecksumDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPragmaChecksumDirectiveTrivia(this);
	}

	public PragmaChecksumDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken checksumKeyword, SyntaxToken file, SyntaxToken guid, SyntaxToken bytes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || pragmaKeyword != PragmaKeyword || checksumKeyword != ChecksumKeyword || file != File || guid != Guid || bytes != Bytes || endOfDirectiveToken != EndOfDirectiveToken)
		{
			PragmaChecksumDirectiveTriviaSyntax pragmaChecksumDirectiveTriviaSyntax = SyntaxFactory.PragmaChecksumDirectiveTrivia(hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				pragmaChecksumDirectiveTriviaSyntax = pragmaChecksumDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				pragmaChecksumDirectiveTriviaSyntax = pragmaChecksumDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return pragmaChecksumDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PragmaChecksumDirectiveTriviaSyntax(Kind, hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PragmaChecksumDirectiveTriviaSyntax(Kind, hashToken, pragmaKeyword, checksumKeyword, file, guid, bytes, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}

	internal PragmaChecksumDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 7;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			hashToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			pragmaKeyword = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			checksumKeyword = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			file = syntaxToken4;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			guid = syntaxToken5;
		}
		SyntaxToken syntaxToken6 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken6 != null)
		{
			AdjustFlagsAndWidth(syntaxToken6);
			bytes = syntaxToken6;
		}
		SyntaxToken syntaxToken7 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken7 != null)
		{
			AdjustFlagsAndWidth(syntaxToken7);
			endOfDirectiveToken = syntaxToken7;
		}
		isActive = reader.ReadBoolean();
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(hashToken);
		writer.WriteValue(pragmaKeyword);
		writer.WriteValue(checksumKeyword);
		writer.WriteValue(file);
		writer.WriteValue(guid);
		writer.WriteValue(bytes);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
	}

	static PragmaChecksumDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PragmaChecksumDirectiveTriviaSyntax), (ObjectReader r) => new PragmaChecksumDirectiveTriviaSyntax(r));
	}
}

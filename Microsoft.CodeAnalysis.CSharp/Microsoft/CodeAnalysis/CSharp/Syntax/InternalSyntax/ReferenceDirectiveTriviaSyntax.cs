using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ReferenceDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken referenceKeyword;

	internal readonly SyntaxToken file;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken ReferenceKeyword => referenceKeyword;

	public SyntaxToken File => file;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal ReferenceDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(referenceKeyword);
		this.referenceKeyword = referenceKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ReferenceDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(referenceKeyword);
		this.referenceKeyword = referenceKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal ReferenceDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(referenceKeyword);
		this.referenceKeyword = referenceKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)hashToken, 
			1 => referenceKeyword, 
			2 => file, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ReferenceDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReferenceDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitReferenceDirectiveTrivia(this);
	}

	public ReferenceDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken referenceKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || referenceKeyword != ReferenceKeyword || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			ReferenceDirectiveTriviaSyntax referenceDirectiveTriviaSyntax = SyntaxFactory.ReferenceDirectiveTrivia(hashToken, referenceKeyword, file, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				referenceDirectiveTriviaSyntax = referenceDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				referenceDirectiveTriviaSyntax = referenceDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return referenceDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ReferenceDirectiveTriviaSyntax(Kind, hashToken, referenceKeyword, file, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ReferenceDirectiveTriviaSyntax(Kind, hashToken, referenceKeyword, file, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}

	internal ReferenceDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
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
			referenceKeyword = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			file = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			endOfDirectiveToken = syntaxToken4;
		}
		isActive = reader.ReadBoolean();
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(hashToken);
		writer.WriteValue(referenceKeyword);
		writer.WriteValue(file);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
	}

	static ReferenceDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ReferenceDirectiveTriviaSyntax), (ObjectReader r) => new ReferenceDirectiveTriviaSyntax(r));
	}
}

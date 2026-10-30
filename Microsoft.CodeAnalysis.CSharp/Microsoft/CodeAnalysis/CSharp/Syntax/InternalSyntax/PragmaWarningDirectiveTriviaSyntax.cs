using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PragmaWarningDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken pragmaKeyword;

	internal readonly SyntaxToken warningKeyword;

	internal readonly SyntaxToken disableOrRestoreKeyword;

	internal readonly GreenNode errorCodes;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken PragmaKeyword => pragmaKeyword;

	public SyntaxToken WarningKeyword => warningKeyword;

	public SyntaxToken DisableOrRestoreKeyword => disableOrRestoreKeyword;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> ErrorCodes => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(errorCodes));

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal PragmaWarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, GreenNode errorCodes, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(disableOrRestoreKeyword);
		this.disableOrRestoreKeyword = disableOrRestoreKeyword;
		if (errorCodes != null)
		{
			AdjustFlagsAndWidth(errorCodes);
			this.errorCodes = errorCodes;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal PragmaWarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, GreenNode errorCodes, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 6;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(disableOrRestoreKeyword);
		this.disableOrRestoreKeyword = disableOrRestoreKeyword;
		if (errorCodes != null)
		{
			AdjustFlagsAndWidth(errorCodes);
			this.errorCodes = errorCodes;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal PragmaWarningDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, GreenNode errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(pragmaKeyword);
		this.pragmaKeyword = pragmaKeyword;
		AdjustFlagsAndWidth(warningKeyword);
		this.warningKeyword = warningKeyword;
		AdjustFlagsAndWidth(disableOrRestoreKeyword);
		this.disableOrRestoreKeyword = disableOrRestoreKeyword;
		if (errorCodes != null)
		{
			AdjustFlagsAndWidth(errorCodes);
			this.errorCodes = errorCodes;
		}
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => hashToken, 
			1 => pragmaKeyword, 
			2 => warningKeyword, 
			3 => disableOrRestoreKeyword, 
			4 => errorCodes, 
			5 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PragmaWarningDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPragmaWarningDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPragmaWarningDirectiveTrivia(this);
	}

	public PragmaWarningDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken pragmaKeyword, SyntaxToken warningKeyword, SyntaxToken disableOrRestoreKeyword, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> errorCodes, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || pragmaKeyword != PragmaKeyword || warningKeyword != WarningKeyword || disableOrRestoreKeyword != DisableOrRestoreKeyword || errorCodes != ErrorCodes || endOfDirectiveToken != EndOfDirectiveToken)
		{
			PragmaWarningDirectiveTriviaSyntax pragmaWarningDirectiveTriviaSyntax = SyntaxFactory.PragmaWarningDirectiveTrivia(hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				pragmaWarningDirectiveTriviaSyntax = pragmaWarningDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				pragmaWarningDirectiveTriviaSyntax = pragmaWarningDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return pragmaWarningDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PragmaWarningDirectiveTriviaSyntax(Kind, hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PragmaWarningDirectiveTriviaSyntax(Kind, hashToken, pragmaKeyword, warningKeyword, disableOrRestoreKeyword, errorCodes, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}

	internal PragmaWarningDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 6;
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
			warningKeyword = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			disableOrRestoreKeyword = syntaxToken4;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			errorCodes = greenNode;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			endOfDirectiveToken = syntaxToken5;
		}
		isActive = reader.ReadBoolean();
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(hashToken);
		writer.WriteValue(pragmaKeyword);
		writer.WriteValue(warningKeyword);
		writer.WriteValue(disableOrRestoreKeyword);
		writer.WriteValue(errorCodes);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
	}

	static PragmaWarningDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PragmaWarningDirectiveTriviaSyntax), (ObjectReader r) => new PragmaWarningDirectiveTriviaSyntax(r));
	}
}

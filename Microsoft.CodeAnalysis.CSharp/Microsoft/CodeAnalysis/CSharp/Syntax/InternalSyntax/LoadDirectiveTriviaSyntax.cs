using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LoadDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken loadKeyword;

	internal readonly SyntaxToken file;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken LoadKeyword => loadKeyword;

	public SyntaxToken File => file;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
		AdjustFlagsAndWidth(file);
		this.file = file;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal LoadDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(loadKeyword);
		this.loadKeyword = loadKeyword;
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
			1 => loadKeyword, 
			2 => file, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LoadDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLoadDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLoadDirectiveTrivia(this);
	}

	public LoadDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken loadKeyword, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || loadKeyword != LoadKeyword || file != File || endOfDirectiveToken != EndOfDirectiveToken)
		{
			LoadDirectiveTriviaSyntax loadDirectiveTriviaSyntax = SyntaxFactory.LoadDirectiveTrivia(hashToken, loadKeyword, file, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				loadDirectiveTriviaSyntax = loadDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				loadDirectiveTriviaSyntax = loadDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return loadDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new LoadDirectiveTriviaSyntax(Kind, hashToken, loadKeyword, file, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new LoadDirectiveTriviaSyntax(Kind, hashToken, loadKeyword, file, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}

	internal LoadDirectiveTriviaSyntax(ObjectReader reader)
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
			loadKeyword = syntaxToken2;
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
		writer.WriteValue(loadKeyword);
		writer.WriteValue(file);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
	}

	static LoadDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(LoadDirectiveTriviaSyntax), (ObjectReader r) => new LoadDirectiveTriviaSyntax(r));
	}
}

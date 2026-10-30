using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BlockSyntax : StatementSyntax
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode statements;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> Statements => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax>(statements);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal BlockSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode statements, SyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			this.statements = statements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal BlockSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode statements, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			this.statements = statements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal BlockSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode statements, SyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			this.statements = statements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => statements, 
			2 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBlock(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBlock(this);
	}

	public BlockSyntax Update(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || statements != Statements || closeBraceToken != CloseBraceToken)
		{
			BlockSyntax blockSyntax = SyntaxFactory.Block(openBraceToken, statements, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				blockSyntax = blockSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				blockSyntax = blockSyntax.WithAnnotationsGreen(annotations);
			}
			return blockSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BlockSyntax(Kind, openBraceToken, statements, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BlockSyntax(Kind, openBraceToken, statements, closeBraceToken, GetDiagnostics(), annotations);
	}

	internal BlockSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openBraceToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			statements = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeBraceToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openBraceToken);
		writer.WriteValue(statements);
		writer.WriteValue(closeBraceToken);
	}

	static BlockSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BlockSyntax), (ObjectReader r) => new BlockSyntax(r));
	}
}

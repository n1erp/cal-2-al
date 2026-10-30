using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BracketedArgumentListSyntax : BaseArgumentListSyntax
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode arguments;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> Arguments => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(arguments));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal BracketedArgumentListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode arguments, SyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal BracketedArgumentListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode arguments, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal BracketedArgumentListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode arguments, SyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => arguments, 
			2 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BracketedArgumentListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBracketedArgumentList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBracketedArgumentList(this);
	}

	public BracketedArgumentListSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || arguments != Arguments || closeBracketToken != CloseBracketToken)
		{
			BracketedArgumentListSyntax bracketedArgumentListSyntax = SyntaxFactory.BracketedArgumentList(openBracketToken, arguments, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				bracketedArgumentListSyntax = bracketedArgumentListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				bracketedArgumentListSyntax = bracketedArgumentListSyntax.WithAnnotationsGreen(annotations);
			}
			return bracketedArgumentListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BracketedArgumentListSyntax(Kind, openBracketToken, arguments, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BracketedArgumentListSyntax(Kind, openBracketToken, arguments, closeBracketToken, GetDiagnostics(), annotations);
	}

	internal BracketedArgumentListSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openBracketToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			arguments = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeBracketToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openBracketToken);
		writer.WriteValue(arguments);
		writer.WriteValue(closeBracketToken);
	}

	static BracketedArgumentListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BracketedArgumentListSyntax), (ObjectReader r) => new BracketedArgumentListSyntax(r));
	}
}

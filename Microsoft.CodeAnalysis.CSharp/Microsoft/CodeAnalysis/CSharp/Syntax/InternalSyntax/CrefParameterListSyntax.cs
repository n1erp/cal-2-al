using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CrefParameterListSyntax : BaseCrefParameterListSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode parameters;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> Parameters => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(parameters));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal CrefParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CrefParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal CrefParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => parameters, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameterList(this);
	}

	public CrefParameterListSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || parameters != Parameters || closeParenToken != CloseParenToken)
		{
			CrefParameterListSyntax crefParameterListSyntax = SyntaxFactory.CrefParameterList(openParenToken, parameters, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				crefParameterListSyntax = crefParameterListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				crefParameterListSyntax = crefParameterListSyntax.WithAnnotationsGreen(annotations);
			}
			return crefParameterListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CrefParameterListSyntax(Kind, openParenToken, parameters, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CrefParameterListSyntax(Kind, openParenToken, parameters, closeParenToken, GetDiagnostics(), annotations);
	}

	internal CrefParameterListSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openParenToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			parameters = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeParenToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openParenToken);
		writer.WriteValue(parameters);
		writer.WriteValue(closeParenToken);
	}

	static CrefParameterListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CrefParameterListSyntax), (ObjectReader r) => new CrefParameterListSyntax(r));
	}
}

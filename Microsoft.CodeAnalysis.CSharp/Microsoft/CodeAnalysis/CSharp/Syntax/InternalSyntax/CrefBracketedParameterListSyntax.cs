using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CrefBracketedParameterListSyntax : BaseCrefParameterListSyntax
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode parameters;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> Parameters => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(parameters));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal CrefBracketedParameterListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode parameters, SyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal CrefBracketedParameterListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode parameters, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal CrefBracketedParameterListSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode parameters, SyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => parameters, 
			2 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CrefBracketedParameterListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefBracketedParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefBracketedParameterList(this);
	}

	public CrefBracketedParameterListSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> parameters, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || parameters != Parameters || closeBracketToken != CloseBracketToken)
		{
			CrefBracketedParameterListSyntax crefBracketedParameterListSyntax = SyntaxFactory.CrefBracketedParameterList(openBracketToken, parameters, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				crefBracketedParameterListSyntax = crefBracketedParameterListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				crefBracketedParameterListSyntax = crefBracketedParameterListSyntax.WithAnnotationsGreen(annotations);
			}
			return crefBracketedParameterListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CrefBracketedParameterListSyntax(Kind, openBracketToken, parameters, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CrefBracketedParameterListSyntax(Kind, openBracketToken, parameters, closeBracketToken, GetDiagnostics(), annotations);
	}

	internal CrefBracketedParameterListSyntax(ObjectReader reader)
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
			parameters = greenNode;
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
		writer.WriteValue(parameters);
		writer.WriteValue(closeBracketToken);
	}

	static CrefBracketedParameterListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CrefBracketedParameterListSyntax), (ObjectReader r) => new CrefBracketedParameterListSyntax(r));
	}
}

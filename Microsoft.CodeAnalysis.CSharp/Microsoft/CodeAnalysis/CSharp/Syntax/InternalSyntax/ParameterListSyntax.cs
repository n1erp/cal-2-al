using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParameterListSyntax : BaseParameterListSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode parameters;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> Parameters => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(parameters));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal ParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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

	internal ParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken, SyntaxFactoryContext context)
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

	internal ParameterListSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode parameters, SyntaxToken closeParenToken)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParameterListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParameterList(this);
	}

	public ParameterListSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || parameters != Parameters || closeParenToken != CloseParenToken)
		{
			ParameterListSyntax parameterListSyntax = SyntaxFactory.ParameterList(openParenToken, parameters, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parameterListSyntax = parameterListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parameterListSyntax = parameterListSyntax.WithAnnotationsGreen(annotations);
			}
			return parameterListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ParameterListSyntax(Kind, openParenToken, parameters, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ParameterListSyntax(Kind, openParenToken, parameters, closeParenToken, GetDiagnostics(), annotations);
	}

	internal ParameterListSyntax(ObjectReader reader)
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

	static ParameterListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ParameterListSyntax), (ObjectReader r) => new ParameterListSyntax(r));
	}
}

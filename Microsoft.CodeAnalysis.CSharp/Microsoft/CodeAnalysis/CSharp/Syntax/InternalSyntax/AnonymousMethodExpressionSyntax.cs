using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AnonymousMethodExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	internal readonly SyntaxToken asyncKeyword;

	internal readonly SyntaxToken delegateKeyword;

	internal readonly ParameterListSyntax parameterList;

	internal readonly CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword => asyncKeyword;

	public SyntaxToken DelegateKeyword => delegateKeyword;

	public ParameterListSyntax ParameterList => parameterList;

	public override CSharpSyntaxNode Body => body;

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal AnonymousMethodExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body)
		: base(kind)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(delegateKeyword);
		this.delegateKeyword = delegateKeyword;
		if (parameterList != null)
		{
			AdjustFlagsAndWidth(parameterList);
			this.parameterList = parameterList;
		}
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)asyncKeyword, 
			1 => delegateKeyword, 
			2 => parameterList, 
			3 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousMethodExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousMethodExpression(this);
	}

	public AnonymousMethodExpressionSyntax Update(SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || delegateKeyword != DelegateKeyword || parameterList != ParameterList || body != Body)
		{
			AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax = SyntaxFactory.AnonymousMethodExpression(asyncKeyword, delegateKeyword, parameterList, body);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				anonymousMethodExpressionSyntax = anonymousMethodExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				anonymousMethodExpressionSyntax = anonymousMethodExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return anonymousMethodExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AnonymousMethodExpressionSyntax(Kind, asyncKeyword, delegateKeyword, parameterList, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AnonymousMethodExpressionSyntax(Kind, asyncKeyword, delegateKeyword, parameterList, body, GetDiagnostics(), annotations);
	}

	internal AnonymousMethodExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			asyncKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			delegateKeyword = syntaxToken2;
		}
		ParameterListSyntax parameterListSyntax = (ParameterListSyntax)reader.ReadValue();
		if (parameterListSyntax != null)
		{
			AdjustFlagsAndWidth(parameterListSyntax);
			parameterList = parameterListSyntax;
		}
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)reader.ReadValue();
		if (cSharpSyntaxNode != null)
		{
			AdjustFlagsAndWidth(cSharpSyntaxNode);
			body = cSharpSyntaxNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(asyncKeyword);
		writer.WriteValue(delegateKeyword);
		writer.WriteValue(parameterList);
		writer.WriteValue(body);
	}

	static AnonymousMethodExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AnonymousMethodExpressionSyntax), (ObjectReader r) => new AnonymousMethodExpressionSyntax(r));
	}
}

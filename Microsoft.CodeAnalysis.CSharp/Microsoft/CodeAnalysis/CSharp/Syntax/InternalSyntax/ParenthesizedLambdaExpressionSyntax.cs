using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ParenthesizedLambdaExpressionSyntax : LambdaExpressionSyntax
{
	internal readonly SyntaxToken asyncKeyword;

	internal readonly ParameterListSyntax parameterList;

	internal readonly SyntaxToken arrowToken;

	internal readonly CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword => asyncKeyword;

	public ParameterListSyntax ParameterList => parameterList;

	public override SyntaxToken ArrowToken => arrowToken;

	public override CSharpSyntaxNode Body => body;

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal ParenthesizedLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
		: base(kind)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameterList);
		this.parameterList = parameterList;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)asyncKeyword, 
			1 => parameterList, 
			2 => arrowToken, 
			3 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedLambdaExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedLambdaExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedLambdaExpression(this);
	}

	public ParenthesizedLambdaExpressionSyntax Update(SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || parameterList != ParameterList || arrowToken != ArrowToken || body != Body)
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax = SyntaxFactory.ParenthesizedLambdaExpression(asyncKeyword, parameterList, arrowToken, body);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				parenthesizedLambdaExpressionSyntax = parenthesizedLambdaExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				parenthesizedLambdaExpressionSyntax = parenthesizedLambdaExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return parenthesizedLambdaExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ParenthesizedLambdaExpressionSyntax(Kind, asyncKeyword, parameterList, arrowToken, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ParenthesizedLambdaExpressionSyntax(Kind, asyncKeyword, parameterList, arrowToken, body, GetDiagnostics(), annotations);
	}

	internal ParenthesizedLambdaExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			asyncKeyword = syntaxToken;
		}
		ParameterListSyntax parameterListSyntax = (ParameterListSyntax)reader.ReadValue();
		if (parameterListSyntax != null)
		{
			AdjustFlagsAndWidth(parameterListSyntax);
			parameterList = parameterListSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			arrowToken = syntaxToken2;
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
		writer.WriteValue(parameterList);
		writer.WriteValue(arrowToken);
		writer.WriteValue(body);
	}

	static ParenthesizedLambdaExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ParenthesizedLambdaExpressionSyntax), (ObjectReader r) => new ParenthesizedLambdaExpressionSyntax(r));
	}
}

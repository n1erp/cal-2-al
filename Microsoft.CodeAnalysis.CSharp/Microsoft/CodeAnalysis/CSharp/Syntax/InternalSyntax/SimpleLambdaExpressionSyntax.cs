using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SimpleLambdaExpressionSyntax : LambdaExpressionSyntax
{
	internal readonly SyntaxToken asyncKeyword;

	internal readonly ParameterSyntax parameter;

	internal readonly SyntaxToken arrowToken;

	internal readonly CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword => asyncKeyword;

	public ParameterSyntax Parameter => parameter;

	public override SyntaxToken ArrowToken => arrowToken;

	public override CSharpSyntaxNode Body => body;

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
		AdjustFlagsAndWidth(arrowToken);
		this.arrowToken = arrowToken;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal SimpleLambdaExpressionSyntax(SyntaxKind kind, SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
		: base(kind)
	{
		SlotCount = 4;
		if (asyncKeyword != null)
		{
			AdjustFlagsAndWidth(asyncKeyword);
			this.asyncKeyword = asyncKeyword;
		}
		AdjustFlagsAndWidth(parameter);
		this.parameter = parameter;
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
			1 => parameter, 
			2 => arrowToken, 
			3 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleLambdaExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleLambdaExpression(this);
	}

	public SimpleLambdaExpressionSyntax Update(SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || parameter != Parameter || arrowToken != ArrowToken || body != Body)
		{
			SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax = SyntaxFactory.SimpleLambdaExpression(asyncKeyword, parameter, arrowToken, body);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				simpleLambdaExpressionSyntax = simpleLambdaExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				simpleLambdaExpressionSyntax = simpleLambdaExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return simpleLambdaExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SimpleLambdaExpressionSyntax(Kind, asyncKeyword, parameter, arrowToken, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SimpleLambdaExpressionSyntax(Kind, asyncKeyword, parameter, arrowToken, body, GetDiagnostics(), annotations);
	}

	internal SimpleLambdaExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			asyncKeyword = syntaxToken;
		}
		ParameterSyntax parameterSyntax = (ParameterSyntax)reader.ReadValue();
		if (parameterSyntax != null)
		{
			AdjustFlagsAndWidth(parameterSyntax);
			parameter = parameterSyntax;
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
		writer.WriteValue(parameter);
		writer.WriteValue(arrowToken);
		writer.WriteValue(body);
	}

	static SimpleLambdaExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(SimpleLambdaExpressionSyntax), (ObjectReader r) => new SimpleLambdaExpressionSyntax(r));
	}
}

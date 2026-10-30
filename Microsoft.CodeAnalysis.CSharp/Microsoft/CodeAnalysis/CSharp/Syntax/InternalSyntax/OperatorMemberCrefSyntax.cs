using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OperatorMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken operatorKeyword;

	internal readonly SyntaxToken operatorToken;

	internal readonly CrefParameterListSyntax parameters;

	public SyntaxToken OperatorKeyword => operatorKeyword;

	public SyntaxToken OperatorToken => operatorToken;

	public CrefParameterListSyntax Parameters => parameters;

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal OperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)operatorKeyword, 
			1 => operatorToken, 
			2 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OperatorMemberCrefSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOperatorMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorMemberCref(this);
	}

	public OperatorMemberCrefSyntax Update(SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
	{
		if (operatorKeyword != OperatorKeyword || operatorToken != OperatorToken || parameters != Parameters)
		{
			OperatorMemberCrefSyntax operatorMemberCrefSyntax = SyntaxFactory.OperatorMemberCref(operatorKeyword, operatorToken, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				operatorMemberCrefSyntax = operatorMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				operatorMemberCrefSyntax = operatorMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return operatorMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new OperatorMemberCrefSyntax(Kind, operatorKeyword, operatorToken, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new OperatorMemberCrefSyntax(Kind, operatorKeyword, operatorToken, parameters, GetDiagnostics(), annotations);
	}

	internal OperatorMemberCrefSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			operatorKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			operatorToken = syntaxToken2;
		}
		CrefParameterListSyntax crefParameterListSyntax = (CrefParameterListSyntax)reader.ReadValue();
		if (crefParameterListSyntax != null)
		{
			AdjustFlagsAndWidth(crefParameterListSyntax);
			parameters = crefParameterListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(operatorKeyword);
		writer.WriteValue(operatorToken);
		writer.WriteValue(parameters);
	}

	static OperatorMemberCrefSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(OperatorMemberCrefSyntax), (ObjectReader r) => new OperatorMemberCrefSyntax(r));
	}
}

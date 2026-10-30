using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ConversionOperatorMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly SyntaxToken implicitOrExplicitKeyword;

	internal readonly SyntaxToken operatorKeyword;

	internal readonly TypeSyntax type;

	internal readonly CrefParameterListSyntax parameters;

	public SyntaxToken ImplicitOrExplicitKeyword => implicitOrExplicitKeyword;

	public SyntaxToken OperatorKeyword => operatorKeyword;

	public TypeSyntax Type => type;

	public CrefParameterListSyntax Parameters => parameters;

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal ConversionOperatorMemberCrefSyntax(SyntaxKind kind, SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(implicitOrExplicitKeyword);
		this.implicitOrExplicitKeyword = implicitOrExplicitKeyword;
		AdjustFlagsAndWidth(operatorKeyword);
		this.operatorKeyword = operatorKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
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
			0 => (GreenNode)implicitOrExplicitKeyword, 
			1 => operatorKeyword, 
			2 => type, 
			3 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ConversionOperatorMemberCrefSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConversionOperatorMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConversionOperatorMemberCref(this);
	}

	public ConversionOperatorMemberCrefSyntax Update(SyntaxToken implicitOrExplicitKeyword, SyntaxToken operatorKeyword, TypeSyntax type, CrefParameterListSyntax parameters)
	{
		if (implicitOrExplicitKeyword != ImplicitOrExplicitKeyword || operatorKeyword != OperatorKeyword || type != Type || parameters != Parameters)
		{
			ConversionOperatorMemberCrefSyntax conversionOperatorMemberCrefSyntax = SyntaxFactory.ConversionOperatorMemberCref(implicitOrExplicitKeyword, operatorKeyword, type, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				conversionOperatorMemberCrefSyntax = conversionOperatorMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				conversionOperatorMemberCrefSyntax = conversionOperatorMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return conversionOperatorMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ConversionOperatorMemberCrefSyntax(Kind, implicitOrExplicitKeyword, operatorKeyword, type, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ConversionOperatorMemberCrefSyntax(Kind, implicitOrExplicitKeyword, operatorKeyword, type, parameters, GetDiagnostics(), annotations);
	}

	internal ConversionOperatorMemberCrefSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			implicitOrExplicitKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			operatorKeyword = syntaxToken2;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
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
		writer.WriteValue(implicitOrExplicitKeyword);
		writer.WriteValue(operatorKeyword);
		writer.WriteValue(type);
		writer.WriteValue(parameters);
	}

	static ConversionOperatorMemberCrefSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ConversionOperatorMemberCrefSyntax), (ObjectReader r) => new ConversionOperatorMemberCrefSyntax(r));
	}
}

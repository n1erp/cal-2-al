using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CrefParameterSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken refKindKeyword;

	internal readonly TypeSyntax type;

	public SyntaxToken RefKindKeyword => refKindKeyword;

	public TypeSyntax Type => type;

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken refKindKeyword, TypeSyntax type, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken refKindKeyword, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal CrefParameterSyntax(SyntaxKind kind, SyntaxToken refKindKeyword, TypeSyntax type)
		: base(kind)
	{
		SlotCount = 2;
		if (refKindKeyword != null)
		{
			AdjustFlagsAndWidth(refKindKeyword);
			this.refKindKeyword = refKindKeyword;
		}
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)refKindKeyword, 
			1 => type, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefParameter(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameter(this);
	}

	public CrefParameterSyntax Update(SyntaxToken refKindKeyword, TypeSyntax type)
	{
		if (refKindKeyword != RefKindKeyword || type != Type)
		{
			CrefParameterSyntax crefParameterSyntax = SyntaxFactory.CrefParameter(refKindKeyword, type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				crefParameterSyntax = crefParameterSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				crefParameterSyntax = crefParameterSyntax.WithAnnotationsGreen(annotations);
			}
			return crefParameterSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CrefParameterSyntax(Kind, refKindKeyword, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CrefParameterSyntax(Kind, refKindKeyword, type, GetDiagnostics(), annotations);
	}

	internal CrefParameterSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			refKindKeyword = syntaxToken;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(refKindKeyword);
		writer.WriteValue(type);
	}

	static CrefParameterSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CrefParameterSyntax), (ObjectReader r) => new CrefParameterSyntax(r));
	}
}

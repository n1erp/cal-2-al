using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class QualifiedCrefSyntax : CrefSyntax
{
	internal readonly TypeSyntax container;

	internal readonly SyntaxToken dotToken;

	internal readonly MemberCrefSyntax member;

	public TypeSyntax Container => container;

	public SyntaxToken DotToken => dotToken;

	public MemberCrefSyntax Member => member;

	internal QualifiedCrefSyntax(SyntaxKind kind, TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(container);
		this.container = container;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal QualifiedCrefSyntax(SyntaxKind kind, TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(container);
		this.container = container;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal QualifiedCrefSyntax(SyntaxKind kind, TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(container);
		this.container = container;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
		AdjustFlagsAndWidth(member);
		this.member = member;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)container, 
			1 => dotToken, 
			2 => member, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedCrefSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQualifiedCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQualifiedCref(this);
	}

	public QualifiedCrefSyntax Update(TypeSyntax container, SyntaxToken dotToken, MemberCrefSyntax member)
	{
		if (container != Container || dotToken != DotToken || member != Member)
		{
			QualifiedCrefSyntax qualifiedCrefSyntax = SyntaxFactory.QualifiedCref(container, dotToken, member);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				qualifiedCrefSyntax = qualifiedCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				qualifiedCrefSyntax = qualifiedCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return qualifiedCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new QualifiedCrefSyntax(Kind, container, dotToken, member, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new QualifiedCrefSyntax(Kind, container, dotToken, member, GetDiagnostics(), annotations);
	}

	internal QualifiedCrefSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			container = typeSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			dotToken = syntaxToken;
		}
		MemberCrefSyntax memberCrefSyntax = (MemberCrefSyntax)reader.ReadValue();
		if (memberCrefSyntax != null)
		{
			AdjustFlagsAndWidth(memberCrefSyntax);
			member = memberCrefSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(container);
		writer.WriteValue(dotToken);
		writer.WriteValue(member);
	}

	static QualifiedCrefSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(QualifiedCrefSyntax), (ObjectReader r) => new QualifiedCrefSyntax(r));
	}
}

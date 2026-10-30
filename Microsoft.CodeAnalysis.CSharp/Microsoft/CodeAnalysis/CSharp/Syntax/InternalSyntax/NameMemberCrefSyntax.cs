using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NameMemberCrefSyntax : MemberCrefSyntax
{
	internal readonly TypeSyntax name;

	internal readonly CrefParameterListSyntax parameters;

	public TypeSyntax Name => name;

	public CrefParameterListSyntax Parameters => parameters;

	internal NameMemberCrefSyntax(SyntaxKind kind, TypeSyntax name, CrefParameterListSyntax parameters, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal NameMemberCrefSyntax(SyntaxKind kind, TypeSyntax name, CrefParameterListSyntax parameters, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
	}

	internal NameMemberCrefSyntax(SyntaxKind kind, TypeSyntax name, CrefParameterListSyntax parameters)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
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
			0 => (GreenNode)name, 
			1 => parameters, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NameMemberCrefSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNameMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNameMemberCref(this);
	}

	public NameMemberCrefSyntax Update(TypeSyntax name, CrefParameterListSyntax parameters)
	{
		if (name != Name || parameters != Parameters)
		{
			NameMemberCrefSyntax nameMemberCrefSyntax = SyntaxFactory.NameMemberCref(name, parameters);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				nameMemberCrefSyntax = nameMemberCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				nameMemberCrefSyntax = nameMemberCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return nameMemberCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new NameMemberCrefSyntax(Kind, name, parameters, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new NameMemberCrefSyntax(Kind, name, parameters, GetDiagnostics(), annotations);
	}

	internal NameMemberCrefSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			name = typeSyntax;
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
		writer.WriteValue(name);
		writer.WriteValue(parameters);
	}

	static NameMemberCrefSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(NameMemberCrefSyntax), (ObjectReader r) => new NameMemberCrefSyntax(r));
	}
}

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EnumMemberDeclarationSyntax : MemberDeclarationSyntax
{
	internal readonly GreenNode attributeLists;

	internal readonly SyntaxToken identifier;

	internal readonly EqualsValueClauseSyntax equalsValue;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public SyntaxToken Identifier => identifier;

	public EqualsValueClauseSyntax EqualsValue => equalsValue;

	internal EnumMemberDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (equalsValue != null)
		{
			AdjustFlagsAndWidth(equalsValue);
			this.equalsValue = equalsValue;
		}
	}

	internal EnumMemberDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (equalsValue != null)
		{
			AdjustFlagsAndWidth(equalsValue);
			this.equalsValue = equalsValue;
		}
	}

	internal EnumMemberDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue)
		: base(kind)
	{
		SlotCount = 3;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		if (equalsValue != null)
		{
			AdjustFlagsAndWidth(equalsValue);
			this.equalsValue = equalsValue;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => identifier, 
			2 => equalsValue, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EnumMemberDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumMemberDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEnumMemberDeclaration(this);
	}

	public EnumMemberDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue)
	{
		if (attributeLists != AttributeLists || identifier != Identifier || equalsValue != EqualsValue)
		{
			EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = SyntaxFactory.EnumMemberDeclaration(attributeLists, identifier, equalsValue);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				enumMemberDeclarationSyntax = enumMemberDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				enumMemberDeclarationSyntax = enumMemberDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return enumMemberDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new EnumMemberDeclarationSyntax(Kind, attributeLists, identifier, equalsValue, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new EnumMemberDeclarationSyntax(Kind, attributeLists, identifier, equalsValue, GetDiagnostics(), annotations);
	}

	internal EnumMemberDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			attributeLists = greenNode;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = (EqualsValueClauseSyntax)reader.ReadValue();
		if (equalsValueClauseSyntax != null)
		{
			AdjustFlagsAndWidth(equalsValueClauseSyntax);
			equalsValue = equalsValueClauseSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(attributeLists);
		writer.WriteValue(identifier);
		writer.WriteValue(equalsValue);
	}

	static EnumMemberDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(EnumMemberDeclarationSyntax), (ObjectReader r) => new EnumMemberDeclarationSyntax(r));
	}
}

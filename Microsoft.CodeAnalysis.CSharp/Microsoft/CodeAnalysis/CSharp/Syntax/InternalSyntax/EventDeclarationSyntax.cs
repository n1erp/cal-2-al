using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EventDeclarationSyntax : BasePropertyDeclarationSyntax
{
	internal readonly GreenNode attributeLists;

	internal readonly GreenNode modifiers;

	internal readonly SyntaxToken eventKeyword;

	internal readonly TypeSyntax type;

	internal readonly ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	internal readonly SyntaxToken identifier;

	internal readonly AccessorListSyntax accessorList;

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax>(attributeLists);

	public override Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(modifiers);

	public SyntaxToken EventKeyword => eventKeyword;

	public override TypeSyntax Type => type;

	public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => explicitInterfaceSpecifier;

	public SyntaxToken Identifier => identifier;

	public override AccessorListSyntax AccessorList => accessorList;

	internal EventDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(eventKeyword);
		this.eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(accessorList);
		this.accessorList = accessorList;
	}

	internal EventDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(eventKeyword);
		this.eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(accessorList);
		this.accessorList = accessorList;
	}

	internal EventDeclarationSyntax(SyntaxKind kind, GreenNode attributeLists, GreenNode modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList)
		: base(kind)
	{
		SlotCount = 7;
		if (attributeLists != null)
		{
			AdjustFlagsAndWidth(attributeLists);
			this.attributeLists = attributeLists;
		}
		if (modifiers != null)
		{
			AdjustFlagsAndWidth(modifiers);
			this.modifiers = modifiers;
		}
		AdjustFlagsAndWidth(eventKeyword);
		this.eventKeyword = eventKeyword;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (explicitInterfaceSpecifier != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifier);
			this.explicitInterfaceSpecifier = explicitInterfaceSpecifier;
		}
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(accessorList);
		this.accessorList = accessorList;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			1 => modifiers, 
			2 => eventKeyword, 
			3 => type, 
			4 => explicitInterfaceSpecifier, 
			5 => identifier, 
			6 => accessorList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEventDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEventDeclaration(this);
	}

	public EventDeclarationSyntax Update(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> attributeLists, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || eventKeyword != EventKeyword || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || identifier != Identifier || accessorList != AccessorList)
		{
			EventDeclarationSyntax eventDeclarationSyntax = SyntaxFactory.EventDeclaration(attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				eventDeclarationSyntax = eventDeclarationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				eventDeclarationSyntax = eventDeclarationSyntax.WithAnnotationsGreen(annotations);
			}
			return eventDeclarationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new EventDeclarationSyntax(Kind, attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new EventDeclarationSyntax(Kind, attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList, GetDiagnostics(), annotations);
	}

	internal EventDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 7;
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			attributeLists = greenNode;
		}
		GreenNode greenNode2 = (GreenNode)reader.ReadValue();
		if (greenNode2 != null)
		{
			AdjustFlagsAndWidth(greenNode2);
			modifiers = greenNode2;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			eventKeyword = syntaxToken;
		}
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
		ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = (ExplicitInterfaceSpecifierSyntax)reader.ReadValue();
		if (explicitInterfaceSpecifierSyntax != null)
		{
			AdjustFlagsAndWidth(explicitInterfaceSpecifierSyntax);
			explicitInterfaceSpecifier = explicitInterfaceSpecifierSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			identifier = syntaxToken2;
		}
		AccessorListSyntax accessorListSyntax = (AccessorListSyntax)reader.ReadValue();
		if (accessorListSyntax != null)
		{
			AdjustFlagsAndWidth(accessorListSyntax);
			accessorList = accessorListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(attributeLists);
		writer.WriteValue(modifiers);
		writer.WriteValue(eventKeyword);
		writer.WriteValue(type);
		writer.WriteValue(explicitInterfaceSpecifier);
		writer.WriteValue(identifier);
		writer.WriteValue(accessorList);
	}

	static EventDeclarationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(EventDeclarationSyntax), (ObjectReader r) => new EventDeclarationSyntax(r));
	}
}

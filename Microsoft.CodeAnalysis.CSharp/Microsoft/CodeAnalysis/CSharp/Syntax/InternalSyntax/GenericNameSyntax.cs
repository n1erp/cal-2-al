using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class GenericNameSyntax : SimpleNameSyntax
{
	internal readonly SyntaxToken identifier;

	internal readonly TypeArgumentListSyntax typeArgumentList;

	public override SyntaxToken Identifier => identifier;

	public TypeArgumentListSyntax TypeArgumentList => typeArgumentList;

	internal GenericNameSyntax(SyntaxKind kind, SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(typeArgumentList);
		this.typeArgumentList = typeArgumentList;
	}

	internal GenericNameSyntax(SyntaxKind kind, SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(typeArgumentList);
		this.typeArgumentList = typeArgumentList;
	}

	internal GenericNameSyntax(SyntaxKind kind, SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(typeArgumentList);
		this.typeArgumentList = typeArgumentList;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)identifier, 
			1 => typeArgumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGenericName(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGenericName(this);
	}

	public GenericNameSyntax Update(SyntaxToken identifier, TypeArgumentListSyntax typeArgumentList)
	{
		if (identifier != Identifier || typeArgumentList != TypeArgumentList)
		{
			GenericNameSyntax genericNameSyntax = SyntaxFactory.GenericName(identifier, typeArgumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				genericNameSyntax = genericNameSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				genericNameSyntax = genericNameSyntax.WithAnnotationsGreen(annotations);
			}
			return genericNameSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new GenericNameSyntax(Kind, identifier, typeArgumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new GenericNameSyntax(Kind, identifier, typeArgumentList, GetDiagnostics(), annotations);
	}

	internal GenericNameSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
		TypeArgumentListSyntax typeArgumentListSyntax = (TypeArgumentListSyntax)reader.ReadValue();
		if (typeArgumentListSyntax != null)
		{
			AdjustFlagsAndWidth(typeArgumentListSyntax);
			typeArgumentList = typeArgumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(identifier);
		writer.WriteValue(typeArgumentList);
	}

	static GenericNameSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(GenericNameSyntax), (ObjectReader r) => new GenericNameSyntax(r));
	}
}

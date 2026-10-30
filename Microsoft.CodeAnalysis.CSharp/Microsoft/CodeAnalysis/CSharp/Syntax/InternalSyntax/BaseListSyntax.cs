using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BaseListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken colonToken;

	internal readonly GreenNode types;

	public SyntaxToken ColonToken => colonToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> Types => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(types));

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode types, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode types, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal BaseListSyntax(SyntaxKind kind, SyntaxToken colonToken, GreenNode types)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		if (types != null)
		{
			AdjustFlagsAndWidth(types);
			this.types = types;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => colonToken, 
			1 => types, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBaseList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBaseList(this);
	}

	public BaseListSyntax Update(SyntaxToken colonToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		if (colonToken != ColonToken || types != Types)
		{
			BaseListSyntax baseListSyntax = SyntaxFactory.BaseList(colonToken, types);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				baseListSyntax = baseListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				baseListSyntax = baseListSyntax.WithAnnotationsGreen(annotations);
			}
			return baseListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BaseListSyntax(Kind, colonToken, types, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BaseListSyntax(Kind, colonToken, types, GetDiagnostics(), annotations);
	}

	internal BaseListSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			colonToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			types = greenNode;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(colonToken);
		writer.WriteValue(types);
	}

	static BaseListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BaseListSyntax), (ObjectReader r) => new BaseListSyntax(r));
	}
}

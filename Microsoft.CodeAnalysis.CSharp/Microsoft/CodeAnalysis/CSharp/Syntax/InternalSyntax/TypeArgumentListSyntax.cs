using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeArgumentListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanToken;

	internal readonly GreenNode arguments;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanToken => lessThanToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax> Arguments => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(arguments));

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal TypeArgumentListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode arguments, SyntaxToken greaterThanToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal TypeArgumentListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode arguments, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal TypeArgumentListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode arguments, SyntaxToken greaterThanToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (arguments != null)
		{
			AdjustFlagsAndWidth(arguments);
			this.arguments = arguments;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanToken, 
			1 => arguments, 
			2 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeArgumentList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeArgumentList(this);
	}

	public TypeArgumentListSyntax Update(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeSyntax> arguments, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || arguments != Arguments || greaterThanToken != GreaterThanToken)
		{
			TypeArgumentListSyntax typeArgumentListSyntax = SyntaxFactory.TypeArgumentList(lessThanToken, arguments, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeArgumentListSyntax = typeArgumentListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeArgumentListSyntax = typeArgumentListSyntax.WithAnnotationsGreen(annotations);
			}
			return typeArgumentListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TypeArgumentListSyntax(Kind, lessThanToken, arguments, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TypeArgumentListSyntax(Kind, lessThanToken, arguments, greaterThanToken, GetDiagnostics(), annotations);
	}

	internal TypeArgumentListSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			lessThanToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			arguments = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			greaterThanToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(lessThanToken);
		writer.WriteValue(arguments);
		writer.WriteValue(greaterThanToken);
	}

	static TypeArgumentListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TypeArgumentListSyntax), (ObjectReader r) => new TypeArgumentListSyntax(r));
	}
}

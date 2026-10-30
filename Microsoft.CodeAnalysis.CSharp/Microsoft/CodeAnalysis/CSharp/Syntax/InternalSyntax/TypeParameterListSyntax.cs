using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeParameterListSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken lessThanToken;

	internal readonly GreenNode parameters;

	internal readonly SyntaxToken greaterThanToken;

	public SyntaxToken LessThanToken => lessThanToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> Parameters => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(parameters));

	public SyntaxToken GreaterThanToken => greaterThanToken;

	internal TypeParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode parameters, SyntaxToken greaterThanToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal TypeParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode parameters, SyntaxToken greaterThanToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal TypeParameterListSyntax(SyntaxKind kind, SyntaxToken lessThanToken, GreenNode parameters, SyntaxToken greaterThanToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(lessThanToken);
		this.lessThanToken = lessThanToken;
		if (parameters != null)
		{
			AdjustFlagsAndWidth(parameters);
			this.parameters = parameters;
		}
		AdjustFlagsAndWidth(greaterThanToken);
		this.greaterThanToken = greaterThanToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => lessThanToken, 
			1 => parameters, 
			2 => greaterThanToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeParameterList(this);
	}

	public TypeParameterListSyntax Update(SyntaxToken lessThanToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || parameters != Parameters || greaterThanToken != GreaterThanToken)
		{
			TypeParameterListSyntax typeParameterListSyntax = SyntaxFactory.TypeParameterList(lessThanToken, parameters, greaterThanToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeParameterListSyntax = typeParameterListSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeParameterListSyntax = typeParameterListSyntax.WithAnnotationsGreen(annotations);
			}
			return typeParameterListSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TypeParameterListSyntax(Kind, lessThanToken, parameters, greaterThanToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TypeParameterListSyntax(Kind, lessThanToken, parameters, greaterThanToken, GetDiagnostics(), annotations);
	}

	internal TypeParameterListSyntax(ObjectReader reader)
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
			parameters = greenNode;
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
		writer.WriteValue(parameters);
		writer.WriteValue(greaterThanToken);
	}

	static TypeParameterListSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TypeParameterListSyntax), (ObjectReader r) => new TypeParameterListSyntax(r));
	}
}

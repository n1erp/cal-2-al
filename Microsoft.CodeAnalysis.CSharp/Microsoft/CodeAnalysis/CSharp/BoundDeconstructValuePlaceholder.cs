using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDeconstructValuePlaceholder : BoundValuePlaceholderBase
{
	public uint ValEscape { get; }

	public BoundDeconstructValuePlaceholder(SyntaxNode syntax, uint valEscape, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DeconstructValuePlaceholder, syntax, type, hasErrors)
	{
		ValEscape = valEscape;
	}

	public BoundDeconstructValuePlaceholder(SyntaxNode syntax, uint valEscape, TypeSymbol type)
		: base(BoundKind.DeconstructValuePlaceholder, syntax, type)
	{
		ValEscape = valEscape;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeconstructValuePlaceholder(this);
	}

	public BoundDeconstructValuePlaceholder Update(uint valEscape, TypeSymbol type)
	{
		if (valEscape != ValEscape || type != Type)
		{
			return new BoundDeconstructValuePlaceholder(Syntax, valEscape, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}

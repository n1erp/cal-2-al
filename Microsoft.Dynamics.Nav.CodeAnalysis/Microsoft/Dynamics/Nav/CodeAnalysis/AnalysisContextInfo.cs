using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct AnalysisContextInfo
{
	private readonly Compilation compilation;

	private readonly IOperation operation;

	private readonly ISymbol symbol;

	private readonly SyntaxTree tree;

	private readonly SyntaxNode node;

	public AnalysisContextInfo(Compilation compilation)
		: this(compilation, null, null, null, null)
	{
	}

	public AnalysisContextInfo(SemanticModel model)
		: this(model.Compilation, model.SyntaxTree)
	{
	}

	public AnalysisContextInfo(Compilation compilation, ISymbol symbol)
		: this(compilation, null, symbol, null, null)
	{
	}

	public AnalysisContextInfo(Compilation compilation, SyntaxTree tree)
		: this(compilation, null, null, tree, null)
	{
	}

	public AnalysisContextInfo(Compilation compilation, SyntaxNode node)
		: this(compilation, null, null, node.SyntaxTree, node)
	{
	}

	public AnalysisContextInfo(Compilation compilation, IOperation operation)
		: this(compilation, operation, null, operation.Syntax.SyntaxTree, operation.Syntax)
	{
	}

	public AnalysisContextInfo(Compilation compilation, ISymbol symbol, SyntaxNode node)
		: this(compilation, null, symbol, node.SyntaxTree, node)
	{
	}

	public AnalysisContextInfo(Compilation compilation, IOperation operation, ISymbol symbol, SyntaxTree tree, SyntaxNode node)
	{
		this.compilation = compilation;
		this.operation = operation;
		this.symbol = symbol;
		this.tree = tree;
		this.node = node;
	}

	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
	public string GetContext()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (operation != null)
		{
			stringBuilder.AppendLine(string.Format("{0}: {1}", "IOperation", operation.Kind));
		}
		if (symbol?.Name != null)
		{
			stringBuilder.AppendLine(string.Format("{0}: {1} ({2})", "ISymbol", symbol.Name, symbol.Kind));
		}
		if (tree?.FilePath != null)
		{
			stringBuilder.AppendLine(string.Format("{0}: {1}", "SyntaxTree", tree.FilePath));
		}
		if (node != null)
		{
			LinePositionSpan? linePositionSpan = (tree?.GetText())?.Lines?.GetLinePositionSpan(node.Span);
			stringBuilder.AppendLine(string.Format("{0}: {1} [{2}]@{3} {4}", "SyntaxNode", GetFlattenedNodeText(node), node.GetType().Name, node.Span, linePositionSpan.HasValue ? linePositionSpan.Value.ToString() : string.Empty));
		}
		return stringBuilder.ToString();
	}

	private static string GetFlattenedNodeText(SyntaxNode node)
	{
		int num = node.Span.Start;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (SyntaxToken item in node.DescendantTokens())
		{
			if (item.Span.Start - num > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(item);
			num = item.Span.End;
			if (stringBuilder.Length > 30)
			{
				break;
			}
		}
		return string.Concat(stringBuilder, (stringBuilder.Length > 30) ? " ..." : string.Empty);
	}
}

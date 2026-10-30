using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public abstract class CSharpSyntaxNode : SyntaxNode, IFormattable
{
	internal new SyntaxTree SyntaxTree => _syntaxTree ?? ComputeSyntaxTree(this);

	internal new CSharpSyntaxNode Parent => (CSharpSyntaxNode)base.Parent;

	internal new CSharpSyntaxNode ParentOrStructuredTriviaParent => (CSharpSyntaxNode)base.ParentOrStructuredTriviaParent;

	internal Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode CsGreen => (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)Green;

	public override string Language => "C#";

	protected override SyntaxTree SyntaxTreeCore => SyntaxTree;

	internal CSharpSyntaxNode(GreenNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal CSharpSyntaxNode(GreenNode green, int position, SyntaxTree syntaxTree)
		: base(green, position, syntaxTree)
	{
	}

	private static SyntaxTree ComputeSyntaxTree(CSharpSyntaxNode node)
	{
		ArrayBuilder<CSharpSyntaxNode> arrayBuilder = null;
		SyntaxTree syntaxTree;
		while (true)
		{
			syntaxTree = node._syntaxTree;
			if (syntaxTree != null)
			{
				break;
			}
			CSharpSyntaxNode parent = node.Parent;
			if (parent == null)
			{
				Interlocked.CompareExchange(ref node._syntaxTree, CSharpSyntaxTree.CreateWithoutClone(node), null);
				syntaxTree = node._syntaxTree;
				break;
			}
			syntaxTree = parent._syntaxTree;
			if (syntaxTree != null)
			{
				node._syntaxTree = syntaxTree;
				break;
			}
			(arrayBuilder ?? (arrayBuilder = ArrayBuilder<CSharpSyntaxNode>.GetInstance())).Add(node);
			node = parent;
		}
		if (arrayBuilder != null)
		{
			foreach (CSharpSyntaxNode item in arrayBuilder)
			{
				if (item._syntaxTree != null)
				{
					break;
				}
				item._syntaxTree = syntaxTree;
			}
			arrayBuilder.Free();
		}
		return syntaxTree;
	}

	public abstract TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor);

	public abstract void Accept(CSharpSyntaxVisitor visitor);

	public SyntaxKind Kind()
	{
		return (SyntaxKind)Green.RawKind;
	}

	public new SyntaxTriviaList GetLeadingTrivia()
	{
		return GetFirstToken(includeZeroWidth: true).LeadingTrivia;
	}

	public new SyntaxTriviaList GetTrailingTrivia()
	{
		return GetLastToken(includeZeroWidth: true).TrailingTrivia;
	}

	public static SyntaxNode DeserializeFrom(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead)
		{
			throw new InvalidOperationException(CodeAnalysisResources.TheStreamCannotBeReadFrom);
		}
		using ObjectReader objectReader = ObjectReader.TryGetReader(stream, cancellationToken);
		if (objectReader == null)
		{
			throw new ArgumentException(CodeAnalysisResources.Stream_contains_invalid_data, "stream");
		}
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)objectReader.ReadValue()).CreateRed();
	}

	public new Location GetLocation()
	{
		return new SourceLocation(this);
	}

	internal new SyntaxReference GetReference()
	{
		return SyntaxTree.GetReference(this);
	}

	public new IEnumerable<Diagnostic> GetDiagnostics()
	{
		return SyntaxTree.GetDiagnostics(this);
	}

	internal IList<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax> GetDirectives(Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> filter = null)
	{
		return ((SyntaxNodeOrToken)this).GetDirectives(filter);
	}

	public Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax GetFirstDirective(Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> predicate = null)
	{
		foreach (SyntaxNodeOrToken item in ChildNodesAndTokens())
		{
			if (!item.ContainsDirectives)
			{
				continue;
			}
			if (item.IsNode)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax firstDirective = item.AsNode().GetFirstDirective(predicate);
				if (firstDirective != null)
				{
					return firstDirective;
				}
				continue;
			}
			foreach (SyntaxTrivia leadingTrivium in item.AsToken().LeadingTrivia)
			{
				if (leadingTrivium.IsDirective)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax directiveTriviaSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax)leadingTrivium.GetStructure();
					if (predicate == null || predicate(directiveTriviaSyntax))
					{
						return directiveTriviaSyntax;
					}
				}
			}
		}
		return null;
	}

	public Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax GetLastDirective(Func<Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax, bool> predicate = null)
	{
		foreach (SyntaxNodeOrToken item in ChildNodesAndTokens().Reverse())
		{
			if (!item.ContainsDirectives)
			{
				continue;
			}
			if (item.IsNode)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax lastDirective = item.AsNode().GetLastDirective(predicate);
				if (lastDirective != null)
				{
					return lastDirective;
				}
				continue;
			}
			foreach (SyntaxTrivia item2 in item.AsToken().LeadingTrivia.Reverse())
			{
				if (item2.IsDirective)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax directiveTriviaSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax)item2.GetStructure();
					if (predicate == null || predicate(directiveTriviaSyntax))
					{
						return directiveTriviaSyntax;
					}
				}
			}
		}
		return null;
	}

	public new SyntaxToken GetFirstToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return base.GetFirstToken(includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);
	}

	internal SyntaxToken GetFirstToken(Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto = null)
	{
		return SyntaxNavigator.Instance.GetFirstToken(this, predicate, stepInto);
	}

	public new SyntaxToken GetLastToken(bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		return base.GetLastToken(includeZeroWidth, includeSkipped, includeDirectives, includeDocumentationComments);
	}

	public new SyntaxToken FindToken(int position, bool findInsideTrivia = false)
	{
		return base.FindToken(position, findInsideTrivia);
	}

	internal SyntaxToken FindTokenIncludingCrefAndNameAttributes(int position)
	{
		SyntaxToken token = FindToken(position);
		SyntaxTrivia triviaFromSyntaxToken = SyntaxNode.GetTriviaFromSyntaxToken(position, in token);
		if (!SyntaxFacts.IsDocumentationCommentTrivia(triviaFromSyntaxToken.Kind()))
		{
			return token;
		}
		SyntaxToken result = ((CSharpSyntaxNode)triviaFromSyntaxToken.GetStructure()).FindTokenInternal(position);
		for (CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)result.Parent; cSharpSyntaxNode != null; cSharpSyntaxNode = cSharpSyntaxNode.Parent)
		{
			if (cSharpSyntaxNode.Kind() == SyntaxKind.XmlCrefAttribute || cSharpSyntaxNode.Kind() == SyntaxKind.XmlNameAttribute)
			{
				if (!LookupPosition.IsInXmlAttributeValue(position, (Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax)cSharpSyntaxNode))
				{
					return token;
				}
				return result;
			}
		}
		return token;
	}

	public new SyntaxTrivia FindTrivia(int position, Func<SyntaxTrivia, bool> stepInto)
	{
		return base.FindTrivia(position, stepInto);
	}

	public new SyntaxTrivia FindTrivia(int position, bool findInsideTrivia = false)
	{
		return base.FindTrivia(position, findInsideTrivia);
	}

	protected override bool EquivalentToCore(SyntaxNode other)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected internal override SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode> nodes = null, Func<TNode, TNode, SyntaxNode> computeReplacementNode = null, IEnumerable<SyntaxToken> tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken> computeReplacementToken = null, IEnumerable<SyntaxTrivia> trivia = null, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia = null)
	{
		return SyntaxReplacer.Replace(this, nodes, computeReplacementNode, tokens, computeReplacementToken, trivia, computeReplacementTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes)
	{
		return SyntaxReplacer.ReplaceNodeInList(this, originalNode, replacementNodes).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
	{
		return SyntaxReplacer.InsertNodeInList(this, nodeInList, nodesToInsert, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens)
	{
		return SyntaxReplacer.ReplaceTokenInList(this, originalToken, newTokens).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
	{
		return SyntaxReplacer.InsertTokenInList(this, originalToken, newTokens, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia)
	{
		return SyntaxReplacer.ReplaceTriviaInList(this, originalTrivia, newTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
	{
		return SyntaxReplacer.InsertTriviaInList(this, originalTrivia, newTrivia, insertBefore).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options)
	{
		return SyntaxNodeRemover.RemoveNodes(this, nodes.Cast<CSharpSyntaxNode>(), options).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected internal override SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia)
	{
		return SyntaxNormalizer.Normalize(this, indentation, eol, elasticTrivia).AsRootOfNewTreeWithOptionsFrom(SyntaxTree);
	}

	protected override bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false)
	{
		return SyntaxFactory.AreEquivalent(this, (CSharpSyntaxNode)node, topLevel);
	}

	internal override bool ShouldCreateWeakList()
	{
		if (Kind() == SyntaxKind.Block)
		{
			CSharpSyntaxNode parent = Parent;
			if (parent is Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax || parent is Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax)
			{
				return true;
			}
		}
		return false;
	}

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}
}

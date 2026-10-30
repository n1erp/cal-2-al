using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SpeculativeSyntaxTreeSemanticModel : SyntaxTreeSemanticModel
{
	private readonly SyntaxTreeSemanticModel _parentSemanticModel;

	private readonly CSharpSyntaxNode _root;

	private readonly Binder _rootBinder;

	private readonly int _position;

	private readonly SpeculativeBindingOption _bindingOption;

	public override bool IsSpeculativeSemanticModel => true;

	public override int OriginalPositionForSpeculation => _position;

	public override CSharpSemanticModel ParentModel => _parentSemanticModel;

	internal override CSharpSyntaxNode Root => _root;

	public static SpeculativeSyntaxTreeSemanticModel Create(SyntaxTreeSemanticModel parentSemanticModel, TypeSyntax root, Binder rootBinder, int position, SpeculativeBindingOption bindingOption)
	{
		return CreateCore(parentSemanticModel, root, rootBinder, position, bindingOption);
	}

	public static SpeculativeSyntaxTreeSemanticModel Create(SyntaxTreeSemanticModel parentSemanticModel, CrefSyntax root, Binder rootBinder, int position)
	{
		return CreateCore(parentSemanticModel, root, rootBinder, position, SpeculativeBindingOption.BindAsTypeOrNamespace);
	}

	private static SpeculativeSyntaxTreeSemanticModel CreateCore(SyntaxTreeSemanticModel parentSemanticModel, CSharpSyntaxNode root, Binder rootBinder, int position, SpeculativeBindingOption bindingOption)
	{
		return new SpeculativeSyntaxTreeSemanticModel(parentSemanticModel, root, rootBinder, position, bindingOption);
	}

	private SpeculativeSyntaxTreeSemanticModel(SyntaxTreeSemanticModel parentSemanticModel, CSharpSyntaxNode root, Binder rootBinder, int position, SpeculativeBindingOption bindingOption)
		: base(parentSemanticModel.Compilation, parentSemanticModel.SyntaxTree, root.SyntaxTree)
	{
		_parentSemanticModel = parentSemanticModel;
		_root = root;
		_rootBinder = rootBinder;
		_position = position;
		_bindingOption = bindingOption;
	}

	internal override BoundNode Bind(Binder binder, CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		return _parentSemanticModel.Bind(binder, node, diagnostics);
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		return _rootBinder;
	}

	private SpeculativeBindingOption GetSpeculativeBindingOption(ExpressionSyntax node)
	{
		if (SyntaxFacts.IsInNamespaceOrTypeContext(node))
		{
			return SpeculativeBindingOption.BindAsTypeOrNamespace;
		}
		return _bindingOption;
	}

	internal override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node is CrefSyntax cref)
		{
			return _parentSemanticModel.GetSpeculativeSymbolInfo(_position, cref, options);
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)node;
		if ((options & SymbolInfoOptions.PreserveAliases) != 0)
		{
			return new SymbolInfo((AliasSymbol)_parentSemanticModel.GetSpeculativeAliasInfo(_position, expressionSyntax, GetSpeculativeBindingOption(expressionSyntax)), ImmutableArray<ISymbol>.Empty, CandidateReason.None);
		}
		return _parentSemanticModel.GetSpeculativeSymbolInfo(_position, expressionSyntax, GetSpeculativeBindingOption(expressionSyntax));
	}

	internal override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)node;
		return _parentSemanticModel.GetSpeculativeTypeInfoWorker(_position, expressionSyntax, GetSpeculativeBindingOption(expressionSyntax));
	}
}

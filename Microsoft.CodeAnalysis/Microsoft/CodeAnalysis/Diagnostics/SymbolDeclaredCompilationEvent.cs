using System;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SymbolDeclaredCompilationEvent : CompilationEvent
{
	private readonly Lazy<ImmutableArray<SyntaxReference>> _lazyCachedDeclaringReferences;

	private Lazy<SemanticModel> _lazySemanticModel;

	private SemanticModel _semanticModel;

	private WeakReference<SemanticModel> _weakModel;

	private readonly object _gate = new object();

	public ISymbol Symbol { get; }

	public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _lazyCachedDeclaringReferences.Value;

	public SymbolDeclaredCompilationEvent(Compilation compilation, ISymbol symbol)
		: base(compilation)
	{
		Symbol = symbol;
		_lazyCachedDeclaringReferences = new Lazy<ImmutableArray<SyntaxReference>>(() => symbol.DeclaringSyntaxReferences);
	}

	public SymbolDeclaredCompilationEvent(Compilation compilation, ISymbol symbol, Lazy<SemanticModel> lazySemanticModel)
		: this(compilation, symbol)
	{
		_lazySemanticModel = lazySemanticModel;
	}

	private SymbolDeclaredCompilationEvent(SymbolDeclaredCompilationEvent original, SemanticModel newSemanticModel)
		: this(original.Compilation, original.Symbol)
	{
		_semanticModel = newSemanticModel;
	}

	public SemanticModel SemanticModel(SyntaxReference reference)
	{
		lock (_gate)
		{
			SemanticModel target = _semanticModel;
			if (target == null && _lazySemanticModel != null)
			{
				target = (_semanticModel = _lazySemanticModel.Value);
				_lazySemanticModel = null;
			}
			if (target == null)
			{
				_weakModel?.TryGetTarget(out target);
			}
			if (target == null || target.SyntaxTree != reference.SyntaxTree)
			{
				target = Compilation.GetSemanticModel(reference.SyntaxTree);
				_weakModel = new WeakReference<SemanticModel>(target);
			}
			return target;
		}
	}

	public override void FlushCache()
	{
		lock (_gate)
		{
			SemanticModel semanticModel = _semanticModel;
			_lazySemanticModel = null;
			if (semanticModel != null)
			{
				_weakModel = new WeakReference<SemanticModel>(semanticModel);
				_semanticModel = null;
			}
		}
	}

	public SymbolDeclaredCompilationEvent WithSemanticModel(SemanticModel model)
	{
		return new SymbolDeclaredCompilationEvent(this, model);
	}

	public override string ToString()
	{
		string text = Symbol.Name;
		if (text == "")
		{
			text = "<empty>";
		}
		string text2 = ((DeclaringSyntaxReferences.Length != 0) ? (" @ " + string.Join(", ", Enumerable.Select(DeclaringSyntaxReferences, (SyntaxReference r) => r.GetLocation().GetLineSpan()))) : null);
		return "SymbolDeclaredCompilationEvent(" + text + " " + Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) + text2 + ")";
	}
}

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class SymbolDeclaredCompilationEvent : CompilationEvent
{
	private Lazy<SemanticModel> lazySemanticModel;

	private SemanticModel semanticModel;

	private WeakReference<SemanticModel> weakModel;

	private readonly object gate = new object();

	public ISymbol Symbol { get; }

	public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public SymbolDeclaredCompilationEvent(Compilation compilation, ISymbol symbol)
		: base(compilation)
	{
		Symbol = symbol;
		DeclaringSyntaxReferences = symbol.DeclaringSyntaxReferences;
	}

	public SymbolDeclaredCompilationEvent(Compilation compilation, ISymbol symbol, Lazy<SemanticModel> lazySemanticModel)
		: this(compilation, symbol)
	{
		this.lazySemanticModel = lazySemanticModel;
	}

	private SymbolDeclaredCompilationEvent(SymbolDeclaredCompilationEvent original, SemanticModel newSemanticModel)
		: this(original.Compilation, original.Symbol)
	{
		semanticModel = newSemanticModel;
	}

	public SemanticModel SemanticModel(SyntaxReference reference)
	{
		lock (gate)
		{
			SemanticModel target = semanticModel;
			if (target == null && lazySemanticModel != null)
			{
				target = (semanticModel = lazySemanticModel.Value);
				lazySemanticModel = null;
			}
			if (target == null && weakModel != null)
			{
				weakModel.TryGetTarget(out target);
			}
			if (target == null || target.SyntaxTree != reference.SyntaxTree)
			{
				target = Compilation.GetSemanticModel(reference.SyntaxTree);
				weakModel = new WeakReference<SemanticModel>(target);
			}
			return target;
		}
	}

	public override void FlushCache()
	{
		lock (gate)
		{
			SemanticModel semanticModel = this.semanticModel;
			lazySemanticModel = null;
			if (semanticModel != null)
			{
				weakModel = new WeakReference<SemanticModel>(semanticModel);
				this.semanticModel = null;
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
		if (string.IsNullOrEmpty(text))
		{
			text = "<empty>";
		}
		string text2 = ((DeclaringSyntaxReferences.Length != 0) ? (" @ " + string.Join(", ", Enumerable.Select(DeclaringSyntaxReferences, (SyntaxReference r) => r.GetLocation().GetLineSpan()))) : null);
		return "SymbolDeclaredCompilationEvent(" + text + " " + Symbol.ToDisplayString(SymbolDisplayFormat.ShortFormat) + text2 + ")";
	}
}

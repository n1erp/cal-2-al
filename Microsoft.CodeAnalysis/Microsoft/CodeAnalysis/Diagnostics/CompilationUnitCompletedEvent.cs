using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CompilationUnitCompletedEvent : CompilationEvent
{
	private WeakReference<SemanticModel> _weakModel;

	public SemanticModel SemanticModel
	{
		get
		{
			WeakReference<SemanticModel> weakModel = _weakModel;
			if (weakModel == null || !weakModel.TryGetTarget(out var target))
			{
				target = Compilation.GetSemanticModel(CompilationUnit);
				_weakModel = new WeakReference<SemanticModel>(target);
			}
			return target;
		}
		private set
		{
			_weakModel = new WeakReference<SemanticModel>(value);
		}
	}

	public SyntaxTree CompilationUnit { get; }

	public CompilationUnitCompletedEvent(Compilation compilation, SyntaxTree compilationUnit)
		: base(compilation)
	{
		CompilationUnit = compilationUnit;
	}

	public CompilationUnitCompletedEvent(CompilationUnitCompletedEvent original, SemanticModel newSemanticModel)
		: this(original.Compilation, original.CompilationUnit)
	{
		SemanticModel = newSemanticModel;
	}

	public override void FlushCache()
	{
	}

	public CompilationUnitCompletedEvent WithSemanticModel(SemanticModel model)
	{
		return new CompilationUnitCompletedEvent(this, model);
	}

	public override string ToString()
	{
		return "CompilationUnitCompletedEvent(" + CompilationUnit.FilePath + ")";
	}
}

using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class CompilationUnitCompletedEvent : CompilationEvent
{
	private WeakReference<SemanticModel> weakModel;

	public SemanticModel SemanticModel
	{
		get
		{
			WeakReference<SemanticModel> weakReference = weakModel;
			if (weakReference == null || !weakReference.TryGetTarget(out var target))
			{
				target = Compilation.GetSemanticModel(CompilationUnit);
				weakModel = new WeakReference<SemanticModel>(target);
			}
			return target;
		}
		private set
		{
			weakModel = new WeakReference<SemanticModel>(value);
		}
	}

	public SyntaxTree CompilationUnit { get; private set; }

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

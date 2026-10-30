using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class OperationExtensions
{
	private sealed class OperationCollector : OperationWalker
	{
		private readonly List<IOperation> list;

		public OperationCollector(List<IOperation> list)
		{
			this.list = list;
		}

		public override void Visit(IOperation operation)
		{
			if (operation != null)
			{
				list.Add(operation);
			}
			base.Visit(operation);
		}
	}

	public static IEnumerable<IOperation> DescendantsAndSelf(this IOperation operation)
	{
		if (operation == null)
		{
			return SpecializedCollections.EmptyEnumerable<IOperation>();
		}
		List<IOperation> list = new List<IOperation>();
		new OperationCollector(list).Visit(operation);
		return list;
	}

	public static ISymbol GetSymbol(this IOperation operation)
	{
		return operation.Kind switch
		{
			OperationKind.LocalReferenceExpression => (ISymbol)((ILocalReferenceExpression)operation).LocalVariable, 
			OperationKind.GlobalReferenceExpression => ((IGlobalReferenceExpression)operation).GlobalVariable, 
			OperationKind.ParameterReferenceExpression => ((IParameterReferenceExpression)operation).Parameter, 
			OperationKind.FieldAccess => ((IFieldAccess)operation).FieldSymbol, 
			_ => null, 
		};
	}
}

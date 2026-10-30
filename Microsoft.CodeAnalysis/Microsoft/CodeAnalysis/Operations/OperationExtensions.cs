using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Operations;

public static class OperationExtensions
{
	internal static bool HasErrors(this IOperation operation, Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		if (compilation == null)
		{
			throw new ArgumentNullException("compilation");
		}
		if (operation.Syntax == null)
		{
			return true;
		}
		SemanticModel semanticModel = operation.SemanticModel ?? compilation.GetSemanticModel(operation.Syntax.SyntaxTree);
		if (semanticModel.IsSpeculativeSemanticModel)
		{
			return false;
		}
		return semanticModel.GetDiagnostics(operation.Syntax.Span, cancellationToken).Any((Diagnostic d) => d.DefaultSeverity == DiagnosticSeverity.Error);
	}

	public static IEnumerable<IOperation> Descendants(this IOperation operation)
	{
		return Descendants(operation, includeSelf: false);
	}

	public static IEnumerable<IOperation> DescendantsAndSelf(this IOperation operation)
	{
		return Descendants(operation, includeSelf: true);
	}

	private static IEnumerable<IOperation> Descendants(IOperation operation, bool includeSelf)
	{
		if (operation == null)
		{
			yield break;
		}
		if (includeSelf)
		{
			yield return operation;
		}
		ArrayBuilder<IEnumerator<IOperation>> stack = ArrayBuilder<IEnumerator<IOperation>>.GetInstance();
		stack.Push(operation.Children.GetEnumerator());
		while (stack.Any())
		{
			IEnumerator<IOperation> enumerator = stack.Pop();
			if (enumerator.MoveNext())
			{
				IOperation current = enumerator.Current;
				stack.Push(enumerator);
				if (current != null)
				{
					yield return current;
					stack.Push(current.Children.GetEnumerator());
				}
			}
		}
		stack.Free();
	}

	public static ImmutableArray<ILocalSymbol> GetDeclaredVariables(this IVariableDeclarationGroupOperation declarationGroup)
	{
		if (declarationGroup == null)
		{
			throw new ArgumentNullException("declarationGroup");
		}
		ArrayBuilder<ILocalSymbol> instance = ArrayBuilder<ILocalSymbol>.GetInstance();
		foreach (IVariableDeclarationOperation declaration in declarationGroup.Declarations)
		{
			declaration.GetDeclaredVariables(instance);
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<ILocalSymbol> GetDeclaredVariables(this IVariableDeclarationOperation declaration)
	{
		if (declaration == null)
		{
			throw new ArgumentNullException("declaration");
		}
		ArrayBuilder<ILocalSymbol> instance = ArrayBuilder<ILocalSymbol>.GetInstance();
		declaration.GetDeclaredVariables(instance);
		return instance.ToImmutableAndFree();
	}

	private static void GetDeclaredVariables(this IVariableDeclarationOperation declaration, ArrayBuilder<ILocalSymbol> arrayBuilder)
	{
		foreach (IVariableDeclaratorOperation declarator in declaration.Declarators)
		{
			arrayBuilder.Add(declarator.Symbol);
		}
	}

	public static IVariableInitializerOperation GetVariableInitializer(this IVariableDeclaratorOperation declarationOperation)
	{
		if (declarationOperation == null)
		{
			throw new ArgumentNullException("declarationOperation");
		}
		IVariableInitializerOperation initializer = declarationOperation.Initializer;
		if (initializer == null)
		{
			IVariableDeclarationOperation obj = declarationOperation.Parent as IVariableDeclarationOperation;
			if (obj == null)
			{
				return null;
			}
			initializer = obj.Initializer;
		}
		return initializer;
	}

	public static string GetArgumentName(this IDynamicInvocationOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentName(index);
	}

	public static string GetArgumentName(this IDynamicIndexerAccessOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentName(index);
	}

	public static string GetArgumentName(this IDynamicObjectCreationOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentName(index);
	}

	internal static string GetArgumentName(this HasDynamicArgumentsExpression dynamicOperation, int index)
	{
		if (dynamicOperation.Arguments.IsDefaultOrEmpty)
		{
			throw new InvalidOperationException();
		}
		if (index < 0 || index >= dynamicOperation.Arguments.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ImmutableArray<string> argumentNames = dynamicOperation.ArgumentNames;
		if (!argumentNames.IsDefaultOrEmpty)
		{
			return argumentNames[index];
		}
		return null;
	}

	public static RefKind? GetArgumentRefKind(this IDynamicInvocationOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentRefKind(index);
	}

	public static RefKind? GetArgumentRefKind(this IDynamicIndexerAccessOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentRefKind(index);
	}

	public static RefKind? GetArgumentRefKind(this IDynamicObjectCreationOperation dynamicOperation, int index)
	{
		if (dynamicOperation == null)
		{
			throw new ArgumentNullException("dynamicOperation");
		}
		return ((HasDynamicArgumentsExpression)dynamicOperation).GetArgumentRefKind(index);
	}

	internal static RefKind? GetArgumentRefKind(this HasDynamicArgumentsExpression dynamicOperation, int index)
	{
		if (dynamicOperation.Arguments.IsDefaultOrEmpty)
		{
			throw new InvalidOperationException();
		}
		if (index < 0 || index >= dynamicOperation.Arguments.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ImmutableArray<RefKind> argumentRefKinds = dynamicOperation.ArgumentRefKinds;
		if (argumentRefKinds.IsDefault)
		{
			return null;
		}
		if (argumentRefKinds.IsEmpty)
		{
			return RefKind.None;
		}
		return argumentRefKinds[index];
	}

	internal static IOperation GetRootOperation(this IOperation operation)
	{
		while (operation.Parent != null)
		{
			operation = operation.Parent;
		}
		return operation;
	}
}

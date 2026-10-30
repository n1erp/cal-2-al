using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class MethodCall : Expression
{
	public Expression Method { get; set; }

	public List<Expression> Arguments { get; set; }

	public MethodInfo MethodInfo { get; private set; }

	public bool PropertyCall { get; set; }

	public MethodCall(Expression method, List<Expression> arguments, MethodInfo methodInfo, TextPosition position = null)
		: base(AstNodeType.MethodCallNode, position)
	{
		if (methodInfo == null)
		{
			methodInfo = new MethodInfo(MethodInfo.MethodKind.External, null, null, null, -1, 0, isStatic: false, 1, callToSelf: false);
		}
		Method = method;
		Arguments = arguments;
		MethodInfo = methodInfo;
		TypeIdentifier = methodInfo.ReturnType;
		HasSideEffects = true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.OnAccepting(this);
		AcceptAstNode(Method, visitor);
		AcceptExpressions(Arguments, visitor);
		visitor.VisitMethodCallNode(this);
	}
}

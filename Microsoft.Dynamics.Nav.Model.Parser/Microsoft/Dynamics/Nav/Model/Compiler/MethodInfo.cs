using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class MethodInfo
{
	public enum MethodKind
	{
		External,
		BuiltIn,
		UserFunction,
		RecordField,
		NCL,
		CLR,
		Automation,
		OCX,
		EventHandler,
		SubForm,
		Coercion
	}

	public MethodKind MethodType { get; private set; }

	public int InvokeKind { get; private set; }

	public TypeIdentifier ParentType { get; private set; }

	public int MemberId { get; private set; }

	public int SubTypeId { get; private set; }

	public TypeIdentifier ReturnType { get; private set; }

	public List<Parameter> Parameters { get; private set; }

	public bool IsStatic { get; private set; }

	public bool CallToSelf { get; private set; }

	public MethodInfo(MethodKind methodType, TypeIdentifier parentType, TypeIdentifier returnType, List<Parameter> parameters, int memberId, int subTypeId, bool isStatic, int invokeKind, bool callToSelf)
	{
		MethodType = methodType;
		ParentType = parentType;
		InvokeKind = invokeKind;
		MemberId = memberId;
		SubTypeId = subTypeId;
		ReturnType = returnType;
		Parameters = parameters;
		IsStatic = isStatic;
		CallToSelf = callToSelf;
	}
}

using System.Diagnostics;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

[DebuggerDisplay("{closedMethod.FullName}")]
internal class CecilMethodInfo
{
	private readonly MethodDefinition definition;

	private readonly MethodReference closedMethod;

	public bool IsStatic => definition.IsStatic;

	public string Name => definition.Name;

	public bool IsReturnValueOptional => definition.IsReturnValueOptional();

	public bool IsPublic => definition.IsPublic;

	public TypeReference ReturnType => closedMethod.ReturnType;

	public Collection<ParameterDefinition> Parameters => closedMethod.Parameters;

	public Collection<CustomAttribute> CustomAttributes { get; }

	public string Signature { get; }

	public bool IsValid { get; }

	public CecilMethodInfo(MethodDefinition definition, MethodReference closedMethod)
	{
		this.definition = definition;
		this.closedMethod = closedMethod;
		CustomAttributes = definition.CustomAttributes;
		IsValid = closedMethod != null;
		Signature = GetSignatureWithoutClassName(closedMethod ?? definition);
	}

	private static string GetSignatureWithoutClassName(MethodReference x)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			builder.Append(x.Name);
			builder.Append("(");
			if (x.HasParameters)
			{
				Collection<ParameterDefinition> parameters = x.Parameters;
				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterDefinition parameterDefinition = parameters[i];
					if (i > 0)
					{
						builder.Append(",");
					}
					if (parameterDefinition.ParameterType.IsSentinel)
					{
						builder.Append("...,");
					}
					builder.Append(parameterDefinition.ParameterType.FullName);
				}
			}
			builder.Append(")");
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}
}

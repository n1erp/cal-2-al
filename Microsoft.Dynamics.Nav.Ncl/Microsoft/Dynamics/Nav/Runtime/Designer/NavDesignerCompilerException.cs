using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

[Serializable]
internal class NavDesignerCompilerException : NavAppException
{
	public EmitResult EmitResult { get; }

	public IEnumerable<string> CompileMessages
	{
		get
		{
			if (EmitResult == null)
			{
				return Enumerable.Empty<string>();
			}
			return EmitResult.Diagnostics.Select((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic m) => m.GetMessage());
		}
	}

	public NavDesignerCompilerException()
	{
	}

	public NavDesignerCompilerException(string message)
		: base(message)
	{
	}

	public NavDesignerCompilerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavDesignerCompilerException(string message, EmitResult emitResult)
		: base(message)
	{
		EmitResult = emitResult;
	}

	protected NavDesignerCompilerException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
		EmitResult = (EmitResult)serializationInfo.GetValue("EmitResult", typeof(EmitResult));
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("EmitResult", EmitResult, typeof(EmitResult));
		base.GetObjectData(info, context);
	}
}

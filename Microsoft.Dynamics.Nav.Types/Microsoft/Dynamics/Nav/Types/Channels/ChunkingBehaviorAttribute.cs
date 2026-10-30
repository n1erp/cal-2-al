using System;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Dynamics.Nav.Types.Channels;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ChunkingBehaviorAttribute : Attribute, IOperationBehavior
{
	public bool ChunkInput { get; set; }

	public bool ChunkOutput { get; set; }

	void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
	{
		ChunkingBindingParameter chunkingBindingParameter = bindingParameters.Find<ChunkingBindingParameter>();
		if (chunkingBindingParameter == null)
		{
			chunkingBindingParameter = new ChunkingBindingParameter();
			bindingParameters.Add(chunkingBindingParameter);
		}
		if (ChunkInput)
		{
			MessageDescription messageDescription = operationDescription.Messages.FirstOrDefault((MessageDescription m) => m.Direction == MessageDirection.Input);
			if (messageDescription != null)
			{
				chunkingBindingParameter.ChunkedActions.Add(messageDescription.Action);
			}
		}
		if (ChunkOutput)
		{
			MessageDescription messageDescription2 = operationDescription.Messages.FirstOrDefault((MessageDescription m) => m.Direction == MessageDirection.Output);
			if (messageDescription2 != null)
			{
				chunkingBindingParameter.ChunkedActions.Add(messageDescription2.Action);
			}
		}
	}

	void IOperationBehavior.Validate(OperationDescription operationDescription)
	{
	}

	void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
	{
	}

	void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
	{
	}
}

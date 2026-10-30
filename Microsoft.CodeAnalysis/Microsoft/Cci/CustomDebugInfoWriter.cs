using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal sealed class CustomDebugInfoWriter
{
	private MethodDefinitionHandle _methodWithModuleInfo;

	private IMethodBody _methodBodyWithModuleInfo;

	private MethodDefinitionHandle _previousMethodWithUsingInfo;

	private IMethodBody _previousMethodBodyWithUsingInfo;

	private readonly PdbWriter _pdbWriter;

	public CustomDebugInfoWriter(PdbWriter pdbWriter)
	{
		_pdbWriter = pdbWriter;
	}

	public bool ShouldForwardNamespaceScopes(EmitContext context, IMethodBody methodBody, MethodDefinitionHandle methodHandle, out IMethodDefinition forwardToMethod)
	{
		if (ShouldForwardToPreviousMethodWithUsingInfo(context, methodBody))
		{
			if (context.Module.GenerateVisualBasicStylePdb)
			{
				forwardToMethod = _previousMethodBodyWithUsingInfo.MethodDefinition;
			}
			else
			{
				forwardToMethod = null;
			}
			return true;
		}
		_previousMethodBodyWithUsingInfo = methodBody;
		_previousMethodWithUsingInfo = methodHandle;
		forwardToMethod = null;
		return false;
	}

	public byte[] SerializeMethodDebugInfo(EmitContext context, IMethodBody methodBody, MethodDefinitionHandle methodHandle, bool emitEncInfo, bool suppressNewCustomDebugInfo, out bool emitExternNamespaces)
	{
		emitExternNamespaces = false;
		if (_methodBodyWithModuleInfo == null && context.Module.GetAssemblyReferenceAliases(context).Any())
		{
			_methodWithModuleInfo = methodHandle;
			_methodBodyWithModuleInfo = methodBody;
			emitExternNamespaces = true;
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		CustomDebugInfoEncoder encoder = new CustomDebugInfoEncoder(instance);
		if (methodBody.StateMachineTypeName != null)
		{
			encoder.AddStateMachineTypeName(methodBody.StateMachineTypeName);
		}
		else
		{
			SerializeNamespaceScopeMetadata(ref encoder, context, methodBody);
			encoder.AddStateMachineHoistedLocalScopes(methodBody.StateMachineHoistedLocalScopes);
		}
		if (!suppressNewCustomDebugInfo)
		{
			SerializeDynamicLocalInfo(ref encoder, methodBody);
			SerializeTupleElementNames(ref encoder, methodBody);
			if (emitEncInfo)
			{
				EditAndContinueMethodDebugInformation encMethodDebugInfo = MetadataWriter.GetEncMethodDebugInfo(methodBody);
				SerializeCustomDebugInformation(ref encoder, encMethodDebugInfo);
			}
		}
		byte[] result = encoder.ToArray();
		instance.Free();
		return result;
	}

	internal static void SerializeCustomDebugInformation(ref CustomDebugInfoEncoder encoder, EditAndContinueMethodDebugInformation debugInfo)
	{
		if (!debugInfo.LocalSlots.IsDefaultOrEmpty)
		{
			encoder.AddRecord(CustomDebugInfoKind.EditAndContinueLocalSlotMap, debugInfo, (EditAndContinueMethodDebugInformation info, BlobBuilder builder) =>
			{
				info.SerializeLocalSlots(builder);
			});
		}
		if (!debugInfo.Lambdas.IsDefaultOrEmpty)
		{
			encoder.AddRecord(CustomDebugInfoKind.EditAndContinueLambdaMap, debugInfo, (EditAndContinueMethodDebugInformation info, BlobBuilder builder) =>
			{
				info.SerializeLambdaMap(builder);
			});
		}
	}

	private static ArrayBuilder<T> GetLocalInfoToSerialize<T>(IMethodBody methodBody, Func<ILocalDefinition, bool> filter, Func<LocalScope, ILocalDefinition, T> getInfo)
	{
		ArrayBuilder<T> arrayBuilder = null;
		foreach (LocalScope localScope in methodBody.LocalScopes)
		{
			foreach (ILocalDefinition variable in localScope.Variables)
			{
				if (filter(variable))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<T>.GetInstance();
					}
					arrayBuilder.Add(getInfo(default, variable));
				}
			}
			foreach (ILocalDefinition constant in localScope.Constants)
			{
				if (filter(constant))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<T>.GetInstance();
					}
					arrayBuilder.Add(getInfo(localScope, constant));
				}
			}
		}
		return arrayBuilder;
	}

	private static void SerializeDynamicLocalInfo(ref CustomDebugInfoEncoder encoder, IMethodBody methodBody)
	{
		if (methodBody.HasDynamicLocalVariables)
		{
			ArrayBuilder<(string, byte[], int, int)> localInfoToSerialize = GetLocalInfoToSerialize(methodBody, (ILocalDefinition local) =>
			{
				ImmutableArray<bool> dynamicTransformFlags = local.DynamicTransformFlags;
				return !dynamicTransformFlags.IsEmpty && dynamicTransformFlags.Length <= 64 && local.Name.Length < 64;
			}, (LocalScope scope, ILocalDefinition local) => (Name: local.Name, GetDynamicFlags(local), Length: local.DynamicTransformFlags.Length, (local.SlotIndex >= 0) ? local.SlotIndex : 0));
			if (localInfoToSerialize != null)
			{
				encoder.AddDynamicLocals(localInfoToSerialize);
				localInfoToSerialize.Free();
			}
		}
		static byte[] GetDynamicFlags(ILocalDefinition local)
		{
			ImmutableArray<bool> dynamicTransformFlags = local.DynamicTransformFlags;
			byte[] array = new byte[64];
			for (int i = 0; i < dynamicTransformFlags.Length; i++)
			{
				if (dynamicTransformFlags[i])
				{
					array[i] = 1;
				}
			}
			return array;
		}
	}

	private static void SerializeTupleElementNames(ref CustomDebugInfoEncoder encoder, IMethodBody methodBody)
	{
		ArrayBuilder<(string, int, int, int, ImmutableArray<string>)> localInfoToSerialize = GetLocalInfoToSerialize(methodBody, (ILocalDefinition local) => !local.TupleElementNames.IsEmpty, (LocalScope scope, ILocalDefinition local) => (Name: local.Name, SlotIndex: local.SlotIndex, StartOffset: scope.StartOffset, EndOffset: scope.EndOffset, TupleElementNames: local.TupleElementNames));
		if (localInfoToSerialize != null)
		{
			encoder.AddTupleElementNames(localInfoToSerialize);
			localInfoToSerialize.Free();
		}
	}

	private void SerializeNamespaceScopeMetadata(ref CustomDebugInfoEncoder encoder, EmitContext context, IMethodBody methodBody)
	{
		if (context.Module.GenerateVisualBasicStylePdb)
		{
			return;
		}
		if (ShouldForwardToPreviousMethodWithUsingInfo(context, methodBody))
		{
			encoder.AddForwardMethodInfo(_previousMethodWithUsingInfo);
			return;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		for (IImportScope importScope = methodBody.ImportScope; importScope != null; importScope = importScope.Parent)
		{
			instance.Add(importScope.GetUsedNamespaces().Length);
		}
		encoder.AddUsingGroups(instance);
		instance.Free();
		if (_methodBodyWithModuleInfo != null && _methodBodyWithModuleInfo != methodBody)
		{
			encoder.AddForwardModuleInfo(_methodWithModuleInfo);
		}
	}

	private bool ShouldForwardToPreviousMethodWithUsingInfo(EmitContext context, IMethodBody methodBody)
	{
		if (_previousMethodBodyWithUsingInfo == null || _previousMethodBodyWithUsingInfo == methodBody)
		{
			return false;
		}
		if (context.Module.GenerateVisualBasicStylePdb && _pdbWriter.GetOrCreateSerializedNamespaceName(_previousMethodBodyWithUsingInfo.MethodDefinition.ContainingNamespace) != _pdbWriter.GetOrCreateSerializedNamespaceName(methodBody.MethodDefinition.ContainingNamespace))
		{
			return false;
		}
		IImportScope importScope = _previousMethodBodyWithUsingInfo.ImportScope;
		if (methodBody.ImportScope == importScope)
		{
			return true;
		}
		IImportScope importScope2 = methodBody.ImportScope;
		IImportScope importScope3 = importScope;
		while (importScope2 != null && importScope3 != null)
		{
			if (!importScope2.GetUsedNamespaces().SequenceEqual(importScope3.GetUsedNamespaces()))
			{
				return false;
			}
			importScope2 = importScope2.Parent;
			importScope3 = importScope3.Parent;
		}
		return importScope2 == importScope3;
	}
}

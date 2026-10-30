using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Channels;

public class ErrorLoggingDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
{
	private class ErrorLoggingSerializer : XmlObjectSerializer
	{
		private readonly XmlObjectSerializer innerSerializer;

		public ErrorLoggingSerializer(XmlObjectSerializer innerSerializer)
		{
			this.innerSerializer = innerSerializer;
		}

		public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
		{
			try
			{
				innerSerializer.WriteStartObject(writer, graph);
			}
			catch (SerializationException exception)
			{
				EventLogWriter.Write(exception);
				throw;
			}
		}

		public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
		{
			try
			{
				innerSerializer.WriteObjectContent(writer, graph);
			}
			catch (SerializationException e) when (WriteEventLogExceptionFilter(e))
			{
				throw;
			}
		}

		public override void WriteEndObject(XmlDictionaryWriter writer)
		{
			try
			{
				innerSerializer.WriteEndObject(writer);
			}
			catch (SerializationException e) when (WriteEventLogExceptionFilter(e))
			{
				throw;
			}
		}

		public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
		{
			try
			{
				return innerSerializer.ReadObject(reader, verifyObjectName);
			}
			catch (SerializationException e) when (WriteEventLogExceptionFilter(e))
			{
				throw;
			}
		}

		public override bool IsStartObject(XmlDictionaryReader reader)
		{
			try
			{
				return innerSerializer.IsStartObject(reader);
			}
			catch (SerializationException e) when (WriteEventLogExceptionFilter(e))
			{
				throw;
			}
		}

		private static bool WriteEventLogExceptionFilter(SerializationException e)
		{
			EventLogWriter.Write(e);
			return false;
		}
	}

	public ErrorLoggingDataContractSerializerOperationBehavior(OperationDescription operation, DataContractSerializerOperationBehavior originalBehavior)
		: base(operation, originalBehavior.DataContractFormatAttribute)
	{
		MaxItemsInObjectGraph = originalBehavior.MaxItemsInObjectGraph;
		IgnoreExtensionDataObject = originalBehavior.IgnoreExtensionDataObject;
		DataContractSurrogate = originalBehavior.DataContractSurrogate;
	}

	public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
	{
		return new ErrorLoggingSerializer(base.CreateSerializer(type, name, ns, knownTypes));
	}

	public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
	{
		return new ErrorLoggingSerializer(base.CreateSerializer(type, name, ns, knownTypes));
	}
}

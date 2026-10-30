using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal sealed class ObjectReader : IDisposable
{
	private struct ReaderReferenceMap<T> where T : class
	{
		private readonly List<T> _values;

		internal static readonly ObjectPool<List<T>> s_objectListPool = new ObjectPool<List<T>>(() => new List<T>(20));

		private ReaderReferenceMap(List<T> values)
		{
			_values = values;
		}

		public static ReaderReferenceMap<T> Create()
		{
			return new ReaderReferenceMap<T>(s_objectListPool.Allocate());
		}

		public void Dispose()
		{
			_values.Clear();
			s_objectListPool.Free(_values);
		}

		public int GetNextObjectId()
		{
			int count = _values.Count;
			_values.Add(null);
			return count;
		}

		public void AddValue(T value)
		{
			_values.Add(value);
		}

		public void AddValue(int index, T value)
		{
			_values[index] = value;
		}

		public T GetValue(int referenceId)
		{
			return _values[referenceId];
		}
	}

	internal const byte VersionByte1 = 170;

	internal const byte VersionByte2 = 9;

	private readonly BinaryReader _reader;

	private readonly CancellationToken _cancellationToken;

	private ReaderReferenceMap<object> _objectReferenceMap;

	private ReaderReferenceMap<string> _stringReferenceMap;

	private readonly ObjectBinderSnapshot _binderSnapshot;

	private int _recursionDepth;

	private ObjectReader(Stream stream, CancellationToken cancellationToken)
	{
		_reader = new BinaryReader(stream, Encoding.UTF8);
		_objectReferenceMap = ReaderReferenceMap<object>.Create();
		_stringReferenceMap = ReaderReferenceMap<string>.Create();
		_binderSnapshot = ObjectBinder.GetSnapshot();
		_cancellationToken = cancellationToken;
	}

	public static ObjectReader TryGetReader(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (stream == null)
		{
			return null;
		}
		if (stream.ReadByte() != 170 || stream.ReadByte() != 9)
		{
			return null;
		}
		return new ObjectReader(stream, cancellationToken);
	}

	public void Dispose()
	{
		_objectReferenceMap.Dispose();
		_stringReferenceMap.Dispose();
		_recursionDepth = 0;
	}

	public bool ReadBoolean()
	{
		return _reader.ReadBoolean();
	}

	public byte ReadByte()
	{
		return _reader.ReadByte();
	}

	public char ReadChar()
	{
		return (char)_reader.ReadUInt16();
	}

	public decimal ReadDecimal()
	{
		return _reader.ReadDecimal();
	}

	public double ReadDouble()
	{
		return _reader.ReadDouble();
	}

	public float ReadSingle()
	{
		return _reader.ReadSingle();
	}

	public int ReadInt32()
	{
		return _reader.ReadInt32();
	}

	public long ReadInt64()
	{
		return _reader.ReadInt64();
	}

	public sbyte ReadSByte()
	{
		return _reader.ReadSByte();
	}

	public short ReadInt16()
	{
		return _reader.ReadInt16();
	}

	public uint ReadUInt32()
	{
		return _reader.ReadUInt32();
	}

	public ulong ReadUInt64()
	{
		return _reader.ReadUInt64();
	}

	public ushort ReadUInt16()
	{
		return _reader.ReadUInt16();
	}

	public string ReadString()
	{
		return ReadStringValue();
	}

	public Guid ReadGuid()
	{
		ObjectWriter.GuidAccessor guidAccessor = new ObjectWriter.GuidAccessor
		{
			Low64 = ReadInt64(),
			High64 = ReadInt64()
		};
		return guidAccessor.Guid;
	}

	public object ReadValue()
	{
		_ = _recursionDepth;
		_recursionDepth++;
		object result = ((_recursionDepth % 50 != 0) ? ReadValueWorker() : Task.Factory.StartNew(() => ReadValueWorker(), _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult());
		_recursionDepth--;
		return result;
	}

	private object ReadValueWorker()
	{
		ObjectWriter.EncodingKind encodingKind = (ObjectWriter.EncodingKind)_reader.ReadByte();
		switch (encodingKind)
		{
		case ObjectWriter.EncodingKind.Null:
			return null;
		case ObjectWriter.EncodingKind.Boolean_True:
			return true;
		case ObjectWriter.EncodingKind.Boolean_False:
			return false;
		case ObjectWriter.EncodingKind.Int8:
			return _reader.ReadSByte();
		case ObjectWriter.EncodingKind.UInt8:
			return _reader.ReadByte();
		case ObjectWriter.EncodingKind.Int16:
			return _reader.ReadInt16();
		case ObjectWriter.EncodingKind.UInt16:
			return _reader.ReadUInt16();
		case ObjectWriter.EncodingKind.Int32:
			return _reader.ReadInt32();
		case ObjectWriter.EncodingKind.Int32_1Byte:
			return (int)_reader.ReadByte();
		case ObjectWriter.EncodingKind.Int32_2Bytes:
			return (int)_reader.ReadUInt16();
		case ObjectWriter.EncodingKind.Int32_0:
		case ObjectWriter.EncodingKind.Int32_1:
		case ObjectWriter.EncodingKind.Int32_2:
		case ObjectWriter.EncodingKind.Int32_3:
		case ObjectWriter.EncodingKind.Int32_4:
		case ObjectWriter.EncodingKind.Int32_5:
		case ObjectWriter.EncodingKind.Int32_6:
		case ObjectWriter.EncodingKind.Int32_7:
		case ObjectWriter.EncodingKind.Int32_8:
		case ObjectWriter.EncodingKind.Int32_9:
		case ObjectWriter.EncodingKind.Int32_10:
			return (int)(encodingKind - 19);
		case ObjectWriter.EncodingKind.UInt32:
			return _reader.ReadUInt32();
		case ObjectWriter.EncodingKind.UInt32_1Byte:
			return (uint)_reader.ReadByte();
		case ObjectWriter.EncodingKind.UInt32_2Bytes:
			return (uint)_reader.ReadUInt16();
		case ObjectWriter.EncodingKind.UInt32_0:
		case ObjectWriter.EncodingKind.UInt32_1:
		case ObjectWriter.EncodingKind.UInt32_2:
		case ObjectWriter.EncodingKind.UInt32_3:
		case ObjectWriter.EncodingKind.UInt32_4:
		case ObjectWriter.EncodingKind.UInt32_5:
		case ObjectWriter.EncodingKind.UInt32_6:
		case ObjectWriter.EncodingKind.UInt32_7:
		case ObjectWriter.EncodingKind.UInt32_8:
		case ObjectWriter.EncodingKind.UInt32_9:
		case ObjectWriter.EncodingKind.UInt32_10:
			return (uint)(encodingKind - 36);
		case ObjectWriter.EncodingKind.Int64:
			return _reader.ReadInt64();
		case ObjectWriter.EncodingKind.UInt64:
			return _reader.ReadUInt64();
		case ObjectWriter.EncodingKind.Float4:
			return _reader.ReadSingle();
		case ObjectWriter.EncodingKind.Float8:
			return _reader.ReadDouble();
		case ObjectWriter.EncodingKind.Decimal:
			return _reader.ReadDecimal();
		case ObjectWriter.EncodingKind.Char:
			return (char)_reader.ReadUInt16();
		case ObjectWriter.EncodingKind.StringUtf8:
		case ObjectWriter.EncodingKind.StringUtf16:
		case ObjectWriter.EncodingKind.StringRef_1Byte:
		case ObjectWriter.EncodingKind.StringRef_2Bytes:
		case ObjectWriter.EncodingKind.StringRef_4Bytes:
			return ReadStringValue(encodingKind);
		case ObjectWriter.EncodingKind.ObjectRef_4Bytes:
			return _objectReferenceMap.GetValue(_reader.ReadInt32());
		case ObjectWriter.EncodingKind.ObjectRef_1Byte:
			return _objectReferenceMap.GetValue(_reader.ReadByte());
		case ObjectWriter.EncodingKind.ObjectRef_2Bytes:
			return _objectReferenceMap.GetValue(_reader.ReadUInt16());
		case ObjectWriter.EncodingKind.Object:
			return ReadObject();
		case ObjectWriter.EncodingKind.DateTime:
			return DateTime.FromBinary(_reader.ReadInt64());
		case ObjectWriter.EncodingKind.Array:
		case ObjectWriter.EncodingKind.Array_0:
		case ObjectWriter.EncodingKind.Array_1:
		case ObjectWriter.EncodingKind.Array_2:
		case ObjectWriter.EncodingKind.Array_3:
			return ReadArray(encodingKind);
		default:
			throw ExceptionUtilities.UnexpectedValue(encodingKind);
		}
	}

	internal uint ReadCompressedUInt()
	{
		byte b = _reader.ReadByte();
		byte b2 = (byte)(b & 0xC0);
		byte b3 = (byte)(b & -193);
		switch (b2)
		{
		case 0:
			return b3;
		case 64:
		{
			byte b7 = _reader.ReadByte();
			return (uint)((b3 << 8) | b7);
		}
		case 128:
		{
			byte b4 = _reader.ReadByte();
			byte b5 = _reader.ReadByte();
			byte b6 = _reader.ReadByte();
			return (uint)((b3 << 24) | (b4 << 16) | (b5 << 8) | b6);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(b2);
		}
	}

	private string ReadStringValue()
	{
		ObjectWriter.EncodingKind encodingKind = (ObjectWriter.EncodingKind)_reader.ReadByte();
		if (encodingKind != ObjectWriter.EncodingKind.Null)
		{
			return ReadStringValue(encodingKind);
		}
		return null;
	}

	private string ReadStringValue(ObjectWriter.EncodingKind kind)
	{
		switch (kind)
		{
		case ObjectWriter.EncodingKind.StringRef_1Byte:
			return _stringReferenceMap.GetValue(_reader.ReadByte());
		case ObjectWriter.EncodingKind.StringRef_2Bytes:
			return _stringReferenceMap.GetValue(_reader.ReadUInt16());
		case ObjectWriter.EncodingKind.StringRef_4Bytes:
			return _stringReferenceMap.GetValue(_reader.ReadInt32());
		case ObjectWriter.EncodingKind.StringUtf8:
		case ObjectWriter.EncodingKind.StringUtf16:
			return ReadStringLiteral(kind);
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private unsafe string ReadStringLiteral(ObjectWriter.EncodingKind kind)
	{
		string text;
		if (kind == ObjectWriter.EncodingKind.StringUtf8)
		{
			text = _reader.ReadString();
		}
		else
		{
			int num = (int)ReadCompressedUInt();
			fixed (byte* value = _reader.ReadBytes(num * 2))
			{
				text = new string((char*)value, 0, num);
			}
		}
		_stringReferenceMap.AddValue(text);
		return text;
	}

	private Array ReadArray(ObjectWriter.EncodingKind kind)
	{
		int num = kind switch
		{
			ObjectWriter.EncodingKind.Array_0 => 0, 
			ObjectWriter.EncodingKind.Array_1 => 1, 
			ObjectWriter.EncodingKind.Array_2 => 2, 
			ObjectWriter.EncodingKind.Array_3 => 3, 
			_ => (int)ReadCompressedUInt(), 
		};
		ObjectWriter.EncodingKind encodingKind = (ObjectWriter.EncodingKind)_reader.ReadByte();
		Type type = ObjectWriter.s_reverseTypeMap[(int)encodingKind];
		if ((object)type != null)
		{
			return ReadPrimitiveTypeArrayElements(type, encodingKind, num);
		}
		type = ReadTypeAfterTag();
		Array array = Array.CreateInstance(type, num);
		for (int i = 0; i < num; i++)
		{
			object value = ReadValue();
			array.SetValue(value, i);
		}
		return array;
	}

	private Array ReadPrimitiveTypeArrayElements(Type type, ObjectWriter.EncodingKind kind, int length)
	{
		if ((object)type == typeof(byte))
		{
			return _reader.ReadBytes(length);
		}
		if ((object)type == typeof(char))
		{
			return _reader.ReadChars(length);
		}
		if ((object)type == typeof(string))
		{
			return ReadStringArrayElements(CreateArray<string>(length));
		}
		if ((object)type == typeof(bool))
		{
			return ReadBooleanArrayElements(CreateArray<bool>(length));
		}
		return kind switch
		{
			ObjectWriter.EncodingKind.Int8 => (Array)ReadInt8ArrayElements(CreateArray<sbyte>(length)), 
			ObjectWriter.EncodingKind.Int16 => ReadInt16ArrayElements(CreateArray<short>(length)), 
			ObjectWriter.EncodingKind.Int32 => ReadInt32ArrayElements(CreateArray<int>(length)), 
			ObjectWriter.EncodingKind.Int64 => ReadInt64ArrayElements(CreateArray<long>(length)), 
			ObjectWriter.EncodingKind.UInt16 => ReadUInt16ArrayElements(CreateArray<ushort>(length)), 
			ObjectWriter.EncodingKind.UInt32 => ReadUInt32ArrayElements(CreateArray<uint>(length)), 
			ObjectWriter.EncodingKind.UInt64 => ReadUInt64ArrayElements(CreateArray<ulong>(length)), 
			ObjectWriter.EncodingKind.Float4 => ReadFloat4ArrayElements(CreateArray<float>(length)), 
			ObjectWriter.EncodingKind.Float8 => ReadFloat8ArrayElements(CreateArray<double>(length)), 
			ObjectWriter.EncodingKind.Decimal => ReadDecimalArrayElements(CreateArray<decimal>(length)), 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private bool[] ReadBooleanArrayElements(bool[] array)
	{
		int num = BitVector.WordsRequired(array.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			uint word = _reader.ReadUInt32();
			for (int j = 0; j < 32; j++)
			{
				if (num2 >= array.Length)
				{
					return array;
				}
				array[num2++] = BitVector.IsTrue(word, j);
			}
		}
		return array;
	}

	private static T[] CreateArray<T>(int length)
	{
		if (length == 0)
		{
			return Array.Empty<T>();
		}
		return new T[length];
	}

	private string[] ReadStringArrayElements(string[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadStringValue();
		}
		return array;
	}

	private sbyte[] ReadInt8ArrayElements(sbyte[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadSByte();
		}
		return array;
	}

	private short[] ReadInt16ArrayElements(short[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadInt16();
		}
		return array;
	}

	private int[] ReadInt32ArrayElements(int[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadInt32();
		}
		return array;
	}

	private long[] ReadInt64ArrayElements(long[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadInt64();
		}
		return array;
	}

	private ushort[] ReadUInt16ArrayElements(ushort[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadUInt16();
		}
		return array;
	}

	private uint[] ReadUInt32ArrayElements(uint[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadUInt32();
		}
		return array;
	}

	private ulong[] ReadUInt64ArrayElements(ulong[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadUInt64();
		}
		return array;
	}

	private decimal[] ReadDecimalArrayElements(decimal[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadDecimal();
		}
		return array;
	}

	private float[] ReadFloat4ArrayElements(float[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadSingle();
		}
		return array;
	}

	private double[] ReadFloat8ArrayElements(double[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _reader.ReadDouble();
		}
		return array;
	}

	public Type ReadType()
	{
		_reader.ReadByte();
		return Type.GetType(ReadString());
	}

	private Type ReadTypeAfterTag()
	{
		return _binderSnapshot.GetTypeFromId(ReadInt32());
	}

	private object ReadObject()
	{
		int nextObjectId = _objectReferenceMap.GetNextObjectId();
		IObjectWritable objectWritable = _binderSnapshot.GetTypeReaderFromId(ReadInt32())(this);
		if (objectWritable.ShouldReuseInSerialization)
		{
			_objectReferenceMap.AddValue(nextObjectId, objectWritable);
		}
		return objectWritable;
	}

	private static Exception DeserializationReadIncorrectNumberOfValuesException(string typeName)
	{
		throw new InvalidOperationException(string.Format(CodeAnalysisResources.Deserialization_reader_for_0_read_incorrect_number_of_values, typeName));
	}

	private static Exception NoSerializationTypeException(string typeName)
	{
		return new InvalidOperationException(string.Format(CodeAnalysisResources.The_type_0_is_not_understood_by_the_serialization_binder, typeName));
	}

	private static Exception NoSerializationReaderException(string typeName)
	{
		return new InvalidOperationException(string.Format(CodeAnalysisResources.Cannot_serialize_type_0, typeName));
	}
}

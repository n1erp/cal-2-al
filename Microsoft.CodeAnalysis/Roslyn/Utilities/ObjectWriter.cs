using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal sealed class ObjectWriter : IDisposable
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct GuidAccessor
	{
		[FieldOffset(0)]
		public Guid Guid;

		[FieldOffset(0)]
		public long Low64;

		[FieldOffset(8)]
		public long High64;
	}

	private struct WriterReferenceMap(bool valueEquality)
	{
		private readonly Dictionary<object, int> _valueToIdMap = GetDictionaryPool(valueEquality).Allocate();

		private readonly bool _valueEquality = valueEquality;

		private int _nextId = 0;

		private static readonly ObjectPool<Dictionary<object, int>> s_referenceDictionaryPool = new ObjectPool<Dictionary<object, int>>(() => new Dictionary<object, int>(128, ReferenceEqualityComparer.Instance));

		private static readonly ObjectPool<Dictionary<object, int>> s_valueDictionaryPool = new ObjectPool<Dictionary<object, int>>(() => new Dictionary<object, int>(128));

		private static ObjectPool<Dictionary<object, int>> GetDictionaryPool(bool valueEquality)
		{
			if (!valueEquality)
			{
				return s_referenceDictionaryPool;
			}
			return s_valueDictionaryPool;
		}

		public void Dispose()
		{
			ObjectPool<Dictionary<object, int>> dictionaryPool = GetDictionaryPool(_valueEquality);
			if (_valueToIdMap.Count <= 1024)
			{
				_valueToIdMap.Clear();
				dictionaryPool.Free(_valueToIdMap);
			}
		}

		public bool TryGetReferenceId(object value, out int referenceId)
		{
			return _valueToIdMap.TryGetValue(value, out referenceId);
		}

		public void Add(object value, bool isReusable)
		{
			int value2 = _nextId++;
			if (isReusable)
			{
				_valueToIdMap.Add(value, value2);
			}
		}
	}

	internal enum EncodingKind : byte
	{
		Null,
		Type,
		Object,
		ObjectRef_1Byte,
		ObjectRef_2Bytes,
		ObjectRef_4Bytes,
		StringUtf8,
		StringUtf16,
		StringRef_1Byte,
		StringRef_2Bytes,
		StringRef_4Bytes,
		Boolean_True,
		Boolean_False,
		Char,
		Int8,
		Int16,
		Int32,
		Int32_1Byte,
		Int32_2Bytes,
		Int32_0,
		Int32_1,
		Int32_2,
		Int32_3,
		Int32_4,
		Int32_5,
		Int32_6,
		Int32_7,
		Int32_8,
		Int32_9,
		Int32_10,
		Int64,
		UInt8,
		UInt16,
		UInt32,
		UInt32_1Byte,
		UInt32_2Bytes,
		UInt32_0,
		UInt32_1,
		UInt32_2,
		UInt32_3,
		UInt32_4,
		UInt32_5,
		UInt32_6,
		UInt32_7,
		UInt32_8,
		UInt32_9,
		UInt32_10,
		UInt64,
		Float4,
		Float8,
		Decimal,
		DateTime,
		Array,
		Array_0,
		Array_1,
		Array_2,
		Array_3,
		BooleanType,
		StringType,
		Last
	}

	private readonly BinaryWriter _writer;

	private readonly CancellationToken _cancellationToken;

	private WriterReferenceMap _objectReferenceMap;

	private WriterReferenceMap _stringReferenceMap;

	private readonly ObjectBinderSnapshot _binderSnapshot;

	private int _recursionDepth;

	internal const int MaxRecursionDepth = 50;

	internal static readonly Dictionary<Type, EncodingKind> s_typeMap;

	internal static readonly ImmutableArray<Type> s_reverseTypeMap;

	internal const byte ByteMarkerMask = 192;

	internal const byte Byte1Marker = 0;

	internal const byte Byte2Marker = 64;

	internal const byte Byte4Marker = 128;

	public ObjectWriter(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		_writer = new BinaryWriter(stream, Encoding.UTF8);
		_objectReferenceMap = new WriterReferenceMap(valueEquality: false);
		_stringReferenceMap = new WriterReferenceMap(valueEquality: true);
		_cancellationToken = cancellationToken;
		_binderSnapshot = ObjectBinder.GetSnapshot();
		WriteVersion();
	}

	private void WriteVersion()
	{
		_writer.Write((byte)170);
		_writer.Write((byte)9);
	}

	public void Dispose()
	{
		_objectReferenceMap.Dispose();
		_stringReferenceMap.Dispose();
		_recursionDepth = 0;
	}

	public void WriteBoolean(bool value)
	{
		_writer.Write(value);
	}

	public void WriteByte(byte value)
	{
		_writer.Write(value);
	}

	public void WriteChar(char ch)
	{
		_writer.Write((ushort)ch);
	}

	public void WriteDecimal(decimal value)
	{
		_writer.Write(value);
	}

	public void WriteDouble(double value)
	{
		_writer.Write(value);
	}

	public void WriteSingle(float value)
	{
		_writer.Write(value);
	}

	public void WriteInt32(int value)
	{
		_writer.Write(value);
	}

	public void WriteInt64(long value)
	{
		_writer.Write(value);
	}

	public void WriteSByte(sbyte value)
	{
		_writer.Write(value);
	}

	public void WriteInt16(short value)
	{
		_writer.Write(value);
	}

	public void WriteUInt32(uint value)
	{
		_writer.Write(value);
	}

	public void WriteUInt64(ulong value)
	{
		_writer.Write(value);
	}

	public void WriteUInt16(ushort value)
	{
		_writer.Write(value);
	}

	public void WriteString(string value)
	{
		WriteStringValue(value);
	}

	public void WriteGuid(Guid guid)
	{
		GuidAccessor guidAccessor = new GuidAccessor
		{
			Guid = guid
		};
		WriteInt64(guidAccessor.Low64);
		WriteInt64(guidAccessor.High64);
	}

	public void WriteValue(object value)
	{
		if (value == null)
		{
			_writer.Write((byte)0);
			return;
		}
		Type type = value.GetType();
		if (type.GetTypeInfo().IsPrimitive)
		{
			if ((object)value.GetType() == typeof(int))
			{
				WriteEncodedInt32((int)value);
				return;
			}
			if ((object)value.GetType() == typeof(double))
			{
				_writer.Write((byte)49);
				_writer.Write((double)value);
				return;
			}
			if ((object)value.GetType() == typeof(bool))
			{
				_writer.Write((byte)(((bool)value) ? 11 : 12));
				return;
			}
			if ((object)value.GetType() == typeof(char))
			{
				_writer.Write((byte)13);
				_writer.Write((ushort)(char)value);
				return;
			}
			if ((object)value.GetType() == typeof(byte))
			{
				_writer.Write((byte)31);
				_writer.Write((byte)value);
				return;
			}
			if ((object)value.GetType() == typeof(short))
			{
				_writer.Write((byte)15);
				_writer.Write((short)value);
				return;
			}
			if ((object)value.GetType() == typeof(long))
			{
				_writer.Write((byte)30);
				_writer.Write((long)value);
				return;
			}
			if ((object)value.GetType() == typeof(sbyte))
			{
				_writer.Write((byte)14);
				_writer.Write((sbyte)value);
				return;
			}
			if ((object)value.GetType() == typeof(float))
			{
				_writer.Write((byte)48);
				_writer.Write((float)value);
				return;
			}
			if ((object)value.GetType() == typeof(ushort))
			{
				_writer.Write((byte)32);
				_writer.Write((ushort)value);
				return;
			}
			if ((object)value.GetType() == typeof(uint))
			{
				WriteEncodedUInt32((uint)value);
				return;
			}
			if ((object)value.GetType() != typeof(ulong))
			{
				throw ExceptionUtilities.UnexpectedValue(value.GetType());
			}
			_writer.Write((byte)47);
			_writer.Write((ulong)value);
		}
		else if ((object)value.GetType() == typeof(decimal))
		{
			_writer.Write((byte)50);
			_writer.Write((decimal)value);
		}
		else if ((object)value.GetType() == typeof(DateTime))
		{
			_writer.Write((byte)51);
			_writer.Write(((DateTime)value).ToBinary());
		}
		else if ((object)value.GetType() == typeof(string))
		{
			WriteStringValue((string)value);
		}
		else if (type.IsArray)
		{
			Array array = (Array)value;
			if (array.Rank > 1)
			{
				throw new InvalidOperationException(CodeAnalysisResources.Arrays_with_more_than_one_dimension_cannot_be_serialized);
			}
			WriteArray(array);
		}
		else
		{
			WriteObject(value, null);
		}
	}

	public void WriteValue(IObjectWritable value)
	{
		if (value == null)
		{
			_writer.Write((byte)0);
		}
		else
		{
			WriteObject(value, value);
		}
	}

	private void WriteEncodedInt32(int v)
	{
		if (v >= 0 && v <= 10)
		{
			_writer.Write((byte)(19 + v));
		}
		else if (v >= 0 && v < 255)
		{
			_writer.Write((byte)17);
			_writer.Write((byte)v);
		}
		else if (v >= 0 && v < 65535)
		{
			_writer.Write((byte)18);
			_writer.Write((ushort)v);
		}
		else
		{
			_writer.Write((byte)16);
			_writer.Write(v);
		}
	}

	private void WriteEncodedUInt32(uint v)
	{
		if (v >= 0 && v <= 10)
		{
			_writer.Write((byte)(36 + v));
		}
		else if (v >= 0 && v < 255)
		{
			_writer.Write((byte)34);
			_writer.Write((byte)v);
		}
		else if (v >= 0 && v < 65535)
		{
			_writer.Write((byte)35);
			_writer.Write((ushort)v);
		}
		else
		{
			_writer.Write((byte)33);
			_writer.Write(v);
		}
	}

	internal void WriteCompressedUInt(uint value)
	{
		if (value <= 63)
		{
			_writer.Write((byte)value);
			return;
		}
		if (value <= 16383)
		{
			byte value2 = (byte)(((value >> 8) & 0xFF) | 0x40);
			byte value3 = (byte)(value & 0xFF);
			_writer.Write(value2);
			_writer.Write(value3);
			return;
		}
		if (value <= 1073741823)
		{
			byte value4 = (byte)(((value >> 24) & 0xFF) | 0x80);
			byte value5 = (byte)((value >> 16) & 0xFF);
			byte value6 = (byte)((value >> 8) & 0xFF);
			byte value7 = (byte)(value & 0xFF);
			_writer.Write(value4);
			_writer.Write(value5);
			_writer.Write(value6);
			_writer.Write(value7);
			return;
		}
		throw new ArgumentException(CodeAnalysisResources.Value_too_large_to_be_represented_as_a_30_bit_unsigned_integer);
	}

	private unsafe void WriteStringValue(string value)
	{
		if (value == null)
		{
			_writer.Write((byte)0);
			return;
		}
		if (_stringReferenceMap.TryGetReferenceId(value, out var referenceId))
		{
			if (referenceId <= 255)
			{
				_writer.Write((byte)8);
				_writer.Write((byte)referenceId);
			}
			else if (referenceId <= 65535)
			{
				_writer.Write((byte)9);
				_writer.Write((ushort)referenceId);
			}
			else
			{
				_writer.Write((byte)10);
				_writer.Write(referenceId);
			}
			return;
		}
		_stringReferenceMap.Add(value, isReusable: true);
		if (value.IsValidUnicodeString())
		{
			_writer.Write((byte)6);
			_writer.Write(value);
			return;
		}
		_writer.Write((byte)7);
		byte[] array = new byte[value.Length * 2];
		fixed (char* ptr = value)
		{
			Marshal.Copy((IntPtr)ptr, array, 0, array.Length);
		}
		WriteCompressedUInt((uint)value.Length);
		_writer.Write(array);
	}

	private void WriteArray(Array array)
	{
		int length = array.GetLength(0);
		switch (length)
		{
		case 0:
			_writer.Write((byte)53);
			break;
		case 1:
			_writer.Write((byte)54);
			break;
		case 2:
			_writer.Write((byte)55);
			break;
		case 3:
			_writer.Write((byte)56);
			break;
		default:
			_writer.Write((byte)52);
			WriteCompressedUInt((uint)length);
			break;
		}
		Type elementType = array.GetType().GetElementType();
		if (s_typeMap.TryGetValue(elementType, out var value))
		{
			WritePrimitiveType(elementType, value);
			WritePrimitiveTypeArrayElements(elementType, value, array);
			return;
		}
		WriteKnownType(elementType);
		_ = _recursionDepth;
		_recursionDepth++;
		if (_recursionDepth % 50 == 0)
		{
			Task.Factory.StartNew((object a) =>
			{
				WriteArrayValues((Array)a);
			}, array, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}
		else
		{
			WriteArrayValues(array);
		}
		_recursionDepth--;
	}

	private void WriteArrayValues(Array array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			WriteValue(array.GetValue(i));
		}
	}

	private void WritePrimitiveTypeArrayElements(Type type, EncodingKind kind, Array instance)
	{
		if ((object)type == typeof(byte))
		{
			_writer.Write((byte[])instance);
			return;
		}
		if ((object)type == typeof(char))
		{
			_writer.Write((char[])instance);
			return;
		}
		if ((object)type == typeof(string))
		{
			WriteStringArrayElements((string[])instance);
			return;
		}
		if ((object)type == typeof(bool))
		{
			WriteBooleanArrayElements((bool[])instance);
			return;
		}
		switch (kind)
		{
		case EncodingKind.Int8:
			WriteInt8ArrayElements((sbyte[])instance);
			break;
		case EncodingKind.Int16:
			WriteInt16ArrayElements((short[])instance);
			break;
		case EncodingKind.Int32:
			WriteInt32ArrayElements((int[])instance);
			break;
		case EncodingKind.Int64:
			WriteInt64ArrayElements((long[])instance);
			break;
		case EncodingKind.UInt16:
			WriteUInt16ArrayElements((ushort[])instance);
			break;
		case EncodingKind.UInt32:
			WriteUInt32ArrayElements((uint[])instance);
			break;
		case EncodingKind.UInt64:
			WriteUInt64ArrayElements((ulong[])instance);
			break;
		case EncodingKind.Float4:
			WriteFloat4ArrayElements((float[])instance);
			break;
		case EncodingKind.Float8:
			WriteFloat8ArrayElements((double[])instance);
			break;
		case EncodingKind.Decimal:
			WriteDecimalArrayElements((decimal[])instance);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private void WriteBooleanArrayElements(bool[] array)
	{
		BitVector bitVector = BitVector.Create(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			bitVector[i] = array[i];
		}
		foreach (uint item in bitVector.Words())
		{
			_writer.Write(item);
		}
	}

	private void WriteStringArrayElements(string[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			WriteStringValue(array[i]);
		}
	}

	private void WriteInt8ArrayElements(sbyte[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteInt16ArrayElements(short[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteInt32ArrayElements(int[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteInt64ArrayElements(long[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteUInt16ArrayElements(ushort[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteUInt32ArrayElements(uint[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteUInt64ArrayElements(ulong[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteDecimalArrayElements(decimal[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteFloat4ArrayElements(float[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WriteFloat8ArrayElements(double[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			_writer.Write(array[i]);
		}
	}

	private void WritePrimitiveType(Type type, EncodingKind kind)
	{
		_writer.Write((byte)kind);
	}

	public void WriteType(Type type)
	{
		_writer.Write((byte)1);
		WriteString(type.AssemblyQualifiedName);
	}

	private void WriteKnownType(Type type)
	{
		_writer.Write((byte)1);
		WriteInt32(_binderSnapshot.GetTypeId(type));
	}

	private void WriteObject(object instance, IObjectWritable instanceAsWritableOpt)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (_objectReferenceMap.TryGetReferenceId(instance, out var referenceId))
		{
			if (referenceId <= 255)
			{
				_writer.Write((byte)3);
				_writer.Write((byte)referenceId);
			}
			else if (referenceId <= 65535)
			{
				_writer.Write((byte)4);
				_writer.Write((ushort)referenceId);
			}
			else
			{
				_writer.Write((byte)5);
				_writer.Write(referenceId);
			}
			return;
		}
		IObjectWritable objectWritable = instanceAsWritableOpt;
		if (objectWritable == null)
		{
			objectWritable = instance as IObjectWritable;
			if (objectWritable == null)
			{
				throw NoSerializationWriterException(string.Format("{0} must implement {1}", instance.GetType(), "IObjectWritable"));
			}
		}
		_ = _recursionDepth;
		_recursionDepth++;
		if (_recursionDepth % 50 == 0)
		{
			Task.Factory.StartNew((object obj) =>
			{
				WriteObjectWorker((IObjectWritable)obj);
			}, objectWritable, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}
		else
		{
			WriteObjectWorker(objectWritable);
		}
		_recursionDepth--;
	}

	private void WriteObjectWorker(IObjectWritable writable)
	{
		_objectReferenceMap.Add(writable, writable.ShouldReuseInSerialization);
		_writer.Write((byte)2);
		WriteInt32(_binderSnapshot.GetTypeId(writable.GetType()));
		writable.WriteTo(this);
	}

	private static Exception NoSerializationTypeException(string typeName)
	{
		return new InvalidOperationException(string.Format(CodeAnalysisResources.The_type_0_is_not_understood_by_the_serialization_binder, typeName));
	}

	private static Exception NoSerializationWriterException(string typeName)
	{
		return new InvalidOperationException(string.Format(CodeAnalysisResources.Cannot_serialize_type_0, typeName));
	}

	static ObjectWriter()
	{
		s_typeMap = new Dictionary<Type, EncodingKind>
		{
			{
				typeof(bool),
				EncodingKind.BooleanType
			},
			{
				typeof(char),
				EncodingKind.Char
			},
			{
				typeof(string),
				EncodingKind.StringType
			},
			{
				typeof(sbyte),
				EncodingKind.Int8
			},
			{
				typeof(short),
				EncodingKind.Int16
			},
			{
				typeof(int),
				EncodingKind.Int32
			},
			{
				typeof(long),
				EncodingKind.Int64
			},
			{
				typeof(byte),
				EncodingKind.UInt8
			},
			{
				typeof(ushort),
				EncodingKind.UInt16
			},
			{
				typeof(uint),
				EncodingKind.UInt32
			},
			{
				typeof(ulong),
				EncodingKind.UInt64
			},
			{
				typeof(float),
				EncodingKind.Float4
			},
			{
				typeof(double),
				EncodingKind.Float8
			},
			{
				typeof(decimal),
				EncodingKind.Decimal
			}
		};
		Type[] array = new Type[59];
		foreach (KeyValuePair<Type, EncodingKind> item in s_typeMap)
		{
			array[(uint)item.Value] = item.Key;
		}
		s_reverseTypeMap = ImmutableArray.Create(array);
	}
}

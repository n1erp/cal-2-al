using System;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlDecimalHelper
{
	[StructLayout(LayoutKind.Explicit)]
	private struct SqlDecimalUnion
	{
		private const byte StatusSignBitMask = 2;

		[FieldOffset(0)]
		public SqlDecimal SqlDecimal;

		[FieldOffset(0)]
		public uint Flags;

		[FieldOffset(0)]
		public byte Status;

		[FieldOffset(2)]
		public byte Precision;

		[FieldOffset(3)]
		public byte Scale;

		[FieldOffset(4)]
		public uint Low;

		[FieldOffset(8)]
		public uint MedLow;

		[FieldOffset(12)]
		public uint MedHigh;

		[FieldOffset(16)]
		public uint High;

		public bool IsNegative => (Status & 2) != 0;
	}

	private static uint Div10(uint source, ref uint reminder)
	{
		ulong num = ((ulong)reminder << 32) | source;
		reminder = (uint)(num % 10);
		return (uint)(num / 10);
	}

	public static decimal SqlDecimalToDecimal(SqlDecimal sqlDecimal)
	{
		SqlDecimalUnion sqlDecimalUnion = new SqlDecimalUnion
		{
			SqlDecimal = sqlDecimal
		};
		while (sqlDecimalUnion.High != 0 && sqlDecimalUnion.Scale > 0)
		{
			uint reminder = 0u;
			sqlDecimalUnion.High = Div10(sqlDecimalUnion.High, ref reminder);
			sqlDecimalUnion.MedHigh = Div10(sqlDecimalUnion.MedHigh, ref reminder);
			sqlDecimalUnion.MedLow = Div10(sqlDecimalUnion.MedLow, ref reminder);
			sqlDecimalUnion.Low = Div10(sqlDecimalUnion.Low, ref reminder);
			sqlDecimalUnion.Scale--;
		}
		if (sqlDecimalUnion.High != 0)
		{
			throw new OverflowException();
		}
		return new decimal((int)sqlDecimalUnion.Low, (int)sqlDecimalUnion.MedLow, (int)sqlDecimalUnion.MedHigh, sqlDecimalUnion.IsNegative, sqlDecimalUnion.Scale);
	}
}

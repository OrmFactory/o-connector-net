using OBridgeConnector.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBridgeConnector;

public class OBridgeRowDataReader
{
	private readonly List<OBridgeColumn> columns;
	private byte[] nullMask;

	public OBridgeRowDataReader(List<OBridgeColumn> columns)
	{
		this.columns = columns;
	}

	private ValueObject GetValueObject(int ordinal)
	{
		if (IsDBNull(ordinal)) return NullValueObject.Instance;
		return columns[ordinal].ValueObject;
	}

	public bool IsDBNull(int ordinal)
	{
		var column = columns[ordinal];
		if (!column.IsNullable) return false;

		int byteIndex = column.NullableOrdinal / 8;
		int bitIndex = column.NullableOrdinal % 8;
		return (nullMask[byteIndex] & (1 << bitIndex)) == 0;
	}

	public bool GetBoolean(int ordinal) => GetValueObject(ordinal).GetBoolean();
	public byte GetByte(int ordinal) => GetValueObject(ordinal).GetByte();
	public byte[] GetBinary(int ordinal) => GetValueObject(ordinal).GetBinary();
	public char GetChar(int ordinal) => GetValueObject(ordinal).GetChar();
	public DateTime GetDateTime(int ordinal) => GetValueObject(ordinal).GetDateTime();
	public decimal GetDecimal(int ordinal) => GetValueObject(ordinal).GetDecimal();
	public double GetDouble(int ordinal) => GetValueObject(ordinal).GetDouble();
	public float GetFloat(int ordinal) => GetValueObject(ordinal).GetFloat();
	public Guid GetGuid(int ordinal) => GetValueObject(ordinal).GetGuid();
	public short GetInt16(int ordinal) => GetValueObject(ordinal).GetInt16();
	public int GetInt32(int ordinal) => GetValueObject(ordinal).GetInt32();
	public long GetInt64(int ordinal) => GetValueObject(ordinal).GetInt64();
	public string GetString(int ordinal) => GetValueObject(ordinal).GetString();
	public object GetValue(int ordinal) => GetValueObject(ordinal).GetValue();

	public Type GetFieldType(int ordinal) => columns[ordinal].DataType;

	public void ReadRowDataFromArray(byte[] rowData)
	{
		int nullableColumnsCount = columns.Count(c => c.IsNullable);
		int nullMaskBytes = (nullableColumnsCount + 7) / 8;

		var spanReader = new BatchReader(rowData);
		nullMask = spanReader.ReadBytes(nullMaskBytes);
		for (int i = 0; i < columns.Count; i++)
		{
			if (!IsDBNull(i)) columns[i].ValueObject.ReadFromBatch(spanReader);
		}
	}
}
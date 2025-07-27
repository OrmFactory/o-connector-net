using System.Collections;
using System.Data.Common;
using System.Data.SqlTypes;

namespace OBridgeConnector;

public class OBridgeDataReader : DbDataReader
{
	private readonly List<OBridgeColumn> columns;

	private OBridgeDataReader(List<OBridgeColumn> columns)
	{
		this.columns = columns;
	}

	public override int RecordsAffected { get; } = -1;
	public override bool HasRows { get; } = false;
	public override bool IsClosed { get; } = false;

	private List<ValueObject>? currentRow = null;

	public static async Task<OBridgeDataReader> Create(AsyncBinaryReader reader, CancellationToken token)
	{
		byte responseCode = await reader.ReadByteAsync(token);
		if (responseCode == (byte)ResponseTypeEnum.Error) await ReadError(reader, token);
		if (responseCode == (byte)ResponseTypeEnum.TableHeader)
		{
			int columnCount = await reader.Read7BitEncodedIntAsync(token);
			var columnList = new List<OBridgeColumn>();
			for (int i = 0; i < columnCount; i++)
			{
				var column = await OBridgeColumn.FromReader(i, reader, token);
				columnList.Add(column);
			}

			var dbReader = new OBridgeDataReader(columnList);
			return dbReader;
		}

		throw new Exception("Expected TableHeader or Error");
	}

	private static async Task ReadError(AsyncBinaryReader reader, CancellationToken token)
	{
		var errorCode = await reader.ReadByteAsync(token);
		var errorMessage = await reader.ReadStringAsync(token);
		throw new Exception(errorMessage);
	}

	private ValueObject GetValueObject(int ordinal)
	{
		if (currentRow == null) throw new InvalidOperationException("Row not loaded");
		return currentRow[ordinal];
	}

	public override bool GetBoolean(int ordinal) => GetValueObject(ordinal).GetBoolean();
	public override byte GetByte(int ordinal) => GetValueObject(ordinal).GetByte();
	public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
	{
		return GetValueObject(ordinal).GetBytes(dataOffset, buffer, bufferOffset, length);
	}
	public override char GetChar(int ordinal) => GetValueObject(ordinal).GetChar();
	public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
	{
		return GetValueObject(ordinal).GetChars(dataOffset, buffer, bufferOffset, length);
	}
	public override DateTime GetDateTime(int ordinal) => GetValueObject(ordinal).GetDateTime();
	public override decimal GetDecimal(int ordinal) => GetValueObject(ordinal).GetDecimal();
	public override double GetDouble(int ordinal) => GetValueObject(ordinal).GetDouble();
	public override float GetFloat(int ordinal) => GetValueObject(ordinal).GetFloat();
	public override Guid GetGuid(int ordinal) => GetValueObject(ordinal).GetGuid();
	public override short GetInt16(int ordinal) => GetValueObject(ordinal).GetInt16();
	public override int GetInt32(int ordinal) => GetValueObject(ordinal).GetInt32();
	public override long GetInt64(int ordinal) => GetValueObject(ordinal).GetInt64();
	public override string GetString(int ordinal) => GetValueObject(ordinal).GetString();

	public override T GetFieldValue<T>(int ordinal)
	{
		if (typeof(T) == typeof(bool)) return (T)(object)GetBoolean(ordinal);
		if (typeof(T) == typeof(byte)) return (T)(object)GetByte(ordinal);
		if (typeof(T) == typeof(sbyte)) return (T)(object)GetSByte(ordinal);
		if (typeof(T) == typeof(short)) return (T)(object)GetInt16(ordinal);
		if (typeof(T) == typeof(ushort)) return (T)(object)GetUInt16(ordinal);
		if (typeof(T) == typeof(int)) return (T)(object)GetInt32(ordinal);
		if (typeof(T) == typeof(uint)) return (T)(object)GetUInt32(ordinal);
		if (typeof(T) == typeof(long)) return (T)(object)GetInt64(ordinal);
		if (typeof(T) == typeof(ulong)) return (T)(object)GetUInt64(ordinal);
		if (typeof(T) == typeof(char)) return (T)(object)GetChar(ordinal);
		if (typeof(T) == typeof(decimal)) return (T)(object)GetDecimal(ordinal);
		if (typeof(T) == typeof(double)) return (T)(object)GetDouble(ordinal);
		if (typeof(T) == typeof(float)) return (T)(object)GetFloat(ordinal);
		if (typeof(T) == typeof(string)) return (T)(object)GetString(ordinal);
		if (typeof(T) == typeof(DateTime)) return (T)(object)GetDateTime(ordinal);
		if (typeof(T) == typeof(DateTimeOffset)) return (T)(object)GetDateTimeOffset(ordinal);
		if (typeof(T) == typeof(Guid)) return (T)(object)GetGuid(ordinal);
		if (typeof(T) == typeof(Stream)) return (T)(object)GetStream(ordinal);
		if (typeof(T) == typeof(TextReader) || typeof(T) == typeof(StringReader)) return (T)(object)GetTextReader(ordinal);
		if (typeof(T) == typeof(TimeSpan)) return (T)(object)GetTimeSpan(ordinal);
		return base.GetFieldValue<T>(ordinal);
	}

	public override string GetName(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int GetOrdinal(string name)
	{
		throw new NotImplementedException();
	}


	public override object GetValue(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int GetValues(object[] values)
	{
		throw new NotImplementedException();
	}

	public override bool IsDBNull(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override string GetDataTypeName(int ordinal) => columns[ordinal].DataTypeName ?? "";
	public override Type GetFieldType(int ordinal) => columns[ordinal].DataType
	{
		throw new NotImplementedException();
	}

	public override int FieldCount { get; }

	public override object this[int ordinal] => throw new NotImplementedException();

	public override object this[string name] => throw new NotImplementedException();

	public override bool NextResult()
	{
		return NextResultAsync(CancellationToken.None).GetAwaiter().GetResult();
	}

	public override async Task<bool> NextResultAsync(CancellationToken token)
	{
		return false;
	}

	public override bool Read()
	{
		throw new NotImplementedException();
	}

	public override int Depth { get; }

	public override IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
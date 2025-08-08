using OBridgeConnector.OracleTypes;
using OBridgeConnector.ValueObjects;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeDataReader : DbDataReader
{
	private readonly List<OBridgeColumn> columns = new();
	private readonly AsyncBinaryReader reader;
	private readonly OBridgeCommand command;

	public OBridgeDataReader(AsyncBinaryReader reader, OBridgeCommand command)
	{
		this.reader = reader;
		this.command = command;
	}

	private int recordsAffected = -1;
	public override int RecordsAffected => recordsAffected;
	private bool hasRows = false;
	public override bool HasRows => hasRows;
	public override bool IsClosed { get; } = false;
	
	public bool EnableRowDataCollection = false;

	public async Task ReadHeader(CancellationToken token)
	{
		columns.Clear();

		byte responseCode = await reader.ReadByte(token);
		if (responseCode == (byte)ResponseTypeEnum.Error) await ReadError(reader, token);
		if (responseCode == (byte)ResponseTypeEnum.OracleQueryError)
		{
			var message = await reader.ReadString(token);
			throw new Exception(message);
		}
		if (responseCode == (byte)ResponseTypeEnum.TableHeader)
		{
			int columnCount = await reader.Read7BitEncodedInt(token);
			int nullableOrdinal = 0;
			for (int i = 0; i < columnCount; i++)
			{
				var column = new OBridgeColumn();
				await column.LoadFromReader(i, reader, token);
				if (column.IsNullable)
				{
					column.NullableOrdinal = nullableOrdinal;
					nullableOrdinal++;
				}
				columns.Add(column);
			}
			return;
		}

		throw new Exception("Expected TableHeader or Error");
	}

	private static async Task ReadError(AsyncBinaryReader reader, CancellationToken token)
	{
		var errorCode = await reader.ReadByte(token);
		var errorMessage = await reader.ReadString(token);
		throw new Exception(errorMessage);
	}

	private ValueObject GetValueObject(int ordinal)
	{
		if (!hasRows) throw new InvalidOperationException("Row is not loaded");
		if (IsDBNull(ordinal)) return NullValueObject.Instance;
		return columns[ordinal].ValueObject;
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
	public override object GetValue(int ordinal) => GetValueObject(ordinal).GetValue();

	public DateTimeOffset GetDateTimeOffset(int ordinal) => GetValueObject(ordinal).GetDateTimeOffset();
	public OBridgeIntervalYM GetIntervalYM(int ordinal) => GetValueObject(ordinal).GetIntervalYM();
	public OBridgeIntervalDS GetIntervalDS(int ordinal) => GetValueObject(ordinal).GetIntervalDS();
	public TimeSpan GetTimeSpan(int ordinal) => GetValueObject(ordinal).GetTimeSpan();
	public byte[] GetBinary(int ordinal) => GetValueObject(ordinal).GetBinary();

	public override T GetFieldValue<T>(int ordinal)
	{
		if (typeof(T) == typeof(bool)) return (T)(object)GetBoolean(ordinal);
		if (typeof(T) == typeof(byte)) return (T)(object)GetByte(ordinal);
		if (typeof(T) == typeof(short)) return (T)(object)GetInt16(ordinal);
		if (typeof(T) == typeof(int)) return (T)(object)GetInt32(ordinal);
		if (typeof(T) == typeof(long)) return (T)(object)GetInt64(ordinal);
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
		if (typeof(T) == typeof(OBridgeIntervalYM)) return (T)(object)GetIntervalYM(ordinal);
		if (typeof(T) == typeof(OBridgeIntervalDS)) return (T)(object)GetIntervalDS(ordinal);
		if (typeof(T) == typeof(TimeSpan)) return (T)(object)GetTimeSpan(ordinal);
		if (typeof(T) == typeof(byte[])) return (T)(object)GetBinary(ordinal);
		return base.GetFieldValue<T>(ordinal);
	}

	public override string GetName(int ordinal)
	{
		return columns[ordinal].ColumnName;
	}

	public override int GetOrdinal(string name)
	{
		for (int i = 0; i < columns.Count; i++)
		{
			if (columns[i].ColumnName.Equals(name, StringComparison.OrdinalIgnoreCase))
				return i;
		}
		throw new IndexOutOfRangeException($"Column '{name}' not found.");
	}

	public override int GetValues(object[] values)
	{
		int count = Math.Min(values.Length, columns.Count);
		for (int i = 0; i < count; i++)
			values[i] = GetValue(i);

		return count;
	}

	public override bool IsDBNull(int ordinal)
	{
		var column = columns[ordinal];
		if (!column.IsNullable) return false;

		int byteIndex = column.NullableOrdinal / 8;
		int bitIndex = column.NullableOrdinal % 8;
		return (nullMask[byteIndex] & (1 << bitIndex)) == 0;
	}

	public override string GetDataTypeName(int ordinal) => columns[ordinal].DataTypeName ?? "";
	public override Type GetFieldType(int ordinal) => columns[ordinal].DataType;

	public override int FieldCount => columns.Count;

	public override object this[int ordinal] => GetValue(ordinal);

	public override object this[string name] => GetValue(GetOrdinal(name));

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
		return ReadAsync(CancellationToken.None).GetAwaiter().GetResult();
	}

	private byte[] nullMask = [];

	public override async Task<bool> ReadAsync(CancellationToken token)
	{
		byte code = await reader.ReadByte(token);
		if (code == (byte)ResponseTypeEnum.RowData)
		{
			reader.EnableDataCollection = EnableRowDataCollection;
			reader.ClearCollectionBuffer();

			int nullableColumnsCount = columns.Count(c => c.IsNullable);
			int nullMaskBytes = (nullableColumnsCount + 7) / 8;
			nullMask = await reader.ReadBytes(nullMaskBytes, token);
			for (int i = 0; i < columns.Count; i++)
			{
				if (!IsDBNull(i)) await columns[i].ValueObject.ReadFromStream(reader, token);
			}

			if (reader.EnableDataCollection)
			{
				reader.EnableDataCollection = false;
			}

			hasRows = true;
			return true;
		}

		if (code == (byte)ResponseTypeEnum.EndOfRowStream)
		{
			recordsAffected = await reader.Read7BitEncodedInt(token);
			await ReadOutputParameters(token);
			return false;
		}

		if (code == (byte)ResponseTypeEnum.Error)
		{
			await ReadError(reader, token);
			return false;
		}
		throw new Exception($"Unexpected response code: {code}");
	}

	private async Task ReadOutputParameters(CancellationToken token)
	{
		var paramsCount = await reader.Read7BitEncodedInt(token);
		for (int i = 0; i < paramsCount; i++)
		{
			var p = await OBridgeParameter.FromReader(reader, token);
			if (command.Parameters.Contains(p.ParameterName))
			{
				var existingParam = command.Parameters[p.ParameterName];
				existingParam.Value = p.Value;
			}
		}
	}

	public byte[] GetRowData()
	{
		return reader.GetCollectedData();
	}

	public ReadOnlyCollection<DbColumn> GetColumnSchema()
	{
		return columns.Cast<DbColumn>().ToList().AsReadOnly();
	}

	public override Task<ReadOnlyCollection<DbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = new CancellationToken())
	{
		return Task.FromResult(columns.Cast<DbColumn>().ToList().AsReadOnly());
	}

	public override DataTable GetSchemaTable()
	{
		var schemaTable = new DataTable("SchemaTable");

		schemaTable.Columns.Add("ColumnName", typeof(string));
		schemaTable.Columns.Add("ColumnOrdinal", typeof(int));
		schemaTable.Columns.Add("DataType", typeof(Type));
		schemaTable.Columns.Add("DataTypeName", typeof(string));
		schemaTable.Columns.Add("IsNullable", typeof(bool));

		for (int i = 0; i < columns.Count; i++)
		{
			var col = columns[i];
			var row = schemaTable.NewRow();
			row["ColumnName"] = col.ColumnName;
			row["ColumnOrdinal"] = i;
			row["DataType"] = col.DataType;
			row["DataTypeName"] = col.DataTypeName ?? "";
			row["IsNullable"] = col.IsNullable;
			schemaTable.Rows.Add(row);
		}

		return schemaTable;
	}

	public override int Depth { get; }

	public override IEnumerator GetEnumerator()
	{
		while (Read()) yield return this;
	}
}
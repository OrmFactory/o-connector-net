using OBridgeConnector.OracleTypes;
using System.Data;
using System.Data.Common;
using System.Reflection.Metadata;

namespace OBridgeConnector;

public class OBridgeParameter : DbParameter
{
	private DbType dbType;
	private OBridgeDbType? obridgeDbType;
	private string parameterName;
	private object value;
	private string sourceColumn;
	private int size;
	private ParameterDirection direction = ParameterDirection.Input;
	private bool isNullable;
	private bool sourceColumnNullMapping;

	public OBridgeParameter() { }

	public OBridgeParameter(string name, object value)
	{
		ParameterName = name;
		Value = value;
	}

	public OBridgeParameter(string name, OBridgeDbType type)
	{
		ParameterName = name;
		OBridgeDbType = type;
	}

	public override DbType DbType
	{
		get => dbType;
		set
		{
			dbType = value;
			obridgeDbType = MapFromDbType(value);
		}
	}

	public OBridgeDbType OBridgeDbType
	{
		get => obridgeDbType ?? MapFromDbType(dbType);
		set
		{
			obridgeDbType = value;
			dbType = MapToDbType(value);
		}
	}

	public override ParameterDirection Direction
	{
		get => direction;
		set => direction = value;
	}

	public override bool IsNullable
	{
		get => isNullable;
		set => isNullable = value;
	}

	public override string ParameterName
	{
		get => parameterName;
		set => parameterName = value;
	}

	public override string SourceColumn
	{
		get => sourceColumn;
		set => sourceColumn = value;
	}

	public override object Value
	{
		get => value;
		set => this.value = value;
	}

	public override bool SourceColumnNullMapping
	{
		get => sourceColumnNullMapping;
		set => sourceColumnNullMapping = value;
	}

	public override int Size
	{
		get => size;
		set => size = value;
	}

	public override void ResetDbType()
	{
		dbType = DbType.Object;
		obridgeDbType = null;
	}

	private static OBridgeDbType MapFromDbType(DbType type) => type switch
	{
		DbType.AnsiString => OBridgeDbType.Varchar2,
		DbType.String => OBridgeDbType.NVarchar2,
		DbType.StringFixedLength => OBridgeDbType.Char,
		DbType.Int16 => OBridgeDbType.Int16,
		DbType.Int32 => OBridgeDbType.Int32,
		DbType.Int64 => OBridgeDbType.Int64,
		DbType.Decimal => OBridgeDbType.Decimal,
		DbType.Double => OBridgeDbType.Double,
		DbType.Single => OBridgeDbType.Single,
		DbType.Date => OBridgeDbType.Date,
		DbType.DateTime => OBridgeDbType.TimeStamp,
		DbType.Boolean => OBridgeDbType.Boolean,
		DbType.Binary => OBridgeDbType.Raw,
		_ => OBridgeDbType.Object
	};

	private static DbType MapToDbType(OBridgeDbType type) => type switch
	{
		OBridgeDbType.Varchar2 => DbType.AnsiString,
		OBridgeDbType.NVarchar2 => DbType.String,
		OBridgeDbType.Char => DbType.StringFixedLength,
		OBridgeDbType.Int16 => DbType.Int16,
		OBridgeDbType.Int32 => DbType.Int32,
		OBridgeDbType.Int64 => DbType.Int64,
		OBridgeDbType.Decimal => DbType.Decimal,
		OBridgeDbType.Double => DbType.Double,
		OBridgeDbType.Single => DbType.Single,
		OBridgeDbType.Date => DbType.Date,
		OBridgeDbType.TimeStamp => DbType.DateTime,
		OBridgeDbType.Boolean => DbType.Boolean,
		OBridgeDbType.Raw => DbType.Binary,
		_ => DbType.Object
	};

	public void Serialize(Request request)
	{
		request.WriteString(ParameterName);
		request.WriteByte((byte)OBridgeDbType);
		request.WriteByte((byte)Direction);

		if (Direction is ParameterDirection.Output or ParameterDirection.ReturnValue)
			return;

		SerializeNullableValue(request);
	}

	private void SerializeNullableValue(Request request)
	{
		if (Value == null || Value == DBNull.Value)
		{
			request.WriteByte((byte)1);
			return;
		}

		request.WriteByte((byte)0);
		SerializeValue(request);
	}

	public void SerializeValue(Request request)
	{
		switch (OBridgeDbType)
		{
			case OBridgeDbType.Int16:
				request.WriteInt16(Convert.ToInt16(Value));
				break;
			case OBridgeDbType.Int32:
				request.WriteInt32(Convert.ToInt32(Value));
				break;
			case OBridgeDbType.Int64:
				request.WriteInt64(Convert.ToInt64(Value));
				break;
			case OBridgeDbType.Single:
			case OBridgeDbType.BinaryFloat:
				request.WriteFloat(Convert.ToSingle(Value));
				break;
			case OBridgeDbType.Double:
			case OBridgeDbType.BinaryDouble:
				request.WriteDouble(Convert.ToDouble(Value));
				break;
			case OBridgeDbType.Decimal:
				request.WriteDecimal(Convert.ToDecimal(Value));
				break;
			case OBridgeDbType.Boolean:
				request.WriteBoolean(Convert.ToBoolean(Value));
				break;
			case OBridgeDbType.Date:
				request.WriteDateTime(Convert.ToDateTime(Value));
				break;
			case OBridgeDbType.TimeStamp:
			case OBridgeDbType.TimeStampLTZ:
			case OBridgeDbType.TimeStampTZ:
				throw new NotImplementedException(OBridgeDbType.ToString());

			case OBridgeDbType.Char:
			case OBridgeDbType.NChar:
			case OBridgeDbType.Varchar2:
			case OBridgeDbType.NVarchar2:
			case OBridgeDbType.Clob:
			case OBridgeDbType.NClob:
			case OBridgeDbType.Json:
			case OBridgeDbType.ArrayAsJson:
			case OBridgeDbType.ObjectAsJson:
				request.WriteString(Convert.ToString(Value));
				break;
			case OBridgeDbType.Raw:
			case OBridgeDbType.LongRaw:
			case OBridgeDbType.Blob:
				request.WriteBytes((byte[])Value);
				break;
			default:
				throw new NotSupportedException($"Unsupported OBridgeDbType: {OBridgeDbType}");
		}
	}

	public static async Task<OBridgeParameter> FromReader(AsyncBinaryReader reader, CancellationToken token)
	{
		var param = new OBridgeParameter
		{
			ParameterName = await reader.ReadString(token).ConfigureAwait(false),
			OBridgeDbType = (OBridgeDbType)await reader.ReadByte(token).ConfigureAwait(false),
			Direction = (ParameterDirection)await reader.ReadByte(token).ConfigureAwait(false)
		};

		var isNull = await reader.ReadByte(token).ConfigureAwait(false);
		if (isNull == 1)
		{
			param.Value = DBNull.Value;
			return param;
		}

		switch (param.OBridgeDbType)
		{
			case OBridgeDbType.Int16:
				param.Value = await reader.ReadInt16(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Int32:
				param.Value = await reader.ReadInt32(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Int64:
				param.Value = await reader.ReadInt64(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Single:
			case OBridgeDbType.BinaryFloat:
				param.Value = await reader.ReadFloat(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Double:
			case OBridgeDbType.BinaryDouble:
				param.Value = await reader.ReadDouble(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Decimal:
				param.Value = await reader.ReadDecimal(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Boolean:
				param.Value = await reader.ReadBoolean(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Date:
				param.Value = await reader.ReadDateTime(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Char:
			case OBridgeDbType.NChar:
			case OBridgeDbType.Varchar2:
			case OBridgeDbType.NVarchar2:
			case OBridgeDbType.Clob:
			case OBridgeDbType.NClob:
			case OBridgeDbType.Json:
			case OBridgeDbType.ArrayAsJson:
			case OBridgeDbType.ObjectAsJson:
				param.Value = await reader.ReadString(token).ConfigureAwait(false);
				break;
			case OBridgeDbType.Raw:
			case OBridgeDbType.LongRaw:
			case OBridgeDbType.Blob:
				param.Value = await reader.ReadBinary(token).ConfigureAwait(false);
				break;
			default:
				throw new NotSupportedException($"Unsupported OBridgeDbType: {param.OBridgeDbType}");
		}

		return param;
	}
}
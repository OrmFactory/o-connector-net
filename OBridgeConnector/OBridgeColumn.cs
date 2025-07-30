using OBridgeConnector.ValueObjects;
using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeColumn : DbColumn
{
	public async Task LoadFromReader(int ordinal, AsyncBinaryReader reader, CancellationToken token)
	{
		ColumnOrdinal = ordinal;
		var propertyPresenceBits = await reader.ReadByte(token);
		var hasAllowDbNull = (propertyPresenceBits & 0x01) != 0;
		var hasColumnSize = (propertyPresenceBits & 0x02) != 0;
		var hasNumericPrecision = (propertyPresenceBits & 0x04) != 0;
		var hasNumericScale = (propertyPresenceBits & 0x08) != 0;
		var hasIsAliased = (propertyPresenceBits & 0x10) != 0;
		var hasIsExpression = (propertyPresenceBits & 0x20) != 0;
		var hasBaseColumnName = (propertyPresenceBits & 0x40) != 0;
		var hasBaseTableName = (propertyPresenceBits & 0x80) != 0;

		ColumnName = await reader.ReadString(token);
		if (hasAllowDbNull) AllowDBNull = await reader.ReadByte(token) != 0;
		if (hasColumnSize) ColumnSize = await reader.ReadByte(token);
		if (hasNumericPrecision) NumericPrecision = await reader.ReadByte(token);
		if (hasNumericScale)
		{
			var b = await reader.ReadByte(token);
			NumericScale = unchecked((sbyte)b);
		}
		if (hasIsAliased) IsAliased = await reader.ReadByte(token) != 0;
		if (hasIsExpression) IsExpression = await reader.ReadByte(token) != 0;
		if (hasBaseColumnName) BaseColumnName = await reader.ReadString(token);
		if (hasBaseTableName) BaseTableName = await reader.ReadString(token);
	}

	public ValueObject CreateValueObject()
	{

	}
}
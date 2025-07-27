using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeColumn : DbColumn
{
	public static async Task<OBridgeColumn> FromReader(int columnOrdinal, AsyncBinaryReader reader, CancellationToken token)
	{

	}


}

public abstract class ValueObject
{
	public virtual bool GetBoolean() => throw new NotSupportedException();
	public virtual byte GetByte() => throw new NotSupportedException();
	public virtual long GetBytes(long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual char GetChar() => throw new NotSupportedException();
	public virtual long GetChars(long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual DateTime GetDateTime() => throw new NotSupportedException();
	public virtual decimal GetDecimal() => throw new NotSupportedException();
	public virtual double GetDouble() => throw new NotSupportedException();
}
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBridgeConnector;

public class Request
{
	private readonly MemoryStream buffer = new();
	private readonly Encoding encoding = Encoding.UTF8;
	private readonly byte[] intBuffer = new byte[8];

	public Request()
	{

	}

	public Request(CommandEnum command)
	{
		WriteByte((byte)command);
	}

	public void WriteByte(byte value)
	{
		buffer.WriteByte(value);
	}

	public void WriteInt16(short value)
	{
		BinaryPrimitives.WriteInt16LittleEndian(intBuffer, value);
		buffer.Write(intBuffer, 0, 2);
	}

	public void WriteInt32(int value)
	{
		BinaryPrimitives.WriteInt32LittleEndian(intBuffer, value);
		buffer.Write(intBuffer, 0, 4);
	}

	public void WriteInt64(long value)
	{
		BinaryPrimitives.WriteInt64LittleEndian(intBuffer, value);
		buffer.Write(intBuffer, 0, 8);
	}

	public void Write7BitEncodedInt(int value)
	{
		uint v = (uint)value;
		while (v >= 0x80)
		{
			buffer.WriteByte((byte)(v | 0x80));
			v >>= 7;
		}
		buffer.WriteByte((byte)v);
	}

	public void WriteString(string value)
	{
		var bytes = encoding.GetBytes(value);
		Write7BitEncodedInt(bytes.Length);
		buffer.Write(bytes, 0, bytes.Length);
	}

	public async Task SendAsync(Stream output, CancellationToken token)
	{
		buffer.Position = 0;
		await buffer.CopyToAsync(output, token);
	}

	public byte[] ToArray() => buffer.ToArray();

	public void Reset()
	{
		buffer.SetLength(0);
	}

	public void WriteByte(sbyte value)
	{
		WriteByte(unchecked((byte)value));
	}

	public void WriteFloat(float value)
	{
		BinaryPrimitives.WriteSingleLittleEndian(intBuffer, value);
		buffer.Write(intBuffer, 0, 4);
	}

	public void WriteDouble(double value)
	{
		BinaryPrimitives.WriteDoubleLittleEndian(intBuffer, value);
		buffer.Write(intBuffer, 0, 8);
	}

	public void WriteBytes(byte[] bytes)
	{
		buffer.Write(bytes, 0, bytes.Length);
	}

	public void WriteDecimal(decimal value)
	{
		int[] bits = decimal.GetBits(value);
		foreach (int part in bits)
			WriteInt32(part);
	}

	public void WriteBoolean(bool value)
	{
		WriteByte((byte)(value ? 1 : 0));
	}

	public void WriteDateTime(DateTime value)
	{
		WriteInt64(value.Ticks);
	}

	public void Append(Request request)
	{
		buffer.Write(request.buffer.GetBuffer());
	}
}
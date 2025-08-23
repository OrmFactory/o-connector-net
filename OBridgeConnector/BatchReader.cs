using System;
using System.Buffers.Binary;
using System.Text;

namespace OBridgeConnector;

public class BatchReader
{
	private byte[] buffer;
	private int offset = 0;

	private int currentByte = 0;
	private int bitPosition = 0;

	public BatchReader(byte[] bytes)
	{
		this.buffer = bytes;
	}

	public bool ReadBit()
	{
		if (bitPosition == 0) currentByte = ReadByte();

		var result = (currentByte & (1 << (7 - bitPosition))) != 0;
		bitPosition++;
		if (bitPosition == 8) bitPosition = 0;
		return result;
	}

	public int ReadBits(int count)
	{
		int result = 0;

		while (count > 0)
		{
			if (bitPosition == 0) currentByte = ReadByte();

			int bitsAvailable = 8 - bitPosition;
			int bitsToRead = Math.Min(count, bitsAvailable);

			int shift = bitsAvailable - bitsToRead;
			int mask = (1 << bitsToRead) - 1;

			int bits = (currentByte >> shift) & mask;

			result = (result << bitsToRead) | bits;

			bitPosition += bitsToRead;
			if (bitPosition == 8) bitPosition = 0;
			count -= bitsToRead;
		}

		return result;
	}

	public int ReadSignedBits(int bitCount)
	{
		var isNegative = ReadBit();
		int abs = ReadBits(bitCount - 1);
		return isNegative ? -abs : abs;
	}

	public byte ReadByte()
	{
		bitPosition = 0;
		return buffer[offset++];
	}

	public float ReadFloat()
	{
		bitPosition = 0;
		var result = BinaryPrimitives.ReadSingleLittleEndian(buffer.AsSpan(offset, 4));
		offset += 4;
		return result;
	}

	public double ReadDouble()
	{
		bitPosition = 0;
		var result = BinaryPrimitives.ReadDoubleLittleEndian(buffer.AsSpan(offset, 8));
		offset += 8;
		return result;
	}

	public int Read7BitEncodedInt()
	{
		bitPosition = 0;

		int count = 0;
		int shift = 0;

		while (true)
		{
			byte b = ReadByte();
			count |= (b & 0x7F) << shift;

			if ((b & 0x80) == 0)
				break;

			shift += 7;
			if (shift >= 35)
				throw new FormatException("Too many bytes in 7-bit encoded int.");
		}

		return count;
	}

	public string ReadString()
	{
		bitPosition = 0;
		int length = Read7BitEncodedInt();
		if (length == 0) return string.Empty;

		var result = Encoding.UTF8.GetString(buffer.AsSpan(offset, length));
		offset += length;
		return result;
	}

	public ReadOnlySpan<byte> ReadBytesAsSpan(int count)
	{
		bitPosition = 0;
		var result = buffer.AsSpan(offset, count);
		offset += count;
		return result;
	}

	public void ResetBitPosition()
	{
		bitPosition = 0;
	}

	public byte[] ReadBytes(int count)
	{
		return ReadBytesAsSpan(count).ToArray();
	}

	public int Offset => offset;
	public bool HasBytes => Offset < buffer.Length;
}
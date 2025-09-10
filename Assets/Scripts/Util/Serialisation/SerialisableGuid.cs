using System;

namespace Util.Serialisation
{
	[Serializable]
	public struct SerialisableGuid
	{
		public ulong A;
		public ulong B;

		public SerialisableGuid(ulong a, ulong b)
		{
			A = a;
			B = b;
		}

		public SerialisableGuid(Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			A = BitConverter.ToUInt64(bytes, 0);
			B = BitConverter.ToUInt64(bytes, 8);
		}
		public Guid ToGuid()
		{
			byte[] bytes = new byte[16];
			Buffer.BlockCopy(BitConverter.GetBytes(A), 0, bytes, 0, 8);
			Buffer.BlockCopy(BitConverter.GetBytes(B), 0, bytes, 8, 8);
			return new Guid(bytes);
		}

		public bool IsEmpty()
		{
			return A == 0 && B == 0;
		}

		public static implicit operator Guid(SerialisableGuid guid)
		{
			byte[] bytes = new byte[16];
			Buffer.BlockCopy(BitConverter.GetBytes(guid.A), 0, bytes, 0, 8);
			Buffer.BlockCopy(BitConverter.GetBytes(guid.B), 0, bytes, 8, 8);
			return new Guid(bytes);
		}

		public static implicit operator SerialisableGuid(Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			ulong a = BitConverter.ToUInt64(bytes, 0);
			ulong b = BitConverter.ToUInt64(bytes, 8);
			return new SerialisableGuid(a, b);
		}
	}
}
using System;
using System.IO;
using System.Security.Cryptography;

namespace SmartAssembly.Zip
{
	// Token: 0x02000036 RID: 54
	public static class SimpleZip
	{
		// Token: 0x060000B8 RID: 184 RVA: 0x0002F440 File Offset: 0x0002D640
		private static ICryptoTransform GetAesTransform(byte[] key, byte[] iv, bool decrypt)
		{
			ICryptoTransform cryptoTransform;
			using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
			{
				cryptoTransform = (decrypt ? aesCryptoServiceProvider.CreateDecryptor(key, iv) : aesCryptoServiceProvider.CreateEncryptor(key, iv));
			}
			return cryptoTransform;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0002F488 File Offset: 0x0002D688
		public static CompressionAlgorithm GetCompressionAlgorithm(byte[] data)
		{
			if (data == null || data.Length < 4)
			{
				return (CompressionAlgorithm)(-1);
			}
			int num;
			using (SimpleZip.ZipStream zipStream = new SimpleZip.ZipStream(data))
			{
				num = zipStream.ReadInt();
			}
			if (num == 67324752)
			{
				return CompressionAlgorithm.PKZip;
			}
			int num2 = num >> 24;
			num -= num2 << 24;
			if (num == 8223355)
			{
				return (CompressionAlgorithm)num2;
			}
			return (CompressionAlgorithm)(-2);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0002F4EC File Offset: 0x0002D6EC
		public static byte[] Unzip(byte[] buffer)
		{
			SimpleZip.ZipStream zipStream = new SimpleZip.ZipStream(buffer);
			byte[] array = new byte[0];
			int num = zipStream.ReadInt();
			int num2 = num >> 24;
			if (num - (num2 << 24) == 8223355)
			{
				switch (num2)
				{
				case 1:
				{
					int num3 = zipStream.ReadInt();
					array = new byte[num3];
					int num5;
					for (int i = 0; i < num3; i += num5)
					{
						int num4 = zipStream.ReadInt();
						num5 = zipStream.ReadInt();
						byte[] array2 = new byte[num4];
						zipStream.Read(array2, 0, array2.Length);
						new SimpleZip.Inflater(array2).Inflate(array, i, num5);
					}
					goto IL_0116;
				}
				case 3:
				{
					byte[] array3 = new byte[]
					{
						254, 202, 188, 77, 232, 67, 84, 199, 131, 244,
						15, 201, 208, 233, 15, 149
					};
					byte[] array4 = new byte[]
					{
						19, 162, 121, 91, 33, 195, 102, 33, 191, 185,
						203, 221, 224, 233, 97, 181
					};
					using (ICryptoTransform aesTransform = SimpleZip.GetAesTransform(array3, array4, true))
					{
						array = SimpleZip.Unzip(aesTransform.TransformFinalBlock(buffer, 4, buffer.Length - 4));
						goto IL_0116;
					}
					goto IL_010B;
				}
				}
				throw new ArgumentOutOfRangeException("version", num2, "Selected compression algorithm is not supported.");
				IL_0116:
				zipStream.Close();
				zipStream = null;
				return array;
			}
			IL_010B:
			throw new FormatException("Unknown Header");
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0002AD20 File Offset: 0x00028F20
		public static byte[] Zip(byte[] buffer)
		{
			return SimpleZip.Zip(buffer, CompressionAlgorithm.RawZip, null, null);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x0002AD2B File Offset: 0x00028F2B
		public static byte[] ZipAndAes(byte[] buffer, byte[] key, byte[] iv)
		{
			return SimpleZip.Zip(buffer, CompressionAlgorithm.RawZipAndAes, key, iv);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0002F628 File Offset: 0x0002D828
		private static byte[] Zip(byte[] buffer, CompressionAlgorithm algorithm, byte[] key, byte[] iv)
		{
			byte[] array6;
			try
			{
				SimpleZip.ZipStream zipStream = new SimpleZip.ZipStream();
				switch (algorithm)
				{
				case CompressionAlgorithm.RawZip:
				{
					zipStream.WriteInt(25000571);
					zipStream.WriteInt(buffer.Length);
					byte[] array;
					for (int i = 0; i < buffer.Length; i += array.Length)
					{
						array = new byte[Math.Min(2097151, buffer.Length - i)];
						Buffer.BlockCopy(buffer, i, array, 0, array.Length);
						long position = zipStream.Position;
						zipStream.WriteInt(0);
						zipStream.WriteInt(array.Length);
						SimpleZip.Deflater deflater = new SimpleZip.Deflater();
						deflater.SetInput(array);
						while (!deflater.IsNeedingInput)
						{
							byte[] array2 = new byte[512];
							int num = deflater.Deflate(array2);
							if (num <= 0)
							{
								break;
							}
							zipStream.Write(array2, 0, num);
						}
						deflater.Finish();
						while (!deflater.IsFinished)
						{
							byte[] array3 = new byte[512];
							int num2 = deflater.Deflate(array3);
							if (num2 <= 0)
							{
								break;
							}
							zipStream.Write(array3, 0, num2);
						}
						long position2 = zipStream.Position;
						zipStream.Position = position;
						zipStream.WriteInt((int)deflater.TotalOut);
						zipStream.Position = position2;
					}
					goto IL_0185;
				}
				case CompressionAlgorithm.RawZipAndAes:
				{
					zipStream.WriteInt(58555003);
					byte[] array4 = SimpleZip.Zip(buffer, CompressionAlgorithm.RawZip, null, null);
					using (ICryptoTransform aesTransform = SimpleZip.GetAesTransform(key, iv, false))
					{
						byte[] array5 = aesTransform.TransformFinalBlock(array4, 0, array4.Length);
						zipStream.Write(array5, 0, array5.Length);
					}
					goto IL_0185;
				}
				}
				throw new ArgumentOutOfRangeException("algorithm", algorithm, "Selected compression algorithm is not supported.");
				IL_0185:
				zipStream.Flush();
				zipStream.Close();
				array6 = zipStream.ToArray();
			}
			catch (Exception ex)
			{
				SimpleZip.ExceptionMessage = "ERR 2003: " + ex.Message;
				throw;
			}
			return array6;
		}

		// Token: 0x040000A4 RID: 164
		public static string ExceptionMessage;

		// Token: 0x02000037 RID: 55
		internal sealed class Inflater
		{
			// Token: 0x060000BE RID: 190 RVA: 0x0002AD36 File Offset: 0x00028F36
			public Inflater(byte[] bytes)
			{
				this.input = new SimpleZip.StreamManipulator();
				this.outputWindow = new SimpleZip.OutputWindow();
				this.mode = 2;
				this.input.SetInput(bytes, 0, bytes.Length);
			}

			// Token: 0x060000BF RID: 191 RVA: 0x0002F820 File Offset: 0x0002DA20
			private bool DecodeHuffman()
			{
				int i = this.outputWindow.GetFreeSpace();
				while (i >= 258)
				{
					int num;
					switch (this.mode)
					{
					case 7:
						while (((num = this.litlenTree.GetSymbol(this.input)) & -256) == 0)
						{
							this.outputWindow.Write(num);
							if (--i < 258)
							{
								return true;
							}
						}
						if (num >= 257)
						{
							this.repLength = SimpleZip.Inflater.CPLENS[num - 257];
							this.neededBits = SimpleZip.Inflater.CPLEXT[num - 257];
							goto IL_009C;
						}
						if (num < 0)
						{
							return false;
						}
						this.distTree = null;
						this.litlenTree = null;
						this.mode = 2;
						return true;
					case 8:
						goto IL_009C;
					case 9:
						goto IL_00EC;
					case 10:
						break;
					default:
						continue;
					}
					IL_011F:
					if (this.neededBits > 0)
					{
						this.mode = 10;
						int num2 = this.input.PeekBits(this.neededBits);
						if (num2 < 0)
						{
							return false;
						}
						this.input.DropBits(this.neededBits);
						this.repDist += num2;
					}
					this.outputWindow.Repeat(this.repLength, this.repDist);
					i -= this.repLength;
					this.mode = 7;
					continue;
					IL_00EC:
					num = this.distTree.GetSymbol(this.input);
					if (num >= 0)
					{
						this.repDist = SimpleZip.Inflater.CPDIST[num];
						this.neededBits = SimpleZip.Inflater.CPDEXT[num];
						goto IL_011F;
					}
					return false;
					IL_009C:
					if (this.neededBits > 0)
					{
						this.mode = 8;
						int num3 = this.input.PeekBits(this.neededBits);
						if (num3 < 0)
						{
							return false;
						}
						this.input.DropBits(this.neededBits);
						this.repLength += num3;
					}
					this.mode = 9;
					goto IL_00EC;
				}
				return true;
			}

			// Token: 0x060000C0 RID: 192 RVA: 0x0002F9F0 File Offset: 0x0002DBF0
			private bool Decode()
			{
				switch (this.mode)
				{
				case 2:
				{
					if (this.isLastBlock)
					{
						this.mode = 12;
						return false;
					}
					int num = this.input.PeekBits(3);
					if (num < 0)
					{
						return false;
					}
					this.input.DropBits(3);
					if ((num & 1) != 0)
					{
						this.isLastBlock = true;
					}
					switch (num >> 1)
					{
					case 0:
						this.input.SkipToByteBoundary();
						this.mode = 3;
						break;
					case 1:
						this.litlenTree = SimpleZip.InflaterHuffmanTree.defLitLenTree;
						this.distTree = SimpleZip.InflaterHuffmanTree.defDistTree;
						this.mode = 7;
						break;
					case 2:
						this.dynHeader = new SimpleZip.InflaterDynHeader();
						this.mode = 6;
						break;
					}
					return true;
				}
				case 3:
					if ((this.uncomprLen = this.input.PeekBits(16)) < 0)
					{
						return false;
					}
					this.input.DropBits(16);
					this.mode = 4;
					break;
				case 4:
					break;
				case 5:
					goto IL_0131;
				case 6:
					if (!this.dynHeader.Decode(this.input))
					{
						return false;
					}
					this.litlenTree = this.dynHeader.BuildLitLenTree();
					this.distTree = this.dynHeader.BuildDistTree();
					this.mode = 7;
					goto IL_01B5;
				case 7:
				case 8:
				case 9:
				case 10:
					goto IL_01B5;
				case 11:
					return false;
				case 12:
					return false;
				default:
					return false;
				}
				if (this.input.PeekBits(16) < 0)
				{
					return false;
				}
				this.input.DropBits(16);
				this.mode = 5;
				IL_0131:
				int num2 = this.outputWindow.CopyStored(this.input, this.uncomprLen);
				this.uncomprLen -= num2;
				if (this.uncomprLen == 0)
				{
					this.mode = 2;
					return true;
				}
				return !this.input.IsNeedingInput;
				IL_01B5:
				return this.DecodeHuffman();
			}

			// Token: 0x060000C1 RID: 193 RVA: 0x0002FBBC File Offset: 0x0002DDBC
			public int Inflate(byte[] buf, int offset, int len)
			{
				int num = 0;
				for (;;)
				{
					if (this.mode != 11)
					{
						int num2 = this.outputWindow.CopyOutput(buf, offset, len);
						offset += num2;
						num += num2;
						len -= num2;
						if (len == 0)
						{
							return num;
						}
					}
					if (!this.Decode())
					{
						if (this.outputWindow.GetAvailable() <= 0)
						{
							break;
						}
						if (this.mode == 11)
						{
							break;
						}
					}
				}
				return num;
			}

			// Token: 0x040000A5 RID: 165
			private static readonly int[] CPLENS = new int[]
			{
				3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
				15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
				67, 83, 99, 115, 131, 163, 195, 227, 258
			};

			// Token: 0x040000A6 RID: 166
			private static readonly int[] CPLEXT = new int[]
			{
				0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
				1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
				4, 4, 4, 4, 5, 5, 5, 5, 0
			};

			// Token: 0x040000A7 RID: 167
			private static readonly int[] CPDIST = new int[]
			{
				1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
				33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
				1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
			};

			// Token: 0x040000A8 RID: 168
			private static readonly int[] CPDEXT = new int[]
			{
				0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
				4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
				9, 9, 10, 10, 11, 11, 12, 12, 13, 13
			};

			// Token: 0x040000A9 RID: 169
			private const int DECODE_HEADER = 0;

			// Token: 0x040000AA RID: 170
			private const int DECODE_DICT = 1;

			// Token: 0x040000AB RID: 171
			private const int DECODE_BLOCKS = 2;

			// Token: 0x040000AC RID: 172
			private const int DECODE_STORED_LEN1 = 3;

			// Token: 0x040000AD RID: 173
			private const int DECODE_STORED_LEN2 = 4;

			// Token: 0x040000AE RID: 174
			private const int DECODE_STORED = 5;

			// Token: 0x040000AF RID: 175
			private const int DECODE_DYN_HEADER = 6;

			// Token: 0x040000B0 RID: 176
			private const int DECODE_HUFFMAN = 7;

			// Token: 0x040000B1 RID: 177
			private const int DECODE_HUFFMAN_LENBITS = 8;

			// Token: 0x040000B2 RID: 178
			private const int DECODE_HUFFMAN_DIST = 9;

			// Token: 0x040000B3 RID: 179
			private const int DECODE_HUFFMAN_DISTBITS = 10;

			// Token: 0x040000B4 RID: 180
			private const int DECODE_CHKSUM = 11;

			// Token: 0x040000B5 RID: 181
			private const int FINISHED = 12;

			// Token: 0x040000B6 RID: 182
			private int mode;

			// Token: 0x040000B7 RID: 183
			private int neededBits;

			// Token: 0x040000B8 RID: 184
			private int repLength;

			// Token: 0x040000B9 RID: 185
			private int repDist;

			// Token: 0x040000BA RID: 186
			private int uncomprLen;

			// Token: 0x040000BB RID: 187
			private bool isLastBlock;

			// Token: 0x040000BC RID: 188
			private SimpleZip.StreamManipulator input;

			// Token: 0x040000BD RID: 189
			private SimpleZip.OutputWindow outputWindow;

			// Token: 0x040000BE RID: 190
			private SimpleZip.InflaterDynHeader dynHeader;

			// Token: 0x040000BF RID: 191
			private SimpleZip.InflaterHuffmanTree litlenTree;

			// Token: 0x040000C0 RID: 192
			private SimpleZip.InflaterHuffmanTree distTree;
		}

		// Token: 0x02000038 RID: 56
		internal sealed class StreamManipulator
		{
			// Token: 0x060000C3 RID: 195 RVA: 0x0002FC8C File Offset: 0x0002DE8C
			public int PeekBits(int n)
			{
				if (this.bits_in_buffer < n)
				{
					if (this.window_start == this.window_end)
					{
						return -1;
					}
					uint num = this.buffer;
					byte[] array = this.window;
					int num2 = this.window_start;
					this.window_start = num2 + 1;
					uint num3 = array[num2] & 255U;
					byte[] array2 = this.window;
					num2 = this.window_start;
					this.window_start = num2 + 1;
					this.buffer = num | ((num3 | ((array2[num2] & 255U) << 8)) << this.bits_in_buffer);
					this.bits_in_buffer += 16;
				}
				return (int)((ulong)this.buffer & (ulong)((long)((1 << n) - 1)));
			}

			// Token: 0x060000C4 RID: 196 RVA: 0x0002AD6B File Offset: 0x00028F6B
			public void DropBits(int n)
			{
				this.buffer >>= n;
				this.bits_in_buffer -= n;
			}

			// Token: 0x17000021 RID: 33
			// (get) Token: 0x060000C5 RID: 197 RVA: 0x0002AD8C File Offset: 0x00028F8C
			public int AvailableBits
			{
				get
				{
					return this.bits_in_buffer;
				}
			}

			// Token: 0x17000022 RID: 34
			// (get) Token: 0x060000C6 RID: 198 RVA: 0x0002AD94 File Offset: 0x00028F94
			public int AvailableBytes
			{
				get
				{
					return this.window_end - this.window_start + (this.bits_in_buffer >> 3);
				}
			}

			// Token: 0x060000C7 RID: 199 RVA: 0x0002ADAC File Offset: 0x00028FAC
			public void SkipToByteBoundary()
			{
				this.buffer >>= this.bits_in_buffer & 7;
				this.bits_in_buffer &= -8;
			}

			// Token: 0x17000023 RID: 35
			// (get) Token: 0x060000C8 RID: 200 RVA: 0x0002ADD5 File Offset: 0x00028FD5
			public bool IsNeedingInput
			{
				get
				{
					return this.window_start == this.window_end;
				}
			}

			// Token: 0x060000C9 RID: 201 RVA: 0x0002FD2C File Offset: 0x0002DF2C
			public int CopyBytes(byte[] output, int offset, int length)
			{
				int num = 0;
				while (this.bits_in_buffer > 0 && length > 0)
				{
					output[offset++] = (byte)this.buffer;
					this.buffer >>= 8;
					this.bits_in_buffer -= 8;
					length--;
					num++;
				}
				if (length == 0)
				{
					return num;
				}
				int num2 = this.window_end - this.window_start;
				if (length > num2)
				{
					length = num2;
				}
				Array.Copy(this.window, this.window_start, output, offset, length);
				this.window_start += length;
				if (((this.window_start - this.window_end) & 1) != 0)
				{
					byte[] array = this.window;
					int num3 = this.window_start;
					this.window_start = num3 + 1;
					this.buffer = array[num3] & 255U;
					this.bits_in_buffer = 8;
				}
				return num + length;
			}

			// Token: 0x060000CA RID: 202 RVA: 0x0002ADE5 File Offset: 0x00028FE5
			public void Reset()
			{
				this.bits_in_buffer = 0;
				this.window_end = 0;
				this.window_start = 0;
				this.buffer = 0U;
			}

			// Token: 0x060000CB RID: 203 RVA: 0x0002FDFC File Offset: 0x0002DFFC
			public void SetInput(byte[] buf, int off, int len)
			{
				if (this.window_start < this.window_end)
				{
					throw new InvalidOperationException();
				}
				int num = off + len;
				if (0 <= off && off <= num && num <= buf.Length)
				{
					if ((len & 1) != 0)
					{
						this.buffer |= (uint)((uint)(buf[off++] & byte.MaxValue) << this.bits_in_buffer);
						this.bits_in_buffer += 8;
					}
					this.window = buf;
					this.window_start = off;
					this.window_end = num;
					return;
				}
				throw new ArgumentOutOfRangeException();
			}

			// Token: 0x040000C1 RID: 193
			private byte[] window;

			// Token: 0x040000C2 RID: 194
			private int window_start;

			// Token: 0x040000C3 RID: 195
			private int window_end;

			// Token: 0x040000C4 RID: 196
			private uint buffer;

			// Token: 0x040000C5 RID: 197
			private int bits_in_buffer;
		}

		// Token: 0x02000039 RID: 57
		internal sealed class OutputWindow
		{
			// Token: 0x060000CD RID: 205 RVA: 0x0002FE84 File Offset: 0x0002E084
			public void Write(int abyte)
			{
				int num = this.windowFilled;
				this.windowFilled = num + 1;
				if (num == 32768)
				{
					throw new InvalidOperationException();
				}
				byte[] array = this.window;
				num = this.windowEnd;
				this.windowEnd = num + 1;
				array[num] = (byte)abyte;
				this.windowEnd &= 32767;
			}

			// Token: 0x060000CE RID: 206 RVA: 0x0002FEDC File Offset: 0x0002E0DC
			private void SlowRepeat(int repStart, int len)
			{
				while (len-- > 0)
				{
					byte[] array = this.window;
					int num = this.windowEnd;
					this.windowEnd = num + 1;
					array[num] = this.window[repStart++];
					this.windowEnd &= 32767;
					repStart &= 32767;
				}
			}

			// Token: 0x060000CF RID: 207 RVA: 0x0002FF38 File Offset: 0x0002E138
			public void Repeat(int len, int dist)
			{
				if ((this.windowFilled += len) > 32768)
				{
					throw new InvalidOperationException();
				}
				int num = (this.windowEnd - dist) & 32767;
				int num2 = 32768 - len;
				if (num > num2 || this.windowEnd >= num2)
				{
					this.SlowRepeat(num, len);
					return;
				}
				if (len <= dist)
				{
					Array.Copy(this.window, num, this.window, this.windowEnd, len);
					this.windowEnd += len;
					return;
				}
				while (len-- > 0)
				{
					byte[] array = this.window;
					int num3 = this.windowEnd;
					this.windowEnd = num3 + 1;
					array[num3] = this.window[num++];
				}
			}

			// Token: 0x060000D0 RID: 208 RVA: 0x0002FFEC File Offset: 0x0002E1EC
			public int CopyStored(SimpleZip.StreamManipulator input, int len)
			{
				len = Math.Min(Math.Min(len, 32768 - this.windowFilled), input.AvailableBytes);
				int num = 32768 - this.windowEnd;
				int num2;
				if (len > num)
				{
					num2 = input.CopyBytes(this.window, this.windowEnd, num);
					if (num2 == num)
					{
						num2 += input.CopyBytes(this.window, 0, len - num);
					}
				}
				else
				{
					num2 = input.CopyBytes(this.window, this.windowEnd, len);
				}
				this.windowEnd = (this.windowEnd + num2) & 32767;
				this.windowFilled += num2;
				return num2;
			}

			// Token: 0x060000D1 RID: 209 RVA: 0x00030090 File Offset: 0x0002E290
			public void CopyDict(byte[] dict, int offset, int len)
			{
				if (this.windowFilled > 0)
				{
					throw new InvalidOperationException();
				}
				if (len > 32768)
				{
					offset += len - 32768;
					len = 32768;
				}
				Array.Copy(dict, offset, this.window, 0, len);
				this.windowEnd = len & 32767;
			}

			// Token: 0x060000D2 RID: 210 RVA: 0x0002AE03 File Offset: 0x00029003
			public int GetFreeSpace()
			{
				return 32768 - this.windowFilled;
			}

			// Token: 0x060000D3 RID: 211 RVA: 0x0002AE11 File Offset: 0x00029011
			public int GetAvailable()
			{
				return this.windowFilled;
			}

			// Token: 0x060000D4 RID: 212 RVA: 0x000300E4 File Offset: 0x0002E2E4
			public int CopyOutput(byte[] output, int offset, int len)
			{
				int num = this.windowEnd;
				if (len > this.windowFilled)
				{
					len = this.windowFilled;
				}
				else
				{
					num = (this.windowEnd - this.windowFilled + len) & 32767;
				}
				int num2 = len;
				int num3 = len - num;
				if (num3 > 0)
				{
					Array.Copy(this.window, 32768 - num3, output, offset, num3);
					offset += num3;
					len = num;
				}
				Array.Copy(this.window, num - len, output, offset, len);
				this.windowFilled -= num2;
				if (this.windowFilled < 0)
				{
					throw new InvalidOperationException();
				}
				return num2;
			}

			// Token: 0x060000D5 RID: 213 RVA: 0x0002AE19 File Offset: 0x00029019
			public void Reset()
			{
				this.windowEnd = 0;
				this.windowFilled = 0;
			}

			// Token: 0x040000C6 RID: 198
			private const int WINDOW_SIZE = 32768;

			// Token: 0x040000C7 RID: 199
			private const int WINDOW_MASK = 32767;

			// Token: 0x040000C8 RID: 200
			private byte[] window = new byte[32768];

			// Token: 0x040000C9 RID: 201
			private int windowEnd;

			// Token: 0x040000CA RID: 202
			private int windowFilled;
		}

		// Token: 0x0200003A RID: 58
		internal sealed class InflaterHuffmanTree
		{
			// Token: 0x060000D7 RID: 215 RVA: 0x00030178 File Offset: 0x0002E378
			static InflaterHuffmanTree()
			{
				byte[] array = new byte[288];
				int i = 0;
				while (i < 144)
				{
					array[i++] = 8;
				}
				while (i < 256)
				{
					array[i++] = 9;
				}
				while (i < 280)
				{
					array[i++] = 7;
				}
				while (i < 288)
				{
					array[i++] = 8;
				}
				SimpleZip.InflaterHuffmanTree.defLitLenTree = new SimpleZip.InflaterHuffmanTree(array);
				array = new byte[32];
				i = 0;
				while (i < 32)
				{
					array[i++] = 5;
				}
				SimpleZip.InflaterHuffmanTree.defDistTree = new SimpleZip.InflaterHuffmanTree(array);
			}

			// Token: 0x060000D8 RID: 216 RVA: 0x0002AE41 File Offset: 0x00029041
			public InflaterHuffmanTree(byte[] codeLengths)
			{
				this.BuildTree(codeLengths);
			}

			// Token: 0x060000D9 RID: 217 RVA: 0x0003020C File Offset: 0x0002E40C
			private void BuildTree(byte[] codeLengths)
			{
				int[] array = new int[16];
				int[] array2 = new int[16];
				foreach (int num in codeLengths)
				{
					if (num > 0)
					{
						array[num]++;
					}
				}
				int num2 = 0;
				int num3 = 512;
				for (int j = 1; j <= 15; j++)
				{
					array2[j] = num2;
					num2 += array[j] << 16 - j;
					if (j >= 10)
					{
						int num4 = array2[j] & 130944;
						int num5 = num2 & 130944;
						num3 += num5 - num4 >> 16 - j;
					}
				}
				this.tree = new short[num3];
				int num6 = 512;
				for (int k = 15; k >= 10; k--)
				{
					int num7 = num2 & 130944;
					num2 -= array[k] << 16 - k;
					for (int l = num2 & 130944; l < num7; l += 128)
					{
						this.tree[(int)SimpleZip.DeflaterHuffman.BitReverse(l)] = (short)((-num6 << 4) | k);
						num6 += 1 << k - 9;
					}
				}
				for (int m = 0; m < codeLengths.Length; m++)
				{
					int num8 = (int)codeLengths[m];
					if (num8 != 0)
					{
						num2 = array2[num8];
						int num9 = (int)SimpleZip.DeflaterHuffman.BitReverse(num2);
						if (num8 <= 9)
						{
							do
							{
								this.tree[num9] = (short)((m << 4) | num8);
								num9 += 1 << num8;
							}
							while (num9 < 512);
						}
						else
						{
							int num10 = (int)this.tree[num9 & 511];
							int num11 = 1 << (num10 & 15);
							num10 = -(num10 >> 4);
							do
							{
								this.tree[num10 | (num9 >> 9)] = (short)((m << 4) | num8);
								num9 += 1 << num8;
							}
							while (num9 < num11);
						}
						array2[num8] = num2 + (1 << 16 - num8);
					}
				}
			}

			// Token: 0x060000DA RID: 218 RVA: 0x000303EC File Offset: 0x0002E5EC
			public int GetSymbol(SimpleZip.StreamManipulator input)
			{
				int num;
				if ((num = input.PeekBits(9)) >= 0)
				{
					int num2;
					if ((num2 = (int)this.tree[num]) >= 0)
					{
						input.DropBits(num2 & 15);
						return num2 >> 4;
					}
					int num3 = -(num2 >> 4);
					int num4 = num2 & 15;
					if ((num = input.PeekBits(num4)) >= 0)
					{
						num2 = (int)this.tree[num3 | (num >> 9)];
						input.DropBits(num2 & 15);
						return num2 >> 4;
					}
					int availableBits = input.AvailableBits;
					num = input.PeekBits(availableBits);
					num2 = (int)this.tree[num3 | (num >> 9)];
					if ((num2 & 15) <= availableBits)
					{
						input.DropBits(num2 & 15);
						return num2 >> 4;
					}
					return -1;
				}
				else
				{
					int availableBits2 = input.AvailableBits;
					num = input.PeekBits(availableBits2);
					int num2 = (int)this.tree[num];
					if (num2 >= 0 && (num2 & 15) <= availableBits2)
					{
						input.DropBits(num2 & 15);
						return num2 >> 4;
					}
					return -1;
				}
			}

			// Token: 0x040000CB RID: 203
			private const int MAX_BITLEN = 15;

			// Token: 0x040000CC RID: 204
			private short[] tree;

			// Token: 0x040000CD RID: 205
			public static readonly SimpleZip.InflaterHuffmanTree defLitLenTree;

			// Token: 0x040000CE RID: 206
			public static readonly SimpleZip.InflaterHuffmanTree defDistTree;
		}

		// Token: 0x0200003B RID: 59
		internal sealed class InflaterDynHeader
		{
			// Token: 0x060000DB RID: 219 RVA: 0x000304C4 File Offset: 0x0002E6C4
			public bool Decode(SimpleZip.StreamManipulator input)
			{
				for (;;)
				{
					switch (this.mode)
					{
					case 0:
						this.lnum = input.PeekBits(5);
						if (this.lnum >= 0)
						{
							this.lnum += 257;
							input.DropBits(5);
							this.mode = 1;
							goto IL_01E0;
						}
						return false;
					case 1:
						goto IL_01E0;
					case 2:
						goto IL_0192;
					case 3:
						goto IL_0159;
					case 4:
						break;
					case 5:
						goto IL_002A;
					default:
						continue;
					}
					IL_00E4:
					int symbol;
					while (((symbol = this.blTree.GetSymbol(input)) & -16) == 0)
					{
						byte[] array = this.litdistLens;
						int num = this.ptr;
						this.ptr = num + 1;
						array[num] = (this.lastLen = (byte)symbol);
						if (this.ptr == this.num)
						{
							return true;
						}
					}
					if (symbol >= 0)
					{
						if (symbol >= 17)
						{
							this.lastLen = 0;
						}
						this.repSymbol = symbol - 16;
						this.mode = 5;
						goto IL_002A;
					}
					return false;
					IL_0159:
					while (this.ptr < this.blnum)
					{
						int num2 = input.PeekBits(3);
						if (num2 < 0)
						{
							return false;
						}
						input.DropBits(3);
						this.blLens[SimpleZip.InflaterDynHeader.BL_ORDER[this.ptr]] = (byte)num2;
						this.ptr++;
					}
					this.blTree = new SimpleZip.InflaterHuffmanTree(this.blLens);
					this.blLens = null;
					this.ptr = 0;
					this.mode = 4;
					goto IL_00E4;
					IL_002A:
					int num3 = SimpleZip.InflaterDynHeader.repBits[this.repSymbol];
					int num4 = input.PeekBits(num3);
					if (num4 < 0)
					{
						return false;
					}
					input.DropBits(num3);
					num4 += SimpleZip.InflaterDynHeader.repMin[this.repSymbol];
					while (num4-- > 0)
					{
						byte[] array2 = this.litdistLens;
						int num = this.ptr;
						this.ptr = num + 1;
						array2[num] = this.lastLen;
					}
					if (this.ptr == this.num)
					{
						break;
					}
					this.mode = 4;
					continue;
					IL_0192:
					this.blnum = input.PeekBits(4);
					if (this.blnum >= 0)
					{
						this.blnum += 4;
						input.DropBits(4);
						this.blLens = new byte[19];
						this.ptr = 0;
						this.mode = 3;
						goto IL_0159;
					}
					return false;
					IL_01E0:
					this.dnum = input.PeekBits(5);
					if (this.dnum >= 0)
					{
						this.dnum++;
						input.DropBits(5);
						this.num = this.lnum + this.dnum;
						this.litdistLens = new byte[this.num];
						this.mode = 2;
						goto IL_0192;
					}
					return false;
				}
				return true;
			}

			// Token: 0x060000DC RID: 220 RVA: 0x00030760 File Offset: 0x0002E960
			public SimpleZip.InflaterHuffmanTree BuildLitLenTree()
			{
				byte[] array = new byte[this.lnum];
				Array.Copy(this.litdistLens, 0, array, 0, this.lnum);
				return new SimpleZip.InflaterHuffmanTree(array);
			}

			// Token: 0x060000DD RID: 221 RVA: 0x00030794 File Offset: 0x0002E994
			public SimpleZip.InflaterHuffmanTree BuildDistTree()
			{
				byte[] array = new byte[this.dnum];
				Array.Copy(this.litdistLens, this.lnum, array, 0, this.dnum);
				return new SimpleZip.InflaterHuffmanTree(array);
			}

			// Token: 0x040000CF RID: 207
			private const int LNUM = 0;

			// Token: 0x040000D0 RID: 208
			private const int DNUM = 1;

			// Token: 0x040000D1 RID: 209
			private const int BLNUM = 2;

			// Token: 0x040000D2 RID: 210
			private const int BLLENS = 3;

			// Token: 0x040000D3 RID: 211
			private const int LENS = 4;

			// Token: 0x040000D4 RID: 212
			private const int REPS = 5;

			// Token: 0x040000D5 RID: 213
			private static readonly int[] repMin = new int[] { 3, 3, 11 };

			// Token: 0x040000D6 RID: 214
			private static readonly int[] repBits = new int[] { 2, 3, 7 };

			// Token: 0x040000D7 RID: 215
			private byte[] blLens;

			// Token: 0x040000D8 RID: 216
			private byte[] litdistLens;

			// Token: 0x040000D9 RID: 217
			private SimpleZip.InflaterHuffmanTree blTree;

			// Token: 0x040000DA RID: 218
			private int mode;

			// Token: 0x040000DB RID: 219
			private int lnum;

			// Token: 0x040000DC RID: 220
			private int dnum;

			// Token: 0x040000DD RID: 221
			private int blnum;

			// Token: 0x040000DE RID: 222
			private int num;

			// Token: 0x040000DF RID: 223
			private int repSymbol;

			// Token: 0x040000E0 RID: 224
			private byte lastLen;

			// Token: 0x040000E1 RID: 225
			private int ptr;

			// Token: 0x040000E2 RID: 226
			private static readonly int[] BL_ORDER = new int[]
			{
				16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
				11, 4, 12, 3, 13, 2, 14, 1, 15
			};
		}

		// Token: 0x0200003C RID: 60
		internal sealed class Deflater
		{
			// Token: 0x060000E0 RID: 224 RVA: 0x0002AE50 File Offset: 0x00029050
			public Deflater()
			{
				this.pending = new SimpleZip.DeflaterPending();
				this.engine = new SimpleZip.DeflaterEngine(this.pending);
			}

			// Token: 0x17000024 RID: 36
			// (get) Token: 0x060000E1 RID: 225 RVA: 0x0002AE7C File Offset: 0x0002907C
			public long TotalOut
			{
				get
				{
					return this.totalOut;
				}
			}

			// Token: 0x060000E2 RID: 226 RVA: 0x0002AE84 File Offset: 0x00029084
			public void Finish()
			{
				this.state |= 12;
			}

			// Token: 0x17000025 RID: 37
			// (get) Token: 0x060000E3 RID: 227 RVA: 0x0002AE95 File Offset: 0x00029095
			public bool IsFinished
			{
				get
				{
					return this.state == 30 && this.pending.IsFlushed;
				}
			}

			// Token: 0x17000026 RID: 38
			// (get) Token: 0x060000E4 RID: 228 RVA: 0x0002AEAE File Offset: 0x000290AE
			public bool IsNeedingInput
			{
				get
				{
					return this.engine.NeedsInput();
				}
			}

			// Token: 0x060000E5 RID: 229 RVA: 0x0002AEBB File Offset: 0x000290BB
			public void SetInput(byte[] buffer)
			{
				this.engine.SetInput(buffer);
			}

			// Token: 0x060000E6 RID: 230 RVA: 0x0003081C File Offset: 0x0002EA1C
			public int Deflate(byte[] output)
			{
				int num = 0;
				int num2 = output.Length;
				int num3 = num2;
				for (;;)
				{
					int num4 = this.pending.Flush(output, num, num2);
					num += num4;
					this.totalOut += (long)num4;
					num2 -= num4;
					if (num2 == 0 || this.state == 30)
					{
						goto IL_00DD;
					}
					if (!this.engine.Deflate((this.state & 4) != 0, (this.state & 8) != 0))
					{
						if (this.state == 16)
						{
							break;
						}
						if (this.state == 20)
						{
							for (int i = 8 + (-this.pending.BitCount & 7); i > 0; i -= 10)
							{
								this.pending.WriteBits(2, 10);
							}
							this.state = 16;
						}
						else if (this.state == 28)
						{
							this.pending.AlignToByte();
							this.state = 30;
						}
					}
				}
				return num3 - num2;
				IL_00DD:
				return num3 - num2;
			}

			// Token: 0x040000E3 RID: 227
			private const int IS_FLUSHING = 4;

			// Token: 0x040000E4 RID: 228
			private const int IS_FINISHING = 8;

			// Token: 0x040000E5 RID: 229
			private const int BUSY_STATE = 16;

			// Token: 0x040000E6 RID: 230
			private const int FLUSHING_STATE = 20;

			// Token: 0x040000E7 RID: 231
			private const int FINISHING_STATE = 28;

			// Token: 0x040000E8 RID: 232
			private const int FINISHED_STATE = 30;

			// Token: 0x040000E9 RID: 233
			private int state = 16;

			// Token: 0x040000EA RID: 234
			private long totalOut;

			// Token: 0x040000EB RID: 235
			private SimpleZip.DeflaterPending pending;

			// Token: 0x040000EC RID: 236
			private SimpleZip.DeflaterEngine engine;
		}

		// Token: 0x0200003D RID: 61
		internal sealed class DeflaterHuffman
		{
			// Token: 0x060000E7 RID: 231 RVA: 0x0002AEC9 File Offset: 0x000290C9
			public static short BitReverse(int toReverse)
			{
				return (short)(((int)SimpleZip.DeflaterHuffman.bit4Reverse[toReverse & 15] << 12) | ((int)SimpleZip.DeflaterHuffman.bit4Reverse[(toReverse >> 4) & 15] << 8) | ((int)SimpleZip.DeflaterHuffman.bit4Reverse[(toReverse >> 8) & 15] << 4) | (int)SimpleZip.DeflaterHuffman.bit4Reverse[toReverse >> 12]);
			}

			// Token: 0x060000E8 RID: 232 RVA: 0x0003090C File Offset: 0x0002EB0C
			static DeflaterHuffman()
			{
				int i = 0;
				while (i < 144)
				{
					SimpleZip.DeflaterHuffman.staticLCodes[i] = SimpleZip.DeflaterHuffman.BitReverse(48 + i << 8);
					SimpleZip.DeflaterHuffman.staticLLength[i++] = 8;
				}
				while (i < 256)
				{
					SimpleZip.DeflaterHuffman.staticLCodes[i] = SimpleZip.DeflaterHuffman.BitReverse(256 + i << 7);
					SimpleZip.DeflaterHuffman.staticLLength[i++] = 9;
				}
				while (i < 280)
				{
					SimpleZip.DeflaterHuffman.staticLCodes[i] = SimpleZip.DeflaterHuffman.BitReverse(-256 + i << 9);
					SimpleZip.DeflaterHuffman.staticLLength[i++] = 7;
				}
				while (i < 286)
				{
					SimpleZip.DeflaterHuffman.staticLCodes[i] = SimpleZip.DeflaterHuffman.BitReverse(-88 + i << 8);
					SimpleZip.DeflaterHuffman.staticLLength[i++] = 8;
				}
				SimpleZip.DeflaterHuffman.staticDCodes = new short[30];
				SimpleZip.DeflaterHuffman.staticDLength = new byte[30];
				for (i = 0; i < 30; i++)
				{
					SimpleZip.DeflaterHuffman.staticDCodes[i] = SimpleZip.DeflaterHuffman.BitReverse(i << 11);
					SimpleZip.DeflaterHuffman.staticDLength[i] = 5;
				}
			}

			// Token: 0x060000E9 RID: 233 RVA: 0x00030A4C File Offset: 0x0002EC4C
			public DeflaterHuffman(SimpleZip.DeflaterPending pending)
			{
				this.pending = pending;
				this.literalTree = new SimpleZip.DeflaterHuffman.Tree(this, 286, 257, 15);
				this.distTree = new SimpleZip.DeflaterHuffman.Tree(this, 30, 1, 15);
				this.blTree = new SimpleZip.DeflaterHuffman.Tree(this, 19, 4, 7);
				this.d_buf = new short[16384];
				this.l_buf = new byte[16384];
			}

			// Token: 0x060000EA RID: 234 RVA: 0x0002AF02 File Offset: 0x00029102
			public void Init()
			{
				this.last_lit = 0;
				this.extra_bits = 0;
			}

			// Token: 0x060000EB RID: 235 RVA: 0x00030AC0 File Offset: 0x0002ECC0
			private int Lcode(int len)
			{
				if (len == 255)
				{
					return 285;
				}
				int num = 257;
				while (len >= 8)
				{
					num += 4;
					len >>= 1;
				}
				return num + len;
			}

			// Token: 0x060000EC RID: 236 RVA: 0x00030AF4 File Offset: 0x0002ECF4
			private int Dcode(int distance)
			{
				int num = 0;
				while (distance >= 4)
				{
					num += 2;
					distance >>= 1;
				}
				return num + distance;
			}

			// Token: 0x060000ED RID: 237 RVA: 0x00030B18 File Offset: 0x0002ED18
			public void SendAllTrees(int blTreeCodes)
			{
				this.blTree.BuildCodes();
				this.literalTree.BuildCodes();
				this.distTree.BuildCodes();
				this.pending.WriteBits(this.literalTree.numCodes - 257, 5);
				this.pending.WriteBits(this.distTree.numCodes - 1, 5);
				this.pending.WriteBits(blTreeCodes - 4, 4);
				for (int i = 0; i < blTreeCodes; i++)
				{
					this.pending.WriteBits((int)this.blTree.length[SimpleZip.DeflaterHuffman.BL_ORDER[i]], 3);
				}
				this.literalTree.WriteTree(this.blTree);
				this.distTree.WriteTree(this.blTree);
			}

			// Token: 0x060000EE RID: 238 RVA: 0x00030BD8 File Offset: 0x0002EDD8
			public void CompressBlock()
			{
				for (int i = 0; i < this.last_lit; i++)
				{
					int num = (int)(this.l_buf[i] & byte.MaxValue);
					int num2 = (int)this.d_buf[i];
					if (num2-- != 0)
					{
						int num3 = this.Lcode(num);
						this.literalTree.WriteSymbol(num3);
						int num4 = (num3 - 261) / 4;
						if (num4 > 0 && num4 <= 5)
						{
							this.pending.WriteBits(num & ((1 << num4) - 1), num4);
						}
						int num5 = this.Dcode(num2);
						this.distTree.WriteSymbol(num5);
						num4 = num5 / 2 - 1;
						if (num4 > 0)
						{
							this.pending.WriteBits(num2 & ((1 << num4) - 1), num4);
						}
					}
					else
					{
						this.literalTree.WriteSymbol(num);
					}
				}
				this.literalTree.WriteSymbol(256);
			}

			// Token: 0x060000EF RID: 239 RVA: 0x00030CB8 File Offset: 0x0002EEB8
			public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
			{
				this.pending.WriteBits((lastBlock > false) ? 1 : 0, 3);
				this.pending.AlignToByte();
				this.pending.WriteShort(storedLength);
				this.pending.WriteShort(~storedLength);
				this.pending.WriteBlock(stored, storedOffset, storedLength);
				this.Init();
			}

			// Token: 0x060000F0 RID: 240 RVA: 0x00030D10 File Offset: 0x0002EF10
			public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
			{
				short[] freqs = this.literalTree.freqs;
				int num = 256;
				freqs[num] += 1;
				this.literalTree.BuildTree();
				this.distTree.BuildTree();
				this.literalTree.CalcBLFreq(this.blTree);
				this.distTree.CalcBLFreq(this.blTree);
				this.blTree.BuildTree();
				int num2 = 4;
				for (int i = 18; i > num2; i--)
				{
					if (this.blTree.length[SimpleZip.DeflaterHuffman.BL_ORDER[i]] > 0)
					{
						num2 = i + 1;
					}
				}
				int num3 = 14 + num2 * 3 + this.blTree.GetEncodedLength() + this.literalTree.GetEncodedLength() + this.distTree.GetEncodedLength() + this.extra_bits;
				int num4 = this.extra_bits;
				for (int j = 0; j < 286; j++)
				{
					num4 += (int)(this.literalTree.freqs[j] * (short)SimpleZip.DeflaterHuffman.staticLLength[j]);
				}
				for (int k = 0; k < 30; k++)
				{
					num4 += (int)(this.distTree.freqs[k] * (short)SimpleZip.DeflaterHuffman.staticDLength[k]);
				}
				if (num3 >= num4)
				{
					num3 = num4;
				}
				if (storedOffset >= 0 && storedLength + 4 < num3 >> 3)
				{
					this.FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
					return;
				}
				if (num3 == num4)
				{
					this.pending.WriteBits(2 + ((lastBlock > false) ? 1 : 0), 3);
					this.literalTree.SetStaticCodes(SimpleZip.DeflaterHuffman.staticLCodes, SimpleZip.DeflaterHuffman.staticLLength);
					this.distTree.SetStaticCodes(SimpleZip.DeflaterHuffman.staticDCodes, SimpleZip.DeflaterHuffman.staticDLength);
					this.CompressBlock();
					this.Init();
					return;
				}
				this.pending.WriteBits(4 + ((lastBlock > false) ? 1 : 0), 3);
				this.SendAllTrees(num2);
				this.CompressBlock();
				this.Init();
			}

			// Token: 0x060000F1 RID: 241 RVA: 0x0002AF12 File Offset: 0x00029112
			public bool IsFull()
			{
				return this.last_lit >= 16384;
			}

			// Token: 0x060000F2 RID: 242 RVA: 0x00030EC8 File Offset: 0x0002F0C8
			public bool TallyLit(int lit)
			{
				this.d_buf[this.last_lit] = 0;
				byte[] array = this.l_buf;
				int num = this.last_lit;
				this.last_lit = num + 1;
				array[num] = (byte)lit;
				short[] freqs = this.literalTree.freqs;
				freqs[lit] += 1;
				return this.IsFull();
			}

			// Token: 0x060000F3 RID: 243 RVA: 0x00030F1C File Offset: 0x0002F11C
			public bool TallyDist(int dist, int len)
			{
				this.d_buf[this.last_lit] = (short)dist;
				byte[] array = this.l_buf;
				int num = this.last_lit;
				this.last_lit = num + 1;
				array[num] = (byte)(len - 3);
				int num2 = this.Lcode(len - 3);
				short[] freqs = this.literalTree.freqs;
				int num3 = num2;
				freqs[num3] += 1;
				if (num2 >= 265 && num2 < 285)
				{
					this.extra_bits += (num2 - 261) / 4;
				}
				int num4 = this.Dcode(dist - 1);
				short[] freqs2 = this.distTree.freqs;
				int num5 = num4;
				freqs2[num5] += 1;
				if (num4 >= 4)
				{
					this.extra_bits += num4 / 2 - 1;
				}
				return this.IsFull();
			}

			// Token: 0x040000ED RID: 237
			private const int BUFSIZE = 16384;

			// Token: 0x040000EE RID: 238
			private const int LITERAL_NUM = 286;

			// Token: 0x040000EF RID: 239
			private const int DIST_NUM = 30;

			// Token: 0x040000F0 RID: 240
			private const int BITLEN_NUM = 19;

			// Token: 0x040000F1 RID: 241
			private const int REP_3_6 = 16;

			// Token: 0x040000F2 RID: 242
			private const int REP_3_10 = 17;

			// Token: 0x040000F3 RID: 243
			private const int REP_11_138 = 18;

			// Token: 0x040000F4 RID: 244
			private const int EOF_SYMBOL = 256;

			// Token: 0x040000F5 RID: 245
			private static readonly int[] BL_ORDER = new int[]
			{
				16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
				11, 4, 12, 3, 13, 2, 14, 1, 15
			};

			// Token: 0x040000F6 RID: 246
			private static readonly byte[] bit4Reverse = new byte[]
			{
				0, 8, 4, 12, 2, 10, 6, 14, 1, 9,
				5, 13, 3, 11, 7, 15
			};

			// Token: 0x040000F7 RID: 247
			private SimpleZip.DeflaterPending pending;

			// Token: 0x040000F8 RID: 248
			private SimpleZip.DeflaterHuffman.Tree literalTree;

			// Token: 0x040000F9 RID: 249
			private SimpleZip.DeflaterHuffman.Tree distTree;

			// Token: 0x040000FA RID: 250
			private SimpleZip.DeflaterHuffman.Tree blTree;

			// Token: 0x040000FB RID: 251
			private short[] d_buf;

			// Token: 0x040000FC RID: 252
			private byte[] l_buf;

			// Token: 0x040000FD RID: 253
			private int last_lit;

			// Token: 0x040000FE RID: 254
			private int extra_bits;

			// Token: 0x040000FF RID: 255
			private static readonly short[] staticLCodes = new short[286];

			// Token: 0x04000100 RID: 256
			private static readonly byte[] staticLLength = new byte[286];

			// Token: 0x04000101 RID: 257
			private static readonly short[] staticDCodes;

			// Token: 0x04000102 RID: 258
			private static readonly byte[] staticDLength;

			// Token: 0x0200003E RID: 62
			public sealed class Tree
			{
				// Token: 0x060000F4 RID: 244 RVA: 0x0002AF24 File Offset: 0x00029124
				public Tree(SimpleZip.DeflaterHuffman dh, int elems, int minCodes, int maxLength)
				{
					this.dh = dh;
					this.minNumCodes = minCodes;
					this.maxLength = maxLength;
					this.freqs = new short[elems];
					this.bl_counts = new int[maxLength];
				}

				// Token: 0x060000F5 RID: 245 RVA: 0x0002AF5B File Offset: 0x0002915B
				public void WriteSymbol(int code)
				{
					this.dh.pending.WriteBits((int)this.codes[code] & 65535, (int)this.length[code]);
				}

				// Token: 0x060000F6 RID: 246 RVA: 0x0002AF83 File Offset: 0x00029183
				public void SetStaticCodes(short[] stCodes, byte[] stLength)
				{
					this.codes = stCodes;
					this.length = stLength;
				}

				// Token: 0x060000F7 RID: 247 RVA: 0x00030FD8 File Offset: 0x0002F1D8
				public void BuildCodes()
				{
					int[] array = new int[this.maxLength];
					int num = 0;
					this.codes = new short[this.freqs.Length];
					for (int i = 0; i < this.maxLength; i++)
					{
						array[i] = num;
						num += this.bl_counts[i] << 15 - i;
					}
					for (int j = 0; j < this.numCodes; j++)
					{
						int num2 = (int)this.length[j];
						if (num2 > 0)
						{
							this.codes[j] = SimpleZip.DeflaterHuffman.BitReverse(array[num2 - 1]);
							array[num2 - 1] += 1 << 16 - num2;
						}
					}
				}

				// Token: 0x060000F8 RID: 248 RVA: 0x0003107C File Offset: 0x0002F27C
				private void BuildLength(int[] childs)
				{
					this.length = new byte[this.freqs.Length];
					int num = childs.Length / 2;
					int num2 = (num + 1) / 2;
					int num3 = 0;
					for (int i = 0; i < this.maxLength; i++)
					{
						this.bl_counts[i] = 0;
					}
					int[] array = new int[num];
					array[num - 1] = 0;
					for (int j = num - 1; j >= 0; j--)
					{
						if (childs[2 * j + 1] != -1)
						{
							int num4 = array[j] + 1;
							if (num4 > this.maxLength)
							{
								num4 = this.maxLength;
								num3++;
							}
							array[childs[2 * j]] = (array[childs[2 * j + 1]] = num4);
						}
						else
						{
							int num5 = array[j];
							this.bl_counts[num5 - 1]++;
							this.length[childs[2 * j]] = (byte)array[j];
						}
					}
					if (num3 == 0)
					{
						return;
					}
					int num6 = this.maxLength - 1;
					for (;;)
					{
						if (this.bl_counts[--num6] != 0)
						{
							do
							{
								this.bl_counts[num6]--;
								this.bl_counts[++num6]++;
								num3 -= 1 << this.maxLength - 1 - num6;
							}
							while (num3 > 0 && num6 < this.maxLength - 1);
							if (num3 <= 0)
							{
								break;
							}
						}
					}
					this.bl_counts[this.maxLength - 1] += num3;
					this.bl_counts[this.maxLength - 2] -= num3;
					int num7 = 2 * num2;
					for (int num8 = this.maxLength; num8 != 0; num8--)
					{
						int k = this.bl_counts[num8 - 1];
						while (k > 0)
						{
							int num9 = 2 * childs[num7++];
							if (childs[num9 + 1] == -1)
							{
								this.length[childs[num9]] = (byte)num8;
								k--;
							}
						}
					}
				}

				// Token: 0x060000F9 RID: 249 RVA: 0x00031254 File Offset: 0x0002F454
				public void BuildTree()
				{
					int num = this.freqs.Length;
					int[] array = new int[num];
					int i = 0;
					int num2 = 0;
					for (int j = 0; j < num; j++)
					{
						int num3 = (int)this.freqs[j];
						if (num3 != 0)
						{
							int num4 = i++;
							int num5;
							while (num4 > 0 && (int)this.freqs[array[num5 = (num4 - 1) / 2]] > num3)
							{
								array[num4] = array[num5];
								num4 = num5;
							}
							array[num4] = j;
							num2 = j;
						}
					}
					while (i < 2)
					{
						int num6 = ((num2 < 2) ? (++num2) : 0);
						array[i++] = num6;
					}
					this.numCodes = Math.Max(num2 + 1, this.minNumCodes);
					int num7 = i;
					int[] array2 = new int[4 * i - 2];
					int[] array3 = new int[2 * i - 1];
					int num8 = num7;
					for (int k = 0; k < i; k++)
					{
						int num9 = array[k];
						array2[2 * k] = num9;
						array2[2 * k + 1] = -1;
						array3[k] = (int)this.freqs[num9] << 8;
						array[k] = k;
					}
					do
					{
						int num10 = array[0];
						int num11 = array[--i];
						int num12 = 0;
						int l;
						for (l = 1; l < i; l = l * 2 + 1)
						{
							if (l + 1 < i && array3[array[l]] > array3[array[l + 1]])
							{
								l++;
							}
							array[num12] = array[l];
							num12 = l;
						}
						int num13 = array3[num11];
						while ((l = num12) > 0 && array3[array[num12 = (l - 1) / 2]] > num13)
						{
							array[l] = array[num12];
						}
						array[l] = num11;
						int num14 = array[0];
						num11 = num8++;
						array2[2 * num11] = num10;
						array2[2 * num11 + 1] = num14;
						int num15 = Math.Min(array3[num10] & 255, array3[num14] & 255);
						num13 = (array3[num11] = array3[num10] + array3[num14] - num15 + 1);
						num12 = 0;
						for (l = 1; l < i; l = num12 * 2 + 1)
						{
							if (l + 1 < i && array3[array[l]] > array3[array[l + 1]])
							{
								l++;
							}
							array[num12] = array[l];
							num12 = l;
						}
						while ((l = num12) > 0 && array3[array[num12 = (l - 1) / 2]] > num13)
						{
							array[l] = array[num12];
						}
						array[l] = num11;
					}
					while (i > 1);
					this.BuildLength(array2);
				}

				// Token: 0x060000FA RID: 250 RVA: 0x000314AC File Offset: 0x0002F6AC
				public int GetEncodedLength()
				{
					int num = 0;
					for (int i = 0; i < this.freqs.Length; i++)
					{
						num += (int)(this.freqs[i] * (short)this.length[i]);
					}
					return num;
				}

				// Token: 0x060000FB RID: 251 RVA: 0x000314E4 File Offset: 0x0002F6E4
				public void CalcBLFreq(SimpleZip.DeflaterHuffman.Tree blTree)
				{
					int num = -1;
					int i = 0;
					while (i < this.numCodes)
					{
						int num2 = 1;
						int num3 = (int)this.length[i];
						int num4;
						int num5;
						if (num3 == 0)
						{
							num4 = 138;
							num5 = 3;
						}
						else
						{
							num4 = 6;
							num5 = 3;
							if (num != num3)
							{
								short[] array = blTree.freqs;
								int num6 = num3;
								array[num6] += 1;
								num2 = 0;
							}
						}
						num = num3;
						i++;
						while (i < this.numCodes)
						{
							if (num != (int)this.length[i])
							{
								break;
							}
							i++;
							if (++num2 >= num4)
							{
								break;
							}
						}
						if (num2 < num5)
						{
							short[] array2 = blTree.freqs;
							int num7 = num;
							array2[num7] += (short)num2;
						}
						else if (num != 0)
						{
							short[] array3 = blTree.freqs;
							int num8 = 16;
							array3[num8] += 1;
						}
						else if (num2 <= 10)
						{
							short[] array4 = blTree.freqs;
							int num9 = 17;
							array4[num9] += 1;
						}
						else
						{
							short[] array5 = blTree.freqs;
							int num10 = 18;
							array5[num10] += 1;
						}
					}
				}

				// Token: 0x060000FC RID: 252 RVA: 0x000315D0 File Offset: 0x0002F7D0
				public void WriteTree(SimpleZip.DeflaterHuffman.Tree blTree)
				{
					int num = -1;
					int i = 0;
					while (i < this.numCodes)
					{
						int num2 = 1;
						int num3 = (int)this.length[i];
						int num4;
						int num5;
						if (num3 == 0)
						{
							num4 = 138;
							num5 = 3;
						}
						else
						{
							num4 = 6;
							num5 = 3;
							if (num != num3)
							{
								blTree.WriteSymbol(num3);
								num2 = 0;
							}
						}
						num = num3;
						i++;
						while (i < this.numCodes)
						{
							if (num != (int)this.length[i])
							{
								break;
							}
							i++;
							if (++num2 >= num4)
							{
								break;
							}
						}
						if (num2 < num5)
						{
							while (num2-- > 0)
							{
								blTree.WriteSymbol(num);
							}
						}
						else if (num != 0)
						{
							blTree.WriteSymbol(16);
							this.dh.pending.WriteBits(num2 - 3, 2);
						}
						else if (num2 <= 10)
						{
							blTree.WriteSymbol(17);
							this.dh.pending.WriteBits(num2 - 3, 3);
						}
						else
						{
							blTree.WriteSymbol(18);
							this.dh.pending.WriteBits(num2 - 11, 7);
						}
					}
				}

				// Token: 0x04000103 RID: 259
				public short[] freqs;

				// Token: 0x04000104 RID: 260
				public byte[] length;

				// Token: 0x04000105 RID: 261
				public int minNumCodes;

				// Token: 0x04000106 RID: 262
				public int numCodes;

				// Token: 0x04000107 RID: 263
				private short[] codes;

				// Token: 0x04000108 RID: 264
				private int[] bl_counts;

				// Token: 0x04000109 RID: 265
				private int maxLength;

				// Token: 0x0400010A RID: 266
				private SimpleZip.DeflaterHuffman dh;
			}
		}

		// Token: 0x0200003F RID: 63
		internal sealed class DeflaterEngine
		{
			// Token: 0x060000FD RID: 253 RVA: 0x000316CC File Offset: 0x0002F8CC
			public DeflaterEngine(SimpleZip.DeflaterPending pending)
			{
				this.pending = pending;
				this.huffman = new SimpleZip.DeflaterHuffman(pending);
				this.window = new byte[65536];
				this.head = new short[32768];
				this.prev = new short[32768];
				this.strstart = 1;
				this.blockStart = 1;
			}

			// Token: 0x060000FE RID: 254 RVA: 0x0002AF93 File Offset: 0x00029193
			private void UpdateHash()
			{
				this.ins_h = ((int)this.window[this.strstart] << 5) ^ (int)this.window[this.strstart + 1];
			}

			// Token: 0x060000FF RID: 255 RVA: 0x00031730 File Offset: 0x0002F930
			private int InsertString()
			{
				int num = ((this.ins_h << 5) ^ (int)this.window[this.strstart + 2]) & 32767;
				short num2 = (this.prev[this.strstart & 32767] = this.head[num]);
				this.head[num] = (short)this.strstart;
				this.ins_h = num;
				return (int)num2 & 65535;
			}

			// Token: 0x06000100 RID: 256 RVA: 0x00031798 File Offset: 0x0002F998
			private void SlideWindow()
			{
				Array.Copy(this.window, 32768, this.window, 0, 32768);
				this.matchStart -= 32768;
				this.strstart -= 32768;
				this.blockStart -= 32768;
				for (int i = 0; i < 32768; i++)
				{
					int num = (int)this.head[i] & 65535;
					this.head[i] = (short)((num >= 32768) ? (num - 32768) : 0);
				}
				for (int j = 0; j < 32768; j++)
				{
					int num2 = (int)this.prev[j] & 65535;
					this.prev[j] = (short)((num2 >= 32768) ? (num2 - 32768) : 0);
				}
			}

			// Token: 0x06000101 RID: 257 RVA: 0x0003186C File Offset: 0x0002FA6C
			public void FillWindow()
			{
				if (this.strstart >= 65274)
				{
					this.SlideWindow();
				}
				while (this.lookahead < 262 && this.inputOff < this.inputEnd)
				{
					int num = 65536 - this.lookahead - this.strstart;
					if (num > this.inputEnd - this.inputOff)
					{
						num = this.inputEnd - this.inputOff;
					}
					Array.Copy(this.inputBuf, this.inputOff, this.window, this.strstart + this.lookahead, num);
					this.inputOff += num;
					this.totalIn += num;
					this.lookahead += num;
				}
				if (this.lookahead >= 3)
				{
					this.UpdateHash();
				}
			}

			// Token: 0x06000102 RID: 258 RVA: 0x00031948 File Offset: 0x0002FB48
			private bool FindLongestMatch(int curMatch)
			{
				int num = 128;
				int num2 = 128;
				short[] array = this.prev;
				int num3 = this.strstart;
				int num4 = this.strstart + this.matchLen;
				int num5 = Math.Max(this.matchLen, 2);
				int num6 = Math.Max(this.strstart - 32506, 0);
				int num7 = this.strstart + 258 - 1;
				byte b = this.window[num4 - 1];
				byte b2 = this.window[num4];
				if (num5 >= 8)
				{
					num >>= 2;
				}
				if (num2 > this.lookahead)
				{
					num2 = this.lookahead;
				}
				do
				{
					if (this.window[curMatch + num5] == b2 && this.window[curMatch + num5 - 1] == b && this.window[curMatch] == this.window[num3] && this.window[curMatch + 1] == this.window[num3 + 1])
					{
						int num8 = curMatch + 2;
						num3 += 2;
						while (this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && num3 < num7)
						{
						}
						if (num3 > num4)
						{
							this.matchStart = curMatch;
							num4 = num3;
							num5 = num3 - this.strstart;
							if (num5 >= num2)
							{
								break;
							}
							b = this.window[num4 - 1];
							b2 = this.window[num4];
						}
						num3 = this.strstart;
					}
					if ((curMatch = (int)array[curMatch & 32767] & 65535) <= num6)
					{
						break;
					}
				}
				while (--num != 0);
				this.matchLen = Math.Min(num5, this.lookahead);
				return this.matchLen >= 3;
			}

			// Token: 0x06000103 RID: 259 RVA: 0x00031BC4 File Offset: 0x0002FDC4
			private bool DeflateSlow(bool flush, bool finish)
			{
				if (this.lookahead < 262 && !flush)
				{
					return false;
				}
				while (this.lookahead >= 262 || flush)
				{
					if (this.lookahead == 0)
					{
						if (this.prevAvailable)
						{
							this.huffman.TallyLit((int)(this.window[this.strstart - 1] & byte.MaxValue));
						}
						this.prevAvailable = false;
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, finish);
						this.blockStart = this.strstart;
						return false;
					}
					if (this.strstart >= 65274)
					{
						this.SlideWindow();
					}
					int num = this.matchStart;
					int num2 = this.matchLen;
					if (this.lookahead >= 3)
					{
						int num3 = this.InsertString();
						if (num3 != 0 && this.strstart - num3 <= 32506 && this.FindLongestMatch(num3) && this.matchLen <= 5 && this.matchLen == 3 && this.strstart - this.matchStart > 4096)
						{
							this.matchLen = 2;
						}
					}
					if (num2 >= 3 && this.matchLen <= num2)
					{
						this.huffman.TallyDist(this.strstart - 1 - num, num2);
						num2 -= 2;
						do
						{
							this.strstart++;
							this.lookahead--;
							if (this.lookahead >= 3)
							{
								this.InsertString();
							}
						}
						while (--num2 > 0);
						this.strstart++;
						this.lookahead--;
						this.prevAvailable = false;
						this.matchLen = 2;
					}
					else
					{
						if (this.prevAvailable)
						{
							this.huffman.TallyLit((int)(this.window[this.strstart - 1] & byte.MaxValue));
						}
						this.prevAvailable = true;
						this.strstart++;
						this.lookahead--;
					}
					if (this.huffman.IsFull())
					{
						int num4 = this.strstart - this.blockStart;
						if (this.prevAvailable)
						{
							num4--;
						}
						bool flag = finish && this.lookahead == 0 && !this.prevAvailable;
						this.huffman.FlushBlock(this.window, this.blockStart, num4, flag);
						this.blockStart += num4;
						return !flag;
					}
				}
				return true;
			}

			// Token: 0x06000104 RID: 260 RVA: 0x00031E38 File Offset: 0x00030038
			public bool Deflate(bool flush, bool finish)
			{
				bool flag2;
				do
				{
					this.FillWindow();
					bool flag = flush && this.inputOff == this.inputEnd;
					flag2 = this.DeflateSlow(flag, finish);
				}
				while (this.pending.IsFlushed && flag2);
				return flag2;
			}

			// Token: 0x06000105 RID: 261 RVA: 0x0002AFBA File Offset: 0x000291BA
			public void SetInput(byte[] buffer)
			{
				this.inputBuf = buffer;
				this.inputOff = 0;
				this.inputEnd = buffer.Length;
			}

			// Token: 0x06000106 RID: 262 RVA: 0x0002AFD3 File Offset: 0x000291D3
			public bool NeedsInput()
			{
				return this.inputEnd == this.inputOff;
			}

			// Token: 0x0400010B RID: 267
			private const int MAX_MATCH = 258;

			// Token: 0x0400010C RID: 268
			private const int MIN_MATCH = 3;

			// Token: 0x0400010D RID: 269
			private const int WSIZE = 32768;

			// Token: 0x0400010E RID: 270
			private const int WMASK = 32767;

			// Token: 0x0400010F RID: 271
			private const int HASH_SIZE = 32768;

			// Token: 0x04000110 RID: 272
			private const int HASH_MASK = 32767;

			// Token: 0x04000111 RID: 273
			private const int HASH_SHIFT = 5;

			// Token: 0x04000112 RID: 274
			private const int MIN_LOOKAHEAD = 262;

			// Token: 0x04000113 RID: 275
			private const int MAX_DIST = 32506;

			// Token: 0x04000114 RID: 276
			private const int TOO_FAR = 4096;

			// Token: 0x04000115 RID: 277
			private int ins_h;

			// Token: 0x04000116 RID: 278
			private short[] head;

			// Token: 0x04000117 RID: 279
			private short[] prev;

			// Token: 0x04000118 RID: 280
			private int matchStart;

			// Token: 0x04000119 RID: 281
			private int matchLen;

			// Token: 0x0400011A RID: 282
			private bool prevAvailable;

			// Token: 0x0400011B RID: 283
			private int blockStart;

			// Token: 0x0400011C RID: 284
			private int strstart;

			// Token: 0x0400011D RID: 285
			private int lookahead;

			// Token: 0x0400011E RID: 286
			private byte[] window;

			// Token: 0x0400011F RID: 287
			private byte[] inputBuf;

			// Token: 0x04000120 RID: 288
			private int totalIn;

			// Token: 0x04000121 RID: 289
			private int inputOff;

			// Token: 0x04000122 RID: 290
			private int inputEnd;

			// Token: 0x04000123 RID: 291
			private SimpleZip.DeflaterPending pending;

			// Token: 0x04000124 RID: 292
			private SimpleZip.DeflaterHuffman huffman;
		}

		// Token: 0x02000040 RID: 64
		internal sealed class DeflaterPending
		{
			// Token: 0x06000107 RID: 263 RVA: 0x00031E7C File Offset: 0x0003007C
			public void WriteShort(int s)
			{
				byte[] array = this.buf;
				int num = this.end;
				this.end = num + 1;
				array[num] = (byte)s;
				byte[] array2 = this.buf;
				num = this.end;
				this.end = num + 1;
				array2[num] = (byte)(s >> 8);
			}

			// Token: 0x06000108 RID: 264 RVA: 0x0002AFE3 File Offset: 0x000291E3
			public void WriteBlock(byte[] block, int offset, int len)
			{
				Array.Copy(block, offset, this.buf, this.end, len);
				this.end += len;
			}

			// Token: 0x17000027 RID: 39
			// (get) Token: 0x06000109 RID: 265 RVA: 0x0002B007 File Offset: 0x00029207
			public int BitCount
			{
				get
				{
					return this.bitCount;
				}
			}

			// Token: 0x0600010A RID: 266 RVA: 0x00031EC0 File Offset: 0x000300C0
			public void AlignToByte()
			{
				if (this.bitCount > 0)
				{
					byte[] array = this.buf;
					int num = this.end;
					this.end = num + 1;
					array[num] = (byte)this.bits;
					if (this.bitCount > 8)
					{
						byte[] array2 = this.buf;
						num = this.end;
						this.end = num + 1;
						array2[num] = (byte)(this.bits >> 8);
					}
				}
				this.bits = 0U;
				this.bitCount = 0;
			}

			// Token: 0x0600010B RID: 267 RVA: 0x00031F30 File Offset: 0x00030130
			public void WriteBits(int b, int count)
			{
				this.bits |= (uint)((uint)b << this.bitCount);
				this.bitCount += count;
				if (this.bitCount >= 16)
				{
					byte[] array = this.buf;
					int num = this.end;
					this.end = num + 1;
					array[num] = (byte)this.bits;
					byte[] array2 = this.buf;
					num = this.end;
					this.end = num + 1;
					array2[num] = (byte)(this.bits >> 8);
					this.bits >>= 16;
					this.bitCount -= 16;
				}
			}

			// Token: 0x17000028 RID: 40
			// (get) Token: 0x0600010C RID: 268 RVA: 0x0002B00F File Offset: 0x0002920F
			public bool IsFlushed
			{
				get
				{
					return this.end == 0;
				}
			}

			// Token: 0x0600010D RID: 269 RVA: 0x00031FCC File Offset: 0x000301CC
			public int Flush(byte[] output, int offset, int length)
			{
				if (this.bitCount >= 8)
				{
					byte[] array = this.buf;
					int num = this.end;
					this.end = num + 1;
					array[num] = (byte)this.bits;
					this.bits >>= 8;
					this.bitCount -= 8;
				}
				if (length > this.end - this.start)
				{
					length = this.end - this.start;
					Array.Copy(this.buf, this.start, output, offset, length);
					this.start = 0;
					this.end = 0;
				}
				else
				{
					Array.Copy(this.buf, this.start, output, offset, length);
					this.start += length;
				}
				return length;
			}

			// Token: 0x04000125 RID: 293
			protected byte[] buf = new byte[65536];

			// Token: 0x04000126 RID: 294
			private int start;

			// Token: 0x04000127 RID: 295
			private int end;

			// Token: 0x04000128 RID: 296
			private uint bits;

			// Token: 0x04000129 RID: 297
			private int bitCount;
		}

		// Token: 0x02000041 RID: 65
		internal sealed class ZipStream : MemoryStream
		{
			// Token: 0x0600010F RID: 271 RVA: 0x0002B032 File Offset: 0x00029232
			public void WriteShort(int value)
			{
				this.WriteByte((byte)(value & 255));
				this.WriteByte((byte)((value >> 8) & 255));
			}

			// Token: 0x06000110 RID: 272 RVA: 0x0002B052 File Offset: 0x00029252
			public void WriteInt(int value)
			{
				this.WriteShort(value);
				this.WriteShort(value >> 16);
			}

			// Token: 0x06000111 RID: 273 RVA: 0x0002B065 File Offset: 0x00029265
			public int ReadShort()
			{
				return this.ReadByte() | (this.ReadByte() << 8);
			}

			// Token: 0x06000112 RID: 274 RVA: 0x0002B076 File Offset: 0x00029276
			public int ReadInt()
			{
				return this.ReadShort() | (this.ReadShort() << 16);
			}

			// Token: 0x06000113 RID: 275 RVA: 0x0002B088 File Offset: 0x00029288
			public ZipStream()
			{
			}

			// Token: 0x06000114 RID: 276 RVA: 0x0002B090 File Offset: 0x00029290
			public ZipStream(byte[] buffer)
				: base(buffer, false)
			{
			}
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;

namespace System.Data.SqlClient
{
	// Token: 0x0200018D RID: 397
	internal class SqlAeadAes256CbcHmac256Algorithm : SqlClientEncryptionAlgorithm
	{
		// Token: 0x060017EF RID: 6127 RVA: 0x000ABA90 File Offset: 0x000AAE90
		internal SqlAeadAes256CbcHmac256Algorithm(SqlAeadAes256CbcHmac256EncryptionKey encryptionKey, SqlClientEncryptionType encryptionType, byte algorithmVersion)
		{
			this._columnEncryptionKey = encryptionKey;
			this._algorithmVersion = algorithmVersion;
			SqlAeadAes256CbcHmac256Algorithm._version[0] = algorithmVersion;
			if (encryptionType == SqlClientEncryptionType.Deterministic)
			{
				this._isDeterministic = true;
			}
			this._cryptoProviderPool = new ConcurrentQueue<AesCryptoServiceProvider>();
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x000ABAD0 File Offset: 0x000AAED0
		internal override byte[] EncryptData(byte[] plainText)
		{
			return this.EncryptData(plainText, true);
		}

		// Token: 0x060017F1 RID: 6129 RVA: 0x000ABAE8 File Offset: 0x000AAEE8
		protected byte[] EncryptData(byte[] plainText, bool hasAuthenticationTag)
		{
			byte[] array = new byte[16];
			if (this._isDeterministic)
			{
				SqlSecurityUtility.GetHMACWithSHA256(plainText, this._columnEncryptionKey.IVKey, array);
			}
			else
			{
				SqlSecurityUtility.GenerateRandomBytes(array);
			}
			int num = plainText.Length / 16 + 1;
			int num2 = (hasAuthenticationTag ? 32 : 0);
			int num3 = 1 + num2;
			int num4 = num3 + 16;
			int num5 = 1 + num2 + array.Length + num * 16;
			byte[] array2 = new byte[num5];
			array2[0] = this._algorithmVersion;
			Buffer.BlockCopy(array, 0, array2, num3, array.Length);
			AesCryptoServiceProvider aesCryptoServiceProvider;
			if (!this._cryptoProviderPool.TryDequeue(out aesCryptoServiceProvider))
			{
				aesCryptoServiceProvider = new AesCryptoServiceProvider();
				try
				{
					aesCryptoServiceProvider.Key = this._columnEncryptionKey.EncryptionKey;
					aesCryptoServiceProvider.Mode = CipherMode.CBC;
					aesCryptoServiceProvider.Padding = PaddingMode.PKCS7;
				}
				catch (Exception)
				{
					if (aesCryptoServiceProvider != null)
					{
						aesCryptoServiceProvider.Dispose();
					}
					throw;
				}
			}
			try
			{
				aesCryptoServiceProvider.IV = array;
				using (ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor())
				{
					int num6 = 0;
					int num7 = num4;
					if (num > 1)
					{
						num6 = (num - 1) * 16;
						num7 += cryptoTransform.TransformBlock(plainText, 0, num6, array2, num7);
					}
					byte[] array3 = cryptoTransform.TransformFinalBlock(plainText, num6, plainText.Length - num6);
					Buffer.BlockCopy(array3, 0, array2, num7, array3.Length);
					num7 += array3.Length;
				}
				if (hasAuthenticationTag)
				{
					using (HMACSHA256 hmacsha = new HMACSHA256(this._columnEncryptionKey.MACKey))
					{
						hmacsha.TransformBlock(SqlAeadAes256CbcHmac256Algorithm._version, 0, SqlAeadAes256CbcHmac256Algorithm._version.Length, SqlAeadAes256CbcHmac256Algorithm._version, 0);
						hmacsha.TransformBlock(array, 0, array.Length, array, 0);
						hmacsha.TransformBlock(array2, num4, num * 16, array2, num4);
						hmacsha.TransformFinalBlock(SqlAeadAes256CbcHmac256Algorithm._versionSize, 0, SqlAeadAes256CbcHmac256Algorithm._versionSize.Length);
						byte[] hash = hmacsha.Hash;
						Buffer.BlockCopy(hash, 0, array2, 1, num2);
					}
				}
			}
			finally
			{
				this._cryptoProviderPool.Enqueue(aesCryptoServiceProvider);
			}
			return array2;
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x000ABD1C File Offset: 0x000AB11C
		internal override byte[] DecryptData(byte[] cipherText)
		{
			return this.DecryptData(cipherText, true);
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x000ABD34 File Offset: 0x000AB134
		protected byte[] DecryptData(byte[] cipherText, bool hasAuthenticationTag)
		{
			byte[] array = new byte[16];
			int num = (hasAuthenticationTag ? 65 : 33);
			if (cipherText.Length < num)
			{
				throw SQL.InvalidCipherTextSize(cipherText.Length, num);
			}
			int num2 = 0;
			if (cipherText[num2] != this._algorithmVersion)
			{
				throw SQL.InvalidAlgorithmVersion(cipherText[num2], this._algorithmVersion);
			}
			num2++;
			int num3 = 0;
			if (hasAuthenticationTag)
			{
				num3 = num2;
				num2 += 32;
			}
			Buffer.BlockCopy(cipherText, num2, array, 0, array.Length);
			num2 += array.Length;
			int num4 = num2;
			int num5 = cipherText.Length - num2;
			if (hasAuthenticationTag)
			{
				byte[] array2 = this.PrepareAuthenticationTag(array, cipherText, num4, num5);
				if (!SqlSecurityUtility.CompareBytes(array2, cipherText, num3, array2.Length))
				{
					throw SQL.InvalidAuthenticationTag();
				}
			}
			return this.DecryptData(array, cipherText, num4, num5);
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x000ABDE0 File Offset: 0x000AB1E0
		private byte[] DecryptData(byte[] iv, byte[] cipherText, int offset, int count)
		{
			AesCryptoServiceProvider aesCryptoServiceProvider;
			if (!this._cryptoProviderPool.TryDequeue(out aesCryptoServiceProvider))
			{
				aesCryptoServiceProvider = new AesCryptoServiceProvider();
				try
				{
					aesCryptoServiceProvider.Key = this._columnEncryptionKey.EncryptionKey;
					aesCryptoServiceProvider.Mode = CipherMode.CBC;
					aesCryptoServiceProvider.Padding = PaddingMode.PKCS7;
				}
				catch (Exception)
				{
					if (aesCryptoServiceProvider != null)
					{
						aesCryptoServiceProvider.Dispose();
					}
					throw;
				}
			}
			byte[] array;
			try
			{
				aesCryptoServiceProvider.IV = iv;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateDecryptor())
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
						{
							cryptoStream.Write(cipherText, offset, count);
							cryptoStream.FlushFinalBlock();
							array = memoryStream.ToArray();
						}
					}
				}
			}
			finally
			{
				this._cryptoProviderPool.Enqueue(aesCryptoServiceProvider);
			}
			return array;
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x000ABF18 File Offset: 0x000AB318
		private byte[] PrepareAuthenticationTag(byte[] iv, byte[] cipherText, int offset, int length)
		{
			byte[] array = new byte[32];
			byte[] hash;
			using (HMACSHA256 hmacsha = new HMACSHA256(this._columnEncryptionKey.MACKey))
			{
				int num = hmacsha.TransformBlock(SqlAeadAes256CbcHmac256Algorithm._version, 0, SqlAeadAes256CbcHmac256Algorithm._version.Length, SqlAeadAes256CbcHmac256Algorithm._version, 0);
				num = hmacsha.TransformBlock(iv, 0, iv.Length, iv, 0);
				num = hmacsha.TransformBlock(cipherText, offset, length, cipherText, offset);
				hmacsha.TransformFinalBlock(SqlAeadAes256CbcHmac256Algorithm._versionSize, 0, SqlAeadAes256CbcHmac256Algorithm._versionSize.Length);
				hash = hmacsha.Hash;
			}
			Buffer.BlockCopy(hash, 0, array, 0, array.Length);
			return array;
		}

		// Token: 0x04000E6E RID: 3694
		internal const string AlgorithmName = "AEAD_AES_256_CBC_HMAC_SHA256";

		// Token: 0x04000E6F RID: 3695
		private const int _KeySizeInBytes = 32;

		// Token: 0x04000E70 RID: 3696
		private const int _BlockSizeInBytes = 16;

		// Token: 0x04000E71 RID: 3697
		private const int _MinimumCipherTextLengthInBytesNoAuthenticationTag = 33;

		// Token: 0x04000E72 RID: 3698
		private const int _MinimumCipherTextLengthInBytesWithAuthenticationTag = 65;

		// Token: 0x04000E73 RID: 3699
		private const CipherMode _cipherMode = CipherMode.CBC;

		// Token: 0x04000E74 RID: 3700
		private const PaddingMode _paddingMode = PaddingMode.PKCS7;

		// Token: 0x04000E75 RID: 3701
		private readonly bool _isDeterministic;

		// Token: 0x04000E76 RID: 3702
		private readonly byte _algorithmVersion;

		// Token: 0x04000E77 RID: 3703
		private readonly SqlAeadAes256CbcHmac256EncryptionKey _columnEncryptionKey;

		// Token: 0x04000E78 RID: 3704
		private readonly ConcurrentQueue<AesCryptoServiceProvider> _cryptoProviderPool;

		// Token: 0x04000E79 RID: 3705
		private static readonly byte[] _version = new byte[] { 1 };

		// Token: 0x04000E7A RID: 3706
		private static readonly byte[] _versionSize = new byte[] { 1 };
	}
}

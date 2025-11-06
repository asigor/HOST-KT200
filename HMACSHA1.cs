using System;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	/// <summary>Computes a Hash-based Message Authentication Code (HMAC) using the <see cref="T:System.Security.Cryptography.SHA1" /> hash function.</summary>
	// Token: 0x02000265 RID: 613
	[ComVisible(true)]
	public class HMACSHA1 : HMAC
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.HMACSHA1" /> class with a randomly generated key.</summary>
		// Token: 0x060021C2 RID: 8642 RVA: 0x00077A80 File Offset: 0x00075C80
		public HMACSHA1()
			: this(Utils.GenerateRandom(64))
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.HMACSHA1" /> class with the specified key data.</summary>
		/// <param name="key">The secret key for <see cref="T:System.Security.Cryptography.HMACSHA1" /> encryption. The key can be any length, but if it is more than 64 bytes long it is hashed (using SHA-1) to derive a 64-byte key. Therefore, the recommended size of the secret key is 64 bytes.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> parameter is <see langword="null" />.</exception>
		// Token: 0x060021C3 RID: 8643 RVA: 0x00077A8F File Offset: 0x00075C8F
		public HMACSHA1(byte[] key)
			: this(key, false)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.HMACSHA1" /> class with the specified key data and a value that specifies whether to use the managed version of the SHA1 algorithm.</summary>
		/// <param name="key">The secret key for <see cref="T:System.Security.Cryptography.HMACSHA1" /> encryption. The key can be any length, but if it is more than 64 bytes long, it is hashed (using SHA-1) to derive a 64-byte key. Therefore, the recommended size of the secret key is 64 bytes.</param>
		/// <param name="useManagedSha1">
		///   <see langword="true" /> to use the managed implementation of the SHA1 algorithm (the <see cref="T:System.Security.Cryptography.SHA1Managed" /> class); <see langword="false" /> to use the unmanaged implementation (the <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> class).</param>
		// Token: 0x060021C4 RID: 8644 RVA: 0x00077A9C File Offset: 0x00075C9C
		public HMACSHA1(byte[] key, bool useManagedSha1)
		{
			this.m_hashName = "SHA1";
			this.HashSizeValue = 160;
			if (base.GetType() == typeof(HMACSHA1))
			{
				this.m_impl = new NativeHmac(CapiNative.AlgorithmID.Sha1);
			}
			else if (useManagedSha1)
			{
				this.m_hash1 = new SHA1Managed();
				this.m_hash2 = new SHA1Managed();
			}
			else
			{
				this.m_hash1 = new SHA1CryptoServiceProvider();
				this.m_hash2 = new SHA1CryptoServiceProvider();
			}
			base.InitializeKey(key);
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x00077B26 File Offset: 0x00075D26
		internal sealed override HashAlgorithm InstantiateHash()
		{
			return new SHA1CryptoServiceProvider();
		}
	}
}

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	/// <summary>Represents the base class from which all implementations of cryptographic hash algorithms must derive.</summary>
	// Token: 0x02000269 RID: 617
	[ComVisible(true)]
	public abstract class HashAlgorithm : IDisposable, ICryptoTransform
	{
		/// <summary>Gets the size, in bits, of the computed hash code.</summary>
		/// <returns>The size, in bits, of the computed hash code.</returns>
		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x060021D6 RID: 8662 RVA: 0x00077F30 File Offset: 0x00076130
		public virtual int HashSize
		{
			get
			{
				return this.HashSizeValue;
			}
		}

		/// <summary>Gets the value of the computed hash code.</summary>
		/// <returns>The current value of the computed hash code.</returns>
		/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">
		///   <see cref="F:System.Security.Cryptography.HashAlgorithm.HashValue" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x060021D7 RID: 8663 RVA: 0x00077F38 File Offset: 0x00076138
		public virtual byte[] Hash
		{
			get
			{
				if (this.m_bDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				if (this.State != 0)
				{
					throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_HashNotYetFinalized"));
				}
				return (byte[])this.HashValue.Clone();
			}
		}

		/// <summary>Creates an instance of the default implementation of a hash algorithm.</summary>
		/// <returns>A new <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> instance, unless the default settings have been changed using the .</returns>
		// Token: 0x060021D8 RID: 8664 RVA: 0x00077F71 File Offset: 0x00076171
		public static HashAlgorithm Create()
		{
			return HashAlgorithm.Create("System.Security.Cryptography.HashAlgorithm");
		}

		/// <summary>Creates an instance of the specified implementation of a hash algorithm.</summary>
		/// <param name="hashName">The hash algorithm implementation to use. The following table shows the valid values for the <paramref name="hashName" /> parameter and the algorithms they map to.  
		///   Parameter value  
		///
		///   Implements  
		///
		///   SHA  
		///
		///  <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> SHA1  
		///
		///  <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> System.Security.Cryptography.SHA1  
		///
		///  <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> System.Security.Cryptography.HashAlgorithm  
		///
		///  <see cref="T:System.Security.Cryptography.SHA1CryptoServiceProvider" /> MD5  
		///
		///  <see cref="T:System.Security.Cryptography.MD5CryptoServiceProvider" /> System.Security.Cryptography.MD5  
		///
		///  <see cref="T:System.Security.Cryptography.MD5CryptoServiceProvider" /> SHA256  
		///
		///  <see cref="T:System.Security.Cryptography.SHA256Managed" /> SHA-256  
		///
		///  <see cref="T:System.Security.Cryptography.SHA256Managed" /> System.Security.Cryptography.SHA256  
		///
		///  <see cref="T:System.Security.Cryptography.SHA256Managed" /> SHA384  
		///
		///  <see cref="T:System.Security.Cryptography.SHA384Managed" /> SHA-384  
		///
		///  <see cref="T:System.Security.Cryptography.SHA384Managed" /> System.Security.Cryptography.SHA384  
		///
		///  <see cref="T:System.Security.Cryptography.SHA384Managed" /> SHA512  
		///
		///  <see cref="T:System.Security.Cryptography.SHA512Managed" /> SHA-512  
		///
		///  <see cref="T:System.Security.Cryptography.SHA512Managed" /> System.Security.Cryptography.SHA512  
		///
		///  <see cref="T:System.Security.Cryptography.SHA512Managed" /></param>
		/// <returns>A new instance of the specified hash algorithm, or <see langword="null" /> if <paramref name="hashName" /> is not a valid hash algorithm.</returns>
		// Token: 0x060021D9 RID: 8665 RVA: 0x00077F7D File Offset: 0x0007617D
		public static HashAlgorithm Create(string hashName)
		{
			return (HashAlgorithm)CryptoConfig.CreateFromName(hashName);
		}

		/// <summary>Computes the hash value for the specified <see cref="T:System.IO.Stream" /> object.</summary>
		/// <param name="inputStream">The input to compute the hash code for.</param>
		/// <returns>The computed hash code.</returns>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x060021DA RID: 8666 RVA: 0x00077F8C File Offset: 0x0007618C
		public byte[] ComputeHash(Stream inputStream)
		{
			if (this.m_bDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			byte[] array = new byte[4096];
			int num;
			do
			{
				num = inputStream.Read(array, 0, 4096);
				if (num > 0)
				{
					this.HashCore(array, 0, num);
				}
			}
			while (num > 0);
			this.HashValue = this.HashFinal();
			byte[] array2 = (byte[])this.HashValue.Clone();
			this.Initialize();
			return array2;
		}

		/// <summary>Computes the hash value for the specified byte array.</summary>
		/// <param name="buffer">The input to compute the hash code for.</param>
		/// <returns>The computed hash code.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x060021DB RID: 8667 RVA: 0x00077FF8 File Offset: 0x000761F8
		public byte[] ComputeHash(byte[] buffer)
		{
			if (this.m_bDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			this.HashCore(buffer, 0, buffer.Length);
			this.HashValue = this.HashFinal();
			byte[] array = (byte[])this.HashValue.Clone();
			this.Initialize();
			return array;
		}

		/// <summary>Computes the hash value for the specified region of the specified byte array.</summary>
		/// <param name="buffer">The input to compute the hash code for.</param>
		/// <param name="offset">The offset into the byte array from which to begin using data.</param>
		/// <param name="count">The number of bytes in the array to use as data.</param>
		/// <returns>The computed hash code.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="count" /> is an invalid value.  
		/// -or-  
		/// <paramref name="buffer" /> length is invalid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="offset" /> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x060021DC RID: 8668 RVA: 0x00078054 File Offset: 0x00076254
		public byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0 || count > buffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (buffer.Length - count < offset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (this.m_bDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			this.HashCore(buffer, offset, count);
			this.HashValue = this.HashFinal();
			byte[] array = (byte[])this.HashValue.Clone();
			this.Initialize();
			return array;
		}

		/// <summary>When overridden in a derived class, gets the input block size.</summary>
		/// <returns>The input block size.</returns>
		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x060021DD RID: 8669 RVA: 0x000780F6 File Offset: 0x000762F6
		public virtual int InputBlockSize
		{
			get
			{
				return 1;
			}
		}

		/// <summary>When overridden in a derived class, gets the output block size.</summary>
		/// <returns>The output block size.</returns>
		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x060021DE RID: 8670 RVA: 0x000780F9 File Offset: 0x000762F9
		public virtual int OutputBlockSize
		{
			get
			{
				return 1;
			}
		}

		/// <summary>When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.</summary>
		/// <returns>
		///   <see langword="true" /> if multiple blocks can be transformed; otherwise, <see langword="false" />.</returns>
		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x060021DF RID: 8671 RVA: 0x000780FC File Offset: 0x000762FC
		public virtual bool CanTransformMultipleBlocks
		{
			get
			{
				return true;
			}
		}

		/// <summary>Gets a value indicating whether the current transform can be reused.</summary>
		/// <returns>Always <see langword="true" />.</returns>
		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x060021E0 RID: 8672 RVA: 0x000780FF File Offset: 0x000762FF
		public virtual bool CanReuseTransform
		{
			get
			{
				return true;
			}
		}

		/// <summary>Computes the hash value for the specified region of the input byte array and copies the specified region of the input byte array to the specified region of the output byte array.</summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>The number of bytes written.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="inputCount" /> uses an invalid value.  
		/// -or-  
		/// <paramref name="inputBuffer" /> has an invalid length.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="inputBuffer" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x060021E1 RID: 8673 RVA: 0x00078104 File Offset: 0x00076304
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException("inputBuffer");
			}
			if (inputOffset < 0)
			{
				throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (inputCount < 0 || inputCount > inputBuffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (inputBuffer.Length - inputCount < inputOffset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (this.m_bDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			this.State = 1;
			this.HashCore(inputBuffer, inputOffset, inputCount);
			if (outputBuffer != null && (inputBuffer != outputBuffer || inputOffset != outputOffset))
			{
				Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
			}
			return inputCount;
		}

		/// <summary>Computes the hash value for the specified region of the specified byte array.</summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>An array that is a copy of the part of the input that is hashed.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="inputCount" /> uses an invalid value.  
		/// -or-  
		/// <paramref name="inputBuffer" /> has an invalid offset length.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="inputBuffer" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="inputOffset" /> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
		// Token: 0x060021E2 RID: 8674 RVA: 0x000781A4 File Offset: 0x000763A4
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException("inputBuffer");
			}
			if (inputOffset < 0)
			{
				throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (inputCount < 0 || inputCount > inputBuffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (inputBuffer.Length - inputCount < inputOffset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (this.m_bDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			this.HashCore(inputBuffer, inputOffset, inputCount);
			this.HashValue = this.HashFinal();
			byte[] array;
			if (inputCount != 0)
			{
				array = new byte[inputCount];
				Buffer.InternalBlockCopy(inputBuffer, inputOffset, array, 0, inputCount);
			}
			else
			{
				array = EmptyArray<byte>.Value;
			}
			this.State = 0;
			return array;
		}

		/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
		// Token: 0x060021E3 RID: 8675 RVA: 0x00078252 File Offset: 0x00076452
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Releases all resources used by the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
		// Token: 0x060021E4 RID: 8676 RVA: 0x00078261 File Offset: 0x00076461
		public void Clear()
		{
			((IDisposable)this).Dispose();
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing">
		///   <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		// Token: 0x060021E5 RID: 8677 RVA: 0x00078269 File Offset: 0x00076469
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.HashValue != null)
				{
					Array.Clear(this.HashValue, 0, this.HashValue.Length);
				}
				this.HashValue = null;
				this.m_bDisposed = true;
			}
		}

		/// <summary>Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.</summary>
		// Token: 0x060021E6 RID: 8678
		public abstract void Initialize();

		/// <summary>When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.</summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		// Token: 0x060021E7 RID: 8679
		protected abstract void HashCore(byte[] array, int ibStart, int cbSize);

		/// <summary>When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.</summary>
		/// <returns>The computed hash code.</returns>
		// Token: 0x060021E8 RID: 8680
		protected abstract byte[] HashFinal();

		/// <summary>Represents the size, in bits, of the computed hash code.</summary>
		// Token: 0x04000C55 RID: 3157
		protected int HashSizeValue;

		/// <summary>Represents the value of the computed hash code.</summary>
		// Token: 0x04000C56 RID: 3158
		protected internal byte[] HashValue;

		/// <summary>Represents the state of the hash computation.</summary>
		// Token: 0x04000C57 RID: 3159
		protected int State;

		// Token: 0x04000C58 RID: 3160
		private bool m_bDisposed;
	}
}

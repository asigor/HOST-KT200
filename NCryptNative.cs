using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography
{
	// Token: 0x020001A3 RID: 419
	internal static class NCryptNative
	{
		// Token: 0x060009DC RID: 2524 RVA: 0x0002E9D8 File Offset: 0x0002CBD8
		[SecuritySafeCritical]
		private static byte[] DecryptData<T>(SafeNCryptKeyHandle key, byte[] data, ref T paddingInfo, AsymmetricPaddingMode paddingMode, NCryptNative.NCryptDecryptor<T> decryptor) where T : struct
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = decryptor(key, data, data.Length, ref paddingInfo, null, 0, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = decryptor(key, data, data.Length, ref paddingInfo, array, array.Length, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			if (array.Length != num)
			{
				byte[] array2 = array;
				Array.Resize<byte>(ref array, num);
				Array.Clear(array2, 0, array2.Length);
			}
			return array;
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x0002EA50 File Offset: 0x0002CC50
		[SecuritySafeCritical]
		internal static byte[] DecryptDataPkcs1(SafeNCryptKeyHandle key, byte[] data)
		{
			BCryptNative.BCRYPT_PKCS1_PADDING_INFO bcrypt_PKCS1_PADDING_INFO = default(BCryptNative.BCRYPT_PKCS1_PADDING_INFO);
			return NCryptNative.DecryptData<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(key, data, ref bcrypt_PKCS1_PADDING_INFO, AsymmetricPaddingMode.Pkcs1, new NCryptNative.NCryptDecryptor<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(NCryptNative.Pkcs1PaddingDecryptionWrapper));
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x0002EA7C File Offset: 0x0002CC7C
		[SecuritySafeCritical]
		internal static byte[] DecryptDataOaep(SafeNCryptKeyHandle key, byte[] data, string hashAlgorithm)
		{
			BCryptNative.BCRYPT_OAEP_PADDING_INFO bcrypt_OAEP_PADDING_INFO = default(BCryptNative.BCRYPT_OAEP_PADDING_INFO);
			bcrypt_OAEP_PADDING_INFO.pszAlgId = hashAlgorithm;
			return NCryptNative.DecryptData<BCryptNative.BCRYPT_OAEP_PADDING_INFO>(key, data, ref bcrypt_OAEP_PADDING_INFO, AsymmetricPaddingMode.Oaep, new NCryptNative.NCryptDecryptor<BCryptNative.BCRYPT_OAEP_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptDecrypt));
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x00007D4D File Offset: 0x00005F4D
		[SecurityCritical]
		private static NCryptNative.ErrorCode Pkcs1PaddingDecryptionWrapper(SafeNCryptKeyHandle hKey, byte[] pbInput, int cbInput, ref BCryptNative.BCRYPT_PKCS1_PADDING_INFO pvPadding, byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags)
		{
			return NCryptNative.UnsafeNativeMethods.NCryptDecrypt(hKey, pbInput, cbInput, IntPtr.Zero, pbOutput, cbOutput, out pcbResult, dwFlags);
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x0002EAB0 File Offset: 0x0002CCB0
		[SecuritySafeCritical]
		private static byte[] EncryptData<T>(SafeNCryptKeyHandle key, byte[] data, ref T paddingInfo, AsymmetricPaddingMode paddingMode, NCryptNative.NCryptEncryptor<T> encryptor) where T : struct
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = encryptor(key, data, data.Length, ref paddingInfo, null, 0, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = encryptor(key, data, data.Length, ref paddingInfo, array, array.Length, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return array;
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x0002EB0C File Offset: 0x0002CD0C
		[SecuritySafeCritical]
		internal static byte[] EncryptDataOaep(SafeNCryptKeyHandle key, byte[] data, string hashAlgorithm)
		{
			BCryptNative.BCRYPT_OAEP_PADDING_INFO bcrypt_OAEP_PADDING_INFO = default(BCryptNative.BCRYPT_OAEP_PADDING_INFO);
			bcrypt_OAEP_PADDING_INFO.pszAlgId = hashAlgorithm;
			return NCryptNative.EncryptData<BCryptNative.BCRYPT_OAEP_PADDING_INFO>(key, data, ref bcrypt_OAEP_PADDING_INFO, AsymmetricPaddingMode.Oaep, new NCryptNative.NCryptEncryptor<BCryptNative.BCRYPT_OAEP_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptEncrypt));
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x0002EB40 File Offset: 0x0002CD40
		[SecuritySafeCritical]
		internal static byte[] EncryptDataPkcs1(SafeNCryptKeyHandle key, byte[] data)
		{
			BCryptNative.BCRYPT_PKCS1_PADDING_INFO bcrypt_PKCS1_PADDING_INFO = default(BCryptNative.BCRYPT_PKCS1_PADDING_INFO);
			return NCryptNative.EncryptData<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(key, data, ref bcrypt_PKCS1_PADDING_INFO, AsymmetricPaddingMode.Pkcs1, new NCryptNative.NCryptEncryptor<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(NCryptNative.Pkcs1PaddingEncryptionWrapper));
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x00007D64 File Offset: 0x00005F64
		[SecurityCritical]
		private static NCryptNative.ErrorCode Pkcs1PaddingEncryptionWrapper(SafeNCryptKeyHandle hKey, byte[] pbInput, int cbInput, ref BCryptNative.BCRYPT_PKCS1_PADDING_INFO pvPadding, byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags)
		{
			return NCryptNative.UnsafeNativeMethods.NCryptEncrypt(hKey, pbInput, cbInput, IntPtr.Zero, pbOutput, cbOutput, out pcbResult, dwFlags);
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x0002EB6C File Offset: 0x0002CD6C
		[SecuritySafeCritical]
		private static byte[] SignHash<T>(SafeNCryptKeyHandle key, byte[] hash, ref T paddingInfo, AsymmetricPaddingMode paddingMode, NCryptNative.NCryptHashSigner<T> signer) where T : struct
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = signer(key, ref paddingInfo, hash, hash.Length, null, 0, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = signer(key, ref paddingInfo, hash, hash.Length, array, array.Length, out num, paddingMode);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return array;
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x0002EBC8 File Offset: 0x0002CDC8
		[SecuritySafeCritical]
		internal static byte[] SignHashPkcs1(SafeNCryptKeyHandle key, byte[] hash, string hashAlgorithm)
		{
			BCryptNative.BCRYPT_PKCS1_PADDING_INFO bcrypt_PKCS1_PADDING_INFO = default(BCryptNative.BCRYPT_PKCS1_PADDING_INFO);
			bcrypt_PKCS1_PADDING_INFO.pszAlgId = hashAlgorithm;
			return NCryptNative.SignHash<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(key, hash, ref bcrypt_PKCS1_PADDING_INFO, AsymmetricPaddingMode.Pkcs1, new NCryptNative.NCryptHashSigner<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptSignHash));
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x0002EBFC File Offset: 0x0002CDFC
		[SecuritySafeCritical]
		internal static byte[] SignHashPss(SafeNCryptKeyHandle key, byte[] hash, string hashAlgorithm, int saltBytes)
		{
			BCryptNative.BCRYPT_PSS_PADDING_INFO bcrypt_PSS_PADDING_INFO = default(BCryptNative.BCRYPT_PSS_PADDING_INFO);
			bcrypt_PSS_PADDING_INFO.pszAlgId = hashAlgorithm;
			bcrypt_PSS_PADDING_INFO.cbSalt = saltBytes;
			return NCryptNative.SignHash<BCryptNative.BCRYPT_PSS_PADDING_INFO>(key, hash, ref bcrypt_PSS_PADDING_INFO, AsymmetricPaddingMode.Pss, new NCryptNative.NCryptHashSigner<BCryptNative.BCRYPT_PSS_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptSignHash));
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x0002EC38 File Offset: 0x0002CE38
		[SecuritySafeCritical]
		private static bool VerifySignature<T>(SafeNCryptKeyHandle key, byte[] hash, byte[] signature, ref T paddingInfo, AsymmetricPaddingMode paddingMode, NCryptNative.NCryptSignatureVerifier<T> verifier) where T : struct
		{
			NCryptNative.ErrorCode errorCode = verifier(key, ref paddingInfo, hash, hash.Length, signature, signature.Length, paddingMode);
			return errorCode == NCryptNative.ErrorCode.Success;
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x0002EC60 File Offset: 0x0002CE60
		[SecuritySafeCritical]
		internal static bool VerifySignaturePkcs1(SafeNCryptKeyHandle key, byte[] hash, string hashAlgorithm, byte[] signature)
		{
			BCryptNative.BCRYPT_PKCS1_PADDING_INFO bcrypt_PKCS1_PADDING_INFO = default(BCryptNative.BCRYPT_PKCS1_PADDING_INFO);
			bcrypt_PKCS1_PADDING_INFO.pszAlgId = hashAlgorithm;
			return NCryptNative.VerifySignature<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(key, hash, signature, ref bcrypt_PKCS1_PADDING_INFO, AsymmetricPaddingMode.Pkcs1, new NCryptNative.NCryptSignatureVerifier<BCryptNative.BCRYPT_PKCS1_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptVerifySignature));
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x0002EC94 File Offset: 0x0002CE94
		[SecuritySafeCritical]
		internal static bool VerifySignaturePss(SafeNCryptKeyHandle key, byte[] hash, string hashAlgorithm, int saltBytes, byte[] signature)
		{
			BCryptNative.BCRYPT_PSS_PADDING_INFO bcrypt_PSS_PADDING_INFO = default(BCryptNative.BCRYPT_PSS_PADDING_INFO);
			bcrypt_PSS_PADDING_INFO.pszAlgId = hashAlgorithm;
			bcrypt_PSS_PADDING_INFO.cbSalt = saltBytes;
			return NCryptNative.VerifySignature<BCryptNative.BCRYPT_PSS_PADDING_INFO>(key, hash, signature, ref bcrypt_PSS_PADDING_INFO, AsymmetricPaddingMode.Pss, new NCryptNative.NCryptSignatureVerifier<BCryptNative.BCRYPT_PSS_PADDING_INFO>(NCryptNative.UnsafeNativeMethods.NCryptVerifySignature));
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x060009EA RID: 2538 RVA: 0x0002ECD4 File Offset: 0x0002CED4
		internal static bool NCryptSupported
		{
			[SecuritySafeCritical]
			get
			{
				if (!NCryptNative.s_haveNcryptSupported)
				{
					using (Microsoft.Win32.SafeLibraryHandle safeLibraryHandle = Microsoft.Win32.UnsafeNativeMethods.LoadLibraryEx("ncrypt", IntPtr.Zero, 0))
					{
						NCryptNative.s_ncryptSupported = !safeLibraryHandle.IsInvalid;
						NCryptNative.s_haveNcryptSupported = true;
					}
				}
				return NCryptNative.s_ncryptSupported;
			}
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x0002ED38 File Offset: 0x0002CF38
		internal static byte[] BuildEccPublicBlob(string algorithm, BigInteger x, BigInteger y)
		{
			BCryptNative.KeyBlobMagicNumber keyBlobMagicNumber;
			int num;
			BCryptNative.MapAlgorithmIdToMagic(algorithm, out keyBlobMagicNumber, out num);
			byte[] array = NCryptNative.ReverseBytes(NCryptNative.FillKeyParameter(x.ToByteArray(), num));
			byte[] array2 = NCryptNative.ReverseBytes(NCryptNative.FillKeyParameter(y.ToByteArray(), num));
			byte[] array3 = new byte[8 + array.Length + array2.Length];
			Buffer.BlockCopy(BitConverter.GetBytes((int)keyBlobMagicNumber), 0, array3, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(array.Length), 0, array3, 4, 4);
			Buffer.BlockCopy(array, 0, array3, 8, array.Length);
			Buffer.BlockCopy(array2, 0, array3, 8 + array.Length, array2.Length);
			return array3;
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x0002EDC8 File Offset: 0x0002CFC8
		[SecurityCritical]
		internal static SafeNCryptKeyHandle CreatePersistedKey(SafeNCryptProviderHandle provider, string algorithm, string name, CngKeyCreationOptions options)
		{
			SafeNCryptKeyHandle safeNCryptKeyHandle = null;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptCreatePersistedKey(provider, out safeNCryptKeyHandle, algorithm, name, 0, options);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptKeyHandle;
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x0002EDF0 File Offset: 0x0002CFF0
		[SecurityCritical]
		internal static void DeleteKey(SafeNCryptKeyHandle key)
		{
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptDeleteKey(key, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			key.SetHandleAsInvalid();
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x0002EE18 File Offset: 0x0002D018
		[SecurityCritical]
		private unsafe static byte[] DeriveKeyMaterial(SafeNCryptSecretHandle secretAgreement, string kdf, string hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend, NCryptNative.SecretAgreementFlags flags)
		{
			List<NCryptNative.NCryptBuffer> list = new List<NCryptNative.NCryptBuffer>();
			IntPtr intPtr = IntPtr.Zero;
			RuntimeHelpers.PrepareConstrainedRegions();
			byte[] array4;
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					intPtr = Marshal.StringToCoTaskMemUni(hashAlgorithm);
				}
				list.Add(new NCryptNative.NCryptBuffer
				{
					cbBuffer = (hashAlgorithm.Length + 1) * 2,
					BufferType = NCryptNative.BufferType.KdfHashAlgorithm,
					pvBuffer = intPtr
				});
				try
				{
					fixed (byte[] array = hmacKey)
					{
						byte* ptr;
						if (hmacKey == null || array.Length == 0)
						{
							ptr = null;
						}
						else
						{
							ptr = &array[0];
						}
						fixed (byte[] array2 = secretPrepend)
						{
							byte* ptr2;
							if (secretPrepend == null || array2.Length == 0)
							{
								ptr2 = null;
							}
							else
							{
								ptr2 = &array2[0];
							}
							fixed (byte[] array3 = secretAppend)
							{
								byte* ptr3;
								if (secretAppend == null || array3.Length == 0)
								{
									ptr3 = null;
								}
								else
								{
									ptr3 = &array3[0];
								}
								if (ptr != null)
								{
									list.Add(new NCryptNative.NCryptBuffer
									{
										cbBuffer = hmacKey.Length,
										BufferType = NCryptNative.BufferType.KdfHmacKey,
										pvBuffer = new IntPtr((void*)ptr)
									});
								}
								if (ptr2 != null)
								{
									list.Add(new NCryptNative.NCryptBuffer
									{
										cbBuffer = secretPrepend.Length,
										BufferType = NCryptNative.BufferType.KdfSecretPrepend,
										pvBuffer = new IntPtr((void*)ptr2)
									});
								}
								if (ptr3 != null)
								{
									list.Add(new NCryptNative.NCryptBuffer
									{
										cbBuffer = secretAppend.Length,
										BufferType = NCryptNative.BufferType.KdfSecretAppend,
										pvBuffer = new IntPtr((void*)ptr3)
									});
								}
								array4 = NCryptNative.DeriveKeyMaterial(secretAgreement, kdf, list.ToArray(), flags);
							}
						}
					}
				}
				finally
				{
					byte[] array = null;
					byte[] array2 = null;
					byte[] array3 = null;
				}
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(intPtr);
				}
			}
			return array4;
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x0002EFF0 File Offset: 0x0002D1F0
		[SecurityCritical]
		private unsafe static byte[] DeriveKeyMaterial(SafeNCryptSecretHandle secretAgreement, string kdf, NCryptNative.NCryptBuffer[] parameters, NCryptNative.SecretAgreementFlags flags)
		{
			NCryptNative.NCryptBuffer* ptr;
			if (parameters == null || parameters.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &parameters[0];
			}
			NCryptNative.NCryptBufferDesc ncryptBufferDesc = default(NCryptNative.NCryptBufferDesc);
			ncryptBufferDesc.ulVersion = 0;
			ncryptBufferDesc.cBuffers = parameters.Length;
			ncryptBufferDesc.pBuffers = new IntPtr((void*)ptr);
			int num = 0;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptDeriveKey(secretAgreement, kdf, ref ncryptBufferDesc, null, 0, out num, flags);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = NCryptNative.UnsafeNativeMethods.NCryptDeriveKey(secretAgreement, kdf, ref ncryptBufferDesc, array, array.Length, out num, flags);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return array;
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x00007D7B File Offset: 0x00005F7B
		[SecurityCritical]
		internal static byte[] DeriveKeyMaterialHash(SafeNCryptSecretHandle secretAgreement, string hashAlgorithm, byte[] secretPrepend, byte[] secretAppend, NCryptNative.SecretAgreementFlags flags)
		{
			return NCryptNative.DeriveKeyMaterial(secretAgreement, "HASH", hashAlgorithm, null, secretPrepend, secretAppend, flags);
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x00007D8E File Offset: 0x00005F8E
		[SecurityCritical]
		internal static byte[] DeriveKeyMaterialHmac(SafeNCryptSecretHandle secretAgreement, string hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend, NCryptNative.SecretAgreementFlags flags)
		{
			return NCryptNative.DeriveKeyMaterial(secretAgreement, "HMAC", hashAlgorithm, hmacKey, secretPrepend, secretAppend, flags);
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x0002F090 File Offset: 0x0002D290
		[SecurityCritical]
		internal unsafe static byte[] DeriveKeyMaterialTls(SafeNCryptSecretHandle secretAgreement, byte[] label, byte[] seed, NCryptNative.SecretAgreementFlags flags)
		{
			NCryptNative.NCryptBuffer[] array = new NCryptNative.NCryptBuffer[2];
			byte* ptr;
			if (label == null || label.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &label[0];
			}
			byte* ptr2;
			if (seed == null || seed.Length == 0)
			{
				ptr2 = null;
			}
			else
			{
				ptr2 = &seed[0];
			}
			array[0] = new NCryptNative.NCryptBuffer
			{
				cbBuffer = label.Length,
				BufferType = NCryptNative.BufferType.KdfTlsLabel,
				pvBuffer = new IntPtr((void*)ptr)
			};
			array[1] = new NCryptNative.NCryptBuffer
			{
				cbBuffer = seed.Length,
				BufferType = NCryptNative.BufferType.KdfTlsSeed,
				pvBuffer = new IntPtr((void*)ptr2)
			};
			return NCryptNative.DeriveKeyMaterial(secretAgreement, "TLS_PRF", array, flags);
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x0002F144 File Offset: 0x0002D344
		[SecurityCritical]
		internal static SafeNCryptSecretHandle DeriveSecretAgreement(SafeNCryptKeyHandle privateKey, SafeNCryptKeyHandle otherPartyPublicKey)
		{
			SafeNCryptSecretHandle safeNCryptSecretHandle;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptSecretAgreement(privateKey, otherPartyPublicKey, out safeNCryptSecretHandle, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptSecretHandle;
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0002F168 File Offset: 0x0002D368
		[SecurityCritical]
		internal static byte[] ExportKey(SafeNCryptKeyHandle key, string format)
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptExportKey(key, IntPtr.Zero, format, IntPtr.Zero, null, 0, out num, 0);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = NCryptNative.UnsafeNativeMethods.NCryptExportKey(key, IntPtr.Zero, format, IntPtr.Zero, array, array.Length, out num, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return array;
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0002F1CC File Offset: 0x0002D3CC
		private static byte[] FillKeyParameter(byte[] key, int keySize)
		{
			int num = keySize / 8 + ((keySize % 8 == 0) ? 0 : 1);
			if (key.Length == num)
			{
				return key;
			}
			byte[] array = new byte[num];
			Buffer.BlockCopy(key, 0, array, 0, Math.Min(key.Length, array.Length));
			return array;
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x0002F20C File Offset: 0x0002D40C
		[SecurityCritical]
		internal static void FinalizeKey(SafeNCryptKeyHandle key)
		{
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptFinalizeKey(key, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x0002F22C File Offset: 0x0002D42C
		[SecurityCritical]
		internal static byte[] GetProperty(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions, out bool foundProperty)
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, null, 0, out num, propertyOptions);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall && errorCode != NCryptNative.ErrorCode.NotFound)
			{
				throw new CryptographicException((int)errorCode);
			}
			foundProperty = errorCode != NCryptNative.ErrorCode.NotFound;
			byte[] array = null;
			if (errorCode != NCryptNative.ErrorCode.NotFound && num > 0)
			{
				array = new byte[num];
				errorCode = NCryptNative.UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, array, array.Length, out num, propertyOptions);
				if (errorCode != NCryptNative.ErrorCode.Success)
				{
					throw new CryptographicException((int)errorCode);
				}
				foundProperty = true;
			}
			return array;
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x0002F2A4 File Offset: 0x0002D4A4
		[SecurityCritical]
		internal static int GetPropertyAsDWord(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
		{
			bool flag;
			byte[] property = NCryptNative.GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
			if (!flag || property == null)
			{
				return 0;
			}
			return BitConverter.ToInt32(property, 0);
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x0002F2CC File Offset: 0x0002D4CC
		[SecurityCritical]
		internal static NCryptNative.ErrorCode GetPropertyAsInt(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions, ref int propertyValue)
		{
			int num;
			return NCryptNative.UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, ref propertyValue, 4, out num, propertyOptions);
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x0002F2EC File Offset: 0x0002D4EC
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static IntPtr GetPropertyAsIntPtr(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
		{
			int size = IntPtr.Size;
			IntPtr zero = IntPtr.Zero;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, out zero, IntPtr.Size, out size, propertyOptions);
			if (errorCode == NCryptNative.ErrorCode.NotFound)
			{
				return IntPtr.Zero;
			}
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return zero;
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x0002F330 File Offset: 0x0002D530
		[SecurityCritical]
		internal unsafe static string GetPropertyAsString(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
		{
			bool flag;
			byte[] property = NCryptNative.GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
			if (!flag || property == null)
			{
				return null;
			}
			if (property.Length == 0)
			{
				return string.Empty;
			}
			byte[] array;
			byte* ptr;
			if ((array = property) == null || array.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array[0];
			}
			return Marshal.PtrToStringUni(new IntPtr((void*)ptr));
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x0002F380 File Offset: 0x0002D580
		[SecurityCritical]
		internal unsafe static T GetPropertyAsStruct<T>(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions) where T : struct
		{
			bool flag;
			byte[] property = NCryptNative.GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
			if (!flag || property == null)
			{
				return new T();
			}
			byte[] array;
			byte* ptr;
			if ((array = property) == null || array.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array[0];
			}
			return (T)((object)Marshal.PtrToStructure(new IntPtr((void*)ptr), typeof(T)));
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x0002F3D8 File Offset: 0x0002D5D8
		[SecurityCritical]
		internal static SafeNCryptKeyHandle ImportKey(SafeNCryptProviderHandle provider, byte[] keyBlob, string format)
		{
			SafeNCryptKeyHandle safeNCryptKeyHandle = null;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptImportKey(provider, IntPtr.Zero, format, IntPtr.Zero, out safeNCryptKeyHandle, keyBlob, keyBlob.Length, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptKeyHandle;
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x0002F40C File Offset: 0x0002D60C
		[SecurityCritical]
		internal static SafeNCryptKeyHandle ImportKey(SafeNCryptProviderHandle provider, byte[] keyBlob, string format, IntPtr pParametersList)
		{
			SafeNCryptKeyHandle safeNCryptKeyHandle = null;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptImportKey(provider, IntPtr.Zero, format, pParametersList, out safeNCryptKeyHandle, keyBlob, keyBlob.Length, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptKeyHandle;
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x0002F43C File Offset: 0x0002D63C
		[SecurityCritical]
		internal static SafeNCryptKeyHandle OpenKey(SafeNCryptProviderHandle provider, string name, CngKeyOpenOptions options)
		{
			SafeNCryptKeyHandle safeNCryptKeyHandle = null;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptOpenKey(provider, out safeNCryptKeyHandle, name, 0, options);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptKeyHandle;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x0002F464 File Offset: 0x0002D664
		[SecurityCritical]
		internal static SafeNCryptProviderHandle OpenStorageProvider(string providerName)
		{
			SafeNCryptProviderHandle safeNCryptProviderHandle = null;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptOpenStorageProvider(out safeNCryptProviderHandle, providerName, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return safeNCryptProviderHandle;
		}

		// Token: 0x06000A01 RID: 2561 RVA: 0x00007DA2 File Offset: 0x00005FA2
		private static byte[] ReverseBytes(byte[] buffer)
		{
			return NCryptNative.ReverseBytes(buffer, 0, buffer.Length, false);
		}

		// Token: 0x06000A02 RID: 2562 RVA: 0x00007DAF File Offset: 0x00005FAF
		private static byte[] ReverseBytes(byte[] buffer, int offset, int count)
		{
			return NCryptNative.ReverseBytes(buffer, offset, count, false);
		}

		// Token: 0x06000A03 RID: 2563 RVA: 0x0002F488 File Offset: 0x0002D688
		private static byte[] ReverseBytes(byte[] buffer, int offset, int count, bool padWithZeroByte)
		{
			byte[] array;
			if (padWithZeroByte)
			{
				array = new byte[count + 1];
			}
			else
			{
				array = new byte[count];
			}
			int num = offset + count - 1;
			for (int i = 0; i < count; i++)
			{
				array[i] = buffer[num - i];
			}
			return array;
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x00007DBA File Offset: 0x00005FBA
		[SecurityCritical]
		internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, int value, CngPropertyOptions propertyOptions)
		{
			NCryptNative.SetProperty(ncryptObject, propertyName, BitConverter.GetBytes(value), propertyOptions);
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x0002F4C8 File Offset: 0x0002D6C8
		[SecurityCritical]
		internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, string value, CngPropertyOptions propertyOptions)
		{
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptSetProperty(ncryptObject, propertyName, value, (value.Length + 1) * 2, propertyOptions);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
		}

		// Token: 0x06000A06 RID: 2566 RVA: 0x0002F4F4 File Offset: 0x0002D6F4
		[SecurityCritical]
		internal unsafe static void SetProperty<T>(SafeNCryptHandle ncryptObject, string propertyName, T value, CngPropertyOptions propertyOptions) where T : struct
		{
			byte[] array = new byte[Marshal.SizeOf(typeof(T))];
			byte[] array2;
			byte* ptr;
			if ((array2 = array) == null || array2.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array2[0];
			}
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					Marshal.StructureToPtr(value, new IntPtr((void*)ptr), false);
					flag = true;
				}
				NCryptNative.SetProperty(ncryptObject, propertyName, array, propertyOptions);
			}
			finally
			{
				if (flag)
				{
					Marshal.DestroyStructure(new IntPtr((void*)ptr), typeof(T));
				}
			}
			array2 = null;
		}

		// Token: 0x06000A07 RID: 2567 RVA: 0x0002F594 File Offset: 0x0002D794
		[SecurityCritical]
		internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, byte[] value, CngPropertyOptions propertyOptions)
		{
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptSetProperty(ncryptObject, propertyName, value, (value != null) ? value.Length : 0, propertyOptions);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x0002F5C0 File Offset: 0x0002D7C0
		[SecurityCritical]
		internal static byte[] SignHash(SafeNCryptKeyHandle key, byte[] hash)
		{
			int num = 0;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, null, 0, out num, 0);
			if (errorCode != NCryptNative.ErrorCode.Success && errorCode != NCryptNative.ErrorCode.BufferTooSmall)
			{
				throw new CryptographicException((int)errorCode);
			}
			byte[] array = new byte[num];
			errorCode = NCryptNative.UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, array, array.Length, out num, 0);
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			return array;
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x0002F620 File Offset: 0x0002D820
		[SecurityCritical]
		internal static byte[] SignHash(SafeNCryptKeyHandle key, byte[] hash, int expectedSize)
		{
			byte[] array = new byte[expectedSize];
			int num = 0;
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, array, array.Length, out num, 0);
			if (errorCode == NCryptNative.ErrorCode.BufferTooSmall)
			{
				array = new byte[num];
				errorCode = NCryptNative.UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, array, array.Length, out num, 0);
			}
			if (errorCode != NCryptNative.ErrorCode.Success)
			{
				throw new CryptographicException((int)errorCode);
			}
			Array.Resize<byte>(ref array, num);
			return array;
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x0002F688 File Offset: 0x0002D888
		internal static void UnpackEccPublicBlob(byte[] blob, out BigInteger x, out BigInteger y)
		{
			int num = BitConverter.ToInt32(blob, 4);
			x = new BigInteger(NCryptNative.ReverseBytes(blob, 8, num, true));
			y = new BigInteger(NCryptNative.ReverseBytes(blob, 8 + num, num, true));
		}

		// Token: 0x06000A0B RID: 2571 RVA: 0x0002F6C8 File Offset: 0x0002D8C8
		[SecurityCritical]
		internal static bool VerifySignature(SafeNCryptKeyHandle key, byte[] hash, byte[] signature)
		{
			NCryptNative.ErrorCode errorCode = NCryptNative.UnsafeNativeMethods.NCryptVerifySignature(key, IntPtr.Zero, hash, hash.Length, signature, signature.Length, 0);
			return errorCode == NCryptNative.ErrorCode.Success;
		}

		// Token: 0x04000955 RID: 2389
		private static volatile bool s_haveNcryptSupported;

		// Token: 0x04000956 RID: 2390
		private static volatile bool s_ncryptSupported;

		// Token: 0x020001A4 RID: 420
		internal enum BufferType
		{
			// Token: 0x04000958 RID: 2392
			KdfHashAlgorithm,
			// Token: 0x04000959 RID: 2393
			KdfSecretPrepend,
			// Token: 0x0400095A RID: 2394
			KdfSecretAppend,
			// Token: 0x0400095B RID: 2395
			KdfHmacKey,
			// Token: 0x0400095C RID: 2396
			KdfTlsLabel,
			// Token: 0x0400095D RID: 2397
			KdfTlsSeed
		}

		// Token: 0x020001A5 RID: 421
		internal enum ErrorCode
		{
			// Token: 0x0400095F RID: 2399
			Success,
			// Token: 0x04000960 RID: 2400
			BadSignature = -2146893818,
			// Token: 0x04000961 RID: 2401
			NotFound = -2146893807,
			// Token: 0x04000962 RID: 2402
			KeyDoesNotExist = -2146893802,
			// Token: 0x04000963 RID: 2403
			BufferTooSmall = -2146893784,
			// Token: 0x04000964 RID: 2404
			NoMoreItems = -2146893782
		}

		// Token: 0x020001A6 RID: 422
		internal static class KeyPropertyName
		{
			// Token: 0x04000965 RID: 2405
			internal const string Algorithm = "Algorithm Name";

			// Token: 0x04000966 RID: 2406
			internal const string AlgorithmGroup = "Algorithm Group";

			// Token: 0x04000967 RID: 2407
			internal const string ExportPolicy = "Export Policy";

			// Token: 0x04000968 RID: 2408
			internal const string KeyType = "Key Type";

			// Token: 0x04000969 RID: 2409
			internal const string KeyUsage = "Key Usage";

			// Token: 0x0400096A RID: 2410
			internal const string Length = "Length";

			// Token: 0x0400096B RID: 2411
			internal const string Name = "Name";

			// Token: 0x0400096C RID: 2412
			internal const string ParentWindowHandle = "HWND Handle";

			// Token: 0x0400096D RID: 2413
			internal const string PublicKeyLength = "PublicKeyLength";

			// Token: 0x0400096E RID: 2414
			internal const string ProviderHandle = "Provider Handle";

			// Token: 0x0400096F RID: 2415
			internal const string UIPolicy = "UI Policy";

			// Token: 0x04000970 RID: 2416
			internal const string UniqueName = "Unique Name";

			// Token: 0x04000971 RID: 2417
			internal const string UseContext = "Use Context";

			// Token: 0x04000972 RID: 2418
			internal const string ClrIsEphemeral = "CLR IsEphemeral";
		}

		// Token: 0x020001A7 RID: 423
		internal static class ProviderPropertyName
		{
			// Token: 0x04000973 RID: 2419
			internal const string Name = "Name";
		}

		// Token: 0x020001A8 RID: 424
		[Flags]
		internal enum SecretAgreementFlags
		{
			// Token: 0x04000975 RID: 2421
			None = 0,
			// Token: 0x04000976 RID: 2422
			UseSecretAsHmacKey = 1
		}

		// Token: 0x020001A9 RID: 425
		internal struct NCRYPT_UI_POLICY
		{
			// Token: 0x04000977 RID: 2423
			public int dwVersion;

			// Token: 0x04000978 RID: 2424
			public CngUIProtectionLevels dwFlags;

			// Token: 0x04000979 RID: 2425
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszCreationTitle;

			// Token: 0x0400097A RID: 2426
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszFriendlyName;

			// Token: 0x0400097B RID: 2427
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszDescription;
		}

		// Token: 0x020001AA RID: 426
		internal struct NCryptBuffer
		{
			// Token: 0x0400097C RID: 2428
			public int cbBuffer;

			// Token: 0x0400097D RID: 2429
			public NCryptNative.BufferType BufferType;

			// Token: 0x0400097E RID: 2430
			public IntPtr pvBuffer;
		}

		// Token: 0x020001AB RID: 427
		internal struct NCryptBufferDesc
		{
			// Token: 0x0400097F RID: 2431
			public int ulVersion;

			// Token: 0x04000980 RID: 2432
			public int cBuffers;

			// Token: 0x04000981 RID: 2433
			public IntPtr pBuffers;
		}

		// Token: 0x020001AC RID: 428
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical(SecurityCriticalScope.Everything)]
		internal static class UnsafeNativeMethods
		{
			// Token: 0x06000A0C RID: 2572
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptCreatePersistedKey(SafeNCryptProviderHandle hProvider, out SafeNCryptKeyHandle phKey, string pszAlgId, string pszKeyName, int dwLegacyKeySpec, CngKeyCreationOptions dwFlags);

			// Token: 0x06000A0D RID: 2573
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptDeleteKey(SafeNCryptKeyHandle hKey, int flags);

			// Token: 0x06000A0E RID: 2574
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptDeriveKey(SafeNCryptSecretHandle hSharedSecret, string pwszKDF, [In] ref NCryptNative.NCryptBufferDesc pParameterList, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbDerivedKey, int cbDerivedKey, out int pcbResult, NCryptNative.SecretAgreementFlags dwFlags);

			// Token: 0x06000A0F RID: 2575
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptExportKey(SafeNCryptKeyHandle hKey, IntPtr hExportKey, string pszBlobType, IntPtr pParameterList, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, int dwFlags);

			// Token: 0x06000A10 RID: 2576
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptFinalizeKey(SafeNCryptKeyHandle hKey, int dwFlags);

			// Token: 0x06000A11 RID: 2577
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, CngPropertyOptions dwFlags);

			// Token: 0x06000A12 RID: 2578
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, ref int pbOutput, int cbOutput, out int pcbResult, CngPropertyOptions dwFlags);

			// Token: 0x06000A13 RID: 2579
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, out IntPtr pbOutput, int cbOutput, out int pcbResult, CngPropertyOptions dwFlags);

			// Token: 0x06000A14 RID: 2580
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptImportKey(SafeNCryptProviderHandle hProvider, IntPtr hImportKey, string pszBlobType, IntPtr pParameterList, out SafeNCryptKeyHandle phKey, [MarshalAs(UnmanagedType.LPArray)] byte[] pbData, int cbData, int dwFlags);

			// Token: 0x06000A15 RID: 2581
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptOpenKey(SafeNCryptProviderHandle hProvider, out SafeNCryptKeyHandle phKey, string pszKeyName, int dwLegacyKeySpec, CngKeyOpenOptions dwFlags);

			// Token: 0x06000A16 RID: 2582
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptOpenStorageProvider(out SafeNCryptProviderHandle phProvider, string pszProviderName, int dwFlags);

			// Token: 0x06000A17 RID: 2583
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptSecretAgreement(SafeNCryptKeyHandle hPrivKey, SafeNCryptKeyHandle hPubKey, out SafeNCryptSecretHandle phSecret, int dwFlags);

			// Token: 0x06000A18 RID: 2584
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, [MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, CngPropertyOptions dwFlags);

			// Token: 0x06000A19 RID: 2585
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, string pbInput, int cbInput, CngPropertyOptions dwFlags);

			// Token: 0x06000A1A RID: 2586
			[DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
			internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, IntPtr pbInput, int cbInput, CngPropertyOptions dwFlags);

			// Token: 0x06000A1B RID: 2587
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptSignHash(SafeNCryptKeyHandle hKey, IntPtr pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] byte[] pbSignature, int cbSignature, out int pcbResult, int dwFlags);

			// Token: 0x06000A1C RID: 2588
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptVerifySignature(SafeNCryptKeyHandle hKey, IntPtr pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] byte[] pbSignature, int cbSignature, int dwFlags);

			// Token: 0x06000A1D RID: 2589
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptSignHash(SafeNCryptKeyHandle hKey, [In] ref BCryptNative.BCRYPT_PKCS1_PADDING_INFO pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbSignature, int cbSignature, out int pcbResult, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A1E RID: 2590
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptSignHash(SafeNCryptKeyHandle hKey, [In] ref BCryptNative.BCRYPT_PSS_PADDING_INFO pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbSignature, int cbSignature, out int pcbResult, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A1F RID: 2591
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptVerifySignature(SafeNCryptKeyHandle hKey, [In] ref BCryptNative.BCRYPT_PKCS1_PADDING_INFO pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbSignature, int cbSignature, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A20 RID: 2592
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptVerifySignature(SafeNCryptKeyHandle hKey, [In] ref BCryptNative.BCRYPT_PSS_PADDING_INFO pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbSignature, int cbSignature, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A21 RID: 2593
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptDecrypt(SafeNCryptKeyHandle hKey, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbInput, int cbInput, [In] ref BCryptNative.BCRYPT_OAEP_PADDING_INFO pvPadding, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A22 RID: 2594
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptDecrypt(SafeNCryptKeyHandle hKey, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbInput, int cbInput, IntPtr pvPaddingZero, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A23 RID: 2595
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptEncrypt(SafeNCryptKeyHandle hKey, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbInput, int cbInput, [In] ref BCryptNative.BCRYPT_OAEP_PADDING_INFO pvPadding, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);

			// Token: 0x06000A24 RID: 2596
			[DllImport("ncrypt.dll")]
			internal static extern NCryptNative.ErrorCode NCryptEncrypt(SafeNCryptKeyHandle hKey, [MarshalAs(UnmanagedType.LPArray)] [In] byte[] pbInput, int cbInput, IntPtr pvPaddingZero, [MarshalAs(UnmanagedType.LPArray)] [Out] byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);
		}

		// Token: 0x020001AD RID: 429
		// (Invoke) Token: 0x06000A26 RID: 2598
		[SecuritySafeCritical]
		private delegate NCryptNative.ErrorCode NCryptDecryptor<T>(SafeNCryptKeyHandle hKey, byte[] pbInput, int cbInput, ref T pvPadding, byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);

		// Token: 0x020001AE RID: 430
		// (Invoke) Token: 0x06000A2A RID: 2602
		[SecuritySafeCritical]
		private delegate NCryptNative.ErrorCode NCryptEncryptor<T>(SafeNCryptKeyHandle hKey, byte[] pbInput, int cbInput, ref T pvPadding, byte[] pbOutput, int cbOutput, out int pcbResult, AsymmetricPaddingMode dwFlags);

		// Token: 0x020001AF RID: 431
		// (Invoke) Token: 0x06000A2E RID: 2606
		[SecuritySafeCritical]
		private delegate NCryptNative.ErrorCode NCryptHashSigner<T>(SafeNCryptKeyHandle hKey, ref T pvPaddingInfo, byte[] pbHashValue, int cbHashValue, byte[] pbSignature, int cbSignature, out int pcbResult, AsymmetricPaddingMode dwFlags);

		// Token: 0x020001B0 RID: 432
		// (Invoke) Token: 0x06000A32 RID: 2610
		[SecuritySafeCritical]
		private delegate NCryptNative.ErrorCode NCryptSignatureVerifier<T>(SafeNCryptKeyHandle hKey, ref T pvPaddingInfo, byte[] pbHashValue, int cbHashValue, byte[] pbSignature, int cbSignature, AsymmetricPaddingMode dwFlags) where T : struct;
	}
}

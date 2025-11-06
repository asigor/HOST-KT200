using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Configuration;

namespace System.Web.Security.Cryptography
{
	// Token: 0x0200060C RID: 1548
	internal sealed class MachineKeyMasterKeyProvider : IMasterKeyProvider
	{
		// Token: 0x06004DC0 RID: 19904 RVA: 0x0010DAF0 File Offset: 0x0010BCF0
		internal MachineKeyMasterKeyProvider(MachineKeySection machineKeySection, string applicationId = null, string applicationName = null, CryptographicKey autogenKeys = null, KeyDerivationFunction keyDerivationFunction = null)
		{
			this._machineKeySection = machineKeySection;
			this._applicationId = applicationId;
			this._applicationName = applicationName;
			this._autogenKeys = autogenKeys;
			this._keyDerivationFunction = keyDerivationFunction;
		}

		// Token: 0x170016C0 RID: 5824
		// (get) Token: 0x06004DC1 RID: 19905 RVA: 0x0010DB1D File Offset: 0x0010BD1D
		internal string ApplicationName
		{
			get
			{
				if (this._applicationName == null)
				{
					this._applicationName = HttpRuntime.AppDomainAppVirtualPath ?? Process.GetCurrentProcess().MainModule.ModuleName;
				}
				return this._applicationName;
			}
		}

		// Token: 0x170016C1 RID: 5825
		// (get) Token: 0x06004DC2 RID: 19906 RVA: 0x0010DB4B File Offset: 0x0010BD4B
		internal string ApplicationId
		{
			get
			{
				if (this._applicationId == null)
				{
					this._applicationId = HttpRuntime.AppDomainAppId;
				}
				return this._applicationId;
			}
		}

		// Token: 0x170016C2 RID: 5826
		// (get) Token: 0x06004DC3 RID: 19907 RVA: 0x0010DB66 File Offset: 0x0010BD66
		internal CryptographicKey AutogenKeys
		{
			get
			{
				if (this._autogenKeys == null)
				{
					this._autogenKeys = new CryptographicKey(HttpRuntime.s_autogenKeys);
				}
				return this._autogenKeys;
			}
		}

		// Token: 0x170016C3 RID: 5827
		// (get) Token: 0x06004DC4 RID: 19908 RVA: 0x0010DB86 File Offset: 0x0010BD86
		internal KeyDerivationFunction KeyDerivationFunction
		{
			get
			{
				if (this._keyDerivationFunction == null)
				{
					this._keyDerivationFunction = new KeyDerivationFunction(SP800_108.DeriveKey);
				}
				return this._keyDerivationFunction;
			}
		}

		// Token: 0x06004DC5 RID: 19909 RVA: 0x0010DBA8 File Offset: 0x0010BDA8
		private static void AddSpecificPurposeString(IList<string> specificPurposes, string key, string value)
		{
			specificPurposes.Add(key + ": " + value);
		}

		// Token: 0x06004DC6 RID: 19910 RVA: 0x0010DBBC File Offset: 0x0010BDBC
		private CryptographicKey GenerateCryptographicKey(string configAttributeName, string configAttributeValue, int autogenKeyOffset, int autogenKeyCount, string errorResourceString)
		{
			byte[] array = CryptoUtil.HexToBinary(configAttributeValue);
			if (array != null && array.Length != 0)
			{
				return new CryptographicKey(array);
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (configAttributeValue != null)
			{
				foreach (string text in configAttributeValue.Split(new char[] { ',' }))
				{
					if (!(text == "AutoGenerate"))
					{
						if (!(text == "IsolateApps"))
						{
							if (!(text == "IsolateByAppId"))
							{
								throw ConfigUtil.MakeConfigurationErrorsException(SR.GetString(errorResourceString), null, this._machineKeySection.ElementInformation.Properties[configAttributeName]);
							}
							flag3 = true;
						}
						else
						{
							flag2 = true;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				throw ConfigUtil.MakeConfigurationErrorsException(SR.GetString(errorResourceString), null, this._machineKeySection.ElementInformation.Properties[configAttributeName]);
			}
			CryptographicKey cryptographicKey = this.AutogenKeys.ExtractBits(autogenKeyOffset, autogenKeyCount);
			List<string> list = new List<string>();
			if (flag2)
			{
				MachineKeyMasterKeyProvider.AddSpecificPurposeString(list, "IsolateApps", this.ApplicationName);
			}
			if (flag3)
			{
				MachineKeyMasterKeyProvider.AddSpecificPurposeString(list, "IsolateByAppId", this.ApplicationId);
			}
			Purpose purpose = new Purpose("MachineKeyDerivation", list.ToArray());
			return this.KeyDerivationFunction(cryptographicKey, purpose);
		}

		// Token: 0x06004DC7 RID: 19911 RVA: 0x0010DCFD File Offset: 0x0010BEFD
		public CryptographicKey GetEncryptionKey()
		{
			if (this._encryptionKey == null)
			{
				this._encryptionKey = this.GenerateCryptographicKey("decryptionKey", this._machineKeySection.DecryptionKey, 0, 256, "Invalid_decryption_key");
			}
			return this._encryptionKey;
		}

		// Token: 0x06004DC8 RID: 19912 RVA: 0x0010DD34 File Offset: 0x0010BF34
		public CryptographicKey GetValidationKey()
		{
			if (this._validationKey == null)
			{
				this._validationKey = this.GenerateCryptographicKey("validationKey", this._machineKeySection.ValidationKey, 256, 256, "Invalid_validation_key");
			}
			return this._validationKey;
		}

		// Token: 0x0400296A RID: 10602
		private const int AUTOGEN_ENCRYPTION_OFFSET = 0;

		// Token: 0x0400296B RID: 10603
		private const int AUTOGEN_ENCRYPTION_KEYLENGTH = 256;

		// Token: 0x0400296C RID: 10604
		private const int AUTOGEN_VALIDATION_OFFSET = 256;

		// Token: 0x0400296D RID: 10605
		private const int AUTOGEN_VALIDATION_KEYLENGTH = 256;

		// Token: 0x0400296E RID: 10606
		private const string AUTOGEN_KEYDERIVATION_PRIMARYPURPOSE = "MachineKeyDerivation";

		// Token: 0x0400296F RID: 10607
		private const string AUTOGEN_KEYDERIVATION_ISOLATEAPPS_SPECIFICPURPOSE = "IsolateApps";

		// Token: 0x04002970 RID: 10608
		private const string AUTOGEN_KEYDERIVATION_ISOLATEBYAPPID_SPECIFICPURPOSE = "IsolateByAppId";

		// Token: 0x04002971 RID: 10609
		private string _applicationId;

		// Token: 0x04002972 RID: 10610
		private string _applicationName;

		// Token: 0x04002973 RID: 10611
		private CryptographicKey _autogenKeys;

		// Token: 0x04002974 RID: 10612
		private CryptographicKey _encryptionKey;

		// Token: 0x04002975 RID: 10613
		private KeyDerivationFunction _keyDerivationFunction;

		// Token: 0x04002976 RID: 10614
		private readonly MachineKeySection _machineKeySection;

		// Token: 0x04002977 RID: 10615
		private CryptographicKey _validationKey;
	}
}

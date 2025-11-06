using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.ComponentModel
{
	/// <summary>Provides properties and methods to add a license to a component and to manage a <see cref="T:System.ComponentModel.LicenseProvider" />. This class cannot be inherited.</summary>
	// Token: 0x0200057F RID: 1407
	[HostProtection(SecurityAction.LinkDemand, ExternalProcessMgmt = true)]
	public sealed class LicenseManager
	{
		// Token: 0x060033FB RID: 13307 RVA: 0x000E400A File Offset: 0x000E220A
		private LicenseManager()
		{
		}

		/// <summary>Gets or sets the current <see cref="T:System.ComponentModel.LicenseContext" />, which specifies when you can use the licensed object.</summary>
		/// <returns>A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed object.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" /> property is currently locked and cannot be changed.</exception>
		// Token: 0x17000CB6 RID: 3254
		// (get) Token: 0x060033FC RID: 13308 RVA: 0x000E4014 File Offset: 0x000E2214
		// (set) Token: 0x060033FD RID: 13309 RVA: 0x000E4074 File Offset: 0x000E2274
		public static LicenseContext CurrentContext
		{
			get
			{
				if (LicenseManager.context == null)
				{
					object obj = LicenseManager.internalSyncObject;
					lock (obj)
					{
						if (LicenseManager.context == null)
						{
							LicenseManager.context = new RuntimeLicenseContext();
						}
					}
				}
				return LicenseManager.context;
			}
			set
			{
				object obj = LicenseManager.internalSyncObject;
				lock (obj)
				{
					if (LicenseManager.contextLockHolder != null)
					{
						throw new InvalidOperationException(SR.GetString("LicMgrContextCannotBeChanged"));
					}
					LicenseManager.context = value;
				}
			}
		}

		/// <summary>Gets the <see cref="T:System.ComponentModel.LicenseUsageMode" /> which specifies when you can use the licensed object for the <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" />.</summary>
		/// <returns>One of the <see cref="T:System.ComponentModel.LicenseUsageMode" /> values, as specified in the <see cref="P:System.ComponentModel.LicenseManager.CurrentContext" /> property.</returns>
		// Token: 0x17000CB7 RID: 3255
		// (get) Token: 0x060033FE RID: 13310 RVA: 0x000E40CC File Offset: 0x000E22CC
		public static LicenseUsageMode UsageMode
		{
			get
			{
				if (LicenseManager.context != null)
				{
					return LicenseManager.context.UsageMode;
				}
				return LicenseUsageMode.Runtime;
			}
		}

		// Token: 0x060033FF RID: 13311 RVA: 0x000E40E8 File Offset: 0x000E22E8
		private static void CacheProvider(Type type, LicenseProvider provider)
		{
			if (LicenseManager.providers == null)
			{
				LicenseManager.providers = new Hashtable();
			}
			LicenseManager.providers[type] = provider;
			if (provider != null)
			{
				if (LicenseManager.providerInstances == null)
				{
					LicenseManager.providerInstances = new Hashtable();
				}
				LicenseManager.providerInstances[provider.GetType()] = provider;
			}
		}

		/// <summary>Creates an instance of the specified type, given a context in which you can use the licensed instance.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type to create. </param>
		/// <param name="creationContext">A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed instance. </param>
		/// <returns>An instance of the specified type.</returns>
		// Token: 0x06003400 RID: 13312 RVA: 0x000E4143 File Offset: 0x000E2343
		public static object CreateWithContext(Type type, LicenseContext creationContext)
		{
			return LicenseManager.CreateWithContext(type, creationContext, new object[0]);
		}

		/// <summary>Creates an instance of the specified type with the specified arguments, given a context in which you can use the licensed instance.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type to create. </param>
		/// <param name="creationContext">A <see cref="T:System.ComponentModel.LicenseContext" /> that specifies when you can use the licensed instance. </param>
		/// <param name="args">An array of type <see cref="T:System.Object" /> that represents the arguments for the type. </param>
		/// <returns>An instance of the specified type with the given array of arguments.</returns>
		// Token: 0x06003401 RID: 13313 RVA: 0x000E4154 File Offset: 0x000E2354
		public static object CreateWithContext(Type type, LicenseContext creationContext, object[] args)
		{
			object obj = null;
			object obj2 = LicenseManager.internalSyncObject;
			lock (obj2)
			{
				LicenseContext currentContext = LicenseManager.CurrentContext;
				try
				{
					LicenseManager.CurrentContext = creationContext;
					LicenseManager.LockContext(LicenseManager.selfLock);
					try
					{
						obj = SecurityUtils.SecureCreateInstance(type, args);
					}
					catch (TargetInvocationException ex)
					{
						throw ex.InnerException;
					}
				}
				finally
				{
					LicenseManager.UnlockContext(LicenseManager.selfLock);
					LicenseManager.CurrentContext = currentContext;
				}
			}
			return obj;
		}

		// Token: 0x06003402 RID: 13314 RVA: 0x000E41E4 File Offset: 0x000E23E4
		private static bool GetCachedNoLicenseProvider(Type type)
		{
			return LicenseManager.providers != null && LicenseManager.providers.ContainsKey(type);
		}

		// Token: 0x06003403 RID: 13315 RVA: 0x000E41FE File Offset: 0x000E23FE
		private static LicenseProvider GetCachedProvider(Type type)
		{
			if (LicenseManager.providers != null)
			{
				return (LicenseProvider)LicenseManager.providers[type];
			}
			return null;
		}

		// Token: 0x06003404 RID: 13316 RVA: 0x000E421D File Offset: 0x000E241D
		private static LicenseProvider GetCachedProviderInstance(Type providerType)
		{
			if (LicenseManager.providerInstances != null)
			{
				return (LicenseProvider)LicenseManager.providerInstances[providerType];
			}
			return null;
		}

		// Token: 0x06003405 RID: 13317 RVA: 0x000E423C File Offset: 0x000E243C
		private static IntPtr GetLicenseInteropHelperType()
		{
			return typeof(LicenseManager.LicenseInteropHelper).TypeHandle.Value;
		}

		/// <summary>Returns whether the given type has a valid license.</summary>
		/// <param name="type">The <see cref="T:System.Type" /> to find a valid license for. </param>
		/// <returns>
		///     <see langword="true" /> if the given type is licensed; otherwise, <see langword="false" />.</returns>
		// Token: 0x06003406 RID: 13318 RVA: 0x000E4260 File Offset: 0x000E2460
		public static bool IsLicensed(Type type)
		{
			License license;
			bool flag = LicenseManager.ValidateInternal(type, null, false, out license);
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
			return flag;
		}

		/// <summary>Determines whether a valid license can be granted for the specified type.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the <see cref="T:System.ComponentModel.License" />. </param>
		/// <returns>
		///     <see langword="true" /> if a valid license can be granted; otherwise, <see langword="false" />.</returns>
		// Token: 0x06003407 RID: 13319 RVA: 0x000E4284 File Offset: 0x000E2484
		public static bool IsValid(Type type)
		{
			License license;
			bool flag = LicenseManager.ValidateInternal(type, null, false, out license);
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
			return flag;
		}

		/// <summary>Determines whether a valid license can be granted for the specified instance of the type. This method creates a valid <see cref="T:System.ComponentModel.License" />.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
		/// <param name="instance">An object of the specified type or a type derived from the specified type. </param>
		/// <param name="license">A <see cref="T:System.ComponentModel.License" /> that is a valid license, or <see langword="null" /> if a valid license cannot be granted. </param>
		/// <returns>
		///     <see langword="true" /> if a valid <see cref="T:System.ComponentModel.License" /> can be granted; otherwise, <see langword="false" />.</returns>
		// Token: 0x06003408 RID: 13320 RVA: 0x000E42A8 File Offset: 0x000E24A8
		public static bool IsValid(Type type, object instance, out License license)
		{
			return LicenseManager.ValidateInternal(type, instance, false, out license);
		}

		/// <summary>Prevents changes being made to the current <see cref="T:System.ComponentModel.LicenseContext" /> of the given object.</summary>
		/// <param name="contextUser">The object whose current context you want to lock. </param>
		/// <exception cref="T:System.InvalidOperationException">The context is already locked.</exception>
		// Token: 0x06003409 RID: 13321 RVA: 0x000E42B4 File Offset: 0x000E24B4
		public static void LockContext(object contextUser)
		{
			object obj = LicenseManager.internalSyncObject;
			lock (obj)
			{
				if (LicenseManager.contextLockHolder != null)
				{
					throw new InvalidOperationException(SR.GetString("LicMgrAlreadyLocked"));
				}
				LicenseManager.contextLockHolder = contextUser;
			}
		}

		/// <summary>Allows changes to be made to the current <see cref="T:System.ComponentModel.LicenseContext" /> of the given object.</summary>
		/// <param name="contextUser">The object whose current context you want to unlock. </param>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="contextUser" /> represents a different user than the one specified in a previous call to <see cref="M:System.ComponentModel.LicenseManager.LockContext(System.Object)" />. </exception>
		// Token: 0x0600340A RID: 13322 RVA: 0x000E430C File Offset: 0x000E250C
		public static void UnlockContext(object contextUser)
		{
			object obj = LicenseManager.internalSyncObject;
			lock (obj)
			{
				if (LicenseManager.contextLockHolder != contextUser)
				{
					throw new ArgumentException(SR.GetString("LicMgrDifferentUser"));
				}
				LicenseManager.contextLockHolder = null;
			}
		}

		// Token: 0x0600340B RID: 13323 RVA: 0x000E4364 File Offset: 0x000E2564
		private static bool ValidateInternal(Type type, object instance, bool allowExceptions, out License license)
		{
			string text;
			return LicenseManager.ValidateInternalRecursive(LicenseManager.CurrentContext, type, instance, allowExceptions, out license, out text);
		}

		// Token: 0x0600340C RID: 13324 RVA: 0x000E4384 File Offset: 0x000E2584
		private static bool ValidateInternalRecursive(LicenseContext context, Type type, object instance, bool allowExceptions, out License license, out string licenseKey)
		{
			LicenseProvider licenseProvider = LicenseManager.GetCachedProvider(type);
			if (licenseProvider == null && !LicenseManager.GetCachedNoLicenseProvider(type))
			{
				LicenseProviderAttribute licenseProviderAttribute = (LicenseProviderAttribute)Attribute.GetCustomAttribute(type, typeof(LicenseProviderAttribute), false);
				if (licenseProviderAttribute != null)
				{
					Type licenseProvider2 = licenseProviderAttribute.LicenseProvider;
					licenseProvider = LicenseManager.GetCachedProviderInstance(licenseProvider2);
					if (licenseProvider == null)
					{
						licenseProvider = (LicenseProvider)SecurityUtils.SecureCreateInstance(licenseProvider2);
					}
				}
				LicenseManager.CacheProvider(type, licenseProvider);
			}
			license = null;
			bool flag = true;
			licenseKey = null;
			if (licenseProvider != null)
			{
				license = licenseProvider.GetLicense(context, type, instance, allowExceptions);
				if (license == null)
				{
					flag = false;
				}
				else
				{
					licenseKey = license.LicenseKey;
				}
			}
			if (flag && instance == null)
			{
				Type baseType = type.BaseType;
				if (baseType != typeof(object) && baseType != null)
				{
					if (license != null)
					{
						license.Dispose();
						license = null;
					}
					string text;
					flag = LicenseManager.ValidateInternalRecursive(context, baseType, null, allowExceptions, out license, out text);
					if (license != null)
					{
						license.Dispose();
						license = null;
					}
				}
			}
			return flag;
		}

		/// <summary>Determines whether a license can be granted for the specified type.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
		/// <exception cref="T:System.ComponentModel.LicenseException">A <see cref="T:System.ComponentModel.License" /> cannot be granted. </exception>
		// Token: 0x0600340D RID: 13325 RVA: 0x000E446C File Offset: 0x000E266C
		public static void Validate(Type type)
		{
			License license;
			if (!LicenseManager.ValidateInternal(type, null, true, out license))
			{
				throw new LicenseException(type);
			}
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
		}

		/// <summary>Determines whether a license can be granted for the instance of the specified type.</summary>
		/// <param name="type">A <see cref="T:System.Type" /> that represents the type of object that requests the license. </param>
		/// <param name="instance">An <see cref="T:System.Object" /> of the specified type or a type derived from the specified type. </param>
		/// <returns>A valid <see cref="T:System.ComponentModel.License" />.</returns>
		/// <exception cref="T:System.ComponentModel.LicenseException">The type is licensed, but a <see cref="T:System.ComponentModel.License" /> cannot be granted. </exception>
		// Token: 0x0600340E RID: 13326 RVA: 0x000E4498 File Offset: 0x000E2698
		public static License Validate(Type type, object instance)
		{
			License license;
			if (!LicenseManager.ValidateInternal(type, instance, true, out license))
			{
				throw new LicenseException(type, instance);
			}
			return license;
		}

		// Token: 0x040029B9 RID: 10681
		private static readonly object selfLock = new object();

		// Token: 0x040029BA RID: 10682
		private static volatile LicenseContext context = null;

		// Token: 0x040029BB RID: 10683
		private static object contextLockHolder = null;

		// Token: 0x040029BC RID: 10684
		private static volatile Hashtable providers;

		// Token: 0x040029BD RID: 10685
		private static volatile Hashtable providerInstances;

		// Token: 0x040029BE RID: 10686
		private static object internalSyncObject = new object();

		// Token: 0x02000895 RID: 2197
		private class LicenseInteropHelper
		{
			// Token: 0x06004590 RID: 17808 RVA: 0x00122ED0 File Offset: 0x001210D0
			private static object AllocateAndValidateLicense(RuntimeTypeHandle rth, IntPtr bstrKey, int fDesignTime)
			{
				Type typeFromHandle = Type.GetTypeFromHandle(rth);
				LicenseManager.LicenseInteropHelper.CLRLicenseContext clrlicenseContext = new LicenseManager.LicenseInteropHelper.CLRLicenseContext((fDesignTime != 0) ? LicenseUsageMode.Designtime : LicenseUsageMode.Runtime, typeFromHandle);
				if (fDesignTime == 0 && bstrKey != (IntPtr)0)
				{
					clrlicenseContext.SetSavedLicenseKey(typeFromHandle, Marshal.PtrToStringBSTR(bstrKey));
				}
				object obj;
				try
				{
					obj = LicenseManager.CreateWithContext(typeFromHandle, clrlicenseContext);
				}
				catch (LicenseException ex)
				{
					throw new COMException(ex.Message, -2147221230);
				}
				return obj;
			}

			// Token: 0x06004591 RID: 17809 RVA: 0x00122F40 File Offset: 0x00121140
			private static int RequestLicKey(RuntimeTypeHandle rth, ref IntPtr pbstrKey)
			{
				Type typeFromHandle = Type.GetTypeFromHandle(rth);
				License license;
				string text;
				if (!LicenseManager.ValidateInternalRecursive(LicenseManager.CurrentContext, typeFromHandle, null, false, out license, out text))
				{
					return -2147483640;
				}
				if (text == null)
				{
					return -2147483640;
				}
				pbstrKey = Marshal.StringToBSTR(text);
				if (license != null)
				{
					license.Dispose();
					license = null;
				}
				return 0;
			}

			// Token: 0x06004592 RID: 17810 RVA: 0x00122F8C File Offset: 0x0012118C
			private void GetLicInfo(RuntimeTypeHandle rth, ref int pRuntimeKeyAvail, ref int pLicVerified)
			{
				pRuntimeKeyAvail = 0;
				pLicVerified = 0;
				Type typeFromHandle = Type.GetTypeFromHandle(rth);
				if (this.helperContext == null)
				{
					this.helperContext = new DesigntimeLicenseContext();
				}
				else
				{
					this.helperContext.savedLicenseKeys.Clear();
				}
				License license;
				string text;
				if (LicenseManager.ValidateInternalRecursive(this.helperContext, typeFromHandle, null, false, out license, out text))
				{
					if (this.helperContext.savedLicenseKeys.Contains(typeFromHandle.AssemblyQualifiedName))
					{
						pRuntimeKeyAvail = 1;
					}
					if (license != null)
					{
						license.Dispose();
						license = null;
						pLicVerified = 1;
					}
				}
			}

			// Token: 0x06004593 RID: 17811 RVA: 0x00123008 File Offset: 0x00121208
			private void GetCurrentContextInfo(ref int fDesignTime, ref IntPtr bstrKey, RuntimeTypeHandle rth)
			{
				this.savedLicenseContext = LicenseManager.CurrentContext;
				this.savedType = Type.GetTypeFromHandle(rth);
				if (this.savedLicenseContext.UsageMode == LicenseUsageMode.Designtime)
				{
					fDesignTime = 1;
					bstrKey = (IntPtr)0;
					return;
				}
				fDesignTime = 0;
				string savedLicenseKey = this.savedLicenseContext.GetSavedLicenseKey(this.savedType, null);
				bstrKey = Marshal.StringToBSTR(savedLicenseKey);
			}

			// Token: 0x06004594 RID: 17812 RVA: 0x00123064 File Offset: 0x00121264
			private void SaveKeyInCurrentContext(IntPtr bstrKey)
			{
				if (bstrKey != (IntPtr)0)
				{
					this.savedLicenseContext.SetSavedLicenseKey(this.savedType, Marshal.PtrToStringBSTR(bstrKey));
				}
			}

			// Token: 0x040037BE RID: 14270
			private const int S_OK = 0;

			// Token: 0x040037BF RID: 14271
			private const int E_NOTIMPL = -2147467263;

			// Token: 0x040037C0 RID: 14272
			private const int CLASS_E_NOTLICENSED = -2147221230;

			// Token: 0x040037C1 RID: 14273
			private const int E_FAIL = -2147483640;

			// Token: 0x040037C2 RID: 14274
			private DesigntimeLicenseContext helperContext;

			// Token: 0x040037C3 RID: 14275
			private LicenseContext savedLicenseContext;

			// Token: 0x040037C4 RID: 14276
			private Type savedType;

			// Token: 0x02000933 RID: 2355
			internal class CLRLicenseContext : LicenseContext
			{
				// Token: 0x060046A1 RID: 18081 RVA: 0x00126B4C File Offset: 0x00124D4C
				public CLRLicenseContext(LicenseUsageMode usageMode, Type type)
				{
					this.usageMode = usageMode;
					this.type = type;
				}

				// Token: 0x17000FEA RID: 4074
				// (get) Token: 0x060046A2 RID: 18082 RVA: 0x00126B62 File Offset: 0x00124D62
				public override LicenseUsageMode UsageMode
				{
					get
					{
						return this.usageMode;
					}
				}

				// Token: 0x060046A3 RID: 18083 RVA: 0x00126B6A File Offset: 0x00124D6A
				public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
				{
					if (!(type == this.type))
					{
						return null;
					}
					return this.key;
				}

				// Token: 0x060046A4 RID: 18084 RVA: 0x00126B82 File Offset: 0x00124D82
				public override void SetSavedLicenseKey(Type type, string key)
				{
					if (type == this.type)
					{
						this.key = key;
					}
				}

				// Token: 0x04003DD4 RID: 15828
				private LicenseUsageMode usageMode;

				// Token: 0x04003DD5 RID: 15829
				private Type type;

				// Token: 0x04003DD6 RID: 15830
				private string key;
			}
		}
	}
}

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	// Token: 0x02000166 RID: 358
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNclNativeMethods
	{
		// Token: 0x06000CF5 RID: 3317
		[DllImport("kernel32.dll")]
		internal static extern IntPtr CreateSemaphore([In] IntPtr lpSemaphoreAttributes, [In] int lInitialCount, [In] int lMaximumCount, [In] IntPtr lpName);

		// Token: 0x06000CF6 RID: 3318
		[DllImport("kernel32.dll")]
		internal static extern bool ReleaseSemaphore([In] IntPtr hSemaphore, [In] int lReleaseCount, [In] IntPtr lpPreviousCount);

		// Token: 0x06000CF7 RID: 3319
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern uint GetCurrentThreadId();

		// Token: 0x06000CF8 RID: 3320
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal unsafe static extern uint CancelIoEx(CriticalHandle handle, NativeOverlapped* overlapped);

		// Token: 0x06000CF9 RID: 3321
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern uint CancelIoEx(SafeHandle handle, IntPtr overlapped);

		// Token: 0x06000CFA RID: 3322
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
		internal static extern bool SetFileCompletionNotificationModes(CriticalHandle handle, UnsafeNclNativeMethods.FileCompletionNotificationModes modes);

		// Token: 0x06000CFB RID: 3323
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetProcessHeap();

		// Token: 0x06000CFC RID: 3324
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool HeapFree([In] IntPtr hHeap, [In] uint dwFlags, [In] IntPtr lpMem);

		// Token: 0x06000CFD RID: 3325
		[SecurityCritical]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetProcAddress(SafeLoadLibrary hModule, string entryPoint);

		// Token: 0x06000CFE RID: 3326
		[SecurityCritical]
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string entryPoint);

		// Token: 0x06000CFF RID: 3327
		[DllImport("kernel32.dll", ExactSpelling = true)]
		internal static extern void DebugBreak();

		// Token: 0x06000D00 RID: 3328
		[DllImport("ole32.dll", PreserveSig = false)]
		public static extern void CoCreateInstance([In] ref Guid clsid, IntPtr pUnkOuter, int context, [In] ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object o);

		// Token: 0x040011D2 RID: 4562
		private const string KERNEL32 = "kernel32.dll";

		// Token: 0x040011D3 RID: 4563
		private const string WS2_32 = "ws2_32.dll";

		// Token: 0x040011D4 RID: 4564
		private const string SECUR32 = "secur32.dll";

		// Token: 0x040011D5 RID: 4565
		private const string CRYPT32 = "crypt32.dll";

		// Token: 0x040011D6 RID: 4566
		private const string ADVAPI32 = "advapi32.dll";

		// Token: 0x040011D7 RID: 4567
		private const string HTTPAPI = "httpapi.dll";

		// Token: 0x040011D8 RID: 4568
		private const string SCHANNEL = "schannel.dll";

		// Token: 0x040011D9 RID: 4569
		private const string RASAPI32 = "rasapi32.dll";

		// Token: 0x040011DA RID: 4570
		private const string WININET = "wininet.dll";

		// Token: 0x040011DB RID: 4571
		private const string WINHTTP = "winhttp.dll";

		// Token: 0x040011DC RID: 4572
		private const string BCRYPT = "bcrypt.dll";

		// Token: 0x040011DD RID: 4573
		private const string USER32 = "user32.dll";

		// Token: 0x040011DE RID: 4574
		private const string TOKENBINDING = "tokenbinding.dll";

		// Token: 0x040011DF RID: 4575
		private const string OLE32 = "ole32.dll";

		// Token: 0x040011E0 RID: 4576
		internal const int CLSCTX_SERVER = 21;

		// Token: 0x02000712 RID: 1810
		internal static class ErrorCodes
		{
			// Token: 0x04003116 RID: 12566
			internal const uint ERROR_SUCCESS = 0U;

			// Token: 0x04003117 RID: 12567
			internal const uint ERROR_HANDLE_EOF = 38U;

			// Token: 0x04003118 RID: 12568
			internal const uint ERROR_NOT_SUPPORTED = 50U;

			// Token: 0x04003119 RID: 12569
			internal const uint ERROR_INVALID_PARAMETER = 87U;

			// Token: 0x0400311A RID: 12570
			internal const uint ERROR_ALREADY_EXISTS = 183U;

			// Token: 0x0400311B RID: 12571
			internal const uint ERROR_MORE_DATA = 234U;

			// Token: 0x0400311C RID: 12572
			internal const uint ERROR_OPERATION_ABORTED = 995U;

			// Token: 0x0400311D RID: 12573
			internal const uint ERROR_IO_PENDING = 997U;

			// Token: 0x0400311E RID: 12574
			internal const uint ERROR_NOT_FOUND = 1168U;

			// Token: 0x0400311F RID: 12575
			internal const uint ERROR_CONNECTION_INVALID = 1229U;
		}

		// Token: 0x02000713 RID: 1811
		internal static class NTStatus
		{
			// Token: 0x04003120 RID: 12576
			internal const uint STATUS_SUCCESS = 0U;

			// Token: 0x04003121 RID: 12577
			internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524U;
		}

		// Token: 0x02000714 RID: 1812
		[Flags]
		internal enum FileCompletionNotificationModes : byte
		{
			// Token: 0x04003123 RID: 12579
			None = 0,
			// Token: 0x04003124 RID: 12580
			SkipCompletionPortOnSuccess = 1,
			// Token: 0x04003125 RID: 12581
			SkipSetEventOnHandle = 2
		}

		// Token: 0x02000715 RID: 1813
		[SuppressUnmanagedCodeSecurity]
		internal static class RegistryHelper
		{
			// Token: 0x0600409E RID: 16542
			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

			// Token: 0x0600409F RID: 16543
			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegOpenKeyEx(SafeRegistryHandle key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey);

			// Token: 0x060040A0 RID: 16544
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern uint RegCloseKey(IntPtr key);

			// Token: 0x060040A1 RID: 16545
			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern uint RegNotifyChangeKeyValue(SafeRegistryHandle key, bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async);

			// Token: 0x060040A2 RID: 16546
			[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern uint RegOpenCurrentUser(uint samDesired, out SafeRegistryHandle resultKey);

			// Token: 0x060040A3 RID: 16547
			[DllImport("advapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern uint RegQueryValueEx(SafeRegistryHandle key, string valueName, IntPtr reserved, out uint type, [Out] byte[] data, [In] [Out] ref uint size);

			// Token: 0x04003126 RID: 12582
			internal const uint REG_NOTIFY_CHANGE_LAST_SET = 4U;

			// Token: 0x04003127 RID: 12583
			internal const uint REG_BINARY = 3U;

			// Token: 0x04003128 RID: 12584
			internal const uint KEY_READ = 131097U;

			// Token: 0x04003129 RID: 12585
			internal static readonly IntPtr HKEY_CURRENT_USER = (IntPtr)(-2147483647);

			// Token: 0x0400312A RID: 12586
			internal static readonly IntPtr HKEY_LOCAL_MACHINE = (IntPtr)(-2147483646);
		}

		// Token: 0x02000716 RID: 1814
		[SuppressUnmanagedCodeSecurity]
		internal class RasHelper
		{
			// Token: 0x060040A5 RID: 16549 RVA: 0x0010EF20 File Offset: 0x0010D120
			static RasHelper()
			{
				if (ComNetOS.InstallationType == WindowsInstallationType.ServerCore)
				{
					UnsafeNclNativeMethods.RasHelper.s_RasSupported = false;
				}
				else
				{
					UnsafeNclNativeMethods.RasHelper.s_RasSupported = true;
				}
				if (Logging.On)
				{
					Logging.PrintInfo(Logging.Web, global::System.SR.GetString("net_log_proxy_ras_supported", new object[] { UnsafeNclNativeMethods.RasHelper.s_RasSupported }));
				}
			}

			// Token: 0x060040A6 RID: 16550 RVA: 0x0010EF74 File Offset: 0x0010D174
			internal RasHelper()
			{
				if (!UnsafeNclNativeMethods.RasHelper.s_RasSupported)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_log_proxy_ras_notsupported_exception"));
				}
				this.m_RasEvent = new ManualResetEvent(false);
				uint num = UnsafeNclNativeMethods.RasHelper.RasConnectionNotification((IntPtr)(-1), this.m_RasEvent.SafeWaitHandle, 3U);
				if (num != 0U)
				{
					this.m_Suppressed = true;
					this.m_RasEvent.Close();
					this.m_RasEvent = null;
				}
			}

			// Token: 0x17000EEF RID: 3823
			// (get) Token: 0x060040A7 RID: 16551 RVA: 0x0010EFDE File Offset: 0x0010D1DE
			internal static bool RasSupported
			{
				get
				{
					return UnsafeNclNativeMethods.RasHelper.s_RasSupported;
				}
			}

			// Token: 0x17000EF0 RID: 3824
			// (get) Token: 0x060040A8 RID: 16552 RVA: 0x0010EFE8 File Offset: 0x0010D1E8
			internal bool HasChanged
			{
				get
				{
					if (this.m_Suppressed)
					{
						return false;
					}
					ManualResetEvent rasEvent = this.m_RasEvent;
					if (rasEvent == null)
					{
						throw new ObjectDisposedException(base.GetType().FullName);
					}
					return rasEvent.WaitOne(0, false);
				}
			}

			// Token: 0x060040A9 RID: 16553 RVA: 0x0010F024 File Offset: 0x0010D224
			internal void Reset()
			{
				if (!this.m_Suppressed)
				{
					ManualResetEvent rasEvent = this.m_RasEvent;
					if (rasEvent == null)
					{
						throw new ObjectDisposedException(base.GetType().FullName);
					}
					rasEvent.Reset();
				}
			}

			// Token: 0x060040AA RID: 16554 RVA: 0x0010F05C File Offset: 0x0010D25C
			internal static string GetCurrentConnectoid()
			{
				uint num = (uint)Marshal.SizeOf(typeof(UnsafeNclNativeMethods.RasHelper.RASCONN));
				if (!UnsafeNclNativeMethods.RasHelper.s_RasSupported)
				{
					return null;
				}
				uint num2 = 4U;
				UnsafeNclNativeMethods.RasHelper.RASCONN[] array;
				checked
				{
					uint num4;
					for (;;)
					{
						uint num3 = num * num2;
						array = new UnsafeNclNativeMethods.RasHelper.RASCONN[num2];
						array[0].dwSize = num;
						num4 = UnsafeNclNativeMethods.RasHelper.RasEnumConnections(array, ref num3, ref num2);
						if (num4 != 603U)
						{
							break;
						}
						num2 = (num3 + num - 1U) / num;
					}
					if (num2 == 0U || num4 != 0U)
					{
						return null;
					}
				}
				for (uint num5 = 0U; num5 < num2; num5 += 1U)
				{
					UnsafeNclNativeMethods.RasHelper.RASCONNSTATUS rasconnstatus = default(UnsafeNclNativeMethods.RasHelper.RASCONNSTATUS);
					rasconnstatus.dwSize = (uint)Marshal.SizeOf(rasconnstatus);
					if (UnsafeNclNativeMethods.RasHelper.RasGetConnectStatus(array[(int)num5].hrasconn, ref rasconnstatus) == 0U && rasconnstatus.rasconnstate == UnsafeNclNativeMethods.RasHelper.RASCONNSTATE.RASCS_Connected)
					{
						return array[(int)num5].szEntryName;
					}
				}
				return null;
			}

			// Token: 0x060040AB RID: 16555
			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasEnumConnections([In] [Out] UnsafeNclNativeMethods.RasHelper.RASCONN[] lprasconn, ref uint lpcb, ref uint lpcConnections);

			// Token: 0x060040AC RID: 16556
			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasGetConnectStatus([In] IntPtr hrasconn, [In] [Out] ref UnsafeNclNativeMethods.RasHelper.RASCONNSTATUS lprasconnstatus);

			// Token: 0x060040AD RID: 16557
			[DllImport("rasapi32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ThrowOnUnmappableChar = true)]
			private static extern uint RasConnectionNotification([In] IntPtr hrasconn, [In] SafeWaitHandle hEvent, uint dwFlags);

			// Token: 0x0400312B RID: 12587
			private static readonly bool s_RasSupported;

			// Token: 0x0400312C RID: 12588
			private ManualResetEvent m_RasEvent;

			// Token: 0x0400312D RID: 12589
			private bool m_Suppressed;

			// Token: 0x0400312E RID: 12590
			private const int RAS_MaxEntryName = 256;

			// Token: 0x0400312F RID: 12591
			private const int RAS_MaxDeviceType = 16;

			// Token: 0x04003130 RID: 12592
			private const int RAS_MaxDeviceName = 128;

			// Token: 0x04003131 RID: 12593
			private const int RAS_MaxPhoneNumber = 128;

			// Token: 0x04003132 RID: 12594
			private const int RAS_MaxCallbackNumber = 128;

			// Token: 0x04003133 RID: 12595
			private const uint RASCN_Connection = 1U;

			// Token: 0x04003134 RID: 12596
			private const uint RASCN_Disconnection = 2U;

			// Token: 0x04003135 RID: 12597
			private const int UNLEN = 256;

			// Token: 0x04003136 RID: 12598
			private const int PWLEN = 256;

			// Token: 0x04003137 RID: 12599
			private const int DNLEN = 15;

			// Token: 0x04003138 RID: 12600
			private const int MAX_PATH = 260;

			// Token: 0x04003139 RID: 12601
			private const uint RASBASE = 600U;

			// Token: 0x0400313A RID: 12602
			private const uint ERROR_DIAL_ALREADY_IN_PROGRESS = 756U;

			// Token: 0x0400313B RID: 12603
			private const uint ERROR_BUFFER_TOO_SMALL = 603U;

			// Token: 0x0400313C RID: 12604
			private const int RASCS_PAUSED = 4096;

			// Token: 0x0400313D RID: 12605
			private const int RASCS_DONE = 8192;

			// Token: 0x020008CE RID: 2254
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
			private struct RASCONN
			{
				// Token: 0x04003B63 RID: 15203
				internal uint dwSize;

				// Token: 0x04003B64 RID: 15204
				internal IntPtr hrasconn;

				// Token: 0x04003B65 RID: 15205
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szEntryName;

				// Token: 0x04003B66 RID: 15206
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
				internal string szDeviceType;

				// Token: 0x04003B67 RID: 15207
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szDeviceName;
			}

			// Token: 0x020008CF RID: 2255
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			private struct RASCONNSTATUS
			{
				// Token: 0x04003B68 RID: 15208
				internal uint dwSize;

				// Token: 0x04003B69 RID: 15209
				internal UnsafeNclNativeMethods.RasHelper.RASCONNSTATE rasconnstate;

				// Token: 0x04003B6A RID: 15210
				internal uint dwError;

				// Token: 0x04003B6B RID: 15211
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
				internal string szDeviceType;

				// Token: 0x04003B6C RID: 15212
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szDeviceName;
			}

			// Token: 0x020008D0 RID: 2256
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			private struct RASDIALPARAMS
			{
				// Token: 0x04003B6D RID: 15213
				internal uint dwSize;

				// Token: 0x04003B6E RID: 15214
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szEntryName;

				// Token: 0x04003B6F RID: 15215
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szPhoneNumber;

				// Token: 0x04003B70 RID: 15216
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
				internal string szCallbackNumber;

				// Token: 0x04003B71 RID: 15217
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szUserName;

				// Token: 0x04003B72 RID: 15218
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
				internal string szPassword;

				// Token: 0x04003B73 RID: 15219
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
				internal string szDomain;
			}

			// Token: 0x020008D1 RID: 2257
			private enum RASCONNSTATE
			{
				// Token: 0x04003B75 RID: 15221
				RASCS_OpenPort,
				// Token: 0x04003B76 RID: 15222
				RASCS_PortOpened,
				// Token: 0x04003B77 RID: 15223
				RASCS_ConnectDevice,
				// Token: 0x04003B78 RID: 15224
				RASCS_DeviceConnected,
				// Token: 0x04003B79 RID: 15225
				RASCS_AllDevicesConnected,
				// Token: 0x04003B7A RID: 15226
				RASCS_Authenticate,
				// Token: 0x04003B7B RID: 15227
				RASCS_AuthNotify,
				// Token: 0x04003B7C RID: 15228
				RASCS_AuthRetry,
				// Token: 0x04003B7D RID: 15229
				RASCS_AuthCallback,
				// Token: 0x04003B7E RID: 15230
				RASCS_AuthChangePassword,
				// Token: 0x04003B7F RID: 15231
				RASCS_AuthProject,
				// Token: 0x04003B80 RID: 15232
				RASCS_AuthLinkSpeed,
				// Token: 0x04003B81 RID: 15233
				RASCS_AuthAck,
				// Token: 0x04003B82 RID: 15234
				RASCS_ReAuthenticate,
				// Token: 0x04003B83 RID: 15235
				RASCS_Authenticated,
				// Token: 0x04003B84 RID: 15236
				RASCS_PrepareForCallback,
				// Token: 0x04003B85 RID: 15237
				RASCS_WaitForModemReset,
				// Token: 0x04003B86 RID: 15238
				RASCS_WaitForCallback,
				// Token: 0x04003B87 RID: 15239
				RASCS_Projected,
				// Token: 0x04003B88 RID: 15240
				RASCS_StartAuthentication,
				// Token: 0x04003B89 RID: 15241
				RASCS_CallbackComplete,
				// Token: 0x04003B8A RID: 15242
				RASCS_LogonNetwork,
				// Token: 0x04003B8B RID: 15243
				RASCS_SubEntryConnected,
				// Token: 0x04003B8C RID: 15244
				RASCS_SubEntryDisconnected,
				// Token: 0x04003B8D RID: 15245
				RASCS_Interactive = 4096,
				// Token: 0x04003B8E RID: 15246
				RASCS_RetryAuthentication,
				// Token: 0x04003B8F RID: 15247
				RASCS_CallbackSetByCaller,
				// Token: 0x04003B90 RID: 15248
				RASCS_PasswordExpired,
				// Token: 0x04003B91 RID: 15249
				RASCS_InvokeEapUI,
				// Token: 0x04003B92 RID: 15250
				RASCS_Connected = 8192,
				// Token: 0x04003B93 RID: 15251
				RASCS_Disconnected
			}
		}

		// Token: 0x02000717 RID: 1815
		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles_SECURITY
		{
			// Token: 0x060040AE RID: 16558
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);

			// Token: 0x060040AF RID: 16559
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);

			// Token: 0x060040B0 RID: 16560
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);

			// Token: 0x060040B1 RID: 16561
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In] [Out] SecurityBufferDescriptor outputBuffer, [In] [Out] ref ContextFlags attributes, out long timeStamp);

			// Token: 0x060040B2 RID: 16562
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int QueryContextAttributesW(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);

			// Token: 0x060040B3 RID: 16563
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int SetContextAttributesW(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] byte[] buffer, [In] int bufferSize);

			// Token: 0x060040B4 RID: 16564
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int EnumerateSecurityPackagesW(out int pkgnum, out SafeFreeContextBuffer_SECURITY handle);

			// Token: 0x060040B5 RID: 16565
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentity authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			// Token: 0x060040B6 RID: 16566
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			// Token: 0x060040B7 RID: 16567
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] SafeSspiAuthDataHandle authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			// Token: 0x060040B8 RID: 16568
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			// Token: 0x060040B9 RID: 16569
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential2 authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);

			// Token: 0x060040BA RID: 16570
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int InitializeSecurityContextW(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In] [Out] SecurityBufferDescriptor outputBuffer, [In] [Out] ref ContextFlags attributes, out long timeStamp);

			// Token: 0x060040BB RID: 16571
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int CompleteAuthToken([In] void* inContextPtr, [In] [Out] SecurityBufferDescriptor inputBuffers);

			// Token: 0x060040BC RID: 16572
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int ApplyControlToken([In] void* inContextPtr, [In] [Out] SecurityBufferDescriptor inputBuffers);
		}

		// Token: 0x02000718 RID: 1816
		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandlesSafeOverlappedFree
		{
			// Token: 0x060040BD RID: 16573
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeOverlappedFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
		}

		// Token: 0x02000719 RID: 1817
		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandlesXPOrLater
		{
			// Token: 0x060040BE RID: 16574
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern int GetAddrInfoW([In] string nodename, [In] string servicename, [In] ref AddressInfo hints, out SafeFreeAddrInfo handle);

			// Token: 0x060040BF RID: 16575
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern void freeaddrinfo([In] IntPtr info);
		}

		// Token: 0x0200071A RID: 1818
		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNetHandles
		{
			// Token: 0x060040C0 RID: 16576
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int QuerySecurityContextToken(ref SSPIHandle phContext, out SafeCloseHandle handle);

			// Token: 0x060040C1 RID: 16577
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCreateRequestQueue(UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION version, string pName, Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES pSecurityAttributes, uint flags, out HttpRequestQueueV2Handle pReqQueueHandle);

			// Token: 0x060040C2 RID: 16578
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCloseRequestQueue(IntPtr pReqQueueHandle);

			// Token: 0x060040C3 RID: 16579
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern bool CloseHandle(IntPtr handle);

			// Token: 0x060040C4 RID: 16580
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeLocalFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);

			// Token: 0x060040C5 RID: 16581
			[DllImport("kernel32.dll", EntryPoint = "LocalAlloc", SetLastError = true)]
			internal static extern SafeLocalFreeChannelBinding LocalAllocChannelBinding(int uFlags, UIntPtr sizetdwBytes);

			// Token: 0x060040C6 RID: 16582
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern IntPtr LocalFree(IntPtr handle);

			// Token: 0x060040C7 RID: 16583
			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern SafeLoadLibrary LoadLibraryExW([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);

			// Token: 0x060040C8 RID: 16584
			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			public static extern IntPtr GetModuleHandleW(string modName);

			// Token: 0x060040C9 RID: 16585
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern bool FreeLibrary([In] IntPtr hModule);

			// Token: 0x060040CA RID: 16586
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern void CertFreeCertificateChain([In] IntPtr pChainContext);

			// Token: 0x060040CB RID: 16587
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern void CertFreeCertificateChainList([In] IntPtr ppChainContext);

			// Token: 0x060040CC RID: 16588
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern bool CertFreeCertificateContext([In] IntPtr certContext);

			// Token: 0x060040CD RID: 16589
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern IntPtr GlobalFree(IntPtr handle);

			// Token: 0x060040CE RID: 16590
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SafeCloseSocket.InnerSafeCloseSocket accept([In] IntPtr socketHandle, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x060040CF RID: 16591
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SocketError closesocket([In] IntPtr socketHandle);

			// Token: 0x060040D0 RID: 16592
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SocketError ioctlsocket([In] IntPtr handle, [In] int cmd, [In] [Out] ref int argp);

			// Token: 0x060040D1 RID: 16593
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SocketError WSAEventSelect([In] IntPtr handle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);

			// Token: 0x060040D2 RID: 16594
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("ws2_32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SocketError setsockopt([In] IntPtr handle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);

			// Token: 0x060040D3 RID: 16595
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("wininet.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool RetrieveUrlCacheEntryFileW([In] char* urlName, [In] byte* entryPtr, [In] [Out] ref int entryBufSize, [In] int dwReserved);

			// Token: 0x060040D4 RID: 16596
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("wininet.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool UnlockUrlCacheEntryFileW([In] char* urlName, [In] int dwReserved);
		}

		// Token: 0x0200071B RID: 1819
		[SuppressUnmanagedCodeSecurity]
		internal static class OSSOCK
		{
			// Token: 0x060040D5 RID: 16597
			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] IntPtr protocolInfo, [In] uint group, [In] SocketConstructorFlags flags);

			// Token: 0x060040D6 RID: 16598
			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal unsafe static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] byte* pinnedBuffer, [In] uint group, [In] SocketConstructorFlags flags);

			// Token: 0x060040D7 RID: 16599
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError WSAStartup([In] short wVersionRequested, out WSAData lpWSAData);

			// Token: 0x060040D8 RID: 16600
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError ioctlsocket([In] SafeCloseSocket socketHandle, [In] int cmd, [In] [Out] ref int argp);

			// Token: 0x060040D9 RID: 16601
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern IntPtr gethostbyname([In] string host);

			// Token: 0x060040DA RID: 16602
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern IntPtr gethostbyaddr([In] ref int addr, [In] int len, [In] ProtocolFamily type);

			// Token: 0x060040DB RID: 16603
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError gethostname([Out] StringBuilder hostName, [In] int bufferLength);

			// Token: 0x060040DC RID: 16604
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getpeername([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x060040DD RID: 16605
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out int optionValue, [In] [Out] ref int optionLength);

			// Token: 0x060040DE RID: 16606
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [Out] byte[] optionValue, [In] [Out] ref int optionLength);

			// Token: 0x060040DF RID: 16607
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out Linger optionValue, [In] [Out] ref int optionLength);

			// Token: 0x060040E0 RID: 16608
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPMulticastRequest optionValue, [In] [Out] ref int optionLength);

			// Token: 0x060040E1 RID: 16609
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPv6MulticastRequest optionValue, [In] [Out] ref int optionLength);

			// Token: 0x060040E2 RID: 16610
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref int optionValue, [In] int optionLength);

			// Token: 0x060040E3 RID: 16611
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] byte[] optionValue, [In] int optionLength);

			// Token: 0x060040E4 RID: 16612
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IntPtr pointer, [In] int optionLength);

			// Token: 0x060040E5 RID: 16613
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);

			// Token: 0x060040E6 RID: 16614
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPMulticastRequest mreq, [In] int optionLength);

			// Token: 0x060040E7 RID: 16615
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPv6MulticastRequest mreq, [In] int optionLength);

			// Token: 0x060040E8 RID: 16616
			[DllImport("mswsock.dll", SetLastError = true)]
			internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			// Token: 0x060040E9 RID: 16617
			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile2([In] SafeCloseSocket socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			// Token: 0x060040EA RID: 16618
			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile_Blocking([In] IntPtr socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			// Token: 0x060040EB RID: 16619
			[DllImport("mswsock.dll", EntryPoint = "TransmitFile", SetLastError = true)]
			internal static extern bool TransmitFile_Blocking2([In] IntPtr socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);

			// Token: 0x060040EC RID: 16620
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int send([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);

			// Token: 0x060040ED RID: 16621
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int recv([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);

			// Token: 0x060040EE RID: 16622
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError listen([In] SafeCloseSocket socketHandle, [In] int backlog);

			// Token: 0x060040EF RID: 16623
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError bind([In] SafeCloseSocket socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize);

			// Token: 0x060040F0 RID: 16624
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError shutdown([In] SafeCloseSocket socketHandle, [In] int how);

			// Token: 0x060040F1 RID: 16625
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int sendto([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [In] byte[] socketAddress, [In] int socketAddressSize);

			// Token: 0x060040F2 RID: 16626
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int recvfrom([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x060040F3 RID: 16627
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError getsockname([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x060040F4 RID: 16628
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern int select([In] int ignoredParameter, [In] [Out] IntPtr[] readfds, [In] [Out] IntPtr[] writefds, [In] [Out] IntPtr[] exceptfds, [In] ref TimeValue timeout);

			// Token: 0x060040F5 RID: 16629
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern int select([In] int ignoredParameter, [In] [Out] IntPtr[] readfds, [In] [Out] IntPtr[] writefds, [In] [Out] IntPtr[] exceptfds, [In] IntPtr nullTimeout);

			// Token: 0x060040F6 RID: 16630
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAConnect([In] IntPtr socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr inBuffer, [In] IntPtr outBuffer, [In] IntPtr sQOS, [In] IntPtr gQOS);

			// Token: 0x060040F7 RID: 16631
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040F8 RID: 16632
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040F9 RID: 16633
			[DllImport("ws2_32.dll", EntryPoint = "WSASend", SetLastError = true)]
			internal static extern SocketError WSASend_Blocking([In] IntPtr socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FA RID: 16634
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FB RID: 16635
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeNativeOverlapped overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FC RID: 16636
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] [Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FD RID: 16637
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In] [Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In] [Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FE RID: 16638
			[DllImport("ws2_32.dll", EntryPoint = "WSARecv", SetLastError = true)]
			internal static extern SocketError WSARecv_Blocking([In] IntPtr socketHandle, [In] [Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In] [Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x060040FF RID: 16639
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] [Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x06004100 RID: 16640
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In] [Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In] [Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeNativeOverlapped overlapped, [In] IntPtr completionRoutine);

			// Token: 0x06004101 RID: 16641
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] SafeHandle Event, [In] AsyncEventBits NetworkEvents);

			// Token: 0x06004102 RID: 16642
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);

			// Token: 0x06004103 RID: 16643
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAIoctl([In] SafeCloseSocket socketHandle, [In] int ioControlCode, [In] [Out] ref Guid guid, [In] int guidSize, out IntPtr funcPtr, [In] int funcPtrSize, out int bytesTransferred, [In] IntPtr shouldBeNull, [In] IntPtr shouldBeNull2);

			// Token: 0x06004104 RID: 16644
			[DllImport("ws2_32.dll", EntryPoint = "WSAIoctl", SetLastError = true)]
			internal static extern SocketError WSAIoctl_Blocking([In] IntPtr socketHandle, [In] int ioControlCode, [In] byte[] inBuffer, [In] int inBufferSize, [Out] byte[] outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x06004105 RID: 16645
			[DllImport("ws2_32.dll", EntryPoint = "WSAIoctl", SetLastError = true)]
			internal static extern SocketError WSAIoctl_Blocking_Internal([In] IntPtr socketHandle, [In] uint ioControlCode, [In] IntPtr inBuffer, [In] int inBufferSize, [Out] IntPtr outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);

			// Token: 0x06004106 RID: 16646
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern SocketError WSAEnumNetworkEvents([In] SafeCloseSocket socketHandle, [In] SafeWaitHandle Event, [In] [Out] ref NetworkEvents networkEvents);

			// Token: 0x06004107 RID: 16647
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal unsafe static extern int WSADuplicateSocket([In] SafeCloseSocket socketHandle, [In] uint targetProcessID, [In] byte* pinnedBuffer);

			// Token: 0x06004108 RID: 16648
			[DllImport("ws2_32.dll", SetLastError = true)]
			internal static extern bool WSAGetOverlappedResult([In] SafeCloseSocket socketHandle, [In] SafeHandle overlapped, out uint bytesTransferred, [In] bool wait, out SocketFlags socketFlags);

			// Token: 0x06004109 RID: 16649
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = false)]
			internal static extern SocketError WSAStringToAddress([In] string addressString, [In] AddressFamily addressFamily, [In] IntPtr lpProtocolInfo, [Out] byte[] socketAddress, [In] [Out] ref int socketAddressSize);

			// Token: 0x0600410A RID: 16650
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Ansi, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError WSAAddressToString([In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr lpProtocolInfo, [Out] StringBuilder addressString, [In] [Out] ref int addressStringLength);

			// Token: 0x0600410B RID: 16651
			[DllImport("ws2_32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
			internal static extern SocketError GetNameInfoW([In] byte[] sa, [In] int salen, [In] [Out] StringBuilder host, [In] int hostlen, [In] [Out] StringBuilder serv, [In] int servlen, [In] int flags);

			// Token: 0x0600410C RID: 16652
			[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			internal static extern int WSAEnumProtocols([MarshalAs(UnmanagedType.LPArray)] [In] int[] lpiProtocols, [In] SafeLocalFree lpProtocolBuffer, [In] [Out] ref uint lpdwBufferLength);

			// Token: 0x0400313E RID: 12606
			private const string WS2_32 = "ws2_32.dll";

			// Token: 0x0400313F RID: 12607
			private const string mswsock = "mswsock.dll";

			// Token: 0x020008D2 RID: 2258
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct WSAPROTOCOLCHAIN
			{
				// Token: 0x04003B94 RID: 15252
				internal int ChainLen;

				// Token: 0x04003B95 RID: 15253
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
				internal uint[] ChainEntries;
			}

			// Token: 0x020008D3 RID: 2259
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct WSAPROTOCOL_INFO
			{
				// Token: 0x04003B96 RID: 15254
				internal uint dwServiceFlags1;

				// Token: 0x04003B97 RID: 15255
				internal uint dwServiceFlags2;

				// Token: 0x04003B98 RID: 15256
				internal uint dwServiceFlags3;

				// Token: 0x04003B99 RID: 15257
				internal uint dwServiceFlags4;

				// Token: 0x04003B9A RID: 15258
				internal uint dwProviderFlags;

				// Token: 0x04003B9B RID: 15259
				private Guid ProviderId;

				// Token: 0x04003B9C RID: 15260
				internal uint dwCatalogEntryId;

				// Token: 0x04003B9D RID: 15261
				private UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOLCHAIN ProtocolChain;

				// Token: 0x04003B9E RID: 15262
				internal int iVersion;

				// Token: 0x04003B9F RID: 15263
				internal AddressFamily iAddressFamily;

				// Token: 0x04003BA0 RID: 15264
				internal int iMaxSockAddr;

				// Token: 0x04003BA1 RID: 15265
				internal int iMinSockAddr;

				// Token: 0x04003BA2 RID: 15266
				internal int iSocketType;

				// Token: 0x04003BA3 RID: 15267
				internal int iProtocol;

				// Token: 0x04003BA4 RID: 15268
				internal int iProtocolMaxOffset;

				// Token: 0x04003BA5 RID: 15269
				internal int iNetworkByteOrder;

				// Token: 0x04003BA6 RID: 15270
				internal int iSecurityScheme;

				// Token: 0x04003BA7 RID: 15271
				internal uint dwMessageSize;

				// Token: 0x04003BA8 RID: 15272
				internal uint dwProviderReserved;

				// Token: 0x04003BA9 RID: 15273
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
				internal string szProtocol;
			}

			// Token: 0x020008D4 RID: 2260
			internal struct ControlData
			{
				// Token: 0x04003BAA RID: 15274
				internal UIntPtr length;

				// Token: 0x04003BAB RID: 15275
				internal uint level;

				// Token: 0x04003BAC RID: 15276
				internal uint type;

				// Token: 0x04003BAD RID: 15277
				internal uint address;

				// Token: 0x04003BAE RID: 15278
				internal uint index;
			}

			// Token: 0x020008D5 RID: 2261
			internal struct ControlDataIPv6
			{
				// Token: 0x04003BAF RID: 15279
				internal UIntPtr length;

				// Token: 0x04003BB0 RID: 15280
				internal uint level;

				// Token: 0x04003BB1 RID: 15281
				internal uint type;

				// Token: 0x04003BB2 RID: 15282
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
				internal byte[] address;

				// Token: 0x04003BB3 RID: 15283
				internal uint index;
			}

			// Token: 0x020008D6 RID: 2262
			internal struct WSAMsg
			{
				// Token: 0x04003BB4 RID: 15284
				internal IntPtr socketAddress;

				// Token: 0x04003BB5 RID: 15285
				internal uint addressLength;

				// Token: 0x04003BB6 RID: 15286
				internal IntPtr buffers;

				// Token: 0x04003BB7 RID: 15287
				internal uint count;

				// Token: 0x04003BB8 RID: 15288
				internal WSABuffer controlBuffer;

				// Token: 0x04003BB9 RID: 15289
				internal SocketFlags flags;
			}

			// Token: 0x020008D7 RID: 2263
			[Flags]
			internal enum TransmitPacketsElementFlags : uint
			{
				// Token: 0x04003BBB RID: 15291
				None = 0U,
				// Token: 0x04003BBC RID: 15292
				Memory = 1U,
				// Token: 0x04003BBD RID: 15293
				File = 2U,
				// Token: 0x04003BBE RID: 15294
				EndOfPacket = 4U
			}

			// Token: 0x020008D8 RID: 2264
			[StructLayout(LayoutKind.Explicit)]
			internal struct TransmitPacketsElement
			{
				// Token: 0x04003BBF RID: 15295
				[FieldOffset(0)]
				internal UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags flags;

				// Token: 0x04003BC0 RID: 15296
				[FieldOffset(4)]
				internal uint length;

				// Token: 0x04003BC1 RID: 15297
				[FieldOffset(8)]
				internal long fileOffset;

				// Token: 0x04003BC2 RID: 15298
				[FieldOffset(8)]
				internal IntPtr buffer;

				// Token: 0x04003BC3 RID: 15299
				[FieldOffset(16)]
				internal IntPtr fileHandle;
			}

			// Token: 0x020008D9 RID: 2265
			internal struct SOCKET_ADDRESS
			{
				// Token: 0x04003BC4 RID: 15300
				internal IntPtr lpSockAddr;

				// Token: 0x04003BC5 RID: 15301
				internal int iSockaddrLength;
			}

			// Token: 0x020008DA RID: 2266
			internal struct SOCKET_ADDRESS_LIST
			{
				// Token: 0x04003BC6 RID: 15302
				internal int iAddressCount;

				// Token: 0x04003BC7 RID: 15303
				internal UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS Addresses;
			}

			// Token: 0x020008DB RID: 2267
			internal struct TransmitFileBuffersStruct
			{
				// Token: 0x04003BC8 RID: 15304
				internal IntPtr preBuffer;

				// Token: 0x04003BC9 RID: 15305
				internal int preBufferLength;

				// Token: 0x04003BCA RID: 15306
				internal IntPtr postBuffer;

				// Token: 0x04003BCB RID: 15307
				internal int postBufferLength;
			}
		}

		// Token: 0x0200071C RID: 1820
		[SuppressUnmanagedCodeSecurity]
		internal static class NativePKI
		{
			// Token: 0x0600410D RID: 16653
			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int CertVerifyCertificateChainPolicy([In] IntPtr policy, [In] SafeFreeCertChain chainContext, [In] ref ChainPolicyParameter cpp, [In] [Out] ref ChainPolicyStatus ps);

			// Token: 0x0600410E RID: 16654
			[DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true)]
			private static extern bool CertSelectCertificateChains([In] IntPtr pSelectionContext, [In] UnsafeNclNativeMethods.NativePKI.CertificateSelect flags, [In] IntPtr pChainParameters, [In] int cCriteria, [In] SafeCertSelectCritera rgpCriteria, [In] IntPtr hStore, out int pcSelection, out SafeFreeCertChainList pprgpSelection);

			// Token: 0x0600410F RID: 16655 RVA: 0x0010F128 File Offset: 0x0010D328
			[FriendAccessAllowed]
			internal static X509CertificateCollection FindClientCertificates()
			{
				if (!ComNetOS.IsWin7orLater)
				{
					throw new PlatformNotSupportedException();
				}
				X509CertificateCollection x509CertificateCollection = new X509CertificateCollection();
				X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				x509Store.Open(OpenFlags.MaxAllowed);
				int num = 0;
				SafeFreeCertChainList safeFreeCertChainList = null;
				SafeCertSelectCritera safeCertSelectCritera = new SafeCertSelectCritera();
				try
				{
					if (!UnsafeNclNativeMethods.NativePKI.CertSelectCertificateChains(IntPtr.Zero, UnsafeNclNativeMethods.NativePKI.CertificateSelect.HasPrivateKey, IntPtr.Zero, safeCertSelectCritera.Count, safeCertSelectCritera, x509Store.StoreHandle, out num, out safeFreeCertChainList))
					{
						throw new Win32Exception();
					}
					for (int i = 0; i < num; i++)
					{
						using (SafeFreeCertChain safeFreeCertChain = new SafeFreeCertChain(Marshal.ReadIntPtr(safeFreeCertChainList.DangerousGetHandle() + i * Marshal.SizeOf(typeof(IntPtr))), true))
						{
							X509Chain x509Chain = new X509Chain(safeFreeCertChain.DangerousGetHandle());
							if (x509Chain.ChainElements.Count > 0)
							{
								X509Certificate2 certificate = x509Chain.ChainElements[0].Certificate;
								x509CertificateCollection.Add(certificate);
							}
							x509Chain.Reset();
						}
					}
				}
				finally
				{
					x509Store.Close();
					safeFreeCertChainList.Dispose();
					safeCertSelectCritera.Dispose();
				}
				return x509CertificateCollection;
			}

			// Token: 0x020008DC RID: 2268
			internal struct CRYPT_OBJID_BLOB
			{
				// Token: 0x04003BCC RID: 15308
				public uint cbData;

				// Token: 0x04003BCD RID: 15309
				public IntPtr pbData;
			}

			// Token: 0x020008DD RID: 2269
			internal struct CERT_EXTENSION
			{
				// Token: 0x04003BCE RID: 15310
				public IntPtr pszObjId;

				// Token: 0x04003BCF RID: 15311
				public uint fCritical;

				// Token: 0x04003BD0 RID: 15312
				public UnsafeNclNativeMethods.NativePKI.CRYPT_OBJID_BLOB Value;
			}

			// Token: 0x020008DE RID: 2270
			internal struct CERT_SELECT_CRITERIA
			{
				// Token: 0x04003BD1 RID: 15313
				public uint dwType;

				// Token: 0x04003BD2 RID: 15314
				public uint cPara;

				// Token: 0x04003BD3 RID: 15315
				public IntPtr ppPara;
			}

			// Token: 0x020008DF RID: 2271
			[Flags]
			private enum CertificateSelect
			{
				// Token: 0x04003BD5 RID: 15317
				None = 0,
				// Token: 0x04003BD6 RID: 15318
				AllowExpired = 1,
				// Token: 0x04003BD7 RID: 15319
				TrustedRoot = 2,
				// Token: 0x04003BD8 RID: 15320
				DisallowSelfsigned = 4,
				// Token: 0x04003BD9 RID: 15321
				HasPrivateKey = 8,
				// Token: 0x04003BDA RID: 15322
				HasKeyForSignature = 16,
				// Token: 0x04003BDB RID: 15323
				HasKeyForKeyExchange = 32,
				// Token: 0x04003BDC RID: 15324
				HardwareOnly = 64,
				// Token: 0x04003BDD RID: 15325
				AllowDuplicates = 128
			}
		}

		// Token: 0x0200071D RID: 1821
		[SuppressUnmanagedCodeSecurity]
		internal static class NativeNTSSPI
		{
			// Token: 0x06004110 RID: 16656
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern int EncryptMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In] [Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);

			// Token: 0x06004111 RID: 16657
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern int DecryptMessage([In] ref SSPIHandle contextHandle, [In] [Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber, uint* qualityOfProtection);
		}

		// Token: 0x0200071E RID: 1822
		[SuppressUnmanagedCodeSecurity]
		internal static class WinHttp
		{
			// Token: 0x06004112 RID: 16658
			[DllImport("winhttp.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern bool WinHttpDetectAutoProxyConfigUrl(UnsafeNclNativeMethods.WinHttp.AutoDetectType autoDetectFlags, out SafeGlobalFree autoConfigUrl);

			// Token: 0x06004113 RID: 16659
			[DllImport("winhttp.dll", SetLastError = true)]
			internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref UnsafeNclNativeMethods.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig);

			// Token: 0x06004114 RID: 16660
			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern SafeInternetHandle WinHttpOpen(string userAgent, UnsafeNclNativeMethods.WinHttp.AccessType accessType, string proxyName, string proxyBypass, int dwFlags);

			// Token: 0x06004115 RID: 16661
			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool WinHttpSetTimeouts(SafeInternetHandle session, int resolveTimeout, int connectTimeout, int sendTimeout, int receiveTimeout);

			// Token: 0x06004116 RID: 16662
			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool WinHttpGetProxyForUrl(SafeInternetHandle session, string url, [In] ref UnsafeNclNativeMethods.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out UnsafeNclNativeMethods.WinHttp.WINHTTP_PROXY_INFO proxyInfo);

			// Token: 0x06004117 RID: 16663
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("winhttp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern bool WinHttpCloseHandle(IntPtr httpSession);

			// Token: 0x020008E0 RID: 2272
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
			{
				// Token: 0x04003BDE RID: 15326
				public bool AutoDetect;

				// Token: 0x04003BDF RID: 15327
				public IntPtr AutoConfigUrl;

				// Token: 0x04003BE0 RID: 15328
				public IntPtr Proxy;

				// Token: 0x04003BE1 RID: 15329
				public IntPtr ProxyBypass;
			}

			// Token: 0x020008E1 RID: 2273
			[Flags]
			internal enum AutoProxyFlags
			{
				// Token: 0x04003BE3 RID: 15331
				AutoDetect = 1,
				// Token: 0x04003BE4 RID: 15332
				AutoProxyConfigUrl = 2,
				// Token: 0x04003BE5 RID: 15333
				RunInProcess = 65536,
				// Token: 0x04003BE6 RID: 15334
				RunOutProcessOnly = 131072
			}

			// Token: 0x020008E2 RID: 2274
			internal enum AccessType
			{
				// Token: 0x04003BE8 RID: 15336
				DefaultProxy,
				// Token: 0x04003BE9 RID: 15337
				NoProxy,
				// Token: 0x04003BEA RID: 15338
				NamedProxy = 3
			}

			// Token: 0x020008E3 RID: 2275
			[Flags]
			internal enum AutoDetectType
			{
				// Token: 0x04003BEC RID: 15340
				None = 0,
				// Token: 0x04003BED RID: 15341
				Dhcp = 1,
				// Token: 0x04003BEE RID: 15342
				DnsA = 2
			}

			// Token: 0x020008E4 RID: 2276
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_AUTOPROXY_OPTIONS
			{
				// Token: 0x04003BEF RID: 15343
				public UnsafeNclNativeMethods.WinHttp.AutoProxyFlags Flags;

				// Token: 0x04003BF0 RID: 15344
				public UnsafeNclNativeMethods.WinHttp.AutoDetectType AutoDetectFlags;

				// Token: 0x04003BF1 RID: 15345
				[MarshalAs(UnmanagedType.LPWStr)]
				public string AutoConfigUrl;

				// Token: 0x04003BF2 RID: 15346
				private IntPtr lpvReserved;

				// Token: 0x04003BF3 RID: 15347
				private int dwReserved;

				// Token: 0x04003BF4 RID: 15348
				public bool AutoLogonIfChallenged;
			}

			// Token: 0x020008E5 RID: 2277
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			internal struct WINHTTP_PROXY_INFO
			{
				// Token: 0x04003BF5 RID: 15349
				public UnsafeNclNativeMethods.WinHttp.AccessType AccessType;

				// Token: 0x04003BF6 RID: 15350
				public IntPtr Proxy;

				// Token: 0x04003BF7 RID: 15351
				public IntPtr ProxyBypass;
			}

			// Token: 0x020008E6 RID: 2278
			internal enum ErrorCodes
			{
				// Token: 0x04003BF9 RID: 15353
				Success,
				// Token: 0x04003BFA RID: 15354
				OutOfHandles = 12001,
				// Token: 0x04003BFB RID: 15355
				Timeout,
				// Token: 0x04003BFC RID: 15356
				InternalError = 12004,
				// Token: 0x04003BFD RID: 15357
				InvalidUrl,
				// Token: 0x04003BFE RID: 15358
				UnrecognizedScheme,
				// Token: 0x04003BFF RID: 15359
				NameNotResolved,
				// Token: 0x04003C00 RID: 15360
				InvalidOption = 12009,
				// Token: 0x04003C01 RID: 15361
				OptionNotSettable = 12011,
				// Token: 0x04003C02 RID: 15362
				Shutdown,
				// Token: 0x04003C03 RID: 15363
				LoginFailure = 12015,
				// Token: 0x04003C04 RID: 15364
				OperationCancelled = 12017,
				// Token: 0x04003C05 RID: 15365
				IncorrectHandleType,
				// Token: 0x04003C06 RID: 15366
				IncorrectHandleState,
				// Token: 0x04003C07 RID: 15367
				CannotConnect = 12029,
				// Token: 0x04003C08 RID: 15368
				ConnectionError,
				// Token: 0x04003C09 RID: 15369
				ResendRequest = 12032,
				// Token: 0x04003C0A RID: 15370
				AuthCertNeeded = 12044,
				// Token: 0x04003C0B RID: 15371
				CannotCallBeforeOpen = 12100,
				// Token: 0x04003C0C RID: 15372
				CannotCallBeforeSend,
				// Token: 0x04003C0D RID: 15373
				CannotCallAfterSend,
				// Token: 0x04003C0E RID: 15374
				CannotCallAfterOpen,
				// Token: 0x04003C0F RID: 15375
				HeaderNotFound = 12150,
				// Token: 0x04003C10 RID: 15376
				InvalidServerResponse = 12152,
				// Token: 0x04003C11 RID: 15377
				InvalidHeader,
				// Token: 0x04003C12 RID: 15378
				InvalidQueryRequest,
				// Token: 0x04003C13 RID: 15379
				HeaderAlreadyExists,
				// Token: 0x04003C14 RID: 15380
				RedirectFailed,
				// Token: 0x04003C15 RID: 15381
				AutoProxyServiceError = 12178,
				// Token: 0x04003C16 RID: 15382
				BadAutoProxyScript = 12166,
				// Token: 0x04003C17 RID: 15383
				UnableToDownloadScript,
				// Token: 0x04003C18 RID: 15384
				NotInitialized = 12172,
				// Token: 0x04003C19 RID: 15385
				SecureFailure = 12175,
				// Token: 0x04003C1A RID: 15386
				SecureCertDateInvalid = 12037,
				// Token: 0x04003C1B RID: 15387
				SecureCertCNInvalid,
				// Token: 0x04003C1C RID: 15388
				SecureInvalidCA = 12045,
				// Token: 0x04003C1D RID: 15389
				SecureCertRevFailed = 12057,
				// Token: 0x04003C1E RID: 15390
				SecureChannelError = 12157,
				// Token: 0x04003C1F RID: 15391
				SecureInvalidCert = 12169,
				// Token: 0x04003C20 RID: 15392
				SecureCertRevoked,
				// Token: 0x04003C21 RID: 15393
				SecureCertWrongUsage = 12179,
				// Token: 0x04003C22 RID: 15394
				AudodetectionFailed,
				// Token: 0x04003C23 RID: 15395
				HeaderCountExceeded,
				// Token: 0x04003C24 RID: 15396
				HeaderSizeOverflow,
				// Token: 0x04003C25 RID: 15397
				ChunkedEncodingHeaderSizeOverflow,
				// Token: 0x04003C26 RID: 15398
				ResponseDrainOverflow,
				// Token: 0x04003C27 RID: 15399
				ClientCertNoPrivateKey,
				// Token: 0x04003C28 RID: 15400
				ClientCertNoAccessPrivateKey
			}
		}

		// Token: 0x0200071F RID: 1823
		[SuppressUnmanagedCodeSecurity]
		internal static class UnsafeWinInetCache
		{
			// Token: 0x06004118 RID: 16664
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool CreateUrlCacheEntryW([In] string urlName, [In] int expectedFileSize, [In] string fileExtension, [Out] StringBuilder fileName, [In] int dwReserved);

			// Token: 0x06004119 RID: 16665
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool CommitUrlCacheEntryW([In] string urlName, [In] string localFileName, [In] _WinInetCache.FILETIME expireTime, [In] _WinInetCache.FILETIME lastModifiedTime, [In] _WinInetCache.EntryType EntryType, [In] byte* headerInfo, [In] int headerSizeTChars, [In] string fileExtension, [In] string originalUrl);

			// Token: 0x0600411A RID: 16666
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool GetUrlCacheEntryInfoW([In] string urlName, [In] byte* entryPtr, [In] [Out] ref int bufferSz);

			// Token: 0x0600411B RID: 16667
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern bool SetUrlCacheEntryInfoW([In] string lpszUrlName, [In] byte* EntryPtr, [In] _WinInetCache.Entry_FC fieldControl);

			// Token: 0x0600411C RID: 16668
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool DeleteUrlCacheEntryW([In] string urlName);

			// Token: 0x0600411D RID: 16669
			[DllImport("wininet.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern bool UnlockUrlCacheEntryFileW([In] string urlName, [In] int dwReserved);

			// Token: 0x04003140 RID: 12608
			public const int MAX_PATH = 260;
		}

		// Token: 0x02000720 RID: 1824
		[SuppressUnmanagedCodeSecurity]
		internal static class SspiHelper
		{
			// Token: 0x0600411E RID: 16670
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			[DllImport("secur32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern SecurityStatus SspiFreeAuthIdentity([In] IntPtr authData);

			// Token: 0x0600411F RID: 16671
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern SecurityStatus SspiEncodeStringsAsAuthIdentity([In] string userName, [In] string domainName, [In] string password, out SafeSspiAuthDataHandle authData);
		}

		// Token: 0x02000721 RID: 1825
		[SuppressUnmanagedCodeSecurity]
		internal static class HttpApi
		{
			// Token: 0x06004120 RID: 16672
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpInitialize(UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION version, uint flags, void* pReserved);

			// Token: 0x06004121 RID: 16673
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveRequestEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, NativeOverlapped* pOverlapped);

			// Token: 0x06004122 RID: 16674
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "HttpReceiveRequestEntityBody", ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveRequestEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, [In] SafeHandle pOverlapped);

			// Token: 0x06004123 RID: 16675
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

			// Token: 0x06004124 RID: 16676
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, byte* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

			// Token: 0x06004125 RID: 16677
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpReceiveHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, uint flags, UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* pRequestBuffer, uint requestBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);

			// Token: 0x06004126 RID: 16678
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpSendHttpResponse(CriticalHandle requestQueueHandle, ulong requestId, uint flags, UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE* pHttpResponse, void* pCachePolicy, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

			// Token: 0x06004127 RID: 16679
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpSendResponseEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pEntityChunks, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

			// Token: 0x06004128 RID: 16680
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCancelHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, IntPtr pOverlapped);

			// Token: 0x06004129 RID: 16681
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "HttpSendResponseEntityBody", ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpSendResponseEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, IntPtr pEntityChunks, out uint pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, SafeHandle pOverlapped, IntPtr pLogData);

			// Token: 0x0600412A RID: 16682
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpWaitForDisconnect(CriticalHandle requestQueueHandle, ulong connectionId, NativeOverlapped* pOverlapped);

			// Token: 0x0600412B RID: 16683
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpCreateServerSession(UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION version, ulong* serverSessionId, uint reserved);

			// Token: 0x0600412C RID: 16684
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal unsafe static extern uint HttpCreateUrlGroup(ulong serverSessionId, ulong* urlGroupId, uint reserved);

			// Token: 0x0600412D RID: 16685
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpAddUrlToUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, ulong context, uint pReserved);

			// Token: 0x0600412E RID: 16686
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpSetUrlGroupProperty(ulong urlGroupId, UnsafeNclNativeMethods.HttpApi.HTTP_SERVER_PROPERTY serverProperty, IntPtr pPropertyInfo, uint propertyInfoLength);

			// Token: 0x0600412F RID: 16687
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpRemoveUrlFromUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, uint flags);

			// Token: 0x06004130 RID: 16688
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCloseServerSession(ulong serverSessionId);

			// Token: 0x06004131 RID: 16689
			[DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			internal static extern uint HttpCloseUrlGroup(ulong urlGroupId);

			// Token: 0x06004132 RID: 16690
			[DllImport("tokenbinding.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
			public unsafe static extern int TokenBindingVerifyMessage([In] byte* tokenBindingMessage, [In] uint tokenBindingMessageSize, [In] UnsafeNclNativeMethods.HttpApi.TOKENBINDING_KEY_PARAMETERS_TYPE keyType, [In] byte* tlsUnique, [In] uint tlsUniqueSize, out UnsafeNclNativeMethods.HttpApi.HeapAllocHandle resultList);

			// Token: 0x06004133 RID: 16691
			[DllImport("tokenbinding.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "TokenBindingVerifyMessage")]
			public unsafe static extern int TokenBindingVerifyMessage_V1([In] byte* tokenBindingMessage, [In] uint tokenBindingMessageSize, [In] IntPtr keyType, [In] byte* tlsUnique, [In] uint tlsUniqueSize, out UnsafeNclNativeMethods.HttpApi.HeapAllocHandle resultList);

			// Token: 0x17000EF1 RID: 3825
			// (get) Token: 0x06004134 RID: 16692 RVA: 0x0010F250 File Offset: 0x0010D450
			internal static UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION Version
			{
				get
				{
					return UnsafeNclNativeMethods.HttpApi.version;
				}
			}

			// Token: 0x17000EF2 RID: 3826
			// (get) Token: 0x06004135 RID: 16693 RVA: 0x0010F257 File Offset: 0x0010D457
			internal static UnsafeNclNativeMethods.HttpApi.HTTP_API_VERSION ApiVersion
			{
				get
				{
					if (UnsafeNclNativeMethods.HttpApi.version.HttpApiMajorVersion == 2 && UnsafeNclNativeMethods.HttpApi.version.HttpApiMinorVersion == 0)
					{
						return UnsafeNclNativeMethods.HttpApi.HTTP_API_VERSION.Version20;
					}
					if (UnsafeNclNativeMethods.HttpApi.version.HttpApiMajorVersion == 1 && UnsafeNclNativeMethods.HttpApi.version.HttpApiMinorVersion == 0)
					{
						return UnsafeNclNativeMethods.HttpApi.HTTP_API_VERSION.Version10;
					}
					return UnsafeNclNativeMethods.HttpApi.HTTP_API_VERSION.Invalid;
				}
			}

			// Token: 0x17000EF3 RID: 3827
			// (get) Token: 0x06004136 RID: 16694 RVA: 0x0010F290 File Offset: 0x0010D490
			internal static bool ExtendedProtectionSupported
			{
				get
				{
					return UnsafeNclNativeMethods.HttpApi.extendedProtectionSupported;
				}
			}

			// Token: 0x06004137 RID: 16695 RVA: 0x0010F29C File Offset: 0x0010D49C
			static HttpApi()
			{
				UnsafeNclNativeMethods.HttpApi.InitHttpApi(2, 0);
			}

			// Token: 0x06004138 RID: 16696 RVA: 0x0010F360 File Offset: 0x0010D560
			private static void InitHttpApi(ushort majorVersion, ushort minorVersion)
			{
				UnsafeNclNativeMethods.HttpApi.version.HttpApiMajorVersion = majorVersion;
				UnsafeNclNativeMethods.HttpApi.version.HttpApiMinorVersion = minorVersion;
				UnsafeNclNativeMethods.HttpApi.extendedProtectionSupported = true;
				uint num;
				if (ComNetOS.IsWin7orLater)
				{
					num = UnsafeNclNativeMethods.HttpApi.HttpInitialize(UnsafeNclNativeMethods.HttpApi.version, 1U, null);
				}
				else
				{
					num = UnsafeNclNativeMethods.HttpApi.HttpInitialize(UnsafeNclNativeMethods.HttpApi.version, 5U, null);
					if (num == 87U)
					{
						if (Logging.On)
						{
							Logging.PrintWarning(Logging.HttpListener, global::System.SR.GetString("net_listener_cbt_not_supported"));
						}
						UnsafeNclNativeMethods.HttpApi.extendedProtectionSupported = false;
						num = UnsafeNclNativeMethods.HttpApi.HttpInitialize(UnsafeNclNativeMethods.HttpApi.version, 1U, null);
					}
				}
				UnsafeNclNativeMethods.HttpApi.supported = num == 0U;
			}

			// Token: 0x17000EF4 RID: 3828
			// (get) Token: 0x06004139 RID: 16697 RVA: 0x0010F3F3 File Offset: 0x0010D5F3
			internal static bool Supported
			{
				get
				{
					return UnsafeNclNativeMethods.HttpApi.supported;
				}
			}

			// Token: 0x0600413A RID: 16698 RVA: 0x0010F3FC File Offset: 0x0010D5FC
			internal unsafe static WebHeaderCollection GetHeaders(byte[] memoryBlob, IntPtr originalAddress)
			{
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerRequest);
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr;
					long num = (long)((byte*)ptr - (byte*)(void*)originalAddress);
					if (ptr2->Headers.UnknownHeaderCount != 0)
					{
						UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* ptr3 = num + ptr2->Headers.pUnknownHeaders / sizeof(UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER);
						for (int i = 0; i < (int)ptr2->Headers.UnknownHeaderCount; i++)
						{
							if (ptr3->pName != null && ptr3->NameLength > 0)
							{
								string text = new string(ptr3->pName + num, 0, (int)ptr3->NameLength);
								string text2;
								if (ptr3->pRawValue != null && ptr3->RawValueLength > 0)
								{
									text2 = new string(ptr3->pRawValue + num, 0, (int)ptr3->RawValueLength);
								}
								else
								{
									text2 = string.Empty;
								}
								webHeaderCollection.AddInternal(text, text2);
							}
							ptr3++;
						}
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER* ptr4 = &ptr2->Headers.KnownHeaders;
					for (int i = 0; i < 41; i++)
					{
						if (ptr4->pRawValue != null)
						{
							string text3 = new string(ptr4->pRawValue + num, 0, (int)ptr4->RawValueLength);
							webHeaderCollection.AddInternal(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString(i), text3);
						}
						ptr4++;
					}
				}
				return webHeaderCollection;
			}

			// Token: 0x0600413B RID: 16699 RVA: 0x0010F558 File Offset: 0x0010D758
			private unsafe static string GetKnownHeader(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* request, long fixup, int headerIndex)
			{
				string text = null;
				UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER* ptr = &request->Headers.KnownHeaders + headerIndex;
				if (ptr->pRawValue != null)
				{
					text = new string(ptr->pRawValue + fixup, 0, (int)ptr->RawValueLength);
				}
				return text;
			}

			// Token: 0x0600413C RID: 16700 RVA: 0x0010F59F File Offset: 0x0010D79F
			internal unsafe static string GetKnownHeader(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* request, int headerIndex)
			{
				return UnsafeNclNativeMethods.HttpApi.GetKnownHeader(request, 0L, headerIndex);
			}

			// Token: 0x0600413D RID: 16701 RVA: 0x0010F5AC File Offset: 0x0010D7AC
			internal unsafe static string GetKnownHeader(byte[] memoryBlob, IntPtr originalAddress, int headerIndex)
			{
				byte* ptr;
				if (memoryBlob == null || memoryBlob.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &memoryBlob[0];
				}
				return UnsafeNclNativeMethods.HttpApi.GetKnownHeader((UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr, (long)((byte*)ptr - (byte*)(void*)originalAddress), headerIndex);
			}

			// Token: 0x0600413E RID: 16702 RVA: 0x0010F5E4 File Offset: 0x0010D7E4
			private unsafe static string GetVerb(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* request, long fixup)
			{
				string text = null;
				if (request->Verb > UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbUnknown && request->Verb < UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbMaximum)
				{
					text = UnsafeNclNativeMethods.HttpApi.HttpVerbs[(int)request->Verb];
				}
				else if (request->Verb == UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbUnknown && request->pUnknownVerb != null)
				{
					text = new string(request->pUnknownVerb + fixup, 0, (int)request->UnknownVerbLength);
				}
				return text;
			}

			// Token: 0x0600413F RID: 16703 RVA: 0x0010F63F File Offset: 0x0010D83F
			internal unsafe static string GetVerb(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* request)
			{
				return UnsafeNclNativeMethods.HttpApi.GetVerb(request, 0L);
			}

			// Token: 0x06004140 RID: 16704 RVA: 0x0010F64C File Offset: 0x0010D84C
			internal unsafe static string GetVerb(byte[] memoryBlob, IntPtr originalAddress)
			{
				byte* ptr;
				if (memoryBlob == null || memoryBlob.Length == 0)
				{
					ptr = null;
				}
				else
				{
					ptr = &memoryBlob[0];
				}
				return UnsafeNclNativeMethods.HttpApi.GetVerb((UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr, (long)((byte*)ptr - (byte*)(void*)originalAddress));
			}

			// Token: 0x06004141 RID: 16705 RVA: 0x0010F684 File Offset: 0x0010D884
			internal unsafe static UnsafeNclNativeMethods.HttpApi.HTTP_VERB GetKnownVerb(byte[] memoryBlob, IntPtr originalAddress)
			{
				UnsafeNclNativeMethods.HttpApi.HTTP_VERB http_VERB = UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbUnknown;
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr;
					if (ptr2->Verb > UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbUnparsed && ptr2->Verb < UnsafeNclNativeMethods.HttpApi.HTTP_VERB.HttpVerbMaximum)
					{
						http_VERB = ptr2->Verb;
					}
				}
				return http_VERB;
			}

			// Token: 0x06004142 RID: 16706 RVA: 0x0010F6CC File Offset: 0x0010D8CC
			internal unsafe static uint GetChunks(byte[] memoryBlob, IntPtr originalAddress, ref int dataChunkIndex, ref uint dataChunkOffset, byte[] buffer, int offset, int size)
			{
				uint num = 0U;
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr;
					long num2 = (long)((byte*)ptr - (byte*)(void*)originalAddress);
					if (ptr2->EntityChunkCount > 0 && dataChunkIndex < (int)ptr2->EntityChunkCount && dataChunkIndex != -1)
					{
						UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* ptr3 = num2 + (ptr2->pEntityChunks + dataChunkIndex) / sizeof(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK);
						fixed (byte[] array2 = buffer)
						{
							byte* ptr4;
							if (buffer == null || array2.Length == 0)
							{
								ptr4 = null;
							}
							else
							{
								ptr4 = &array2[0];
							}
							byte* ptr5 = ptr4 + offset;
							while (dataChunkIndex < (int)ptr2->EntityChunkCount && (ulong)num < (ulong)((long)size))
							{
								if (dataChunkOffset >= ptr3->BufferLength)
								{
									dataChunkOffset = 0U;
									dataChunkIndex++;
									ptr3++;
								}
								else
								{
									byte* ptr6 = ptr3->pBuffer + dataChunkOffset + num2;
									uint num3 = ptr3->BufferLength - dataChunkOffset;
									if (num3 > (uint)size)
									{
										num3 = (uint)size;
									}
									for (uint num4 = 0U; num4 < num3; num4 += 1U)
									{
										*(ptr5++) = *(ptr6++);
									}
									num += num3;
									dataChunkOffset += num3;
								}
							}
						}
					}
					if (dataChunkIndex == (int)ptr2->EntityChunkCount)
					{
						dataChunkIndex = -1;
					}
				}
				return num;
			}

			// Token: 0x06004143 RID: 16707 RVA: 0x0010F800 File Offset: 0x0010DA00
			internal unsafe static IPEndPoint GetRemoteEndPoint(byte[] memoryBlob, IntPtr originalAddress)
			{
				SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, 16);
				SocketAddress socketAddress2 = new SocketAddress(AddressFamily.InterNetworkV6, 28);
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr;
					IntPtr intPtr = ((ptr2->Address.pRemoteAddress != null) ? ((IntPtr)((void*)((byte*)((IntPtr)((long)((byte*)ptr - (byte*)(void*)originalAddress))) + ptr2->Address.pRemoteAddress))) : IntPtr.Zero);
					UnsafeNclNativeMethods.HttpApi.CopyOutAddress(intPtr, ref socketAddress, ref socketAddress2);
				}
				IPEndPoint ipendPoint = null;
				if (socketAddress != null)
				{
					ipendPoint = IPEndPoint.Any.Create(socketAddress) as IPEndPoint;
				}
				else if (socketAddress2 != null)
				{
					ipendPoint = IPEndPoint.IPv6Any.Create(socketAddress2) as IPEndPoint;
				}
				return ipendPoint;
			}

			// Token: 0x06004144 RID: 16708 RVA: 0x0010F8B0 File Offset: 0x0010DAB0
			internal unsafe static IPEndPoint GetLocalEndPoint(byte[] memoryBlob, IntPtr originalAddress)
			{
				SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, 16);
				SocketAddress socketAddress2 = new SocketAddress(AddressFamily.InterNetworkV6, 28);
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST*)ptr;
					IntPtr intPtr = ((ptr2->Address.pLocalAddress != null) ? ((IntPtr)((void*)((byte*)((IntPtr)((long)((byte*)ptr - (byte*)(void*)originalAddress))) + ptr2->Address.pLocalAddress))) : IntPtr.Zero);
					UnsafeNclNativeMethods.HttpApi.CopyOutAddress(intPtr, ref socketAddress, ref socketAddress2);
				}
				IPEndPoint ipendPoint = null;
				if (socketAddress != null)
				{
					ipendPoint = IPEndPoint.Any.Create(socketAddress) as IPEndPoint;
				}
				else if (socketAddress2 != null)
				{
					ipendPoint = IPEndPoint.IPv6Any.Create(socketAddress2) as IPEndPoint;
				}
				return ipendPoint;
			}

			// Token: 0x06004145 RID: 16709 RVA: 0x0010F960 File Offset: 0x0010DB60
			internal unsafe static UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO* GetTlsTokenBindingRequestInfo(byte[] memoryBlob, IntPtr originalAddress)
			{
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2*)ptr;
					long num = (long)((byte*)ptr - (byte*)(void*)originalAddress);
					for (int i = 0; i < (int)ptr2->RequestInfoCount; i++)
					{
						UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO* ptr3 = num + (ptr2->pRequestInfo + i) / sizeof(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO);
						if (ptr3 != null && ptr3->InfoType == UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO_TYPE.HttpRequestInfoTypeSslTokenBinding)
						{
							return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO*)((byte*)ptr3->pInfo + num);
						}
					}
				}
				return null;
			}

			// Token: 0x06004146 RID: 16710 RVA: 0x0010F9E0 File Offset: 0x0010DBE0
			internal unsafe static UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO_V1* GetTlsTokenBindingRequestInfo_V1(byte[] memoryBlob, IntPtr originalAddress)
			{
				fixed (byte[] array = memoryBlob)
				{
					byte* ptr;
					if (memoryBlob == null || array.Length == 0)
					{
						ptr = null;
					}
					else
					{
						ptr = &array[0];
					}
					UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2* ptr2 = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2*)ptr;
					long num = (long)((byte*)ptr - (byte*)(void*)originalAddress);
					for (int i = 0; i < (int)ptr2->RequestInfoCount; i++)
					{
						UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO* ptr3 = num + (ptr2->pRequestInfo + i) / sizeof(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO);
						if (ptr3 != null && ptr3->InfoType == UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO_TYPE.HttpRequestInfoTypeSslTokenBindingDraft)
						{
							return (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO_V1*)((byte*)ptr3->pInfo + num);
						}
					}
				}
				return null;
			}

			// Token: 0x06004147 RID: 16711 RVA: 0x0010FA60 File Offset: 0x0010DC60
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			private unsafe static void CopyOutAddress(IntPtr address, ref SocketAddress v4address, ref SocketAddress v6address)
			{
				if (address != IntPtr.Zero)
				{
					ushort num = *(ushort*)(void*)address;
					if (num == 2)
					{
						v6address = null;
						byte[] array;
						byte* ptr;
						if ((array = v4address.m_Buffer) == null || array.Length == 0)
						{
							ptr = null;
						}
						else
						{
							ptr = &array[0];
						}
						for (int i = 2; i < 16; i++)
						{
							ptr[i] = ((byte*)(void*)address)[i];
						}
						array = null;
						return;
					}
					if (num == 23)
					{
						v4address = null;
						byte[] array;
						byte* ptr2;
						if ((array = v6address.m_Buffer) == null || array.Length == 0)
						{
							ptr2 = null;
						}
						else
						{
							ptr2 = &array[0];
						}
						for (int j = 2; j < 28; j++)
						{
							ptr2[j] = ((byte*)(void*)address)[j];
						}
						array = null;
						return;
					}
				}
				v4address = null;
				v6address = null;
			}

			// Token: 0x04003141 RID: 12609
			internal const int MaxTimeout = 6;

			// Token: 0x04003142 RID: 12610
			internal static readonly string[] HttpVerbs = new string[]
			{
				null, "Unknown", "Invalid", "OPTIONS", "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE",
				"CONNECT", "TRACK", "MOVE", "COPY", "PROPFIND", "PROPPATCH", "MKCOL", "LOCK", "UNLOCK", "SEARCH"
			};

			// Token: 0x04003143 RID: 12611
			private const int HttpHeaderRequestMaximum = 41;

			// Token: 0x04003144 RID: 12612
			private const int HttpHeaderResponseMaximum = 30;

			// Token: 0x04003145 RID: 12613
			private static UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION version;

			// Token: 0x04003146 RID: 12614
			private static volatile bool extendedProtectionSupported;

			// Token: 0x04003147 RID: 12615
			private static volatile bool supported;

			// Token: 0x020008E7 RID: 2279
			internal sealed class HeapAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
			{
				// Token: 0x06004630 RID: 17968 RVA: 0x001250C5 File Offset: 0x001232C5
				private HeapAllocHandle()
					: base(true)
				{
				}

				// Token: 0x06004631 RID: 17969 RVA: 0x001250CE File Offset: 0x001232CE
				protected override bool ReleaseHandle()
				{
					return UnsafeNclNativeMethods.HeapFree(UnsafeNclNativeMethods.HttpApi.HeapAllocHandle.ProcessHeap, 0U, this.handle);
				}

				// Token: 0x04003C29 RID: 15401
				private static readonly IntPtr ProcessHeap = UnsafeNclNativeMethods.GetProcessHeap();
			}

			// Token: 0x020008E8 RID: 2280
			internal enum HTTP_API_VERSION
			{
				// Token: 0x04003C2B RID: 15403
				Invalid,
				// Token: 0x04003C2C RID: 15404
				Version10,
				// Token: 0x04003C2D RID: 15405
				Version20
			}

			// Token: 0x020008E9 RID: 2281
			internal enum HTTP_SERVER_PROPERTY
			{
				// Token: 0x04003C2F RID: 15407
				HttpServerAuthenticationProperty,
				// Token: 0x04003C30 RID: 15408
				HttpServerLoggingProperty,
				// Token: 0x04003C31 RID: 15409
				HttpServerQosProperty,
				// Token: 0x04003C32 RID: 15410
				HttpServerTimeoutsProperty,
				// Token: 0x04003C33 RID: 15411
				HttpServerQueueLengthProperty,
				// Token: 0x04003C34 RID: 15412
				HttpServerStateProperty,
				// Token: 0x04003C35 RID: 15413
				HttpServer503VerbosityProperty,
				// Token: 0x04003C36 RID: 15414
				HttpServerBindingProperty,
				// Token: 0x04003C37 RID: 15415
				HttpServerExtendedAuthenticationProperty,
				// Token: 0x04003C38 RID: 15416
				HttpServerListenEndpointProperty,
				// Token: 0x04003C39 RID: 15417
				HttpServerChannelBindProperty,
				// Token: 0x04003C3A RID: 15418
				HttpServerProtectionLevelProperty
			}

			// Token: 0x020008EA RID: 2282
			internal enum HTTP_REQUEST_INFO_TYPE
			{
				// Token: 0x04003C3C RID: 15420
				HttpRequestInfoTypeAuth,
				// Token: 0x04003C3D RID: 15421
				HttpRequestInfoTypeChannelBind,
				// Token: 0x04003C3E RID: 15422
				HttpRequestInfoTypeSslProtocol,
				// Token: 0x04003C3F RID: 15423
				HttpRequestInfoTypeSslTokenBindingDraft,
				// Token: 0x04003C40 RID: 15424
				HttpRequestInfoTypeSslTokenBinding
			}

			// Token: 0x020008EB RID: 2283
			internal enum HTTP_RESPONSE_INFO_TYPE
			{
				// Token: 0x04003C42 RID: 15426
				HttpResponseInfoTypeMultipleKnownHeaders,
				// Token: 0x04003C43 RID: 15427
				HttpResponseInfoTypeAuthenticationProperty,
				// Token: 0x04003C44 RID: 15428
				HttpResponseInfoTypeQosProperty
			}

			// Token: 0x020008EC RID: 2284
			internal enum HTTP_TIMEOUT_TYPE
			{
				// Token: 0x04003C46 RID: 15430
				EntityBody,
				// Token: 0x04003C47 RID: 15431
				DrainEntityBody,
				// Token: 0x04003C48 RID: 15432
				RequestQueue,
				// Token: 0x04003C49 RID: 15433
				IdleConnection,
				// Token: 0x04003C4A RID: 15434
				HeaderWait,
				// Token: 0x04003C4B RID: 15435
				MinSendRate
			}

			// Token: 0x020008ED RID: 2285
			internal struct HTTP_VERSION
			{
				// Token: 0x04003C4C RID: 15436
				internal ushort MajorVersion;

				// Token: 0x04003C4D RID: 15437
				internal ushort MinorVersion;
			}

			// Token: 0x020008EE RID: 2286
			internal struct HTTP_KNOWN_HEADER
			{
				// Token: 0x04003C4E RID: 15438
				internal ushort RawValueLength;

				// Token: 0x04003C4F RID: 15439
				internal unsafe sbyte* pRawValue;
			}

			// Token: 0x020008EF RID: 2287
			[StructLayout(LayoutKind.Sequential, Size = 32)]
			internal struct HTTP_DATA_CHUNK
			{
				// Token: 0x04003C50 RID: 15440
				internal UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE DataChunkType;

				// Token: 0x04003C51 RID: 15441
				internal uint p0;

				// Token: 0x04003C52 RID: 15442
				internal unsafe byte* pBuffer;

				// Token: 0x04003C53 RID: 15443
				internal uint BufferLength;
			}

			// Token: 0x020008F0 RID: 2288
			internal struct HTTPAPI_VERSION
			{
				// Token: 0x04003C54 RID: 15444
				internal ushort HttpApiMajorVersion;

				// Token: 0x04003C55 RID: 15445
				internal ushort HttpApiMinorVersion;
			}

			// Token: 0x020008F1 RID: 2289
			internal struct HTTP_COOKED_URL
			{
				// Token: 0x04003C56 RID: 15446
				internal ushort FullUrlLength;

				// Token: 0x04003C57 RID: 15447
				internal ushort HostLength;

				// Token: 0x04003C58 RID: 15448
				internal ushort AbsPathLength;

				// Token: 0x04003C59 RID: 15449
				internal ushort QueryStringLength;

				// Token: 0x04003C5A RID: 15450
				internal unsafe ushort* pFullUrl;

				// Token: 0x04003C5B RID: 15451
				internal unsafe ushort* pHost;

				// Token: 0x04003C5C RID: 15452
				internal unsafe ushort* pAbsPath;

				// Token: 0x04003C5D RID: 15453
				internal unsafe ushort* pQueryString;
			}

			// Token: 0x020008F2 RID: 2290
			internal struct SOCKADDR
			{
				// Token: 0x04003C5E RID: 15454
				internal ushort sa_family;

				// Token: 0x04003C5F RID: 15455
				internal byte sa_data;

				// Token: 0x04003C60 RID: 15456
				internal byte sa_data_02;

				// Token: 0x04003C61 RID: 15457
				internal byte sa_data_03;

				// Token: 0x04003C62 RID: 15458
				internal byte sa_data_04;

				// Token: 0x04003C63 RID: 15459
				internal byte sa_data_05;

				// Token: 0x04003C64 RID: 15460
				internal byte sa_data_06;

				// Token: 0x04003C65 RID: 15461
				internal byte sa_data_07;

				// Token: 0x04003C66 RID: 15462
				internal byte sa_data_08;

				// Token: 0x04003C67 RID: 15463
				internal byte sa_data_09;

				// Token: 0x04003C68 RID: 15464
				internal byte sa_data_10;

				// Token: 0x04003C69 RID: 15465
				internal byte sa_data_11;

				// Token: 0x04003C6A RID: 15466
				internal byte sa_data_12;

				// Token: 0x04003C6B RID: 15467
				internal byte sa_data_13;

				// Token: 0x04003C6C RID: 15468
				internal byte sa_data_14;
			}

			// Token: 0x020008F3 RID: 2291
			internal struct HTTP_TRANSPORT_ADDRESS
			{
				// Token: 0x04003C6D RID: 15469
				internal unsafe UnsafeNclNativeMethods.HttpApi.SOCKADDR* pRemoteAddress;

				// Token: 0x04003C6E RID: 15470
				internal unsafe UnsafeNclNativeMethods.HttpApi.SOCKADDR* pLocalAddress;
			}

			// Token: 0x020008F4 RID: 2292
			internal struct HTTP_SSL_CLIENT_CERT_INFO
			{
				// Token: 0x04003C6F RID: 15471
				internal uint CertFlags;

				// Token: 0x04003C70 RID: 15472
				internal uint CertEncodedSize;

				// Token: 0x04003C71 RID: 15473
				internal unsafe byte* pCertEncoded;

				// Token: 0x04003C72 RID: 15474
				internal unsafe void* Token;

				// Token: 0x04003C73 RID: 15475
				internal byte CertDeniedByMapper;
			}

			// Token: 0x020008F5 RID: 2293
			internal enum HTTP_SERVICE_BINDING_TYPE : uint
			{
				// Token: 0x04003C75 RID: 15477
				HttpServiceBindingTypeNone,
				// Token: 0x04003C76 RID: 15478
				HttpServiceBindingTypeW,
				// Token: 0x04003C77 RID: 15479
				HttpServiceBindingTypeA
			}

			// Token: 0x020008F6 RID: 2294
			internal struct HTTP_SERVICE_BINDING_BASE
			{
				// Token: 0x04003C78 RID: 15480
				internal UnsafeNclNativeMethods.HttpApi.HTTP_SERVICE_BINDING_TYPE Type;
			}

			// Token: 0x020008F7 RID: 2295
			internal struct HTTP_REQUEST_CHANNEL_BIND_STATUS
			{
				// Token: 0x04003C79 RID: 15481
				internal IntPtr ServiceName;

				// Token: 0x04003C7A RID: 15482
				internal IntPtr ChannelToken;

				// Token: 0x04003C7B RID: 15483
				internal uint ChannelTokenSize;

				// Token: 0x04003C7C RID: 15484
				internal uint Flags;
			}

			// Token: 0x020008F8 RID: 2296
			internal struct HTTP_UNKNOWN_HEADER
			{
				// Token: 0x04003C7D RID: 15485
				internal ushort NameLength;

				// Token: 0x04003C7E RID: 15486
				internal ushort RawValueLength;

				// Token: 0x04003C7F RID: 15487
				internal unsafe sbyte* pName;

				// Token: 0x04003C80 RID: 15488
				internal unsafe sbyte* pRawValue;
			}

			// Token: 0x020008F9 RID: 2297
			internal struct HTTP_SSL_INFO
			{
				// Token: 0x04003C81 RID: 15489
				internal ushort ServerCertKeySize;

				// Token: 0x04003C82 RID: 15490
				internal ushort ConnectionKeySize;

				// Token: 0x04003C83 RID: 15491
				internal uint ServerCertIssuerSize;

				// Token: 0x04003C84 RID: 15492
				internal uint ServerCertSubjectSize;

				// Token: 0x04003C85 RID: 15493
				internal unsafe sbyte* pServerCertIssuer;

				// Token: 0x04003C86 RID: 15494
				internal unsafe sbyte* pServerCertSubject;

				// Token: 0x04003C87 RID: 15495
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo;

				// Token: 0x04003C88 RID: 15496
				internal uint SslClientCertNegotiated;
			}

			// Token: 0x020008FA RID: 2298
			internal struct HTTP_RESPONSE_HEADERS
			{
				// Token: 0x04003C89 RID: 15497
				internal ushort UnknownHeaderCount;

				// Token: 0x04003C8A RID: 15498
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pUnknownHeaders;

				// Token: 0x04003C8B RID: 15499
				internal ushort TrailerCount;

				// Token: 0x04003C8C RID: 15500
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pTrailers;

				// Token: 0x04003C8D RID: 15501
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders;

				// Token: 0x04003C8E RID: 15502
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_02;

				// Token: 0x04003C8F RID: 15503
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_03;

				// Token: 0x04003C90 RID: 15504
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_04;

				// Token: 0x04003C91 RID: 15505
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_05;

				// Token: 0x04003C92 RID: 15506
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_06;

				// Token: 0x04003C93 RID: 15507
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_07;

				// Token: 0x04003C94 RID: 15508
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_08;

				// Token: 0x04003C95 RID: 15509
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_09;

				// Token: 0x04003C96 RID: 15510
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_10;

				// Token: 0x04003C97 RID: 15511
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_11;

				// Token: 0x04003C98 RID: 15512
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_12;

				// Token: 0x04003C99 RID: 15513
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_13;

				// Token: 0x04003C9A RID: 15514
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_14;

				// Token: 0x04003C9B RID: 15515
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_15;

				// Token: 0x04003C9C RID: 15516
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_16;

				// Token: 0x04003C9D RID: 15517
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_17;

				// Token: 0x04003C9E RID: 15518
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_18;

				// Token: 0x04003C9F RID: 15519
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_19;

				// Token: 0x04003CA0 RID: 15520
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_20;

				// Token: 0x04003CA1 RID: 15521
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_21;

				// Token: 0x04003CA2 RID: 15522
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_22;

				// Token: 0x04003CA3 RID: 15523
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_23;

				// Token: 0x04003CA4 RID: 15524
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_24;

				// Token: 0x04003CA5 RID: 15525
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_25;

				// Token: 0x04003CA6 RID: 15526
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_26;

				// Token: 0x04003CA7 RID: 15527
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_27;

				// Token: 0x04003CA8 RID: 15528
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_28;

				// Token: 0x04003CA9 RID: 15529
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_29;

				// Token: 0x04003CAA RID: 15530
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_30;
			}

			// Token: 0x020008FB RID: 2299
			internal struct HTTP_REQUEST_HEADERS
			{
				// Token: 0x04003CAB RID: 15531
				internal ushort UnknownHeaderCount;

				// Token: 0x04003CAC RID: 15532
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pUnknownHeaders;

				// Token: 0x04003CAD RID: 15533
				internal ushort TrailerCount;

				// Token: 0x04003CAE RID: 15534
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pTrailers;

				// Token: 0x04003CAF RID: 15535
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders;

				// Token: 0x04003CB0 RID: 15536
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_02;

				// Token: 0x04003CB1 RID: 15537
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_03;

				// Token: 0x04003CB2 RID: 15538
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_04;

				// Token: 0x04003CB3 RID: 15539
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_05;

				// Token: 0x04003CB4 RID: 15540
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_06;

				// Token: 0x04003CB5 RID: 15541
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_07;

				// Token: 0x04003CB6 RID: 15542
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_08;

				// Token: 0x04003CB7 RID: 15543
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_09;

				// Token: 0x04003CB8 RID: 15544
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_10;

				// Token: 0x04003CB9 RID: 15545
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_11;

				// Token: 0x04003CBA RID: 15546
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_12;

				// Token: 0x04003CBB RID: 15547
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_13;

				// Token: 0x04003CBC RID: 15548
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_14;

				// Token: 0x04003CBD RID: 15549
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_15;

				// Token: 0x04003CBE RID: 15550
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_16;

				// Token: 0x04003CBF RID: 15551
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_17;

				// Token: 0x04003CC0 RID: 15552
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_18;

				// Token: 0x04003CC1 RID: 15553
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_19;

				// Token: 0x04003CC2 RID: 15554
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_20;

				// Token: 0x04003CC3 RID: 15555
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_21;

				// Token: 0x04003CC4 RID: 15556
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_22;

				// Token: 0x04003CC5 RID: 15557
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_23;

				// Token: 0x04003CC6 RID: 15558
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_24;

				// Token: 0x04003CC7 RID: 15559
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_25;

				// Token: 0x04003CC8 RID: 15560
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_26;

				// Token: 0x04003CC9 RID: 15561
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_27;

				// Token: 0x04003CCA RID: 15562
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_28;

				// Token: 0x04003CCB RID: 15563
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_29;

				// Token: 0x04003CCC RID: 15564
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_30;

				// Token: 0x04003CCD RID: 15565
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_31;

				// Token: 0x04003CCE RID: 15566
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_32;

				// Token: 0x04003CCF RID: 15567
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_33;

				// Token: 0x04003CD0 RID: 15568
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_34;

				// Token: 0x04003CD1 RID: 15569
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_35;

				// Token: 0x04003CD2 RID: 15570
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_36;

				// Token: 0x04003CD3 RID: 15571
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_37;

				// Token: 0x04003CD4 RID: 15572
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_38;

				// Token: 0x04003CD5 RID: 15573
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_39;

				// Token: 0x04003CD6 RID: 15574
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_40;

				// Token: 0x04003CD7 RID: 15575
				internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_41;
			}

			// Token: 0x020008FC RID: 2300
			internal enum HTTP_VERB
			{
				// Token: 0x04003CD9 RID: 15577
				HttpVerbUnparsed,
				// Token: 0x04003CDA RID: 15578
				HttpVerbUnknown,
				// Token: 0x04003CDB RID: 15579
				HttpVerbInvalid,
				// Token: 0x04003CDC RID: 15580
				HttpVerbOPTIONS,
				// Token: 0x04003CDD RID: 15581
				HttpVerbGET,
				// Token: 0x04003CDE RID: 15582
				HttpVerbHEAD,
				// Token: 0x04003CDF RID: 15583
				HttpVerbPOST,
				// Token: 0x04003CE0 RID: 15584
				HttpVerbPUT,
				// Token: 0x04003CE1 RID: 15585
				HttpVerbDELETE,
				// Token: 0x04003CE2 RID: 15586
				HttpVerbTRACE,
				// Token: 0x04003CE3 RID: 15587
				HttpVerbCONNECT,
				// Token: 0x04003CE4 RID: 15588
				HttpVerbTRACK,
				// Token: 0x04003CE5 RID: 15589
				HttpVerbMOVE,
				// Token: 0x04003CE6 RID: 15590
				HttpVerbCOPY,
				// Token: 0x04003CE7 RID: 15591
				HttpVerbPROPFIND,
				// Token: 0x04003CE8 RID: 15592
				HttpVerbPROPPATCH,
				// Token: 0x04003CE9 RID: 15593
				HttpVerbMKCOL,
				// Token: 0x04003CEA RID: 15594
				HttpVerbLOCK,
				// Token: 0x04003CEB RID: 15595
				HttpVerbUNLOCK,
				// Token: 0x04003CEC RID: 15596
				HttpVerbSEARCH,
				// Token: 0x04003CED RID: 15597
				HttpVerbMaximum
			}

			// Token: 0x020008FD RID: 2301
			internal enum HTTP_DATA_CHUNK_TYPE
			{
				// Token: 0x04003CEF RID: 15599
				HttpDataChunkFromMemory,
				// Token: 0x04003CF0 RID: 15600
				HttpDataChunkFromFileHandle,
				// Token: 0x04003CF1 RID: 15601
				HttpDataChunkFromFragmentCache,
				// Token: 0x04003CF2 RID: 15602
				HttpDataChunkMaximum
			}

			// Token: 0x020008FE RID: 2302
			internal struct HTTP_RESPONSE_INFO
			{
				// Token: 0x04003CF3 RID: 15603
				internal UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_INFO_TYPE Type;

				// Token: 0x04003CF4 RID: 15604
				internal uint Length;

				// Token: 0x04003CF5 RID: 15605
				internal unsafe void* pInfo;
			}

			// Token: 0x020008FF RID: 2303
			internal struct HTTP_RESPONSE
			{
				// Token: 0x04003CF6 RID: 15606
				internal uint Flags;

				// Token: 0x04003CF7 RID: 15607
				internal UnsafeNclNativeMethods.HttpApi.HTTP_VERSION Version;

				// Token: 0x04003CF8 RID: 15608
				internal ushort StatusCode;

				// Token: 0x04003CF9 RID: 15609
				internal ushort ReasonLength;

				// Token: 0x04003CFA RID: 15610
				internal unsafe sbyte* pReason;

				// Token: 0x04003CFB RID: 15611
				internal UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADERS Headers;

				// Token: 0x04003CFC RID: 15612
				internal ushort EntityChunkCount;

				// Token: 0x04003CFD RID: 15613
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pEntityChunks;

				// Token: 0x04003CFE RID: 15614
				internal ushort ResponseInfoCount;

				// Token: 0x04003CFF RID: 15615
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_INFO* pResponseInfo;
			}

			// Token: 0x02000900 RID: 2304
			internal struct HTTP_REQUEST_INFO
			{
				// Token: 0x04003D00 RID: 15616
				internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO_TYPE InfoType;

				// Token: 0x04003D01 RID: 15617
				internal uint InfoLength;

				// Token: 0x04003D02 RID: 15618
				internal unsafe void* pInfo;
			}

			// Token: 0x02000901 RID: 2305
			internal struct HTTP_REQUEST
			{
				// Token: 0x04003D03 RID: 15619
				internal uint Flags;

				// Token: 0x04003D04 RID: 15620
				internal ulong ConnectionId;

				// Token: 0x04003D05 RID: 15621
				internal ulong RequestId;

				// Token: 0x04003D06 RID: 15622
				internal ulong UrlContext;

				// Token: 0x04003D07 RID: 15623
				internal UnsafeNclNativeMethods.HttpApi.HTTP_VERSION Version;

				// Token: 0x04003D08 RID: 15624
				internal UnsafeNclNativeMethods.HttpApi.HTTP_VERB Verb;

				// Token: 0x04003D09 RID: 15625
				internal ushort UnknownVerbLength;

				// Token: 0x04003D0A RID: 15626
				internal ushort RawUrlLength;

				// Token: 0x04003D0B RID: 15627
				internal unsafe sbyte* pUnknownVerb;

				// Token: 0x04003D0C RID: 15628
				internal unsafe sbyte* pRawUrl;

				// Token: 0x04003D0D RID: 15629
				internal UnsafeNclNativeMethods.HttpApi.HTTP_COOKED_URL CookedUrl;

				// Token: 0x04003D0E RID: 15630
				internal UnsafeNclNativeMethods.HttpApi.HTTP_TRANSPORT_ADDRESS Address;

				// Token: 0x04003D0F RID: 15631
				internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADERS Headers;

				// Token: 0x04003D10 RID: 15632
				internal ulong BytesReceived;

				// Token: 0x04003D11 RID: 15633
				internal ushort EntityChunkCount;

				// Token: 0x04003D12 RID: 15634
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pEntityChunks;

				// Token: 0x04003D13 RID: 15635
				internal ulong RawConnectionId;

				// Token: 0x04003D14 RID: 15636
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_INFO* pSslInfo;
			}

			// Token: 0x02000902 RID: 2306
			internal struct HTTP_REQUEST_V2
			{
				// Token: 0x04003D15 RID: 15637
				internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST RequestV1;

				// Token: 0x04003D16 RID: 15638
				internal ushort RequestInfoCount;

				// Token: 0x04003D17 RID: 15639
				internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO* pRequestInfo;
			}

			// Token: 0x02000903 RID: 2307
			internal struct HTTP_TIMEOUT_LIMIT_INFO
			{
				// Token: 0x04003D18 RID: 15640
				internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS Flags;

				// Token: 0x04003D19 RID: 15641
				internal ushort EntityBody;

				// Token: 0x04003D1A RID: 15642
				internal ushort DrainEntityBody;

				// Token: 0x04003D1B RID: 15643
				internal ushort RequestQueue;

				// Token: 0x04003D1C RID: 15644
				internal ushort IdleConnection;

				// Token: 0x04003D1D RID: 15645
				internal ushort HeaderWait;

				// Token: 0x04003D1E RID: 15646
				internal uint MinSendRate;
			}

			// Token: 0x02000904 RID: 2308
			internal struct HTTP_BINDING_INFO
			{
				// Token: 0x04003D1F RID: 15647
				internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS Flags;

				// Token: 0x04003D20 RID: 15648
				internal IntPtr RequestQueueHandle;
			}

			// Token: 0x02000905 RID: 2309
			internal struct HTTP_REQUEST_TOKEN_BINDING_INFO
			{
				// Token: 0x04003D21 RID: 15649
				public unsafe byte* TokenBinding;

				// Token: 0x04003D22 RID: 15650
				public uint TokenBindingSize;

				// Token: 0x04003D23 RID: 15651
				public unsafe byte* TlsUnique;

				// Token: 0x04003D24 RID: 15652
				public uint TlsUniqueSize;

				// Token: 0x04003D25 RID: 15653
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_KEY_PARAMETERS_TYPE KeyType;
			}

			// Token: 0x02000906 RID: 2310
			internal struct HTTP_REQUEST_TOKEN_BINDING_INFO_V1
			{
				// Token: 0x04003D26 RID: 15654
				public unsafe byte* TokenBinding;

				// Token: 0x04003D27 RID: 15655
				public uint TokenBindingSize;

				// Token: 0x04003D28 RID: 15656
				public unsafe byte* TlsUnique;

				// Token: 0x04003D29 RID: 15657
				public uint TlsUniqueSize;

				// Token: 0x04003D2A RID: 15658
				public IntPtr KeyType;
			}

			// Token: 0x02000907 RID: 2311
			internal enum TOKENBINDING_HASH_ALGORITHM_V1 : byte
			{
				// Token: 0x04003D2C RID: 15660
				TOKENBINDING_HASH_ALGORITHM_SHA256 = 4
			}

			// Token: 0x02000908 RID: 2312
			internal enum TOKENBINDING_SIGNATURE_ALGORITHM_V1 : byte
			{
				// Token: 0x04003D2E RID: 15662
				TOKENBINDING_SIGNATURE_ALGORITHM_RSA = 1,
				// Token: 0x04003D2F RID: 15663
				TOKENBINDING_SIGNATURE_ALGORITHM_ECDSAP256 = 3
			}

			// Token: 0x02000909 RID: 2313
			internal enum TOKENBINDING_TYPE : byte
			{
				// Token: 0x04003D31 RID: 15665
				TOKENBINDING_TYPE_PROVIDED,
				// Token: 0x04003D32 RID: 15666
				TOKENBINDING_TYPE_REFERRED
			}

			// Token: 0x0200090A RID: 2314
			internal enum TOKENBINDING_EXTENSION_FORMAT
			{
				// Token: 0x04003D34 RID: 15668
				TOKENBINDING_EXTENSION_FORMAT_UNDEFINED
			}

			// Token: 0x0200090B RID: 2315
			internal enum TOKENBINDING_KEY_PARAMETERS_TYPE : byte
			{
				// Token: 0x04003D36 RID: 15670
				TOKENBINDING_KEY_PARAMETERS_TYPE_RSA_PKCS_SHA256,
				// Token: 0x04003D37 RID: 15671
				TOKENBINDING_KEY_PARAMETERS_TYPE_RSA_PSS_SHA256,
				// Token: 0x04003D38 RID: 15672
				TOKENBINDING_KEY_PARAMETERS_TYPE_ECDSA_SHA256
			}

			// Token: 0x0200090C RID: 2316
			internal struct TOKENBINDING_IDENTIFIER
			{
				// Token: 0x04003D39 RID: 15673
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_KEY_PARAMETERS_TYPE keyType;
			}

			// Token: 0x0200090D RID: 2317
			internal struct TOKENBINDING_IDENTIFIER_V1
			{
				// Token: 0x04003D3A RID: 15674
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE bindingType;

				// Token: 0x04003D3B RID: 15675
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_HASH_ALGORITHM_V1 hashAlgorithm;

				// Token: 0x04003D3C RID: 15676
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_SIGNATURE_ALGORITHM_V1 signatureAlgorithm;
			}

			// Token: 0x0200090E RID: 2318
			internal struct TOKENBINDING_RESULT_DATA
			{
				// Token: 0x04003D3D RID: 15677
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE bindingType;

				// Token: 0x04003D3E RID: 15678
				public uint identifierSize;

				// Token: 0x04003D3F RID: 15679
				public unsafe UnsafeNclNativeMethods.HttpApi.TOKENBINDING_IDENTIFIER* identifierData;

				// Token: 0x04003D40 RID: 15680
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_EXTENSION_FORMAT extensionFormat;

				// Token: 0x04003D41 RID: 15681
				public uint extensionSize;

				// Token: 0x04003D42 RID: 15682
				public IntPtr extensionData;
			}

			// Token: 0x0200090F RID: 2319
			internal struct TOKENBINDING_RESULT_DATA_V1
			{
				// Token: 0x04003D43 RID: 15683
				public uint identifierSize;

				// Token: 0x04003D44 RID: 15684
				public unsafe UnsafeNclNativeMethods.HttpApi.TOKENBINDING_IDENTIFIER_V1* identifierData;

				// Token: 0x04003D45 RID: 15685
				public UnsafeNclNativeMethods.HttpApi.TOKENBINDING_EXTENSION_FORMAT extensionFormat;

				// Token: 0x04003D46 RID: 15686
				public uint extensionSize;

				// Token: 0x04003D47 RID: 15687
				public IntPtr extensionData;
			}

			// Token: 0x02000910 RID: 2320
			internal struct TOKENBINDING_RESULT_LIST
			{
				// Token: 0x04003D48 RID: 15688
				public uint resultCount;

				// Token: 0x04003D49 RID: 15689
				public unsafe UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_DATA* resultData;
			}

			// Token: 0x02000911 RID: 2321
			internal struct TOKENBINDING_RESULT_LIST_V1
			{
				// Token: 0x04003D4A RID: 15690
				public uint resultCount;

				// Token: 0x04003D4B RID: 15691
				public unsafe UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_DATA_V1* resultData;
			}

			// Token: 0x02000912 RID: 2322
			[Flags]
			internal enum HTTP_FLAGS : uint
			{
				// Token: 0x04003D4D RID: 15693
				NONE = 0U,
				// Token: 0x04003D4E RID: 15694
				HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY = 1U,
				// Token: 0x04003D4F RID: 15695
				HTTP_RECEIVE_SECURE_CHANNEL_TOKEN = 1U,
				// Token: 0x04003D50 RID: 15696
				HTTP_SEND_RESPONSE_FLAG_DISCONNECT = 1U,
				// Token: 0x04003D51 RID: 15697
				HTTP_SEND_RESPONSE_FLAG_MORE_DATA = 2U,
				// Token: 0x04003D52 RID: 15698
				HTTP_SEND_RESPONSE_FLAG_BUFFER_DATA = 4U,
				// Token: 0x04003D53 RID: 15699
				HTTP_SEND_RESPONSE_FLAG_RAW_HEADER = 4U,
				// Token: 0x04003D54 RID: 15700
				HTTP_SEND_REQUEST_FLAG_MORE_DATA = 1U,
				// Token: 0x04003D55 RID: 15701
				HTTP_PROPERTY_FLAG_PRESENT = 1U,
				// Token: 0x04003D56 RID: 15702
				HTTP_INITIALIZE_SERVER = 1U,
				// Token: 0x04003D57 RID: 15703
				HTTP_INITIALIZE_CBT = 4U,
				// Token: 0x04003D58 RID: 15704
				HTTP_SEND_RESPONSE_FLAG_OPAQUE = 64U
			}

			// Token: 0x02000913 RID: 2323
			internal static class HTTP_REQUEST_HEADER_ID
			{
				// Token: 0x06004633 RID: 17971 RVA: 0x001250ED File Offset: 0x001232ED
				internal static string ToString(int position)
				{
					return UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.m_Strings[position];
				}

				// Token: 0x04003D59 RID: 15705
				private static string[] m_Strings = new string[]
				{
					"Cache-Control", "Connection", "Date", "Keep-Alive", "Pragma", "Trailer", "Transfer-Encoding", "Upgrade", "Via", "Warning",
					"Allow", "Content-Length", "Content-Type", "Content-Encoding", "Content-Language", "Content-Location", "Content-MD5", "Content-Range", "Expires", "Last-Modified",
					"Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language", "Authorization", "Cookie", "Expect", "From", "Host", "If-Match",
					"If-Modified-Since", "If-None-Match", "If-Range", "If-Unmodified-Since", "Max-Forwards", "Proxy-Authorization", "Referer", "Range", "Te", "Translate",
					"User-Agent"
				};
			}

			// Token: 0x02000914 RID: 2324
			internal static class HTTP_RESPONSE_HEADER_ID
			{
				// Token: 0x06004635 RID: 17973 RVA: 0x0012527C File Offset: 0x0012347C
				static HTTP_RESPONSE_HEADER_ID()
				{
					for (int i = 0; i < 30; i++)
					{
						UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.m_Hashtable.Add(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.m_Strings[i], i);
					}
				}

				// Token: 0x06004636 RID: 17974 RVA: 0x001253CC File Offset: 0x001235CC
				internal static int IndexOfKnownHeader(string HeaderName)
				{
					object obj = UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.m_Hashtable[HeaderName];
					if (obj != null)
					{
						return (int)obj;
					}
					return -1;
				}

				// Token: 0x06004637 RID: 17975 RVA: 0x001253F0 File Offset: 0x001235F0
				internal static string ToString(int position)
				{
					return UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.m_Strings[position];
				}

				// Token: 0x04003D5A RID: 15706
				private static Hashtable m_Hashtable = new Hashtable(30);

				// Token: 0x04003D5B RID: 15707
				private static string[] m_Strings = new string[]
				{
					"Cache-Control", "Connection", "Date", "Keep-Alive", "Pragma", "Trailer", "Transfer-Encoding", "Upgrade", "Via", "Warning",
					"Allow", "Content-Length", "Content-Type", "Content-Encoding", "Content-Language", "Content-Location", "Content-MD5", "Content-Range", "Expires", "Last-Modified",
					"Accept-Ranges", "Age", "ETag", "Location", "Proxy-Authenticate", "Retry-After", "Server", "Set-Cookie", "Vary", "WWW-Authenticate"
				};

				// Token: 0x0200093B RID: 2363
				internal enum Enum
				{
					// Token: 0x04003DE3 RID: 15843
					HttpHeaderCacheControl,
					// Token: 0x04003DE4 RID: 15844
					HttpHeaderConnection,
					// Token: 0x04003DE5 RID: 15845
					HttpHeaderDate,
					// Token: 0x04003DE6 RID: 15846
					HttpHeaderKeepAlive,
					// Token: 0x04003DE7 RID: 15847
					HttpHeaderPragma,
					// Token: 0x04003DE8 RID: 15848
					HttpHeaderTrailer,
					// Token: 0x04003DE9 RID: 15849
					HttpHeaderTransferEncoding,
					// Token: 0x04003DEA RID: 15850
					HttpHeaderUpgrade,
					// Token: 0x04003DEB RID: 15851
					HttpHeaderVia,
					// Token: 0x04003DEC RID: 15852
					HttpHeaderWarning,
					// Token: 0x04003DED RID: 15853
					HttpHeaderAllow,
					// Token: 0x04003DEE RID: 15854
					HttpHeaderContentLength,
					// Token: 0x04003DEF RID: 15855
					HttpHeaderContentType,
					// Token: 0x04003DF0 RID: 15856
					HttpHeaderContentEncoding,
					// Token: 0x04003DF1 RID: 15857
					HttpHeaderContentLanguage,
					// Token: 0x04003DF2 RID: 15858
					HttpHeaderContentLocation,
					// Token: 0x04003DF3 RID: 15859
					HttpHeaderContentMd5,
					// Token: 0x04003DF4 RID: 15860
					HttpHeaderContentRange,
					// Token: 0x04003DF5 RID: 15861
					HttpHeaderExpires,
					// Token: 0x04003DF6 RID: 15862
					HttpHeaderLastModified,
					// Token: 0x04003DF7 RID: 15863
					HttpHeaderAcceptRanges,
					// Token: 0x04003DF8 RID: 15864
					HttpHeaderAge,
					// Token: 0x04003DF9 RID: 15865
					HttpHeaderEtag,
					// Token: 0x04003DFA RID: 15866
					HttpHeaderLocation,
					// Token: 0x04003DFB RID: 15867
					HttpHeaderProxyAuthenticate,
					// Token: 0x04003DFC RID: 15868
					HttpHeaderRetryAfter,
					// Token: 0x04003DFD RID: 15869
					HttpHeaderServer,
					// Token: 0x04003DFE RID: 15870
					HttpHeaderSetCookie,
					// Token: 0x04003DFF RID: 15871
					HttpHeaderVary,
					// Token: 0x04003E00 RID: 15872
					HttpHeaderWwwAuthenticate,
					// Token: 0x04003E01 RID: 15873
					HttpHeaderResponseMaximum,
					// Token: 0x04003E02 RID: 15874
					HttpHeaderMaximum = 41
				}
			}
		}

		// Token: 0x02000722 RID: 1826
		[SuppressUnmanagedCodeSecurity]
		internal static class SecureStringHelper
		{
			// Token: 0x06004148 RID: 16712 RVA: 0x0010FB1C File Offset: 0x0010DD1C
			internal static string CreateString(SecureString secureString)
			{
				IntPtr intPtr = IntPtr.Zero;
				if (secureString == null || secureString.Length == 0)
				{
					return string.Empty;
				}
				string text;
				try
				{
					intPtr = Marshal.SecureStringToBSTR(secureString);
					text = Marshal.PtrToStringBSTR(intPtr);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.ZeroFreeBSTR(intPtr);
					}
				}
				return text;
			}

			// Token: 0x06004149 RID: 16713 RVA: 0x0010FB78 File Offset: 0x0010DD78
			internal unsafe static SecureString CreateSecureString(string plainString)
			{
				if (plainString == null || plainString.Length == 0)
				{
					return new SecureString();
				}
				SecureString secureString;
				fixed (string text = plainString)
				{
					char* ptr = text;
					if (ptr != null)
					{
						ptr += RuntimeHelpers.OffsetToStringData / 2;
					}
					secureString = new SecureString(ptr, plainString.Length);
				}
				return secureString;
			}
		}

		// Token: 0x02000723 RID: 1827
		[FriendAccessAllowed]
		internal class AppXHelper
		{
			// Token: 0x0600414A RID: 16714 RVA: 0x0010FBB8 File Offset: 0x0010DDB8
			[SecuritySafeCritical]
			private static IntPtr GetPrimaryWindowHandle()
			{
				IntPtr intPtr = IntPtr.Zero;
				UnsafeNclNativeMethods.AppXHelper.GuiThreadInfo guiThreadInfo = default(UnsafeNclNativeMethods.AppXHelper.GuiThreadInfo);
				guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);
				if (UnsafeNclNativeMethods.AppXHelper.GetGUIThreadInfo(0, ref guiThreadInfo) != 0U && guiThreadInfo.hwndActive != IntPtr.Zero)
				{
					int num;
					UnsafeNclNativeMethods.AppXHelper.GetWindowThreadProcessId(guiThreadInfo.hwndActive, out num);
					if (num == Process.GetCurrentProcess().Id)
					{
						intPtr = guiThreadInfo.hwndActive;
					}
				}
				return intPtr;
			}

			// Token: 0x0600414B RID: 16715
			[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
			private static extern uint GetGUIThreadInfo(int threadId, ref UnsafeNclNativeMethods.AppXHelper.GuiThreadInfo info);

			// Token: 0x0600414C RID: 16716
			[DllImport("user32.dll", ExactSpelling = true)]
			private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int processId);

			// Token: 0x04003148 RID: 12616
			[SecuritySafeCritical]
			internal static Lazy<IntPtr> PrimaryWindowHandle = new Lazy<IntPtr>(() => UnsafeNclNativeMethods.AppXHelper.GetPrimaryWindowHandle());

			// Token: 0x02000915 RID: 2325
			private struct GuiThreadInfo
			{
				// Token: 0x04003D5C RID: 15708
				public int cbSize;

				// Token: 0x04003D5D RID: 15709
				public int flags;

				// Token: 0x04003D5E RID: 15710
				public IntPtr hwndActive;

				// Token: 0x04003D5F RID: 15711
				public IntPtr hwndFocus;

				// Token: 0x04003D60 RID: 15712
				public IntPtr hwndCapture;

				// Token: 0x04003D61 RID: 15713
				public IntPtr hwndMenuOwner;

				// Token: 0x04003D62 RID: 15714
				public IntPtr hwndMoveSize;

				// Token: 0x04003D63 RID: 15715
				public IntPtr hwndCaret;

				// Token: 0x04003D64 RID: 15716
				public int left;

				// Token: 0x04003D65 RID: 15717
				public int top;

				// Token: 0x04003D66 RID: 15718
				public int right;

				// Token: 0x04003D67 RID: 15719
				public int bottom;
			}
		}

		// Token: 0x02000724 RID: 1828
		internal static class TokenBindingOSHelper
		{
			// Token: 0x0600414F RID: 16719 RVA: 0x0010FC48 File Offset: 0x0010DE48
			[SecurityCritical]
			private static void EnsureTokenBindingOSHelperInitialized()
			{
				if (UnsafeNclNativeMethods.TokenBindingOSHelper.s_Initialized)
				{
					return;
				}
				object obj = UnsafeNclNativeMethods.TokenBindingOSHelper.s_Lock;
				lock (obj)
				{
					if (!UnsafeNclNativeMethods.TokenBindingOSHelper.s_Initialized)
					{
						try
						{
							string text = Path.Combine(Environment.SystemDirectory, "tokenbinding.dll");
							SafeLoadLibrary safeLoadLibrary = SafeLoadLibrary.LoadLibraryEx(text);
							if (!safeLoadLibrary.IsInvalid)
							{
								UnsafeNclNativeMethods.TokenBindingOSHelper.s_supportsTokenBinding = safeLoadLibrary.HasFunction("TokenBindingVerifyMessage");
							}
							UnsafeNclNativeMethods.TokenBindingOSHelper.s_Initialized = true;
						}
						catch (Exception ex)
						{
							if (NclUtilities.IsFatal(ex))
							{
								throw;
							}
						}
					}
				}
			}

			// Token: 0x17000EF5 RID: 3829
			// (get) Token: 0x06004150 RID: 16720 RVA: 0x0010FCEC File Offset: 0x0010DEEC
			internal static bool SupportsTokenBinding
			{
				get
				{
					UnsafeNclNativeMethods.TokenBindingOSHelper.EnsureTokenBindingOSHelperInitialized();
					return UnsafeNclNativeMethods.TokenBindingOSHelper.s_supportsTokenBinding;
				}
			}

			// Token: 0x04003149 RID: 12617
			private static bool s_supportsTokenBinding = false;

			// Token: 0x0400314A RID: 12618
			private static object s_Lock = new object();

			// Token: 0x0400314B RID: 12619
			private static volatile bool s_Initialized = false;
		}
	}
}

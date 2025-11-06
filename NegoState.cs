using System;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Threading;

namespace System.Net.Security
{
	// Token: 0x0200035D RID: 861
	internal class NegoState
	{
		// Token: 0x06001F24 RID: 7972 RVA: 0x00090B5B File Offset: 0x0008ED5B
		internal NegoState(Stream innerStream, bool leaveStreamOpen)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this._InnerStream = innerStream;
			this._LeaveStreamOpen = leaveStreamOpen;
		}

		// Token: 0x17000818 RID: 2072
		// (get) Token: 0x06001F25 RID: 7973 RVA: 0x00090B7F File Offset: 0x0008ED7F
		internal static string DefaultPackage
		{
			get
			{
				return "Negotiate";
			}
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x00090B88 File Offset: 0x0008ED88
		internal void ValidateCreateContext(string package, NetworkCredential credential, string servicePrincipalName, ExtendedProtectionPolicy policy, ProtectionLevel protectionLevel, TokenImpersonationLevel impersonationLevel)
		{
			if (policy != null)
			{
				if (!AuthenticationManager.OSSupportsExtendedProtection)
				{
					if (policy.PolicyEnforcement == PolicyEnforcement.Always)
					{
						throw new PlatformNotSupportedException(SR.GetString("security_ExtendedProtection_NoOSSupport"));
					}
				}
				else if (policy.CustomChannelBinding == null && policy.CustomServiceNames == null)
				{
					throw new ArgumentException(SR.GetString("net_auth_must_specify_extended_protection_scheme"), "policy");
				}
				this._ExtendedProtectionPolicy = policy;
			}
			else
			{
				this._ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
			}
			this.ValidateCreateContext(package, true, credential, servicePrincipalName, this._ExtendedProtectionPolicy.CustomChannelBinding, protectionLevel, impersonationLevel);
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x00090C10 File Offset: 0x0008EE10
		internal void ValidateCreateContext(string package, bool isServer, NetworkCredential credential, string servicePrincipalName, ChannelBinding channelBinding, ProtectionLevel protectionLevel, TokenImpersonationLevel impersonationLevel)
		{
			if (this._Exception != null && !this._CanRetryAuthentication)
			{
				throw this._Exception;
			}
			if (this._Context != null && this._Context.IsValidContext)
			{
				throw new InvalidOperationException(SR.GetString("net_auth_reauth"));
			}
			if (credential == null)
			{
				throw new ArgumentNullException("credential");
			}
			if (servicePrincipalName == null)
			{
				throw new ArgumentNullException("servicePrincipalName");
			}
			if (impersonationLevel != TokenImpersonationLevel.Identification && impersonationLevel != TokenImpersonationLevel.Impersonation && impersonationLevel != TokenImpersonationLevel.Delegation)
			{
				throw new ArgumentOutOfRangeException("impersonationLevel", impersonationLevel.ToString(), SR.GetString("net_auth_supported_impl_levels"));
			}
			if (this._Context != null && this.IsServer != isServer)
			{
				throw new InvalidOperationException(SR.GetString("net_auth_client_server"));
			}
			this._Exception = null;
			this._RemoteOk = false;
			this._Framer = new StreamFramer(this._InnerStream);
			this._Framer.WriteHeader.MessageId = 22;
			this._ExpectedProtectionLevel = protectionLevel;
			this._ExpectedImpersonationLevel = (isServer ? impersonationLevel : TokenImpersonationLevel.None);
			this._WriteSequenceNumber = 0U;
			this._ReadSequenceNumber = 0U;
			ContextFlags contextFlags = ContextFlags.Connection;
			if (protectionLevel == ProtectionLevel.None && !isServer)
			{
				package = "NTLM";
			}
			else if (protectionLevel == ProtectionLevel.EncryptAndSign)
			{
				contextFlags |= ContextFlags.Confidentiality;
			}
			else if (protectionLevel == ProtectionLevel.Sign)
			{
				contextFlags |= ContextFlags.ReplayDetect | ContextFlags.SequenceDetect | ContextFlags.AcceptStream;
			}
			if (isServer)
			{
				if (this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported)
				{
					contextFlags |= ContextFlags.AllowMissingBindings;
				}
				if (this._ExtendedProtectionPolicy.PolicyEnforcement != PolicyEnforcement.Never && this._ExtendedProtectionPolicy.ProtectionScenario == ProtectionScenario.TrustedProxy)
				{
					contextFlags |= ContextFlags.ProxyBindings;
				}
			}
			else
			{
				if (protectionLevel != ProtectionLevel.None)
				{
					contextFlags |= ContextFlags.MutualAuth;
				}
				if (impersonationLevel == TokenImpersonationLevel.Identification)
				{
					contextFlags |= ContextFlags.AcceptIntegrity;
				}
				if (impersonationLevel == TokenImpersonationLevel.Delegation)
				{
					contextFlags |= ContextFlags.Delegate;
				}
			}
			this._CanRetryAuthentication = false;
			if (!(credential is SystemNetworkCredential))
			{
				ExceptionHelper.ControlPrincipalPermission.Demand();
			}
			try
			{
				this._Context = new NTAuthentication(isServer, package, credential, servicePrincipalName, contextFlags, channelBinding);
			}
			catch (Win32Exception ex)
			{
				throw new AuthenticationException(SR.GetString("net_auth_SSPI"), ex);
			}
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x00090DFC File Offset: 0x0008EFFC
		private Exception SetException(Exception e)
		{
			if (this._Exception == null || !(this._Exception is ObjectDisposedException))
			{
				this._Exception = e;
			}
			if (this._Exception != null && this._Context != null)
			{
				this._Context.CloseContext();
			}
			return this._Exception;
		}

		// Token: 0x17000819 RID: 2073
		// (get) Token: 0x06001F29 RID: 7977 RVA: 0x00090E3B File Offset: 0x0008F03B
		internal bool IsAuthenticated
		{
			get
			{
				return this._Context != null && this.HandshakeComplete && this._Exception == null && this._RemoteOk;
			}
		}

		// Token: 0x1700081A RID: 2074
		// (get) Token: 0x06001F2A RID: 7978 RVA: 0x00090E5D File Offset: 0x0008F05D
		internal bool IsMutuallyAuthenticated
		{
			get
			{
				return this.IsAuthenticated && !this._Context.IsNTLM && this._Context.IsMutualAuthFlag;
			}
		}

		// Token: 0x1700081B RID: 2075
		// (get) Token: 0x06001F2B RID: 7979 RVA: 0x00090E83 File Offset: 0x0008F083
		internal bool IsEncrypted
		{
			get
			{
				return this.IsAuthenticated && this._Context.IsConfidentialityFlag;
			}
		}

		// Token: 0x1700081C RID: 2076
		// (get) Token: 0x06001F2C RID: 7980 RVA: 0x00090E9A File Offset: 0x0008F09A
		internal bool IsSigned
		{
			get
			{
				return this.IsAuthenticated && (this._Context.IsIntegrityFlag || this._Context.IsConfidentialityFlag);
			}
		}

		// Token: 0x1700081D RID: 2077
		// (get) Token: 0x06001F2D RID: 7981 RVA: 0x00090EC0 File Offset: 0x0008F0C0
		internal bool IsServer
		{
			get
			{
				return this._Context != null && this._Context.IsServer;
			}
		}

		// Token: 0x1700081E RID: 2078
		// (get) Token: 0x06001F2E RID: 7982 RVA: 0x00090ED7 File Offset: 0x0008F0D7
		internal bool CanGetSecureStream
		{
			get
			{
				return this._Context.IsConfidentialityFlag || this._Context.IsIntegrityFlag;
			}
		}

		// Token: 0x1700081F RID: 2079
		// (get) Token: 0x06001F2F RID: 7983 RVA: 0x00090EF3 File Offset: 0x0008F0F3
		internal TokenImpersonationLevel AllowedImpersonation
		{
			get
			{
				this.CheckThrow(true);
				return this.PrivateImpersonationLevel;
			}
		}

		// Token: 0x17000820 RID: 2080
		// (get) Token: 0x06001F30 RID: 7984 RVA: 0x00090F02 File Offset: 0x0008F102
		private TokenImpersonationLevel PrivateImpersonationLevel
		{
			get
			{
				if (this._Context.IsDelegationFlag && this._Context.ProtocolName != "NTLM")
				{
					return TokenImpersonationLevel.Delegation;
				}
				if (!this._Context.IsIdentifyFlag)
				{
					return TokenImpersonationLevel.Impersonation;
				}
				return TokenImpersonationLevel.Identification;
			}
		}

		// Token: 0x17000821 RID: 2081
		// (get) Token: 0x06001F31 RID: 7985 RVA: 0x00090F3A File Offset: 0x0008F13A
		private bool HandshakeComplete
		{
			get
			{
				return this._Context.IsCompleted && this._Context.IsValidContext;
			}
		}

		// Token: 0x06001F32 RID: 7986 RVA: 0x00090F58 File Offset: 0x0008F158
		internal IIdentity GetIdentity()
		{
			this.CheckThrow(true);
			string text = (this._Context.IsServer ? this._Context.AssociatedName : this._Context.Spn);
			string text2 = "NTLM";
			text2 = this._Context.ProtocolName;
			if (this._Context.IsServer)
			{
				SafeCloseHandle safeCloseHandle = null;
				try
				{
					safeCloseHandle = this._Context.GetContextToken();
					string protocolName = this._Context.ProtocolName;
					return new WindowsIdentity(safeCloseHandle.DangerousGetHandle(), protocolName, WindowsAccountType.Normal, true);
				}
				catch (SecurityException)
				{
				}
				finally
				{
					if (safeCloseHandle != null)
					{
						safeCloseHandle.Close();
					}
				}
			}
			return new GenericIdentity(text, text2);
		}

		// Token: 0x06001F33 RID: 7987 RVA: 0x00091018 File Offset: 0x0008F218
		internal void CheckThrow(bool authSucessCheck)
		{
			if (this._Exception != null)
			{
				throw this._Exception;
			}
			if (authSucessCheck && !this.IsAuthenticated)
			{
				throw new InvalidOperationException(SR.GetString("net_auth_noauth"));
			}
		}

		// Token: 0x06001F34 RID: 7988 RVA: 0x00091044 File Offset: 0x0008F244
		internal void Close()
		{
			this._Exception = new ObjectDisposedException("NegotiateStream");
			if (this._Context != null)
			{
				this._Context.CloseContext();
			}
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x0009106C File Offset: 0x0008F26C
		internal void ProcessAuthentication(LazyAsyncResult lazyResult)
		{
			this.CheckThrow(false);
			if (Interlocked.Exchange(ref this._NestedAuth, 1) == 1)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidnestedcall", new object[]
				{
					(lazyResult == null) ? "BeginAuthenticate" : "Authenticate",
					"authenticate"
				}));
			}
			try
			{
				if (this._Context.IsServer)
				{
					this.StartReceiveBlob(lazyResult);
				}
				else
				{
					this.StartSendBlob(null, lazyResult);
				}
			}
			catch (Exception ex)
			{
				ex = this.SetException(ex);
				throw;
			}
			finally
			{
				if (lazyResult == null || this._Exception != null)
				{
					this._NestedAuth = 0;
				}
			}
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x0009111C File Offset: 0x0008F31C
		internal void EndProcessAuthentication(IAsyncResult result)
		{
			if (result == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult lazyAsyncResult = result as LazyAsyncResult;
			if (lazyAsyncResult == null)
			{
				throw new ArgumentException(SR.GetString("net_io_async_result", new object[] { result.GetType().FullName }), "asyncResult");
			}
			if (Interlocked.Exchange(ref this._NestedAuth, 0) == 0)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndAuthenticate" }));
			}
			lazyAsyncResult.InternalWaitForCompletion();
			Exception ex = lazyAsyncResult.Result as Exception;
			if (ex != null)
			{
				ex = this.SetException(ex);
				throw ex;
			}
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x000911B8 File Offset: 0x0008F3B8
		private bool CheckSpn()
		{
			if (this._Context.IsKerberos)
			{
				return true;
			}
			if (this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Never || this._ExtendedProtectionPolicy.CustomServiceNames == null)
			{
				return true;
			}
			if (!AuthenticationManager.OSSupportsExtendedProtection)
			{
				return true;
			}
			string clientSpecifiedSpn = this._Context.ClientSpecifiedSpn;
			if (string.IsNullOrEmpty(clientSpecifiedSpn))
			{
				return this._ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.WhenSupported;
			}
			return this._ExtendedProtectionPolicy.CustomServiceNames.Contains(clientSpecifiedSpn);
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x00091230 File Offset: 0x0008F430
		private void StartSendBlob(byte[] message, LazyAsyncResult lazyResult)
		{
			Win32Exception ex = null;
			if (message != NegoState._EmptyMessage)
			{
				message = this.GetOutgoingBlob(message, ref ex);
			}
			if (ex != null)
			{
				this.StartSendAuthResetSignal(lazyResult, message, ex);
				return;
			}
			if (this.HandshakeComplete)
			{
				if (this._Context.IsServer && !this.CheckSpn())
				{
					Exception ex2 = new AuthenticationException(SR.GetString("net_auth_bad_client_creds_or_target_mismatch"));
					int num = 1790;
					message = new byte[8];
					for (int i = message.Length - 1; i >= 0; i--)
					{
						message[i] = (byte)(num & 255);
						num = (int)((uint)num >> 8);
					}
					this.StartSendAuthResetSignal(lazyResult, message, ex2);
					return;
				}
				if (this.PrivateImpersonationLevel < this._ExpectedImpersonationLevel)
				{
					Exception ex3 = new AuthenticationException(SR.GetString("net_auth_context_expectation", new object[]
					{
						this._ExpectedImpersonationLevel.ToString(),
						this.PrivateImpersonationLevel.ToString()
					}));
					int num2 = 1790;
					message = new byte[8];
					for (int j = message.Length - 1; j >= 0; j--)
					{
						message[j] = (byte)(num2 & 255);
						num2 = (int)((uint)num2 >> 8);
					}
					this.StartSendAuthResetSignal(lazyResult, message, ex3);
					return;
				}
				ProtectionLevel protectionLevel = (this._Context.IsConfidentialityFlag ? ProtectionLevel.EncryptAndSign : (this._Context.IsIntegrityFlag ? ProtectionLevel.Sign : ProtectionLevel.None));
				if (protectionLevel < this._ExpectedProtectionLevel)
				{
					Exception ex4 = new AuthenticationException(SR.GetString("net_auth_context_expectation", new object[]
					{
						protectionLevel.ToString(),
						this._ExpectedProtectionLevel.ToString()
					}));
					int num3 = 1790;
					message = new byte[8];
					for (int k = message.Length - 1; k >= 0; k--)
					{
						message[k] = (byte)(num3 & 255);
						num3 = (int)((uint)num3 >> 8);
					}
					this.StartSendAuthResetSignal(lazyResult, message, ex4);
					return;
				}
				this._Framer.WriteHeader.MessageId = 20;
				if (this._Context.IsServer)
				{
					this._RemoteOk = true;
					if (message == null)
					{
						message = NegoState._EmptyMessage;
					}
				}
			}
			else if (message == null || message == NegoState._EmptyMessage)
			{
				throw new InternalException();
			}
			if (message != null)
			{
				if (lazyResult == null)
				{
					this._Framer.WriteMessage(message);
				}
				else
				{
					IAsyncResult asyncResult = this._Framer.BeginWriteMessage(message, NegoState._WriteCallback, lazyResult);
					if (!asyncResult.CompletedSynchronously)
					{
						return;
					}
					this._Framer.EndWriteMessage(asyncResult);
				}
			}
			this.CheckCompletionBeforeNextReceive(lazyResult);
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x00091490 File Offset: 0x0008F690
		private void CheckCompletionBeforeNextReceive(LazyAsyncResult lazyResult)
		{
			if (this.HandshakeComplete && this._RemoteOk)
			{
				if (lazyResult != null)
				{
					lazyResult.InvokeCallback();
				}
				return;
			}
			this.StartReceiveBlob(lazyResult);
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x000914B4 File Offset: 0x0008F6B4
		private void StartReceiveBlob(LazyAsyncResult lazyResult)
		{
			byte[] array;
			if (lazyResult == null)
			{
				array = this._Framer.ReadMessage();
			}
			else
			{
				IAsyncResult asyncResult = this._Framer.BeginReadMessage(NegoState._ReadCallback, lazyResult);
				if (!asyncResult.CompletedSynchronously)
				{
					return;
				}
				array = this._Framer.EndReadMessage(asyncResult);
			}
			this.ProcessReceivedBlob(array, lazyResult);
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x00091504 File Offset: 0x0008F704
		private void ProcessReceivedBlob(byte[] message, LazyAsyncResult lazyResult)
		{
			if (message == null)
			{
				throw new AuthenticationException(SR.GetString("net_auth_eof"), null);
			}
			if (this._Framer.ReadHeader.MessageId == 21)
			{
				Win32Exception ex = null;
				if (message.Length >= 8)
				{
					long num = 0L;
					for (int i = 0; i < 8; i++)
					{
						num = (num << 8) + (long)((ulong)message[i]);
					}
					ex = new Win32Exception((int)num);
				}
				if (ex != null)
				{
					if (ex.NativeErrorCode == -2146893044)
					{
						throw new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds"), ex);
					}
					if (ex.NativeErrorCode == 1790)
					{
						throw new AuthenticationException(SR.GetString("net_auth_context_expectation_remote"), ex);
					}
				}
				throw new AuthenticationException(SR.GetString("net_auth_alert"), ex);
			}
			if (this._Framer.ReadHeader.MessageId == 20)
			{
				this._RemoteOk = true;
			}
			else if (this._Framer.ReadHeader.MessageId != 22)
			{
				throw new AuthenticationException(SR.GetString("net_io_header_id", new object[]
				{
					"MessageId",
					this._Framer.ReadHeader.MessageId,
					22
				}), null);
			}
			this.CheckCompletionBeforeNextSend(message, lazyResult);
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x0009162C File Offset: 0x0008F82C
		private void CheckCompletionBeforeNextSend(byte[] message, LazyAsyncResult lazyResult)
		{
			if (!this.HandshakeComplete)
			{
				this.StartSendBlob(message, lazyResult);
				return;
			}
			if (!this._RemoteOk)
			{
				throw new AuthenticationException(SR.GetString("net_io_header_id", new object[]
				{
					"MessageId",
					this._Framer.ReadHeader.MessageId,
					20
				}), null);
			}
			if (lazyResult != null)
			{
				lazyResult.InvokeCallback();
			}
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x0009169C File Offset: 0x0008F89C
		private void StartSendAuthResetSignal(LazyAsyncResult lazyResult, byte[] message, Exception exception)
		{
			this._Framer.WriteHeader.MessageId = 21;
			Win32Exception ex = exception as Win32Exception;
			if (ex != null && ex.NativeErrorCode == -2146893044)
			{
				if (this.IsServer)
				{
					exception = new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds"), exception);
				}
				else
				{
					exception = new InvalidCredentialException(SR.GetString("net_auth_bad_client_creds_or_target_mismatch"), exception);
				}
			}
			if (!(exception is AuthenticationException))
			{
				exception = new AuthenticationException(SR.GetString("net_auth_SSPI"), exception);
			}
			if (lazyResult == null)
			{
				this._Framer.WriteMessage(message);
			}
			else
			{
				lazyResult.Result = exception;
				IAsyncResult asyncResult = this._Framer.BeginWriteMessage(message, NegoState._WriteCallback, lazyResult);
				if (!asyncResult.CompletedSynchronously)
				{
					return;
				}
				this._Framer.EndWriteMessage(asyncResult);
			}
			this._CanRetryAuthentication = true;
			throw exception;
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x00091764 File Offset: 0x0008F964
		private static void WriteCallback(IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
			{
				return;
			}
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)transportResult.AsyncState;
			try
			{
				NegoState negoState = (NegoState)lazyAsyncResult.AsyncObject;
				negoState._Framer.EndWriteMessage(transportResult);
				if (lazyAsyncResult.Result is Exception)
				{
					negoState._CanRetryAuthentication = true;
					throw (Exception)lazyAsyncResult.Result;
				}
				negoState.CheckCompletionBeforeNextReceive(lazyAsyncResult);
			}
			catch (Exception ex)
			{
				if (lazyAsyncResult.InternalPeekCompleted)
				{
					throw;
				}
				lazyAsyncResult.InvokeCallback(ex);
			}
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x000917EC File Offset: 0x0008F9EC
		private static void ReadCallback(IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
			{
				return;
			}
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)transportResult.AsyncState;
			try
			{
				NegoState negoState = (NegoState)lazyAsyncResult.AsyncObject;
				byte[] array = negoState._Framer.EndReadMessage(transportResult);
				negoState.ProcessReceivedBlob(array, lazyAsyncResult);
			}
			catch (Exception ex)
			{
				if (lazyAsyncResult.InternalPeekCompleted)
				{
					throw;
				}
				lazyAsyncResult.InvokeCallback(ex);
			}
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x00091858 File Offset: 0x0008FA58
		private byte[] GetOutgoingBlob(byte[] incomingBlob, ref Win32Exception e)
		{
			SecurityStatus securityStatus;
			byte[] array = this._Context.GetOutgoingBlob(incomingBlob, false, out securityStatus);
			if ((securityStatus & (SecurityStatus)(-2147483648)) != SecurityStatus.OK)
			{
				e = new Win32Exception((int)securityStatus);
				array = new byte[8];
				for (int i = array.Length - 1; i >= 0; i--)
				{
					array[i] = (byte)(securityStatus & (SecurityStatus)255);
					securityStatus >>= 8;
				}
			}
			if (array != null && array.Length == 0)
			{
				array = NegoState._EmptyMessage;
			}
			return array;
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x000918BA File Offset: 0x0008FABA
		internal int EncryptData(byte[] buffer, int offset, int count, ref byte[] outBuffer)
		{
			this.CheckThrow(true);
			this._WriteSequenceNumber += 1U;
			return this._Context.Encrypt(buffer, offset, count, ref outBuffer, this._WriteSequenceNumber);
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x000918E7 File Offset: 0x0008FAE7
		internal int DecryptData(byte[] buffer, int offset, int count, out int newOffset)
		{
			this.CheckThrow(true);
			this._ReadSequenceNumber += 1U;
			return this._Context.Decrypt(buffer, offset, count, out newOffset, this._ReadSequenceNumber);
		}

		// Token: 0x04001CDE RID: 7390
		private const int ERROR_TRUST_FAILURE = 1790;

		// Token: 0x04001CDF RID: 7391
		private static readonly byte[] _EmptyMessage = new byte[0];

		// Token: 0x04001CE0 RID: 7392
		private static readonly AsyncCallback _ReadCallback = new AsyncCallback(NegoState.ReadCallback);

		// Token: 0x04001CE1 RID: 7393
		private static readonly AsyncCallback _WriteCallback = new AsyncCallback(NegoState.WriteCallback);

		// Token: 0x04001CE2 RID: 7394
		private Stream _InnerStream;

		// Token: 0x04001CE3 RID: 7395
		private bool _LeaveStreamOpen;

		// Token: 0x04001CE4 RID: 7396
		private Exception _Exception;

		// Token: 0x04001CE5 RID: 7397
		private StreamFramer _Framer;

		// Token: 0x04001CE6 RID: 7398
		private NTAuthentication _Context;

		// Token: 0x04001CE7 RID: 7399
		private int _NestedAuth;

		// Token: 0x04001CE8 RID: 7400
		internal const int c_MaxReadFrameSize = 65536;

		// Token: 0x04001CE9 RID: 7401
		internal const int c_MaxWriteDataSize = 64512;

		// Token: 0x04001CEA RID: 7402
		private bool _CanRetryAuthentication;

		// Token: 0x04001CEB RID: 7403
		private ProtectionLevel _ExpectedProtectionLevel;

		// Token: 0x04001CEC RID: 7404
		private TokenImpersonationLevel _ExpectedImpersonationLevel;

		// Token: 0x04001CED RID: 7405
		private uint _WriteSequenceNumber;

		// Token: 0x04001CEE RID: 7406
		private uint _ReadSequenceNumber;

		// Token: 0x04001CEF RID: 7407
		private ExtendedProtectionPolicy _ExtendedProtectionPolicy;

		// Token: 0x04001CF0 RID: 7408
		private bool _RemoteOk;
	}
}

using System;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Net.Configuration;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Net
{
	/// <summary>Provides an HTTP-specific implementation of the <see cref="T:System.Net.WebRequest" /> class.</summary>
	// Token: 0x02000109 RID: 265
	[FriendAccessAllowed]
	[global::__DynamicallyInvokable]
	[Serializable]
	public class HttpWebRequest : WebRequest, ISerializable
	{
		// Token: 0x1700020D RID: 525
		// (get) Token: 0x060009B7 RID: 2487 RVA: 0x000361F2 File Offset: 0x000343F2
		// (set) Token: 0x060009B8 RID: 2488 RVA: 0x000361FA File Offset: 0x000343FA
		[FriendAccessAllowed]
		internal object ServerCertificateValidationCallbackContext { get; set; }

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x060009B9 RID: 2489 RVA: 0x00036203 File Offset: 0x00034403
		// (set) Token: 0x060009BA RID: 2490 RVA: 0x0003620B File Offset: 0x0003440B
		[FriendAccessAllowed]
		internal bool CheckCertificateRevocationList { get; set; }

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x060009BB RID: 2491 RVA: 0x00036214 File Offset: 0x00034414
		// (set) Token: 0x060009BC RID: 2492 RVA: 0x0003621C File Offset: 0x0003441C
		[FriendAccessAllowed]
		internal SslProtocols SslProtocols { get; set; }

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060009BD RID: 2493 RVA: 0x00036225 File Offset: 0x00034425
		// (set) Token: 0x060009BE RID: 2494 RVA: 0x0003622D File Offset: 0x0003442D
		[FriendAccessAllowed]
		internal RtcState RtcState { get; set; }

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x00036236 File Offset: 0x00034436
		internal TimerThread.Timer RequestTimer
		{
			get
			{
				return this._Timer;
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x060009C0 RID: 2496 RVA: 0x0003623E File Offset: 0x0003443E
		internal bool Aborted
		{
			get
			{
				return this.m_Aborted != 0;
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x060009C1 RID: 2497 RVA: 0x00036249 File Offset: 0x00034449
		// (set) Token: 0x060009C2 RID: 2498 RVA: 0x00036251 File Offset: 0x00034451
		internal Connection TunnelConnection
		{
			get
			{
				return this.m_TunnelConnection;
			}
			set
			{
				this.m_TunnelConnection = value;
			}
		}

		/// <summary>Gets or sets a value that indicates whether the request should follow redirection responses.</summary>
		/// <returns>
		///     <see langword="true" /> if the request should automatically follow redirection responses from the Internet resource; otherwise, <see langword="false" />. The default value is <see langword="true" />.</returns>
		// Token: 0x17000214 RID: 532
		// (get) Token: 0x060009C3 RID: 2499 RVA: 0x0003625A File Offset: 0x0003445A
		// (set) Token: 0x060009C4 RID: 2500 RVA: 0x00036267 File Offset: 0x00034467
		public virtual bool AllowAutoRedirect
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.AllowAutoRedirect) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.AllowAutoRedirect;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.AllowAutoRedirect;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to buffer the data sent to the Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> to enable buffering of the data sent to the Internet resource; <see langword="false" /> to disable buffering. The default is <see langword="true" />.</returns>
		// Token: 0x17000215 RID: 533
		// (get) Token: 0x060009C5 RID: 2501 RVA: 0x0003628A File Offset: 0x0003448A
		// (set) Token: 0x060009C6 RID: 2502 RVA: 0x00036297 File Offset: 0x00034497
		public virtual bool AllowWriteStreamBuffering
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.AllowWriteStreamBuffering) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.AllowWriteStreamBuffering;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.AllowWriteStreamBuffering;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to buffer the received from the Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> to buffer the received from the Internet resource; otherwise, <see langword="false" />.
		///     <see langword="true" /> to enable buffering of the data received from the Internet resource; <see langword="false" /> to disable buffering. The default is <see langword="false" />.</returns>
		// Token: 0x17000216 RID: 534
		// (get) Token: 0x060009C7 RID: 2503 RVA: 0x000362BA File Offset: 0x000344BA
		// (set) Token: 0x060009C8 RID: 2504 RVA: 0x000362BD File Offset: 0x000344BD
		[global::__DynamicallyInvokable]
		public virtual bool AllowReadStreamBuffering
		{
			[global::__DynamicallyInvokable]
			get
			{
				return false;
			}
			[global::__DynamicallyInvokable]
			set
			{
				if (value)
				{
					throw new InvalidOperationException(global::System.SR.GetString("NotSupported"));
				}
			}
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x060009C9 RID: 2505 RVA: 0x000362D2 File Offset: 0x000344D2
		// (set) Token: 0x060009CA RID: 2506 RVA: 0x000362DF File Offset: 0x000344DF
		private bool ExpectContinue
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.ExpectContinue) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.ExpectContinue;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.ExpectContinue;
			}
		}

		/// <summary>Gets a value that indicates whether a response has been received from an Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> if a response has been received; otherwise, <see langword="false" />.</returns>
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x00036302 File Offset: 0x00034502
		[global::__DynamicallyInvokable]
		public virtual bool HaveResponse
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._ReadAResult != null && this._ReadAResult.InternalPeekCompleted;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x060009CC RID: 2508 RVA: 0x00036319 File Offset: 0x00034519
		// (set) Token: 0x060009CD RID: 2509 RVA: 0x00036321 File Offset: 0x00034521
		internal bool NtlmKeepAlive
		{
			get
			{
				return this.m_NtlmKeepAlive;
			}
			set
			{
				this.m_NtlmKeepAlive = value;
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x060009CE RID: 2510 RVA: 0x0003632A File Offset: 0x0003452A
		// (set) Token: 0x060009CF RID: 2511 RVA: 0x00036332 File Offset: 0x00034532
		internal bool NeedsToReadForResponse
		{
			get
			{
				return this.m_NeedsToReadForResponse;
			}
			set
			{
				this.m_NeedsToReadForResponse = value;
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x060009D0 RID: 2512 RVA: 0x0003633B File Offset: 0x0003453B
		internal bool BodyStarted
		{
			get
			{
				return this.m_BodyStarted;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to make a persistent connection to the Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> if the request to the Internet resource should contain a <see langword="Connection" /> HTTP header with the value Keep-alive; otherwise, <see langword="false" />. The default is <see langword="true" />.</returns>
		// Token: 0x1700021C RID: 540
		// (get) Token: 0x060009D1 RID: 2513 RVA: 0x00036343 File Offset: 0x00034543
		// (set) Token: 0x060009D2 RID: 2514 RVA: 0x0003634B File Offset: 0x0003454B
		public bool KeepAlive
		{
			get
			{
				return this.m_KeepAlive;
			}
			set
			{
				this.m_KeepAlive = value;
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x060009D3 RID: 2515 RVA: 0x00036354 File Offset: 0x00034554
		// (set) Token: 0x060009D4 RID: 2516 RVA: 0x0003635C File Offset: 0x0003455C
		internal bool LockConnection
		{
			get
			{
				return this.m_LockConnection;
			}
			set
			{
				this.m_LockConnection = value;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to pipeline the request to the Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> if the request should be pipelined; otherwise, <see langword="false" />. The default is <see langword="true" />.</returns>
		// Token: 0x1700021E RID: 542
		// (get) Token: 0x060009D5 RID: 2517 RVA: 0x00036365 File Offset: 0x00034565
		// (set) Token: 0x060009D6 RID: 2518 RVA: 0x0003636D File Offset: 0x0003456D
		public bool Pipelined
		{
			get
			{
				return this.m_Pipelined;
			}
			set
			{
				this.m_Pipelined = value;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to send an Authorization header with the request.</summary>
		/// <returns>
		///     <see langword="true" /> to send an  HTTP Authorization header with requests after authentication has taken place; otherwise, <see langword="false" />. The default is <see langword="false" />.</returns>
		// Token: 0x1700021F RID: 543
		// (get) Token: 0x060009D7 RID: 2519 RVA: 0x00036376 File Offset: 0x00034576
		// (set) Token: 0x060009D8 RID: 2520 RVA: 0x0003637E File Offset: 0x0003457E
		public override bool PreAuthenticate
		{
			get
			{
				return this.m_PreAuthenticate;
			}
			set
			{
				this.m_PreAuthenticate = value;
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x060009D9 RID: 2521 RVA: 0x00036387 File Offset: 0x00034587
		// (set) Token: 0x060009DA RID: 2522 RVA: 0x00036395 File Offset: 0x00034595
		private bool ProxySet
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.ProxySet) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.ProxySet;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.ProxySet;
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x060009DB RID: 2523 RVA: 0x000363B9 File Offset: 0x000345B9
		private bool RequestSubmitted
		{
			get
			{
				return this.m_RequestSubmitted;
			}
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x000363C4 File Offset: 0x000345C4
		private bool SetRequestSubmitted()
		{
			bool requestSubmitted = this.RequestSubmitted;
			this.m_RequestSubmitted = true;
			return requestSubmitted;
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x060009DD RID: 2525 RVA: 0x000363E0 File Offset: 0x000345E0
		// (set) Token: 0x060009DE RID: 2526 RVA: 0x000363E8 File Offset: 0x000345E8
		internal bool Saw100Continue
		{
			get
			{
				return this.m_Saw100Continue;
			}
			set
			{
				this.m_Saw100Continue = value;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to allow high-speed NTLM-authenticated connection sharing.</summary>
		/// <returns>
		///     <see langword="true" /> to keep the authenticated connection open; otherwise, <see langword="false" />.</returns>
		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060009DF RID: 2527 RVA: 0x000363F1 File Offset: 0x000345F1
		// (set) Token: 0x060009E0 RID: 2528 RVA: 0x000363FF File Offset: 0x000345FF
		public bool UnsafeAuthenticatedConnectionSharing
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.UnsafeAuthenticatedConnectionSharing) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.UnsafeAuthenticatedConnectionSharing;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.UnsafeAuthenticatedConnectionSharing;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060009E1 RID: 2529 RVA: 0x0003642D File Offset: 0x0003462D
		internal bool UnsafeOrProxyAuthenticatedConnectionSharing
		{
			get
			{
				return this.m_IsCurrentAuthenticationStateProxy || this.UnsafeAuthenticatedConnectionSharing;
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060009E2 RID: 2530 RVA: 0x0003643F File Offset: 0x0003463F
		// (set) Token: 0x060009E3 RID: 2531 RVA: 0x00036450 File Offset: 0x00034650
		private bool IsVersionHttp10
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.IsVersionHttp10) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.IsVersionHttp10;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.IsVersionHttp10;
			}
		}

		/// <summary>Gets or sets a value that indicates whether to send data in segments to the Internet resource.</summary>
		/// <returns>
		///     <see langword="true" /> to send data to the Internet resource in segments; otherwise, <see langword="false" />. The default value is <see langword="false" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060009E4 RID: 2532 RVA: 0x0003647A File Offset: 0x0003467A
		// (set) Token: 0x060009E5 RID: 2533 RVA: 0x0003648C File Offset: 0x0003468C
		public bool SendChunked
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.SendChunked) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_writestarted"));
				}
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.SendChunked;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.SendChunked;
			}
		}

		/// <summary>Gets or sets the type of decompression that is used.</summary>
		/// <returns>A T:System.Net.DecompressionMethods object that indicates the type of decompression that is used. </returns>
		/// <exception cref="T:System.InvalidOperationException">The object's current state does not allow this property to be set.</exception>
		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060009E6 RID: 2534 RVA: 0x000364D9 File Offset: 0x000346D9
		// (set) Token: 0x060009E7 RID: 2535 RVA: 0x000364E1 File Offset: 0x000346E1
		public DecompressionMethods AutomaticDecompression
		{
			get
			{
				return this.m_AutomaticDecompression;
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_writestarted"));
				}
				this.m_AutomaticDecompression = value;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060009E8 RID: 2536 RVA: 0x00036502 File Offset: 0x00034702
		// (set) Token: 0x060009E9 RID: 2537 RVA: 0x0003650A File Offset: 0x0003470A
		internal HttpWriteMode HttpWriteMode
		{
			get
			{
				return this._HttpWriteMode;
			}
			set
			{
				this._HttpWriteMode = value;
			}
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x00036513 File Offset: 0x00034713
		internal string AuthHeader(HttpResponseHeader header)
		{
			if (this._HttpResponse == null)
			{
				return null;
			}
			return this._HttpResponse.Headers[header];
		}

		/// <summary>Gets or sets the default cache policy for this request.</summary>
		/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> that specifies the cache policy in effect for this request when no other policy is applicable.</returns>
		// Token: 0x17000229 RID: 553
		// (get) Token: 0x060009EB RID: 2539 RVA: 0x00036530 File Offset: 0x00034730
		// (set) Token: 0x060009EC RID: 2540 RVA: 0x00036558 File Offset: 0x00034758
		public new static RequestCachePolicy DefaultCachePolicy
		{
			get
			{
				RequestCachePolicy policy = RequestCacheManager.GetBinding(Uri.UriSchemeHttp).Policy;
				if (policy == null)
				{
					return WebRequest.DefaultCachePolicy;
				}
				return policy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				RequestCacheBinding binding = RequestCacheManager.GetBinding(Uri.UriSchemeHttp);
				RequestCacheManager.SetBinding(Uri.UriSchemeHttp, new RequestCacheBinding(binding.Cache, binding.Validator, value));
			}
		}

		/// <summary>Gets or sets the default for the <see cref="P:System.Net.HttpWebRequest.MaximumResponseHeadersLength" /> property.</summary>
		/// <returns>The length, in kilobytes (1024 bytes), of the default maximum for response headers received. The default configuration file sets this value to 64 kilobytes.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value is not equal to -1 and is less than zero. </exception>
		// Token: 0x1700022A RID: 554
		// (get) Token: 0x060009ED RID: 2541 RVA: 0x00036596 File Offset: 0x00034796
		// (set) Token: 0x060009EE RID: 2542 RVA: 0x000365A2 File Offset: 0x000347A2
		public static int DefaultMaximumResponseHeadersLength
		{
			get
			{
				return SettingsSectionInternal.Section.MaximumResponseHeadersLength;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_toosmall"));
				}
				SettingsSectionInternal.Section.MaximumResponseHeadersLength = value;
			}
		}

		/// <summary>Gets or sets the default maximum length of an HTTP error response.</summary>
		/// <returns>The default maximum length of an HTTP error response.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
		// Token: 0x1700022B RID: 555
		// (get) Token: 0x060009EF RID: 2543 RVA: 0x000365D6 File Offset: 0x000347D6
		// (set) Token: 0x060009F0 RID: 2544 RVA: 0x000365E2 File Offset: 0x000347E2
		public static int DefaultMaximumErrorResponseLength
		{
			get
			{
				return SettingsSectionInternal.Section.MaximumErrorResponseLength;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_toosmall"));
				}
				SettingsSectionInternal.Section.MaximumErrorResponseLength = value;
			}
		}

		/// <summary>Gets or sets the maximum allowed length of the response headers.</summary>
		/// <returns>The length, in kilobytes (1024 bytes), of the response headers.</returns>
		/// <exception cref="T:System.InvalidOperationException">The property is set after the request has already been submitted. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value is less than 0 and is not equal to -1. </exception>
		// Token: 0x1700022C RID: 556
		// (get) Token: 0x060009F1 RID: 2545 RVA: 0x00036616 File Offset: 0x00034816
		// (set) Token: 0x060009F2 RID: 2546 RVA: 0x0003661E File Offset: 0x0003481E
		public int MaximumResponseHeadersLength
		{
			get
			{
				return this._MaximumResponseHeadersLength;
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
				}
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_toosmall"));
				}
				this._MaximumResponseHeadersLength = value;
			}
		}

		// Token: 0x1700022D RID: 557
		// (set) Token: 0x060009F3 RID: 2547 RVA: 0x0003665C File Offset: 0x0003485C
		internal HttpAbortDelegate AbortDelegate
		{
			set
			{
				this._AbortDelegate = value;
			}
		}

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x060009F4 RID: 2548 RVA: 0x00036665 File Offset: 0x00034865
		internal LazyAsyncResult ConnectionAsyncResult
		{
			get
			{
				return this._ConnectionAResult;
			}
		}

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x060009F5 RID: 2549 RVA: 0x0003666D File Offset: 0x0003486D
		internal LazyAsyncResult ConnectionReaderAsyncResult
		{
			get
			{
				return this._ConnectionReaderAResult;
			}
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x060009F6 RID: 2550 RVA: 0x00036675 File Offset: 0x00034875
		internal bool UserRetrievedWriteStream
		{
			get
			{
				return this._WriteAResult != null && this._WriteAResult.InternalPeekCompleted;
			}
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060009F7 RID: 2551 RVA: 0x0003668C File Offset: 0x0003488C
		private bool IsOutstandingGetRequestStream
		{
			get
			{
				return this._WriteAResult != null && !this._WriteAResult.InternalPeekCompleted;
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060009F8 RID: 2552 RVA: 0x000366A6 File Offset: 0x000348A6
		// (set) Token: 0x060009F9 RID: 2553 RVA: 0x000366B1 File Offset: 0x000348B1
		internal bool Async
		{
			get
			{
				return this._RequestIsAsync > TriState.False;
			}
			set
			{
				if (this._RequestIsAsync == TriState.Unspecified)
				{
					this._RequestIsAsync = (value ? TriState.True : TriState.False);
				}
			}
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060009FA RID: 2554 RVA: 0x000366C9 File Offset: 0x000348C9
		// (set) Token: 0x060009FB RID: 2555 RVA: 0x000366D1 File Offset: 0x000348D1
		internal UnlockConnectionDelegate UnlockConnectionDelegate
		{
			get
			{
				return this._UnlockDelegate;
			}
			set
			{
				this._UnlockDelegate = value;
			}
		}

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060009FC RID: 2556 RVA: 0x000366DA File Offset: 0x000348DA
		private bool UsesProxy
		{
			get
			{
				return this.ServicePoint.InternalProxyServicePoint;
			}
		}

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060009FD RID: 2557 RVA: 0x000366E7 File Offset: 0x000348E7
		internal HttpStatusCode ResponseStatusCode
		{
			get
			{
				return this._HttpResponse.StatusCode;
			}
		}

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x060009FE RID: 2558 RVA: 0x000366F4 File Offset: 0x000348F4
		internal bool UsesProxySemantics
		{
			get
			{
				return this.ServicePoint.InternalProxyServicePoint && ((this._Uri.Scheme != Uri.UriSchemeHttps && !this.IsWebSocketRequest) || this.IsTunnelRequest);
			}
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x060009FF RID: 2559 RVA: 0x00036727 File Offset: 0x00034927
		internal Uri ChallengedUri
		{
			get
			{
				return this.CurrentAuthenticationState.ChallengedUri;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000A00 RID: 2560 RVA: 0x00036734 File Offset: 0x00034934
		internal AuthenticationState ProxyAuthenticationState
		{
			get
			{
				if (this._ProxyAuthenticationState == null)
				{
					this._ProxyAuthenticationState = new AuthenticationState(true);
				}
				return this._ProxyAuthenticationState;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000A01 RID: 2561 RVA: 0x00036750 File Offset: 0x00034950
		// (set) Token: 0x06000A02 RID: 2562 RVA: 0x0003676C File Offset: 0x0003496C
		internal AuthenticationState ServerAuthenticationState
		{
			get
			{
				if (this._ServerAuthenticationState == null)
				{
					this._ServerAuthenticationState = new AuthenticationState(false);
				}
				return this._ServerAuthenticationState;
			}
			set
			{
				this._ServerAuthenticationState = value;
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000A03 RID: 2563 RVA: 0x00036775 File Offset: 0x00034975
		// (set) Token: 0x06000A04 RID: 2564 RVA: 0x0003678C File Offset: 0x0003498C
		internal AuthenticationState CurrentAuthenticationState
		{
			get
			{
				if (!this.m_IsCurrentAuthenticationStateProxy)
				{
					return this._ServerAuthenticationState;
				}
				return this._ProxyAuthenticationState;
			}
			set
			{
				this.m_IsCurrentAuthenticationStateProxy = this._ProxyAuthenticationState == value;
			}
		}

		/// <summary>Gets or sets the collection of security certificates that are associated with this request.</summary>
		/// <returns>The <see cref="T:System.Security.Cryptography.X509Certificates.X509CertificateCollection" /> that contains the security certificates associated with this request.</returns>
		/// <exception cref="T:System.ArgumentNullException">The value specified for a set operation is <see langword="null" />. </exception>
		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000A05 RID: 2565 RVA: 0x0003679D File Offset: 0x0003499D
		// (set) Token: 0x06000A06 RID: 2566 RVA: 0x000367B8 File Offset: 0x000349B8
		public X509CertificateCollection ClientCertificates
		{
			get
			{
				if (this._ClientCertificates == null)
				{
					this._ClientCertificates = new X509CertificateCollection();
				}
				return this._ClientCertificates;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this._ClientCertificates = value;
			}
		}

		/// <summary>Gets or sets the cookies associated with the request.</summary>
		/// <returns>A <see cref="T:System.Net.CookieContainer" /> that contains the cookies associated with this request.</returns>
		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000A07 RID: 2567 RVA: 0x000367CF File Offset: 0x000349CF
		// (set) Token: 0x06000A08 RID: 2568 RVA: 0x000367D7 File Offset: 0x000349D7
		[global::__DynamicallyInvokable]
		public virtual CookieContainer CookieContainer
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._CookieContainer;
			}
			[global::__DynamicallyInvokable]
			set
			{
				this._CookieContainer = value;
			}
		}

		/// <summary>Gets a value that indicates whether the request provides support for a <see cref="T:System.Net.CookieContainer" />.</summary>
		/// <returns>
		///     <see langword="true" /> if the request provides support for a <see cref="T:System.Net.CookieContainer" />; otherwise, <see langword="false" />.
		///     <see langword="true" /> if a <see cref="T:System.Net.CookieContainer" /> is supported; otherwise, <see langword="false" />. </returns>
		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000A09 RID: 2569 RVA: 0x000367E0 File Offset: 0x000349E0
		[global::__DynamicallyInvokable]
		public virtual bool SupportsCookieContainer
		{
			[global::__DynamicallyInvokable]
			get
			{
				return true;
			}
		}

		/// <summary>Gets the original Uniform Resource Identifier (URI) of the request.</summary>
		/// <returns>A <see cref="T:System.Uri" /> that contains the URI of the Internet resource passed to the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method.</returns>
		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000A0A RID: 2570 RVA: 0x000367E3 File Offset: 0x000349E3
		[global::__DynamicallyInvokable]
		public override Uri RequestUri
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._OriginUri;
			}
		}

		/// <summary>Gets or sets the <see langword="Content-length" /> HTTP header.</summary>
		/// <returns>The number of bytes of data to send to the Internet resource. The default is -1, which indicates the property has not been set and that there is no request data to send.</returns>
		/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The new <see cref="P:System.Net.HttpWebRequest.ContentLength" /> value is less than 0. </exception>
		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000A0B RID: 2571 RVA: 0x000367EB File Offset: 0x000349EB
		// (set) Token: 0x06000A0C RID: 2572 RVA: 0x000367F4 File Offset: 0x000349F4
		public override long ContentLength
		{
			get
			{
				return this._ContentLength;
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_writestarted"));
				}
				if (value < 0L)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_clsmall"));
				}
				this._ContentLength = value;
				this._originalContentLength = value;
			}
		}

		/// <summary>Gets or sets the time-out value in milliseconds for the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> and <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> methods.</summary>
		/// <returns>The number of milliseconds to wait before the request times out. The default value is 100,000 milliseconds (100 seconds).</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified is less than zero and is not <see cref="F:System.Threading.Timeout.Infinite" />.</exception>
		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000A0D RID: 2573 RVA: 0x00036841 File Offset: 0x00034A41
		// (set) Token: 0x06000A0E RID: 2574 RVA: 0x00036849 File Offset: 0x00034A49
		public override int Timeout
		{
			get
			{
				return this._Timeout;
			}
			set
			{
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_io_timeout_use_ge_zero"));
				}
				if (this._Timeout != value)
				{
					this._Timeout = value;
					this._TimerQueue = null;
				}
			}
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000A0F RID: 2575 RVA: 0x00036880 File Offset: 0x00034A80
		private TimerThread.Queue TimerQueue
		{
			get
			{
				TimerThread.Queue queue = this._TimerQueue;
				if (queue == null)
				{
					queue = TimerThread.GetOrCreateQueue((this._Timeout == 0) ? 1 : this._Timeout);
					this._TimerQueue = queue;
				}
				return queue;
			}
		}

		/// <summary>Gets or sets a time-out in milliseconds when writing to or reading from a stream.</summary>
		/// <returns>The number of milliseconds before the writing or reading times out. The default value is 300,000 milliseconds (5 minutes).</returns>
		/// <exception cref="T:System.InvalidOperationException">The request has already been sent. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value specified for a set operation is less than or equal to zero and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /></exception>
		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000A10 RID: 2576 RVA: 0x000368B6 File Offset: 0x00034AB6
		// (set) Token: 0x06000A11 RID: 2577 RVA: 0x000368BE File Offset: 0x00034ABE
		public int ReadWriteTimeout
		{
			get
			{
				return this._ReadWriteTimeout;
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
				}
				if (value <= 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_io_timeout_use_gt_zero"));
				}
				this._ReadWriteTimeout = value;
			}
		}

		/// <summary>Gets or sets a timeout, in milliseconds, to wait until the 100-Continue is received from the server. </summary>
		/// <returns>The timeout, in milliseconds, to wait until the 100-Continue is received. </returns>
		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000A12 RID: 2578 RVA: 0x000368FC File Offset: 0x00034AFC
		// (set) Token: 0x06000A13 RID: 2579 RVA: 0x00036904 File Offset: 0x00034B04
		[global::__DynamicallyInvokable]
		public int ContinueTimeout
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this.m_ContinueTimeout;
			}
			[global::__DynamicallyInvokable]
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
				}
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException("value", global::System.SR.GetString("net_io_timeout_use_ge_zero"));
				}
				if (this.m_ContinueTimeout != value)
				{
					this.m_ContinueTimeout = value;
					if (value == 350)
					{
						this.m_ContinueTimerQueue = HttpWebRequest.s_ContinueTimerQueue;
						return;
					}
					this.m_ContinueTimerQueue = null;
				}
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000A14 RID: 2580 RVA: 0x00036971 File Offset: 0x00034B71
		private TimerThread.Queue ContinueTimerQueue
		{
			get
			{
				if (this.m_ContinueTimerQueue == null)
				{
					this.m_ContinueTimerQueue = TimerThread.GetOrCreateQueue((this.m_ContinueTimeout == 0) ? 1 : this.m_ContinueTimeout);
				}
				return this.m_ContinueTimerQueue;
			}
		}

		// Token: 0x06000A15 RID: 2581 RVA: 0x000369A0 File Offset: 0x00034BA0
		internal long SwitchToContentLength()
		{
			if (this.HaveResponse)
			{
				return -1L;
			}
			if (this.HttpWriteMode == HttpWriteMode.Chunked)
			{
				ConnectStream connectStream = this._OldSubmitWriteStream;
				if (connectStream == null)
				{
					connectStream = this._SubmitWriteStream;
				}
				if (connectStream.Connection != null && connectStream.Connection.IISVersion >= 6)
				{
					return -1L;
				}
			}
			long num = -1L;
			long contentLength = this._ContentLength;
			if (this.HttpWriteMode != HttpWriteMode.None)
			{
				if (this.HttpWriteMode == HttpWriteMode.Buffer)
				{
					this._ContentLength = (long)this._SubmitWriteStream.BufferedData.Length;
					this.m_OriginallyBuffered = true;
					this.HttpWriteMode = HttpWriteMode.ContentLength;
					return -1L;
				}
				if (this.NtlmKeepAlive && this._OldSubmitWriteStream == null)
				{
					this._ContentLength = 0L;
					this._SubmitWriteStream.SuppressWrite = true;
					if (!this._SubmitWriteStream.BufferOnly)
					{
						num = contentLength;
					}
					if (this.HttpWriteMode == HttpWriteMode.Chunked)
					{
						this.HttpWriteMode = HttpWriteMode.ContentLength;
						this._SubmitWriteStream.SwitchToContentLength();
						num = -2L;
						this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
					}
				}
				if (this._OldSubmitWriteStream != null)
				{
					if (this.NtlmKeepAlive)
					{
						this._ContentLength = 0L;
					}
					else if (this._ContentLength == 0L || this.HttpWriteMode == HttpWriteMode.Chunked)
					{
						if (this._resendRequestContent == null)
						{
							if (this._OldSubmitWriteStream.BufferedData != null)
							{
								this._ContentLength = (long)this._OldSubmitWriteStream.BufferedData.Length;
							}
						}
						else if (this.HttpWriteMode == HttpWriteMode.Chunked)
						{
							this._ContentLength = -1L;
						}
						else
						{
							this._ContentLength = this._originalContentLength;
						}
					}
					if (this.HttpWriteMode == HttpWriteMode.Chunked && (this._resendRequestContent == null || this.NtlmKeepAlive))
					{
						this.HttpWriteMode = HttpWriteMode.ContentLength;
						this._SubmitWriteStream.SwitchToContentLength();
						this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
						if (this._resendRequestContent != null)
						{
							num = -2L;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06000A16 RID: 2582 RVA: 0x00036B57 File Offset: 0x00034D57
		private void PostSwitchToContentLength(long value)
		{
			if (value > -1L)
			{
				this._ContentLength = value;
			}
			if (value == -2L)
			{
				this._ContentLength = -1L;
				this.HttpWriteMode = HttpWriteMode.Chunked;
			}
		}

		// Token: 0x06000A17 RID: 2583 RVA: 0x00036B7C File Offset: 0x00034D7C
		private void ClearAuthenticatedConnectionResources()
		{
			if (this.ProxyAuthenticationState.UniqueGroupId != null || this.ServerAuthenticationState.UniqueGroupId != null)
			{
				this.ServicePoint.ReleaseConnectionGroup(this.GetConnectionGroupLine());
			}
			UnlockConnectionDelegate unlockConnectionDelegate = this.UnlockConnectionDelegate;
			try
			{
				if (unlockConnectionDelegate != null)
				{
					unlockConnectionDelegate();
				}
				this.UnlockConnectionDelegate = null;
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
			}
			this.ProxyAuthenticationState.ClearSession(this);
			this.ServerAuthenticationState.ClearSession(this);
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000A18 RID: 2584 RVA: 0x00036C04 File Offset: 0x00034E04
		// (set) Token: 0x06000A19 RID: 2585 RVA: 0x00036C0C File Offset: 0x00034E0C
		internal bool HeadersCompleted
		{
			get
			{
				return this.m_HeadersCompleted;
			}
			set
			{
				this.m_HeadersCompleted = value;
			}
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x00036C18 File Offset: 0x00034E18
		private void CheckProtocol(bool onRequestStream)
		{
			if (!this.CanGetRequestStream)
			{
				if (onRequestStream)
				{
					throw new ProtocolViolationException(global::System.SR.GetString("net_nouploadonget"));
				}
				if ((this.HttpWriteMode != HttpWriteMode.Unknown && this.HttpWriteMode != HttpWriteMode.None) || this.ContentLength > 0L || this.SendChunked)
				{
					throw new ProtocolViolationException(global::System.SR.GetString("net_nocontentlengthonget"));
				}
				this.HttpWriteMode = HttpWriteMode.None;
			}
			else if (this.HttpWriteMode == HttpWriteMode.Unknown)
			{
				if (this.SendChunked)
				{
					if (this.ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11 || this.ServicePoint.HttpBehaviour == HttpBehaviour.Unknown)
					{
						this.HttpWriteMode = HttpWriteMode.Chunked;
					}
					else
					{
						if (!this.AllowWriteStreamBuffering)
						{
							throw new ProtocolViolationException(global::System.SR.GetString("net_nochunkuploadonhttp10"));
						}
						this.HttpWriteMode = HttpWriteMode.Buffer;
					}
				}
				else
				{
					this.HttpWriteMode = ((this.ContentLength >= 0L) ? HttpWriteMode.ContentLength : (onRequestStream ? HttpWriteMode.Buffer : HttpWriteMode.None));
				}
			}
			if (this.HttpWriteMode != HttpWriteMode.Chunked)
			{
				if ((onRequestStream || this._OriginVerb.Equals(KnownHttpVerb.Post) || this._OriginVerb.Equals(KnownHttpVerb.Put)) && this.ContentLength == -1L && !this.AllowWriteStreamBuffering && this.KeepAlive)
				{
					throw new ProtocolViolationException(global::System.SR.GetString("net_contentlengthmissing"));
				}
				if (!ValidationHelper.IsBlankString(this.TransferEncoding))
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_needchunked"));
				}
			}
		}

		/// <summary>Begins an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
		/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate. </param>
		/// <param name="state">The state object for this request. </param>
		/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request.</returns>
		/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is <see langword="true" />, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is <see langword="false" />, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
		/// <exception cref="T:System.InvalidOperationException">The stream is being used by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />-or- 
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />.-or- The thread pool is running out of threads. </exception>
		/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
		/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
		// Token: 0x06000A1B RID: 2587 RVA: 0x00036D64 File Offset: 0x00034F64
		[global::__DynamicallyInvokable]
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			bool flag = false;
			IAsyncResult asyncResult;
			try
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "BeginGetRequestStream", "");
				}
				this.CheckProtocol(true);
				ContextAwareResult contextAwareResult = new ContextAwareResult(this.IdentityRequired, true, this, state, callback);
				object obj = contextAwareResult.StartPostingAsyncOp();
				lock (obj)
				{
					if (this._WriteAResult != null && this._WriteAResult.InternalPeekCompleted)
					{
						if (this._WriteAResult.Result is Exception)
						{
							throw (Exception)this._WriteAResult.Result;
						}
						try
						{
							contextAwareResult.InvokeCallback(this._WriteAResult.Result);
							goto IL_0154;
						}
						catch (Exception ex)
						{
							this.Abort(ex, 1);
							throw;
						}
					}
					if (!this.RequestSubmitted && NclUtilities.IsThreadPoolLow())
					{
						Exception ex2 = new InvalidOperationException(global::System.SR.GetString("net_needmorethreads"));
						this.Abort(ex2, 1);
						throw ex2;
					}
					lock (this)
					{
						if (this._WriteAResult != null)
						{
							throw new InvalidOperationException(global::System.SR.GetString("net_repcall"));
						}
						if (this.SetRequestSubmitted())
						{
							throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
						}
						if (this._ReadAResult != null)
						{
							throw (Exception)this._ReadAResult.Result;
						}
						this._WriteAResult = contextAwareResult;
						this.Async = true;
					}
					this.CurrentMethod = this._OriginVerb;
					this.BeginSubmitRequest();
					IL_0154:
					contextAwareResult.FinishPostingAsyncOp();
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "BeginGetRequestStream", contextAwareResult);
				}
				flag = true;
				asyncResult = contextAwareResult;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogBeginGetRequestStream(flag, false);
				}
			}
			return asyncResult;
		}

		/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data.</summary>
		/// <param name="asyncResult">The pending request for a stream. </param>
		/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="asyncResult" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
		/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
		// Token: 0x06000A1C RID: 2588 RVA: 0x00036F74 File Offset: 0x00035174
		[global::__DynamicallyInvokable]
		public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			TransportContext transportContext;
			return this.EndGetRequestStream(asyncResult, out transportContext);
		}

		/// <summary>Ends an asynchronous request for a <see cref="T:System.IO.Stream" /> object to use to write data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
		/// <param name="asyncResult">The pending request for a stream.</param>
		/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
		/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />. </exception>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="asyncResult" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult" />. </exception>
		/// <exception cref="T:System.IO.IOException">The request did not complete, and no stream is available. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
		// Token: 0x06000A1D RID: 2589 RVA: 0x00036F8C File Offset: 0x0003518C
		public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
		{
			bool flag = false;
			Stream stream;
			try
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "EndGetRequestStream", "");
				}
				context = null;
				if (asyncResult == null)
				{
					throw new ArgumentNullException("asyncResult");
				}
				LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
				if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
				{
					throw new ArgumentException(global::System.SR.GetString("net_io_invalidasyncresult"), "asyncResult");
				}
				if (lazyAsyncResult.EndCalled)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_io_invalidendcall", new object[] { "EndGetRequestStream" }));
				}
				ConnectStream connectStream = lazyAsyncResult.InternalWaitForCompletion() as ConnectStream;
				lazyAsyncResult.EndCalled = true;
				if (connectStream == null)
				{
					if (Logging.On)
					{
						Logging.Exception(Logging.Web, this, "EndGetRequestStream", lazyAsyncResult.Result as Exception);
					}
					throw (Exception)lazyAsyncResult.Result;
				}
				context = new ConnectStreamContext(connectStream);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "EndGetRequestStream", connectStream);
				}
				flag = true;
				stream = connectStream;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogEndGetRequestStream(flag, false);
				}
			}
			return stream;
		}

		/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data.</summary>
		/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
		/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is <see langword="true" />, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is <see langword="false" />, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />. </exception>
		/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
		/// <exception cref="T:System.ObjectDisposedException">In a .NET Compact Framework application, a request stream with zero content length was not obtained and closed correctly. For more information about handling zero content length requests, see Network Programming in the .NET Compact Framework.</exception>
		// Token: 0x06000A1E RID: 2590 RVA: 0x000370A8 File Offset: 0x000352A8
		public override Stream GetRequestStream()
		{
			TransportContext transportContext;
			return this.GetRequestStream(out transportContext);
		}

		/// <summary>Gets a <see cref="T:System.IO.Stream" /> object to use to write request data and outputs the <see cref="T:System.Net.TransportContext" /> associated with the stream.</summary>
		/// <param name="context">The <see cref="T:System.Net.TransportContext" /> for the <see cref="T:System.IO.Stream" />.</param>
		/// <returns>A <see cref="T:System.IO.Stream" /> to use to write request data.</returns>
		/// <exception cref="T:System.Exception">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method was unable to obtain the <see cref="T:System.IO.Stream" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method is called more than once.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />. </exception>
		/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, requests that write data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
		/// <exception cref="T:System.Net.ProtocolViolationException">The <see cref="P:System.Net.HttpWebRequest.Method" /> property is GET or HEAD.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is <see langword="true" />, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is <see langword="false" />, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
		// Token: 0x06000A1F RID: 2591 RVA: 0x000370C0 File Offset: 0x000352C0
		public Stream GetRequestStream(out TransportContext context)
		{
			bool flag = false;
			Stream stream;
			try
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogBeginGetRequestStream(true, true);
				}
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "GetRequestStream", "");
				}
				context = null;
				this.CheckProtocol(true);
				if (this._WriteAResult == null || !this._WriteAResult.InternalPeekCompleted)
				{
					lock (this)
					{
						if (this._WriteAResult != null)
						{
							throw new InvalidOperationException(global::System.SR.GetString("net_repcall"));
						}
						if (this.SetRequestSubmitted())
						{
							throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
						}
						if (this._ReadAResult != null)
						{
							throw (Exception)this._ReadAResult.Result;
						}
						this._WriteAResult = new LazyAsyncResult(this, null, null);
						this.Async = false;
					}
					this.CurrentMethod = this._OriginVerb;
					while (this.m_Retry)
					{
						if (this._WriteAResult.InternalPeekCompleted)
						{
							break;
						}
						this._OldSubmitWriteStream = null;
						this._SubmitWriteStream = null;
						this.BeginSubmitRequest();
					}
					while (this.Aborted && !this._WriteAResult.InternalPeekCompleted)
					{
						if (!(this._CoreResponse is Exception))
						{
							Thread.SpinWait(1);
						}
						else
						{
							this.CheckWriteSideResponseProcessing();
						}
					}
				}
				ConnectStream connectStream = this._WriteAResult.InternalWaitForCompletion() as ConnectStream;
				this._WriteAResult.EndCalled = true;
				flag = true;
				if (connectStream == null)
				{
					if (Logging.On)
					{
						Logging.Exception(Logging.Web, this, "EndGetRequestStream", this._WriteAResult.Result as Exception);
					}
					throw (Exception)this._WriteAResult.Result;
				}
				context = new ConnectStreamContext(connectStream);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "GetRequestStream", connectStream);
				}
				stream = connectStream;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogEndGetRequestStream(flag, true);
				}
			}
			return stream;
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000A20 RID: 2592 RVA: 0x000372CC File Offset: 0x000354CC
		private bool CanGetRequestStream
		{
			get
			{
				return !this.CurrentMethod.ContentBodyNotAllowed;
			}
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x000372EC File Offset: 0x000354EC
		internal bool CanGetResponseStream
		{
			get
			{
				return !this.CurrentMethod.ExpectNoContentResponse;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000A22 RID: 2594 RVA: 0x0003730C File Offset: 0x0003550C
		internal bool RequireBody
		{
			get
			{
				return this.CurrentMethod.RequireContentBody;
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000A23 RID: 2595 RVA: 0x00037326 File Offset: 0x00035526
		internal bool HasEntityBody
		{
			get
			{
				return this.HttpWriteMode == HttpWriteMode.Chunked || this.HttpWriteMode == HttpWriteMode.Buffer || (this.HttpWriteMode == HttpWriteMode.ContentLength && this.ContentLength > 0L);
			}
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x00037354 File Offset: 0x00035554
		internal void ErrorStatusCodeNotify(Connection connection, bool isKeepAlive, bool fatal)
		{
			ConnectStream submitWriteStream = this._SubmitWriteStream;
			if (submitWriteStream != null && submitWriteStream.Connection == connection)
			{
				if (!fatal)
				{
					submitWriteStream.ErrorResponseNotify(isKeepAlive);
					return;
				}
				if (!this.Aborted)
				{
					submitWriteStream.FatalResponseNotify();
				}
			}
		}

		// Token: 0x06000A25 RID: 2597 RVA: 0x00037390 File Offset: 0x00035590
		private HttpProcessingResult DoSubmitRequestProcessing(ref Exception exception)
		{
			HttpProcessingResult httpProcessingResult = HttpProcessingResult.Continue;
			this.m_Retry = false;
			bool ntlmKeepAlive = this.NtlmKeepAlive;
			try
			{
				if (this._HttpResponse != null)
				{
					if (this._CookieContainer != null)
					{
						CookieModule.OnReceivedHeaders(this);
					}
					this.ProxyAuthenticationState.Update(this);
					this.ServerAuthenticationState.Update(this);
				}
				bool flag = false;
				bool flag2 = true;
				bool flag3 = false;
				if (this._HttpResponse == null)
				{
					flag = true;
				}
				else if (this.CheckResubmitForCache(ref exception) || this.CheckResubmit(ref exception, ref flag3))
				{
					flag = true;
					flag2 = false;
				}
				else if (flag3)
				{
					flag = false;
					flag2 = false;
					httpProcessingResult = HttpProcessingResult.WriteWait;
					this._AutoRedirects--;
					this.OpenWriteSideResponseWindow();
					ConnectionReturnResult connectionReturnResult = new ConnectionReturnResult(1);
					ConnectionReturnResult.Add(ref connectionReturnResult, this, this._HttpResponse.CoreResponseData);
					this.m_PendingReturnResult = connectionReturnResult;
					this._HttpResponse = null;
					this._SubmitWriteStream.FinishedAfterWrite = true;
					this.SetRequestContinue();
				}
				ServicePoint servicePoint = null;
				if (flag2)
				{
					WebException ex = exception as WebException;
					if (ex != null && ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal)
					{
						ProxyChain proxyChain = this._ProxyChain;
						if (proxyChain != null)
						{
							servicePoint = ServicePointManager.FindServicePoint(proxyChain);
						}
						flag = servicePoint != null;
					}
				}
				if (flag)
				{
					if (base.CacheProtocol != null && this._HttpResponse != null)
					{
						base.CacheProtocol.Reset();
					}
					this.ClearRequestForResubmit(ntlmKeepAlive);
					WebException ex2 = exception as WebException;
					if (ex2 != null && (ex2.Status == WebExceptionStatus.PipelineFailure || ex2.Status == WebExceptionStatus.KeepAliveFailure))
					{
						this.m_Extra401Retry = true;
					}
					if (servicePoint == null)
					{
						servicePoint = this.FindServicePoint(true);
					}
					else
					{
						this._ServicePoint = servicePoint;
					}
					if (this.Async)
					{
						this.SubmitRequest(servicePoint);
					}
					else
					{
						this.m_Retry = true;
					}
					httpProcessingResult = HttpProcessingResult.WriteWait;
				}
			}
			finally
			{
				if (httpProcessingResult == HttpProcessingResult.Continue)
				{
					this.ClearAuthenticatedConnectionResources();
				}
			}
			return httpProcessingResult;
		}

		/// <summary>Begins an asynchronous request to an Internet resource.</summary>
		/// <param name="callback">The <see cref="T:System.AsyncCallback" /> delegate </param>
		/// <param name="state">The state object for this request. </param>
		/// <returns>An <see cref="T:System.IAsyncResult" /> that references the asynchronous request for a response.</returns>
		/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />-or- 
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />.-or- The thread pool is running out of threads. </exception>
		/// <exception cref="T:System.Net.ProtocolViolationException">
		///         <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="true" />.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is <see langword="true" />, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is <see langword="false" />, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" /> and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT.-or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called. </exception>
		// Token: 0x06000A26 RID: 2598 RVA: 0x00037544 File Offset: 0x00035744
		[global::__DynamicallyInvokable]
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			bool flag = false;
			IAsyncResult asyncResult;
			try
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "BeginGetResponse", "");
				}
				if (!this.RequestSubmitted)
				{
					this.CheckProtocol(false);
				}
				ConnectStream connectStream = ((this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream);
				if (connectStream != null && !connectStream.IsClosed)
				{
					if (connectStream.BytesLeftToWrite > 0L)
					{
						throw new ProtocolViolationException(global::System.SR.GetString("net_entire_body_not_written"));
					}
					connectStream.Close();
				}
				else if (connectStream == null && this.HasEntityBody)
				{
					throw new ProtocolViolationException(global::System.SR.GetString("net_must_provide_request_body"));
				}
				ContextAwareResult contextAwareResult = new ContextAwareResult(this.IdentityRequired, true, this, state, callback);
				if (!this.RequestSubmitted && NclUtilities.IsThreadPoolLow())
				{
					Exception ex = new InvalidOperationException(global::System.SR.GetString("net_needmorethreads"));
					this.Abort(ex, 1);
					throw ex;
				}
				object obj = contextAwareResult.StartPostingAsyncOp();
				lock (obj)
				{
					bool flag3 = false;
					bool flag5;
					lock (this)
					{
						flag5 = this.SetRequestSubmitted();
						if (this.HaveResponse)
						{
							flag3 = true;
						}
						else
						{
							if (this._ReadAResult != null)
							{
								throw new InvalidOperationException(global::System.SR.GetString("net_repcall"));
							}
							this._ReadAResult = contextAwareResult;
							this.Async = true;
						}
					}
					this.CheckDeferredCallDone(connectStream);
					if (flag3)
					{
						if (Logging.On)
						{
							Logging.Exit(Logging.Web, this, "BeginGetResponse", this._ReadAResult.Result);
						}
						Exception ex2 = this._ReadAResult.Result as Exception;
						if (ex2 != null)
						{
							throw ex2;
						}
						try
						{
							contextAwareResult.InvokeCallback(this._ReadAResult.Result);
							goto IL_01C8;
						}
						catch (Exception ex3)
						{
							this.Abort(ex3, 1);
							throw;
						}
					}
					if (!flag5)
					{
						this.CurrentMethod = this._OriginVerb;
					}
					if (this._RerequestCount <= 0)
					{
						if (flag5)
						{
							goto IL_01C8;
						}
					}
					while (this.m_Retry)
					{
						this.BeginSubmitRequest();
					}
					IL_01C8:
					contextAwareResult.FinishPostingAsyncOp();
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "BeginGetResponse", contextAwareResult);
				}
				flag = true;
				asyncResult = contextAwareResult;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogBeginGetResponse(flag, false);
				}
			}
			return asyncResult;
		}

		/// <summary>Ends an asynchronous request to an Internet resource.</summary>
		/// <param name="asyncResult">The pending request for a response. </param>
		/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="asyncResult" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.InvalidOperationException">This method was called previously using <paramref name="asyncResult." />-or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> property is greater than 0 but the data has not been written to the request stream. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- An error occurred while processing the request. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="asyncResult" /> was not returned by the current instance from a call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
		// Token: 0x06000A27 RID: 2599 RVA: 0x000377C8 File Offset: 0x000359C8
		[global::__DynamicallyInvokable]
		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			bool flag = false;
			int num = -1;
			WebResponse webResponse;
			try
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "EndGetResponse", "");
				}
				if (asyncResult == null)
				{
					throw new ArgumentNullException("asyncResult");
				}
				LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
				if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
				{
					throw new ArgumentException(global::System.SR.GetString("net_io_invalidasyncresult"), "asyncResult");
				}
				if (lazyAsyncResult.EndCalled)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_io_invalidendcall", new object[] { "EndGetResponse" }));
				}
				HttpWebResponse httpWebResponse = lazyAsyncResult.InternalWaitForCompletion() as HttpWebResponse;
				lazyAsyncResult.EndCalled = true;
				if (httpWebResponse == null)
				{
					if (Logging.On)
					{
						Logging.Exception(Logging.Web, this, "EndGetResponse", lazyAsyncResult.Result as Exception);
					}
					NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestFailed);
					throw (Exception)lazyAsyncResult.Result;
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "EndGetResponse", httpWebResponse);
				}
				this.InitLifetimeTracking(httpWebResponse);
				num = HttpWebRequest.GetStatusCode(httpWebResponse);
				flag = true;
				webResponse = httpWebResponse;
			}
			catch (WebException ex)
			{
				HttpWebResponse httpWebResponse2 = ex.Response as HttpWebResponse;
				num = HttpWebRequest.GetStatusCode(httpWebResponse2);
				throw;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogEndGetResponse(flag, false, num);
				}
			}
			return webResponse;
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x00037934 File Offset: 0x00035B34
		private void CheckDeferredCallDone(ConnectStream stream)
		{
			object obj = Interlocked.Exchange(ref this.m_PendingReturnResult, DBNull.Value);
			if (obj == NclConstants.Sentinel)
			{
				this.EndSubmitRequest();
				return;
			}
			if (obj != null && obj != DBNull.Value)
			{
				stream.ProcessWriteCallDone(obj as ConnectionReturnResult);
			}
		}

		/// <summary>Returns a response from an Internet resource.</summary>
		/// <returns>A <see cref="T:System.Net.WebResponse" /> that contains the response from the Internet resource.</returns>
		/// <exception cref="T:System.InvalidOperationException">The stream is already in use by a previous call to <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to a value and <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />. </exception>
		/// <exception cref="T:System.Net.ProtocolViolationException">
		///         <see cref="P:System.Net.HttpWebRequest.Method" /> is GET or HEAD, and either <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater or equal to zero or <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="true" />.-or- 
		///         <see cref="P:System.Net.HttpWebRequest.KeepAlive" /> is <see langword="true" />, <see cref="P:System.Net.HttpWebRequest.AllowWriteStreamBuffering" /> is <see langword="false" />, <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is -1, <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />, and <see cref="P:System.Net.HttpWebRequest.Method" /> is POST or PUT. -or- The <see cref="T:System.Net.HttpWebRequest" /> has an entity body but the <see cref="M:System.Net.HttpWebRequest.GetResponse" /> method is called without calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" /> method. -or- The <see cref="P:System.Net.HttpWebRequest.ContentLength" /> is greater than zero, but the application does not write all of the promised data.</exception>
		/// <exception cref="T:System.NotSupportedException">The request cache validator indicated that the response for this request can be served from the cache; however, this request includes data to be sent to the server. Requests that send data must not use the cache. This exception can occur if you are using a custom cache validator that is incorrectly implemented. </exception>
		/// <exception cref="T:System.Net.WebException">
		///         <see cref="M:System.Net.HttpWebRequest.Abort" /> was previously called.-or- The time-out period for the request expired.-or- An error occurred while processing the request. </exception>
		// Token: 0x06000A29 RID: 2601 RVA: 0x00037978 File Offset: 0x00035B78
		public override WebResponse GetResponse()
		{
			bool flag = false;
			int num = -1;
			WebResponse webResponse;
			try
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogBeginGetResponse(true, true);
				}
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "GetResponse", "");
				}
				if (!this.RequestSubmitted)
				{
					this.CheckProtocol(false);
				}
				ConnectStream connectStream = ((this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream);
				if (connectStream != null && !connectStream.IsClosed)
				{
					if (connectStream.BytesLeftToWrite > 0L)
					{
						throw new ProtocolViolationException(global::System.SR.GetString("net_entire_body_not_written"));
					}
					connectStream.Close();
				}
				else if (connectStream == null && this.HasEntityBody)
				{
					throw new ProtocolViolationException(global::System.SR.GetString("net_must_provide_request_body"));
				}
				bool flag2 = false;
				HttpWebResponse httpWebResponse = null;
				bool flag4;
				lock (this)
				{
					flag4 = this.SetRequestSubmitted();
					if (this.HaveResponse)
					{
						flag2 = true;
						httpWebResponse = this._ReadAResult.Result as HttpWebResponse;
					}
					else
					{
						if (this._ReadAResult != null)
						{
							throw new InvalidOperationException(global::System.SR.GetString("net_repcall"));
						}
						this.Async = false;
						if (this.Async)
						{
							ContextAwareResult contextAwareResult = new ContextAwareResult(this.IdentityRequired, true, this, null, null);
							contextAwareResult.StartPostingAsyncOp(false);
							contextAwareResult.FinishPostingAsyncOp();
							this._ReadAResult = contextAwareResult;
						}
						else
						{
							this._ReadAResult = new LazyAsyncResult(this, null, null);
						}
					}
				}
				this.CheckDeferredCallDone(connectStream);
				if (!flag2)
				{
					if (this._Timer == null)
					{
						this._Timer = this.TimerQueue.CreateTimer(HttpWebRequest.s_TimeoutCallback, this);
					}
					if (!flag4)
					{
						this.CurrentMethod = this._OriginVerb;
					}
					while (this.m_Retry)
					{
						this.BeginSubmitRequest();
					}
					while (!this.Async && this.Aborted && !this._ReadAResult.InternalPeekCompleted)
					{
						if (!(this._CoreResponse is Exception))
						{
							Thread.SpinWait(1);
						}
						else
						{
							this.CheckWriteSideResponseProcessing();
						}
					}
					httpWebResponse = this._ReadAResult.InternalWaitForCompletion() as HttpWebResponse;
					this._ReadAResult.EndCalled = true;
				}
				if (httpWebResponse == null)
				{
					if (Logging.On)
					{
						Logging.Exception(Logging.Web, this, "GetResponse", this._ReadAResult.Result as Exception);
					}
					NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestFailed);
					throw (Exception)this._ReadAResult.Result;
				}
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "GetResponse", httpWebResponse);
				}
				if (!flag2)
				{
					this.InitLifetimeTracking(httpWebResponse);
				}
				num = HttpWebRequest.GetStatusCode(httpWebResponse);
				flag = true;
				webResponse = httpWebResponse;
			}
			catch (WebException ex)
			{
				HttpWebResponse httpWebResponse2 = ex.Response as HttpWebResponse;
				num = HttpWebRequest.GetStatusCode(httpWebResponse2);
				throw;
			}
			finally
			{
				if (FrameworkEventSource.Log.IsEnabled())
				{
					base.LogEndGetResponse(flag, true, num);
				}
			}
			return webResponse;
		}

		// Token: 0x06000A2A RID: 2602 RVA: 0x00037C70 File Offset: 0x00035E70
		private void InitLifetimeTracking(HttpWebResponse httpWebResponse)
		{
			IRequestLifetimeTracker requestLifetimeTracker = httpWebResponse.ResponseStream as IRequestLifetimeTracker;
			requestLifetimeTracker.TrackRequestLifetime(this.m_StartTimestamp);
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x00037C98 File Offset: 0x00035E98
		internal void WriteCallDone(ConnectStream stream, ConnectionReturnResult returnResult)
		{
			if (stream != ((this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream))
			{
				stream.ProcessWriteCallDone(returnResult);
				return;
			}
			if (!this.UserRetrievedWriteStream)
			{
				stream.ProcessWriteCallDone(returnResult);
				return;
			}
			if (stream.FinishedAfterWrite)
			{
				stream.ProcessWriteCallDone(returnResult);
				return;
			}
			object obj = ((returnResult == null) ? Missing.Value : returnResult);
			object obj2 = Interlocked.CompareExchange(ref this.m_PendingReturnResult, obj, null);
			if (obj2 == DBNull.Value)
			{
				stream.ProcessWriteCallDone(returnResult);
			}
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x00037D10 File Offset: 0x00035F10
		internal void NeedEndSubmitRequest()
		{
			object obj = Interlocked.CompareExchange(ref this.m_PendingReturnResult, NclConstants.Sentinel, null);
			if (obj == DBNull.Value)
			{
				this.EndSubmitRequest();
			}
		}

		/// <summary>Gets the Uniform Resource Identifier (URI) of the Internet resource that actually responds to the request.</summary>
		/// <returns>A <see cref="T:System.Uri" /> that identifies the Internet resource that actually responds to the request. The default is the URI used by the <see cref="M:System.Net.WebRequest.Create(System.String)" /> method to initialize the request.</returns>
		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000A2D RID: 2605 RVA: 0x00037D3D File Offset: 0x00035F3D
		public Uri Address
		{
			get
			{
				return this._Uri;
			}
		}

		/// <summary>Gets or sets the delegate method called when an HTTP 100-continue response is received from the Internet resource.</summary>
		/// <returns>A delegate that implements the callback method that executes when an HTTP Continue response is returned from the Internet resource. The default value is <see langword="null" />.</returns>
		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000A2E RID: 2606 RVA: 0x00037D45 File Offset: 0x00035F45
		// (set) Token: 0x06000A2F RID: 2607 RVA: 0x00037D4D File Offset: 0x00035F4D
		public HttpContinueDelegate ContinueDelegate
		{
			get
			{
				return this._ContinueDelegate;
			}
			set
			{
				this._ContinueDelegate = value;
			}
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x00037D58 File Offset: 0x00035F58
		internal void CallContinueDelegateCallback(object state)
		{
			CoreResponseData coreResponseData = (CoreResponseData)state;
			this.ContinueDelegate((int)coreResponseData.m_StatusCode, coreResponseData.m_ResponseHeaders);
		}

		/// <summary>Gets the service point to use for the request.</summary>
		/// <returns>A <see cref="T:System.Net.ServicePoint" /> that represents the network connection to the Internet resource.</returns>
		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000A31 RID: 2609 RVA: 0x00037D83 File Offset: 0x00035F83
		public ServicePoint ServicePoint
		{
			get
			{
				return this.FindServicePoint(false);
			}
		}

		/// <summary>Get or set the Host header value to use in an HTTP request independent from the request URI.</summary>
		/// <returns>The Host header value in the HTTP request.</returns>
		/// <exception cref="T:System.ArgumentNullException">The Host header cannot be set to <see langword="null" />. </exception>
		/// <exception cref="T:System.ArgumentException">The Host header cannot be set to an invalid value. </exception>
		/// <exception cref="T:System.InvalidOperationException">The Host header cannot be set after the <see cref="T:System.Net.HttpWebRequest" /> has already started to be sent. </exception>
		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000A32 RID: 2610 RVA: 0x00037D8C File Offset: 0x00035F8C
		// (set) Token: 0x06000A33 RID: 2611 RVA: 0x00037DEC File Offset: 0x00035FEC
		public string Host
		{
			get
			{
				if (this.UseCustomHost)
				{
					return HttpWebRequest.GetHostAndPortString(this._HostUri.Host, this._HostUri.Port, this._HostHasPort);
				}
				return HttpWebRequest.GetHostAndPortString(this._Uri.Host, this._Uri.Port, !this._Uri.IsDefaultPort);
			}
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_writestarted"));
				}
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				Uri uri;
				if (value.IndexOf('/') != -1 || !this.TryGetHostUri(value, out uri))
				{
					throw new ArgumentException(global::System.SR.GetString("net_invalid_host"));
				}
				this.CheckConnectPermission(uri, false);
				this._HostUri = uri;
				if (!this._HostUri.IsDefaultPort)
				{
					this._HostHasPort = true;
					return;
				}
				if (value.IndexOf(':') == -1)
				{
					this._HostHasPort = false;
					return;
				}
				int num = value.IndexOf(']');
				if (num == -1)
				{
					this._HostHasPort = true;
					return;
				}
				this._HostHasPort = value.LastIndexOf(':') > num;
			}
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000A34 RID: 2612 RVA: 0x00037E9D File Offset: 0x0003609D
		internal bool UseCustomHost
		{
			get
			{
				return this._HostUri != null && !this._RedirectedToDifferentHost;
			}
		}

		/// <summary>Gets or sets the maximum number of redirects that the request follows.</summary>
		/// <returns>The maximum number of redirection responses that the request follows. The default value is 50.</returns>
		/// <exception cref="T:System.ArgumentException">The value is set to 0 or less. </exception>
		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000A35 RID: 2613 RVA: 0x00037EB8 File Offset: 0x000360B8
		// (set) Token: 0x06000A36 RID: 2614 RVA: 0x00037EC0 File Offset: 0x000360C0
		public int MaximumAutomaticRedirections
		{
			get
			{
				return this._MaximumAllowedRedirections;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException(global::System.SR.GetString("net_toosmall"), "value");
				}
				this._MaximumAllowedRedirections = value;
			}
		}

		/// <summary>Gets or sets the method for the request.</summary>
		/// <returns>The request method to use to contact the Internet resource. The default value is GET.</returns>
		/// <exception cref="T:System.ArgumentException">No method is supplied.-or- The method string contains invalid characters. </exception>
		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06000A37 RID: 2615 RVA: 0x00037EE2 File Offset: 0x000360E2
		// (set) Token: 0x06000A38 RID: 2616 RVA: 0x00037EF0 File Offset: 0x000360F0
		[global::__DynamicallyInvokable]
		public override string Method
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._OriginVerb.Name;
			}
			[global::__DynamicallyInvokable]
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					throw new ArgumentException(global::System.SR.GetString("net_badmethod"), "value");
				}
				if (ValidationHelper.IsInvalidHttpString(value))
				{
					throw new ArgumentException(global::System.SR.GetString("net_badmethod"), "value");
				}
				this._OriginVerb = KnownHttpVerb.Parse(value);
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000A39 RID: 2617 RVA: 0x00037F43 File Offset: 0x00036143
		// (set) Token: 0x06000A3A RID: 2618 RVA: 0x00037F5A File Offset: 0x0003615A
		internal KnownHttpVerb CurrentMethod
		{
			get
			{
				if (this._Verb == null)
				{
					return this._OriginVerb;
				}
				return this._Verb;
			}
			set
			{
				this._Verb = value;
			}
		}

		/// <summary>Gets or sets authentication information for the request.</summary>
		/// <returns>An <see cref="T:System.Net.ICredentials" /> that contains the authentication credentials associated with the request. The default is <see langword="null" />.</returns>
		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x00037F63 File Offset: 0x00036163
		// (set) Token: 0x06000A3C RID: 2620 RVA: 0x00037F6B File Offset: 0x0003616B
		[global::__DynamicallyInvokable]
		public override ICredentials Credentials
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._AuthInfo;
			}
			[global::__DynamicallyInvokable]
			set
			{
				this._AuthInfo = value;
			}
		}

		/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that controls whether default credentials are sent with requests.</summary>
		/// <returns>
		///     <see langword="true" /> if the default credentials are used; otherwise <see langword="false" />. The default value is <see langword="false" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">You attempted to set this property after the request was sent.</exception>
		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x00037F74 File Offset: 0x00036174
		// (set) Token: 0x06000A3E RID: 2622 RVA: 0x00037F86 File Offset: 0x00036186
		[global::__DynamicallyInvokable]
		public override bool UseDefaultCredentials
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this.Credentials is SystemNetworkCredential;
			}
			[global::__DynamicallyInvokable]
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_writestarted"));
				}
				this._AuthInfo = (value ? CredentialCache.DefaultCredentials : null);
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x00037FB1 File Offset: 0x000361B1
		// (set) Token: 0x06000A40 RID: 2624 RVA: 0x00037FC2 File Offset: 0x000361C2
		internal bool IsTunnelRequest
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.IsTunnelRequest) > (HttpWebRequest.Booleans)0U;
			}
			set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.IsTunnelRequest;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.IsTunnelRequest;
			}
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x00037FEC File Offset: 0x000361EC
		// (set) Token: 0x06000A42 RID: 2626 RVA: 0x00037FFD File Offset: 0x000361FD
		internal bool IsWebSocketRequest
		{
			get
			{
				return (this._Booleans & HttpWebRequest.Booleans.IsWebSocketRequest) > (HttpWebRequest.Booleans)0U;
			}
			private set
			{
				if (value)
				{
					this._Booleans |= HttpWebRequest.Booleans.IsWebSocketRequest;
					return;
				}
				this._Booleans &= ~HttpWebRequest.Booleans.IsWebSocketRequest;
			}
		}

		/// <summary>Gets or sets the name of the connection group for the request.</summary>
		/// <returns>The name of the connection group for this request. The default value is <see langword="null" />.</returns>
		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x00038027 File Offset: 0x00036227
		// (set) Token: 0x06000A44 RID: 2628 RVA: 0x0003802F File Offset: 0x0003622F
		public override string ConnectionGroupName
		{
			get
			{
				return this._ConnectionGroupName;
			}
			set
			{
				if (!this.IsWebSocketRequest)
				{
					this._ConnectionGroupName = value;
				}
			}
		}

		// Token: 0x17000257 RID: 599
		// (set) Token: 0x06000A45 RID: 2629 RVA: 0x00038040 File Offset: 0x00036240
		internal bool InternalConnectionGroup
		{
			set
			{
				this.m_InternalConnectionGroup = value;
			}
		}

		/// <summary>Specifies a collection of the name/value pairs that make up the HTTP headers.</summary>
		/// <returns>A <see cref="T:System.Net.WebHeaderCollection" /> that contains the name/value pairs that make up the headers for the HTTP request.</returns>
		/// <exception cref="T:System.InvalidOperationException">The request has been started by calling the <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> method. </exception>
		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000A46 RID: 2630 RVA: 0x00038049 File Offset: 0x00036249
		// (set) Token: 0x06000A47 RID: 2631 RVA: 0x00038054 File Offset: 0x00036254
		[global::__DynamicallyInvokable]
		public override WebHeaderCollection Headers
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._HttpRequestHeaders;
			}
			[global::__DynamicallyInvokable]
			set
			{
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
				}
				WebHeaderCollection webHeaderCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
				foreach (string text in value.AllKeys)
				{
					webHeaderCollection.Add(text, value[text]);
				}
				this._HttpRequestHeaders = webHeaderCollection;
			}
		}

		/// <summary>Gets or sets proxy information for the request.</summary>
		/// <returns>The <see cref="T:System.Net.IWebProxy" /> object to use to proxy the request. The default value is set by calling the <see cref="P:System.Net.GlobalProxySelection.Select" /> property.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///         <see cref="P:System.Net.HttpWebRequest.Proxy" /> is set to <see langword="null" />. </exception>
		/// <exception cref="T:System.InvalidOperationException">The request has been started by calling <see cref="M:System.Net.HttpWebRequest.GetRequestStream" />, <see cref="M:System.Net.HttpWebRequest.BeginGetRequestStream(System.AsyncCallback,System.Object)" />, <see cref="M:System.Net.HttpWebRequest.GetResponse" />, or <see cref="M:System.Net.HttpWebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" />. </exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have permission for the requested operation. </exception>
		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000A48 RID: 2632 RVA: 0x000380B3 File Offset: 0x000362B3
		// (set) Token: 0x06000A49 RID: 2633 RVA: 0x000380C5 File Offset: 0x000362C5
		public override IWebProxy Proxy
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return this._Proxy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (this.RequestSubmitted)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_reqsubmitted"));
				}
				this.InternalProxy = value;
			}
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000A4A RID: 2634 RVA: 0x000380F0 File Offset: 0x000362F0
		// (set) Token: 0x06000A4B RID: 2635 RVA: 0x000380F8 File Offset: 0x000362F8
		internal IWebProxy InternalProxy
		{
			get
			{
				return this._Proxy;
			}
			set
			{
				this.ProxySet = true;
				this._Proxy = value;
				if (this._ProxyChain != null)
				{
					this._ProxyChain.Dispose();
				}
				this._ProxyChain = null;
				ServicePoint servicePoint = this.FindServicePoint(true);
			}
		}

		/// <summary>Gets or sets the version of HTTP to use for the request.</summary>
		/// <returns>The HTTP version to use for the request. The default is <see cref="F:System.Net.HttpVersion.Version11" />.</returns>
		/// <exception cref="T:System.ArgumentException">The HTTP version is set to a value other than 1.0 or 1.1. </exception>
		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000A4C RID: 2636 RVA: 0x00038135 File Offset: 0x00036335
		// (set) Token: 0x06000A4D RID: 2637 RVA: 0x0003814A File Offset: 0x0003634A
		public Version ProtocolVersion
		{
			get
			{
				if (!this.IsVersionHttp10)
				{
					return HttpVersion.Version11;
				}
				return HttpVersion.Version10;
			}
			set
			{
				if (value.Equals(HttpVersion.Version11))
				{
					this.IsVersionHttp10 = false;
					return;
				}
				if (value.Equals(HttpVersion.Version10))
				{
					this.IsVersionHttp10 = true;
					return;
				}
				throw new ArgumentException(global::System.SR.GetString("net_wrongversion"), "value");
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Content-type" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="Content-type" /> HTTP header. The default value is <see langword="null" />.</returns>
		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000A4E RID: 2638 RVA: 0x0003818A File Offset: 0x0003638A
		// (set) Token: 0x06000A4F RID: 2639 RVA: 0x0003819C File Offset: 0x0003639C
		[global::__DynamicallyInvokable]
		public override string ContentType
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._HttpRequestHeaders["Content-Type"];
			}
			[global::__DynamicallyInvokable]
			set
			{
				this.SetSpecialHeaders("Content-Type", value);
			}
		}

		/// <summary>Gets or sets the media type of the request.</summary>
		/// <returns>The media type of the request. The default value is <see langword="null" />.</returns>
		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000A50 RID: 2640 RVA: 0x000381AA File Offset: 0x000363AA
		// (set) Token: 0x06000A51 RID: 2641 RVA: 0x000381B2 File Offset: 0x000363B2
		public string MediaType
		{
			get
			{
				return this._MediaType;
			}
			set
			{
				this._MediaType = value;
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Transfer-encoding" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="Transfer-encoding" /> HTTP header. The default value is <see langword="null" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set when <see cref="P:System.Net.HttpWebRequest.SendChunked" /> is <see langword="false" />. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <see cref="P:System.Net.HttpWebRequest.TransferEncoding" /> is set to the value "Chunked". </exception>
		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000A52 RID: 2642 RVA: 0x000381BB File Offset: 0x000363BB
		// (set) Token: 0x06000A53 RID: 2643 RVA: 0x000381D0 File Offset: 0x000363D0
		public string TransferEncoding
		{
			get
			{
				return this._HttpRequestHeaders["Transfer-Encoding"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				bool flag = text.IndexOf("chunked") != -1;
				if (flag)
				{
					throw new ArgumentException(global::System.SR.GetString("net_nochunked"), "value");
				}
				if (!this.SendChunked)
				{
					throw new InvalidOperationException(global::System.SR.GetString("net_needchunked"));
				}
				this._HttpRequestHeaders.CheckUpdate("Transfer-Encoding", value);
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Connection" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="Connection" /> HTTP header. The default value is <see langword="null" />.</returns>
		/// <exception cref="T:System.ArgumentException">The value of <see cref="P:System.Net.HttpWebRequest.Connection" /> is set to Keep-alive or Close. </exception>
		// Token: 0x1700025F RID: 607
		// (get) Token: 0x06000A54 RID: 2644 RVA: 0x00038255 File Offset: 0x00036455
		// (set) Token: 0x06000A55 RID: 2645 RVA: 0x00038268 File Offset: 0x00036468
		public string Connection
		{
			get
			{
				return this._HttpRequestHeaders["Connection"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					this._HttpRequestHeaders.RemoveInternal("Connection");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				bool flag = text.IndexOf("keep-alive") != -1;
				bool flag2 = text.IndexOf("close") != -1;
				if (flag || flag2)
				{
					throw new ArgumentException(global::System.SR.GetString("net_connarg"), "value");
				}
				this._HttpRequestHeaders.CheckUpdate("Connection", value);
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Accept" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="Accept" /> HTTP header. The default value is <see langword="null" />.</returns>
		// Token: 0x17000260 RID: 608
		// (get) Token: 0x06000A56 RID: 2646 RVA: 0x000382E9 File Offset: 0x000364E9
		// (set) Token: 0x06000A57 RID: 2647 RVA: 0x000382FB File Offset: 0x000364FB
		[global::__DynamicallyInvokable]
		public string Accept
		{
			[global::__DynamicallyInvokable]
			get
			{
				return this._HttpRequestHeaders["Accept"];
			}
			[global::__DynamicallyInvokable]
			set
			{
				this.SetSpecialHeaders("Accept", value);
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Referer" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="Referer" /> HTTP header. The default value is <see langword="null" />.</returns>
		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000A58 RID: 2648 RVA: 0x00038309 File Offset: 0x00036509
		// (set) Token: 0x06000A59 RID: 2649 RVA: 0x0003831B File Offset: 0x0003651B
		public string Referer
		{
			get
			{
				return this._HttpRequestHeaders["Referer"];
			}
			set
			{
				this.SetSpecialHeaders("Referer", value);
			}
		}

		/// <summary>Gets or sets the value of the <see langword="User-agent" /> HTTP header.</summary>
		/// <returns>The value of the <see langword="User-agent" /> HTTP header. The default value is <see langword="null" />.The value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000A5A RID: 2650 RVA: 0x00038329 File Offset: 0x00036529
		// (set) Token: 0x06000A5B RID: 2651 RVA: 0x0003833B File Offset: 0x0003653B
		public string UserAgent
		{
			get
			{
				return this._HttpRequestHeaders["User-Agent"];
			}
			set
			{
				this.SetSpecialHeaders("User-Agent", value);
			}
		}

		/// <summary>Gets or sets the value of the <see langword="Expect" /> HTTP header.</summary>
		/// <returns>The contents of the <see langword="Expect" /> HTTP header. The default value is <see langword="null" />.The value for this property is stored in <see cref="T:System.Net.WebHeaderCollection" />. If WebHeaderCollection is set, the property value is lost.</returns>
		/// <exception cref="T:System.ArgumentException">
		///         <see langword="Expect" /> is set to a string that contains "100-continue" as a substring. </exception>
		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000A5C RID: 2652 RVA: 0x00038349 File Offset: 0x00036549
		// (set) Token: 0x06000A5D RID: 2653 RVA: 0x0003835C File Offset: 0x0003655C
		public string Expect
		{
			get
			{
				return this._HttpRequestHeaders["Expect"];
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					this._HttpRequestHeaders.RemoveInternal("Expect");
					return;
				}
				string text = value.ToLower(CultureInfo.InvariantCulture);
				bool flag = text.IndexOf("100-continue") != -1;
				if (flag)
				{
					throw new ArgumentException(global::System.SR.GetString("net_no100"), "value");
				}
				this._HttpRequestHeaders.CheckUpdate("Expect", value);
			}
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x000383CC File Offset: 0x000365CC
		private DateTime GetDateHeaderHelper(string headerName)
		{
			string text = this._HttpRequestHeaders[headerName];
			if (text == null)
			{
				return DateTime.MinValue;
			}
			return HttpProtocolUtils.string2date(text);
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x000383F5 File Offset: 0x000365F5
		private void SetDateHeaderHelper(string headerName, DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				this.SetSpecialHeaders(headerName, null);
				return;
			}
			this.SetSpecialHeaders(headerName, HttpProtocolUtils.date2string(dateTime));
		}

		/// <summary>Gets or sets the value of the <see langword="If-Modified-Since" /> HTTP header.</summary>
		/// <returns>A <see cref="T:System.DateTime" /> that contains the contents of the <see langword="If-Modified-Since" /> HTTP header. The default value is the current date and time.</returns>
		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000A60 RID: 2656 RVA: 0x0003841A File Offset: 0x0003661A
		// (set) Token: 0x06000A61 RID: 2657 RVA: 0x00038427 File Offset: 0x00036627
		public DateTime IfModifiedSince
		{
			get
			{
				return this.GetDateHeaderHelper("If-Modified-Since");
			}
			set
			{
				this.SetDateHeaderHelper("If-Modified-Since", value);
			}
		}

		/// <summary>Get or set the <see langword="Date" /> HTTP header value to use in an HTTP request.</summary>
		/// <returns>The Date header value in the HTTP request.</returns>
		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x00038435 File Offset: 0x00036635
		// (set) Token: 0x06000A63 RID: 2659 RVA: 0x00038442 File Offset: 0x00036642
		public DateTime Date
		{
			get
			{
				return this.GetDateHeaderHelper("Date");
			}
			set
			{
				this.SetDateHeaderHelper("Date", value);
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000A64 RID: 2660 RVA: 0x00038450 File Offset: 0x00036650
		internal byte[] WriteBuffer
		{
			get
			{
				return this._WriteBuffer;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000A65 RID: 2661 RVA: 0x00038458 File Offset: 0x00036658
		internal int WriteBufferLength
		{
			get
			{
				return this._WriteBufferLength;
			}
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x00038460 File Offset: 0x00036660
		internal void FreeWriteBuffer()
		{
			if (this._WriteBufferFromPinnableCache)
			{
				HttpWebRequest._WriteBufferCache.FreeBuffer(this._WriteBuffer);
				this._WriteBufferFromPinnableCache = false;
			}
			this._WriteBufferLength = 0;
			this._WriteBuffer = null;
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x0003848F File Offset: 0x0003668F
		private void SetWriteBuffer(int bufferSize)
		{
			if (ServicePointManager.UseHttpPipeliningAndBufferPooling && bufferSize <= 512)
			{
				this._WriteBuffer = HttpWebRequest._WriteBufferCache.AllocateBuffer();
				this._WriteBufferFromPinnableCache = true;
			}
			else
			{
				this._WriteBuffer = new byte[bufferSize];
			}
			this._WriteBufferLength = bufferSize;
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x000384CC File Offset: 0x000366CC
		private void SetSpecialHeaders(string HeaderName, string value)
		{
			value = WebHeaderCollection.CheckBadChars(value, true);
			this._HttpRequestHeaders.RemoveInternal(HeaderName);
			if (value.Length != 0)
			{
				this._HttpRequestHeaders.AddInternal(HeaderName, value);
			}
		}

		/// <summary>Cancels a request to an Internet resource.</summary>
		// Token: 0x06000A69 RID: 2665 RVA: 0x000384F8 File Offset: 0x000366F8
		[global::__DynamicallyInvokable]
		public override void Abort()
		{
			this.Abort(null, 1);
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x00038504 File Offset: 0x00036704
		private void Abort(Exception exception, int abortState)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "Abort", (exception == null) ? "" : exception.Message);
			}
			if (Interlocked.CompareExchange(ref this.m_Aborted, abortState, 0) == 0)
			{
				NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestAborted);
				this.m_OnceFailed = true;
				this.CancelTimer();
				WebException ex = exception as WebException;
				if (exception == null)
				{
					ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
				}
				else if (ex == null)
				{
					ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), exception, WebExceptionStatus.RequestCanceled, this._HttpResponse);
				}
				try
				{
					Thread.MemoryBarrier();
					HttpAbortDelegate abortDelegate = this._AbortDelegate;
					if (abortDelegate == null || abortDelegate(this, ex))
					{
						this.SetResponse(ex);
					}
					else
					{
						LazyAsyncResult lazyAsyncResult = null;
						LazyAsyncResult lazyAsyncResult2 = null;
						if (!this.Async)
						{
							lock (this)
							{
								lazyAsyncResult = this._WriteAResult;
								lazyAsyncResult2 = this._ReadAResult;
							}
						}
						if (lazyAsyncResult != null)
						{
							lazyAsyncResult.InvokeCallback(ex);
						}
						if (lazyAsyncResult2 != null)
						{
							lazyAsyncResult2.InvokeCallback(ex);
						}
					}
					if (!this.Async)
					{
						LazyAsyncResult connectionAsyncResult = this.ConnectionAsyncResult;
						LazyAsyncResult connectionReaderAsyncResult = this.ConnectionReaderAsyncResult;
						if (connectionAsyncResult != null)
						{
							connectionAsyncResult.InvokeCallback(ex);
						}
						if (connectionReaderAsyncResult != null)
						{
							connectionReaderAsyncResult.InvokeCallback(ex);
						}
					}
					if (this.IsWebSocketRequest && this.ServicePoint != null)
					{
						this.ServicePoint.CloseConnectionGroup(this.ConnectionGroupName);
					}
				}
				catch (InternalException)
				{
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "Abort", "");
			}
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x0003869C File Offset: 0x0003689C
		private void CancelTimer()
		{
			TimerThread.Timer timer = this._Timer;
			if (timer != null)
			{
				timer.Cancel();
			}
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x000386BA File Offset: 0x000368BA
		private static void TimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
		{
			ThreadPool.UnsafeQueueUserWorkItem(HttpWebRequest.s_AbortWrapper, context);
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x000386C8 File Offset: 0x000368C8
		private static void AbortWrapper(object context)
		{
			((HttpWebRequest)context).Abort(new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout), 1);
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x000386E4 File Offset: 0x000368E4
		private ServicePoint FindServicePoint(bool forceFind)
		{
			ServicePoint servicePoint = this._ServicePoint;
			if (servicePoint == null || forceFind)
			{
				lock (this)
				{
					if (this._ServicePoint == null || forceFind)
					{
						if (!this.ProxySet)
						{
							this._Proxy = WebRequest.InternalDefaultWebProxy;
						}
						if (this._ProxyChain != null)
						{
							this._ProxyChain.Dispose();
						}
						this._ServicePoint = ServicePointManager.FindServicePoint(this._Uri, this._Proxy, out this._ProxyChain, ref this._AbortDelegate, ref this.m_Aborted);
						if (Logging.On)
						{
							Logging.Associate(Logging.Web, this, this._ServicePoint);
						}
					}
				}
				servicePoint = this._ServicePoint;
			}
			return servicePoint;
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x000387A8 File Offset: 0x000369A8
		private void InvokeGetRequestStreamCallback()
		{
			LazyAsyncResult writeAResult = this._WriteAResult;
			if (writeAResult != null)
			{
				try
				{
					writeAResult.InvokeCallback(this._SubmitWriteStream);
				}
				catch (Exception ex)
				{
					if (NclUtilities.IsFatal(ex))
					{
						throw;
					}
					this.Abort(ex, 1);
					throw;
				}
			}
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x000387F4 File Offset: 0x000369F4
		internal void SetRequestSubmitDone(ConnectStream submitStream)
		{
			if (!this.Async)
			{
				this.ConnectionAsyncResult.InvokeCallback();
			}
			if (this.AllowWriteStreamBuffering)
			{
				submitStream.EnableWriteBuffering();
			}
			if (submitStream.CanTimeout)
			{
				submitStream.ReadTimeout = this.ReadWriteTimeout;
				submitStream.WriteTimeout = this.ReadWriteTimeout;
			}
			if (Logging.On)
			{
				Logging.Associate(Logging.Web, this, submitStream);
			}
			TransportContext transportContext = new ConnectStreamContext(submitStream);
			this.ServerAuthenticationState.TransportContext = transportContext;
			this.ProxyAuthenticationState.TransportContext = transportContext;
			this._SubmitWriteStream = submitStream;
			if (this.RtcState != null && this.RtcState.inputData != null && !this.RtcState.IsAborted)
			{
				this.RtcState.outputData = new byte[4];
				this.RtcState.result = this._SubmitWriteStream.SetRtcOption(this.RtcState.inputData, this.RtcState.outputData);
				if (!this.RtcState.IsEnabled())
				{
					this.Abort(null, 1);
				}
				this.RtcState.connectComplete.Set();
			}
			if (this.Async && this._CoreResponse != null && this._CoreResponse != DBNull.Value)
			{
				submitStream.CallDone();
				return;
			}
			this.EndSubmitRequest();
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x0003892C File Offset: 0x00036B2C
		internal void WriteHeadersCallback(WebExceptionStatus errorStatus, ConnectStream stream, bool async)
		{
			if (errorStatus == WebExceptionStatus.Success)
			{
				if (!this.EndWriteHeaders(async))
				{
					errorStatus = WebExceptionStatus.Pending;
					return;
				}
				if (stream.BytesLeftToWrite == 0L)
				{
					stream.CallDone();
				}
			}
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x0003895A File Offset: 0x00036B5A
		internal void SetRequestContinue()
		{
			this.SetRequestContinue(null);
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00038964 File Offset: 0x00036B64
		internal void SetRequestContinue(CoreResponseData continueResponse)
		{
			this._RequestContinueCount++;
			if (this.HttpWriteMode == HttpWriteMode.None)
			{
				return;
			}
			if (this.m_ContinueGate.Complete())
			{
				if (continueResponse != null && this.ContinueDelegate != null)
				{
					ExecutionContext executionContext = (this.Async ? this.GetWritingContext().ContextCopy : null);
					if (executionContext == null)
					{
						this.ContinueDelegate((int)continueResponse.m_StatusCode, continueResponse.m_ResponseHeaders);
					}
					else
					{
						ExecutionContext.Run(executionContext, new ContextCallback(this.CallContinueDelegateCallback), continueResponse);
					}
				}
				this.EndWriteHeaders_Part2();
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x000389ED File Offset: 0x00036BED
		internal int RequestContinueCount
		{
			get
			{
				return this._RequestContinueCount;
			}
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x000389F5 File Offset: 0x00036BF5
		internal void OpenWriteSideResponseWindow()
		{
			this._CoreResponse = DBNull.Value;
			this._NestedWriteSideCheck = 0;
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x00038A0C File Offset: 0x00036C0C
		internal void CheckWriteSideResponseProcessing()
		{
			object obj = (this.Async ? Interlocked.CompareExchange(ref this._CoreResponse, null, DBNull.Value) : this._CoreResponse);
			if (obj == DBNull.Value)
			{
				return;
			}
			if (obj == null)
			{
				return;
			}
			if (!this.Async)
			{
				int num = this._NestedWriteSideCheck + 1;
				this._NestedWriteSideCheck = num;
				if (num != 1)
				{
					return;
				}
			}
			this.FinishContinueWait();
			Exception ex = obj as Exception;
			if (ex != null)
			{
				this.SetResponse(ex);
				return;
			}
			this.SetResponse(obj as CoreResponseData);
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x00038A8C File Offset: 0x00036C8C
		internal void SetAndOrProcessResponse(object responseOrException)
		{
			if (responseOrException == null)
			{
				throw new InternalException();
			}
			CoreResponseData coreResponseData = responseOrException as CoreResponseData;
			WebException ex = responseOrException as WebException;
			object obj = Interlocked.CompareExchange(ref this._CoreResponse, responseOrException, DBNull.Value);
			if (obj != null)
			{
				if (obj.GetType() == typeof(CoreResponseData))
				{
					if (coreResponseData != null)
					{
						throw new InternalException();
					}
					if (ex != null && ex.InternalStatus != WebExceptionInternalStatus.ServicePointFatal && ex.InternalStatus != WebExceptionInternalStatus.RequestFatal)
					{
						return;
					}
				}
				else if (obj.GetType() != typeof(DBNull))
				{
					if (coreResponseData == null)
					{
						throw new InternalException();
					}
					ICloseEx closeEx = coreResponseData.m_ConnectStream as ICloseEx;
					if (closeEx != null)
					{
						closeEx.CloseEx(CloseExState.Silent);
						return;
					}
					coreResponseData.m_ConnectStream.Close();
					return;
				}
			}
			if (obj == DBNull.Value)
			{
				if (!this.Async)
				{
					LazyAsyncResult connectionAsyncResult = this.ConnectionAsyncResult;
					LazyAsyncResult connectionReaderAsyncResult = this.ConnectionReaderAsyncResult;
					connectionAsyncResult.InvokeCallback(responseOrException);
					connectionReaderAsyncResult.InvokeCallback(responseOrException);
					return;
				}
				if (!this.AllowWriteStreamBuffering && this.IsOutstandingGetRequestStream && this.FinishContinueWait())
				{
					if (coreResponseData != null)
					{
						this.SetResponse(coreResponseData);
						return;
					}
					this.SetResponse(ex);
				}
				return;
			}
			else if (obj != null)
			{
				Exception ex2 = responseOrException as Exception;
				if (ex2 != null)
				{
					this.SetResponse(ex2);
					return;
				}
				throw new InternalException();
			}
			else
			{
				obj = Interlocked.CompareExchange(ref this._CoreResponse, responseOrException, null);
				if (obj != null && coreResponseData != null)
				{
					ICloseEx closeEx2 = coreResponseData.m_ConnectStream as ICloseEx;
					if (closeEx2 != null)
					{
						closeEx2.CloseEx(CloseExState.Silent);
						return;
					}
					coreResponseData.m_ConnectStream.Close();
					return;
				}
				else
				{
					if (!this.Async)
					{
						throw new InternalException();
					}
					this.FinishContinueWait();
					if (coreResponseData != null)
					{
						this.SetResponse(coreResponseData);
						return;
					}
					this.SetResponse(responseOrException as Exception);
					return;
				}
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00038C20 File Offset: 0x00036E20
		private void SetResponse(CoreResponseData coreResponseData)
		{
			try
			{
				if (!this.Async)
				{
					LazyAsyncResult connectionAsyncResult = this.ConnectionAsyncResult;
					LazyAsyncResult connectionReaderAsyncResult = this.ConnectionReaderAsyncResult;
					connectionAsyncResult.InvokeCallback(coreResponseData);
					connectionReaderAsyncResult.InvokeCallback(coreResponseData);
				}
				if (coreResponseData != null)
				{
					if (coreResponseData.m_ConnectStream.CanTimeout)
					{
						coreResponseData.m_ConnectStream.WriteTimeout = this.ReadWriteTimeout;
						coreResponseData.m_ConnectStream.ReadTimeout = this.ReadWriteTimeout;
					}
					this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreResponseData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression, this.IsWebSocketRequest, this.ConnectionGroupName);
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, coreResponseData.m_ConnectStream);
					}
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, this._HttpResponse);
					}
					this.ProcessResponse();
				}
				else
				{
					this.Abort(null, 1);
				}
			}
			catch (Exception ex)
			{
				this.Abort(ex, 2);
			}
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00038D1C File Offset: 0x00036F1C
		private void ProcessResponse()
		{
			Exception ex = null;
			if (this.DoSubmitRequestProcessing(ref ex) == HttpProcessingResult.Continue)
			{
				this.CancelTimer();
				object obj = ((ex != null) ? ex : this._HttpResponse);
				if (this._ReadAResult == null)
				{
					lock (this)
					{
						if (this._ReadAResult == null)
						{
							this._ReadAResult = new LazyAsyncResult(null, null, null);
						}
					}
				}
				try
				{
					this.FinishRequest(this._HttpResponse, ex);
					this._ReadAResult.InvokeCallback(obj);
					try
					{
						this.SetRequestContinue();
					}
					catch
					{
					}
				}
				catch (Exception ex2)
				{
					this.Abort(ex2, 1);
					throw;
				}
				finally
				{
					if (ex == null && this._ReadAResult.Result != this._HttpResponse)
					{
						WebException ex3 = this._ReadAResult.Result as WebException;
						if (ex3 != null && ex3.Response != null)
						{
							this._HttpResponse.Abort();
						}
					}
				}
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00038E30 File Offset: 0x00037030
		private void SetResponse(Exception E)
		{
			HttpProcessingResult httpProcessingResult = HttpProcessingResult.Continue;
			WebException ex = (this.HaveResponse ? (this._ReadAResult.Result as WebException) : null);
			WebException ex2 = E as WebException;
			if (ex != null && (ex.InternalStatus == WebExceptionInternalStatus.RequestFatal || ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal) && (ex2 == null || ex2.InternalStatus != WebExceptionInternalStatus.RequestFatal))
			{
				E = ex;
			}
			else
			{
				ex = ex2;
			}
			if (E != null && Logging.On)
			{
				Logging.Exception(Logging.Web, this, "", ex);
			}
			try
			{
				if (ex != null && (ex.InternalStatus == WebExceptionInternalStatus.Isolated || ex.InternalStatus == WebExceptionInternalStatus.ServicePointFatal || (ex.InternalStatus == WebExceptionInternalStatus.Recoverable && !this.m_OnceFailed)))
				{
					if (ex.InternalStatus == WebExceptionInternalStatus.Recoverable)
					{
						this.m_OnceFailed = true;
					}
					this.Pipelined = false;
					if (this._SubmitWriteStream != null && this._OldSubmitWriteStream == null && this._SubmitWriteStream.BufferOnly)
					{
						this._OldSubmitWriteStream = this._SubmitWriteStream;
					}
					httpProcessingResult = this.DoSubmitRequestProcessing(ref E);
				}
			}
			catch (Exception ex3)
			{
				if (NclUtilities.IsFatal(ex3))
				{
					throw;
				}
				httpProcessingResult = HttpProcessingResult.Continue;
				E = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), ex3, WebExceptionStatus.RequestCanceled, this._HttpResponse);
			}
			finally
			{
				if (httpProcessingResult == HttpProcessingResult.Continue)
				{
					this.CancelTimer();
					if (!(E is WebException) && !(E is SecurityException))
					{
						if (this._HttpResponse == null)
						{
							E = new WebException(E.Message, E);
						}
						else
						{
							E = new WebException(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), E, WebExceptionStatus.ProtocolError, this._HttpResponse);
						}
					}
					LazyAsyncResult lazyAsyncResult = null;
					HttpWebResponse httpWebResponse = this._HttpResponse;
					LazyAsyncResult writeAResult;
					lock (this)
					{
						writeAResult = this._WriteAResult;
						if (this._ReadAResult == null)
						{
							this._ReadAResult = new LazyAsyncResult(null, null, null, E);
						}
						else
						{
							lazyAsyncResult = this._ReadAResult;
						}
					}
					try
					{
						this.FinishRequest(httpWebResponse, E);
						try
						{
							if (writeAResult != null)
							{
								writeAResult.InvokeCallback(E);
							}
						}
						finally
						{
							if (lazyAsyncResult != null)
							{
								lazyAsyncResult.InvokeCallback(E);
							}
						}
					}
					finally
					{
						httpWebResponse = this._ReadAResult.Result as HttpWebResponse;
						if (httpWebResponse != null)
						{
							httpWebResponse.Abort();
						}
						if (base.CacheProtocol != null)
						{
							base.CacheProtocol.Abort();
						}
					}
				}
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000A7B RID: 2683 RVA: 0x000390D4 File Offset: 0x000372D4
		private bool IdentityRequired
		{
			get
			{
				CredentialCache credentialCache;
				return this.Credentials != null && (this.Credentials is SystemNetworkCredential || (!(this.Credentials is NetworkCredential) && ((credentialCache = this.Credentials as CredentialCache) == null || credentialCache.IsDefaultInCache)));
			}
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00039120 File Offset: 0x00037320
		internal override ContextAwareResult GetConnectingContext()
		{
			if (!this.Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = ((this.HttpWriteMode == HttpWriteMode.None || this._OldSubmitWriteStream != null || this._WriteAResult == null || this._WriteAResult.IsCompleted) ? this._ReadAResult : this._WriteAResult) as ContextAwareResult;
			if (contextAwareResult == null)
			{
				throw new InternalException();
			}
			return contextAwareResult;
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x0003917C File Offset: 0x0003737C
		internal override ContextAwareResult GetWritingContext()
		{
			if (!this.Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = this._WriteAResult as ContextAwareResult;
			if (contextAwareResult == null || contextAwareResult.InternalPeekCompleted || this.HttpWriteMode == HttpWriteMode.None || this.HttpWriteMode == HttpWriteMode.Buffer || this.m_PendingReturnResult == DBNull.Value || this.m_OriginallyBuffered)
			{
				contextAwareResult = this._ReadAResult as ContextAwareResult;
			}
			if (contextAwareResult == null)
			{
				throw new InternalException();
			}
			return contextAwareResult;
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x000391E8 File Offset: 0x000373E8
		internal override ContextAwareResult GetReadingContext()
		{
			if (!this.Async)
			{
				return null;
			}
			ContextAwareResult contextAwareResult = this._ReadAResult as ContextAwareResult;
			if (contextAwareResult == null)
			{
				contextAwareResult = this._WriteAResult as ContextAwareResult;
				if (contextAwareResult == null)
				{
					throw new InternalException();
				}
			}
			return contextAwareResult;
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00039224 File Offset: 0x00037424
		private void BeginSubmitRequest()
		{
			this.SubmitRequest(this.FindServicePoint(false));
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00039234 File Offset: 0x00037434
		private void SubmitRequest(ServicePoint servicePoint)
		{
			if (!this.Async)
			{
				this._ConnectionAResult = new LazyAsyncResult(this, null, null);
				this._ConnectionReaderAResult = new LazyAsyncResult(this, null, null);
				this.OpenWriteSideResponseWindow();
			}
			if (this._Timer == null && !this.Async)
			{
				this._Timer = this.TimerQueue.CreateTimer(HttpWebRequest.s_TimeoutCallback, this);
			}
			try
			{
				if (this._SubmitWriteStream != null && this._SubmitWriteStream.IsPostStream)
				{
					if (this._OldSubmitWriteStream == null && !this._SubmitWriteStream.ErrorInStream && this.AllowWriteStreamBuffering)
					{
						this._OldSubmitWriteStream = this._SubmitWriteStream;
					}
					this._WriteBufferLength = 0;
				}
				this.m_Retry = false;
				if (this.PreAuthenticate)
				{
					if (this.UsesProxySemantics && this._Proxy != null && this._Proxy.Credentials != null)
					{
						this.ProxyAuthenticationState.PreAuthIfNeeded(this, this._Proxy.Credentials);
					}
					if (this.Credentials != null)
					{
						this.ServerAuthenticationState.PreAuthIfNeeded(this, this.Credentials);
					}
				}
				if (this.WriteBufferLength == 0)
				{
					this.UpdateHeaders();
				}
				if (!this.CheckCacheRetrieveBeforeSubmit())
				{
					servicePoint.SubmitRequest(this, this.GetConnectionGroupLine());
				}
			}
			finally
			{
				if (!this.Async)
				{
					this.CheckWriteSideResponseProcessing();
				}
			}
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x0003937C File Offset: 0x0003757C
		private bool CheckCacheRetrieveBeforeSubmit()
		{
			if (base.CacheProtocol == null)
			{
				return false;
			}
			bool flag;
			try
			{
				Uri uri = this.GetRemoteResourceUri();
				if (uri.Fragment.Length != 0)
				{
					uri = new Uri(uri.GetParts(UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path | UriComponents.Query, UriFormat.SafeUnescaped));
				}
				base.CacheProtocol.GetRetrieveStatus(uri, this);
				if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
				{
					throw base.CacheProtocol.ProtocolException;
				}
				if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
				{
					flag = false;
				}
				else
				{
					if (this.HttpWriteMode != HttpWriteMode.None)
					{
						throw new NotSupportedException(global::System.SR.GetString("net_cache_not_supported_body"));
					}
					HttpRequestCacheValidator httpRequestCacheValidator = (HttpRequestCacheValidator)base.CacheProtocol.Validator;
					CoreResponseData coreResponseData = new CoreResponseData();
					coreResponseData.m_IsVersionHttp11 = httpRequestCacheValidator.CacheHttpVersion.Equals(HttpVersion.Version11);
					coreResponseData.m_StatusCode = httpRequestCacheValidator.CacheStatusCode;
					coreResponseData.m_StatusDescription = httpRequestCacheValidator.CacheStatusDescription;
					coreResponseData.m_ResponseHeaders = httpRequestCacheValidator.CacheHeaders;
					coreResponseData.m_ContentLength = base.CacheProtocol.ResponseStreamLength;
					coreResponseData.m_ConnectStream = base.CacheProtocol.ResponseStream;
					this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreResponseData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression, this.IsWebSocketRequest, this.ConnectionGroupName);
					this._HttpResponse.InternalSetFromCache = true;
					this._HttpResponse.InternalSetIsCacheFresh = httpRequestCacheValidator.CacheFreshnessStatus != CacheFreshnessStatus.Stale;
					this.ProcessResponse();
					flag = true;
				}
			}
			catch (Exception ex)
			{
				this.Abort(ex, 1);
				throw;
			}
			return flag;
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00039510 File Offset: 0x00037710
		private bool CheckCacheRetrieveOnResponse()
		{
			if (base.CacheProtocol == null)
			{
				return true;
			}
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
			{
				throw base.CacheProtocol.ProtocolException;
			}
			Stream responseStream = this._HttpResponse.ResponseStream;
			base.CacheProtocol.GetRevalidateStatus(this._HttpResponse, this._HttpResponse.ResponseStream);
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
			{
				return false;
			}
			if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse && base.CacheProtocol.ProtocolStatus != CacheValidationStatus.CombineCachedAndServerResponse)
			{
				return true;
			}
			if (this.HttpWriteMode != HttpWriteMode.None)
			{
				throw new NotSupportedException(global::System.SR.GetString("net_cache_not_supported_body"));
			}
			CoreResponseData coreResponseData = new CoreResponseData();
			HttpRequestCacheValidator httpRequestCacheValidator = (HttpRequestCacheValidator)base.CacheProtocol.Validator;
			coreResponseData.m_IsVersionHttp11 = httpRequestCacheValidator.CacheHttpVersion.Equals(HttpVersion.Version11);
			coreResponseData.m_StatusCode = httpRequestCacheValidator.CacheStatusCode;
			coreResponseData.m_StatusDescription = httpRequestCacheValidator.CacheStatusDescription;
			coreResponseData.m_ResponseHeaders = ((base.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse) ? new WebHeaderCollection(httpRequestCacheValidator.CacheHeaders) : httpRequestCacheValidator.CacheHeaders);
			coreResponseData.m_ContentLength = base.CacheProtocol.ResponseStreamLength;
			coreResponseData.m_ConnectStream = base.CacheProtocol.ResponseStream;
			this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreResponseData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression, this.IsWebSocketRequest, this.ConnectionGroupName);
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
			{
				this._HttpResponse.InternalSetFromCache = true;
				this._HttpResponse.InternalSetIsCacheFresh = base.CacheProtocol.IsCacheFresh;
				if (responseStream != null)
				{
					try
					{
						responseStream.Close();
					}
					catch
					{
					}
				}
			}
			return true;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x000396C4 File Offset: 0x000378C4
		private void CheckCacheUpdateOnResponse()
		{
			if (base.CacheProtocol == null)
			{
				return;
			}
			if (base.CacheProtocol.GetUpdateStatus(this._HttpResponse, this._HttpResponse.ResponseStream) == CacheValidationStatus.UpdateResponseInformation)
			{
				this._HttpResponse.ResponseStream = base.CacheProtocol.ResponseStream;
				return;
			}
			if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
			{
				throw base.CacheProtocol.ProtocolException;
			}
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x0003972C File Offset: 0x0003792C
		private void EndSubmitRequest()
		{
			try
			{
				if (this.HttpWriteMode == HttpWriteMode.Buffer)
				{
					this.InvokeGetRequestStreamCallback();
				}
				else
				{
					if (this.WriteBufferLength == 0)
					{
						long num = this.SwitchToContentLength();
						this.SerializeHeaders();
						this.PostSwitchToContentLength(num);
					}
					this._SubmitWriteStream.WriteHeaders(this.Async);
				}
			}
			catch
			{
				ConnectStream submitWriteStream = this._SubmitWriteStream;
				if (submitWriteStream != null)
				{
					submitWriteStream.CallDone();
				}
				throw;
			}
			finally
			{
				if (!this.Async)
				{
					this.CheckWriteSideResponseProcessing();
				}
			}
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x000397B8 File Offset: 0x000379B8
		internal bool EndWriteHeaders(bool async)
		{
			try
			{
				if (this.ShouldWaitFor100Continue())
				{
					return !async;
				}
				if (this.FinishContinueWait() && this.CompleteContinueGate())
				{
					this.EndWriteHeaders_Part2();
				}
			}
			catch
			{
				ConnectStream submitWriteStream = this._SubmitWriteStream;
				if (submitWriteStream != null)
				{
					submitWriteStream.CallDone();
				}
				throw;
			}
			return true;
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x00039814 File Offset: 0x00037A14
		internal bool ShouldWaitFor100Continue()
		{
			return (this.ContentLength > 0L || this.HttpWriteMode == HttpWriteMode.Chunked) && this.ExpectContinue && this._ServicePoint.Understands100Continue;
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x00039840 File Offset: 0x00037A40
		private static void ContinueTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)context;
			if (httpWebRequest.HttpWriteMode == HttpWriteMode.None)
			{
				return;
			}
			if (!httpWebRequest.FinishContinueWait() || !httpWebRequest.CompleteContinueGate())
			{
				return;
			}
			ThreadPool.UnsafeQueueUserWorkItem(HttpWebRequest.s_EndWriteHeaders_Part2Callback, httpWebRequest);
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x0003987C File Offset: 0x00037A7C
		internal void StartContinueWait()
		{
			bool flag = this.m_ContinueGate.Trigger(true);
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x00039898 File Offset: 0x00037A98
		internal void StartAsync100ContinueTimer()
		{
			if (this.m_ContinueGate.StartTriggering(true))
			{
				try
				{
					if (this.ShouldWaitFor100Continue())
					{
						this.m_ContinueTimer = this.ContinueTimerQueue.CreateTimer(HttpWebRequest.s_ContinueTimeoutCallback, this);
					}
				}
				finally
				{
					this.m_ContinueGate.FinishTriggering();
				}
			}
		}

		// Token: 0x06000A8A RID: 2698 RVA: 0x000398F0 File Offset: 0x00037AF0
		internal bool FinishContinueWait()
		{
			if (this.m_ContinueGate.StartSignaling(false))
			{
				try
				{
					TimerThread.Timer continueTimer = this.m_ContinueTimer;
					this.m_ContinueTimer = null;
					if (continueTimer != null)
					{
						continueTimer.Cancel();
					}
				}
				finally
				{
					this.m_ContinueGate.FinishSignaling();
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x00039944 File Offset: 0x00037B44
		private bool CompleteContinueGate()
		{
			return this.m_ContinueGate.Complete();
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x00039951 File Offset: 0x00037B51
		private static void EndWriteHeaders_Part2Wrapper(object state)
		{
			((HttpWebRequest)state).EndWriteHeaders_Part2();
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x00039960 File Offset: 0x00037B60
		internal void EndWriteHeaders_Part2()
		{
			try
			{
				ConnectStream connectStream = this._SubmitWriteStream;
				if (this.HttpWriteMode != HttpWriteMode.None)
				{
					this.m_BodyStarted = true;
					if (this.AllowWriteStreamBuffering || this._resendRequestContent != null)
					{
						if (connectStream.BufferOnly)
						{
							this._OldSubmitWriteStream = connectStream;
						}
						if (this._OldSubmitWriteStream != null || (this.UserRetrievedWriteStream && this._resendRequestContent != null))
						{
							if (this._resendRequestContent == null)
							{
								connectStream.ResubmitWrite(this._OldSubmitWriteStream, this.NtlmKeepAlive && this.ContentLength == 0L);
							}
							else if (this.NtlmKeepAlive && (this.ContentLength == 0L || this.HttpWriteMode == HttpWriteMode.Chunked))
							{
								if (this.ContentLength == 0L)
								{
									connectStream.BytesLeftToWrite = 0L;
								}
							}
							else
							{
								if (this.HttpWriteMode != HttpWriteMode.Chunked)
								{
									connectStream.BytesLeftToWrite = this._originalContentLength;
								}
								try
								{
									this._resendRequestContent(connectStream);
								}
								catch (Exception ex)
								{
									this.Abort(ex, 1);
								}
							}
							connectStream.CloseInternal(true);
						}
					}
				}
				else
				{
					if (connectStream != null)
					{
						connectStream.CloseInternal(true);
						connectStream = null;
					}
					this._OldSubmitWriteStream = null;
				}
				this.InvokeGetRequestStreamCallback();
			}
			catch
			{
				ConnectStream submitWriteStream = this._SubmitWriteStream;
				if (submitWriteStream != null)
				{
					submitWriteStream.CallDone();
				}
				throw;
			}
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x00039AB8 File Offset: 0x00037CB8
		private int GenerateConnectRequestLine(int headersSize)
		{
			HostHeaderString hostHeaderString = new HostHeaderString(this.GetSafeHostAndPort(true, true));
			int num = this.CurrentMethod.Name.Length + hostHeaderString.ByteCount + 12 + headersSize;
			this.SetWriteBuffer(num);
			int num2 = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
			this.WriteBuffer[num2++] = 32;
			hostHeaderString.Copy(this.WriteBuffer, num2);
			num2 += hostHeaderString.ByteCount;
			this.WriteBuffer[num2++] = 32;
			return num2;
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x00039B5A File Offset: 0x00037D5A
		private string GetSafeHostAndPort(bool addDefaultPort, bool forcePunycode)
		{
			if (this.IsTunnelRequest)
			{
				return HttpWebRequest.GetSafeHostAndPort(this._OriginUri, addDefaultPort, forcePunycode);
			}
			return HttpWebRequest.GetSafeHostAndPort(this._Uri, addDefaultPort, forcePunycode);
		}

		// Token: 0x06000A90 RID: 2704 RVA: 0x00039B80 File Offset: 0x00037D80
		private static string GetSafeHostAndPort(Uri sourceUri, bool addDefaultPort, bool forcePunycode)
		{
			string text;
			if (sourceUri.HostNameType == UriHostNameType.IPv6)
			{
				text = "[" + HttpWebRequest.TrimScopeID(sourceUri.DnsSafeHost) + "]";
			}
			else
			{
				text = (forcePunycode ? sourceUri.IdnHost : sourceUri.DnsSafeHost);
			}
			return HttpWebRequest.GetHostAndPortString(text, sourceUri.Port, addDefaultPort || !sourceUri.IsDefaultPort);
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x00039BE0 File Offset: 0x00037DE0
		private static string GetHostAndPortString(string hostName, int port, bool addPort)
		{
			if (addPort)
			{
				return hostName + ":" + port.ToString();
			}
			return hostName;
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00039BFC File Offset: 0x00037DFC
		private bool TryGetHostUri(string hostName, out Uri hostUri)
		{
			StringBuilder stringBuilder = new StringBuilder(this._Uri.Scheme);
			stringBuilder.Append("://");
			stringBuilder.Append(hostName);
			stringBuilder.Append(this._Uri.PathAndQuery);
			return Uri.TryCreate(stringBuilder.ToString(), UriKind.Absolute, out hostUri);
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x00039C50 File Offset: 0x00037E50
		private static string TrimScopeID(string s)
		{
			int num = s.LastIndexOf('%');
			if (num > 0)
			{
				return s.Substring(0, num);
			}
			return s;
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x00039C74 File Offset: 0x00037E74
		private int GenerateProxyRequestLine(int headersSize)
		{
			if (this._Uri.Scheme == Uri.UriSchemeFtp)
			{
				return this.GenerateFtpProxyRequestLine(headersSize);
			}
			string components = this._Uri.GetComponents(UriComponents.Scheme | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			HostHeaderString hostHeaderString = new HostHeaderString(this.GetSafeHostAndPort(false, true));
			string components2 = this._Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
			int num = this.CurrentMethod.Name.Length + components.Length + hostHeaderString.ByteCount + components2.Length + 12 + headersSize;
			this.SetWriteBuffer(num);
			int num2 = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
			this.WriteBuffer[num2++] = 32;
			num2 += Encoding.ASCII.GetBytes(components, 0, components.Length, this.WriteBuffer, num2);
			hostHeaderString.Copy(this.WriteBuffer, num2);
			num2 += hostHeaderString.ByteCount;
			num2 += Encoding.ASCII.GetBytes(components2, 0, components2.Length, this.WriteBuffer, num2);
			this.WriteBuffer[num2++] = 32;
			return num2;
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x00039D9C File Offset: 0x00037F9C
		private int GenerateFtpProxyRequestLine(int headersSize)
		{
			string components = this._Uri.GetComponents(UriComponents.Scheme | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			string text = this._Uri.GetComponents(UriComponents.UserInfo | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			HostHeaderString hostHeaderString = new HostHeaderString(this.GetSafeHostAndPort(false, true));
			string components2 = this._Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
			if (text == "")
			{
				string text2 = null;
				string text3 = null;
				NetworkCredential credential = this.Credentials.GetCredential(this._Uri, "basic");
				if (credential != null && credential != FtpWebRequest.DefaultNetworkCredential)
				{
					text2 = credential.InternalGetDomainUserName();
					text3 = credential.InternalGetPassword();
					text3 = ((text3 == null) ? string.Empty : text3);
				}
				if (text2 != null)
				{
					text2 = text2.Replace(":", "%3A");
					text3 = text3.Replace(":", "%3A");
					text2 = text2.Replace("\\", "%5C");
					text3 = text3.Replace("\\", "%5C");
					text2 = text2.Replace("/", "%2F");
					text3 = text3.Replace("/", "%2F");
					text2 = text2.Replace("?", "%3F");
					text3 = text3.Replace("?", "%3F");
					text2 = text2.Replace("#", "%23");
					text3 = text3.Replace("#", "%23");
					text2 = text2.Replace("%", "%25");
					text3 = text3.Replace("%", "%25");
					text2 = text2.Replace("@", "%40");
					text3 = text3.Replace("@", "%40");
					text = text2 + ":" + text3 + "@";
				}
			}
			int num = this.CurrentMethod.Name.Length + components.Length + text.Length + hostHeaderString.ByteCount + components2.Length + 12 + headersSize;
			this.SetWriteBuffer(num);
			int num2 = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
			this.WriteBuffer[num2++] = 32;
			num2 += Encoding.ASCII.GetBytes(components, 0, components.Length, this.WriteBuffer, num2);
			num2 += Encoding.ASCII.GetBytes(text, 0, text.Length, this.WriteBuffer, num2);
			hostHeaderString.Copy(this.WriteBuffer, num2);
			num2 += hostHeaderString.ByteCount;
			num2 += Encoding.ASCII.GetBytes(components2, 0, components2.Length, this.WriteBuffer, num2);
			this.WriteBuffer[num2++] = 32;
			return num2;
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x0003A064 File Offset: 0x00038264
		private int GenerateRequestLine(int headersSize)
		{
			string pathAndQuery = this._Uri.PathAndQuery;
			int num = this.CurrentMethod.Name.Length + pathAndQuery.Length + 12 + headersSize;
			this.SetWriteBuffer(num);
			int num2 = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
			this.WriteBuffer[num2++] = 32;
			num2 += Encoding.ASCII.GetBytes(pathAndQuery, 0, pathAndQuery.Length, this.WriteBuffer, num2);
			this.WriteBuffer[num2++] = 32;
			return num2;
		}

		// Token: 0x06000A97 RID: 2711 RVA: 0x0003A10A File Offset: 0x0003830A
		internal Uri GetRemoteResourceUri()
		{
			if (this.UseCustomHost)
			{
				return this._HostUri;
			}
			return this._Uri;
		}

		// Token: 0x06000A98 RID: 2712 RVA: 0x0003A124 File Offset: 0x00038324
		internal void UpdateHeaders()
		{
			bool flag = this.IsTunnelRequest && this._OriginUri.Scheme == Uri.UriSchemeHttp;
			string text;
			if (this.UseCustomHost)
			{
				text = HttpWebRequest.GetSafeHostAndPort(this._HostUri, this._HostHasPort || flag, false);
			}
			else
			{
				text = this.GetSafeHostAndPort(flag, false);
			}
			HostHeaderString hostHeaderString = new HostHeaderString(text);
			string @string = WebHeaderCollection.HeaderEncoding.GetString(hostHeaderString.Bytes, 0, hostHeaderString.ByteCount);
			this._HttpRequestHeaders.ChangeInternal("Host", @string);
			if (this._CookieContainer != null)
			{
				CookieModule.OnSendingHeaders(this);
			}
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x0003A1B4 File Offset: 0x000383B4
		internal void SerializeHeaders()
		{
			if (this.HttpWriteMode != HttpWriteMode.None)
			{
				if (this.HttpWriteMode == HttpWriteMode.Chunked)
				{
					this._HttpRequestHeaders.AddInternal("Transfer-Encoding", "chunked");
				}
				else if (this.ContentLength >= 0L)
				{
					this._HttpRequestHeaders.ChangeInternal("Content-Length", this._ContentLength.ToString(NumberFormatInfo.InvariantInfo));
				}
				this.ExpectContinue = this.ExpectContinue && !this.IsVersionHttp10 && this.ServicePoint.Expect100Continue;
				if ((this.ContentLength > 0L || this.HttpWriteMode == HttpWriteMode.Chunked) && this.ExpectContinue)
				{
					this._HttpRequestHeaders.AddInternal("Expect", "100-continue");
				}
			}
			string text = this._HttpRequestHeaders.Get("Accept-Encoding") ?? string.Empty;
			if ((this.AutomaticDecompression & DecompressionMethods.GZip) != DecompressionMethods.None && text.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) < 0)
			{
				if ((this.AutomaticDecompression & DecompressionMethods.Deflate) != DecompressionMethods.None && text.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) < 0)
				{
					this._HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip, deflate");
				}
				else
				{
					this._HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip");
				}
			}
			else if ((this.AutomaticDecompression & DecompressionMethods.Deflate) != DecompressionMethods.None && text.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) < 0)
			{
				this._HttpRequestHeaders.AddInternal("Accept-Encoding", "deflate");
			}
			string text2 = "Connection";
			if (this.UsesProxySemantics || this.IsTunnelRequest)
			{
				this._HttpRequestHeaders.RemoveInternal("Connection");
				text2 = "Proxy-Connection";
				if (!ValidationHelper.IsBlankString(this.Connection))
				{
					this._HttpRequestHeaders.AddInternal("Proxy-Connection", this._HttpRequestHeaders["Connection"]);
				}
			}
			else
			{
				this._HttpRequestHeaders.RemoveInternal("Proxy-Connection");
			}
			if (this.IsWebSocketRequest)
			{
				string text3 = this._HttpRequestHeaders.Get("Connection") ?? string.Empty;
				if (text3.IndexOf("Upgrade", StringComparison.OrdinalIgnoreCase) < 0)
				{
					this._HttpRequestHeaders.AddInternal("Connection", "Upgrade");
				}
			}
			if (this.KeepAlive || this.NtlmKeepAlive)
			{
				if (this.IsVersionHttp10 || this.ServicePoint.HttpBehaviour <= HttpBehaviour.HTTP10)
				{
					this._HttpRequestHeaders.AddInternal((this.UsesProxySemantics || this.IsTunnelRequest) ? "Proxy-Connection" : "Connection", "Keep-Alive");
				}
			}
			else if (!this.IsVersionHttp10)
			{
				this._HttpRequestHeaders.AddInternal(text2, "Close");
			}
			string text4 = this._HttpRequestHeaders.ToString();
			int byteCount = WebHeaderCollection.HeaderEncoding.GetByteCount(text4);
			int num;
			if (this.CurrentMethod.ConnectRequest)
			{
				num = this.GenerateConnectRequestLine(byteCount);
			}
			else if (this.UsesProxySemantics)
			{
				num = this.GenerateProxyRequestLine(byteCount);
			}
			else
			{
				num = this.GenerateRequestLine(byteCount);
			}
			Buffer.BlockCopy(HttpWebRequest.HttpBytes, 0, this.WriteBuffer, num, HttpWebRequest.HttpBytes.Length);
			num += HttpWebRequest.HttpBytes.Length;
			this.WriteBuffer[num++] = 49;
			this.WriteBuffer[num++] = 46;
			this.WriteBuffer[num++] = (this.IsVersionHttp10 ? 48 : 49);
			this.WriteBuffer[num++] = 13;
			this.WriteBuffer[num++] = 10;
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, this, "Request: " + Encoding.ASCII.GetString(this.WriteBuffer, 0, num));
			}
			WebHeaderCollection.HeaderEncoding.GetBytes(text4, 0, text4.Length, this.WriteBuffer, num);
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class.</summary>
		// Token: 0x06000A9A RID: 2714 RVA: 0x0003A532 File Offset: 0x00038732
		[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public HttpWebRequest()
		{
		}

		// Token: 0x06000A9B RID: 2715 RVA: 0x0003A560 File Offset: 0x00038760
		internal HttpWebRequest(Uri uri, ServicePoint servicePoint)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "HttpWebRequest", uri);
			}
			this.CheckCertificateRevocationList = ServicePointManager.CheckCertificateRevocationList;
			this.SslProtocols = (SslProtocols)ServicePointManager.SecurityProtocol;
			this.CheckConnectPermission(uri, false);
			this.m_StartTimestamp = NetworkingPerfCounters.GetTimestamp();
			NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestCreated);
			this._HttpRequestHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
			this._Proxy = WebRequest.InternalDefaultWebProxy;
			this._HttpWriteMode = HttpWriteMode.Unknown;
			this._MaximumAllowedRedirections = 50;
			this._Timeout = 100000;
			this._TimerQueue = WebRequest.DefaultTimerQueue;
			this._ReadWriteTimeout = 300000;
			this._MaximumResponseHeadersLength = HttpWebRequest.DefaultMaximumResponseHeadersLength;
			this._ContentLength = -1L;
			this._originalContentLength = -1L;
			this._OriginVerb = KnownHttpVerb.Get;
			this._OriginUri = uri;
			this._Uri = this._OriginUri;
			this._ServicePoint = servicePoint;
			this._RequestIsAsync = TriState.Unspecified;
			this.m_ContinueTimeout = 350;
			this.m_ContinueTimerQueue = HttpWebRequest.s_ContinueTimerQueue;
			base.SetupCacheProtocol(this._OriginUri);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "HttpWebRequest", null);
			}
		}

		// Token: 0x06000A9C RID: 2716 RVA: 0x0003A6AC File Offset: 0x000388AC
		internal HttpWebRequest(Uri proxyUri, Uri requestUri, HttpWebRequest orginalRequest)
			: this(proxyUri, null)
		{
			this._OriginVerb = KnownHttpVerb.Parse("CONNECT");
			this.Pipelined = false;
			this._OriginUri = requestUri;
			this.IsTunnelRequest = true;
			this._ConnectionGroupName = ServicePointManager.SpecialConnectGroupName + "(" + HttpWebRequest.UniqueGroupId + ")";
			this.m_InternalConnectionGroup = true;
			this.ServerAuthenticationState = new AuthenticationState(true);
			base.CacheProtocol = null;
			this.m_ContinueTimeout = 350;
			this.m_ContinueTimerQueue = HttpWebRequest.s_ContinueTimerQueue;
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x0003A738 File Offset: 0x00038938
		internal HttpWebRequest(Uri uri, bool returnResponseOnFailureStatusCode, string connectionGroupName, Action<Stream> resendRequestContent)
			: this(uri, null)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "HttpWebRequest", string.Concat(new string[]
				{
					"uri: '",
					(uri != null) ? uri.ToString() : null,
					"', connectionGroupName: '",
					connectionGroupName,
					"'"
				}));
			}
			this._returnResponseOnFailureStatusCode = returnResponseOnFailureStatusCode;
			this._resendRequestContent = resendRequestContent;
			this._Booleans &= ~HttpWebRequest.Booleans.AllowWriteStreamBuffering;
			this.m_InternalConnectionGroup = true;
			this._ConnectionGroupName = connectionGroupName;
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "HttpWebRequest", null);
			}
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x0003A7DF File Offset: 0x000389DF
		internal HttpWebRequest(Uri uri, ServicePoint servicePoint, bool isWebSocketRequest, string connectionGroupName)
			: this(uri, servicePoint)
		{
			this.IsWebSocketRequest = isWebSocketRequest;
			this._ConnectionGroupName = connectionGroupName;
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Net.HttpWebRequest" /> class from the specified instances of the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> classes.</summary>
		/// <param name="serializationInfo">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
		/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> object that contains the source and destination of the serialized stream associated with the new <see cref="T:System.Net.HttpWebRequest" /> object. </param>
		// Token: 0x06000A9F RID: 2719 RVA: 0x0003A7F8 File Offset: 0x000389F8
		[Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected HttpWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "HttpWebRequest", serializationInfo);
			}
			this.CheckCertificateRevocationList = ServicePointManager.CheckCertificateRevocationList;
			this.SslProtocols = (SslProtocols)ServicePointManager.SecurityProtocol;
			this._HttpRequestHeaders = (WebHeaderCollection)serializationInfo.GetValue("_HttpRequestHeaders", typeof(WebHeaderCollection));
			this._Proxy = (IWebProxy)serializationInfo.GetValue("_Proxy", typeof(IWebProxy));
			this.KeepAlive = serializationInfo.GetBoolean("_KeepAlive");
			this.Pipelined = serializationInfo.GetBoolean("_Pipelined");
			this.AllowAutoRedirect = serializationInfo.GetBoolean("_AllowAutoRedirect");
			if (!serializationInfo.GetBoolean("_AllowWriteStreamBuffering"))
			{
				this._Booleans &= ~HttpWebRequest.Booleans.AllowWriteStreamBuffering;
			}
			this.HttpWriteMode = (HttpWriteMode)serializationInfo.GetInt32("_HttpWriteMode");
			this._MaximumAllowedRedirections = serializationInfo.GetInt32("_MaximumAllowedRedirections");
			this._AutoRedirects = serializationInfo.GetInt32("_AutoRedirects");
			this._Timeout = serializationInfo.GetInt32("_Timeout");
			this.m_ContinueTimeout = 350;
			this.m_ContinueTimerQueue = HttpWebRequest.s_ContinueTimerQueue;
			try
			{
				this._ReadWriteTimeout = serializationInfo.GetInt32("_ReadWriteTimeout");
			}
			catch
			{
				this._ReadWriteTimeout = 300000;
			}
			try
			{
				this._MaximumResponseHeadersLength = serializationInfo.GetInt32("_MaximumResponseHeadersLength");
			}
			catch
			{
				this._MaximumResponseHeadersLength = HttpWebRequest.DefaultMaximumResponseHeadersLength;
			}
			this._ContentLength = serializationInfo.GetInt64("_ContentLength");
			this._MediaType = serializationInfo.GetString("_MediaType");
			this._OriginVerb = KnownHttpVerb.Parse(serializationInfo.GetString("_OriginVerb"));
			this._ConnectionGroupName = serializationInfo.GetString("_ConnectionGroupName");
			this.ProtocolVersion = (Version)serializationInfo.GetValue("_Version", typeof(Version));
			this._OriginUri = (Uri)serializationInfo.GetValue("_OriginUri", typeof(Uri));
			base.SetupCacheProtocol(this._OriginUri);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "HttpWebRequest", null);
			}
		}

		/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
		/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
		// Token: 0x06000AA0 RID: 2720 RVA: 0x0003AA60 File Offset: 0x00038C60
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			this.GetObjectData(serializationInfo, streamingContext);
		}

		/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data required to serialize the target object.</summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data. </param>
		/// <param name="streamingContext">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the destination for this serialization.</param>
		// Token: 0x06000AA1 RID: 2721 RVA: 0x0003AA6C File Offset: 0x00038C6C
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue("_HttpRequestHeaders", this._HttpRequestHeaders, typeof(WebHeaderCollection));
			serializationInfo.AddValue("_Proxy", this._Proxy, typeof(IWebProxy));
			serializationInfo.AddValue("_KeepAlive", this.KeepAlive);
			serializationInfo.AddValue("_Pipelined", this.Pipelined);
			serializationInfo.AddValue("_AllowAutoRedirect", this.AllowAutoRedirect);
			serializationInfo.AddValue("_AllowWriteStreamBuffering", this.AllowWriteStreamBuffering);
			serializationInfo.AddValue("_HttpWriteMode", this.HttpWriteMode);
			serializationInfo.AddValue("_MaximumAllowedRedirections", this._MaximumAllowedRedirections);
			serializationInfo.AddValue("_AutoRedirects", this._AutoRedirects);
			serializationInfo.AddValue("_Timeout", this._Timeout);
			serializationInfo.AddValue("_ReadWriteTimeout", this._ReadWriteTimeout);
			serializationInfo.AddValue("_MaximumResponseHeadersLength", this._MaximumResponseHeadersLength);
			serializationInfo.AddValue("_ContentLength", this.ContentLength);
			serializationInfo.AddValue("_MediaType", this._MediaType);
			serializationInfo.AddValue("_OriginVerb", this._OriginVerb);
			serializationInfo.AddValue("_ConnectionGroupName", this._ConnectionGroupName);
			serializationInfo.AddValue("_Version", this.ProtocolVersion, typeof(Version));
			serializationInfo.AddValue("_OriginUri", this._OriginUri, typeof(Uri));
			base.GetObjectData(serializationInfo, streamingContext);
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x0003ABE0 File Offset: 0x00038DE0
		internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
		{
			StringBuilder stringBuilder = new StringBuilder(connectionGroupName);
			stringBuilder.Append(unsafeConnectionGroup ? "U>" : "S>");
			if (isInternalGroup)
			{
				stringBuilder.Append("I>");
			}
			return stringBuilder;
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x0003AC1C File Offset: 0x00038E1C
		internal string GetConnectionGroupLine()
		{
			StringBuilder stringBuilder = HttpWebRequest.GenerateConnectionGroup(this._ConnectionGroupName, this.UnsafeAuthenticatedConnectionSharing, this.m_InternalConnectionGroup);
			if (this._Uri.Scheme == Uri.UriSchemeHttps || this.IsTunnelRequest)
			{
				if (this.UsesProxy)
				{
					stringBuilder.Append(this.GetSafeHostAndPort(true, false));
					stringBuilder.Append("$");
				}
				if (this._ClientCertificates != null && this.ClientCertificates.Count > 0)
				{
					stringBuilder.Append(this.ClientCertificates.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
				}
				if (this.ServerCertificateValidationCallback != null)
				{
					stringBuilder.Append("&");
					stringBuilder.Append(HttpWebRequest.GetDelegateId(this.ServerCertificateValidationCallback));
				}
			}
			if (this.ProxyAuthenticationState.UniqueGroupId != null)
			{
				stringBuilder.Append(this.ProxyAuthenticationState.UniqueGroupId);
			}
			else if (this.ServerAuthenticationState.UniqueGroupId != null)
			{
				stringBuilder.Append(this.ServerAuthenticationState.UniqueGroupId);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000AA4 RID: 2724 RVA: 0x0003AD24 File Offset: 0x00038F24
		private static string GetDelegateId(RemoteCertificateValidationCallback callback)
		{
			string text2;
			try
			{
				new ReflectionPermission(PermissionState.Unrestricted).Assert();
				MethodInfo method = callback.Method;
				string name = callback.Method.Name;
				object target = callback.Target;
				string text;
				if (target == null)
				{
					text = method.DeclaringType.FullName;
				}
				else
				{
					text = target.GetType().Name + "#" + target.GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
				}
				text2 = text + "::" + name;
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
			return text2;
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000AA5 RID: 2725 RVA: 0x0003ADBC File Offset: 0x00038FBC
		// (set) Token: 0x06000AA6 RID: 2726 RVA: 0x0003ADC4 File Offset: 0x00038FC4
		internal ServerCertValidationCallback ServerCertValidationCallback { get; private set; }

		/// <summary>Gets or sets a callback function to validate the server certificate.</summary>
		/// <returns>A callback function to validate the server certificate.A callback function to validate the server certificate.</returns>
		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000AA7 RID: 2727 RVA: 0x0003ADCD File Offset: 0x00038FCD
		// (set) Token: 0x06000AA8 RID: 2728 RVA: 0x0003ADE4 File Offset: 0x00038FE4
		public RemoteCertificateValidationCallback ServerCertificateValidationCallback
		{
			get
			{
				if (this.ServerCertValidationCallback == null)
				{
					return null;
				}
				return this.ServerCertValidationCallback.ValidationCallback;
			}
			set
			{
				ExceptionHelper.InfrastructurePermission.Demand();
				if (value == null)
				{
					this.ServerCertValidationCallback = null;
					return;
				}
				this.ServerCertValidationCallback = new ServerCertValidationCallback(value);
			}
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x0003AE08 File Offset: 0x00039008
		private bool CheckResubmitForAuth()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (this.UsesProxySemantics && this._Proxy != null && this._Proxy.Credentials != null)
			{
				try
				{
					flag |= this.ProxyAuthenticationState.AttemptAuthenticate(this, this._Proxy.Credentials);
				}
				catch (Win32Exception)
				{
					if (!this.m_Extra401Retry)
					{
						throw;
					}
					flag3 = true;
				}
				flag2 = true;
			}
			if (this.Credentials != null && !flag3)
			{
				try
				{
					flag |= this.ServerAuthenticationState.AttemptAuthenticate(this, this.Credentials);
				}
				catch (Win32Exception)
				{
					if (!this.m_Extra401Retry)
					{
						throw;
					}
					flag = false;
				}
				flag2 = true;
			}
			if (!flag && flag2 && this.m_Extra401Retry)
			{
				this.ClearAuthenticatedConnectionResources();
				this.m_Extra401Retry = false;
				flag = true;
			}
			return flag;
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x0003AED4 File Offset: 0x000390D4
		private bool CheckResubmitForCache(ref Exception e)
		{
			if (this.CheckCacheRetrieveOnResponse())
			{
				this.CheckCacheUpdateOnResponse();
				return false;
			}
			if (this.AllowAutoRedirect)
			{
				if (Logging.On)
				{
					Logging.PrintWarning(Logging.Web, this, "", global::System.SR.GetString("net_log_cache_validation_failed_resubmit"));
				}
				return true;
			}
			if (Logging.On)
			{
				Logging.PrintError(Logging.Web, this, "", global::System.SR.GetString("net_log_cache_refused_server_response"));
			}
			e = new InvalidOperationException(global::System.SR.GetString("net_cache_not_accept_response"));
			return false;
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x0003AF4F File Offset: 0x0003914F
		private void SetExceptionIfRequired(string message, ref Exception e)
		{
			this.SetExceptionIfRequired(message, null, ref e);
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x0003AF5C File Offset: 0x0003915C
		private void SetExceptionIfRequired(string message, Exception innerException, ref Exception e)
		{
			if (this._returnResponseOnFailureStatusCode)
			{
				if (Logging.On)
				{
					if (innerException != null)
					{
						Logging.Exception(Logging.Web, this, "", innerException);
					}
					Logging.PrintWarning(Logging.Web, this, "", message);
					return;
				}
			}
			else
			{
				e = new WebException(message, innerException, WebExceptionStatus.ProtocolError, this._HttpResponse);
			}
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x0003AFB0 File Offset: 0x000391B0
		private bool CheckResubmit(ref Exception e, ref bool disableUpload)
		{
			bool flag = false;
			if (this.ResponseStatusCode == HttpStatusCode.Unauthorized || this.ResponseStatusCode == HttpStatusCode.ProxyAuthenticationRequired)
			{
				try
				{
					if (!(flag = this.CheckResubmitForAuth()))
					{
						this.SetExceptionIfRequired(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), ref e);
						return false;
					}
					goto IL_0516;
				}
				catch (Win32Exception ex)
				{
					throw new WebException(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), ex, WebExceptionStatus.ProtocolError, this._HttpResponse);
				}
			}
			if (this.ServerAuthenticationState != null && this.ServerAuthenticationState.Authorization != null)
			{
				HttpWebResponse httpResponse = this._HttpResponse;
				if (httpResponse != null)
				{
					httpResponse.InternalSetIsMutuallyAuthenticated = this.ServerAuthenticationState.Authorization.MutuallyAuthenticated;
					if (base.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired && !httpResponse.IsMutuallyAuthenticated)
					{
						throw new WebException(global::System.SR.GetString("net_webstatus_RequestCanceled"), new ProtocolViolationException(global::System.SR.GetString("net_mutualauthfailed")), WebExceptionStatus.RequestCanceled, httpResponse);
					}
				}
			}
			if (this.ResponseStatusCode == HttpStatusCode.BadRequest && this.SendChunked && this.HttpWriteMode != HttpWriteMode.ContentLength && this.ServicePoint.InternalProxyServicePoint && this.AllowWriteStreamBuffering)
			{
				this.ClearAuthenticatedConnectionResources();
				return true;
			}
			if (this.AllowAutoRedirect && (this.ResponseStatusCode == HttpStatusCode.MultipleChoices || this.ResponseStatusCode == HttpStatusCode.MovedPermanently || this.ResponseStatusCode == HttpStatusCode.Found || this.ResponseStatusCode == HttpStatusCode.SeeOther || this.ResponseStatusCode == HttpStatusCode.TemporaryRedirect))
			{
				this._AutoRedirects++;
				if (this._AutoRedirects > this._MaximumAllowedRedirections)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_tooManyRedirections"), ref e);
					return false;
				}
				string location = this._HttpResponse.Headers.Location;
				if (location == null)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), ref e);
					return false;
				}
				Uri uri;
				try
				{
					uri = new Uri(this._Uri, location);
				}
				catch (UriFormatException ex2)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_resubmitprotofailed"), ex2, ref e);
					return false;
				}
				if (this.IsWebSocketRequest)
				{
					if (uri.Scheme == Uri.UriSchemeWs)
					{
						uri = new UriBuilder(uri)
						{
							Scheme = Uri.UriSchemeHttp
						}.Uri;
					}
					else if (uri.Scheme == Uri.UriSchemeWss)
					{
						uri = new UriBuilder(uri)
						{
							Scheme = Uri.UriSchemeHttps
						}.Uri;
					}
				}
				if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_resubmitprotofailed"), ref e);
					return false;
				}
				if (!this.HasRedirectPermission(uri, ref e))
				{
					return false;
				}
				Uri uri2 = this._Uri;
				this._Uri = uri;
				this._RedirectedToDifferentHost = Uri.Compare(this._OriginUri, this._Uri, UriComponents.HostAndPort, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) != 0;
				if (this.UseCustomHost)
				{
					string hostAndPortString = HttpWebRequest.GetHostAndPortString(this._HostUri.Host, this._HostUri.Port, true);
					Uri uri3;
					bool flag2 = this.TryGetHostUri(hostAndPortString, out uri3);
					if (!this.HasRedirectPermission(uri3, ref e))
					{
						this._Uri = uri2;
						return false;
					}
					this._HostUri = uri3;
				}
				if (this.ResponseStatusCode > (HttpStatusCode)299 && Logging.On)
				{
					Logging.PrintWarning(Logging.Web, this, "", global::System.SR.GetString("net_log_server_response_error_code", new object[] { ((int)this.ResponseStatusCode).ToString(NumberFormatInfo.InvariantInfo) }));
				}
				if (this.HttpWriteMode != HttpWriteMode.None)
				{
					HttpStatusCode responseStatusCode = this.ResponseStatusCode;
					if (responseStatusCode - HttpStatusCode.MovedPermanently > 1)
					{
						if (responseStatusCode != HttpStatusCode.TemporaryRedirect)
						{
							disableUpload = true;
						}
					}
					else if (this.CurrentMethod.Equals(KnownHttpVerb.Post))
					{
						disableUpload = true;
					}
					if (disableUpload)
					{
						if (!this.AllowWriteStreamBuffering && this.IsOutstandingGetRequestStream)
						{
							return false;
						}
						this.CurrentMethod = KnownHttpVerb.Get;
						this.ExpectContinue = false;
						this.HttpWriteMode = HttpWriteMode.None;
					}
				}
				ICredentials credentials = this.Credentials as CredentialCache;
				if (credentials == null)
				{
					credentials = this.Credentials as SystemNetworkCredential;
				}
				if (credentials == null)
				{
					this.Credentials = null;
				}
				this.ProxyAuthenticationState.ClearAuthReq(this);
				this.ServerAuthenticationState.ClearAuthReq(this);
				if (this._OriginUri.Scheme == Uri.UriSchemeHttps)
				{
					this._HttpRequestHeaders.RemoveInternal("Referer");
				}
			}
			else
			{
				if (this.ResponseStatusCode > (HttpStatusCode)399)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), ref e);
					return false;
				}
				if (this.AllowAutoRedirect && this.ResponseStatusCode > (HttpStatusCode)299)
				{
					this.SetExceptionIfRequired(global::System.SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), ref e);
					return false;
				}
				return false;
			}
			IL_0516:
			if (this.HttpWriteMode != HttpWriteMode.None && !this.AllowWriteStreamBuffering && this._resendRequestContent == null && this.UserRetrievedWriteStream && (this.HttpWriteMode != HttpWriteMode.ContentLength || this.ContentLength != 0L))
			{
				e = new WebException(global::System.SR.GetString("net_need_writebuffering"), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
				return false;
			}
			if (!flag)
			{
				this.ClearAuthenticatedConnectionResources();
			}
			if (Logging.On)
			{
				Logging.PrintWarning(Logging.Web, this, "", global::System.SR.GetString("net_log_resubmitting_request"));
			}
			return true;
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x0003B56C File Offset: 0x0003976C
		private bool HasRedirectPermission(Uri uri, ref Exception resultException)
		{
			try
			{
				this.CheckConnectPermission(uri, this.Async);
			}
			catch (SecurityException ex)
			{
				resultException = new SecurityException(global::System.SR.GetString("net_redirect_perm"), new WebException(global::System.SR.GetString("net_resubmitcanceled"), ex, WebExceptionStatus.ProtocolError, this._HttpResponse));
				return false;
			}
			return true;
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x0003B5C8 File Offset: 0x000397C8
		private void CheckConnectPermission(Uri uri, bool needExecutionContext)
		{
			ExecutionContext executionContext = (needExecutionContext ? this.GetReadingContext().ContextCopy : null);
			CodeAccessPermission codeAccessPermission = new WebPermission(NetworkAccess.Connect, uri);
			if (executionContext == null)
			{
				codeAccessPermission.Demand();
				return;
			}
			ExecutionContext.Run(executionContext, NclUtilities.ContextRelativeDemandCallback, codeAccessPermission);
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0003B608 File Offset: 0x00039808
		private void ClearRequestForResubmit(bool ntlmFollowupRequest)
		{
			this._HttpRequestHeaders.RemoveInternal("Host");
			this._HttpRequestHeaders.RemoveInternal("Connection");
			this._HttpRequestHeaders.RemoveInternal("Proxy-Connection");
			this._HttpRequestHeaders.RemoveInternal("Content-Length");
			this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
			this._HttpRequestHeaders.RemoveInternal("Expect");
			if (this._HttpResponse != null && this._HttpResponse.ResponseStream != null)
			{
				if (!this._HttpResponse.KeepAlive)
				{
					ConnectStream connectStream = this._HttpResponse.ResponseStream as ConnectStream;
					if (connectStream != null)
					{
						connectStream.ErrorResponseNotify(false);
					}
				}
				ICloseEx closeEx = this._HttpResponse.ResponseStream as ICloseEx;
				if (closeEx != null)
				{
					closeEx.CloseEx(CloseExState.Silent);
				}
				else
				{
					this._HttpResponse.ResponseStream.Close();
				}
			}
			this._AbortDelegate = null;
			this.m_BodyStarted = false;
			this.HeadersCompleted = false;
			this._WriteBufferLength = 0;
			this.m_Extra401Retry = false;
			HttpWebResponse httpResponse = this._HttpResponse;
			this._HttpResponse = null;
			this.m_ContinueGate.Reset();
			this._RerequestCount++;
			if (!this.Aborted && this.Async)
			{
				this._CoreResponse = null;
			}
			if (this._SubmitWriteStream != null)
			{
				if (((httpResponse != null && httpResponse.KeepAlive) || this._SubmitWriteStream.IgnoreSocketErrors) && this.HasEntityBody)
				{
					if (this.AllowWriteStreamBuffering)
					{
						this.SetRequestContinue();
					}
					if (ntlmFollowupRequest)
					{
						this.NeedsToReadForResponse = !this.ShouldWaitFor100Continue();
						this._SubmitWriteStream.CallDone();
					}
					else if (!this.AllowWriteStreamBuffering)
					{
						this.NeedsToReadForResponse = !this.ShouldWaitFor100Continue();
						this._SubmitWriteStream.CloseInternal(true);
					}
					else if (!this.Async && this.UserRetrievedWriteStream)
					{
						this._SubmitWriteStream.CallDone();
					}
				}
				if ((this.Async || this.UserRetrievedWriteStream) && this._OldSubmitWriteStream != null && this._OldSubmitWriteStream != this._SubmitWriteStream)
				{
					this._SubmitWriteStream.CloseInternal(true);
				}
			}
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x0003B80C File Offset: 0x00039A0C
		private void FinishRequest(HttpWebResponse response, Exception errorException)
		{
			if (!this._ReadAResult.InternalPeekCompleted && this.m_Aborted != 1 && response != null && errorException != null)
			{
				response.ResponseStream = this.MakeMemoryStream(response.ResponseStream);
			}
			if (errorException != null && this._SubmitWriteStream != null && !this._SubmitWriteStream.IsClosed)
			{
				this._SubmitWriteStream.ErrorResponseNotify(this._SubmitWriteStream.Connection.KeepAlive);
			}
			if (errorException == null && this._HttpResponse != null && (this._HttpWriteMode == HttpWriteMode.Chunked || this._ContentLength > 0L) && this.ExpectContinue && !this.Saw100Continue && this._ServicePoint.Understands100Continue && !this.IsTunnelRequest && this.ResponseStatusCode <= (HttpStatusCode)299)
			{
				this._ServicePoint.Understands100Continue = false;
			}
		}

		// Token: 0x06000AB2 RID: 2738 RVA: 0x0003B8D8 File Offset: 0x00039AD8
		private Stream MakeMemoryStream(Stream stream)
		{
			if (stream == null || stream is SyncMemoryStream)
			{
				return stream;
			}
			SyncMemoryStream syncMemoryStream = new SyncMemoryStream(0);
			try
			{
				if (stream.CanRead)
				{
					byte[] array = new byte[1024];
					int num = ((HttpWebRequest.DefaultMaximumErrorResponseLength == -1) ? array.Length : (HttpWebRequest.DefaultMaximumErrorResponseLength * 1024));
					int num2;
					while ((num2 = stream.Read(array, 0, Math.Min(array.Length, num))) > 0)
					{
						syncMemoryStream.Write(array, 0, num2);
						if (HttpWebRequest.DefaultMaximumErrorResponseLength != -1)
						{
							num -= num2;
						}
					}
				}
				syncMemoryStream.Position = 0L;
			}
			catch
			{
			}
			finally
			{
				try
				{
					ICloseEx closeEx = stream as ICloseEx;
					if (closeEx != null)
					{
						closeEx.CloseEx(CloseExState.Silent);
					}
					else
					{
						stream.Close();
					}
				}
				catch
				{
				}
			}
			return syncMemoryStream;
		}

		/// <summary>Adds a byte range header to the request for a specified range.</summary>
		/// <param name="from">The position at which to start sending data. </param>
		/// <param name="to">The position at which to stop sending data. </param>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///         <paramref name="from" /> is greater than <paramref name="to" />-or- 
		///         <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB3 RID: 2739 RVA: 0x0003B9B0 File Offset: 0x00039BB0
		public void AddRange(int from, int to)
		{
			this.AddRange("bytes", (long)from, (long)to);
		}

		/// <summary>Adds a byte range header to the request for a specified range.</summary>
		/// <param name="from">The position at which to start sending data.</param>
		/// <param name="to">The position at which to stop sending data.</param>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///         <paramref name="from" /> is greater than <paramref name="to" />-or- 
		///         <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB4 RID: 2740 RVA: 0x0003B9C1 File Offset: 0x00039BC1
		public void AddRange(long from, long to)
		{
			this.AddRange("bytes", from, to);
		}

		/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
		/// <param name="range">The starting or ending point of the range. </param>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB5 RID: 2741 RVA: 0x0003B9D0 File Offset: 0x00039BD0
		public void AddRange(int range)
		{
			this.AddRange("bytes", (long)range);
		}

		/// <summary>Adds a byte range header to a request for a specific range from the beginning or end of the requested data.</summary>
		/// <param name="range">The starting or ending point of the range.</param>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB6 RID: 2742 RVA: 0x0003B9DF File Offset: 0x00039BDF
		public void AddRange(long range)
		{
			this.AddRange("bytes", range);
		}

		/// <summary>Adds a range header to a request for a specified range.</summary>
		/// <param name="rangeSpecifier">The description of the range. </param>
		/// <param name="from">The position at which to start sending data. </param>
		/// <param name="to">The position at which to stop sending data. </param>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="rangeSpecifier" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///         <paramref name="from" /> is greater than <paramref name="to" />-or- 
		///         <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB7 RID: 2743 RVA: 0x0003B9ED File Offset: 0x00039BED
		public void AddRange(string rangeSpecifier, int from, int to)
		{
			this.AddRange(rangeSpecifier, (long)from, (long)to);
		}

		/// <summary>Adds a range header to a request for a specified range.</summary>
		/// <param name="rangeSpecifier">The description of the range.</param>
		/// <param name="from">The position at which to start sending data.</param>
		/// <param name="to">The position at which to stop sending data.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="rangeSpecifier" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///         <paramref name="from" /> is greater than <paramref name="to" />-or- 
		///         <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB8 RID: 2744 RVA: 0x0003B9FC File Offset: 0x00039BFC
		public void AddRange(string rangeSpecifier, long from, long to)
		{
			if (rangeSpecifier == null)
			{
				throw new ArgumentNullException("rangeSpecifier");
			}
			if (from < 0L || to < 0L)
			{
				throw new ArgumentOutOfRangeException("from, to", global::System.SR.GetString("net_rangetoosmall"));
			}
			if (from > to)
			{
				throw new ArgumentOutOfRangeException("from", global::System.SR.GetString("net_fromto"));
			}
			if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
			{
				throw new ArgumentException(global::System.SR.GetString("net_nottoken"), "rangeSpecifier");
			}
			if (!this.AddRange(rangeSpecifier, from.ToString(NumberFormatInfo.InvariantInfo), to.ToString(NumberFormatInfo.InvariantInfo)))
			{
				throw new InvalidOperationException(global::System.SR.GetString("net_rangetype"));
			}
		}

		/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
		/// <param name="rangeSpecifier">The description of the range. </param>
		/// <param name="range">The starting or ending point of the range. </param>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="rangeSpecifier" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000AB9 RID: 2745 RVA: 0x0003BA9D File Offset: 0x00039C9D
		public void AddRange(string rangeSpecifier, int range)
		{
			this.AddRange(rangeSpecifier, (long)range);
		}

		/// <summary>Adds a Range header to a request for a specific range from the beginning or end of the requested data.</summary>
		/// <param name="rangeSpecifier">The description of the range.</param>
		/// <param name="range">The starting or ending point of the range.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///         <paramref name="rangeSpecifier" /> is <see langword="null" />. </exception>
		/// <exception cref="T:System.ArgumentException">
		///         <paramref name="rangeSpecifier" /> is invalid. </exception>
		/// <exception cref="T:System.InvalidOperationException">The range header could not be added. </exception>
		// Token: 0x06000ABA RID: 2746 RVA: 0x0003BAA8 File Offset: 0x00039CA8
		public void AddRange(string rangeSpecifier, long range)
		{
			if (rangeSpecifier == null)
			{
				throw new ArgumentNullException("rangeSpecifier");
			}
			if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
			{
				throw new ArgumentException(global::System.SR.GetString("net_nottoken"), "rangeSpecifier");
			}
			if (!this.AddRange(rangeSpecifier, range.ToString(NumberFormatInfo.InvariantInfo), (range >= 0L) ? "" : null))
			{
				throw new InvalidOperationException(global::System.SR.GetString("net_rangetype"));
			}
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x0003BB14 File Offset: 0x00039D14
		private bool AddRange(string rangeSpecifier, string from, string to)
		{
			string text = this._HttpRequestHeaders["Range"];
			if (text == null || text.Length == 0)
			{
				text = rangeSpecifier + "=";
			}
			else
			{
				if (string.Compare(text.Substring(0, text.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
				{
					return false;
				}
				text = string.Empty;
			}
			text += from.ToString();
			if (to != null)
			{
				text = text + "-" + to;
			}
			this._HttpRequestHeaders.SetAddVerified("Range", text);
			return true;
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000ABC RID: 2748 RVA: 0x0003BB9C File Offset: 0x00039D9C
		private static string UniqueGroupId
		{
			get
			{
				return Interlocked.Increment(ref HttpWebRequest.s_UniqueGroupId).ToString(NumberFormatInfo.InvariantInfo);
			}
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x0003BBC0 File Offset: 0x00039DC0
		private static int GetStatusCode(HttpWebResponse httpWebResponse)
		{
			int num = -1;
			if (FrameworkEventSource.Log.IsEnabled() && httpWebResponse != null)
			{
				try
				{
					num = (int)httpWebResponse.StatusCode;
				}
				catch (ObjectDisposedException)
				{
				}
			}
			return num;
		}

		// Token: 0x04000ED4 RID: 3796
		private bool m_Saw100Continue;

		// Token: 0x04000ED5 RID: 3797
		private bool m_KeepAlive = true;

		// Token: 0x04000ED6 RID: 3798
		private bool m_LockConnection;

		// Token: 0x04000ED7 RID: 3799
		private bool m_NtlmKeepAlive;

		// Token: 0x04000ED8 RID: 3800
		private bool m_PreAuthenticate;

		// Token: 0x04000ED9 RID: 3801
		private DecompressionMethods m_AutomaticDecompression;

		// Token: 0x04000EDA RID: 3802
		private int m_Aborted;

		// Token: 0x04000EDB RID: 3803
		private bool m_OnceFailed;

		// Token: 0x04000EDC RID: 3804
		private bool m_Pipelined = true;

		// Token: 0x04000EDD RID: 3805
		private bool m_Retry = true;

		// Token: 0x04000EDE RID: 3806
		private bool m_HeadersCompleted;

		// Token: 0x04000EDF RID: 3807
		private bool m_IsCurrentAuthenticationStateProxy;

		// Token: 0x04000EE0 RID: 3808
		private bool m_NeedsToReadForResponse = true;

		// Token: 0x04000EE1 RID: 3809
		private bool m_BodyStarted;

		// Token: 0x04000EE2 RID: 3810
		private bool m_RequestSubmitted;

		// Token: 0x04000EE3 RID: 3811
		private bool m_OriginallyBuffered;

		// Token: 0x04000EE4 RID: 3812
		private bool m_Extra401Retry;

		// Token: 0x04000EE5 RID: 3813
		private long m_StartTimestamp;

		// Token: 0x04000EE6 RID: 3814
		internal const HttpStatusCode MaxOkStatus = (HttpStatusCode)299;

		// Token: 0x04000EE7 RID: 3815
		private const HttpStatusCode MaxRedirectionStatus = (HttpStatusCode)399;

		// Token: 0x04000EE8 RID: 3816
		private const int RequestLineConstantSize = 12;

		// Token: 0x04000EE9 RID: 3817
		private const string ContinueHeader = "100-continue";

		// Token: 0x04000EEA RID: 3818
		internal const string ChunkedHeader = "chunked";

		// Token: 0x04000EEB RID: 3819
		internal const string GZipHeader = "gzip";

		// Token: 0x04000EEC RID: 3820
		internal const string DeflateHeader = "deflate";

		// Token: 0x04000EED RID: 3821
		internal const int DefaultReadWriteTimeout = 300000;

		// Token: 0x04000EEE RID: 3822
		internal const int DefaultContinueTimeout = 350;

		// Token: 0x04000EEF RID: 3823
		private static readonly byte[] HttpBytes = new byte[] { 72, 84, 84, 80, 47 };

		// Token: 0x04000EF0 RID: 3824
		private static readonly WaitCallback s_EndWriteHeaders_Part2Callback = new WaitCallback(HttpWebRequest.EndWriteHeaders_Part2Wrapper);

		// Token: 0x04000EF1 RID: 3825
		private static readonly TimerThread.Callback s_ContinueTimeoutCallback = new TimerThread.Callback(HttpWebRequest.ContinueTimeoutCallback);

		// Token: 0x04000EF2 RID: 3826
		private static readonly TimerThread.Queue s_ContinueTimerQueue = TimerThread.GetOrCreateQueue(350);

		// Token: 0x04000EF3 RID: 3827
		private static readonly TimerThread.Callback s_TimeoutCallback = new TimerThread.Callback(HttpWebRequest.TimeoutCallback);

		// Token: 0x04000EF4 RID: 3828
		private static readonly WaitCallback s_AbortWrapper = new WaitCallback(HttpWebRequest.AbortWrapper);

		// Token: 0x04000EF5 RID: 3829
		private static int s_UniqueGroupId;

		// Token: 0x04000EF6 RID: 3830
		private HttpWebRequest.Booleans _Booleans = HttpWebRequest.Booleans.Default;

		// Token: 0x04000EF7 RID: 3831
		private TimerThread.Timer m_ContinueTimer;

		// Token: 0x04000EF8 RID: 3832
		private InterlockedGate m_ContinueGate;

		// Token: 0x04000EF9 RID: 3833
		private int m_ContinueTimeout;

		// Token: 0x04000EFA RID: 3834
		private TimerThread.Queue m_ContinueTimerQueue;

		// Token: 0x04000EFB RID: 3835
		private object m_PendingReturnResult;

		// Token: 0x04000EFC RID: 3836
		private Connection m_TunnelConnection;

		// Token: 0x04000EFD RID: 3837
		private LazyAsyncResult _WriteAResult;

		// Token: 0x04000EFE RID: 3838
		private LazyAsyncResult _ReadAResult;

		// Token: 0x04000EFF RID: 3839
		private LazyAsyncResult _ConnectionAResult;

		// Token: 0x04000F00 RID: 3840
		private LazyAsyncResult _ConnectionReaderAResult;

		// Token: 0x04000F01 RID: 3841
		private TriState _RequestIsAsync;

		// Token: 0x04000F02 RID: 3842
		private HttpContinueDelegate _ContinueDelegate;

		// Token: 0x04000F03 RID: 3843
		internal ServicePoint _ServicePoint;

		// Token: 0x04000F04 RID: 3844
		internal HttpWebResponse _HttpResponse;

		// Token: 0x04000F05 RID: 3845
		private object _CoreResponse;

		// Token: 0x04000F06 RID: 3846
		private int _NestedWriteSideCheck;

		// Token: 0x04000F07 RID: 3847
		private KnownHttpVerb _Verb;

		// Token: 0x04000F08 RID: 3848
		private KnownHttpVerb _OriginVerb;

		// Token: 0x04000F09 RID: 3849
		private bool _HostHasPort;

		// Token: 0x04000F0A RID: 3850
		private Uri _HostUri;

		// Token: 0x04000F0B RID: 3851
		private WebHeaderCollection _HttpRequestHeaders;

		// Token: 0x04000F0C RID: 3852
		private byte[] _WriteBuffer;

		// Token: 0x04000F0D RID: 3853
		private int _WriteBufferLength;

		// Token: 0x04000F0E RID: 3854
		private const int CachedWriteBufferSize = 512;

		// Token: 0x04000F0F RID: 3855
		private static global::System.PinnableBufferCache _WriteBufferCache = new global::System.PinnableBufferCache("System.Net.HttpWebRequest", 512);

		// Token: 0x04000F10 RID: 3856
		private bool _WriteBufferFromPinnableCache;

		// Token: 0x04000F11 RID: 3857
		private HttpWriteMode _HttpWriteMode;

		// Token: 0x04000F12 RID: 3858
		private Uri _Uri;

		// Token: 0x04000F13 RID: 3859
		private Uri _OriginUri;

		// Token: 0x04000F14 RID: 3860
		private string _MediaType;

		// Token: 0x04000F15 RID: 3861
		private long _ContentLength;

		// Token: 0x04000F16 RID: 3862
		private IWebProxy _Proxy;

		// Token: 0x04000F17 RID: 3863
		private ProxyChain _ProxyChain;

		// Token: 0x04000F18 RID: 3864
		private string _ConnectionGroupName;

		// Token: 0x04000F19 RID: 3865
		private bool m_InternalConnectionGroup;

		// Token: 0x04000F1A RID: 3866
		private AuthenticationState _ProxyAuthenticationState;

		// Token: 0x04000F1B RID: 3867
		private AuthenticationState _ServerAuthenticationState;

		// Token: 0x04000F1C RID: 3868
		private ICredentials _AuthInfo;

		// Token: 0x04000F1D RID: 3869
		private HttpAbortDelegate _AbortDelegate;

		// Token: 0x04000F1E RID: 3870
		private ConnectStream _SubmitWriteStream;

		// Token: 0x04000F1F RID: 3871
		private ConnectStream _OldSubmitWriteStream;

		// Token: 0x04000F20 RID: 3872
		private int _MaximumAllowedRedirections;

		// Token: 0x04000F21 RID: 3873
		private int _AutoRedirects;

		// Token: 0x04000F22 RID: 3874
		private bool _RedirectedToDifferentHost;

		// Token: 0x04000F23 RID: 3875
		private int _RerequestCount;

		// Token: 0x04000F24 RID: 3876
		private int _Timeout;

		// Token: 0x04000F25 RID: 3877
		private TimerThread.Timer _Timer;

		// Token: 0x04000F26 RID: 3878
		private TimerThread.Queue _TimerQueue;

		// Token: 0x04000F27 RID: 3879
		private int _RequestContinueCount;

		// Token: 0x04000F28 RID: 3880
		private int _ReadWriteTimeout;

		// Token: 0x04000F29 RID: 3881
		private CookieContainer _CookieContainer;

		// Token: 0x04000F2A RID: 3882
		private int _MaximumResponseHeadersLength;

		// Token: 0x04000F2B RID: 3883
		private UnlockConnectionDelegate _UnlockDelegate;

		// Token: 0x04000F2C RID: 3884
		private bool _returnResponseOnFailureStatusCode;

		// Token: 0x04000F2D RID: 3885
		private Action<Stream> _resendRequestContent;

		// Token: 0x04000F2E RID: 3886
		private long _originalContentLength;

		// Token: 0x04000F2F RID: 3887
		private X509CertificateCollection _ClientCertificates;

		// Token: 0x02000707 RID: 1799
		private static class AbortState
		{
			// Token: 0x040030D5 RID: 12501
			public const int Public = 1;

			// Token: 0x040030D6 RID: 12502
			public const int Internal = 2;
		}

		// Token: 0x02000708 RID: 1800
		[Flags]
		private enum Booleans : uint
		{
			// Token: 0x040030D8 RID: 12504
			AllowAutoRedirect = 1U,
			// Token: 0x040030D9 RID: 12505
			AllowWriteStreamBuffering = 2U,
			// Token: 0x040030DA RID: 12506
			ExpectContinue = 4U,
			// Token: 0x040030DB RID: 12507
			ProxySet = 16U,
			// Token: 0x040030DC RID: 12508
			UnsafeAuthenticatedConnectionSharing = 64U,
			// Token: 0x040030DD RID: 12509
			IsVersionHttp10 = 128U,
			// Token: 0x040030DE RID: 12510
			SendChunked = 256U,
			// Token: 0x040030DF RID: 12511
			EnableDecompression = 512U,
			// Token: 0x040030E0 RID: 12512
			IsTunnelRequest = 1024U,
			// Token: 0x040030E1 RID: 12513
			IsWebSocketRequest = 2048U,
			// Token: 0x040030E2 RID: 12514
			Default = 7U
		}
	}
}

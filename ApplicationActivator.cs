using System;
using System.Collections;
using System.Deployment.Application.Manifest;
using System.Deployment.Internal;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Deployment.Application
{
	// Token: 0x020000EA RID: 234
	internal class ApplicationActivator
	{
		// Token: 0x06000387 RID: 903 RVA: 0x000092FC File Offset: 0x000074FC
		private void DisplayActivationFailureReason(Exception exception, string errorPageUrl)
		{
			string text = Resources.GetString("ErrorMessage_GenericActivationFailure");
			string @string = Resources.GetString("ErrorMessage_GenericLinkUrlMessage");
			Exception innerMostException = this.GetInnerMostException(exception);
			if (exception is DeploymentDownloadException)
			{
				text = Resources.GetString("ErrorMessage_NetworkError");
				DeploymentDownloadException ex = (DeploymentDownloadException)exception;
				if (ex.SubType == ExceptionTypes.SizeLimitForPartialTrustOnlineAppExceeded)
				{
					text = Resources.GetString("ErrorMessage_SizeLimitForPartialTrustOnlineAppExceeded");
				}
				if (innerMostException is WebException)
				{
					WebException ex2 = (WebException)innerMostException;
					if (ex2.Response != null && ex2.Response is HttpWebResponse)
					{
						HttpWebResponse httpWebResponse = (HttpWebResponse)ex2.Response;
						if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
						{
							text = Resources.GetString("ErrorMessage_FileMissing");
						}
						else if (httpWebResponse.StatusCode == HttpStatusCode.Unauthorized)
						{
							text = Resources.GetString("ErrorMessage_AuthenticationError");
						}
						else if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden)
						{
							text = Resources.GetString("ErrorMessage_Forbidden");
						}
					}
				}
				else if (innerMostException is FileNotFoundException || innerMostException is DirectoryNotFoundException)
				{
					text = Resources.GetString("ErrorMessage_FileMissing");
				}
				else if (innerMostException is UnauthorizedAccessException)
				{
					text = Resources.GetString("ErrorMessage_AuthenticationError");
				}
				else if (innerMostException is IOException && !this.IsWebExceptionInExceptionStack(exception))
				{
					text = Resources.GetString("ErrorMessage_DownloadIOError");
				}
			}
			else if (exception is InvalidDeploymentException)
			{
				InvalidDeploymentException ex3 = (InvalidDeploymentException)exception;
				if (ex3.SubType == ExceptionTypes.ManifestLoad)
				{
					text = Resources.GetString("ErrorMessage_ManifestCannotBeLoaded");
				}
				else if (ex3.SubType == ExceptionTypes.Manifest || ex3.SubType == ExceptionTypes.ManifestParse || ex3.SubType == ExceptionTypes.ManifestSemanticValidation)
				{
					text = Resources.GetString("ErrorMessage_InvalidManifest");
				}
				else if (ex3.SubType == ExceptionTypes.Validation || ex3.SubType == ExceptionTypes.HashValidation || ex3.SubType == ExceptionTypes.SignatureValidation || ex3.SubType == ExceptionTypes.RefDefValidation || ex3.SubType == ExceptionTypes.ClrValidation || ex3.SubType == ExceptionTypes.StronglyNamedAssemblyVerification || ex3.SubType == ExceptionTypes.IdentityMatchValidationForMixedModeAssembly || ex3.SubType == ExceptionTypes.AppFileLocationValidation || ex3.SubType == ExceptionTypes.FileSizeValidation)
				{
					text = Resources.GetString("ErrorMessage_ValidationFailed");
				}
				else if (ex3.SubType == ExceptionTypes.UnsupportedElevetaionRequest)
				{
					text = Resources.GetString("ErrorMessage_ManifestExecutionLevelNotSupported");
				}
			}
			else if (exception is DeploymentException)
			{
				if (((DeploymentException)exception).SubType == ExceptionTypes.ComponentStore)
				{
					text = Resources.GetString("ErrorMessage_StoreError");
				}
				else if (((DeploymentException)exception).SubType == ExceptionTypes.ActivationLimitExceeded)
				{
					text = Resources.GetString("ErrorMessage_ConcurrentActivationLimitExceeded");
				}
				else if (((DeploymentException)exception).SubType == ExceptionTypes.DiskIsFull)
				{
					text = Resources.GetString("ErrorMessage_DiskIsFull");
				}
				else if (((DeploymentException)exception).SubType == ExceptionTypes.DeploymentUriDifferent)
				{
					text = exception.Message;
				}
				else if (((DeploymentException)exception).SubType == ExceptionTypes.GroupMultipleMatch)
				{
					text = exception.Message;
				}
				else if (((DeploymentException)exception).SubType == ExceptionTypes.TrustFailDependentPlatform)
				{
					text = exception.Message;
				}
			}
			string text2 = Logger.GetLogFilePath();
			if (!Logger.FlushCurrentThreadLogs())
			{
				text2 = null;
			}
			string text3 = null;
			if (errorPageUrl != null)
			{
				text3 = string.Format("{0}?outer={1}&&inner={2}&&msg={3}", new object[]
				{
					errorPageUrl,
					exception.GetType().ToString(),
					innerMostException.GetType().ToString(),
					innerMostException.Message
				});
				if (text3.Length > 2048)
				{
					text3 = text3.Substring(0, 2048);
				}
			}
			this._ui.ShowError(Resources.GetString("UI_ErrorTitle"), text, text2, text3, @string);
		}

		// Token: 0x06000388 RID: 904 RVA: 0x00009670 File Offset: 0x00007870
		private void DisplayPlatformDetectionFailureUI(DependentPlatformMissingException ex)
		{
			Uri uri = null;
			if (this._fullTrust)
			{
				uri = ex.SupportUrl;
			}
			this._ui.ShowPlatform(ex.Message, uri);
		}

		// Token: 0x06000389 RID: 905 RVA: 0x000096A0 File Offset: 0x000078A0
		public void ActivateDeployment(string activationUrl, bool isShortcut)
		{
			LifetimeManager.StartOperation();
			bool flag = false;
			try
			{
				object[] array = new object[] { activationUrl, isShortcut, null, null, null };
				flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), array);
				if (!flag)
				{
					throw new OutOfMemoryException();
				}
			}
			finally
			{
				if (!flag)
				{
					LifetimeManager.EndOperation();
				}
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0000970C File Offset: 0x0000790C
		public void ActivateDeploymentEx(string activationUrl, int unsignedPolicy, int signedPolicy)
		{
			LifetimeManager.StartOperation();
			bool flag = false;
			try
			{
				ApplicationActivator.BrowserSettings browserSettings = new ApplicationActivator.BrowserSettings();
				browserSettings.ManagedSignedFlag = ApplicationActivator.BrowserSettings.GetManagedFlagValue(signedPolicy);
				browserSettings.ManagedUnSignedFlag = ApplicationActivator.BrowserSettings.GetManagedFlagValue(unsignedPolicy);
				object[] array = new object[] { activationUrl, false, null, null, browserSettings };
				flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), array);
				if (!flag)
				{
					throw new OutOfMemoryException();
				}
			}
			finally
			{
				if (!flag)
				{
					LifetimeManager.EndOperation();
				}
			}
		}

		// Token: 0x0600038B RID: 907 RVA: 0x00009798 File Offset: 0x00007998
		public void ActivateApplicationExtension(string textualSubId, string deploymentProviderUrl, string targetAssociatedFile)
		{
			LifetimeManager.StartOperation();
			bool flag = false;
			try
			{
				object[] array = new object[] { targetAssociatedFile, false, textualSubId, deploymentProviderUrl, null };
				flag = ThreadPool.QueueUserWorkItem(new WaitCallback(this.ActivateDeploymentWorker), array);
				if (!flag)
				{
					throw new OutOfMemoryException();
				}
			}
			finally
			{
				if (!flag)
				{
					LifetimeManager.EndOperation();
				}
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00009804 File Offset: 0x00007A04
		private void ActivateDeploymentWorker(object state)
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			try
			{
				CodeMarker_Singleton.Instance.CodeMarker(525);
				object[] array = (object[])state;
				text = ((string)array[0]) ?? string.Empty;
				Logger.StartCurrentThreadLogging();
				Logger.SetSubscriptionUrl(text);
				Logger.AddInternalState("Activation through dfsvc.exe started.");
				Logger.AddMethodCall("ActivateDeploymentWorker({0},{1},{2},{3},{4}) called.", array);
				bool flag = (bool)array[1];
				if (array[2] != null)
				{
					text2 = (string)array[2];
				}
				if (array[3] != null)
				{
					text3 = (string)array[3];
				}
				ApplicationActivator.BrowserSettings browserSettings = null;
				if (array[4] != null)
				{
					browserSettings = (ApplicationActivator.BrowserSettings)array[4];
				}
				Uri uri = null;
				string text4 = null;
				try
				{
					int num = this.CheckActivationInProgress(text);
					this._ui = new UserInterface(false);
					if (!PolicyKeys.SuppressLimitOnNumberOfActivations() && num > 8)
					{
						throw new DeploymentException(ExceptionTypes.ActivationLimitExceeded, Resources.GetString("Ex_TooManyLiveActivation"));
					}
					if (text.Length > 16384)
					{
						throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_UrlTooLong"));
					}
					uri = new Uri(text);
					try
					{
						UriHelper.ValidateSupportedSchemeInArgument(uri, "activationUrl");
					}
					catch (ArgumentException ex)
					{
						throw new InvalidDeploymentException(ExceptionTypes.UriSchemeNotSupported, Resources.GetString("Ex_NotSupportedUriScheme"), ex);
					}
					Logger.AddPhaseInformation(Resources.GetString("PhaseLog_StartOfActivation"), new object[] { text });
					this.PerformDeploymentActivationWithRetry(uri, flag, text2, text3, browserSettings, ref text4);
					Logger.AddPhaseInformation(Resources.GetString("ActivateManifestSucceeded"), new object[] { text });
				}
				catch (DependentPlatformMissingException ex2)
				{
					Logger.AddErrorInformation(ex2, Resources.GetString("ActivateManifestException"), new object[] { text });
					if (this._ui == null)
					{
						this._ui = new UserInterface();
					}
					if (!this._ui.SplashCancelled())
					{
						this.DisplayPlatformDetectionFailureUI(ex2);
					}
				}
				catch (DownloadCancelledException ex3)
				{
					Logger.AddErrorInformation(ex3, Resources.GetString("ActivateManifestException"), new object[] { text });
				}
				catch (TrustNotGrantedException ex4)
				{
					Logger.AddErrorInformation(ex4, Resources.GetString("ActivateManifestException"), new object[] { text });
				}
				catch (DeploymentException ex5)
				{
					Logger.AddErrorInformation(ex5, Resources.GetString("ActivateManifestException"), new object[] { text });
					if (ex5.SubType != ExceptionTypes.ActivationInProgress)
					{
						if (this._ui == null)
						{
							this._ui = new UserInterface();
						}
						if (!this._ui.SplashCancelled())
						{
							if (ex5.SubType == ExceptionTypes.ActivationLimitExceeded)
							{
								if (Interlocked.CompareExchange(ref ApplicationActivator._liveActivationLimitUIStatus, 1, 0) == 0)
								{
									this.DisplayActivationFailureReason(ex5, text4);
									Interlocked.CompareExchange(ref ApplicationActivator._liveActivationLimitUIStatus, 0, 1);
								}
							}
							else
							{
								this.DisplayActivationFailureReason(ex5, text4);
							}
						}
					}
				}
				catch (Exception ex6)
				{
					if (ex6 is AccessViolationException || ex6 is OutOfMemoryException)
					{
						throw;
					}
					if (PolicyKeys.DisableGenericExceptionHandler())
					{
						throw;
					}
					Logger.AddErrorInformation(ex6, Resources.GetString("ActivateManifestException"), new object[] { text });
					if (this._ui == null)
					{
						this._ui = new UserInterface();
					}
					if (!this._ui.SplashCancelled())
					{
						this.DisplayActivationFailureReason(ex6, text4);
					}
				}
			}
			finally
			{
				this.RemoveActivationInProgressEntry(text);
				if (this._ui != null)
				{
					this._ui.Dispose();
					this._ui = null;
				}
				CodeMarker_Singleton.Instance.CodeMarker(526);
				Logger.EndCurrentThreadLogging();
				LifetimeManager.EndOperation();
			}
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00009BCC File Offset: 0x00007DCC
		private void UninstallApplicationAndRedirectActivation(ref bool isShortCut, ref Uri deploymentProviderUri, string textualSubId, Uri activationUri)
		{
			try
			{
				DefinitionIdentity definitionIdentity = null;
				SubscriptionState subscriptionState = null;
				TempFile tempFile = null;
				Uri uri = null;
				SubscriptionStore currentUser = SubscriptionStore.CurrentUser;
				currentUser.RefreshStorePointer();
				if (isShortCut)
				{
					int num = activationUri.LocalPath.IndexOf('|', 0);
					string text = ((num > 0) ? activationUri.LocalPath.Substring(0, num) : activationUri.LocalPath);
					ShellExposure.ParseAppShortcut(text, out definitionIdentity, out uri);
					subscriptionState = currentUser.GetSubscriptionState(definitionIdentity);
				}
				else if (textualSubId != null)
				{
					definitionIdentity = new DefinitionIdentity(textualSubId);
					subscriptionState = currentUser.GetSubscriptionState(definitionIdentity);
				}
				else
				{
					AssemblyManifest assemblyManifest = DownloadManager.DownloadDeploymentManifestBypass(currentUser, ref deploymentProviderUri, out tempFile, out subscriptionState, null, null);
				}
				subscriptionState.SubscriptionStore.UninstallSubscription(subscriptionState);
				if (isShortCut)
				{
					deploymentProviderUri = uri;
					isShortCut = false;
				}
			}
			catch (DeploymentException ex)
			{
				Logger.AddErrorInformation(Resources.GetString("Uninstall_FailedMsg"), ex);
				ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
				exceptionDispatchInfo.Throw();
			}
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00009CAC File Offset: 0x00007EAC
		private void CleanApplicationReInstall(bool isShortcut, Uri deploymentUri, Uri activationUri, ApplicationActivator.BrowserSettings browserSettings, string textualSubId, string errorPageUrl, string deploymentProviderUrlFromExtension, string shortcutFilePath)
		{
			if (isShortcut && string.IsNullOrEmpty(shortcutFilePath))
			{
				throw new DeploymentException(ExceptionTypes.InvalidShortcut, Resources.GetString("Ex_InvalidShortcutFormat"));
			}
			int num = shortcutFilePath.IndexOf('|', 0);
			if (num > 0)
			{
				shortcutFilePath = shortcutFilePath.Substring(0, num);
			}
			FileInfo fileInfo = null;
			string text = string.Empty;
			if (isShortcut)
			{
				fileInfo = new FileInfo(shortcutFilePath);
				text = string.Format("{0}_{1}", shortcutFilePath, Guid.NewGuid());
				fileInfo = fileInfo.CopyTo(text);
			}
			this.UninstallApplicationAndRedirectActivation(ref isShortcut, ref deploymentUri, textualSubId, activationUri);
			try
			{
				this.PerformDeploymentActivation(activationUri, isShortcut, textualSubId, deploymentProviderUrlFromExtension, browserSettings, ref errorPageUrl, ref deploymentUri);
			}
			catch (Exception ex)
			{
				if (text != string.Empty)
				{
					fileInfo.MoveTo(shortcutFilePath);
				}
				ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
				exceptionDispatchInfo.Throw();
			}
			if (text != string.Empty)
			{
				fileInfo.Delete();
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00009D90 File Offset: 0x00007F90
		private void PerformDeploymentActivationWithRetry(Uri activationUri, bool isShortcut, string textualSubId, string deploymentProviderUrlFromExtension, ApplicationActivator.BrowserSettings browserSettings, ref string errorPageUrl)
		{
			Uri uri = null;
			try
			{
				this.PerformDeploymentActivation(activationUri, isShortcut, textualSubId, deploymentProviderUrlFromExtension, browserSettings, ref errorPageUrl, ref uri);
			}
			catch (Exception ex)
			{
				if (textualSubId == null)
				{
					if (ex is DirectoryNotFoundException || ex is FileNotFoundException || ex is DriveNotFoundException || (ex is COMException && (ex.HResult & 65535) == 267))
					{
						Logger.AddMethodCall("Partial store corruption scenario called due to exception {0}", new object[] { ex.ToString() });
						this.CleanApplicationReInstall(isShortcut, uri, activationUri, browserSettings, textualSubId, errorPageUrl, deploymentProviderUrlFromExtension, activationUri.LocalPath);
					}
					else if (ex is FileLoadException)
					{
						Logger.AddMethodCall("Locked file scenario called");
						Thread.Sleep(10000);
						this.PerformDeploymentActivation(activationUri, isShortcut, textualSubId, deploymentProviderUrlFromExtension, browserSettings, ref errorPageUrl, ref uri);
					}
					else
					{
						ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
						exceptionDispatchInfo.Throw();
					}
				}
				else
				{
					ExceptionDispatchInfo exceptionDispatchInfo2 = ExceptionDispatchInfo.Capture(ex);
					exceptionDispatchInfo2.Throw();
				}
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x00009E80 File Offset: 0x00008080
		private void PerformDeploymentActivation(Uri activationUri, bool isShortcut, string textualSubId, string deploymentProviderUrlFromExtension, ApplicationActivator.BrowserSettings browserSettings, ref string errorPageUrl, ref Uri deploymentUri)
		{
			TempFile tempFile = null;
			Logger.AddMethodCall("PerformDeploymentActivation called.");
			try
			{
				string text = null;
				Uri uri = null;
				bool flag = false;
				this._subStore = SubscriptionStore.CurrentUser;
				this._subStore.RefreshStorePointer();
				if (null == deploymentUri)
				{
					deploymentUri = activationUri;
				}
				bool flag2 = false;
				ActivationDescription activationDescription;
				if (textualSubId != null)
				{
					Logger.AddInternalState("Activating through file association.");
					flag2 = true;
					activationDescription = this.ProcessOrFollowExtension(activationUri, textualSubId, deploymentProviderUrlFromExtension, ref errorPageUrl, out tempFile);
					if (activationDescription == null)
					{
						return;
					}
				}
				else if (isShortcut)
				{
					Logger.AddInternalState("Activating through shortcut.");
					text = activationUri.LocalPath;
					activationDescription = this.ProcessOrFollowShortcut(text, ref errorPageUrl, out tempFile);
					if (activationDescription == null)
					{
						return;
					}
				}
				else
				{
					Logger.AddInternalState("Activating through deployment manifest.");
					Logger.AddInternalState("Start processing deployment manifest.");
					SubscriptionState subscriptionState;
					AssemblyManifest assemblyManifest = DownloadManager.DownloadDeploymentManifestBypass(this._subStore, ref deploymentUri, out tempFile, out subscriptionState, null, null);
					if (browserSettings != null && tempFile != null)
					{
						browserSettings.Validate(tempFile.Path);
					}
					if (assemblyManifest.Description != null)
					{
						errorPageUrl = assemblyManifest.Description.ErrorReportUrl;
					}
					activationDescription = new ActivationDescription();
					if (subscriptionState != null)
					{
						text = null;
						activationDescription.SetApplicationManifest(subscriptionState.CurrentApplicationManifest, null, null);
						activationDescription.AppId = subscriptionState.CurrentBind;
						Logger.AddInternalState("Running from the store. Bypass further downloads and verifications.");
						flag = true;
					}
					else
					{
						text = tempFile.Path;
					}
					Logger.SetDeploymentManifest(assemblyManifest);
					Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ProcessingDeploymentManifestComplete"));
					Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
					activationDescription.SetDeploymentManifest(assemblyManifest, deploymentUri, text);
					activationDescription.IsUpdate = false;
					activationDescription.ActType = ActivationType.InstallViaDotApplication;
					uri = activationUri;
				}
				if (this._ui.SplashCancelled())
				{
					throw new DownloadCancelledException();
				}
				if (activationDescription.DeployManifest.Deployment == null)
				{
					throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_NotDeploymentOrShortcut"));
				}
				bool flag3 = false;
				SubscriptionState subscriptionState2 = this._subStore.GetSubscriptionState(activationDescription.DeployManifest);
				this.CheckDeploymentProviderValidity(activationDescription, subscriptionState2);
				if (!flag)
				{
					Logger.AddInternalState("Could not find application in store. Continue with downloading application manifest.");
					flag3 = this.InstallApplication(ref subscriptionState2, activationDescription);
					Logger.AddPhaseInformation(Resources.GetString("PhaseLog_InstallationComplete"));
					Logger.AddInternalState("Installation of application has successfully completed.");
				}
				else
				{
					this._subStore.SetLastCheckTimeToNow(subscriptionState2);
				}
				if (activationDescription.DeployManifest.Deployment.DisallowUrlActivation && !isShortcut && (!activationUri.IsFile || activationUri.IsUnc))
				{
					if (flag3)
					{
						this._ui.ShowMessage(Resources.GetString("Activation_DisallowUrlActivationMessageAfterInstall"), Resources.GetString("Activation_DisallowUrlActivationCaptionAfterInstall"));
					}
					else
					{
						this._ui.ShowMessage(Resources.GetString("Activation_DisallowUrlActivationMessage"), Resources.GetString("Activation_DisallowUrlActivationCaption"));
					}
				}
				else if (flag2)
				{
					this.Activate(activationDescription.AppId, activationDescription.AppManifest, activationUri.AbsoluteUri, true);
				}
				else if (isShortcut)
				{
					string text2 = null;
					int num = text.IndexOf('|', 0);
					if (num > 0 && num + 1 < text.Length)
					{
						text2 = text.Substring(num + 1);
					}
					if (text2 == null)
					{
						this.Activate(activationDescription.AppId, activationDescription.AppManifest, null, false);
					}
					else
					{
						this.Activate(activationDescription.AppId, activationDescription.AppManifest, text2, true);
					}
				}
				else
				{
					this.Activate(activationDescription.AppId, activationDescription.AppManifest, uri.AbsoluteUri, false);
				}
			}
			finally
			{
				if (tempFile != null)
				{
					tempFile.Dispose();
				}
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0000A1C8 File Offset: 0x000083C8
		private ActivationDescription ProcessOrFollowExtension(Uri associatedFile, string textualSubId, string deploymentProviderUrlFromExtension, ref string errorPageUrl, out TempFile deployFile)
		{
			deployFile = null;
			Logger.AddMethodCall(string.Concat(new string[]
			{
				"ProcessOrFollowExtension(",
				(associatedFile != null) ? associatedFile.ToString() : null,
				",",
				textualSubId,
				",",
				deploymentProviderUrlFromExtension,
				",",
				errorPageUrl,
				") called."
			}));
			DefinitionIdentity definitionIdentity = new DefinitionIdentity(textualSubId);
			SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(definitionIdentity);
			ActivationDescription activationDescription = null;
			if (subscriptionState.IsInstalled && subscriptionState.IsShellVisible)
			{
				Logger.AddInternalState("Application family is already installed and Shell Visible.");
				this.PerformDeploymentUpdate(ref subscriptionState, ref errorPageUrl);
				this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, associatedFile.AbsoluteUri, true);
			}
			else
			{
				Logger.AddInternalState("Application family is not installed or is not Shell-Visible.  Try to deploy it from the deployment provider specified in the extension : " + deploymentProviderUrlFromExtension);
				if (string.IsNullOrEmpty(deploymentProviderUrlFromExtension))
				{
					throw new DeploymentException(ExceptionTypes.Activation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileAssociationNoDpUrl"), new object[] { textualSubId }));
				}
				Uri uri = new Uri(deploymentProviderUrlFromExtension);
				Logger.AddInternalState("Start processing deployment manifest.");
				AssemblyManifest assemblyManifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref uri, out deployFile);
				if (assemblyManifest.Description != null)
				{
					errorPageUrl = assemblyManifest.Description.ErrorReportUrl;
				}
				Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
				if (!assemblyManifest.Deployment.Install)
				{
					throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_FileAssociationRefOnline"));
				}
				activationDescription = new ActivationDescription();
				activationDescription.SetDeploymentManifest(assemblyManifest, uri, deployFile.Path);
				activationDescription.IsUpdate = false;
				activationDescription.ActType = ActivationType.InstallViaFileAssociation;
			}
			return activationDescription;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x0000A350 File Offset: 0x00008550
		private ActivationDescription ProcessOrFollowShortcut(string shortcutFile, ref string errorPageUrl, out TempFile deployFile)
		{
			deployFile = null;
			Logger.AddMethodCall(string.Concat(new string[] { "ProcessOrFollowShortcut(shortcutFile=", shortcutFile, ",errorPageUrl=", errorPageUrl, ") called." }));
			string text = shortcutFile;
			string text2 = null;
			int num = shortcutFile.IndexOf('|', 0);
			if (num > 0)
			{
				text = shortcutFile.Substring(0, num);
				if (num + 1 < shortcutFile.Length)
				{
					text2 = shortcutFile.Substring(num + 1);
				}
			}
			Logger.AddInternalState("shortcutParameter=" + text2);
			DefinitionIdentity definitionIdentity;
			Uri uri;
			ShellExposure.ParseAppShortcut(text, out definitionIdentity, out uri);
			SubscriptionState subscriptionState = this._subStore.GetSubscriptionState(definitionIdentity);
			ActivationDescription activationDescription = null;
			if (subscriptionState.IsInstalled && subscriptionState.IsShellVisible)
			{
				Logger.AddInternalState("Application family is already installed and Shell Visible.");
				this.PerformDeploymentUpdate(ref subscriptionState, ref errorPageUrl);
				if (text2 == null)
				{
					this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, null, false);
				}
				else
				{
					this.Activate(subscriptionState.CurrentBind, subscriptionState.CurrentApplicationManifest, text2, true);
				}
			}
			else
			{
				Uri uri2 = uri;
				string text3 = "Application family is not installed or is not Shell-Visible.  Try to deploy it from the deployment provider specified in the shortcut : ";
				Uri uri3 = uri2;
				Logger.AddInternalState(text3 + ((uri3 != null) ? uri3.ToString() : null));
				Logger.AddInternalState("Start processing deployment manifest.");
				AssemblyManifest assemblyManifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref uri2, out deployFile);
				Logger.AddInternalState("Processing of deployment manifest has successfully completed.");
				if (assemblyManifest.Description != null)
				{
					errorPageUrl = assemblyManifest.Description.ErrorReportUrl;
				}
				if (!assemblyManifest.Deployment.Install)
				{
					throw new DeploymentException(ExceptionTypes.Activation, Resources.GetString("Ex_ShortcutRefOnlineOnly"));
				}
				activationDescription = new ActivationDescription();
				activationDescription.SetDeploymentManifest(assemblyManifest, uri2, deployFile.Path);
				activationDescription.IsUpdate = false;
				activationDescription.ActType = ActivationType.InstallViaShortcut;
			}
			return activationDescription;
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0000A4F0 File Offset: 0x000086F0
		private void Activate(DefinitionAppId appId, AssemblyManifest appManifest, string activationParameter, bool useActivationParameter)
		{
			using (ActivationContext activationContext = ActivationContext.CreatePartialActivationContext(appId.ToApplicationIdentity()))
			{
				InternalActivationContextHelper.PrepareForExecution(activationContext);
				this._subStore.ActivateApplication(appId, activationParameter, useActivationParameter);
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0000A53C File Offset: 0x0000873C
		private void PerformDeploymentUpdate(ref SubscriptionState subState, ref string errorPageUrl)
		{
			DeploymentUpdate deploymentUpdate = subState.CurrentDeploymentManifest.Deployment.DeploymentUpdate;
			bool flag = deploymentUpdate != null && deploymentUpdate.BeforeApplicationStartup;
			Logger.AddPhaseInformation(Resources.GetString("PhaseLog_DeploymentUpdateCheck"));
			Logger.AddMethodCall("PerformDeploymentUpdate called.");
			string text = "UpdateOnStart=";
			string text2 = flag.ToString();
			string text3 = ",PendingDeployment=";
			DefinitionIdentity pendingDeployment = subState.PendingDeployment;
			Logger.AddInternalState(text + text2 + text3 + ((pendingDeployment != null) ? pendingDeployment.ToString() : null));
			if (flag || (subState.PendingDeployment != null && !ApplicationActivator.SkipUpdate(subState, subState.PendingDeployment)))
			{
				TempFile tempFile = null;
				try
				{
					Uri deploymentProviderUri = subState.DeploymentProviderUri;
					AssemblyManifest assemblyManifest;
					try
					{
						string text4 = "Start processing deployment manifest for update check : ";
						Uri uri = deploymentProviderUri;
						Logger.AddInternalState(text4 + ((uri != null) ? uri.ToString() : null));
						assemblyManifest = DownloadManager.DownloadDeploymentManifest(this._subStore, ref deploymentProviderUri, out tempFile);
						Logger.AddInternalState("End processing deployment manifest.");
						if (assemblyManifest.Description != null)
						{
							errorPageUrl = assemblyManifest.Description.ErrorReportUrl;
						}
					}
					catch (DeploymentDownloadException ex)
					{
						Logger.AddErrorInformation(ex, Resources.GetString("Upd_UpdateCheckDownloadFailed"), new object[] { subState.SubscriptionId.ToString() });
						return;
					}
					if (this._ui.SplashCancelled())
					{
						throw new DownloadCancelledException();
					}
					if (!ApplicationActivator.SkipUpdate(subState, assemblyManifest.Identity) && this._subStore.CheckUpdateInManifest(subState, deploymentProviderUri, assemblyManifest, subState.CurrentDeployment.Version) != null && !assemblyManifest.Identity.Equals(subState.ExcludedDeployment))
					{
						Logger.AddInternalState("Update available in the deployment server.");
						ActivationDescription activationDescription = new ActivationDescription();
						activationDescription.SetDeploymentManifest(assemblyManifest, deploymentProviderUri, tempFile.Path);
						activationDescription.IsUpdate = true;
						activationDescription.IsRequiredUpdate = false;
						activationDescription.ActType = ActivationType.UpdateViaShortcutOrFA;
						if (assemblyManifest.Deployment.MinimumRequiredVersion != null && assemblyManifest.Deployment.MinimumRequiredVersion.CompareTo(subState.CurrentDeployment.Version) > 0)
						{
							activationDescription.IsRequiredUpdate = true;
						}
						this.CheckDeploymentProviderValidity(activationDescription, subState);
						this.ConsumeUpdatedDeployment(ref subState, activationDescription);
					}
				}
				finally
				{
					if (tempFile != null)
					{
						tempFile.Dispose();
					}
				}
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x0000A780 File Offset: 0x00008980
		private void CheckDeploymentProviderValidity(ActivationDescription actDesc, SubscriptionState subState)
		{
			if (actDesc.DeployManifest.Deployment.Install && actDesc.DeployManifest.Deployment.ProviderCodebaseUri == null && subState != null && subState.DeploymentProviderUri != null)
			{
				Uri uri = ((subState.DeploymentProviderUri.Query != null && subState.DeploymentProviderUri.Query.Length > 0) ? new Uri(subState.DeploymentProviderUri.GetLeftPart(UriPartial.Path)) : subState.DeploymentProviderUri);
				Logger.AddInternalState("Checking deployment provider validity.");
				string text = "providerCodebaseUri=";
				Uri uri2 = uri;
				Logger.AddInternalState(text + ((uri2 != null) ? uri2.ToString() : null));
				Logger.AddInternalState("actDesc.ToAppCodebase()=" + actDesc.ToAppCodebase());
				if (!uri.Equals(actDesc.ToAppCodebase()))
				{
					throw new DeploymentException(ExceptionTypes.DeploymentUriDifferent, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_DeploymentUriDifferent"), new object[] { actDesc.DeployManifest.Description.FilteredProduct }), new DeploymentException(ExceptionTypes.DeploymentUriDifferent, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DeploymentUriDifferentExText"), new object[]
					{
						actDesc.DeployManifest.Description.FilteredProduct,
						actDesc.DeploySourceUri.AbsoluteUri,
						subState.DeploymentProviderUri.AbsoluteUri
					})));
				}
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0000A8E0 File Offset: 0x00008AE0
		private void ConsumeUpdatedDeployment(ref SubscriptionState subState, ActivationDescription actDesc)
		{
			AssemblyManifest deployManifest = actDesc.DeployManifest;
			DefinitionIdentity identity = deployManifest.Identity;
			Uri deploySourceUri = actDesc.DeploySourceUri;
			Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ConsumeUpdatedDeployment"));
			Logger.AddInternalState("Consuming new update.");
			if (!actDesc.IsRequiredUpdate)
			{
				Logger.AddInternalState("Update is not a required update.");
				Description effectiveDescription = subState.EffectiveDescription;
				UserInterfaceInfo userInterfaceInfo = new UserInterfaceInfo();
				userInterfaceInfo.formTitle = Resources.GetString("UI_UpdateTitle");
				userInterfaceInfo.productName = effectiveDescription.Product;
				userInterfaceInfo.supportUrl = effectiveDescription.SupportUrl;
				userInterfaceInfo.sourceSite = UserInterface.GetDisplaySite(deploySourceUri);
				UserInterfaceModalResult userInterfaceModalResult = this._ui.ShowUpdate(userInterfaceInfo);
				if (userInterfaceModalResult == UserInterfaceModalResult.Skip)
				{
					TimeSpan timeSpan = new TimeSpan(7, 0, 0, 0);
					DateTime dateTime = DateTime.UtcNow + timeSpan;
					this._subStore.SetUpdateSkipTime(subState, identity, dateTime);
					Logger.AddPhaseInformation(Resources.GetString("Upd_DeployUpdateSkipping"));
					Logger.AddInternalState("User has decided to skip the update.");
					return;
				}
				if (userInterfaceModalResult == UserInterfaceModalResult.Cancel)
				{
					Logger.AddInternalState("Do not update now, but prompt for update on next activation.");
					return;
				}
			}
			this.InstallApplication(ref subState, actDesc);
			Logger.AddPhaseInformation(Resources.GetString("Upd_Consumed"), new object[]
			{
				identity.ToString(),
				deploySourceUri
			});
			Logger.AddInternalState("Update consumed.");
		}

		// Token: 0x06000397 RID: 919 RVA: 0x0000AA14 File Offset: 0x00008C14
		private bool InstallApplication(ref SubscriptionState subState, ActivationDescription actDesc)
		{
			bool flag = false;
			Logger.AddMethodCall("InstallApplication called.");
			Logger.AddPhaseInformation(Resources.GetString("PhaseLog_InstallApplication"));
			this._subStore.CheckDeploymentSubscriptionState(subState, actDesc.DeployManifest);
			long num;
			using (this._subStore.AcquireReferenceTransaction(out num))
			{
				TempDirectory tempDirectory = null;
				try
				{
					flag = this.DownloadApplication(subState, actDesc, num, out tempDirectory);
					actDesc.CommitDeploy = true;
					actDesc.IsConfirmed = true;
					actDesc.TimeStamp = DateTime.UtcNow;
					if (actDesc.CommitApp)
					{
						this.SetMarkOfTheWebIfNeeded(actDesc);
					}
					Logger.AddPhaseInformation(Resources.GetString("PhaseLog_CommitApplication"));
					this._subStore.CommitApplication(ref subState, actDesc);
				}
				finally
				{
					if (tempDirectory != null)
					{
						tempDirectory.Dispose();
					}
				}
			}
			return flag;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x0000AAE4 File Offset: 0x00008CE4
		private void SetMarkOfTheWebIfNeeded(CommitApplicationParams p)
		{
			string deployManifestPath = p.DeployManifestPath;
			string absoluteUri = p.AppSourceUri.AbsoluteUri;
			if (p.AppPayloadPath == null)
			{
				return;
			}
			string text = Path.Combine(p.AppPayloadPath, p.AppId.ApplicationIdentity.Name);
			if (global::System.IO.File.Exists(text) && PlatformDetector.IsWin8orLater() && p.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted() && AssemblyManifest.AnalyzeManifestCertificate(deployManifestPath) != AssemblyManifest.CertificateStatus.TrustedPublisher && Utilities.IsAppRepCheckRequired(absoluteUri))
			{
				Utilities.SetMarkOfTheWeb(text);
			}
		}

		// Token: 0x06000399 RID: 921 RVA: 0x0000AB68 File Offset: 0x00008D68
		private bool DownloadApplication(SubscriptionState subState, ActivationDescription actDesc, long transactionId, out TempDirectory downloadTemp)
		{
			bool flag = false;
			Logger.AddMethodCall("DownloadApplication called.");
			downloadTemp = this._subStore.AcquireTempDirectory();
			Logger.AddInternalState("Start processing application manifest.");
			Uri uri;
			string text;
			AssemblyManifest assemblyManifest = DownloadManager.DownloadApplicationManifest(actDesc.DeployManifest, downloadTemp.Path, actDesc.DeploySourceUri, out uri, out text);
			AssemblyManifest.ReValidateManifestSignatures(actDesc.DeployManifest, assemblyManifest);
			if (assemblyManifest.EntryPoints[0].HostInBrowser)
			{
				throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_HostInBrowserAppNotSupported"));
			}
			if (assemblyManifest.EntryPoints[0].CustomHostSpecified)
			{
				throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_CustomHostSpecifiedAppNotSupported"));
			}
			if (assemblyManifest.EntryPoints[0].CustomUX && (actDesc.ActType == ActivationType.InstallViaDotApplication || actDesc.ActType == ActivationType.InstallViaFileAssociation || actDesc.ActType == ActivationType.InstallViaShortcut || actDesc.ActType == ActivationType.None))
			{
				throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_CustomUXAppNotSupported"));
			}
			Logger.AddPhaseInformation(Resources.GetString("PhaseLog_ProcessingApplicationManifestComplete"));
			Logger.AddInternalState("Processing of application manifest has successfully completed.");
			actDesc.SetApplicationManifest(assemblyManifest, uri, text);
			Logger.SetApplicationManifest(assemblyManifest);
			this._subStore.CheckCustomUXFlag(subState, actDesc.AppManifest);
			actDesc.AppId = new DefinitionAppId(actDesc.ToAppCodebase(), new DefinitionIdentity[]
			{
				actDesc.DeployManifest.Identity,
				actDesc.AppManifest.Identity
			});
			Logger.AddInternalState("Start request of trust and detection of platform.");
			if (assemblyManifest.EntryPoints[0].CustomUX)
			{
				Logger.AddInternalState("This is a CustomUX application. Calling PersistTrustWithoutEvaluation.");
				actDesc.Trust = ApplicationTrust.PersistTrustWithoutEvaluation(actDesc.ToActivationContext());
			}
			else
			{
				this._ui.Hide();
				if (this._ui.SplashCancelled())
				{
					throw new DownloadCancelledException();
				}
				if (subState.IsInstalled && !string.Equals(subState.EffectiveCertificatePublicKeyToken, actDesc.EffectiveCertificatePublicKeyToken, StringComparison.Ordinal))
				{
					Logger.AddInternalState("EffectiveCertificatePublicKeyToken has changed between versions: subState.EffectiveCertificatePublicKeyToken=" + subState.EffectiveCertificatePublicKeyToken + ",actDesc.EffectiveCertificatePublicKeyToken=" + actDesc.EffectiveCertificatePublicKeyToken);
					Logger.AddInternalState("Removing the cached trust decision for CurrentBind.");
					ApplicationTrust.RemoveCachedTrust(subState.CurrentBind);
				}
				try
				{
					actDesc.Trust = ApplicationTrust.RequestTrust(subState, actDesc.DeployManifest.Deployment.Install, actDesc.IsUpdate, actDesc.ToActivationContext());
				}
				catch (Exception ex)
				{
					Logger.AddErrorInformation(Resources.GetString("Ex_DetermineTrustFailed"), ex);
					if (!(ex is TrustNotGrantedException))
					{
						try
						{
							PlatformDetector.VerifyPlatformDependencies(actDesc.AppManifest, actDesc.DeployManifest, downloadTemp.Path);
						}
						catch (Exception ex2)
						{
							if (ex2 is DependentPlatformMissingException)
							{
								throw new DeploymentException(ExceptionTypes.TrustFailDependentPlatform, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("ErrorMessage_TrustFailDependentPlatformMissing"), new object[] { ex2.Message }), ex);
							}
						}
					}
					throw;
				}
			}
			this._fullTrust = actDesc.Trust.DefaultGrantSet.PermissionSet.IsUnrestricted();
			Logger.AddInternalState("_fullTrust = " + this._fullTrust.ToString());
			if (!this._fullTrust && actDesc.AppManifest.FileAssociations.Length != 0)
			{
				throw new DeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_FileExtensionNotSupported"));
			}
			PlatformDetector.VerifyPlatformDependencies(actDesc.AppManifest, actDesc.DeployManifest, downloadTemp.Path);
			Logger.AddPhaseInformation(Resources.GetString("PhaseLog_PlatformDetectAndTrustGrantComplete"));
			Logger.AddInternalState("Request of trust and detection of platform is complete.");
			Logger.AddInternalState("Start downloading  and verifying dependencies.");
			if (!this._subStore.CheckAndReferenceApplication(subState, actDesc.AppId, transactionId))
			{
				flag = true;
				Description effectiveDescription = actDesc.EffectiveDescription;
				UserInterfaceInfo userInterfaceInfo = new UserInterfaceInfo();
				userInterfaceInfo.productName = effectiveDescription.Product;
				if (actDesc.IsUpdate)
				{
					if (actDesc.IsRequiredUpdate)
					{
						userInterfaceInfo.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleRequiredUpdate"), new object[] { userInterfaceInfo.productName });
					}
					else
					{
						userInterfaceInfo.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleUpdate"), new object[] { userInterfaceInfo.productName });
					}
				}
				else if (!actDesc.DeployManifest.Deployment.Install)
				{
					userInterfaceInfo.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleDownload"), new object[] { userInterfaceInfo.productName });
				}
				else
				{
					userInterfaceInfo.formTitle = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UI_ProgressTitleInstall"), new object[] { userInterfaceInfo.productName });
				}
				userInterfaceInfo.supportUrl = effectiveDescription.SupportUrl;
				userInterfaceInfo.sourceSite = UserInterface.GetDisplaySite(actDesc.DeploySourceUri);
				if (assemblyManifest.Description != null && assemblyManifest.Description.IconFileFS != null)
				{
					userInterfaceInfo.iconFilePath = Path.Combine(downloadTemp.Path, assemblyManifest.Description.IconFileFS);
				}
				ProgressPiece progressPiece = this._ui.ShowProgress(userInterfaceInfo);
				DownloadOptions downloadOptions = null;
				bool flag2 = !actDesc.DeployManifest.Deployment.Install;
				if (!this._fullTrust && flag2)
				{
					downloadOptions = new DownloadOptions();
					downloadOptions.EnforceSizeLimit = true;
					downloadOptions.SizeLimit = this._subStore.GetSizeLimitInBytesForSemiTrustApps();
					downloadOptions.Size = actDesc.DeployManifest.SizeInBytes + actDesc.AppManifest.SizeInBytes;
				}
				DownloadManager.DownloadDependencies(subState, actDesc.DeployManifest, actDesc.AppManifest, actDesc.AppSourceUri, downloadTemp.Path, null, progressPiece, downloadOptions);
				Logger.AddPhaseInformation(Resources.GetString("PhaseLog_DownloadDependenciesComplete"));
				actDesc.CommitApp = true;
				actDesc.AppPayloadPath = downloadTemp.Path;
				actDesc.AppGroup = null;
			}
			return flag;
		}

		// Token: 0x0600039A RID: 922 RVA: 0x0000B0DC File Offset: 0x000092DC
		private static bool SkipUpdate(SubscriptionState subState, DefinitionIdentity targetIdentity)
		{
			Logger.AddMethodCall("SkipUpdate called.");
			if (subState.UpdateSkippedDeployment != null && targetIdentity != null && subState.UpdateSkippedDeployment.Equals(targetIdentity) && subState.UpdateSkipTime > DateTime.UtcNow)
			{
				Logger.AddInternalState("Skipped Update. UpdateSkipTime was " + subState.UpdateSkipTime.ToString());
				return true;
			}
			Logger.AddInternalState("Update is not skipped.");
			return false;
		}

		// Token: 0x0600039B RID: 923 RVA: 0x0000B148 File Offset: 0x00009348
		private Exception GetInnerMostException(Exception exception)
		{
			if (exception.InnerException != null)
			{
				return this.GetInnerMostException(exception.InnerException);
			}
			return exception;
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0000B160 File Offset: 0x00009360
		private bool IsWebExceptionInExceptionStack(Exception exception)
		{
			return exception != null && (exception is WebException || this.IsWebExceptionInExceptionStack(exception.InnerException));
		}

		// Token: 0x0600039D RID: 925 RVA: 0x0000B180 File Offset: 0x00009380
		private int CheckActivationInProgress(string activationUrl)
		{
			object syncRoot = ApplicationActivator._activationsInProgress.SyncRoot;
			int count;
			lock (syncRoot)
			{
				if (ApplicationActivator._activationsInProgress.Contains(activationUrl))
				{
					ApplicationActivator applicationActivator = (ApplicationActivator)ApplicationActivator._activationsInProgress[activationUrl];
					applicationActivator.ActivateUI();
					this._remActivationInProgressEntry = false;
					throw new DeploymentException(ExceptionTypes.ActivationInProgress, Resources.GetString("Ex_ActivationInProgressException"));
				}
				ApplicationActivator._activationsInProgress.Add(activationUrl, this);
				this._remActivationInProgressEntry = true;
				count = ApplicationActivator._activationsInProgress.Count;
			}
			return count;
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0000B21C File Offset: 0x0000941C
		private void RemoveActivationInProgressEntry(string activationUrl)
		{
			if (!this._remActivationInProgressEntry)
			{
				return;
			}
			if (activationUrl == null)
			{
				return;
			}
			object syncRoot = ApplicationActivator._activationsInProgress.SyncRoot;
			lock (syncRoot)
			{
				ApplicationActivator._activationsInProgress.Remove(activationUrl);
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0000B274 File Offset: 0x00009474
		private void ActivateUI()
		{
			if (this._ui == null)
			{
				return;
			}
			this._ui.Activate();
		}

		// Token: 0x0400033C RID: 828
		private static Hashtable _activationsInProgress = new Hashtable();

		// Token: 0x0400033D RID: 829
		private bool _remActivationInProgressEntry;

		// Token: 0x0400033E RID: 830
		private SubscriptionStore _subStore;

		// Token: 0x0400033F RID: 831
		private UserInterface _ui;

		// Token: 0x04000340 RID: 832
		private bool _fullTrust;

		// Token: 0x04000341 RID: 833
		private const int _liveActivationLimitUINotVisible = 0;

		// Token: 0x04000342 RID: 834
		private const int _liveActivationLimitUIVisible = 1;

		// Token: 0x04000343 RID: 835
		private static int _liveActivationLimitUIStatus = 0;

		// Token: 0x04000344 RID: 836
		private const int ActivateArgumentCount = 5;

		// Token: 0x02000197 RID: 407
		private class BrowserSettings
		{
			// Token: 0x060008C6 RID: 2246 RVA: 0x000271C0 File Offset: 0x000253C0
			public void Validate(string manifestPath)
			{
				Logger.AddMethodCall("BrowserSettings.Validate(" + manifestPath + ") called.");
				AssemblyManifest.CertificateStatus certificateStatus = AssemblyManifest.AnalyzeManifestCertificate(manifestPath);
				if (certificateStatus == AssemblyManifest.CertificateStatus.TrustedPublisher || certificateStatus == AssemblyManifest.CertificateStatus.AuthenticodedNotInTrustedList)
				{
					if (this.ManagedSignedFlag != ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_ALLOW && this.ManagedSignedFlag != ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_QUERY)
					{
						throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_SignedManifestDisallow"));
					}
				}
				else if (this.ManagedUnSignedFlag != ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_ALLOW && this.ManagedUnSignedFlag != ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_QUERY)
				{
					throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_UnSignedManifestDisallow"));
				}
				Logger.AddInternalState("Browser settings allow activation. ManagedSignedFlag=" + this.ManagedSignedFlag.ToString() + ",ManagedUnSignedFlag=" + this.ManagedUnSignedFlag.ToString());
			}

			// Token: 0x060008C7 RID: 2247 RVA: 0x0002726C File Offset: 0x0002546C
			public static ApplicationActivator.BrowserSettings.ManagedFlags GetManagedFlagValue(int policyValue)
			{
				switch (policyValue)
				{
				case 0:
					return ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_ALLOW;
				case 1:
					return ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_QUERY;
				case 3:
					return ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_DISALLOW;
				}
				return ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_DISALLOW;
			}

			// Token: 0x04000781 RID: 1921
			public ApplicationActivator.BrowserSettings.ManagedFlags ManagedSignedFlag = ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_DISALLOW;

			// Token: 0x04000782 RID: 1922
			public ApplicationActivator.BrowserSettings.ManagedFlags ManagedUnSignedFlag = ApplicationActivator.BrowserSettings.ManagedFlags.URLPOLICY_DISALLOW;

			// Token: 0x02000220 RID: 544
			public enum ManagedFlags
			{
				// Token: 0x04000954 RID: 2388
				URLPOLICY_ALLOW,
				// Token: 0x04000955 RID: 2389
				URLPOLICY_QUERY,
				// Token: 0x04000956 RID: 2390
				URLPOLICY_DISALLOW = 3
			}
		}
	}
}

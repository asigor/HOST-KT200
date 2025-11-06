using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Data.Sql;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Data.SqlClient
{
	/// <summary>The <see cref="T:System.Data.SqlClient.SqlDependency" /> object represents a query notification dependency between an application and an instance of SQL Server. An application can create a <see cref="T:System.Data.SqlClient.SqlDependency" /> object and register to receive notifications via the <see cref="T:System.Data.SqlClient.OnChangeEventHandler" /> event handler.</summary>
	// Token: 0x020001C3 RID: 451
	public sealed class SqlDependency
	{
		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06001CAC RID: 7340 RVA: 0x000CB55C File Offset: 0x000CA95C
		internal int ObjectID
		{
			get
			{
				return this._objectID;
			}
		}

		/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class with the default settings.</summary>
		// Token: 0x06001CAD RID: 7341 RVA: 0x000CB570 File Offset: 0x000CA970
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public SqlDependency()
			: this(null, null, 0)
		{
		}

		/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class and associates it with the <see cref="T:System.Data.SqlClient.SqlCommand" /> parameter.</summary>
		/// <param name="command">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object to associate with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object. The constructor will set up a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object and bind it to the command.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is NULL.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property, and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency.</exception>
		// Token: 0x06001CAE RID: 7342 RVA: 0x000CB588 File Offset: 0x000CA988
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public SqlDependency(SqlCommand command)
			: this(command, null, 0)
		{
		}

		/// <summary>Creates a new instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class, associates it with the <see cref="T:System.Data.SqlClient.SqlCommand" /> parameter, and specifies notification options and a time-out value.</summary>
		/// <param name="command">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object to associate with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object. The constructor sets up a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object and bind it to the command.</param>
		/// <param name="options">The notification request options to be used by this dependency. <see langword="null" /> to use the default service.</param>
		/// <param name="timeout">The time-out for this notification in seconds. The default is 0, indicating that the server's time-out should be used.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is NULL.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The time-out value is less than zero.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency.  
		///  An attempt was made to create a SqlDependency instance from within SQLCLR.</exception>
		// Token: 0x06001CAF RID: 7343 RVA: 0x000CB5A0 File Offset: 0x000CA9A0
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public SqlDependency(SqlCommand command, string options, int timeout)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency|DEP> %d#, options: '%ls', timeout: '%d'", this.ObjectID, options, timeout);
			try
			{
				if (InOutOfProcHelper.InProc)
				{
					throw SQL.SqlDepCannotBeCreatedInProc();
				}
				if (timeout < 0)
				{
					throw SQL.InvalidSqlDependencyTimeout("timeout");
				}
				this._timeout = timeout;
				if (options != null)
				{
					this._options = options;
				}
				this.AddCommandInternal(command);
				SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddDependencyEntry(this);
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		/// <summary>Gets a value that indicates whether one of the result sets associated with the dependency has changed.</summary>
		/// <returns>A Boolean value indicating whether one of the result sets has changed.</returns>
		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06001CB0 RID: 7344 RVA: 0x000CB690 File Offset: 0x000CAA90
		[ResDescription("SqlDependency_HasChanges")]
		[ResCategory("DataCategory_Data")]
		public bool HasChanges
		{
			get
			{
				return this._dependencyFired;
			}
		}

		/// <summary>Gets a value that uniquely identifies this instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class.</summary>
		/// <returns>A string representation of a GUID that is generated for each instance of the <see cref="T:System.Data.SqlClient.SqlDependency" /> class.</returns>
		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06001CB1 RID: 7345 RVA: 0x000CB6A4 File Offset: 0x000CAAA4
		[ResCategory("DataCategory_Data")]
		[ResDescription("SqlDependency_Id")]
		public string Id
		{
			get
			{
				return this._id;
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06001CB2 RID: 7346 RVA: 0x000CB6B8 File Offset: 0x000CAAB8
		internal static string AppDomainKey
		{
			get
			{
				return SqlDependency._appDomainKey;
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06001CB3 RID: 7347 RVA: 0x000CB6CC File Offset: 0x000CAACC
		internal DateTime ExpirationTime
		{
			get
			{
				return this._expirationTime;
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06001CB4 RID: 7348 RVA: 0x000CB6E0 File Offset: 0x000CAAE0
		internal string Options
		{
			get
			{
				string text = null;
				if (this._options != null)
				{
					text = this._options;
				}
				return text;
			}
		}

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06001CB5 RID: 7349 RVA: 0x000CB700 File Offset: 0x000CAB00
		internal static SqlDependencyProcessDispatcher ProcessDispatcher
		{
			get
			{
				return SqlDependency._processDispatcher;
			}
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06001CB6 RID: 7350 RVA: 0x000CB714 File Offset: 0x000CAB14
		internal int Timeout
		{
			get
			{
				return this._timeout;
			}
		}

		/// <summary>Occurs when a notification is received for any of the commands associated with this <see cref="T:System.Data.SqlClient.SqlDependency" /> object.</summary>
		// Token: 0x14000025 RID: 37
		// (add) Token: 0x06001CB7 RID: 7351 RVA: 0x000CB728 File Offset: 0x000CAB28
		// (remove) Token: 0x06001CB8 RID: 7352 RVA: 0x000CB808 File Offset: 0x000CAC08
		[ResDescription("SqlDependency_OnChange")]
		[ResCategory("DataCategory_Data")]
		public event OnChangeEventHandler OnChange
		{
			add
			{
				IntPtr intPtr;
				Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.OnChange-Add|DEP> %d#", this.ObjectID);
				try
				{
					if (value != null)
					{
						SqlNotificationEventArgs sqlNotificationEventArgs = null;
						object eventHandlerLock = this._eventHandlerLock;
						lock (eventHandlerLock)
						{
							if (this._dependencyFired)
							{
								Bid.NotificationsTrace("<sc.SqlDependency.OnChange-Add|DEP> Dependency already fired, firing new event.\n");
								sqlNotificationEventArgs = new SqlNotificationEventArgs(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
							}
							else
							{
								Bid.NotificationsTrace("<sc.SqlDependency.OnChange-Add|DEP> Dependency has not fired, adding new event.\n");
								SqlDependency.EventContextPair eventContextPair = new SqlDependency.EventContextPair(value, this);
								if (this._eventList.Contains(eventContextPair))
								{
									throw SQL.SqlDependencyEventNoDuplicate();
								}
								this._eventList.Add(eventContextPair);
							}
						}
						if (sqlNotificationEventArgs != null)
						{
							value(this, sqlNotificationEventArgs);
						}
					}
				}
				finally
				{
					Bid.ScopeLeave(ref intPtr);
				}
			}
			remove
			{
				IntPtr intPtr;
				Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.OnChange-Remove|DEP> %d#", this.ObjectID);
				try
				{
					if (value != null)
					{
						SqlDependency.EventContextPair eventContextPair = new SqlDependency.EventContextPair(value, this);
						object eventHandlerLock = this._eventHandlerLock;
						lock (eventHandlerLock)
						{
							int num = this._eventList.IndexOf(eventContextPair);
							if (0 <= num)
							{
								this._eventList.RemoveAt(num);
							}
						}
					}
				}
				finally
				{
					Bid.ScopeLeave(ref intPtr);
				}
			}
		}

		/// <summary>Associates a <see cref="T:System.Data.SqlClient.SqlCommand" /> object with this <see cref="T:System.Data.SqlClient.SqlDependency" /> instance.</summary>
		/// <param name="command">A <see cref="T:System.Data.SqlClient.SqlCommand" /> object containing a statement that is valid for notifications.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="command" /> parameter is null.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.SqlClient.SqlCommand" /> object already has a <see cref="T:System.Data.Sql.SqlNotificationRequest" /> object assigned to its <see cref="P:System.Data.SqlClient.SqlCommand.Notification" /> property, and that <see cref="T:System.Data.Sql.SqlNotificationRequest" /> is not associated with this dependency.</exception>
		// Token: 0x06001CB9 RID: 7353 RVA: 0x000CB8AC File Offset: 0x000CACAC
		[ResCategory("DataCategory_Data")]
		[ResDescription("SqlDependency_AddCommandDependency")]
		public void AddCommandDependency(SqlCommand command)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.AddCommandDependency|DEP> %d#", this.ObjectID);
			try
			{
				if (command == null)
				{
					throw ADP.ArgumentNull("command");
				}
				this.AddCommandInternal(command);
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x000CB908 File Offset: 0x000CAD08
		[ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
		private static ObjectHandle CreateProcessDispatcher(_AppDomain masterDomain)
		{
			return masterDomain.CreateInstance(SqlDependency._assemblyName, SqlDependency._typeName);
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x000CB928 File Offset: 0x000CAD28
		private static void ObtainProcessDispatcher()
		{
			byte[] data = SNINativeMethodWrapper.GetData();
			if (data != null)
			{
				Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> nativeStorage not null, obtaining existing dispatcher AppDomain and ProcessDispatcher.\n");
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream(data);
				SqlDependency._processDispatcher = SqlDependency.GetDeserializedObject(binaryFormatter, memoryStream);
				Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> processDispatcher obtained, ID: %d\n", SqlDependency._processDispatcher.ObjectID);
				return;
			}
			Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP> nativeStorage null, obtaining dispatcher AppDomain and creating ProcessDispatcher.\n");
			_AppDomain defaultAppDomain = SNINativeMethodWrapper.GetDefaultAppDomain();
			if (defaultAppDomain == null)
			{
				Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - unable to obtain default AppDomain!\n");
				throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyProcessDispatcherFailureAppDomain);
			}
			ObjectHandle objectHandle = SqlDependency.CreateProcessDispatcher(defaultAppDomain);
			if (objectHandle == null)
			{
				Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - AppDomain.CreateInstance returned null!\n");
				throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyProcessDispatcherFailureCreateInstance);
			}
			SqlDependencyProcessDispatcher sqlDependencyProcessDispatcher = (SqlDependencyProcessDispatcher)objectHandle.Unwrap();
			if (sqlDependencyProcessDispatcher != null)
			{
				SqlDependency._processDispatcher = sqlDependencyProcessDispatcher.SingletonProcessDispatcher;
				ObjRef objRef = SqlDependency.GetObjRef(SqlDependency._processDispatcher);
				BinaryFormatter binaryFormatter2 = new BinaryFormatter();
				MemoryStream memoryStream2 = new MemoryStream();
				SqlDependency.GetSerializedObject(objRef, binaryFormatter2, memoryStream2);
				SNINativeMethodWrapper.SetData(memoryStream2.GetBuffer());
				return;
			}
			Bid.NotificationsTrace("<sc.SqlDependency.ObtainProcessDispatcher|DEP|ERR> ERROR - ObjectHandle.Unwrap returned null!\n");
			throw ADP.InternalError(ADP.InternalErrorCode.SqlDependencyObtainProcessDispatcherFailureObjectHandle);
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x000CBA20 File Offset: 0x000CAE20
		[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		private static ObjRef GetObjRef(SqlDependencyProcessDispatcher _processDispatcher)
		{
			return RemotingServices.Marshal(_processDispatcher);
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x000CBA34 File Offset: 0x000CAE34
		[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
		private static void GetSerializedObject(ObjRef objRef, BinaryFormatter formatter, MemoryStream stream)
		{
			formatter.Serialize(stream, objRef);
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x000CBA4C File Offset: 0x000CAE4C
		[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
		private static SqlDependencyProcessDispatcher GetDeserializedObject(BinaryFormatter formatter, MemoryStream stream)
		{
			object obj = formatter.Deserialize(stream);
			return (SqlDependencyProcessDispatcher)obj;
		}

		/// <summary>Starts the listener for receiving dependency change notifications from the instance of SQL Server specified by the connection string.</summary>
		/// <param name="connectionString">The connection string for the instance of SQL Server from which to obtain change notifications.</param>
		/// <returns>
		///   <see langword="true" /> if the listener initialized successfully; <see langword="false" /> if a compatible listener already exists.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <paramref name="connectionString" /> parameter is the same as a previous call to this method, but the parameters are different.  
		///  The method was called from within the CLR.</exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
		/// <exception cref="T:System.Data.SqlClient.SqlException">A subsequent call to the method has been made with an equivalent <paramref name="connectionString" /> parameter with a different user, or a user that does not default to the same schema.  
		///  Also, any underlying SqlClient exceptions.</exception>
		// Token: 0x06001CBF RID: 7359 RVA: 0x000CBA68 File Offset: 0x000CAE68
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Start(string connectionString)
		{
			return SqlDependency.Start(connectionString, null, true);
		}

		/// <summary>Starts the listener for receiving dependency change notifications from the instance of SQL Server specified by the connection string using the specified SQL Server Service Broker queue.</summary>
		/// <param name="connectionString">The connection string for the instance of SQL Server from which to obtain change notifications.</param>
		/// <param name="queue">An existing SQL Server Service Broker queue to be used. If <see langword="null" />, the default queue is used.</param>
		/// <returns>
		///   <see langword="true" /> if the listener initialized successfully; <see langword="false" /> if a compatible listener already exists.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
		/// <exception cref="T:System.InvalidOperationException">The <paramref name="connectionString" /> parameter is the same as a previous call to this method, but the parameters are different.  
		///  The method was called from within the CLR.</exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
		/// <exception cref="T:System.Data.SqlClient.SqlException">A subsequent call to the method has been made with an equivalent <paramref name="connectionString" /> parameter but a different user, or a user that does not default to the same schema.  
		///  Also, any underlying SqlClient exceptions.</exception>
		// Token: 0x06001CC0 RID: 7360 RVA: 0x000CBA80 File Offset: 0x000CAE80
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Start(string connectionString, string queue)
		{
			return SqlDependency.Start(connectionString, queue, false);
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x000CBA98 File Offset: 0x000CAE98
		internal static bool Start(string connectionString, string queue, bool useDefaults)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.Start|DEP> AppDomainKey: '%ls', queue: '%ls'", SqlDependency.AppDomainKey, queue);
			bool flag5;
			try
			{
				if (InOutOfProcHelper.InProc)
				{
					throw SQL.SqlDepCannotBeCreatedInProc();
				}
				if (ADP.IsEmpty(connectionString))
				{
					if (connectionString == null)
					{
						throw ADP.ArgumentNull("connectionString");
					}
					throw ADP.Argument("connectionString");
				}
				else
				{
					if (!useDefaults && ADP.IsEmpty(queue))
					{
						useDefaults = true;
						queue = null;
					}
					SqlConnectionString sqlConnectionString = new SqlConnectionString(connectionString);
					sqlConnectionString.DemandPermission();
					if (sqlConnectionString.LocalDBInstance != null)
					{
						LocalDBAPI.DemandLocalDBPermissions();
					}
					bool flag = false;
					bool flag2 = false;
					object startStopLock = SqlDependency._startStopLock;
					lock (startStopLock)
					{
						try
						{
							if (SqlDependency._processDispatcher == null)
							{
								SqlDependency.ObtainProcessDispatcher();
							}
							if (useDefaults)
							{
								string text = null;
								DbConnectionPoolIdentity dbConnectionPoolIdentity = null;
								string text2 = null;
								string text3 = null;
								string text4 = null;
								bool flag4 = false;
								RuntimeHelpers.PrepareConstrainedRegions();
								try
								{
									flag2 = SqlDependency._processDispatcher.StartWithDefault(connectionString, out text, out dbConnectionPoolIdentity, out text2, out text3, ref text4, SqlDependency._appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance, out flag, out flag4);
									Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP> Start (defaults) returned: '%d', with service: '%ls', server: '%ls', database: '%ls'\n", flag2, text4, text, text3);
									goto IL_0163;
								}
								finally
								{
									if (flag4 && !flag)
									{
										SqlDependency.IdentityUserNamePair identityUserNamePair = new SqlDependency.IdentityUserNamePair(dbConnectionPoolIdentity, text2);
										SqlDependency.DatabaseServicePair databaseServicePair = new SqlDependency.DatabaseServicePair(text3, text4);
										if (!SqlDependency.AddToServerUserHash(text, identityUserNamePair, databaseServicePair))
										{
											try
											{
												SqlDependency.Stop(connectionString, queue, useDefaults, true);
											}
											catch (Exception ex)
											{
												if (!ADP.IsCatchableExceptionType(ex))
												{
													throw;
												}
												ADP.TraceExceptionWithoutRethrow(ex);
												Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP|ERR> Exception occurred from Stop() after duplicate was found on Start().\n");
											}
											throw SQL.SqlDependencyDuplicateStart();
										}
									}
								}
							}
							flag2 = SqlDependency._processDispatcher.Start(connectionString, queue, SqlDependency._appDomainKey, SqlDependencyPerAppDomainDispatcher.SingletonInstance);
							Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP> Start (user provided queue) returned: '%d'\n", flag2);
							IL_0163:;
						}
						catch (Exception ex2)
						{
							if (!ADP.IsCatchableExceptionType(ex2))
							{
								throw;
							}
							ADP.TraceExceptionWithoutRethrow(ex2);
							Bid.NotificationsTrace("<sc.SqlDependency.Start|DEP|ERR> Exception occurred from _processDispatcher.Start(...), calling Invalidate(...).\n");
							throw;
						}
					}
					flag5 = flag2;
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return flag5;
		}

		/// <summary>Stops a listener for a connection specified in a previous <see cref="Overload:System.Data.SqlClient.SqlDependency.Start" /> call.</summary>
		/// <param name="connectionString">Connection string for the instance of SQL Server that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String)" /> call.</param>
		/// <returns>
		///   <see langword="true" /> if the listener was completely stopped; <see langword="false" /> if the <see cref="T:System.AppDomain" /> was unbound from the listener, but there are is at least one other <see cref="T:System.AppDomain" /> using the same listener.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
		/// <exception cref="T:System.InvalidOperationException">The method was called from within SQLCLR.</exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
		/// <exception cref="T:System.Data.SqlClient.SqlException">An underlying SqlClient exception occurred.</exception>
		// Token: 0x06001CC2 RID: 7362 RVA: 0x000CBCC4 File Offset: 0x000CB0C4
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Stop(string connectionString)
		{
			return SqlDependency.Stop(connectionString, null, true, false);
		}

		/// <summary>Stops a listener for a connection specified in a previous <see cref="Overload:System.Data.SqlClient.SqlDependency.Start" /> call.</summary>
		/// <param name="connectionString">Connection string for the instance of SQL Server that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String,System.String)" /> call.</param>
		/// <param name="queue">The SQL Server Service Broker queue that was used in a previous <see cref="M:System.Data.SqlClient.SqlDependency.Start(System.String,System.String)" /> call.</param>
		/// <returns>
		///   <see langword="true" /> if the listener was completely stopped; <see langword="false" /> if the <see cref="T:System.AppDomain" /> was unbound from the listener, but there is at least one other <see cref="T:System.AppDomain" /> using the same listener.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="connectionString" /> parameter is NULL.</exception>
		/// <exception cref="T:System.InvalidOperationException">The method was called from within SQLCLR.</exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required <see cref="T:System.Data.SqlClient.SqlClientPermission" /> code access security (CAS) permission.</exception>
		/// <exception cref="T:System.Data.SqlClient.SqlException">And underlying SqlClient exception occurred.</exception>
		// Token: 0x06001CC3 RID: 7363 RVA: 0x000CBCDC File Offset: 0x000CB0DC
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Stop(string connectionString, string queue)
		{
			return SqlDependency.Stop(connectionString, queue, false, false);
		}

		// Token: 0x06001CC4 RID: 7364 RVA: 0x000CBCF4 File Offset: 0x000CB0F4
		internal static bool Stop(string connectionString, string queue, bool useDefaults, bool startFailed)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.Stop|DEP> AppDomainKey: '%ls', queue: '%ls'", SqlDependency.AppDomainKey, queue);
			bool flag5;
			try
			{
				if (InOutOfProcHelper.InProc)
				{
					throw SQL.SqlDepCannotBeCreatedInProc();
				}
				if (ADP.IsEmpty(connectionString))
				{
					if (connectionString == null)
					{
						throw ADP.ArgumentNull("connectionString");
					}
					throw ADP.Argument("connectionString");
				}
				else
				{
					if (!useDefaults && ADP.IsEmpty(queue))
					{
						useDefaults = true;
						queue = null;
					}
					SqlConnectionString sqlConnectionString = new SqlConnectionString(connectionString);
					sqlConnectionString.DemandPermission();
					if (sqlConnectionString.LocalDBInstance != null)
					{
						LocalDBAPI.DemandLocalDBPermissions();
					}
					bool flag = false;
					object startStopLock = SqlDependency._startStopLock;
					lock (startStopLock)
					{
						if (SqlDependency._processDispatcher != null)
						{
							try
							{
								string text = null;
								DbConnectionPoolIdentity dbConnectionPoolIdentity = null;
								string text2 = null;
								string text3 = null;
								string text4 = null;
								if (useDefaults)
								{
									bool flag3 = false;
									RuntimeHelpers.PrepareConstrainedRegions();
									try
									{
										flag = SqlDependency._processDispatcher.Stop(connectionString, out text, out dbConnectionPoolIdentity, out text2, out text3, ref text4, SqlDependency._appDomainKey, out flag3);
										goto IL_010A;
									}
									finally
									{
										if (flag3 && !startFailed)
										{
											SqlDependency.IdentityUserNamePair identityUserNamePair = new SqlDependency.IdentityUserNamePair(dbConnectionPoolIdentity, text2);
											SqlDependency.DatabaseServicePair databaseServicePair = new SqlDependency.DatabaseServicePair(text3, text4);
											SqlDependency.RemoveFromServerUserHash(text, identityUserNamePair, databaseServicePair);
										}
									}
								}
								bool flag4 = false;
								flag = SqlDependency._processDispatcher.Stop(connectionString, out text, out dbConnectionPoolIdentity, out text2, out text3, ref queue, SqlDependency._appDomainKey, out flag4);
								IL_010A:;
							}
							catch (Exception ex)
							{
								if (!ADP.IsCatchableExceptionType(ex))
								{
									throw;
								}
								ADP.TraceExceptionWithoutRethrow(ex);
							}
						}
					}
					flag5 = flag;
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return flag5;
		}

		// Token: 0x06001CC5 RID: 7365 RVA: 0x000CBEA0 File Offset: 0x000CB2A0
		private static bool AddToServerUserHash(string server, SqlDependency.IdentityUserNamePair identityUser, SqlDependency.DatabaseServicePair databaseService)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.AddToServerUserHash|DEP> server: '%ls', database: '%ls', service: '%ls'", server, databaseService.Database, databaseService.Service);
			bool flag3;
			try
			{
				bool flag = false;
				Dictionary<string, Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>> serverUserHash = SqlDependency._serverUserHash;
				lock (serverUserHash)
				{
					Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>> dictionary;
					if (!SqlDependency._serverUserHash.ContainsKey(server))
					{
						Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Hash did not contain server, adding.\n");
						dictionary = new Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>();
						SqlDependency._serverUserHash.Add(server, dictionary);
					}
					else
					{
						dictionary = SqlDependency._serverUserHash[server];
					}
					List<SqlDependency.DatabaseServicePair> list;
					if (!dictionary.ContainsKey(identityUser))
					{
						Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Hash contained server but not user, adding user.\n");
						list = new List<SqlDependency.DatabaseServicePair>();
						dictionary.Add(identityUser, list);
					}
					else
					{
						list = dictionary[identityUser];
					}
					if (!list.Contains(databaseService))
					{
						Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP> Adding database.\n");
						list.Add(databaseService);
						flag = true;
					}
					else
					{
						Bid.NotificationsTrace("<sc.SqlDependency.AddToServerUserHash|DEP|ERR> ERROR - hash already contained server, user, and database - we will throw!.\n");
					}
				}
				flag3 = flag;
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return flag3;
		}

		// Token: 0x06001CC6 RID: 7366 RVA: 0x000CBFB4 File Offset: 0x000CB3B4
		private static void RemoveFromServerUserHash(string server, SqlDependency.IdentityUserNamePair identityUser, SqlDependency.DatabaseServicePair databaseService)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.RemoveFromServerUserHash|DEP> server: '%ls', database: '%ls', service: '%ls'", server, databaseService.Database, databaseService.Service);
			try
			{
				Dictionary<string, Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>> serverUserHash = SqlDependency._serverUserHash;
				lock (serverUserHash)
				{
					if (SqlDependency._serverUserHash.ContainsKey(server))
					{
						Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>> dictionary = SqlDependency._serverUserHash[server];
						if (dictionary.ContainsKey(identityUser))
						{
							List<SqlDependency.DatabaseServicePair> list = dictionary[identityUser];
							int num = list.IndexOf(databaseService);
							if (num >= 0)
							{
								Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> Hash contained server, user, and database - removing database.\n");
								list.RemoveAt(num);
								if (list.Count == 0)
								{
									Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> databaseServiceList count 0, removing the list for this server and user.\n");
									dictionary.Remove(identityUser);
									if (dictionary.Count == 0)
									{
										Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP> identityDatabaseHash count 0, removing the hash for this server.\n");
										SqlDependency._serverUserHash.Remove(server);
									}
								}
							}
							else
							{
								Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash contained server and user but not database!\n");
							}
						}
						else
						{
							Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash contained server but not user!\n");
						}
					}
					else
					{
						Bid.NotificationsTrace("<sc.SqlDependency.RemoveFromServerUserHash|DEP|ERR> ERROR - hash did not contain server!\n");
					}
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x000CC0DC File Offset: 0x000CB4DC
		internal static string GetDefaultComposedOptions(string server, string failoverServer, SqlDependency.IdentityUserNamePair identityUser, string database)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.GetDefaultComposedOptions|DEP> server: '%ls', failoverServer: '%ls', database: '%ls'", server, failoverServer, database);
			string text5;
			try
			{
				Dictionary<string, Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>> serverUserHash = SqlDependency._serverUserHash;
				string text2;
				lock (serverUserHash)
				{
					if (!SqlDependency._serverUserHash.ContainsKey(server))
					{
						if (SqlDependency._serverUserHash.Count == 0)
						{
							Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - no start calls have been made, about to throw.\n");
							throw SQL.SqlDepDefaultOptionsButNoStart();
						}
						if (ADP.IsEmpty(failoverServer) || !SqlDependency._serverUserHash.ContainsKey(failoverServer))
						{
							Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - not listening to this server, about to throw.\n");
							throw SQL.SqlDependencyNoMatchingServerStart();
						}
						Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP> using failover server instead\n");
						server = failoverServer;
					}
					Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>> dictionary = SqlDependency._serverUserHash[server];
					List<SqlDependency.DatabaseServicePair> list = null;
					if (!dictionary.ContainsKey(identityUser))
					{
						if (dictionary.Count > 1)
						{
							Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - not listening for this user, but listening to more than one other user, about to throw.\n");
							throw SQL.SqlDependencyNoMatchingServerStart();
						}
						using (Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>.Enumerator enumerator = dictionary.GetEnumerator())
						{
							if (!enumerator.MoveNext())
							{
								goto IL_00ED;
							}
							KeyValuePair<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>> keyValuePair = enumerator.Current;
							list = keyValuePair.Value;
							goto IL_00ED;
						}
					}
					list = dictionary[identityUser];
					IL_00ED:
					SqlDependency.DatabaseServicePair databaseServicePair = new SqlDependency.DatabaseServicePair(database, null);
					SqlDependency.DatabaseServicePair databaseServicePair2 = null;
					int num = list.IndexOf(databaseServicePair);
					if (num != -1)
					{
						databaseServicePair2 = list[num];
					}
					if (databaseServicePair2 != null)
					{
						database = SqlDependency.FixupServiceOrDatabaseName(databaseServicePair2.Database);
						string text = SqlDependency.FixupServiceOrDatabaseName(databaseServicePair2.Service);
						text2 = "Service=" + text + ";Local Database=" + database;
					}
					else
					{
						if (list.Count != 1)
						{
							Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP|ERR> ERROR - SqlDependency.Start called multiple times for this server/user, but no matching database.\n");
							throw SQL.SqlDependencyNoMatchingServerDatabaseStart();
						}
						object[] array = list.ToArray();
						databaseServicePair2 = (SqlDependency.DatabaseServicePair)array[0];
						string text3 = SqlDependency.FixupServiceOrDatabaseName(databaseServicePair2.Database);
						string text4 = SqlDependency.FixupServiceOrDatabaseName(databaseServicePair2.Service);
						text2 = "Service=" + text4 + ";Local Database=" + text3;
					}
				}
				Bid.NotificationsTrace("<sc.SqlDependency.GetDefaultComposedOptions|DEP> resulting options: '%ls'.\n", text2);
				text5 = text2;
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return text5;
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x000CC2FC File Offset: 0x000CB6FC
		internal void AddToServerList(string server)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.AddToServerList|DEP> %d#, server: '%ls'", this.ObjectID, server);
			try
			{
				List<string> serverList = this._serverList;
				lock (serverList)
				{
					int num = this._serverList.BinarySearch(server, StringComparer.OrdinalIgnoreCase);
					if (0 > num)
					{
						Bid.NotificationsTrace("<sc.SqlDependency.AddToServerList|DEP> Server not present in hashtable, adding server: '%ls'.\n", server);
						num = ~num;
						this._serverList.Insert(num, server);
					}
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		// Token: 0x06001CC9 RID: 7369 RVA: 0x000CC3A8 File Offset: 0x000CB7A8
		internal bool ContainsServer(string server)
		{
			List<string> serverList = this._serverList;
			bool flag2;
			lock (serverList)
			{
				flag2 = this._serverList.Contains(server);
			}
			return flag2;
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x000CC3FC File Offset: 0x000CB7FC
		internal string ComputeHashAndAddToDispatcher(SqlCommand command)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.ComputeHashAndAddToDispatcher|DEP> %d#, SqlCommand: %d#", this.ObjectID, command.ObjectID);
			string text3;
			try
			{
				string text = this.ComputeCommandHash(command.Connection.ConnectionString, command);
				string text2 = SqlDependencyPerAppDomainDispatcher.SingletonInstance.AddCommandEntry(text, this);
				Bid.NotificationsTrace("<sc.SqlDependency.ComputeHashAndAddToDispatcher|DEP> computed id string: '%ls'.\n", text2);
				text3 = text2;
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return text3;
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x000CC478 File Offset: 0x000CB878
		internal void Invalidate(SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.Invalidate|DEP> %d#", this.ObjectID);
			try
			{
				List<SqlDependency.EventContextPair> list = null;
				object eventHandlerLock = this._eventHandlerLock;
				lock (eventHandlerLock)
				{
					if (this._dependencyFired && SqlNotificationInfo.AlreadyChanged != info && SqlNotificationSource.Client != source)
					{
						if (this.ExpirationTime < DateTime.UtcNow)
						{
							Bid.NotificationsTrace("<sc.SqlDependency.Invalidate|DEP> ignore notification received after timeout!");
						}
						else
						{
							Bid.NotificationsTrace("<sc.SqlDependency.Invalidate|DEP|ERR> ERROR - notification received twice - we should never enter this state!");
						}
					}
					else
					{
						this._dependencyFired = true;
						list = this._eventList;
						this._eventList = new List<SqlDependency.EventContextPair>();
					}
				}
				if (list != null)
				{
					Bid.NotificationsTrace("<sc.SqlDependency.Invalidate|DEP> Firing events.\n");
					foreach (SqlDependency.EventContextPair eventContextPair in list)
					{
						eventContextPair.Invoke(new SqlNotificationEventArgs(type, info, source));
					}
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x000CC5A8 File Offset: 0x000CB9A8
		internal void StartTimer(SqlNotificationRequest notificationRequest)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.StartTimer|DEP> %d#", this.ObjectID);
			try
			{
				if (this._expirationTime == DateTime.MaxValue)
				{
					Bid.NotificationsTrace("<sc.SqlDependency.StartTimer|DEP> We've timed out, executing logic.\n");
					int num = 432000;
					if (this._timeout != 0)
					{
						num = this._timeout;
					}
					if (notificationRequest != null && notificationRequest.Timeout < num && notificationRequest.Timeout != 0)
					{
						num = notificationRequest.Timeout;
					}
					this._expirationTime = DateTime.UtcNow.AddSeconds((double)num);
					SqlDependencyPerAppDomainDispatcher.SingletonInstance.StartTimer(this);
				}
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x000CC65C File Offset: 0x000CBA5C
		private void AddCommandInternal(SqlCommand cmd)
		{
			if (cmd != null)
			{
				IntPtr intPtr;
				Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.AddCommandInternal|DEP> %d#, SqlCommand: %d#", this.ObjectID, cmd.ObjectID);
				try
				{
					SqlConnection connection = cmd.Connection;
					if (cmd.Notification != null)
					{
						if (cmd._sqlDep == null || cmd._sqlDep != this)
						{
							Bid.NotificationsTrace("<sc.SqlDependency.AddCommandInternal|DEP|ERR> ERROR - throwing command has existing SqlNotificationRequest exception.\n");
							throw SQL.SqlCommandHasExistingSqlNotificationRequest();
						}
					}
					else
					{
						bool flag = false;
						object eventHandlerLock = this._eventHandlerLock;
						lock (eventHandlerLock)
						{
							if (!this._dependencyFired)
							{
								cmd.Notification = new SqlNotificationRequest();
								cmd.Notification.Timeout = this._timeout;
								if (this._options != null)
								{
									cmd.Notification.Options = this._options;
								}
								cmd._sqlDep = this;
							}
							else if (this._eventList.Count == 0)
							{
								Bid.NotificationsTrace("<sc.SqlDependency.AddCommandInternal|DEP|ERR> ERROR - firing events, though it is unexpected we have events at this point.\n");
								flag = true;
							}
						}
						if (flag)
						{
							this.Invalidate(SqlNotificationType.Subscribe, SqlNotificationInfo.AlreadyChanged, SqlNotificationSource.Client);
						}
					}
				}
				finally
				{
					Bid.ScopeLeave(ref intPtr);
				}
			}
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x000CC784 File Offset: 0x000CBB84
		private string ComputeCommandHash(string connectionString, SqlCommand command)
		{
			IntPtr intPtr;
			Bid.NotificationsScopeEnter(out intPtr, "<sc.SqlDependency.ComputeCommandHash|DEP> %d#, SqlCommand: %d#", this.ObjectID, command.ObjectID);
			string text2;
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("{0};{1}", connectionString, command.CommandText);
				for (int i = 0; i < command.Parameters.Count; i++)
				{
					object value = command.Parameters[i].Value;
					if (value == null || value == DBNull.Value)
					{
						stringBuilder.Append("; NULL");
					}
					else
					{
						Type type = value.GetType();
						if (type == typeof(byte[]))
						{
							stringBuilder.Append(";");
							byte[] array = (byte[])value;
							for (int j = 0; j < array.Length; j++)
							{
								stringBuilder.Append(array[j].ToString("x2", CultureInfo.InvariantCulture));
							}
						}
						else if (type == typeof(char[]))
						{
							stringBuilder.Append((char[])value);
						}
						else if (type == typeof(XmlReader))
						{
							stringBuilder.Append(";");
							stringBuilder.Append(Guid.NewGuid().ToString());
						}
						else
						{
							stringBuilder.Append(";");
							stringBuilder.Append(value.ToString());
						}
					}
				}
				string text = stringBuilder.ToString();
				Bid.NotificationsTrace("<sc.SqlDependency.ComputeCommandHash|DEP> ComputeCommandHash result: '%ls'.\n", text);
				text2 = text;
			}
			finally
			{
				Bid.ScopeLeave(ref intPtr);
			}
			return text2;
		}

		// Token: 0x06001CCF RID: 7375 RVA: 0x000CC924 File Offset: 0x000CBD24
		internal static string FixupServiceOrDatabaseName(string name)
		{
			if (!ADP.IsEmpty(name))
			{
				return "\"" + name.Replace("\"", "\"\"") + "\"";
			}
			return name;
		}

		// Token: 0x04001042 RID: 4162
		private readonly string _id = Guid.NewGuid().ToString() + ";" + SqlDependency._appDomainKey;

		// Token: 0x04001043 RID: 4163
		private string _options;

		// Token: 0x04001044 RID: 4164
		private int _timeout;

		// Token: 0x04001045 RID: 4165
		private bool _dependencyFired;

		// Token: 0x04001046 RID: 4166
		private List<SqlDependency.EventContextPair> _eventList = new List<SqlDependency.EventContextPair>();

		// Token: 0x04001047 RID: 4167
		private object _eventHandlerLock = new object();

		// Token: 0x04001048 RID: 4168
		private DateTime _expirationTime = DateTime.MaxValue;

		// Token: 0x04001049 RID: 4169
		private List<string> _serverList = new List<string>();

		// Token: 0x0400104A RID: 4170
		private static object _startStopLock = new object();

		// Token: 0x0400104B RID: 4171
		private static readonly string _appDomainKey = Guid.NewGuid().ToString();

		// Token: 0x0400104C RID: 4172
		private static Dictionary<string, Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>> _serverUserHash = new Dictionary<string, Dictionary<SqlDependency.IdentityUserNamePair, List<SqlDependency.DatabaseServicePair>>>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x0400104D RID: 4173
		private static SqlDependencyProcessDispatcher _processDispatcher = null;

		// Token: 0x0400104E RID: 4174
		private static readonly string _assemblyName = typeof(SqlDependencyProcessDispatcher).Assembly.FullName;

		// Token: 0x0400104F RID: 4175
		private static readonly string _typeName = typeof(SqlDependencyProcessDispatcher).FullName;

		// Token: 0x04001050 RID: 4176
		internal const Bid.ApiGroup NotificationsTracePoints = Bid.ApiGroup.Dependency;

		// Token: 0x04001051 RID: 4177
		private readonly int _objectID = Interlocked.Increment(ref SqlDependency._objectTypeCount);

		// Token: 0x04001052 RID: 4178
		private static int _objectTypeCount;

		// Token: 0x020003BE RID: 958
		internal class IdentityUserNamePair
		{
			// Token: 0x060034F2 RID: 13554 RVA: 0x00144320 File Offset: 0x00143720
			internal IdentityUserNamePair(DbConnectionPoolIdentity identity, string userName)
			{
				this._identity = identity;
				this._userName = userName;
			}

			// Token: 0x1700084E RID: 2126
			// (get) Token: 0x060034F3 RID: 13555 RVA: 0x00144344 File Offset: 0x00143744
			internal DbConnectionPoolIdentity Identity
			{
				get
				{
					return this._identity;
				}
			}

			// Token: 0x1700084F RID: 2127
			// (get) Token: 0x060034F4 RID: 13556 RVA: 0x00144358 File Offset: 0x00143758
			internal string UserName
			{
				get
				{
					return this._userName;
				}
			}

			// Token: 0x060034F5 RID: 13557 RVA: 0x0014436C File Offset: 0x0014376C
			public override bool Equals(object value)
			{
				SqlDependency.IdentityUserNamePair identityUserNamePair = (SqlDependency.IdentityUserNamePair)value;
				bool flag = false;
				if (identityUserNamePair == null)
				{
					flag = false;
				}
				else if (this == identityUserNamePair)
				{
					flag = true;
				}
				else if (this._identity != null)
				{
					if (this._identity.Equals(identityUserNamePair._identity))
					{
						flag = true;
					}
				}
				else if (this._userName == identityUserNamePair._userName)
				{
					flag = true;
				}
				return flag;
			}

			// Token: 0x060034F6 RID: 13558 RVA: 0x001443C8 File Offset: 0x001437C8
			public override int GetHashCode()
			{
				int num;
				if (this._identity != null)
				{
					num = this._identity.GetHashCode();
				}
				else
				{
					num = this._userName.GetHashCode();
				}
				return num;
			}

			// Token: 0x040020A8 RID: 8360
			private DbConnectionPoolIdentity _identity;

			// Token: 0x040020A9 RID: 8361
			private string _userName;
		}

		// Token: 0x020003BF RID: 959
		private class DatabaseServicePair
		{
			// Token: 0x060034F7 RID: 13559 RVA: 0x001443FC File Offset: 0x001437FC
			internal DatabaseServicePair(string database, string service)
			{
				this._database = database;
				this._service = service;
			}

			// Token: 0x17000850 RID: 2128
			// (get) Token: 0x060034F8 RID: 13560 RVA: 0x00144420 File Offset: 0x00143820
			internal string Database
			{
				get
				{
					return this._database;
				}
			}

			// Token: 0x17000851 RID: 2129
			// (get) Token: 0x060034F9 RID: 13561 RVA: 0x00144434 File Offset: 0x00143834
			internal string Service
			{
				get
				{
					return this._service;
				}
			}

			// Token: 0x060034FA RID: 13562 RVA: 0x00144448 File Offset: 0x00143848
			public override bool Equals(object value)
			{
				SqlDependency.DatabaseServicePair databaseServicePair = (SqlDependency.DatabaseServicePair)value;
				bool flag = false;
				if (databaseServicePair == null)
				{
					flag = false;
				}
				else if (this == databaseServicePair)
				{
					flag = true;
				}
				else if (this._database == databaseServicePair._database)
				{
					flag = true;
				}
				return flag;
			}

			// Token: 0x060034FB RID: 13563 RVA: 0x00144484 File Offset: 0x00143884
			public override int GetHashCode()
			{
				return this._database.GetHashCode();
			}

			// Token: 0x040020AA RID: 8362
			private string _database;

			// Token: 0x040020AB RID: 8363
			private string _service;
		}

		// Token: 0x020003C0 RID: 960
		internal class EventContextPair
		{
			// Token: 0x060034FC RID: 13564 RVA: 0x0014449C File Offset: 0x0014389C
			internal EventContextPair(OnChangeEventHandler eventHandler, SqlDependency dependency)
			{
				this._eventHandler = eventHandler;
				this._context = ExecutionContext.Capture();
				this._dependency = dependency;
			}

			// Token: 0x060034FD RID: 13565 RVA: 0x001444C8 File Offset: 0x001438C8
			public override bool Equals(object value)
			{
				SqlDependency.EventContextPair eventContextPair = (SqlDependency.EventContextPair)value;
				bool flag = false;
				if (eventContextPair == null)
				{
					flag = false;
				}
				else if (this == eventContextPair)
				{
					flag = true;
				}
				else if (this._eventHandler == eventContextPair._eventHandler)
				{
					flag = true;
				}
				return flag;
			}

			// Token: 0x060034FE RID: 13566 RVA: 0x00144504 File Offset: 0x00143904
			public override int GetHashCode()
			{
				return this._eventHandler.GetHashCode();
			}

			// Token: 0x060034FF RID: 13567 RVA: 0x0014451C File Offset: 0x0014391C
			internal void Invoke(SqlNotificationEventArgs args)
			{
				this._args = args;
				ExecutionContext.Run(this._context, SqlDependency.EventContextPair._contextCallback, this);
			}

			// Token: 0x06003500 RID: 13568 RVA: 0x00144544 File Offset: 0x00143944
			private static void InvokeCallback(object eventContextPair)
			{
				SqlDependency.EventContextPair eventContextPair2 = (SqlDependency.EventContextPair)eventContextPair;
				eventContextPair2._eventHandler(eventContextPair2._dependency, eventContextPair2._args);
			}

			// Token: 0x040020AC RID: 8364
			private OnChangeEventHandler _eventHandler;

			// Token: 0x040020AD RID: 8365
			private ExecutionContext _context;

			// Token: 0x040020AE RID: 8366
			private SqlDependency _dependency;

			// Token: 0x040020AF RID: 8367
			private SqlNotificationEventArgs _args;

			// Token: 0x040020B0 RID: 8368
			private static ContextCallback _contextCallback = new ContextCallback(SqlDependency.EventContextPair.InvokeCallback);
		}
	}
}

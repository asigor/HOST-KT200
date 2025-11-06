using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using KT200IILOADER.Properties;
using KT200Loader;
using ZBobb;

namespace KT200IILoader
{
	// Token: 0x02000013 RID: 19
	public sealed class Form1 : Form
	{
		// Token: 0x06000062 RID: 98 RVA: 0x0002C140 File Offset: 0x0002A340
		public Form1()
		{
			this.InitializeComponent();
			this.InitializeCustomTitleBar();
			this.ApplyCustomFonts();
			this.toolTip1.SetToolTip(this.btnOffline, "Start the PROGRAM ,DO NOT CLOSE LOADER!");
			this.toolTip1.SetToolTip(this.btnExit, "Exit the application");
			DebugMessageInterceptor.OnMessageReceived += delegate(string message)
			{
				Console.WriteLine("Trigger!!");
				this.HandleDebugMessage();
			};
			this.StartupCheck();
		}

		// Token: 0x06000063 RID: 99 RVA: 0x0002C1F8 File Offset: 0x0002A3F8
		private void InitializeCustomTitleBar()
		{
			this.label1.Text = "Made by CHIPLOGIC because there was a need of a new GALETTO! If you manged to get here, send me a BEER!";
			this.label1.Dock = DockStyle.Top;
			this.label1.TextAlign = ContentAlignment.MiddleCenter;
			this.label1.BackColor = Color.Transparent;
			this.label1.ForeColor = Color.Black;
			this.label1.Height = 30;
			this.label1.Font = new Font("Microsoft Sans Serif", 1.25f, FontStyle.Bold);
			this.label1.MouseDown += delegate(object sender, MouseEventArgs e)
			{
				this.lastPoint = new Point(e.X, e.Y);
			};
			this.label1.MouseMove += delegate(object sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					base.Left += e.X - this.lastPoint.X;
					base.Top += e.Y - this.lastPoint.Y;
				}
			};
		}

		// Token: 0x06000064 RID: 100 RVA: 0x0002C2A4 File Offset: 0x0002A4A4
		private async void StartupCheck()
		{
			GlobalData.DeleteDll();
			bool flag = true;
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Honey.bin");
			GlobalData.DeleteDll();
			this.extractedData = HoneyDataProcessor.ProcessHoneyBin(text);
			Form1.DebugLogger.Log("=== Extracted Data ===");
			Form1.DebugLogger.Log("INFO_ACTIVATION1: " + this.extractedData.INFO_ACTIVATION1);
			Form1.DebugLogger.Log("INFO_INIT2024  : " + this.extractedData.INFO_INIT2024);
			Form1.DebugLogger.Log("INFO_ACTIVATION2: " + this.extractedData.INFO_ACTIVATION2);
			Form1.DebugLogger.Log("INFO_FIRSTSERIAL: " + this.extractedData.INFO_FIRSTSERIAL);
			Form1.DebugLogger.Log("INFO_SECONDSERIAL: " + this.extractedData.INFO_SECONDSERIAL);
			Form1.DebugLogger.Log("INFO_CHATGPT       : " + this.extractedData.INFO_CHATGPT);
			Form1.DebugLogger.Log("INFO_COMM2024   : " + this.extractedData.INFO_COMM2024);
			Form1.DebugLogger.Log("  ");
			Form1.DebugLogger.Log(string.Format("DATAORA_CLICK: {0}", this.extractedData.DATAORA_CLICK));
			Form1.DebugLogger.Log(string.Format("DATAORA_ACTIVATION1: {0}", this.extractedData.DATAORA_ACTIVATION1));
			Form1.DebugLogger.Log(string.Format("DATAORA_handshake: {0}", this.extractedData.DATAORA_handshake));
			Form1.DebugLogger.Log(string.Format("DATAORA_INIT2024: {0}", this.extractedData.DATAORA_INIT2024));
			Form1.DebugLogger.Log(string.Format("DATAORA_ACTIVATION2: {0}", this.extractedData.DATAORA_ACTIVATION2));
			Form1.DebugLogger.Log(string.Format("DATAORA_CHATGPT: {0}", this.extractedData.DATAORA_CHATGPT));
			Form1.DebugLogger.Log(string.Format("DATAORA_COMM2024: {0}", this.extractedData.DATAORA_COMM2024));
			Form1.DebugLogger.Log("   ");
			int num = 0;
			TaskAwaiter taskAwaiter2;
			try
			{
				string text2 = "C:\\KT200II\\KT200II.exe";
				string targetFolder = "C:\\KT200II\\Loader\\Files";
				if (!File.Exists(text2))
				{
					this.btnOffline.Enabled = false;
					this.LogMessage("KT200II NOT FOUND!", true);
					TaskAwaiter taskAwaiter = Task.Delay(2000).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter);
					}
					taskAwaiter.GetResult();
					Form1.DebugLogger.Log("KT200II executable is missing. Exiting application...");
					flag = false;
				}
				if (!Directory.Exists(targetFolder))
				{
					this.btnOffline.Enabled = false;
					this.LogMessage("MISSING Files FOLDER", false);
					Form1.DebugLogger.Log("Folder '" + targetFolder + "' does not exist.");
					flag = false;
				}
				if (flag)
				{
					this.LogMessage("All ok!", false);
					this.LogMessage("Press start", false);
					Form1.DebugLogger.Log("Startup check passed. All required files and directories are present.");
				}
				else
				{
					this.LogMessage("Startup checks failed", false);
					this.LogMessage("Please resolve the issues and try again!", false);
					Form1.DebugLogger.Log("Startup checks failed.");
					TaskAwaiter taskAwaiter = Task.Delay(4000).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter);
					}
					taskAwaiter.GetResult();
					Application.Exit();
				}
				targetFolder = null;
			}
			catch (Exception obj)
			{
				num = 1;
			}
			object obj;
			if (num == 1)
			{
				Form1.DebugLogger.Log("Error during startup check: " + ((Exception)obj).Message);
				TaskAwaiter taskAwaiter = Task.Delay(4000).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter);
				}
				taskAwaiter.GetResult();
				Application.Exit();
			}
			obj = null;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x0002C2DC File Offset: 0x0002A4DC
		private async void StartKT200()
		{
			GlobalData.move_oledll();
			try
			{
				string text = "C:\\KT200II\\KT200II.exe";
				string directoryName = Path.GetDirectoryName(text);
				if (!File.Exists(text))
				{
					this.LogMessage("KT200II NOT FOUND!!", false);
				}
				else
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = text,
						Arguments = "",
						WorkingDirectory = directoryName,
						UseShellExecute = false,
						CreateNoWindow = false,
						WindowStyle = ProcessWindowStyle.Hidden
					});
					this.LogMessage("KT200II Launching....Please wait", false);
					await Task.Delay(2000);
					this.LogMessage("Waking up the worker bee...", false);
					Form1.DebugLogger.Log("KT200II Launching");
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error starting KT200II.exe : " + ex.Message);
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0002C314 File Offset: 0x0002A514
		private async void StartProcessMonitoring()
		{
			this.processMonitorTimer = new global::System.Timers.Timer(200.0);
			this.processMonitorTimer.Elapsed += this.CheckProcessStatus;
			this.processMonitorTimer.AutoReset = true;
			this.processMonitorTimer.Enabled = true;
			await Task.Delay(3000);
			this.LogMessage("The loader will run in background!", true);
			await Task.Delay(3000);
			this.LogMessage("Enjoy the FULL OFFLINE experience", false);
			await Task.Delay(3000);
			base.Hide();
			base.WindowState = FormWindowState.Minimized;
			Form1.RestoreOriginalDateTime();
			Form1.DebugLogger.Log("Process monitoring started...");
			DebugMessageInterceptor.Start();
		}

		// Token: 0x06000067 RID: 103 RVA: 0x0002C34C File Offset: 0x0002A54C
		private async void CheckProcessStatus(object sender, ElapsedEventArgs e)
		{
			if (Process.GetProcessesByName(this.processToMonitor).Length == 0)
			{
				this.LogMessage("Finishing Up ....closing the loader", true);
				base.Invoke(new Action(delegate
				{
					base.WindowState = FormWindowState.Normal;
					base.Show();
					base.BringToFront();
					base.Activate();
				}));
				this.processMonitorTimer.Stop();
				this.processMonitorTimer.Dispose();
				await Task.Delay(2000);
				this.ExitSequenceOffline();
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x0002C384 File Offset: 0x0002A584
		private async Task ProcessRequestOffline(TcpClient client)
		{
			string activation = this.beforeactivation1 + this.extractedData.INFO_ACTIVATION1 + this.afteractivation1;
			string init2024 = this.beforeinit2024 + this.extractedData.INFO_INIT2024 + this.afterinit2024;
			string activation2 = this.beforeactivation1 + this.extractedData.INFO_ACTIVATION2 + this.afteractivation1;
			string Chatgpt = this.extractedData.INFO_CHATGPT + this.afterhabarnam;
			string comm2024 = this.extractedData.INFO_COMM2024 + this.afterhabarnam;
			string localFolderPath = "C:\\KT200II\\Loader\\Files";
			try
			{
				using (NetworkStream stream = client.GetStream())
				{
					TextReader textReader = new StreamReader(stream);
					StreamWriter writer = new StreamWriter(stream)
					{
						AutoFlush = true
					};
					string text = await textReader.ReadLineAsync();
					if (string.IsNullOrEmpty(text))
					{
						return;
					}
					string[] array = text.Split(new char[] { ' ' });
					if (array.Length < 2)
					{
						return;
					}
					string text2 = array[0];
					string text3 = array[1];
					Form1.DebugLogger.Log("Received request: " + text2 + " " + text3);
					Uri uri = new Uri("http://localhost" + text3);
					string text4 = uri.AbsolutePath.TrimStart(new char[] { '/' });
					Dictionary<string, string> dictionary = Form1.QueryStringParser.ParseQueryString(uri.Query);
					string text5 = "<html><body><h1>Unknown Path</h1></body></html>";
					Form1.CustomResponse customResponse = new Form1.CustomResponse
					{
						StatusCode = 404,
						Headers = new Dictionary<string, string>
						{
							{ "Content-Type", "text/html; charset=UTF-8" },
							{
								"Content-Length",
								Encoding.UTF8.GetByteCount(text5).ToString()
							}
						},
						Body = text5
					};
					bool flag = false;
					string text10;
					if (!(text4 == "weblm/activation.php"))
					{
						if (!(text4 == "handshake.php"))
						{
							if (!(text4 == "info.php"))
							{
								if (!(text4 == "init2024.php"))
								{
									if (!(text4 == "chatgpt.php"))
									{
										if (!(text4 == "comm2024.php"))
										{
											string[] array2 = text4.Split(new char[] { '/' });
											if (array2.Length < 2)
											{
												this.LogMessage("Invalid path: " + text4, false);
												return;
											}
											string text6 = array2[0];
											string text7 = string.Join("/", array2.Skip(1));
											string text8 = Path.Combine(localFolderPath, text6);
											if (!Directory.Exists(text8))
											{
												this.LogMessage("Folder not found: " + text8, false);
												return;
											}
											string text9 = Path.Combine(text8, text7);
											if (!File.Exists(text9))
											{
												this.LogMessage("File not found: " + text9, false);
												return;
											}
											byte[] fileBytes = File.ReadAllBytes(text9);
											TaskAwaiter taskAwaiter = writer.WriteAsync(string.Concat(new string[]
											{
												"HTTP/1.1 200 OK\r\nContent-Type: ",
												"application/octet-stream",
												"\r\n",
												string.Format("Content-Length: {0}\r\n", fileBytes.Length),
												"Connection: keep-alive\r\n\r\n"
											})).GetAwaiter();
											if (!taskAwaiter.IsCompleted)
											{
												await taskAwaiter;
												TaskAwaiter taskAwaiter2;
												taskAwaiter = taskAwaiter2;
												taskAwaiter2 = default(TaskAwaiter);
											}
											taskAwaiter.GetResult();
											await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
											flag = true;
											fileBytes = null;
										}
										else
										{
											this.LogMessage("comm........", false);
											if (this.executattimp)
											{
												this.Waittime();
											}
											this.executattimp = false;
											this.timpinregistrat = false;
											text5 = comm2024;
											customResponse = new Form1.CustomResponse
											{
												StatusCode = 200,
												Headers = new Dictionary<string, string>
												{
													{ "Server", "nginx" },
													{
														"Date",
														DateTime.UtcNow.ToString("r")
													},
													{ "Content-Type", "text/html; charset=UTF-8" },
													{ "Connection", "keep-alive" },
													{
														"Content-Length",
														Encoding.UTF8.GetByteCount(text5).ToString()
													}
												},
												Body = text5
											};
										}
									}
									else
									{
										text5 = Chatgpt;
										customResponse = new Form1.CustomResponse
										{
											StatusCode = 200,
											Headers = new Dictionary<string, string>
											{
												{ "Server", "nginx" },
												{
													"Date",
													DateTime.UtcNow.ToString("r")
												},
												{ "Content-Type", "text/html; charset=UTF-8" },
												{ "Connection", "keep-alive" },
												{
													"Content-Length",
													Encoding.UTF8.GetByteCount(text5).ToString()
												}
											},
											Body = text5
										};
									}
								}
								else
								{
									customResponse = new Form1.CustomResponse
									{
										StatusCode = 200,
										Headers = new Dictionary<string, string>
										{
											{ "Server", "nginx" },
											{
												"Date",
												DateTime.UtcNow.ToString("r")
											},
											{ "Content-Type", "text/html; charset=UTF-8" },
											{ "Transfer-Encoding", "chunked" },
											{ "Connection", "keep-alive" },
											{ "Vary", "Accept-Encoding" }
										},
										Body = init2024
									};
									Form1.ChangeSystemDateTime(this.extractedData.DATAORA_INIT2024);
									GlobalData.PopulateTempData(this.extractedData.DATAORA_INIT2024);
								}
							}
							else
							{
								this.LogMessage("Greet thy working bee......", false);
								text5 = "OK\r\n0\r\n";
								customResponse = new Form1.CustomResponse
								{
									StatusCode = 200,
									Headers = new Dictionary<string, string>
									{
										{ "Server", "nginx" },
										{
											"Date",
											DateTime.UtcNow.ToString("r")
										},
										{ "Content-Type", "text/html; charset=UTF-8" },
										{ "Connection", "keep-alive" },
										{ "Vary", "Accept-Encoding" },
										{
											"Content-Length",
											Encoding.UTF8.GetByteCount(text5).ToString()
										}
									},
									Body = text5
								};
							}
						}
						else
						{
							Form1.ChangeSystemDateTime(this.extractedData.DATAORA_handshake);
							GlobalData.PopulateTempData(this.extractedData.DATAORA_handshake);
							this.LogMessage("Worker bee found another bee ..", false);
							text5 = "OK\r\n0\r\n";
							customResponse = new Form1.CustomResponse
							{
								StatusCode = 200,
								Headers = new Dictionary<string, string>
								{
									{ "Server", "nginx" },
									{
										"Date",
										DateTime.UtcNow.ToString("r")
									},
									{ "Content-Type", "text/html; charset=UTF-8" },
									{ "Connection", "keep-alive" },
									{ "Vary", "Accept-Encoding" },
									{
										"Content-Length",
										Encoding.UTF8.GetByteCount(text5).ToString()
									}
								},
								Body = text5
							};
						}
					}
					else if (dictionary.TryGetValue("code", out text10))
					{
						if (text10.StartsWith(this.extractedData.INFO_FIRSTSERIAL))
						{
							text5 = activation;
							customResponse = new Form1.CustomResponse
							{
								StatusCode = 200,
								Headers = new Dictionary<string, string>
								{
									{ "Server", "nginx" },
									{
										"Date",
										DateTime.UtcNow.ToString("r")
									},
									{ "Content-Type", "text/html; charset=UTF-8" },
									{ "Transfer-Encoding", "chunked" },
									{ "Connection", "keep-alive" },
									{ "Vary", "Accept-Encoding" },
									{
										"Content-Length",
										Encoding.UTF8.GetByteCount(text5).ToString()
									}
								},
								Body = text5
							};
							this.LogMessage("Worker Bee working...", false);
							GlobalData.StartPeriodicSharedMemoryUpdates(TimeSpan.FromMilliseconds(10.0));
							Form1.DebugLogger.Log(activation);
						}
						else if (text10.StartsWith(this.extractedData.INFO_SECONDSERIAL))
						{
							text5 = activation2;
							customResponse = new Form1.CustomResponse
							{
								StatusCode = 200,
								Headers = new Dictionary<string, string>
								{
									{ "Server", "nginx" },
									{
										"Date",
										DateTime.UtcNow.ToString("r")
									},
									{ "Content-Type", "text/html; charset=UTF-8" },
									{ "Transfer-Encoding", "chunked" },
									{ "Connection", "keep-alive" },
									{ "Vary", "Accept-Encoding" },
									{
										"Content-Length",
										Encoding.UTF8.GetByteCount(text5).ToString()
									}
								},
								Body = text5
							};
							this.LogMessage("Worker Bee found alot of honey........", false);
							Form1.DebugLogger.Log(activation2);
							this.StartProcessMonitoring();
							Form1.ChangeSystemDateTime(this.extractedData.DATAORA_COMM2024);
							GlobalData.PopulateTempData(this.extractedData.DATAORA_COMM2024);
						}
						else
						{
							this.LogMessage("Worker Bee found some of the honey........", false);
							text5 = activation2;
							this.StartProcessMonitoring();
							Form1.ChangeSystemDateTime(this.extractedData.DATAORA_COMM2024);
							GlobalData.PopulateTempData(this.extractedData.DATAORA_COMM2024);
							customResponse = new Form1.CustomResponse
							{
								StatusCode = 200,
								Headers = new Dictionary<string, string>
								{
									{ "Server", "nginx" },
									{
										"Date",
										DateTime.UtcNow.ToString("r")
									},
									{ "Content-Type", "text/html; charset=UTF-8" },
									{ "Transfer-Encoding", "chunked" },
									{ "Connection", "keep-alive" },
									{ "Vary", "Accept-Encoding" },
									{
										"Content-Length",
										Encoding.UTF8.GetByteCount(text5).ToString()
									}
								},
								Body = text5
							};
						}
					}
					if (flag)
					{
						return;
					}
					string text11 = this.BuildHttpResponse(customResponse);
					await writer.WriteAsync(text11);
					writer = null;
					customResponse = null;
				}
				NetworkStream stream = null;
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error handling client: " + ex.Message);
			}
			finally
			{
				client.Close();
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0002C3D0 File Offset: 0x0002A5D0
		private string BuildHttpResponse(Form1.CustomResponse customResponse)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format("HTTP/1.1 {0}", customResponse.StatusCode));
			foreach (KeyValuePair<string, string> keyValuePair in customResponse.Headers)
			{
				stringBuilder.AppendLine(keyValuePair.Key + ": " + keyValuePair.Value);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(customResponse.Body);
			return stringBuilder.ToString();
		}

		// Token: 0x0600006A RID: 106 RVA: 0x0002C478 File Offset: 0x0002A678
		private static void ExecuteCommandWithInput(string command)
		{
			try
			{
				using (Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/C " + command)
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}))
				{
					process.StandardInput.WriteLine(command);
					process.StandardInput.Close();
					string text = process.StandardOutput.ReadToEnd();
					string text2 = process.StandardError.ReadToEnd();
					process.WaitForExit();
					if (!string.IsNullOrWhiteSpace(text2))
					{
						Form1.DebugLogger.Log("Command error: " + text2);
					}
					Form1.DebugLogger.Log("Command output: " + text);
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("ExecuteCommandWithInput failed: " + ex.Message);
			}
		}

		// Token: 0x0600006B RID: 107 RVA: 0x0002C564 File Offset: 0x0002A764
		private static string ExecuteCommand(string command)
		{
			string text3;
			using (Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/C " + command)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			}))
			{
				string text = process.StandardOutput.ReadToEnd();
				string text2 = process.StandardError.ReadToEnd();
				process.WaitForExit();
				if (!string.IsNullOrWhiteSpace(text2))
				{
					Form1.DebugLogger.Log("Command error: " + text2);
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					Form1.DebugLogger.Log("Command output: " + text);
				}
				text3 = ((!string.IsNullOrWhiteSpace(text)) ? text : text2);
			}
			return text3;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x0002C620 File Offset: 0x0002A820
		private static void ChangeSystemDateTime(DateTime newDateTime)
		{
			try
			{
				string text = Form1.DetectSystemDateFormat();
				string text2 = newDateTime.ToString(text);
				string text3 = newDateTime.ToString("HH:mm:ss");
				Form1.DebugLogger.Log("Attempting to change system date to: " + text2);
				Form1.ExecuteCommandWithInput("date " + text2);
				Form1.DebugLogger.Log("Attempting to change system time to: " + text3);
				Form1.ExecuteCommandWithInput("time " + text3);
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Failed to change system date and time: " + ex.Message);
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x0002C6B4 File Offset: 0x0002A8B4
		private static string DetectSystemDateFormat()
		{
			string text2;
			try
			{
				string text = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("yyyy", "yyyy").Replace("yy", "yy").Replace("MM", "MM")
					.Replace("dd", "dd")
					.Replace("/", "-")
					.Replace(".", "-")
					.Replace("\\", "-")
					.Trim();
				Form1.DebugLogger.Log("Detected system date format: " + text);
				if (CultureInfo.CurrentCulture.Name.StartsWith("hu"))
				{
					text = "yyyy-MM-dd";
				}
				text2 = text;
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error detecting system date format: " + ex.Message);
				text2 = "yyyy-MM-dd";
			}
			return text2;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x0002C7A0 File Offset: 0x0002A9A0
		private async void btnExit_Click(object sender, EventArgs e)
		{
			if (this.Rulatprogram)
			{
				Form1.RestoreOriginalDateTime();
				this.RemoveHostsEntry();
			}
			this.LogMessage("Worker bee going to sleep now..", false);
			await Task.Delay(1000);
			Application.Exit();
			GlobalData.DeleteDll();
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0002C7D8 File Offset: 0x0002A9D8
		private async void btnOffline_Click(object sender, EventArgs e)
		{
			this.Rulatprogram = true;
			this.EnsureHostsEntry();
			GlobalData.InitializeSharedMemory();
			Form1.originalDateTime = DateTime.Now;
			GlobalData.PopulateTempData(this.extractedData.DATAORA_CLICK);
			GlobalData.PopulateTempData(this.extractedData.DATAORA_CLICK);
			Form1.ChangeSystemDateTime(this.extractedData.DATAORA_CLICK);
			this.btnOffline.Enabled = false;
			this.StartKT200();
			this.listener1 = new TcpListener(IPAddress.Any, 80);
			this.isRunning = true;
			this.listener1.Start();
			this.LogMessage("Starting in Offline Mode .... Please Wait!", false);
			while (this.isRunning)
			{
				try
				{
					Form1.<>c__DisplayClass35_0 CS$<>8__locals1 = new Form1.<>c__DisplayClass35_0();
					CS$<>8__locals1.<>4__this = this;
					TcpClient tcpClient = await this.listener1.AcceptTcpClientAsync();
					CS$<>8__locals1.client = tcpClient;
					Task.Run(() => CS$<>8__locals1.<>4__this.ProcessRequestOffline(CS$<>8__locals1.client));
					CS$<>8__locals1 = null;
				}
				catch (Exception ex)
				{
					Form1.DebugLogger.Log("Error accepting client: " + ex.Message);
				}
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x0002C810 File Offset: 0x0002AA10
		private static void RestoreOriginalDateTime()
		{
			try
			{
				Form1.ChangeSystemDateTime(Form1.originalDateTime);
				string text = Form1.ExecuteCommand("w32tm /resync");
				if (text.Contains("The service has not been started") || text.Contains("0x80070426"))
				{
					Form1.DebugLogger.Log("The Windows Time service is not running. Attempting to start it...");
					string text2 = Form1.ExecuteCommand("net start w32time");
					Form1.DebugLogger.Log("Command output for starting the service: " + text2);
					text = Form1.ExecuteCommand("w32tm /resync");
					Form1.DebugLogger.Log("Retrying w32tm resync: " + text);
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Failed to restore system date and time: " + ex.Message);
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0002C8B8 File Offset: 0x0002AAB8
		private void LogMessage(string message, bool clearLog = false)
		{
			if (this.alphaBlendTextBox1.InvokeRequired)
			{
				this.alphaBlendTextBox1.Invoke(new Action(delegate
				{
					if (clearLog)
					{
						this.alphaBlendTextBox1.Clear();
					}
					this.alphaBlendTextBox1.AppendText(message + Environment.NewLine);
				}));
				return;
			}
			if (clearLog)
			{
				this.alphaBlendTextBox1.Clear();
			}
			this.alphaBlendTextBox1.AppendText(message + Environment.NewLine);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x0002AB0C File Offset: 0x00028D0C
		private void ExitSequenceOffline()
		{
			GlobalData.DeleteDll();
			if (this.Rulatprogram)
			{
				Form1.RestoreOriginalDateTime();
				this.RemoveHostsEntry();
				Task.Delay(1000);
				GlobalData.DeleteDll();
			}
			GlobalData.DeleteDll();
			Application.Exit();
		}

		// Token: 0x06000073 RID: 115 RVA: 0x0002C934 File Offset: 0x0002AB34
		private void EnsureHostsEntry()
		{
			try
			{
				string text = "127.0.0.1 kt.tuner-tools.com";
				List<string> list = File.ReadAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts").ToList<string>();
				if (!list.Contains(text))
				{
					list.Add(text);
					File.WriteAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts", list);
					Form1.DebugLogger.Log("Hosts entry added: " + text);
				}
				else
				{
					Form1.DebugLogger.Log("Hosts entry already exists: " + text);
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error ensuring hosts entry: " + ex.Message);
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x0002C9C0 File Offset: 0x0002ABC0
		private void RemoveHostsEntry()
		{
			try
			{
				string text = "127.0.0.1 kt.tuner-tools.com";
				List<string> list = File.ReadAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts").ToList<string>();
				if (list.Remove(text))
				{
					File.WriteAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts", list);
					Form1.DebugLogger.Log("Hosts entry removed.");
				}
				else
				{
					Form1.DebugLogger.Log("Hosts entry not found.");
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error removing hosts entry: " + ex.Message);
			}
		}

		// Token: 0x06000075 RID: 117
		[DllImport("gdi32.dll")]
		private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

		// Token: 0x06000076 RID: 118 RVA: 0x0002CA38 File Offset: 0x0002AC38
		private PrivateFontCollection LoadFontsFromResources()
		{
			PrivateFontCollection privateFontCollection = new PrivateFontCollection();
			try
			{
				foreach (string text in new string[] { "KT200IILOADER.Resources.TEMPSITC.TTF" })
				{
					using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text))
					{
						if (manifestResourceStream == null)
						{
							throw new Exception("Resource '" + text + "' not found.");
						}
						IntPtr intPtr = Marshal.AllocCoTaskMem((int)manifestResourceStream.Length);
						byte[] array2 = new byte[manifestResourceStream.Length];
						manifestResourceStream.Read(array2, 0, array2.Length);
						Marshal.Copy(array2, 0, intPtr, array2.Length);
						uint num = 0U;
						Form1.AddFontMemResourceEx(intPtr, (uint)array2.Length, IntPtr.Zero, ref num);
						privateFontCollection.AddMemoryFont(intPtr, array2.Length);
						Marshal.FreeCoTaskMem(intPtr);
					}
				}
			}
			catch (Exception ex)
			{
				Form1.DebugLogger.Log("Error loading fonts: " + ex.Message);
			}
			return privateFontCollection;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x0002CB44 File Offset: 0x0002AD44
		private void ApplyCustomFonts()
		{
			PrivateFontCollection privateFontCollection = this.LoadFontsFromResources();
			if (privateFontCollection.Families.Length != 0)
			{
				Font font = new Font(privateFontCollection.Families[0], 14f, FontStyle.Bold);
				Font font2 = new Font(privateFontCollection.Families[0], 12f, FontStyle.Bold);
				this.btnOffline.Font = font;
				this.btnExit.Font = font;
				this.alphaBlendTextBox1.Font = font2;
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x0002A948 File Offset: 0x00028B48
		private void label2_Click(object sender, EventArgs e)
		{
		}

		// Token: 0x06000079 RID: 121 RVA: 0x0002A948 File Offset: 0x00028B48
		private void Form1_Load(object sender, EventArgs e)
		{
		}

		// Token: 0x0600007A RID: 122 RVA: 0x0002CBB0 File Offset: 0x0002ADB0
		private async void Waittime()
		{
			await Task.Delay(3000);
			Form1.RestoreOriginalDateTime();
			Form1.originalDateTime = DateTime.Now;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0002CBE0 File Offset: 0x0002ADE0
		private async void HandleDebugMessage()
		{
			if (!this.executattimp)
			{
				if (!this.timpinregistrat)
				{
					Form1.originalDateTime = DateTime.Now;
					this.timpinregistrat = true;
				}
				this.executattimp = true;
				Form1.DebugLogger.Log("[ACTION] Changing system date to 2...");
				Form1.ChangeSystemDateTime(this.extractedData.DATAORA_COMM2024);
				GlobalData.PopulateTempData(this.extractedData.DATAORA_COMM2024);
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0002CC18 File Offset: 0x0002AE18
		private void InitializeComponent()
		{
			this.components = new Container();
			this.btnOffline = new Button();
			this.btnExit = new Button();
			this.toolTip1 = new ToolTip(this.components);
			this.label1 = new Label();
			this.alphaBlendTextBox1 = new AlphaBlendTextBox();
			this.label2 = new Label();
			base.SuspendLayout();
			this.btnOffline.BackColor = Color.Transparent;
			this.btnOffline.BackgroundImageLayout = ImageLayout.None;
			this.btnOffline.FlatAppearance.BorderColor = Color.Black;
			this.btnOffline.FlatAppearance.BorderSize = 0;
			this.btnOffline.FlatStyle = FlatStyle.Popup;
			this.btnOffline.Font = new Font("Arial Black", 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.btnOffline.ImageAlign = ContentAlignment.MiddleRight;
			this.btnOffline.Location = new Point(12, 9);
			this.btnOffline.Name = "btnOffline";
			this.btnOffline.Size = new Size(95, 40);
			this.btnOffline.TabIndex = 1;
			this.btnOffline.Text = "START";
			this.btnOffline.UseVisualStyleBackColor = false;
			this.btnOffline.Click += this.btnOffline_Click;
			this.btnExit.BackColor = Color.Transparent;
			this.btnExit.BackgroundImageLayout = ImageLayout.None;
			this.btnExit.FlatAppearance.BorderColor = Color.Black;
			this.btnExit.FlatAppearance.BorderSize = 0;
			this.btnExit.FlatAppearance.MouseDownBackColor = Color.Transparent;
			this.btnExit.FlatAppearance.MouseOverBackColor = Color.Transparent;
			this.btnExit.FlatStyle = FlatStyle.Popup;
			this.btnExit.Font = new Font("Arial Black", 14.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.btnExit.ImageAlign = ContentAlignment.MiddleRight;
			this.btnExit.Location = new Point(305, 12);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new Size(95, 37);
			this.btnExit.TabIndex = 2;
			this.btnExit.Text = "EXIT";
			this.btnExit.UseVisualStyleBackColor = false;
			this.btnExit.Click += this.btnExit_Click;
			this.toolTip1.BackColor = Color.Black;
			this.toolTip1.ForeColor = Color.Lime;
			this.label1.BackColor = Color.Transparent;
			this.label1.Dock = DockStyle.Top;
			this.label1.FlatStyle = FlatStyle.Flat;
			this.label1.Font = new Font("Microsoft Sans Serif", 1.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.label1.ForeColor = Color.Black;
			this.label1.Location = new Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new Size(400, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Made by CHIPLOGIC because there was a need of a new GALETTO! If you manged to get here, send me a BEER!";
			this.alphaBlendTextBox1.BackAlpha = 0;
			this.alphaBlendTextBox1.BackColor = Color.FromArgb(0, 0, 0);
			this.alphaBlendTextBox1.BorderStyle = BorderStyle.None;
			this.alphaBlendTextBox1.Cursor = Cursors.Arrow;
			this.alphaBlendTextBox1.Font = new Font("Arial Unicode MS", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.alphaBlendTextBox1.ForeColor = Color.Chartreuse;
			this.alphaBlendTextBox1.Location = new Point(12, 120);
			this.alphaBlendTextBox1.Multiline = true;
			this.alphaBlendTextBox1.Name = "alphaBlendTextBox1";
			this.alphaBlendTextBox1.ReadOnly = true;
			this.alphaBlendTextBox1.Size = new Size(376, 46);
			this.alphaBlendTextBox1.TabIndex = 5;
			this.alphaBlendTextBox1.WordWrap = false;
			this.label2.AutoSize = true;
			this.label2.BackColor = Color.Transparent;
			this.label2.Enabled = false;
			this.label2.Font = new Font("Arial", 14.25f, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
			this.label2.ForeColor = Color.LimeGreen;
			this.label2.Location = new Point(305, 74);
			this.label2.Name = "label2";
			this.label2.Size = new Size(64, 23);
			this.label2.TabIndex = 6;
			this.label2.Text = "v3.4.2";
			base.AutoScaleDimensions = new SizeF(2f, 2f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.AutoSize = true;
			base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.BackColor = Color.Gray;
			this.BackgroundImage = Resources.bkg6_mic;
			this.BackgroundImageLayout = ImageLayout.Stretch;
			base.ClientSize = new Size(400, 170);
			base.ControlBox = false;
			base.Controls.Add(this.label2);
			base.Controls.Add(this.alphaBlendTextBox1);
			base.Controls.Add(this.btnExit);
			base.Controls.Add(this.btnOffline);
			base.Controls.Add(this.label1);
			this.DoubleBuffered = true;
			this.Font = new Font("Microsoft Sans Serif", 1.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.ForeColor = Color.Lime;
			base.FormBorderStyle = FormBorderStyle.None;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "Form1";
			base.ShowIcon = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "1";
			base.TopMost = true;
			base.Load += this.Form1_Load;
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400003C RID: 60
		private ExtractedData extractedData;

		// Token: 0x0400003D RID: 61
		private global::System.Timers.Timer processMonitorTimer;

		// Token: 0x0400003E RID: 62
		private string processToMonitor = "DFOX";

		// Token: 0x0400003F RID: 63
		private static DateTime originalDateTime;

		// Token: 0x04000040 RID: 64
		private TcpListener listener1;

		// Token: 0x04000041 RID: 65
		private const string HostsFilePath = "C:\\Windows\\System32\\drivers\\etc\\hosts";

		// Token: 0x04000042 RID: 66
		private const string HostsEntry = "127.0.0.1 kt.tuner-tools.com";

		// Token: 0x04000043 RID: 67
		private bool isRunning;

		// Token: 0x04000044 RID: 68
		private bool Rulatprogram;

		// Token: 0x04000045 RID: 69
		private bool timpinregistrat;

		// Token: 0x04000046 RID: 70
		private bool executattimp;

		// Token: 0x04000047 RID: 71
		private CancellationTokenSource dateTimeChangeCts;

		// Token: 0x04000048 RID: 72
		private Task dateTimeChangeTask;

		// Token: 0x04000049 RID: 73
		private string beforeactivation1 = "15b\r\nOK\n";

		// Token: 0x0400004A RID: 74
		private string afteractivation1 = "\r\n0\r\n";

		// Token: 0x0400004B RID: 75
		private string afterinit2024 = "\r\n0\r\n";

		// Token: 0x0400004C RID: 76
		private string afterhabarnam = "\r\n";

		// Token: 0x0400004D RID: 77
		private string beforeinit2024 = "49\r\n";

		// Token: 0x0400004E RID: 78
		private readonly object lockObject = new object();

		// Token: 0x0400004F RID: 79
		private Point lastPoint;

		// Token: 0x04000050 RID: 80
		private IContainer components;

		// Token: 0x04000051 RID: 81
		private Button btnOffline;

		// Token: 0x04000052 RID: 82
		private Button btnExit;

		// Token: 0x04000053 RID: 83
		private ToolTip toolTip1;

		// Token: 0x04000054 RID: 84
		private Label label1;

		// Token: 0x04000055 RID: 85
		private AlphaBlendTextBox alphaBlendTextBox1;

		// Token: 0x04000056 RID: 86
		private Label label2;

		// Token: 0x02000014 RID: 20
		private sealed class CustomResponse
		{
			// Token: 0x1700001E RID: 30
			// (get) Token: 0x06000081 RID: 129 RVA: 0x0002AB86 File Offset: 0x00028D86
			// (set) Token: 0x06000082 RID: 130 RVA: 0x0002AB8E File Offset: 0x00028D8E
			public int StatusCode { get; set; }

			// Token: 0x1700001F RID: 31
			// (get) Token: 0x06000083 RID: 131 RVA: 0x0002AB97 File Offset: 0x00028D97
			// (set) Token: 0x06000084 RID: 132 RVA: 0x0002AB9F File Offset: 0x00028D9F
			public Dictionary<string, string> Headers { get; set; }

			// Token: 0x17000020 RID: 32
			// (get) Token: 0x06000085 RID: 133 RVA: 0x0002ABA8 File Offset: 0x00028DA8
			// (set) Token: 0x06000086 RID: 134 RVA: 0x0002ABB0 File Offset: 0x00028DB0
			public string Body { get; set; }
		}

		// Token: 0x02000015 RID: 21
		public static class QueryStringParser
		{
			// Token: 0x06000088 RID: 136 RVA: 0x0002D26C File Offset: 0x0002B46C
			public static Dictionary<string, string> ParseQueryString(string query)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				if (string.IsNullOrWhiteSpace(query))
				{
					return dictionary;
				}
				if (query.StartsWith("?"))
				{
					query = query.Substring(1);
				}
				string[] array = query.Split(new char[] { '&' });
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(new char[] { '=' });
					if (array2.Length == 2)
					{
						string text = Uri.UnescapeDataString(array2[0]);
						string text2 = Uri.UnescapeDataString(array2[1]);
						dictionary[text] = text2;
					}
				}
				return dictionary;
			}
		}

		// Token: 0x02000016 RID: 22
		public static class DebugLogger
		{
			// Token: 0x06000089 RID: 137 RVA: 0x0002A948 File Offset: 0x00028B48
			public static void Log(string message)
			{
			}
		}
	}
}

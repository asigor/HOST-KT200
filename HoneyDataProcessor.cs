using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SmartAssembly.Delegates;
using SmartAssembly.HouseOfCards;

namespace KT200Loader
{
	// Token: 0x0200000C RID: 12
	public static class HoneyDataProcessor
	{
		// Token: 0x06000047 RID: 71 RVA: 0x0002BB74 File Offset: 0x00029D74
		public static ExtractedData ProcessHoneyBin(string honeyBinPath)
		{
			string info_ACTIVATION;
			string info_INIT;
			string info_ACTIVATION2;
			string info_FIRSTSERIAL;
			string info_SECONDSERIAL;
			string info_CHATGPT;
			string info_COMM;
			DateTime dataora_CLICK;
			DateTime dataora_ACTIVATION;
			DateTime dataora_handshake;
			DateTime dataora_INIT;
			DateTime dataora_ACTIVATION2;
			DateTime dataora_CHATGPT;
			DateTime dataora_COMM;
			HoneyDataProcessor.MyDataExtractor.ExtractData(HoneyDataProcessor.ParseHoney.DecryptAndParseAll(honeyBinPath), out info_ACTIVATION, out info_INIT, out info_ACTIVATION2, out info_FIRSTSERIAL, out info_SECONDSERIAL, out info_CHATGPT, out info_COMM, out dataora_CLICK, out dataora_ACTIVATION, out dataora_handshake, out dataora_INIT, out dataora_ACTIVATION2, out dataora_CHATGPT, out dataora_COMM);
			return new ExtractedData
			{
				INFO_ACTIVATION1 = info_ACTIVATION,
				INFO_INIT2024 = info_INIT,
				INFO_ACTIVATION2 = info_ACTIVATION2,
				INFO_FIRSTSERIAL = info_FIRSTSERIAL,
				INFO_SECONDSERIAL = info_SECONDSERIAL,
				INFO_CHATGPT = info_CHATGPT,
				INFO_COMM2024 = info_COMM,
				DATAORA_CLICK = dataora_CLICK,
				DATAORA_ACTIVATION1 = dataora_ACTIVATION,
				DATAORA_handshake = dataora_handshake,
				DATAORA_INIT2024 = dataora_INIT,
				DATAORA_ACTIVATION2 = dataora_ACTIVATION2,
				DATAORA_CHATGPT = dataora_CHATGPT,
				DATAORA_COMM2024 = dataora_COMM
			};
		}

		// Token: 0x0200000D RID: 13
		public static class ParseHoney
		{
			// Token: 0x06000048 RID: 72 RVA: 0x0002BC1C File Offset: 0x00029E1C
			public static List<HoneyDataProcessor.LogItem> DecryptAndParseAll(string honeyBinPath)
			{
				List<HoneyDataProcessor.LogItem> list = new List<HoneyDataProcessor.LogItem>();
				if (!File.Exists(honeyBinPath))
				{
					DebugLogger.Log(HoneyDataProcessor.ParseHoney.getString_0(107395866) + honeyBinPath);
					return list;
				}
				string[] array = File.ReadAllLines(honeyBinPath);
				List<string> list2 = new List<string>();
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string[] array3 = HoneyDataProcessor.ParseHoney.DecryptWithXorAndBase64(array2[i]).Replace(HoneyDataProcessor.ParseHoney.getString_0(107395829), HoneyDataProcessor.ParseHoney.getString_0(107395824)).Split(new char[]
					{
						'\n'
					});
					for (int j = 0; j < array3.Length; j++)
					{
						string text = array3[j].Trim();
						if (!string.IsNullOrEmpty(text))
						{
							list2.Add(text);
						}
					}
				}
				for (int k = 0; k < list2.Count; k++)
				{
					string input = list2[k];
					Match match = HoneyDataProcessor.ParseHoney.TimestampRegex.Match(input);
					if (match.Success)
					{
						HoneyDataProcessor.LogItem logItem = new HoneyDataProcessor.LogItem();
						string value = match.Groups[HoneyDataProcessor.ParseHoney.getString_0(107395823)].Value;
						logItem.IsRequest = value.Equals(HoneyDataProcessor.ParseHoney.getString_0(107395846), StringComparison.OrdinalIgnoreCase);
						logItem.IsResponse = !logItem.IsRequest;
						string value2 = match.Groups[HoneyDataProcessor.ParseHoney.getString_0(107395801)].Value;
						logItem.Timestamp = HoneyDataProcessor.ParseHoney.ExtractDateTime(value2);
						if (logItem.IsRequest)
						{
							if (k + 1 < list2.Count && list2[k + 1].StartsWith(HoneyDataProcessor.ParseHoney.getString_0(107395796), StringComparison.OrdinalIgnoreCase))
							{
								logItem.FullUrl = list2[k + 1].Substring(HoneyDataProcessor.ParseHoney.getString_0(107395796).Length).Trim();
								string endpoint;
								Dictionary<string, string> queryParams;
								HoneyDataProcessor.ParseHoney.ParseUrl(logItem.FullUrl, out endpoint, out queryParams);
								logItem.Endpoint = endpoint;
								logItem.QueryParams = queryParams;
							}
						}
						else if (k + 1 < list2.Count && list2[k + 1].StartsWith(HoneyDataProcessor.ParseHoney.getString_0(107395815), StringComparison.OrdinalIgnoreCase))
						{
							logItem.BodyLine = list2[k + 1].Substring(HoneyDataProcessor.ParseHoney.getString_0(107395815).Length).Trim();
							if (k + 2 < list2.Count && !HoneyDataProcessor.ParseHoney.IsNewSection(list2[k + 2]))
							{
								logItem.ExtraLine = list2[k + 2];
							}
						}
						list.Add(logItem);
					}
				}
				return list;
			}

			// Token: 0x06000049 RID: 73 RVA: 0x0002BECC File Offset: 0x0002A0CC
			private static string DecryptWithXorAndBase64(string base64Line)
			{
				string result;
				try
				{
					byte[] array = Convert.FromBase64String(base64Line);
					for (int i = 0; i < array.Length; i++)
					{
						byte[] array2 = array;
						int num = i;
						array2[num] ^= 85;
					}
					result = Encoding.UTF8.GetString(array);
				}
				catch
				{
					result = string.Empty;
				}
				return result;
			}

			// Token: 0x0600004A RID: 74 RVA: 0x0002BF24 File Offset: 0x0002A124
			private static DateTime ExtractDateTime(string tsRaw)
			{
				int num = tsRaw.IndexOf(HoneyDataProcessor.ParseHoney.getString_0(107395806), StringComparison.OrdinalIgnoreCase);
				if (num < 0)
				{
					return DateTime.MinValue;
				}
				DateTime result;
				DateTime.TryParseExact(tsRaw.Substring(0, num).Trim(), HoneyDataProcessor.ParseHoney.getString_0(107395765), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result);
				return result;
			}

			// Token: 0x0600004B RID: 75 RVA: 0x0002AA89 File Offset: 0x00028C89
			private static bool IsNewSection(string line)
			{
				return line.StartsWith(HoneyDataProcessor.ParseHoney.getString_0(107395736), StringComparison.OrdinalIgnoreCase);
			}

			// Token: 0x0600004C RID: 76 RVA: 0x0002BF80 File Offset: 0x0002A180
			private static void ParseUrl(string url, out string endpoint, out Dictionary<string, string> queryParams)
			{
				endpoint = string.Empty;
				queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				string[] array = url.Split(new char[]
				{
					'?'
				});
				string text = array[0];
				if (text.Contains(HoneyDataProcessor.ParseHoney.getString_0(107395731)))
				{
					string[] array2 = text.Split(new char[]
					{
						'/'
					});
					endpoint = array2[array2.Length - 1];
				}
				else
				{
					endpoint = text;
				}
				if (array.Length > 1)
				{
					foreach (string text2 in array[1].Split(new char[]
					{
						'&'
					}))
					{
						if (!string.IsNullOrWhiteSpace(text2))
						{
							string[] array4 = text2.Split(new char[]
							{
								'='
							});
							if (array4.Length == 2)
							{
								string key = array4[0].Trim();
								string value = Uri.UnescapeDataString(array4[1].Trim());
								queryParams[key] = value;
							}
							else if (array4.Length == 1 && text2.Contains(HoneyDataProcessor.ParseHoney.getString_0(107395726)))
							{
								string key2 = array4[0].Trim();
								queryParams[key2] = HoneyDataProcessor.ParseHoney.getString_0(107395824);
							}
						}
					}
				}
			}

			// Token: 0x0600004D RID: 77 RVA: 0x0002AAA1 File Offset: 0x00028CA1
			// Note: this type is marked as 'beforefieldinit'.
			static ParseHoney()
			{
				Strings.CreateGetStringDelegate(typeof(HoneyDataProcessor.ParseHoney));
				HoneyDataProcessor.ParseHoney.TimestampRegex = new Regex(HoneyDataProcessor.ParseHoney.getString_0(107395753), RegexOptions.Compiled);
			}

			// Token: 0x0400002B RID: 43
			private const byte XorKey = 85;

			// Token: 0x0400002C RID: 44
			private static readonly Regex TimestampRegex;

			// Token: 0x0400002D RID: 45
			[NonSerialized]
			internal static GetString getString_0;
		}

		// Token: 0x0200000E RID: 14
		public static class MyDataExtractor
		{
			// Token: 0x0600004E RID: 78 RVA: 0x0002C0BC File Offset: 0x0002A2BC
			public static void ExtractData(List<HoneyDataProcessor.LogItem> logs, out string INFO_ACTIVATION1, out string INFO_INIT2024, out string INFO_ACTIVATION2, out string INFO_FIRSTSERIAL, out string INFO_SECONDSERIAL, out string INFO_CHATGPT, out string INFO_COMM2024, out DateTime DATAORA_CLICK, out DateTime DATAORA_ACTIVATION1, out DateTime DATAORA_handshake, out DateTime DATAORA_INIT2024, out DateTime DATAORA_ACTIVATION2, out DateTime DATAORA_CHATGPT, out DateTime DATAORA_COMM2024)
			{
				INFO_ACTIVATION1 = null;
				INFO_INIT2024 = null;
				INFO_ACTIVATION2 = null;
				INFO_FIRSTSERIAL = null;
				INFO_SECONDSERIAL = null;
				INFO_CHATGPT = null;
				INFO_COMM2024 = null;
				DATAORA_CLICK = DateTime.MinValue;
				DATAORA_ACTIVATION1 = DateTime.MinValue;
				DATAORA_handshake = DateTime.MinValue;
				DATAORA_INIT2024 = DateTime.MinValue;
				DATAORA_ACTIVATION2 = DateTime.MinValue;
				DATAORA_CHATGPT = DateTime.MinValue;
				DATAORA_COMM2024 = DateTime.MinValue;
				if (logs != null && logs.Count != 0)
				{
					List<HoneyDataProcessor.LogItem> list = (from x in logs
					orderby x.Timestamp
					select x).ToList<HoneyDataProcessor.LogItem>();
					bool flag = false;
					for (int i = 0; i < list.Count; i++)
					{
						HoneyDataProcessor.LogItem logItem = list[i];
						if (logItem.IsRequest)
						{
							if (!flag)
							{
								DATAORA_CLICK = logItem.Timestamp;
								flag = true;
							}
							string a = (logItem.Endpoint ?? HoneyDataProcessor.MyDataExtractor.getString_0(107395825)).ToLowerInvariant();
							string text;
							string text2;
							if (!(a == HoneyDataProcessor.MyDataExtractor.getString_0(107395633)))
							{
								if (!(a == HoneyDataProcessor.MyDataExtractor.getString_0(107396124)))
								{
									if (!(a == HoneyDataProcessor.MyDataExtractor.getString_0(107396135)))
									{
										if (!(a == HoneyDataProcessor.MyDataExtractor.getString_0(107396086)))
										{
											if (a == HoneyDataProcessor.MyDataExtractor.getString_0(107396101))
											{
												DATAORA_COMM2024 = logItem.Timestamp;
												if (i + 1 < list.Count && list[i + 1].IsResponse)
												{
													INFO_COMM2024 = list[i + 1].BodyLine;
												}
											}
										}
										else
										{
											DATAORA_CHATGPT = logItem.Timestamp;
											if (i + 1 < list.Count && list[i + 1].IsResponse)
											{
												INFO_CHATGPT = list[i + 1].BodyLine;
											}
										}
									}
									else
									{
										DATAORA_INIT2024 = logItem.Timestamp;
										if (i + 1 < list.Count && list[i + 1].IsResponse)
										{
											INFO_INIT2024 = list[i + 1].BodyLine;
										}
									}
								}
								else
								{
									DATAORA_handshake = logItem.Timestamp;
								}
							}
							else if (string.IsNullOrEmpty(INFO_FIRSTSERIAL) && logItem.QueryParams.TryGetValue(HoneyDataProcessor.MyDataExtractor.getString_0(107396052), out text))
							{
								INFO_FIRSTSERIAL = text;
								DATAORA_ACTIVATION1 = logItem.Timestamp;
								if (i + 1 < list.Count && list[i + 1].IsResponse)
								{
									INFO_ACTIVATION1 = list[i + 1].ExtraLine;
								}
							}
							else if (string.IsNullOrEmpty(INFO_SECONDSERIAL) && logItem.QueryParams.TryGetValue(HoneyDataProcessor.MyDataExtractor.getString_0(107396052), out text2))
							{
								INFO_SECONDSERIAL = text2;
								DATAORA_ACTIVATION2 = logItem.Timestamp;
								if (i + 1 < list.Count && list[i + 1].IsResponse)
								{
									INFO_ACTIVATION2 = list[i + 1].ExtraLine;
								}
							}
						}
					}
					return;
				}
			}

			// Token: 0x0600004F RID: 79 RVA: 0x0002AACC File Offset: 0x00028CCC
			static MyDataExtractor()
			{
				Strings.CreateGetStringDelegate(typeof(HoneyDataProcessor.MyDataExtractor));
			}

			// Token: 0x0400002E RID: 46
			[NonSerialized]
			internal static GetString getString_0;
		}

		// Token: 0x02000010 RID: 16
		public sealed class LogItem
		{
			// Token: 0x17000016 RID: 22
			// (get) Token: 0x06000053 RID: 83 RVA: 0x0002AAF1 File Offset: 0x00028CF1
			// (set) Token: 0x06000054 RID: 84 RVA: 0x0002AAF9 File Offset: 0x00028CF9
			public bool IsRequest { get; set; }

			// Token: 0x17000017 RID: 23
			// (get) Token: 0x06000055 RID: 85 RVA: 0x0002AB02 File Offset: 0x00028D02
			// (set) Token: 0x06000056 RID: 86 RVA: 0x0002AB0A File Offset: 0x00028D0A
			public bool IsResponse { get; set; }

			// Token: 0x17000018 RID: 24
			// (get) Token: 0x06000057 RID: 87 RVA: 0x0002AB13 File Offset: 0x00028D13
			// (set) Token: 0x06000058 RID: 88 RVA: 0x0002AB1B File Offset: 0x00028D1B
			public DateTime Timestamp { get; set; }

			// Token: 0x17000019 RID: 25
			// (get) Token: 0x06000059 RID: 89 RVA: 0x0002AB24 File Offset: 0x00028D24
			// (set) Token: 0x0600005A RID: 90 RVA: 0x0002AB2C File Offset: 0x00028D2C
			public string FullUrl { get; set; }

			// Token: 0x1700001A RID: 26
			// (get) Token: 0x0600005B RID: 91 RVA: 0x0002AB35 File Offset: 0x00028D35
			// (set) Token: 0x0600005C RID: 92 RVA: 0x0002AB3D File Offset: 0x00028D3D
			public string Endpoint { get; set; }

			// Token: 0x1700001B RID: 27
			// (get) Token: 0x0600005D RID: 93 RVA: 0x0002AB46 File Offset: 0x00028D46
			// (set) Token: 0x0600005E RID: 94 RVA: 0x0002AB4E File Offset: 0x00028D4E
			public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			// Token: 0x1700001C RID: 28
			// (get) Token: 0x0600005F RID: 95 RVA: 0x0002AB57 File Offset: 0x00028D57
			// (set) Token: 0x06000060 RID: 96 RVA: 0x0002AB5F File Offset: 0x00028D5F
			public string BodyLine { get; set; }

			// Token: 0x1700001D RID: 29
			// (get) Token: 0x06000061 RID: 97 RVA: 0x0002AB68 File Offset: 0x00028D68
			// (set) Token: 0x06000062 RID: 98 RVA: 0x0002AB70 File Offset: 0x00028D70
			public string ExtraLine { get; set; }
		}
	}
}

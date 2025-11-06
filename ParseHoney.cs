using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using KT200Loader;
using SmartAssembly.Delegates;
using SmartAssembly.HouseOfCards;

// Token: 0x0200000D RID: 13
public static class ParseHoney
{
	// Token: 0x06000048 RID: 72 RVA: 0x00002E0C File Offset: 0x00002E0C
	public static List<HoneyDataProcessor.LogItem> DecryptAndParseAll(string honeyBinPath)
	{
		List<HoneyDataProcessor.LogItem> list = new List<HoneyDataProcessor.LogItem>();
		if (!File.Exists(honeyBinPath))
		{
			DebugLogger.Log(HoneyDataProcessor.ParseHoney.\u009A(107395866) + honeyBinPath);
			return list;
		}
		string[] array = File.ReadAllLines(honeyBinPath);
		List<string> list2 = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = HoneyDataProcessor.ParseHoney.DecryptWithXorAndBase64(array2[i]).Replace(HoneyDataProcessor.ParseHoney.\u009A(107395829), HoneyDataProcessor.ParseHoney.\u009A(107395824)).Split(new char[] { '\n' });
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
			string text2 = list2[k];
			Match match = HoneyDataProcessor.ParseHoney.TimestampRegex.Match(text2);
			if (match.Success)
			{
				HoneyDataProcessor.LogItem logItem = new HoneyDataProcessor.LogItem();
				string value = match.Groups[HoneyDataProcessor.ParseHoney.\u009A(107395823)].Value;
				logItem.IsRequest = value.Equals(HoneyDataProcessor.ParseHoney.\u009A(107395846), StringComparison.OrdinalIgnoreCase);
				logItem.IsResponse = !logItem.IsRequest;
				string value2 = match.Groups[HoneyDataProcessor.ParseHoney.\u009A(107395801)].Value;
				logItem.Timestamp = HoneyDataProcessor.ParseHoney.ExtractDateTime(value2);
				if (logItem.IsRequest)
				{
					if (k + 1 < list2.Count && list2[k + 1].StartsWith(HoneyDataProcessor.ParseHoney.\u009A(107395796), StringComparison.OrdinalIgnoreCase))
					{
						logItem.FullUrl = list2[k + 1].Substring(HoneyDataProcessor.ParseHoney.\u009A(107395796).Length).Trim();
						string text3;
						Dictionary<string, string> dictionary;
						HoneyDataProcessor.ParseHoney.ParseUrl(logItem.FullUrl, out text3, out dictionary);
						logItem.Endpoint = text3;
						logItem.QueryParams = dictionary;
					}
				}
				else if (k + 1 < list2.Count && list2[k + 1].StartsWith(HoneyDataProcessor.ParseHoney.\u009A(107395815), StringComparison.OrdinalIgnoreCase))
				{
					logItem.BodyLine = list2[k + 1].Substring(HoneyDataProcessor.ParseHoney.\u009A(107395815).Length).Trim();
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

	// Token: 0x06000049 RID: 73 RVA: 0x000030C0 File Offset: 0x000030C0
	private static string DecryptWithXorAndBase64(string base64Line)
	{
		string text;
		try
		{
			byte[] array = Convert.FromBase64String(base64Line);
			for (int i = 0; i < array.Length; i++)
			{
				byte[] array2 = array;
				int num = i;
				array2[num] ^= 85;
			}
			text = Encoding.UTF8.GetString(array);
		}
		catch
		{
			text = string.Empty;
		}
		return text;
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00003118 File Offset: 0x00003118
	private static DateTime ExtractDateTime(string tsRaw)
	{
		int num = tsRaw.IndexOf(HoneyDataProcessor.ParseHoney.\u009A(107395806), StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return DateTime.MinValue;
		}
		DateTime dateTime;
		DateTime.TryParseExact(tsRaw.Substring(0, num).Trim(), HoneyDataProcessor.ParseHoney.\u009A(107395765), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime);
		return dateTime;
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00003174 File Offset: 0x00003174
	private static bool IsNewSection(string line)
	{
		return line.StartsWith(HoneyDataProcessor.ParseHoney.\u009A(107395736), StringComparison.OrdinalIgnoreCase);
	}

	// Token: 0x0600004C RID: 76 RVA: 0x0000318C File Offset: 0x0000318C
	private static void ParseUrl(string url, out string endpoint, out Dictionary<string, string> queryParams)
	{
		endpoint = string.Empty;
		queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string[] array = url.Split(new char[] { '?' });
		string text = array[0];
		if (text.Contains(HoneyDataProcessor.ParseHoney.\u009A(107395731)))
		{
			string[] array2 = text.Split(new char[] { '/' });
			endpoint = array2[array2.Length - 1];
		}
		else
		{
			endpoint = text;
		}
		if (array.Length > 1)
		{
			foreach (string text2 in array[1].Split(new char[] { '&' }))
			{
				if (!string.IsNullOrWhiteSpace(text2))
				{
					string[] array4 = text2.Split(new char[] { '=' });
					if (array4.Length == 2)
					{
						string text3 = array4[0].Trim();
						string text4 = Uri.UnescapeDataString(array4[1].Trim());
						queryParams[text3] = text4;
					}
					else if (array4.Length == 1 && text2.Contains(HoneyDataProcessor.ParseHoney.\u009A(107395726)))
					{
						string text5 = array4[0].Trim();
						queryParams[text5] = HoneyDataProcessor.ParseHoney.\u009A(107395824);
					}
				}
			}
		}
	}

	// Token: 0x0600004D RID: 77 RVA: 0x000032C8 File Offset: 0x000032C8
	// Note: this type is marked as 'beforefieldinit'.
	static ParseHoney()
	{
		Strings.CreateGetStringDelegate(typeof(HoneyDataProcessor.ParseHoney));
		HoneyDataProcessor.ParseHoney.TimestampRegex = new Regex(HoneyDataProcessor.ParseHoney.\u009A(107395753), RegexOptions.Compiled);
	}

	// Token: 0x0400002B RID: 43
	private const byte XorKey = 85;

	// Token: 0x0400002C RID: 44
	private static readonly Regex TimestampRegex;

	// Token: 0x0400002D RID: 45
	[NonSerialized]
	internal static GetString \u009A;
}

using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using SmartAssembly.Delegates;
using SmartAssembly.HouseOfCards;

namespace KT200Loader
{
	// Token: 0x02000011 RID: 17
	public sealed class SharedMemoryManager
	{
		// Token: 0x06000064 RID: 100 RVA: 0x0002C404 File Offset: 0x0002A604
		public void ChangeDateAndTime(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, ushort millisecond)
		{
			try
			{
				using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen(SharedMemoryManager.getString_0(107396915), (long)Marshal.SizeOf<SharedMemoryManager.SYSTEMTIME>()))
				{
					using (MemoryMappedViewAccessor memoryMappedViewAccessor = memoryMappedFile.CreateViewAccessor())
					{
						SharedMemoryManager.SYSTEMTIME systemtime = new SharedMemoryManager.SYSTEMTIME
						{
							wYear = year,
							wMonth = month,
							wDay = day,
							wHour = hour,
							wMinute = minute,
							wSecond = second,
							wMilliseconds = millisecond
						};
						memoryMappedViewAccessor.Write<SharedMemoryManager.SYSTEMTIME>(0L, ref systemtime);
					}
				}
			}
			catch (Exception ex)
			{
				DebugLogger.Log(SharedMemoryManager.getString_0(107396087) + ex.Message);
				throw;
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x0002AB91 File Offset: 0x00028D91
		static SharedMemoryManager()
		{
			Strings.CreateGetStringDelegate(typeof(SharedMemoryManager));
		}

		// Token: 0x04000039 RID: 57
		public const string SharedMemoryName = "Local\\TimeHookSharedMemory";

		// Token: 0x0400003A RID: 58
		[NonSerialized]
		internal static GetString getString_0;

		// Token: 0x02000012 RID: 18
		public struct SYSTEMTIME
		{
			// Token: 0x0400003B RID: 59
			public ushort wYear;

			// Token: 0x0400003C RID: 60
			public ushort wMonth;

			// Token: 0x0400003D RID: 61
			public ushort wDayOfWeek;

			// Token: 0x0400003E RID: 62
			public ushort wDay;

			// Token: 0x0400003F RID: 63
			public ushort wHour;

			// Token: 0x04000040 RID: 64
			public ushort wMinute;

			// Token: 0x04000041 RID: 65
			public ushort wSecond;

			// Token: 0x04000042 RID: 66
			public ushort wMilliseconds;
		}
	}
}

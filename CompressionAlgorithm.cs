using System;

namespace SmartAssembly.Zip
{
	// Token: 0x02000039 RID: 57
	public enum CompressionAlgorithm
	{
		// Token: 0x040000BA RID: 186
		[Obsolete("Use `RawZip`.")]
		PKZip,
		// Token: 0x040000BB RID: 187
		RawZip,
		// Token: 0x040000BC RID: 188
		[Obsolete("Use `RawZipAndAes`.")]
		RawZipAndDes,
		// Token: 0x040000BD RID: 189
		RawZipAndAes
	}
}

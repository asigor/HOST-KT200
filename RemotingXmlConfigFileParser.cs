using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Threading;
using Microsoft.Win32;

namespace System.Runtime.InteropServices
{
	/// <summary>Provides a collection of methods for allocating unmanaged memory, copying unmanaged memory blocks, and converting managed to unmanaged types, as well as other miscellaneous methods used when interacting with unmanaged code.</summary>
	// Token: 0x02000950 RID: 2384
	[__DynamicallyInvokable]
	public static class Marshal
	{
		// Token: 0x060060C2 RID: 24770 RVA: 0x008800EC File Offset: 0x0087E6EC
		private static bool IsWin32Atom(IntPtr ptr)
		{
			long num = (long)ptr;
			return (num & -65536L) == 0L;
		}

		// Token: 0x060060C3 RID: 24771 RVA: 0x0088010C File Offset: 0x0087E70C
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static bool IsNotWin32Atom(IntPtr ptr)
		{
			long num = (long)ptr;
			return (num & -65536L) != 0L;
		}

		// Token: 0x060060C4 RID: 24772
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetSystemMaxDBCSCharSize();

		/// <summary>Copies all characters up to the first null character from an unmanaged ANSI string to a managed <see cref="T:System.String" />, and widens each ANSI character to Unicode.</summary>
		/// <param name="ptr">The address of the first character of the unmanaged string.</param>
		/// <returns>A managed string that holds a copy of the unmanaged ANSI string. If <paramref name="ptr" /> is <see langword="null" />, the method returns a null string.</returns>
		// Token: 0x060060C5 RID: 24773 RVA: 0x0088012C File Offset: 0x0087E72C
		[SecurityCritical]
		public unsafe static string PtrToStringAnsi(IntPtr ptr)
		{
			if (IntPtr.Zero == ptr)
			{
				return null;
			}
			if (Marshal.IsWin32Atom(ptr))
			{
				return null;
			}
			if (Win32Native.lstrlenA(ptr) == 0)
			{
				return string.Empty;
			}
			return new string((sbyte*)((void*)ptr));
		}

		/// <summary>Allocates a managed <see cref="T:System.String" />, copies a specified number of characters from an unmanaged ANSI string into it, and widens each ANSI character to Unicode.</summary>
		/// <param name="ptr">The address of the first character of the unmanaged string.</param>
		/// <param name="len">The byte count of the input string to copy.</param>
		/// <returns>A managed string that holds a copy of the native ANSI string if the value of the <paramref name="ptr" /> parameter is not <see langword="null" />; otherwise, this method returns <see langword="null" />.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="len" /> is less than zero.</exception>
		// Token: 0x060060C6 RID: 24774 RVA: 0x0088016D File Offset: 0x0087E76D
		[SecurityCritical]
		public unsafe static string PtrToStringAnsi(IntPtr ptr, int len)
		{
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException("ptr");
			}
			if (len < 0)
			{
				throw new ArgumentException("len");
			}
			return new string((sbyte*)((void*)ptr), 0, len);
		}

		/// <summary>Allocates a managed <see cref="T:System.String" /> and copies a specified number of characters from an unmanaged Unicode string into it.</summary>
		/// <param name="ptr">The address of the first character of the unmanaged string.</param>
		/// <param name="len">The number of Unicode characters to copy.</param>
		/// <returns>A managed string that holds a copy of the unmanaged string if the value of the <paramref name="ptr" /> parameter is not <see langword="null" />; otherwise, this method returns <see langword="null" />.</returns>
		// Token: 0x060060C7 RID: 24775 RVA: 0x008801A3 File Offset: 0x0087E7A3
		[SecurityCritical]
		public unsafe static string PtrToStringUni(IntPtr ptr, int len)
		{
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException("ptr");
			}
			if (len < 0)
			{
				throw new ArgumentException("len");
			}
			return new string((char*)((void*)ptr), 0, len);
		}

		/// <summary>Allocates a managed <see cref="T:System.String" /> and copies the specified number of characters from a string stored in unmanaged memory into it.</summary>
		/// <param name="ptr">For Unicode platforms, the address of the first Unicode character.  
		///  -or-  
		///  For ANSI plaforms, the address of the first ANSI character.</param>
		/// <param name="len">The number of characters to copy.</param>
		/// <returns>A managed string that holds a copy of the native string if the value of the <paramref name="ptr" /> parameter is not <see langword="null" />; otherwise, this method returns <see langword="null" />.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="len" /> is less than zero.</exception>
		// Token: 0x060060C8 RID: 24776 RVA: 0x0074DA42 File Offset: 0x0074C042
		[SecurityCritical]
		public static string PtrToStringAuto(IntPtr ptr, int len)
		{
			return Marshal.PtrToStringUni(ptr, len);
		}

		/// <summary>Allocates a managed <see cref="T:System.String" /> and copies all characters up to the first null character from an unmanaged Unicode string into it.</summary>
		/// <param name="ptr">The address of the first character of the unmanaged string.</param>
		/// <returns>A managed string that holds a copy of the unmanaged string if the value of the <paramref name="ptr" /> parameter is not <see langword="null" />; otherwise, this method returns <see langword="null" />.</returns>
		// Token: 0x060060C9 RID: 24777 RVA: 0x008801D9 File Offset: 0x0087E7D9
		[SecurityCritical]
		public unsafe static string PtrToStringUni(IntPtr ptr)
		{
			if (IntPtr.Zero == ptr)
			{
				return null;
			}
			if (Marshal.IsWin32Atom(ptr))
			{
				return null;
			}
			return new string((char*)((void*)ptr));
		}

		/// <summary>Allocates a managed <see cref="T:System.String" /> and copies all characters up to the first null character from a string stored in unmanaged memory into it.</summary>
		/// <param name="ptr">For Unicode platforms, the address of the first Unicode character.  
		///  -or-  
		///  For ANSI plaforms, the address of the first ANSI character.</param>
		/// <returns>A managed string that holds a copy of the unmanaged string if the value of the <paramref name="ptr" /> parameter is not <see langword="null" />; otherwise, this method returns <see langword="null" />.</returns>
		// Token: 0x060060CA RID: 24778 RVA: 0x0074DA4B File Offset: 0x0074C04B
		[SecurityCritical]
		public static string PtrToStringAuto(IntPtr ptr)
		{
			return Marshal.PtrToStringUni(ptr);
		}

		/// <summary>Returns the unmanaged size of an object in bytes.</summary>
		/// <param name="structure">The object whose size is to be returned.</param>
		/// <returns>The size of the specified object in unmanaged code.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="structure" /> parameter is <see langword="null" />.</exception>
		// Token: 0x060060CB RID: 24779 RVA: 0x008801FF File Offset: 0x0087E7FF
		[ComVisible(true)]
		public static int SizeOf(object structure)
		{
			if (structure == null)
			{
				throw new ArgumentNullException("structure");
			}
			return Marshal.SizeOfHelper(structure.GetType(), true);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Returns the unmanaged size of an object of a specified type in bytes.</summary>
		/// <param name="structure">The object whose size is to be returned.</param>
		/// <typeparam name="T">The type of the <paramref name="structure" /> parameter.</typeparam>
		/// <returns>The size, in bytes, of the specified object in unmanaged code.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="structure" /> parameter is <see langword="null" />.</exception>
		// Token: 0x060060CC RID: 24780 RVA: 0x0074DA53 File Offset: 0x0074C053
		public static int SizeOf<T>(T structure)
		{
			return Marshal.SizeOf(structure);
		}

		/// <summary>Returns the size of an unmanaged type in bytes.</summary>
		/// <param name="t">The type whose size is to be returned.</param>
		/// <returns>The size of the specified type in unmanaged code.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="t" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="t" /> parameter is <see langword="null" />.</exception>
		// Token: 0x060060CD RID: 24781 RVA: 0x0088021C File Offset: 0x0087E81C
		public static int SizeOf(Type t)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (!(t is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "t");
			}
			if (t.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
			}
			return Marshal.SizeOfHelper(t, true);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Returns the size of an unmanaged type in bytes.</summary>
		/// <typeparam name="T">The type whose size is to be returned.</typeparam>
		/// <returns>The size, in bytes, of the type that is specified by the <typeparamref name="T" /> generic type parameter.</returns>
		// Token: 0x060060CE RID: 24782 RVA: 0x0074DA60 File Offset: 0x0074C060
		public static int SizeOf<T>()
		{
			return Marshal.SizeOf(typeof(T));
		}

		// Token: 0x060060CF RID: 24783 RVA: 0x0077CD54 File Offset: 0x0077B354
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static uint AlignedSizeOf<T>() where T : struct
		{
			uint num = Marshal.SizeOfType(typeof(T));
			if (num == 1U || num == 2U)
			{
				return num;
			}
			if (IntPtr.Size == 8 && num == 4U)
			{
				return num;
			}
			return Marshal.AlignedSizeOfType(typeof(T));
		}

		// Token: 0x060060D0 RID: 24784
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern uint SizeOfType(Type type);

		// Token: 0x060060D1 RID: 24785
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern uint AlignedSizeOfType(Type type);

		// Token: 0x060060D2 RID: 24786
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int SizeOfHelper(Type t, bool throwIfNotMarshalable);

		/// <summary>Returns the field offset of the unmanaged form of the managed class.</summary>
		/// <param name="t">A value type or formatted reference type that specifies the managed class. You must apply the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> to the class.</param>
		/// <param name="fieldName">The field within the <paramref name="t" /> parameter.</param>
		/// <returns>The offset, in bytes, for the <paramref name="fieldName" /> parameter within the specified class that is declared by platform invoke.</returns>
		/// <exception cref="T:System.ArgumentException">The class cannot be exported as a structure or the field is nonpublic. Beginning with the .NET Framework version 2.0, the field may be private.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="t" /> parameter is <see langword="null" />.</exception>
		// Token: 0x060060D3 RID: 24787 RVA: 0x00880280 File Offset: 0x0087E880
		public static IntPtr OffsetOf(Type t, string fieldName)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			FieldInfo field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OffsetOfFieldNotFound", new object[]
				{
					t.FullName
				}), "fieldName");
			}
			RtFieldInfo rtFieldInfo = field as RtFieldInfo;
			if (rtFieldInfo == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"), "fieldName");
			}
			return Marshal.OffsetOfHelper(rtFieldInfo);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Returns the field offset of the unmanaged form of a specified managed class.</summary>
		/// <param name="fieldName">The name of the field in the <paramref name="T" /> type.</param>
		/// <typeparam name="T">A managed value type or formatted reference type. You must apply the <see cref="T:System.Runtime.InteropServices.StructLayoutAttribute" /> attribute to the class.</typeparam>
		/// <returns>The offset, in bytes, for the <paramref name="fieldName" /> parameter within the specified class that is declared by platform invoke.</returns>
		// Token: 0x060060D4 RID: 24788 RVA: 0x0074DA71 File Offset: 0x0074C071
		public static IntPtr OffsetOf<T>(string fieldName)
		{
			return Marshal.OffsetOf(typeof(T), fieldName);
		}

		// Token: 0x060060D5 RID: 24789
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr OffsetOfHelper(IRuntimeFieldInfo f);

		/// <summary>Gets the address of the element at the specified index inside the specified array.</summary>
		/// <param name="arr">The array that contains the desired element.</param>
		/// <param name="index">The index in the <paramref name="arr" /> parameter of the desired element.</param>
		/// <returns>The address of <paramref name="index" /> inside <paramref name="arr" />.</returns>
		// Token: 0x060060D6 RID: 24790
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr UnsafeAddrOfPinnedArrayElement(Array arr, int index);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Gets the address of the element at the specified index in an array of a specified type.</summary>
		/// <param name="arr">The array that contains the desired element.</param>
		/// <param name="index">The index of the desired element in the <paramref name="arr" /> array.</param>
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <returns>The address of <paramref name="index" /> in <paramref name="arr" />.</returns>
		// Token: 0x060060D7 RID: 24791 RVA: 0x0074DA83 File Offset: 0x0074C083
		[SecurityCritical]
		public static IntPtr UnsafeAddrOfPinnedArrayElement<T>(T[] arr, int index)
		{
			return Marshal.UnsafeAddrOfPinnedArrayElement(arr, index);
		}

		/// <summary>Copies data from a one-dimensional, managed 32-bit signed integer array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="startIndex" /> or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060D8 RID: 24792 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(int[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed character array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060D9 RID: 24793 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(char[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed 16-bit signed integer array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DA RID: 24794 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(short[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed 64-bit signed integer array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DB RID: 24795 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(long[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed single-precision floating-point number array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DC RID: 24796 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(float[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed double-precision floating-point number array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DD RID: 24797 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(double[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed 8-bit unsigned integer array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="startIndex" /> and <paramref name="length" /> are not valid.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="startIndex" />, <paramref name="destination" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DE RID: 24798 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(byte[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		/// <summary>Copies data from a one-dimensional, managed <see cref="T:System.IntPtr" /> array to an unmanaged memory pointer.</summary>
		/// <param name="source">The one-dimensional array to copy from.</param>
		/// <param name="startIndex">The zero-based index in the source array where copying should start.</param>
		/// <param name="destination">The memory pointer to copy to.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060DF RID: 24799 RVA: 0x0074DA8C File Offset: 0x0074C08C
		[SecurityCritical]
		public static void Copy(IntPtr[] source, int startIndex, IntPtr destination, int length)
		{
			Marshal.CopyToNative(source, startIndex, destination, length);
		}

		// Token: 0x060060E0 RID: 24800
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopyToNative(object source, int startIndex, IntPtr destination, int length);

		/// <summary>Copies data from an unmanaged memory pointer to a managed 32-bit signed integer array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E1 RID: 24801 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, int[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed character array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E2 RID: 24802 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, char[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed 16-bit signed integer array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E3 RID: 24803 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, short[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed 64-bit signed integer array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E4 RID: 24804 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, long[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed single-precision floating-point number array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E5 RID: 24805 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, float[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed double-precision floating-point number array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E6 RID: 24806 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, double[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed 8-bit unsigned integer array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E7 RID: 24807 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, byte[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		/// <summary>Copies data from an unmanaged memory pointer to a managed <see cref="T:System.IntPtr" /> array.</summary>
		/// <param name="source">The memory pointer to copy from.</param>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="startIndex">The zero-based index in the destination array where copying should start.</param>
		/// <param name="length">The number of array elements to copy.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="source" />, <paramref name="destination" />, <paramref name="startIndex" />, or <paramref name="length" /> is <see langword="null" />.</exception>
		// Token: 0x060060E8 RID: 24808 RVA: 0x0074DA97 File Offset: 0x0074C097
		[SecurityCritical]
		public static void Copy(IntPtr source, IntPtr[] destination, int startIndex, int length)
		{
			Marshal.CopyToManaged(source, destination, startIndex, length);
		}

		// Token: 0x060060E9 RID: 24809
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CopyToManaged(IntPtr source, object destination, int startIndex, int length);

		/// <summary>Reads a single byte at a given offset (or index) from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the source object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The byte read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060EA RID: 24810
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_RU1")]
		public static extern byte ReadByte([MarshalAs(UnmanagedType.AsAny)] [In] object ptr, int ofs);

		/// <summary>Reads a single byte at a given offset (or index) from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory from which to read.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The byte read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060EB RID: 24811 RVA: 0x00880304 File Offset: 0x0087E904
		[SecurityCritical]
		public unsafe static byte ReadByte(IntPtr ptr, int ofs)
		{
			byte result;
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				result = *ptr2;
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
			return result;
		}

		/// <summary>Reads a single byte from unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory from which to read.</param>
		/// <returns>The byte read from unmanaged memory.</returns>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060EC RID: 24812 RVA: 0x0074DAA2 File Offset: 0x0074C0A2
		[SecurityCritical]
		public static byte ReadByte(IntPtr ptr)
		{
			return Marshal.ReadByte(ptr, 0);
		}

		/// <summary>Reads a 16-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the source object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 16-bit signed integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060ED RID: 24813
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("mscoree.dll", EntryPoint = "ND_RI2")]
		public static extern short ReadInt16([MarshalAs(UnmanagedType.AsAny)] [In] object ptr, int ofs);

		/// <summary>Reads a 16-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory from which to read.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 16-bit signed integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060EE RID: 24814 RVA: 0x00880338 File Offset: 0x0087E938
		[SecurityCritical]
		public unsafe static short ReadInt16(IntPtr ptr, int ofs)
		{
			short result;
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 1) == 0)
				{
					result = *(short*)ptr2;
				}
				else
				{
					short num;
					byte* ptr3 = (byte*)(&num);
					*ptr3 = *ptr2;
					ptr3[1] = ptr2[1];
					result = num;
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
			return result;
		}

		/// <summary>Reads a 16-bit signed integer from unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory from which to read.</param>
		/// <returns>The 16-bit signed integer read from unmanaged memory.</returns>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060EF RID: 24815 RVA: 0x0074DAAB File Offset: 0x0074C0AB
		[SecurityCritical]
		public static short ReadInt16(IntPtr ptr)
		{
			return Marshal.ReadInt16(ptr, 0);
		}

		/// <summary>Reads a 32-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the source object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 32-bit signed integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060F0 RID: 24816
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_RI4")]
		public static extern int ReadInt32([MarshalAs(UnmanagedType.AsAny)] [In] object ptr, int ofs);

		/// <summary>Reads a 32-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory from which to read.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 32-bit signed integer read from unmanaged memory.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060F1 RID: 24817 RVA: 0x00880388 File Offset: 0x0087E988
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe static int ReadInt32(IntPtr ptr, int ofs)
		{
			int result;
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 3) == 0)
				{
					result = *(int*)ptr2;
				}
				else
				{
					int num;
					byte* ptr3 = (byte*)(&num);
					*ptr3 = *ptr2;
					ptr3[1] = ptr2[1];
					ptr3[2] = ptr2[2];
					ptr3[3] = ptr2[3];
					result = num;
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
			return result;
		}

		/// <summary>Reads a 32-bit signed integer from unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory from which to read.</param>
		/// <returns>The 32-bit signed integer read from unmanaged memory.</returns>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060F2 RID: 24818 RVA: 0x0074DAB4 File Offset: 0x0074C0B4
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityCritical]
		public static int ReadInt32(IntPtr ptr)
		{
			return Marshal.ReadInt32(ptr, 0);
		}

		/// <summary>Reads a processor native sized integer from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the source object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060F3 RID: 24819 RVA: 0x0074DABD File Offset: 0x0074C0BD
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityCritical]
		public static IntPtr ReadIntPtr([MarshalAs(UnmanagedType.AsAny)] [In] object ptr, int ofs)
		{
			return (IntPtr)Marshal.ReadInt64(ptr, ofs);
		}

		/// <summary>Reads a processor native sized integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory from which to read.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060F4 RID: 24820 RVA: 0x0074DACB File Offset: 0x0074C0CB
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static IntPtr ReadIntPtr(IntPtr ptr, int ofs)
		{
			return (IntPtr)Marshal.ReadInt64(ptr, ofs);
		}

		/// <summary>Reads a processor native-sized integer from unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory from which to read.</param>
		/// <returns>The integer read from unmanaged memory. A 32 bit integer is returned on 32 bit machines and a 64 bit integer is returned on 64 bit machines.</returns>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060F5 RID: 24821 RVA: 0x0074DAD9 File Offset: 0x0074C0D9
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static IntPtr ReadIntPtr(IntPtr ptr)
		{
			return (IntPtr)Marshal.ReadInt64(ptr, 0);
		}

		/// <summary>Reads a 64-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the source object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 64-bit signed integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060F6 RID: 24822
		[SuppressUnmanagedCodeSecurity]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_RI8")]
		public static extern long ReadInt64([MarshalAs(UnmanagedType.AsAny)] [In] object ptr, int ofs);

		/// <summary>Reads a 64-bit signed integer at a given offset from unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory from which to read.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before reading.</param>
		/// <returns>The 64-bit signed integer read from unmanaged memory at the given offset.</returns>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060F7 RID: 24823 RVA: 0x008803E8 File Offset: 0x0087E9E8
		[SecurityCritical]
		public unsafe static long ReadInt64(IntPtr ptr, int ofs)
		{
			long result;
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 7) == 0)
				{
					result = *(long*)ptr2;
				}
				else
				{
					long num;
					byte* ptr3 = (byte*)(&num);
					*ptr3 = *ptr2;
					ptr3[1] = ptr2[1];
					ptr3[2] = ptr2[2];
					ptr3[3] = ptr2[3];
					ptr3[4] = ptr2[4];
					ptr3[5] = ptr2[5];
					ptr3[6] = ptr2[6];
					ptr3[7] = ptr2[7];
					result = num;
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
			return result;
		}

		/// <summary>Reads a 64-bit signed integer from unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory from which to read.</param>
		/// <returns>The 64-bit signed integer read from unmanaged memory.</returns>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060F8 RID: 24824 RVA: 0x0074DAE7 File Offset: 0x0074C0E7
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityCritical]
		public static long ReadInt64(IntPtr ptr)
		{
			return Marshal.ReadInt64(ptr, 0);
		}

		/// <summary>Writes a single byte value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory to write to.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060F9 RID: 24825 RVA: 0x00880468 File Offset: 0x0087EA68
		[SecurityCritical]
		public unsafe static void WriteByte(IntPtr ptr, int ofs, byte val)
		{
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				*ptr2 = val;
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
		}

		/// <summary>Writes a single byte value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060FA RID: 24826
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("mscoree.dll", EntryPoint = "ND_WU1")]
		public static extern void WriteByte([MarshalAs(UnmanagedType.AsAny)] [In] [Out] object ptr, int ofs, byte val);

		/// <summary>Writes a single byte value to unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060FB RID: 24827 RVA: 0x0074DAF0 File Offset: 0x0074C0F0
		[SecurityCritical]
		public static void WriteByte(IntPtr ptr, byte val)
		{
			Marshal.WriteByte(ptr, 0, val);
		}

		/// <summary>Writes a 16-bit signed integer value into unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory to write to.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060FC RID: 24828 RVA: 0x0088049C File Offset: 0x0087EA9C
		[SecurityCritical]
		public unsafe static void WriteInt16(IntPtr ptr, int ofs, short val)
		{
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 1) == 0)
				{
					*(short*)ptr2 = val;
				}
				else
				{
					byte* ptr3 = (byte*)(&val);
					*ptr2 = *ptr3;
					ptr2[1] = ptr3[1];
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
		}

		/// <summary>Writes a 16-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x060060FD RID: 24829
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_WI2")]
		public static extern void WriteInt16([MarshalAs(UnmanagedType.AsAny)] [In] [Out] object ptr, int ofs, short val);

		/// <summary>Writes a 16-bit integer value to unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x060060FE RID: 24830 RVA: 0x0074DAFA File Offset: 0x0074C0FA
		[SecurityCritical]
		public static void WriteInt16(IntPtr ptr, short val)
		{
			Marshal.WriteInt16(ptr, 0, val);
		}

		/// <summary>Writes a 16-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in the native heap to write to.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x060060FF RID: 24831 RVA: 0x0074DB04 File Offset: 0x0074C104
		[SecurityCritical]
		public static void WriteInt16(IntPtr ptr, int ofs, char val)
		{
			Marshal.WriteInt16(ptr, ofs, (short)val);
		}

		/// <summary>Writes a 16-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x06006100 RID: 24832 RVA: 0x0074DB0F File Offset: 0x0074C10F
		[SecurityCritical]
		public static void WriteInt16([In] [Out] object ptr, int ofs, char val)
		{
			Marshal.WriteInt16(ptr, ofs, (short)val);
		}

		/// <summary>Writes a character as a 16-bit integer value to unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x06006101 RID: 24833 RVA: 0x0074DB1A File Offset: 0x0074C11A
		[SecurityCritical]
		public static void WriteInt16(IntPtr ptr, char val)
		{
			Marshal.WriteInt16(ptr, 0, (short)val);
		}

		/// <summary>Writes a 32-bit signed integer value into unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory to write to.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x06006102 RID: 24834 RVA: 0x008804E8 File Offset: 0x0087EAE8
		[SecurityCritical]
		public unsafe static void WriteInt32(IntPtr ptr, int ofs, int val)
		{
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 3) == 0)
				{
					*(int*)ptr2 = val;
				}
				else
				{
					byte* ptr3 = (byte*)(&val);
					*ptr2 = *ptr3;
					ptr2[1] = ptr3[1];
					ptr2[2] = ptr3[2];
					ptr2[3] = ptr3[3];
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
		}

		/// <summary>Writes a 32-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x06006103 RID: 24835
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_WI4")]
		public static extern void WriteInt32([MarshalAs(UnmanagedType.AsAny)] [In] [Out] object ptr, int ofs, int val);

		/// <summary>Writes a 32-bit signed integer value to unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x06006104 RID: 24836 RVA: 0x0074DB25 File Offset: 0x0074C125
		[SecurityCritical]
		public static void WriteInt32(IntPtr ptr, int val)
		{
			Marshal.WriteInt32(ptr, 0, val);
		}

		/// <summary>Writes a processor native-sized integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory to write to.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x06006105 RID: 24837 RVA: 0x0074DB2F File Offset: 0x0074C12F
		[SecurityCritical]
		public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val)
		{
			Marshal.WriteInt64(ptr, ofs, (long)val);
		}

		/// <summary>Writes a processor native sized integer value to unmanaged memory.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x06006106 RID: 24838 RVA: 0x0074DB3E File Offset: 0x0074C13E
		[SecurityCritical]
		public static void WriteIntPtr([MarshalAs(UnmanagedType.AsAny)] [In] [Out] object ptr, int ofs, IntPtr val)
		{
			Marshal.WriteInt64(ptr, ofs, (long)val);
		}

		/// <summary>Writes a processor native sized integer value into unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x06006107 RID: 24839 RVA: 0x0074DB4D File Offset: 0x0074C14D
		[SecurityCritical]
		public static void WriteIntPtr(IntPtr ptr, IntPtr val)
		{
			Marshal.WriteInt64(ptr, 0, (long)val);
		}

		/// <summary>Writes a 64-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory to write.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		// Token: 0x06006108 RID: 24840 RVA: 0x00880544 File Offset: 0x0087EB44
		[SecurityCritical]
		public unsafe static void WriteInt64(IntPtr ptr, int ofs, long val)
		{
			try
			{
				byte* ptr2 = (byte*)((void*)ptr) + ofs;
				if ((ptr2 & 7) == 0)
				{
					*(long*)ptr2 = val;
				}
				else
				{
					byte* ptr3 = (byte*)(&val);
					*ptr2 = *ptr3;
					ptr2[1] = ptr3[1];
					ptr2[2] = ptr3[2];
					ptr2[3] = ptr3[3];
					ptr2[4] = ptr3[4];
					ptr2[5] = ptr3[5];
					ptr2[6] = ptr3[6];
					ptr2[7] = ptr3[7];
				}
			}
			catch (NullReferenceException)
			{
				throw new AccessViolationException();
			}
		}

		/// <summary>Writes a 64-bit signed integer value to unmanaged memory at a specified offset.</summary>
		/// <param name="ptr">The base address in unmanaged memory of the target object.</param>
		/// <param name="ofs">An additional byte offset, which is added to the <paramref name="ptr" /> parameter before writing.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">Base address (<paramref name="ptr" />) plus offset byte (<paramref name="ofs" />) produces a null or invalid address.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="ptr" /> is an <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> object. This method does not accept <see cref="T:System.Runtime.InteropServices.ArrayWithOffset" /> parameters.</exception>
		// Token: 0x06006109 RID: 24841
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("mscoree.dll", EntryPoint = "ND_WI8")]
		public static extern void WriteInt64([MarshalAs(UnmanagedType.AsAny)] [In] [Out] object ptr, int ofs, long val);

		/// <summary>Writes a 64-bit signed integer value to unmanaged memory.</summary>
		/// <param name="ptr">The address in unmanaged memory to write to.</param>
		/// <param name="val">The value to write.</param>
		/// <exception cref="T:System.AccessViolationException">
		///   <paramref name="ptr" /> is not a recognized format.  
		/// -or-  
		/// <paramref name="ptr" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="ptr" /> is invalid.</exception>
		// Token: 0x0600610A RID: 24842 RVA: 0x0074DB5C File Offset: 0x0074C15C
		[SecurityCritical]
		public static void WriteInt64(IntPtr ptr, long val)
		{
			Marshal.WriteInt64(ptr, 0, val);
		}

		/// <summary>Returns the error code returned by the last unmanaged function that was called using platform invoke that has the <see cref="F:System.Runtime.InteropServices.DllImportAttribute.SetLastError" /> flag set.</summary>
		/// <returns>The last error code set by a call to the Win32 SetLastError function.</returns>
		// Token: 0x0600610B RID: 24843
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetLastWin32Error();

		// Token: 0x0600610C RID: 24844
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetLastWin32Error(int error);

		/// <summary>Returns the HRESULT corresponding to the last error incurred by Win32 code executed using <see cref="T:System.Runtime.InteropServices.Marshal" />.</summary>
		/// <returns>The HRESULT corresponding to the last Win32 error code.</returns>
		// Token: 0x0600610D RID: 24845 RVA: 0x008805C0 File Offset: 0x0087EBC0
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static int GetHRForLastWin32Error()
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (((long)lastWin32Error & (long)((ulong)-2147483648)) == (long)((ulong)-2147483648))
			{
				return lastWin32Error;
			}
			return (lastWin32Error & 65535) | -2147024896;
		}

		/// <summary>Executes one-time method setup tasks without calling the method.</summary>
		/// <param name="m">The method to be checked.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="m" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">The <paramref name="m" /> parameter is not a <see cref="T:System.Reflection.MethodInfo" /> object.</exception>
		// Token: 0x0600610E RID: 24846 RVA: 0x008805F4 File Offset: 0x0087EBF4
		[SecurityCritical]
		public static void Prelink(MethodInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			RuntimeMethodInfo runtimeMethodInfo = m as RuntimeMethodInfo;
			if (runtimeMethodInfo == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
			}
			Marshal.InternalPrelink(runtimeMethodInfo);
		}

		// Token: 0x0600610F RID: 24847
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void InternalPrelink(IRuntimeMethodInfo m);

		/// <summary>Performs a pre-link check for all methods on a class.</summary>
		/// <param name="c">The class whose methods are to be checked.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="c" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006110 RID: 24848 RVA: 0x0088063C File Offset: 0x0087EC3C
		[SecurityCritical]
		public static void PrelinkAll(Type c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c");
			}
			MethodInfo[] methods = c.GetMethods();
			if (methods != null)
			{
				for (int i = 0; i < methods.Length; i++)
				{
					Marshal.Prelink(methods[i]);
				}
			}
		}

		/// <summary>Calculates the number of bytes in unmanaged memory that are required to hold the parameters for the specified method.</summary>
		/// <param name="m">The method to be checked.</param>
		/// <returns>The number of bytes required to represent the method parameters in unmanaged memory.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="m" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">The <paramref name="m" /> parameter is not a <see cref="T:System.Reflection.MethodInfo" /> object.</exception>
		// Token: 0x06006111 RID: 24849 RVA: 0x00880680 File Offset: 0x0087EC80
		[SecurityCritical]
		public static int NumParamBytes(MethodInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			RuntimeMethodInfo runtimeMethodInfo = m as RuntimeMethodInfo;
			if (runtimeMethodInfo == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"));
			}
			return Marshal.InternalNumParamBytes(runtimeMethodInfo);
		}

		// Token: 0x06006112 RID: 24850
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern int InternalNumParamBytes(IRuntimeMethodInfo m);

		/// <summary>Retrieves a computer-independent description of an exception, and information about the state that existed for the thread when the exception occurred.</summary>
		/// <returns>A pointer to an EXCEPTION_POINTERS structure.</returns>
		// Token: 0x06006113 RID: 24851
		[SecurityCritical]
		[ComVisible(true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr GetExceptionPointers();

		/// <summary>Retrieves a code that identifies the type of the exception that occurred.</summary>
		/// <returns>The type of the exception.</returns>
		// Token: 0x06006114 RID: 24852
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetExceptionCode();

		/// <summary>Marshals data from a managed object to an unmanaged block of memory.</summary>
		/// <param name="structure">A managed object that holds the data to be marshaled. This object must be a structure or an instance of a formatted class.</param>
		/// <param name="ptr">A pointer to an unmanaged block of memory, which must be allocated before this method is called.</param>
		/// <param name="fDeleteOld">
		///   <see langword="true" /> to call the <see cref="M:System.Runtime.InteropServices.Marshal.DestroyStructure(System.IntPtr,System.Type)" /> method on the <paramref name="ptr" /> parameter before this method copies the data. The block must contain valid data. Note that passing <see langword="false" /> when the memory block already contains data can lead to a memory leak.</param>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="structure" /> is a reference type that is not a formatted class.  
		/// -or-  
		/// <paramref name="structure" /> is an instance of a generic type (in the .NET Framework 4.5 and earlier versions only).</exception>
		// Token: 0x06006115 RID: 24853
		[ComVisible(true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void StructureToPtr(object structure, IntPtr ptr, bool fDeleteOld);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Marshals data from a managed object of a specified type to an unmanaged block of memory.</summary>
		/// <param name="structure">A managed object that holds the data to be marshaled. The object must be a structure or an instance of a formatted class.</param>
		/// <param name="ptr">A pointer to an unmanaged block of memory, which must be allocated before this method is called.</param>
		/// <param name="fDeleteOld">
		///   <see langword="true" /> to call the <see cref="M:System.Runtime.InteropServices.Marshal.DestroyStructure``1(System.IntPtr)" /> method on the <paramref name="ptr" /> parameter before this method copies the data. The block must contain valid data. Note that passing <see langword="false" /> when the memory block already contains data can lead to a memory leak.</param>
		/// <typeparam name="T">The type of the managed object.</typeparam>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="structure" /> is a reference type that is not a formatted class.</exception>
		// Token: 0x06006116 RID: 24854 RVA: 0x0074DB66 File Offset: 0x0074C166
		[SecurityCritical]
		public static void StructureToPtr<T>(T structure, IntPtr ptr, bool fDeleteOld)
		{
			Marshal.StructureToPtr(structure, ptr, fDeleteOld);
		}

		/// <summary>Marshals data from an unmanaged block of memory to a managed object.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <param name="structure">The object to which the data is to be copied. This must be an instance of a formatted class.</param>
		/// <exception cref="T:System.ArgumentException">Structure layout is not sequential or explicit.  
		///  -or-  
		///  Structure is a boxed value type.</exception>
		// Token: 0x06006117 RID: 24855 RVA: 0x0074DB75 File Offset: 0x0074C175
		[ComVisible(true)]
		[SecurityCritical]
		public static void PtrToStructure(IntPtr ptr, object structure)
		{
			Marshal.PtrToStructureHelper(ptr, structure, false);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Marshals data from an unmanaged block of memory to a managed object of the specified type.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <param name="structure">The object to which the data is to be copied.</param>
		/// <typeparam name="T">The type of <paramref name="structure" />. This must be a formatted class.</typeparam>
		/// <exception cref="T:System.ArgumentException">Structure layout is not sequential or explicit.</exception>
		// Token: 0x06006118 RID: 24856 RVA: 0x0074DB7F File Offset: 0x0074C17F
		[SecurityCritical]
		public static void PtrToStructure<T>(IntPtr ptr, T structure)
		{
			Marshal.PtrToStructure(ptr, structure);
		}

		/// <summary>Marshals data from an unmanaged block of memory to a newly allocated managed object of the specified type.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <param name="structureType">The type of object to be created. This object must represent a formatted class or a structure.</param>
		/// <returns>A managed object containing the data pointed to by the <paramref name="ptr" /> parameter.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="structureType" /> parameter layout is not sequential or explicit.  
		///  -or-  
		///  The <paramref name="structureType" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="structureType" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.MissingMethodException">The class specified by <paramref name="structureType" /> does not have an accessible default constructor.</exception>
		// Token: 0x06006119 RID: 24857 RVA: 0x008806C8 File Offset: 0x0087ECC8
		[SecurityCritical]
		[ComVisible(true)]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static object PtrToStructure(IntPtr ptr, Type structureType)
		{
			if (ptr == IntPtr.Zero)
			{
				return null;
			}
			if (structureType == null)
			{
				throw new ArgumentNullException("structureType");
			}
			if (structureType.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "structureType");
			}
			RuntimeType runtimeType = structureType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
			}
			StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
			object obj = runtimeType.CreateInstanceDefaultCtor(false, false, false, ref stackCrawlMark);
			Marshal.PtrToStructureHelper(ptr, obj, true);
			return obj;
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Marshals data from an unmanaged block of memory to a newly allocated managed object of the type specified by a generic type parameter.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <typeparam name="T">The type of the object to which the data is to be copied. This must be a formatted class or a structure.</typeparam>
		/// <returns>A managed object that contains the data that the <paramref name="ptr" /> parameter points to.</returns>
		/// <exception cref="T:System.ArgumentException">The layout of <typeparamref name="T" /> is not sequential or explicit.</exception>
		/// <exception cref="T:System.MissingMethodException">The class specified by <typeparamref name="T" /> does not have an accessible default constructor.</exception>
		// Token: 0x0600611A RID: 24858 RVA: 0x0074DB8D File Offset: 0x0074C18D
		[SecurityCritical]
		public static T PtrToStructure<T>(IntPtr ptr)
		{
			return (T)((object)Marshal.PtrToStructure(ptr, typeof(T)));
		}

		// Token: 0x0600611B RID: 24859
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void PtrToStructureHelper(IntPtr ptr, object structure, bool allowValueClasses);

		/// <summary>Frees all substructures that the specified unmanaged memory block points to.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <param name="structuretype">Type of a formatted class. This provides the layout information necessary to delete the buffer in the <paramref name="ptr" /> parameter.</param>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="structureType" /> has an automatic layout. Use sequential or explicit instead.</exception>
		// Token: 0x0600611C RID: 24860
		[SecurityCritical]
		[ComVisible(true)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void DestroyStructure(IntPtr ptr, Type structuretype);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Frees all substructures of a specified type that the specified unmanaged memory block points to.</summary>
		/// <param name="ptr">A pointer to an unmanaged block of memory.</param>
		/// <typeparam name="T">The type of the formatted structure. This provides the layout information necessary to delete the buffer in the <paramref name="ptr" /> parameter.</typeparam>
		/// <exception cref="T:System.ArgumentException">
		///   <typeparamref name="T" /> has an automatic layout. Use sequential or explicit instead.</exception>
		// Token: 0x0600611D RID: 24861 RVA: 0x0074DBA4 File Offset: 0x0074C1A4
		[SecurityCritical]
		public static void DestroyStructure<T>(IntPtr ptr)
		{
			Marshal.DestroyStructure(ptr, typeof(T));
		}

		/// <summary>Returns the instance handle (HINSTANCE) for the specified module.</summary>
		/// <param name="m">The module whose HINSTANCE is desired.</param>
		/// <returns>The HINSTANCE for <paramref name="m" />; or -1 if the module does not have an HINSTANCE.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="m" /> parameter is <see langword="null" />.</exception>
		// Token: 0x0600611E RID: 24862 RVA: 0x00880758 File Offset: 0x0087ED58
		[SecurityCritical]
		public static IntPtr GetHINSTANCE(Module m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			RuntimeModule runtimeModule = m as RuntimeModule;
			if (runtimeModule == null)
			{
				ModuleBuilder moduleBuilder = m as ModuleBuilder;
				if (moduleBuilder != null)
				{
					runtimeModule = moduleBuilder.InternalModule;
				}
			}
			if (runtimeModule == null)
			{
				throw new ArgumentNullException(Environment.GetResourceString("Argument_MustBeRuntimeModule"));
			}
			return Marshal.GetHINSTANCE(runtimeModule.GetNativeHandle());
		}

		// Token: 0x0600611F RID: 24863
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetHINSTANCE(RuntimeModule m);

		/// <summary>Throws an exception with a specific failure HRESULT value.</summary>
		/// <param name="errorCode">The HRESULT corresponding to the desired exception.</param>
		// Token: 0x06006120 RID: 24864 RVA: 0x0074DBB6 File Offset: 0x0074C1B6
		[SecurityCritical]
		public static void ThrowExceptionForHR(int errorCode)
		{
			if (errorCode < 0)
			{
				Marshal.ThrowExceptionForHRInternal(errorCode, IntPtr.Zero);
			}
		}

		/// <summary>Throws an exception with a specific failure HRESULT, based on the specified IErrorInfo.aspx) interface.</summary>
		/// <param name="errorCode">The HRESULT corresponding to the desired exception.</param>
		/// <param name="errorInfo">A pointer to the IErrorInfo interface that provides more information about the error. You can specify IntPtr(0) to use the current IErrorInfo interface, or IntPtr(-1) to ignore the current IErrorInfo interface and construct the exception just from the error code.</param>
		// Token: 0x06006121 RID: 24865 RVA: 0x0074DBC7 File Offset: 0x0074C1C7
		[SecurityCritical]
		public static void ThrowExceptionForHR(int errorCode, IntPtr errorInfo)
		{
			if (errorCode < 0)
			{
				Marshal.ThrowExceptionForHRInternal(errorCode, errorInfo);
			}
		}

		// Token: 0x06006122 RID: 24866
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ThrowExceptionForHRInternal(int errorCode, IntPtr errorInfo);

		/// <summary>Converts the specified HRESULT error code to a corresponding <see cref="T:System.Exception" /> object.</summary>
		/// <param name="errorCode">The HRESULT to be converted.</param>
		/// <returns>An object that represents the converted HRESULT.</returns>
		// Token: 0x06006123 RID: 24867 RVA: 0x0074DBD4 File Offset: 0x0074C1D4
		[SecurityCritical]
		public static Exception GetExceptionForHR(int errorCode)
		{
			if (errorCode < 0)
			{
				return Marshal.GetExceptionForHRInternal(errorCode, IntPtr.Zero);
			}
			return null;
		}

		/// <summary>Converts the specified HRESULT error code to a corresponding <see cref="T:System.Exception" /> object, with additional error information passed in an IErrorInfo interface for the exception object.</summary>
		/// <param name="errorCode">The HRESULT to be converted.</param>
		/// <param name="errorInfo">A pointer to the <see langword="IErrorInfo" /> interface that provides more information about the error. You can specify IntPtr(0) to use the current <see langword="IErrorInfo" /> interface, or IntPtr(-1) to ignore the current <see langword="IErrorInfo" /> interface and construct the exception just from the error code.</param>
		/// <returns>An object that represents the converted HRESULT and information obtained from <paramref name="errorInfo" />.</returns>
		// Token: 0x06006124 RID: 24868 RVA: 0x0074DBE7 File Offset: 0x0074C1E7
		[SecurityCritical]
		public static Exception GetExceptionForHR(int errorCode, IntPtr errorInfo)
		{
			if (errorCode < 0)
			{
				return Marshal.GetExceptionForHRInternal(errorCode, errorInfo);
			}
			return null;
		}

		// Token: 0x06006125 RID: 24869
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Exception GetExceptionForHRInternal(int errorCode, IntPtr errorInfo);

		/// <summary>Converts the specified exception to an HRESULT.</summary>
		/// <param name="e">The exception to convert to an HRESULT.</param>
		/// <returns>The HRESULT mapped to the supplied exception.</returns>
		// Token: 0x06006126 RID: 24870
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetHRForException(Exception e);

		// Token: 0x06006127 RID: 24871
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int GetHRForException_WinRT(Exception e);

		/// <summary>Gets a pointer to a runtime-generated function that marshals a call from unmanaged to managed code.</summary>
		/// <param name="pfnMethodToWrap">A pointer to the method to marshal.</param>
		/// <param name="pbSignature">A pointer to the method signature.</param>
		/// <param name="cbSignature">The number of bytes in <paramref name="pbSignature" />.</param>
		/// <returns>A pointer to a function that will marshal a call from <paramref name="pfnMethodToWrap" /> to managed code.</returns>
		// Token: 0x06006128 RID: 24872
		[SecurityCritical]
		[Obsolete("The GetUnmanagedThunkForManagedMethodPtr method has been deprecated and will be removed in a future release.", false)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr GetUnmanagedThunkForManagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);

		/// <summary>Gets a pointer to a runtime-generated function that marshals a call from managed to unmanaged code.</summary>
		/// <param name="pfnMethodToWrap">A pointer to the method to marshal.</param>
		/// <param name="pbSignature">A pointer to the method signature.</param>
		/// <param name="cbSignature">The number of bytes in <paramref name="pbSignature" />.</param>
		/// <returns>A pointer to the function that will marshal a call from the <paramref name="pfnMethodToWrap" /> parameter to unmanaged code.</returns>
		// Token: 0x06006129 RID: 24873
		[Obsolete("The GetManagedThunkForUnmanagedMethodPtr method has been deprecated and will be removed in a future release.", false)]
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr GetManagedThunkForUnmanagedMethodPtr(IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature);

		/// <summary>Converts a fiber cookie into the corresponding <see cref="T:System.Threading.Thread" /> instance.</summary>
		/// <param name="cookie">An integer that represents a fiber cookie.</param>
		/// <returns>A thread that corresponds to the <paramref name="cookie" /> parameter.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="cookie" /> parameter is 0.</exception>
		// Token: 0x0600612A RID: 24874 RVA: 0x008807C4 File Offset: 0x0087EDC4
		[Obsolete("The GetThreadFromFiberCookie method has been deprecated.  Use the hosting API to perform this operation.", false)]
		[SecurityCritical]
		public static Thread GetThreadFromFiberCookie(int cookie)
		{
			if (cookie == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ArgumentZero"), "cookie");
			}
			return Marshal.InternalGetThreadFromFiberCookie(cookie);
		}

		// Token: 0x0600612B RID: 24875
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Thread InternalGetThreadFromFiberCookie(int cookie);

		/// <summary>Allocates memory from the unmanaged memory of the process by using the pointer to the specified number of bytes.</summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns>A pointer to the newly allocated memory. This memory must be released using the <see cref="M:System.Runtime.InteropServices.Marshal.FreeHGlobal(System.IntPtr)" /> method.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
		// Token: 0x0600612C RID: 24876 RVA: 0x008807E4 File Offset: 0x0087EDE4
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static IntPtr AllocHGlobal(IntPtr cb)
		{
			UIntPtr sizetdwBytes = new UIntPtr((ulong)cb.ToInt64());
			IntPtr intPtr = Win32Native.LocalAlloc_NoSafeHandle(0, sizetdwBytes);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		/// <summary>Allocates memory from the unmanaged memory of the process by using the specified number of bytes.</summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns>A pointer to the newly allocated memory. This memory must be released using the <see cref="M:System.Runtime.InteropServices.Marshal.FreeHGlobal(System.IntPtr)" /> method.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
		// Token: 0x0600612D RID: 24877 RVA: 0x0074DBF6 File Offset: 0x0074C1F6
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityCritical]
		public static IntPtr AllocHGlobal(int cb)
		{
			return Marshal.AllocHGlobal((IntPtr)cb);
		}

		/// <summary>Frees memory previously allocated from the unmanaged memory of the process.</summary>
		/// <param name="hglobal">The handle returned by the original matching call to <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.IntPtr)" />.</param>
		// Token: 0x0600612E RID: 24878 RVA: 0x0074DC03 File Offset: 0x0074C203
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SecurityCritical]
		public static void FreeHGlobal(IntPtr hglobal)
		{
			if (Marshal.IsNotWin32Atom(hglobal) && IntPtr.Zero != Win32Native.LocalFree(hglobal))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		/// <summary>Resizes a block of memory previously allocated with <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.IntPtr)" />.</summary>
		/// <param name="pv">A pointer to memory allocated with <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.IntPtr)" />.</param>
		/// <param name="cb">The new size of the allocated block. This is not a pointer; it is the byte count you are requesting, cast to type <see cref="T:System.IntPtr" />. If you pass a pointer, it is treated as a size.</param>
		/// <returns>A pointer to the reallocated memory. This memory must be released using <see cref="M:System.Runtime.InteropServices.Marshal.FreeHGlobal(System.IntPtr)" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
		// Token: 0x0600612F RID: 24879 RVA: 0x0088081C File Offset: 0x0087EE1C
		[SecurityCritical]
		public static IntPtr ReAllocHGlobal(IntPtr pv, IntPtr cb)
		{
			IntPtr intPtr = Win32Native.LocalReAlloc(pv, cb, 2);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory, converting into ANSI format as it copies.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>The address, in unmanaged memory, to where <paramref name="s" /> was copied, or 0 if <paramref name="s" /> is <see langword="null" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="s" /> parameter exceeds the maximum length allowed by the operating system.</exception>
		// Token: 0x06006130 RID: 24880 RVA: 0x00880848 File Offset: 0x0087EE48
		[SecurityCritical]
		public unsafe static IntPtr StringToHGlobalAnsi(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			int num = (s.Length + 1) * Marshal.SystemMaxDBCSCharSize;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			UIntPtr sizetdwBytes = new UIntPtr((uint)num);
			IntPtr intPtr = Win32Native.LocalAlloc_NoSafeHandle(0, sizetdwBytes);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			s.ConvertToAnsi((byte*)((void*)intPtr), num, false, false);
			return intPtr;
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>The address, in unmanaged memory, to where the <paramref name="s" /> was copied, or 0 if <paramref name="s" /> is <see langword="null" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">The method could not allocate enough native heap memory.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="s" /> parameter exceeds the maximum length allowed by the operating system.</exception>
		// Token: 0x06006131 RID: 24881 RVA: 0x008808B8 File Offset: 0x0087EEB8
		[SecurityCritical]
		public unsafe static IntPtr StringToHGlobalUni(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			int num = (s.Length + 1) * 2;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			UIntPtr sizetdwBytes = new UIntPtr((uint)num);
			IntPtr intPtr = Win32Native.LocalAlloc_NoSafeHandle(0, sizetdwBytes);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			fixed (string text = s)
			{
				char* ptr = text;
				if (ptr != null)
				{
					ptr += RuntimeHelpers.OffsetToStringData / 2;
				}
				string.wstrcpy((char*)((void*)intPtr), ptr, s.Length + 1);
			}
			return intPtr;
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> into unmanaged memory, converting into ANSI format if required.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>The address, in unmanaged memory, to where the string was copied, or 0 if <paramref name="s" /> is <see langword="null" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x06006132 RID: 24882 RVA: 0x0074DC29 File Offset: 0x0074C229
		[SecurityCritical]
		public static IntPtr StringToHGlobalAuto(string s)
		{
			return Marshal.StringToHGlobalUni(s);
		}

		/// <summary>Retrieves the name of a type library.</summary>
		/// <param name="pTLB">The type library whose name is to be retrieved.</param>
		/// <returns>The name of the type library that the <paramref name="pTLB" /> parameter points to.</returns>
		// Token: 0x06006133 RID: 24883 RVA: 0x0074DC31 File Offset: 0x0074C231
		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibName(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityCritical]
		public static string GetTypeLibName(UCOMITypeLib pTLB)
		{
			return Marshal.GetTypeLibName((ITypeLib)pTLB);
		}

		/// <summary>Retrieves the name of a type library.</summary>
		/// <param name="typelib">The type library whose name is to be retrieved.</param>
		/// <returns>The name of the type library that the <paramref name="typelib" /> parameter points to.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="typelib" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006134 RID: 24884 RVA: 0x0088093C File Offset: 0x0087EF3C
		[SecurityCritical]
		public static string GetTypeLibName(ITypeLib typelib)
		{
			if (typelib == null)
			{
				throw new ArgumentNullException("typelib");
			}
			string result = null;
			string text = null;
			int num = 0;
			string text2 = null;
			typelib.GetDocumentation(-1, out result, out text, out num, out text2);
			return result;
		}

		// Token: 0x06006135 RID: 24885 RVA: 0x00880970 File Offset: 0x0087EF70
		[SecurityCritical]
		internal static string GetTypeLibNameInternal(ITypeLib typelib)
		{
			if (typelib == null)
			{
				throw new ArgumentNullException("typelib");
			}
			ITypeLib2 typeLib = typelib as ITypeLib2;
			if (typeLib != null)
			{
				Guid managedNameGuid = Marshal.ManagedNameGuid;
				object obj;
				try
				{
					typeLib.GetCustData(ref managedNameGuid, out obj);
				}
				catch (Exception)
				{
					obj = null;
				}
				if (obj != null && obj.GetType() == typeof(string))
				{
					string text = (string)obj;
					text = text.Trim();
					if (text.EndsWith(".DLL", StringComparison.OrdinalIgnoreCase))
					{
						text = text.Substring(0, text.Length - 4);
					}
					else if (text.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
					{
						text = text.Substring(0, text.Length - 4);
					}
					return text;
				}
			}
			return Marshal.GetTypeLibName(typelib);
		}

		/// <summary>Retrieves the library identifier (LIBID) of a type library.</summary>
		/// <param name="pTLB">The type library whose LIBID is to be retrieved.</param>
		/// <returns>The LIBID of the type library that the <paramref name="pTLB" /> parameter points to.</returns>
		// Token: 0x06006136 RID: 24886 RVA: 0x0074DC3E File Offset: 0x0074C23E
		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibGuid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityCritical]
		public static Guid GetTypeLibGuid(UCOMITypeLib pTLB)
		{
			return Marshal.GetTypeLibGuid((ITypeLib)pTLB);
		}

		/// <summary>Retrieves the library identifier (LIBID) of a type library.</summary>
		/// <param name="typelib">The type library whose LIBID is to be retrieved.</param>
		/// <returns>The LIBID of the specified type library.</returns>
		// Token: 0x06006137 RID: 24887 RVA: 0x0074DC4C File Offset: 0x0074C24C
		[SecurityCritical]
		public static Guid GetTypeLibGuid(ITypeLib typelib)
		{
			Guid result = default(Guid);
			Marshal.FCallGetTypeLibGuid(ref result, typelib);
			return result;
		}

		// Token: 0x06006138 RID: 24888
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeLibGuid(ref Guid result, ITypeLib pTLB);

		/// <summary>Retrieves the LCID of a type library.</summary>
		/// <param name="pTLB">The type library whose LCID is to be retrieved.</param>
		/// <returns>The LCID of the type library that the <paramref name="pTLB" /> parameter points to.</returns>
		// Token: 0x06006139 RID: 24889 RVA: 0x0074DC6A File Offset: 0x0074C26A
		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeLibLcid(ITypeLib pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityCritical]
		public static int GetTypeLibLcid(UCOMITypeLib pTLB)
		{
			return Marshal.GetTypeLibLcid((ITypeLib)pTLB);
		}

		/// <summary>Retrieves the LCID of a type library.</summary>
		/// <param name="typelib">The type library whose LCID is to be retrieved.</param>
		/// <returns>The LCID of the type library that the <paramref name="typelib" /> parameter points to.</returns>
		// Token: 0x0600613A RID: 24890
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetTypeLibLcid(ITypeLib typelib);

		// Token: 0x0600613B RID: 24891
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void GetTypeLibVersion(ITypeLib typeLibrary, out int major, out int minor);

		// Token: 0x0600613C RID: 24892 RVA: 0x0077CD98 File Offset: 0x0077B398
		[SecurityCritical]
		internal static Guid GetTypeInfoGuid(ITypeInfo typeInfo)
		{
			Guid result = default(Guid);
			Marshal.FCallGetTypeInfoGuid(ref result, typeInfo);
			return result;
		}

		// Token: 0x0600613D RID: 24893
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeInfoGuid(ref Guid result, ITypeInfo typeInfo);

		/// <summary>Retrieves the library identifier (LIBID) that is assigned to a type library when it was exported from the specified assembly.</summary>
		/// <param name="asm">The assembly from which the type library was exported.</param>
		/// <returns>The LIBID that is assigned to a type library when it is exported from the specified assembly.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="asm" /> is <see langword="null" />.</exception>
		// Token: 0x0600613E RID: 24894 RVA: 0x00880A2C File Offset: 0x0087F02C
		[SecurityCritical]
		public static Guid GetTypeLibGuidForAssembly(Assembly asm)
		{
			if (asm == null)
			{
				throw new ArgumentNullException("asm");
			}
			RuntimeAssembly runtimeAssembly = asm as RuntimeAssembly;
			if (runtimeAssembly == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "asm");
			}
			Guid result = default(Guid);
			Marshal.FCallGetTypeLibGuidForAssembly(ref result, runtimeAssembly);
			return result;
		}

		// Token: 0x0600613F RID: 24895
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGetTypeLibGuidForAssembly(ref Guid result, RuntimeAssembly asm);

		// Token: 0x06006140 RID: 24896
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetTypeLibVersionForAssembly(RuntimeAssembly inputAssembly, out int majorVersion, out int minorVersion);

		/// <summary>Retrieves the version number of a type library that will be exported from the specified assembly.</summary>
		/// <param name="inputAssembly">A managed assembly.</param>
		/// <param name="majorVersion">The major version number.</param>
		/// <param name="minorVersion">The minor version number.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="inputAssembly" /> is <see langword="null" />.</exception>
		// Token: 0x06006141 RID: 24897 RVA: 0x00880A84 File Offset: 0x0087F084
		[SecurityCritical]
		public static void GetTypeLibVersionForAssembly(Assembly inputAssembly, out int majorVersion, out int minorVersion)
		{
			if (inputAssembly == null)
			{
				throw new ArgumentNullException("inputAssembly");
			}
			RuntimeAssembly runtimeAssembly = inputAssembly as RuntimeAssembly;
			if (runtimeAssembly == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "inputAssembly");
			}
			Marshal._GetTypeLibVersionForAssembly(runtimeAssembly, out majorVersion, out minorVersion);
		}

		/// <summary>Retrieves the name of the type represented by an ITypeInfo object.</summary>
		/// <param name="pTI">An object that represents an <see langword="ITypeInfo" /> pointer.</param>
		/// <returns>The name of the type that the <paramref name="pTI" /> parameter points to.</returns>
		// Token: 0x06006142 RID: 24898 RVA: 0x0074DC77 File Offset: 0x0074C277
		[Obsolete("Use System.Runtime.InteropServices.Marshal.GetTypeInfoName(ITypeInfo pTLB) instead. http://go.microsoft.com/fwlink/?linkid=14202&ID=0000011.", false)]
		[SecurityCritical]
		public static string GetTypeInfoName(UCOMITypeInfo pTI)
		{
			return Marshal.GetTypeInfoName((ITypeInfo)pTI);
		}

		/// <summary>Retrieves the name of the type represented by an ITypeInfo object.</summary>
		/// <param name="typeInfo">An object that represents an <see langword="ITypeInfo" /> pointer.</param>
		/// <returns>The name of the type that the <paramref name="typeInfo" /> parameter points to.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="typeInfo" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006143 RID: 24899 RVA: 0x00880AD4 File Offset: 0x0087F0D4
		[SecurityCritical]
		public static string GetTypeInfoName(ITypeInfo typeInfo)
		{
			if (typeInfo == null)
			{
				throw new ArgumentNullException("typeInfo");
			}
			string result = null;
			string text = null;
			int num = 0;
			string text2 = null;
			typeInfo.GetDocumentation(-1, out result, out text, out num, out text2);
			return result;
		}

		// Token: 0x06006144 RID: 24900 RVA: 0x00880B08 File Offset: 0x0087F108
		[SecurityCritical]
		internal static string GetTypeInfoNameInternal(ITypeInfo typeInfo, out bool hasManagedName)
		{
			if (typeInfo == null)
			{
				throw new ArgumentNullException("typeInfo");
			}
			ITypeInfo2 typeInfo2 = typeInfo as ITypeInfo2;
			if (typeInfo2 != null)
			{
				Guid managedNameGuid = Marshal.ManagedNameGuid;
				object obj;
				try
				{
					typeInfo2.GetCustData(ref managedNameGuid, out obj);
				}
				catch (Exception)
				{
					obj = null;
				}
				if (obj != null && obj.GetType() == typeof(string))
				{
					hasManagedName = true;
					return (string)obj;
				}
			}
			hasManagedName = false;
			return Marshal.GetTypeInfoName(typeInfo);
		}

		// Token: 0x06006145 RID: 24901 RVA: 0x0077CDB8 File Offset: 0x0077B3B8
		[SecurityCritical]
		internal static string GetManagedTypeInfoNameInternal(ITypeLib typeLib, ITypeInfo typeInfo)
		{
			bool flag;
			string typeInfoNameInternal = Marshal.GetTypeInfoNameInternal(typeInfo, out flag);
			if (flag)
			{
				return typeInfoNameInternal;
			}
			return Marshal.GetTypeLibNameInternal(typeLib) + "." + typeInfoNameInternal;
		}

		// Token: 0x06006146 RID: 24902
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Type GetLoadedTypeForGUID(ref Guid guid);

		/// <summary>Converts an unmanaged ITypeInfo object into a managed <see cref="T:System.Type" /> object.</summary>
		/// <param name="piTypeInfo">The <see langword="ITypeInfo" /> interface to marshal.</param>
		/// <returns>A managed type that represents the unmanaged <see langword="ITypeInfo" /> object.</returns>
		// Token: 0x06006147 RID: 24903 RVA: 0x00880B84 File Offset: 0x0087F184
		[SecurityCritical]
		public static Type GetTypeForITypeInfo(IntPtr piTypeInfo)
		{
			ITypeInfo typeInfo = null;
			ITypeLib typeLib = null;
			Assembly assembly = null;
			int num = 0;
			if (piTypeInfo == IntPtr.Zero)
			{
				return null;
			}
			typeInfo = (ITypeInfo)Marshal.GetObjectForIUnknown(piTypeInfo);
			Guid typeInfoGuid = Marshal.GetTypeInfoGuid(typeInfo);
			Type type = Marshal.GetLoadedTypeForGUID(ref typeInfoGuid);
			if (type != null)
			{
				return type;
			}
			try
			{
				typeInfo.GetContainingTypeLib(out typeLib, out num);
			}
			catch (COMException)
			{
				typeLib = null;
			}
			if (typeLib != null)
			{
				AssemblyName assemblyNameFromTypelib = TypeLibConverter.GetAssemblyNameFromTypelib(typeLib, null, null, null, null, AssemblyNameFlags.None);
				string fullName = assemblyNameFromTypelib.FullName;
				Assembly[] assemblies = Thread.GetDomain().GetAssemblies();
				int num2 = assemblies.Length;
				for (int i = 0; i < num2; i++)
				{
					if (string.Compare(assemblies[i].FullName, fullName, StringComparison.Ordinal) == 0)
					{
						assembly = assemblies[i];
					}
				}
				if (assembly == null)
				{
					TypeLibConverter typeLibConverter = new TypeLibConverter();
					assembly = typeLibConverter.ConvertTypeLibToAssembly(typeLib, Marshal.GetTypeLibName(typeLib) + ".dll", TypeLibImporterFlags.None, new ImporterCallback(), null, null, null, null);
				}
				type = assembly.GetType(Marshal.GetManagedTypeInfoNameInternal(typeLib, typeInfo), true, false);
				if (type != null && !type.IsVisible)
				{
					type = null;
				}
			}
			else
			{
				type = typeof(object);
			}
			return type;
		}

		/// <summary>Returns the type associated with the specified class identifier (CLSID).</summary>
		/// <param name="clsid">The CLSID of the type to return.</param>
		/// <returns>
		///   <see langword="System.__ComObject" /> regardless of whether the CLSID is valid.</returns>
		// Token: 0x06006148 RID: 24904 RVA: 0x00734F92 File Offset: 0x00733592
		[SecuritySafeCritical]
		public static Type GetTypeFromCLSID(Guid clsid)
		{
			return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, false);
		}

		/// <summary>Returns a <see cref="T:System.Runtime.InteropServices.ComTypes.ITypeInfo" /> interface from a managed type.</summary>
		/// <param name="t">The type whose <see langword="ITypeInfo" /> interface is being requested.</param>
		/// <returns>A pointer to the <see langword="ITypeInfo" /> interface for the <paramref name="t" /> parameter.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> is not a visible type to COM.  
		/// -or-  
		/// <paramref name="t" /> is a Windows Runtime type.</exception>
		/// <exception cref="T:System.Runtime.InteropServices.COMException">A type library is registered for the assembly that contains the type, but the type definition cannot be found.</exception>
		// Token: 0x06006149 RID: 24905
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr GetITypeInfoForType(Type t);

		/// <summary>Returns an IUnknown interface from a managed object.</summary>
		/// <param name="o">The object whose <see langword="IUnknown" /> interface is requested.</param>
		/// <returns>The <see langword="IUnknown" /> pointer for the <paramref name="o" /> parameter.</returns>
		// Token: 0x0600614A RID: 24906 RVA: 0x0074DC84 File Offset: 0x0074C284
		[SecurityCritical]
		[__DynamicallyInvokable]
		public static IntPtr GetIUnknownForObject(object o)
		{
			return Marshal.GetIUnknownForObjectNative(o, false);
		}

		/// <summary>Returns an IUnknown interface from a managed object, if the caller is in the same context as that object.</summary>
		/// <param name="o">The object whose <see langword="IUnknown" /> interface is requested.</param>
		/// <returns>The <see langword="IUnknown" /> pointer for the specified object, or <see langword="null" /> if the caller is not in the same context as the specified object.</returns>
		// Token: 0x0600614B RID: 24907 RVA: 0x0074DC8D File Offset: 0x0074C28D
		[SecurityCritical]
		public static IntPtr GetIUnknownForObjectInContext(object o)
		{
			return Marshal.GetIUnknownForObjectNative(o, true);
		}

		// Token: 0x0600614C RID: 24908
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetIUnknownForObjectNative(object o, bool onlyInContext);

		// Token: 0x0600614D RID: 24909
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr GetRawIUnknownForComObjectNoAddRef(object o);

		/// <summary>Returns an IDispatch interface from a managed object.</summary>
		/// <param name="o">The object whose <see langword="IDispatch" /> interface is requested.</param>
		/// <returns>The <see langword="IDispatch" /> pointer for the <paramref name="o" /> parameter.</returns>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="o" /> does not support the requested interface.</exception>
		// Token: 0x0600614E RID: 24910 RVA: 0x0074DC96 File Offset: 0x0074C296
		[SecurityCritical]
		[__DynamicallyInvokable]
		public static IntPtr GetIDispatchForObject(object o)
		{
			return Marshal.GetIDispatchForObjectNative(o, false);
		}

		/// <summary>Returns an IDispatch interface pointer from a managed object, if the caller is in the same context as that object.</summary>
		/// <param name="o">The object whose <see langword="IDispatch" /> interface is requested.</param>
		/// <returns>The <see langword="IDispatch" /> interface pointer for the specified object, or <see langword="null" /> if the caller is not in the same context as the specified object.</returns>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="o" /> does not support the requested interface.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="o" /> is <see langword="null" />.</exception>
		// Token: 0x0600614F RID: 24911 RVA: 0x0074DC9F File Offset: 0x0074C29F
		[SecurityCritical]
		public static IntPtr GetIDispatchForObjectInContext(object o)
		{
			return Marshal.GetIDispatchForObjectNative(o, true);
		}

		// Token: 0x06006150 RID: 24912
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetIDispatchForObjectNative(object o, bool onlyInContext);

		/// <summary>Returns a pointer to an IUnknown interface that represents the specified interface on the specified object. Custom query interface access is enabled by default.</summary>
		/// <param name="o">The object that provides the interface.</param>
		/// <param name="T">The type of interface that is requested.</param>
		/// <returns>The interface pointer that represents the specified interface for the object.</returns>
		/// <exception cref="T:System.ArgumentException">The <typeparamref name="T" /> parameter is not an interface.  
		///  -or-  
		///  The type is not visible to COM.  
		///  -or-  
		///  The <typeparamref name="T" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.InvalidCastException">The <paramref name="o" /> parameter does not support the requested interface.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="o" /> parameter is <see langword="null" />.  
		///  -or-  
		///  The <typeparamref name="T" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006151 RID: 24913 RVA: 0x0074DCA8 File Offset: 0x0074C2A8
		[SecurityCritical]
		public static IntPtr GetComInterfaceForObject(object o, Type T)
		{
			return Marshal.GetComInterfaceForObjectNative(o, T, false, true);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Returns a pointer to an IUnknown interface that represents the specified interface on an object of the specified type. Custom query interface access is enabled by default.</summary>
		/// <param name="o">The object that provides the interface.</param>
		/// <typeparam name="T">The type of <paramref name="o" />.</typeparam>
		/// <typeparam name="TInterface">The type of interface to return.</typeparam>
		/// <returns>The interface pointer that represents the <paramref name="TInterface" /> interface.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="TInterface" /> parameter is not an interface.  
		///  -or-  
		///  The type is not visible to COM.  
		///  -or-  
		///  The <typeparamref name="T" /> parameter is an open generic type.</exception>
		/// <exception cref="T:System.InvalidCastException">The <paramref name="o" /> parameter does not support the <paramref name="TInterface" /> interface.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="o" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006152 RID: 24914 RVA: 0x0074DCB3 File Offset: 0x0074C2B3
		[SecurityCritical]
		public static IntPtr GetComInterfaceForObject<T, TInterface>(T o)
		{
			return Marshal.GetComInterfaceForObject(o, typeof(TInterface));
		}

		/// <summary>Returns a pointer to an IUnknown interface that represents the specified interface on the specified object. Custom query interface access is controlled by the specified customization mode.</summary>
		/// <param name="o">The object that provides the interface.</param>
		/// <param name="T">The type of interface that is requested.</param>
		/// <param name="mode">One of the enumeration values that indicates whether to apply an <see langword="IUnknown::QueryInterface" /> customization that is supplied by an <see cref="T:System.Runtime.InteropServices.ICustomQueryInterface" />.</param>
		/// <returns>The interface pointer that represents the interface for the object.</returns>
		/// <exception cref="T:System.ArgumentException">The <typeparamref name="T" /> parameter is not an interface.  
		///  -or-  
		///  The type is not visible to COM.  
		///  -or-  
		///  The <typeparamref name="T" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.InvalidCastException">The object <paramref name="o" /> does not support the requested interface.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="o" /> parameter is <see langword="null" />.  
		///  -or-  
		///  The <typeparamref name="T" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006153 RID: 24915 RVA: 0x0074DCCC File Offset: 0x0074C2CC
		[SecurityCritical]
		public static IntPtr GetComInterfaceForObject(object o, Type T, CustomQueryInterfaceMode mode)
		{
			bool fEnalbeCustomizedQueryInterface = mode == CustomQueryInterfaceMode.Allow;
			return Marshal.GetComInterfaceForObjectNative(o, T, false, fEnalbeCustomizedQueryInterface);
		}

		/// <summary>Returns an interface pointer that represents the specified interface for an object, if the caller is in the same context as that object.</summary>
		/// <param name="o">The object that provides the interface.</param>
		/// <param name="t">The type of interface that is requested.</param>
		/// <returns>The interface pointer specified by <paramref name="t" /> that represents the interface for the specified object, or <see langword="null" /> if the caller is not in the same context as the object.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> is not an interface.  
		/// -or-  
		/// The type is not visible to COM.</exception>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="o" /> does not support the requested interface.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="o" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="t" /> is <see langword="null" />.</exception>
		// Token: 0x06006154 RID: 24916 RVA: 0x0074DCEB File Offset: 0x0074C2EB
		[SecurityCritical]
		public static IntPtr GetComInterfaceForObjectInContext(object o, Type t)
		{
			return Marshal.GetComInterfaceForObjectNative(o, t, true, true);
		}

		// Token: 0x06006155 RID: 24917
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetComInterfaceForObjectNative(object o, Type t, bool onlyInContext, bool fEnalbeCustomizedQueryInterface);

		/// <summary>Returns an instance of a type that represents a COM object by a pointer to its IUnknown interface.</summary>
		/// <param name="pUnk">A pointer to the <see langword="IUnknown" /> interface.</param>
		/// <returns>An object that represents the specified unmanaged COM object.</returns>
		// Token: 0x06006156 RID: 24918
		[SecurityCritical]
		[__DynamicallyInvokable]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object GetObjectForIUnknown(IntPtr pUnk);

		/// <summary>Creates a unique Runtime Callable Wrapper (RCW) object for a given IUnknown interface.</summary>
		/// <param name="unknown">A managed pointer to an <see langword="IUnknown" /> interface.</param>
		/// <returns>A unique RCW for the specified <see langword="IUnknown" /> interface.</returns>
		// Token: 0x06006157 RID: 24919
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object GetUniqueObjectForIUnknown(IntPtr unknown);

		/// <summary>Returns a managed object of a specified type that represents a COM object.</summary>
		/// <param name="pUnk">A pointer to the <see langword="IUnknown" /> interface of the unmanaged object.</param>
		/// <param name="t">The type of the requested managed class.</param>
		/// <returns>An instance of the class corresponding to the <see cref="T:System.Type" /> object that represents the requested unmanaged COM object.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> is not attributed with <see cref="T:System.Runtime.InteropServices.ComImportAttribute" />.  
		/// -or-  
		/// <paramref name="t" /> is a Windows Runtime type.</exception>
		// Token: 0x06006158 RID: 24920
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object GetTypedObjectForIUnknown(IntPtr pUnk, Type t);

		/// <summary>Aggregates a managed object with the specified COM object.</summary>
		/// <param name="pOuter">The outer <see langword="IUnknown" /> pointer.</param>
		/// <param name="o">An object to aggregate.</param>
		/// <returns>The inner <see langword="IUnknown" /> pointer of the managed object.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="o" /> is a Windows Runtime object.</exception>
		// Token: 0x06006159 RID: 24921
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern IntPtr CreateAggregatedObject(IntPtr pOuter, object o);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Aggregates a managed object of the specified type with the specified COM object.</summary>
		/// <param name="pOuter">The outer IUnknown pointer.</param>
		/// <param name="o">The managed object to aggregate.</param>
		/// <typeparam name="T">The type of the managed object to aggregate.</typeparam>
		/// <returns>The inner IUnknown pointer of the managed object.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="o" /> is a Windows Runtime object.</exception>
		// Token: 0x0600615A RID: 24922 RVA: 0x0074DCF6 File Offset: 0x0074C2F6
		[SecurityCritical]
		public static IntPtr CreateAggregatedObject<T>(IntPtr pOuter, T o)
		{
			return Marshal.CreateAggregatedObject(pOuter, o);
		}

		/// <summary>Notifies the runtime to clean up all Runtime Callable Wrappers (RCWs) allocated in the current context.</summary>
		// Token: 0x0600615B RID: 24923
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void CleanupUnusedObjectsInCurrentContext();

		/// <summary>Indicates whether runtime callable wrappers (RCWs) from any context are available for cleanup.</summary>
		/// <returns>
		///   <see langword="true" /> if there are any RCWs available for cleanup; otherwise, <see langword="false" />.</returns>
		// Token: 0x0600615C RID: 24924
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool AreComObjectsAvailableForCleanup();

		/// <summary>Indicates whether a specified object represents a COM object.</summary>
		/// <param name="o">The object to check.</param>
		/// <returns>
		///   <see langword="true" /> if the <paramref name="o" /> parameter is a COM type; otherwise, <see langword="false" />.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="o" /> is <see langword="null" />.</exception>
		// Token: 0x0600615D RID: 24925
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsComObject(object o);

		/// <summary>Allocates a block of memory of specified size from the COM task memory allocator.</summary>
		/// <param name="cb">The size of the block of memory to be allocated.</param>
		/// <returns>An integer representing the address of the block of memory allocated. This memory must be released with <see cref="M:System.Runtime.InteropServices.Marshal.FreeCoTaskMem(System.IntPtr)" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
		// Token: 0x0600615E RID: 24926 RVA: 0x00880CB4 File Offset: 0x0087F2B4
		[SecurityCritical]
		public static IntPtr AllocCoTaskMem(int cb)
		{
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(new UIntPtr((uint)cb));
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> to a block of memory allocated from the unmanaged COM task allocator.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>An integer representing a pointer to the block of memory allocated for the string, or 0 if s is <see langword="null" />.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="s" /> parameter exceeds the maximum length allowed by the operating system.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x0600615F RID: 24927 RVA: 0x00880CE4 File Offset: 0x0087F2E4
		[SecurityCritical]
		public unsafe static IntPtr StringToCoTaskMemUni(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			int num = (s.Length + 1) * 2;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(new UIntPtr((uint)num));
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			fixed (string text = s)
			{
				char* ptr = text;
				if (ptr != null)
				{
					ptr += RuntimeHelpers.OffsetToStringData / 2;
				}
				string.wstrcpy((char*)((void*)intPtr), ptr, s.Length + 1);
			}
			return intPtr;
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> to a block of memory allocated from the unmanaged COM task allocator.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>The allocated memory block, or 0 if <paramref name="s" /> is <see langword="null" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The length for <paramref name="s" /> is out of range.</exception>
		// Token: 0x06006160 RID: 24928 RVA: 0x0074DD04 File Offset: 0x0074C304
		[SecurityCritical]
		public static IntPtr StringToCoTaskMemAuto(string s)
		{
			return Marshal.StringToCoTaskMemUni(s);
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.String" /> to a block of memory allocated from the unmanaged COM task allocator.</summary>
		/// <param name="s">A managed string to be copied.</param>
		/// <returns>An integer representing a pointer to the block of memory allocated for the string, or 0 if <paramref name="s" /> is <see langword="null" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="s" /> parameter exceeds the maximum length allowed by the operating system.</exception>
		// Token: 0x06006161 RID: 24929 RVA: 0x00880D60 File Offset: 0x0087F360
		[SecurityCritical]
		public unsafe static IntPtr StringToCoTaskMemAnsi(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			int num = (s.Length + 1) * Marshal.SystemMaxDBCSCharSize;
			if (num < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.CoTaskMemAlloc(new UIntPtr((uint)num));
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			s.ConvertToAnsi((byte*)((void*)intPtr), num, false, false);
			return intPtr;
		}

		/// <summary>Frees a block of memory allocated by the unmanaged COM task memory allocator.</summary>
		/// <param name="ptr">The address of the memory to be freed.</param>
		// Token: 0x06006162 RID: 24930 RVA: 0x0074DD0C File Offset: 0x0074C30C
		[SecurityCritical]
		public static void FreeCoTaskMem(IntPtr ptr)
		{
			if (Marshal.IsNotWin32Atom(ptr))
			{
				Win32Native.CoTaskMemFree(ptr);
			}
		}

		/// <summary>Resizes a block of memory previously allocated with <see cref="M:System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Int32)" />.</summary>
		/// <param name="pv">A pointer to memory allocated with <see cref="M:System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Int32)" />.</param>
		/// <param name="cb">The new size of the allocated block.</param>
		/// <returns>An integer representing the address of the reallocated block of memory. This memory must be released with <see cref="M:System.Runtime.InteropServices.Marshal.FreeCoTaskMem(System.IntPtr)" />.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to satisfy the request.</exception>
		// Token: 0x06006163 RID: 24931 RVA: 0x00880DCC File Offset: 0x0087F3CC
		[SecurityCritical]
		public static IntPtr ReAllocCoTaskMem(IntPtr pv, int cb)
		{
			IntPtr intPtr = Win32Native.CoTaskMemRealloc(pv, new UIntPtr((uint)cb));
			if (intPtr == IntPtr.Zero && cb != 0)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		/// <summary>Decrements the reference count of the Runtime Callable Wrapper (RCW) associated with the specified COM object.</summary>
		/// <param name="o">The COM object to release.</param>
		/// <returns>The new value of the reference count of the RCW associated with <paramref name="o" />. This value is typically zero since the RCW keeps just one reference to the wrapped COM object regardless of the number of managed clients calling it.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="o" /> is not a valid COM object.</exception>
		/// <exception cref="T:System.NullReferenceException">
		///   <paramref name="o" /> is <see langword="null" />.</exception>
		// Token: 0x06006164 RID: 24932 RVA: 0x00880E00 File Offset: 0x0087F400
		[SecurityCritical]
		public static int ReleaseComObject(object o)
		{
			__ComObject _ComObject = null;
			try
			{
				_ComObject = (__ComObject)o;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			return _ComObject.ReleaseSelf();
		}

		// Token: 0x06006165 RID: 24933
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int InternalReleaseComObject(object o);

		/// <summary>Releases all references to a Runtime Callable Wrapper (RCW) by setting its reference count to 0.</summary>
		/// <param name="o">The RCW to be released.</param>
		/// <returns>The new value of the reference count of the RCW associated with the <paramref name="o" /> parameter, which is 0 (zero) if the release is successful.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="o" /> is not a valid COM object.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="o" /> is <see langword="null" />.</exception>
		// Token: 0x06006166 RID: 24934 RVA: 0x00880E44 File Offset: 0x0087F444
		[SecurityCritical]
		public static int FinalReleaseComObject(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			__ComObject _ComObject = null;
			try
			{
				_ComObject = (__ComObject)o;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			_ComObject.FinalReleaseSelf();
			return 0;
		}

		// Token: 0x06006167 RID: 24935
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalFinalReleaseComObject(object o);

		/// <summary>Retrieves data that is referenced by the specified key from the specified COM object.</summary>
		/// <param name="obj">The COM object that contains the data that you want.</param>
		/// <param name="key">The key in the internal hash table of <paramref name="obj" /> to retrieve the data from.</param>
		/// <returns>The data represented by the <paramref name="key" /> parameter in the internal hash table of the <paramref name="obj" /> parameter.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="obj" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="key" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="obj" /> is not a COM object.  
		/// -or-  
		/// <paramref name="obj" /> is a Windows Runtime object.</exception>
		// Token: 0x06006168 RID: 24936 RVA: 0x00880E98 File Offset: 0x0087F498
		[SecurityCritical]
		public static object GetComObjectData(object obj, object key)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			__ComObject _ComObject = null;
			try
			{
				_ComObject = (__ComObject)obj;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
			}
			if (obj.GetType().IsWindowsRuntimeObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjIsWinRTObject"), "obj");
			}
			return _ComObject.GetData(key);
		}

		/// <summary>Sets data referenced by the specified key in the specified COM object.</summary>
		/// <param name="obj">The COM object in which to store the data.</param>
		/// <param name="key">The key in the internal hash table of the COM object in which to store the data.</param>
		/// <param name="data">The data to set.</param>
		/// <returns>
		///   <see langword="true" /> if the data was set successfully; otherwise, <see langword="false" />.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="obj" /> is <see langword="null" />.  
		/// -or-  
		/// <paramref name="key" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="obj" /> is not a COM object.  
		/// -or-  
		/// <paramref name="obj" /> is a Windows Runtime object.</exception>
		// Token: 0x06006169 RID: 24937 RVA: 0x00880F1C File Offset: 0x0087F51C
		[SecurityCritical]
		public static bool SetComObjectData(object obj, object key, object data)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			__ComObject _ComObject = null;
			try
			{
				_ComObject = (__ComObject)obj;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "obj");
			}
			if (obj.GetType().IsWindowsRuntimeObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjIsWinRTObject"), "obj");
			}
			return _ComObject.SetData(key, data);
		}

		/// <summary>Wraps the specified COM object in an object of the specified type.</summary>
		/// <param name="o">The object to be wrapped.</param>
		/// <param name="t">The type of wrapper to create.</param>
		/// <returns>The newly wrapped object that is an instance of the desired type.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> must derive from <see langword="__ComObject" />.  
		/// -or-  
		/// <paramref name="t" /> is a Windows Runtime type.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="t" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="o" /> cannot be converted to the destination type because it does not support all required interfaces.</exception>
		// Token: 0x0600616A RID: 24938 RVA: 0x00880FA0 File Offset: 0x0087F5A0
		[SecurityCritical]
		public static object CreateWrapperOfType(object o, Type t)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (!t.IsCOMObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeNotComObject"), "t");
			}
			if (t.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
			}
			if (t.IsWindowsRuntimeObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeIsWinRTType"), "t");
			}
			if (o == null)
			{
				return null;
			}
			if (!o.GetType().IsCOMObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjNotComObject"), "o");
			}
			if (o.GetType().IsWindowsRuntimeObject)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ObjIsWinRTObject"), "o");
			}
			if (o.GetType() == t)
			{
				return o;
			}
			object obj = Marshal.GetComObjectData(o, t);
			if (obj == null)
			{
				obj = Marshal.InternalCreateWrapperOfType(o, t);
				if (!Marshal.SetComObjectData(o, t, obj))
				{
					obj = Marshal.GetComObjectData(o, t);
				}
			}
			return obj;
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Wraps the specified COM object in an object of the specified type.</summary>
		/// <param name="o">The object to be wrapped.</param>
		/// <typeparam name="T">The type of object to wrap.</typeparam>
		/// <typeparam name="TWrapper">The type of object to return.</typeparam>
		/// <returns>The newly wrapped object.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <typeparamref name="T" /> must derive from <see langword="__ComObject" />.  
		/// -or-  
		/// <typeparamref name="T" /> is a Windows Runtime type.</exception>
		/// <exception cref="T:System.InvalidCastException">
		///   <paramref name="o" /> cannot be converted to the <paramref name="TWrapper" /> because it does not support all required interfaces.</exception>
		// Token: 0x0600616B RID: 24939 RVA: 0x0074DD1C File Offset: 0x0074C31C
		[SecurityCritical]
		public static TWrapper CreateWrapperOfType<T, TWrapper>(T o)
		{
			return (TWrapper)((object)Marshal.CreateWrapperOfType(o, typeof(TWrapper)));
		}

		// Token: 0x0600616C RID: 24940
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object InternalCreateWrapperOfType(object o, Type t);

		/// <summary>Releases the thread cache.</summary>
		// Token: 0x0600616D RID: 24941 RVA: 0x0072A3B0 File Offset: 0x007289B0
		[Obsolete("This API did not perform any operation and will be removed in future versions of the CLR.", false)]
		[SecurityCritical]
		public static void ReleaseThreadCache()
		{
		}

		/// <summary>Indicates whether a type is visible to COM clients.</summary>
		/// <param name="t">The type to check for COM visibility.</param>
		/// <returns>
		///   <see langword="true" /> if the type is visible to COM; otherwise, <see langword="false" />.</returns>
		// Token: 0x0600616E RID: 24942
		[SecuritySafeCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsTypeVisibleFromCom(Type t);

		/// <summary>Requests a pointer to a specified interface from a COM object.</summary>
		/// <param name="pUnk">The interface to be queried.</param>
		/// <param name="iid">The interface identifier (IID) of the requested interface.</param>
		/// <param name="ppv">When this method returns, contains a reference to the returned interface.</param>
		/// <returns>An HRESULT that indicates the success or failure of the call.</returns>
		// Token: 0x0600616F RID: 24943
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int QueryInterface(IntPtr pUnk, ref Guid iid, out IntPtr ppv);

		/// <summary>Increments the reference count on the specified interface.</summary>
		/// <param name="pUnk">The interface reference count to increment.</param>
		/// <returns>The new value of the reference count on the <paramref name="pUnk" /> parameter.</returns>
		// Token: 0x06006170 RID: 24944
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int AddRef(IntPtr pUnk);

		/// <summary>Decrements the reference count on the specified interface.</summary>
		/// <param name="pUnk">The interface to release.</param>
		/// <returns>The new value of the reference count on the interface specified by the <paramref name="pUnk" /> parameter.</returns>
		// Token: 0x06006171 RID: 24945
		[SecurityCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int Release(IntPtr pUnk);

		/// <summary>Frees a <see langword="BSTR" /> using the COM SysFreeString function.</summary>
		/// <param name="ptr">The address of the BSTR to be freed.</param>
		// Token: 0x06006172 RID: 24946 RVA: 0x0074DD38 File Offset: 0x0074C338
		[__DynamicallyInvokable]
		[SecurityCritical]
		public static void FreeBSTR(IntPtr ptr)
		{
			if (Marshal.IsNotWin32Atom(ptr))
			{
				Win32Native.SysFreeString(ptr);
			}
		}

		/// <summary>Allocates a BSTR and copies the contents of a managed <see cref="T:System.String" /> into it.</summary>
		/// <param name="s">The managed string to be copied.</param>
		/// <returns>An unmanaged pointer to the <see langword="BSTR" />, or 0 if <paramref name="s" /> is null.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The length for <paramref name="s" /> is out of range.</exception>
		// Token: 0x06006173 RID: 24947 RVA: 0x00881098 File Offset: 0x0087F698
		[SecurityCritical]
		[__DynamicallyInvokable]
		public static IntPtr StringToBSTR(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			if (s.Length + 1 < s.Length)
			{
				throw new ArgumentOutOfRangeException("s");
			}
			IntPtr intPtr = Win32Native.SysAllocStringLen(s, s.Length);
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException();
			}
			return intPtr;
		}

		/// <summary>Allocates a managed <see cref="T:System.String" /> and copies a binary string (BSTR) stored in unmanaged memory into it.</summary>
		/// <param name="ptr">The address of the first character of the unmanaged string.</param>
		/// <returns>A managed string that holds a copy of the unmanaged string.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="ptr" /> equals <see cref="F:System.IntPtr.Zero" />.</exception>
		// Token: 0x06006174 RID: 24948 RVA: 0x0074DD48 File Offset: 0x0074C348
		[__DynamicallyInvokable]
		[SecurityCritical]
		public static string PtrToStringBSTR(IntPtr ptr)
		{
			return Marshal.PtrToStringUni(ptr, (int)Win32Native.SysStringLen(ptr));
		}

		/// <summary>Converts an object to a COM VARIANT.</summary>
		/// <param name="obj">The object for which to get a COM VARIANT.</param>
		/// <param name="pDstNativeVariant">A pointer to receive the VARIANT that corresponds to the <paramref name="obj" /> parameter.</param>
		/// <exception cref="T:System.ArgumentException">The <paramref name="obj" /> parameter is an instance of a generic type.</exception>
		// Token: 0x06006175 RID: 24949
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void GetNativeVariantForObject(object obj, IntPtr pDstNativeVariant);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Converts an object of a specified type to a COM VARIANT.</summary>
		/// <param name="obj">The object for which to get a COM VARIANT.</param>
		/// <param name="pDstNativeVariant">A pointer to receive the VARIANT that corresponds to the <paramref name="obj" /> parameter.</param>
		/// <typeparam name="T">The type of the object to convert.</typeparam>
		// Token: 0x06006176 RID: 24950 RVA: 0x0074DD56 File Offset: 0x0074C356
		[SecurityCritical]
		public static void GetNativeVariantForObject<T>(T obj, IntPtr pDstNativeVariant)
		{
			Marshal.GetNativeVariantForObject(obj, pDstNativeVariant);
		}

		/// <summary>Converts a COM VARIANT to an object.</summary>
		/// <param name="pSrcNativeVariant">A pointer to a COM VARIANT.</param>
		/// <returns>An object that corresponds to the <paramref name="pSrcNativeVariant" /> parameter.</returns>
		/// <exception cref="T:System.Runtime.InteropServices.InvalidOleVariantTypeException">
		///   <paramref name="pSrcNativeVariant" /> is not a valid VARIANT type.</exception>
		/// <exception cref="T:System.NotSupportedException">
		///   <paramref name="pSrcNativeVariant" /> has an unsupported type.</exception>
		// Token: 0x06006177 RID: 24951
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object GetObjectForNativeVariant(IntPtr pSrcNativeVariant);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Converts a COM VARIANT to an object of a specified type.</summary>
		/// <param name="pSrcNativeVariant">A pointer to a COM VARIANT.</param>
		/// <typeparam name="T">The type to which to convert the COM VARIANT.</typeparam>
		/// <returns>An object of the specified type that corresponds to the <paramref name="pSrcNativeVariant" /> parameter.</returns>
		/// <exception cref="T:System.Runtime.InteropServices.InvalidOleVariantTypeException">
		///   <paramref name="pSrcNativeVariant" /> is not a valid VARIANT type.</exception>
		/// <exception cref="T:System.NotSupportedException">
		///   <paramref name="pSrcNativeVariant" /> has an unsupported type.</exception>
		// Token: 0x06006178 RID: 24952 RVA: 0x0074DD64 File Offset: 0x0074C364
		[SecurityCritical]
		public static T GetObjectForNativeVariant<T>(IntPtr pSrcNativeVariant)
		{
			return (T)((object)Marshal.GetObjectForNativeVariant(pSrcNativeVariant));
		}

		/// <summary>Converts an array of COM VARIANTs to an array of objects.</summary>
		/// <param name="aSrcNativeVariant">A pointer to the first element of an array of COM VARIANTs.</param>
		/// <param name="cVars">The count of COM VARIANTs in <paramref name="aSrcNativeVariant" />.</param>
		/// <returns>An object array that corresponds to <paramref name="aSrcNativeVariant" />.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="cVars" /> is a negative number.</exception>
		// Token: 0x06006179 RID: 24953
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern object[] GetObjectsForNativeVariants(IntPtr aSrcNativeVariant, int cVars);

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Converts an array of COM VARIANTs to an array of a specified type.</summary>
		/// <param name="aSrcNativeVariant">A pointer to the first element of an array of COM VARIANTs.</param>
		/// <param name="cVars">The count of COM VARIANTs in <paramref name="aSrcNativeVariant" />.</param>
		/// <typeparam name="T">The type of the array to return.</typeparam>
		/// <returns>An array of <typeparamref name="T" /> objects that corresponds to <paramref name="aSrcNativeVariant" />.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="cVars" /> is a negative number.</exception>
		// Token: 0x0600617A RID: 24954 RVA: 0x0074DD74 File Offset: 0x0074C374
		[SecurityCritical]
		public static T[] GetObjectsForNativeVariants<T>(IntPtr aSrcNativeVariant, int cVars)
		{
			object[] objectsForNativeVariants = Marshal.GetObjectsForNativeVariants(aSrcNativeVariant, cVars);
			T[] array = null;
			if (objectsForNativeVariants != null)
			{
				array = new T[objectsForNativeVariants.Length];
				Array.Copy(objectsForNativeVariants, array, objectsForNativeVariants.Length);
			}
			return array;
		}

		/// <summary>Gets the first slot in the virtual function table (v-table or VTBL) that contains user-defined methods.</summary>
		/// <param name="t">A type that represents an interface.</param>
		/// <returns>The first VTBL slot that contains user-defined methods. The first slot is 3 if the interface is based on IUnknown, and 7 if the interface is based on IDispatch.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> is not visible from COM.</exception>
		// Token: 0x0600617B RID: 24955
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetStartComSlot(Type t);

		/// <summary>Retrieves the last slot in the virtual function table (v-table or VTBL) of a type when exposed to COM.</summary>
		/// <param name="t">A type that represents an interface or class.</param>
		/// <returns>The last VTBL slot of the interface when exposed to COM. If the <paramref name="t" /> parameter is a class, the returned VTBL slot is the last slot in the interface that is generated from the class.</returns>
		// Token: 0x0600617C RID: 24956
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetEndComSlot(Type t);

		/// <summary>Retrieves a <see cref="T:System.Reflection.MemberInfo" /> object for the specified virtual function table (v-table or VTBL) slot.</summary>
		/// <param name="t">The type for which the <see cref="T:System.Reflection.MemberInfo" /> is to be retrieved.</param>
		/// <param name="slot">The VTBL slot.</param>
		/// <param name="memberType">On successful return, one of the enumeration values that specifies the type of the member.</param>
		/// <returns>The object that represents the member at the specified VTBL slot.</returns>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="t" /> is not visible from COM.</exception>
		// Token: 0x0600617D RID: 24957
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern MemberInfo GetMethodInfoForComSlot(Type t, int slot, ref ComMemberType memberType);

		/// <summary>Retrieves the virtual function table (v-table or VTBL) slot for a specified <see cref="T:System.Reflection.MemberInfo" /> type when that type is exposed to COM.</summary>
		/// <param name="m">An object that represents an interface method.</param>
		/// <returns>The VTBL slot <paramref name="m" /> identifier when it is exposed to COM.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="m" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.ArgumentException">The <paramref name="m" /> parameter is not a <see cref="T:System.Reflection.MemberInfo" /> object.  
		///  -or-  
		///  The <paramref name="m" /> parameter is not an interface method.</exception>
		// Token: 0x0600617E RID: 24958 RVA: 0x008810EC File Offset: 0x0087F6EC
		[SecurityCritical]
		public static int GetComSlotForMethodInfo(MemberInfo m)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			if (!(m is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "m");
			}
			if (!m.DeclaringType.IsInterface)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeInterfaceMethod"), "m");
			}
			if (m.DeclaringType.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "m");
			}
			return Marshal.InternalGetComSlotForMethodInfo((IRuntimeMethodInfo)m);
		}

		// Token: 0x0600617F RID: 24959
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int InternalGetComSlotForMethodInfo(IRuntimeMethodInfo m);

		/// <summary>Returns the globally unique identifier (GUID) for the specified type, or generates a GUID using the algorithm used by the Type Library Exporter (Tlbexp.exe).</summary>
		/// <param name="type">The type to generate a GUID for.</param>
		/// <returns>An identifier for the specified type.</returns>
		// Token: 0x06006180 RID: 24960 RVA: 0x0074DDA4 File Offset: 0x0074C3A4
		[SecurityCritical]
		public static Guid GenerateGuidForType(Type type)
		{
			Guid result = default(Guid);
			Marshal.FCallGenerateGuidForType(ref result, type);
			return result;
		}

		// Token: 0x06006181 RID: 24961
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallGenerateGuidForType(ref Guid result, Type type);

		/// <summary>Returns a programmatic identifier (ProgID) for the specified type.</summary>
		/// <param name="type">The type to get a ProgID for.</param>
		/// <returns>The ProgID of the specified type.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not a class that can be create by COM. The class must be public, have a public default constructor, and be COM visible.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006182 RID: 24962 RVA: 0x0088117C File Offset: 0x0087F77C
		[SecurityCritical]
		public static string GenerateProgIdForType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type.IsImport)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustNotBeComImport"), "type");
			}
			if (type.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
			}
			if (!RegistrationServices.TypeRequiresRegistrationHelper(type))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustBeComCreatable"), "type");
			}
			IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type);
			for (int i = 0; i < customAttributes.Count; i++)
			{
				if (customAttributes[i].Constructor.DeclaringType == typeof(ProgIdAttribute))
				{
					IList<CustomAttributeTypedArgument> constructorArguments = customAttributes[i].ConstructorArguments;
					string text = (string)constructorArguments[0].Value;
					if (text == null)
					{
						text = string.Empty;
					}
					return text;
				}
			}
			return type.FullName;
		}

		/// <summary>Gets an interface pointer identified by the specified moniker.</summary>
		/// <param name="monikerName">The moniker corresponding to the desired interface pointer.</param>
		/// <returns>An object containing a reference to the interface pointer identified by the <paramref name="monikerName" /> parameter. A moniker is a name, and in this case, the moniker is defined by an interface.</returns>
		/// <exception cref="T:System.Runtime.InteropServices.COMException">An unrecognized HRESULT was returned by the unmanaged <see langword="BindToMoniker" /> method.</exception>
		// Token: 0x06006183 RID: 24963 RVA: 0x00881268 File Offset: 0x0087F868
		[SecurityCritical]
		public static object BindToMoniker(string monikerName)
		{
			object result = null;
			IBindCtx pbc = null;
			Marshal.CreateBindCtx(0U, out pbc);
			IMoniker pmk = null;
			uint num;
			Marshal.MkParseDisplayName(pbc, monikerName, out num, out pmk);
			Marshal.BindMoniker(pmk, 0U, ref Marshal.IID_IUnknown, out result);
			return result;
		}

		/// <summary>Obtains a running instance of the specified object from the running object table (ROT).</summary>
		/// <param name="progID">The programmatic identifier (ProgID) of the object that was requested.</param>
		/// <returns>The object that was requested; otherwise <see langword="null" />. You can cast this object to any COM interface that it supports.</returns>
		/// <exception cref="T:System.Runtime.InteropServices.COMException">The object was not found.</exception>
		// Token: 0x06006184 RID: 24964 RVA: 0x008812A0 File Offset: 0x0087F8A0
		[SecurityCritical]
		public static object GetActiveObject(string progID)
		{
			object result = null;
			Guid guid;
			try
			{
				Marshal.CLSIDFromProgIDEx(progID, out guid);
			}
			catch (Exception)
			{
				Marshal.CLSIDFromProgID(progID, out guid);
			}
			Marshal.GetActiveObject(ref guid, IntPtr.Zero, out result);
			return result;
		}

		// Token: 0x06006185 RID: 24965
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		// Token: 0x06006186 RID: 24966
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		// Token: 0x06006187 RID: 24967
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CreateBindCtx(uint reserved, out IBindCtx ppbc);

		// Token: 0x06006188 RID: 24968
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void MkParseDisplayName(IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string szUserName, out uint pchEaten, out IMoniker ppmk);

		// Token: 0x06006189 RID: 24969
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void BindMoniker(IMoniker pmk, uint grfOpt, ref Guid iidResult, [MarshalAs(UnmanagedType.Interface)] out object ppvResult);

		// Token: 0x0600618A RID: 24970
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("oleaut32.dll", PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

		// Token: 0x0600618B RID: 24971
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool InternalSwitchCCW(object oldtp, object newtp);

		// Token: 0x0600618C RID: 24972
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object InternalWrapIUnknownWithComObject(IntPtr i);

		// Token: 0x0600618D RID: 24973 RVA: 0x008812E4 File Offset: 0x0087F8E4
		[SecurityCritical]
		private static IntPtr LoadLicenseManager()
		{
			Assembly assembly = Assembly.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			Type type = assembly.GetType("System.ComponentModel.LicenseManager");
			if (type == null || !type.IsVisible)
			{
				return IntPtr.Zero;
			}
			return type.TypeHandle.Value;
		}

		/// <summary>Changes the strength of an object's COM Callable Wrapper (CCW) handle.</summary>
		/// <param name="otp">The object whose CCW holds a reference counted handle. The handle is strong if the reference count on the CCW is greater than zero; otherwise, it is weak.</param>
		/// <param name="fIsWeak">
		///   <see langword="true" /> to change the strength of the handle on the <paramref name="otp" /> parameter to weak, regardless of its reference count; <see langword="false" /> to reset the handle strength on <paramref name="otp" /> to be reference counted.</param>
		// Token: 0x0600618E RID: 24974
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void ChangeWrapperHandleStrength(object otp, bool fIsWeak);

		// Token: 0x0600618F RID: 24975
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InitializeWrapperForWinRT(object o, ref IntPtr pUnk);

		// Token: 0x06006190 RID: 24976
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InitializeManagedWinRTFactoryObject(object o, RuntimeType runtimeClassType);

		// Token: 0x06006191 RID: 24977
		[SecurityCritical]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object GetNativeActivationFactory(Type type);

		// Token: 0x06006192 RID: 24978
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void _GetInspectableIids(ObjectHandleOnStack obj, ObjectHandleOnStack guids);

		// Token: 0x06006193 RID: 24979 RVA: 0x00881330 File Offset: 0x0087F930
		[SecurityCritical]
		internal static Guid[] GetInspectableIids(object obj)
		{
			Guid[] result = null;
			__ComObject _ComObject = obj as __ComObject;
			if (_ComObject != null)
			{
				Marshal._GetInspectableIids(JitHelpers.GetObjectHandleOnStack<__ComObject>(ref _ComObject), JitHelpers.GetObjectHandleOnStack<Guid[]>(ref result));
			}
			return result;
		}

		// Token: 0x06006194 RID: 24980
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void _GetCachedWinRTTypeByIid(ObjectHandleOnStack appDomainObj, Guid iid, out IntPtr rthHandle);

		// Token: 0x06006195 RID: 24981 RVA: 0x00881360 File Offset: 0x0087F960
		[SecurityCritical]
		internal static Type GetCachedWinRTTypeByIid(AppDomain ad, Guid iid)
		{
			IntPtr handle;
			Marshal._GetCachedWinRTTypeByIid(JitHelpers.GetObjectHandleOnStack<AppDomain>(ref ad), iid, out handle);
			return Type.GetTypeFromHandleUnsafe(handle);
		}

		// Token: 0x06006196 RID: 24982
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport("QCall", CharSet = CharSet.Unicode)]
		private static extern void _GetCachedWinRTTypes(ObjectHandleOnStack appDomainObj, ref int epoch, ObjectHandleOnStack winrtTypes);

		// Token: 0x06006197 RID: 24983 RVA: 0x00881384 File Offset: 0x0087F984
		[SecurityCritical]
		internal static Type[] GetCachedWinRTTypes(AppDomain ad, ref int epoch)
		{
			IntPtr[] array = null;
			Marshal._GetCachedWinRTTypes(JitHelpers.GetObjectHandleOnStack<AppDomain>(ref ad), ref epoch, JitHelpers.GetObjectHandleOnStack<IntPtr[]>(ref array));
			Type[] array2 = new Type[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Type.GetTypeFromHandleUnsafe(array[i]);
			}
			return array2;
		}

		// Token: 0x06006198 RID: 24984 RVA: 0x0077CDE4 File Offset: 0x0077B3E4
		[SecurityCritical]
		internal static Type[] GetCachedWinRTTypes(AppDomain ad)
		{
			int num = 0;
			return Marshal.GetCachedWinRTTypes(ad, ref num);
		}

		/// <summary>Converts an unmanaged function pointer to a delegate.</summary>
		/// <param name="ptr">The unmanaged function pointer to be converted.</param>
		/// <param name="t">The type of the delegate to be returned.</param>
		/// <returns>A delegate instance that can be cast to the appropriate delegate type.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="t" /> parameter is not a delegate or is generic.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="ptr" /> parameter is <see langword="null" />.  
		///  -or-  
		///  The <paramref name="t" /> parameter is <see langword="null" />.</exception>
		// Token: 0x06006199 RID: 24985 RVA: 0x008813CC File Offset: 0x0087F9CC
		[SecurityCritical]
		public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
		{
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException("ptr");
			}
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (t as RuntimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "t");
			}
			if (t.IsGenericType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "t");
			}
			Type baseType = t.BaseType;
			if (baseType == null || (baseType != typeof(Delegate) && baseType != typeof(MulticastDelegate)))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "t");
			}
			return Marshal.GetDelegateForFunctionPointerInternal(ptr, t);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Converts an unmanaged function pointer to a delegate of a specified type.</summary>
		/// <param name="ptr">The unmanaged function pointer to convert.</param>
		/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
		/// <returns>A instance of the specified delegate type.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="TDelegate" /> generic parameter is not a delegate, or it is an open generic type.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="ptr" /> parameter is <see langword="null" />.</exception>
		// Token: 0x0600619A RID: 24986 RVA: 0x0074DDC2 File Offset: 0x0074C3C2
		[SecurityCritical]
		public static TDelegate GetDelegateForFunctionPointer<TDelegate>(IntPtr ptr)
		{
			return (TDelegate)((object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(TDelegate)));
		}

		// Token: 0x0600619B RID: 24987
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Delegate GetDelegateForFunctionPointerInternal(IntPtr ptr, Type t);

		/// <summary>Converts a delegate into a function pointer that is callable from unmanaged code.</summary>
		/// <param name="d">The delegate to be passed to unmanaged code.</param>
		/// <returns>A value that can be passed to unmanaged code, which, in turn, can use it to call the underlying managed delegate.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="d" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="d" /> parameter is <see langword="null" />.</exception>
		// Token: 0x0600619C RID: 24988 RVA: 0x0074DDD9 File Offset: 0x0074C3D9
		[SecurityCritical]
		public static IntPtr GetFunctionPointerForDelegate(Delegate d)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}
			return Marshal.GetFunctionPointerForDelegateInternal(d);
		}

		/// <summary>[Supported in the .NET Framework 4.5.1 and later versions]  
		///  Converts a delegate of a specified type to a function pointer that is callable from unmanaged code.</summary>
		/// <param name="d">The delegate to be passed to unmanaged code.</param>
		/// <typeparam name="TDelegate">The type of delegate to convert.</typeparam>
		/// <returns>A value that can be passed to unmanaged code, which, in turn, can use it to call the underlying managed delegate.</returns>
		/// <exception cref="T:System.ArgumentException">The <paramref name="d" /> parameter is a generic type definition.</exception>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="d" /> parameter is <see langword="null" />.</exception>
		// Token: 0x0600619D RID: 24989 RVA: 0x0074DDEF File Offset: 0x0074C3EF
		[SecurityCritical]
		public static IntPtr GetFunctionPointerForDelegate<TDelegate>(TDelegate d)
		{
			return Marshal.GetFunctionPointerForDelegate((Delegate)((object)d));
		}

		// Token: 0x0600619E RID: 24990
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr GetFunctionPointerForDelegateInternal(Delegate d);

		/// <summary>Allocates an unmanaged binary string (BSTR) and copies the contents of a managed <see cref="T:System.Security.SecureString" /> object into it.</summary>
		/// <param name="s">The managed object to copy.</param>
		/// <returns>The address, in unmanaged memory, where the <paramref name="s" /> parameter was copied to, or 0 if a null object was supplied.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x0600619F RID: 24991 RVA: 0x0074DE01 File Offset: 0x0074C401
		[SecurityCritical]
		public static IntPtr SecureStringToBSTR(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToBSTR();
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.Security.SecureString" /> object to a block of memory allocated from the unmanaged COM task allocator.</summary>
		/// <param name="s">The managed object to copy.</param>
		/// <returns>The address, in unmanaged memory, where the <paramref name="s" /> parameter was copied to, or 0 if a null object was supplied.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x060061A0 RID: 24992 RVA: 0x0074DE17 File Offset: 0x0074C417
		[SecurityCritical]
		public static IntPtr SecureStringToCoTaskMemAnsi(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToAnsiStr(false);
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.Security.SecureString" /> object to a block of memory allocated from the unmanaged COM task allocator.</summary>
		/// <param name="s">The managed object to copy.</param>
		/// <returns>The address, in unmanaged memory, where the <paramref name="s" /> parameter was copied to, or 0 if a null object was supplied.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x060061A1 RID: 24993 RVA: 0x0074DE2E File Offset: 0x0074C42E
		[SecurityCritical]
		public static IntPtr SecureStringToCoTaskMemUnicode(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToUniStr(false);
		}

		/// <summary>Frees a BSTR pointer that was allocated using the <see cref="M:System.Runtime.InteropServices.Marshal.SecureStringToBSTR(System.Security.SecureString)" /> method.</summary>
		/// <param name="s">The address of the <see langword="BSTR" /> to free.</param>
		// Token: 0x060061A2 RID: 24994 RVA: 0x00881495 File Offset: 0x0087FA95
		[SecurityCritical]
		public static void ZeroFreeBSTR(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (UIntPtr)(Win32Native.SysStringLen(s) * 2U));
			Marshal.FreeBSTR(s);
		}

		/// <summary>Frees an unmanaged string pointer that was allocated using the <see cref="M:System.Runtime.InteropServices.Marshal.SecureStringToCoTaskMemAnsi(System.Security.SecureString)" /> method.</summary>
		/// <param name="s">The address of the unmanaged string to free.</param>
		// Token: 0x060061A3 RID: 24995 RVA: 0x008814B0 File Offset: 0x0087FAB0
		[SecurityCritical]
		public static void ZeroFreeCoTaskMemAnsi(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (UIntPtr)((ulong)((long)Win32Native.lstrlenA(s))));
			Marshal.FreeCoTaskMem(s);
		}

		/// <summary>Frees an unmanaged string pointer that was allocated using the <see cref="M:System.Runtime.InteropServices.Marshal.SecureStringToCoTaskMemUnicode(System.Security.SecureString)" /> method.</summary>
		/// <param name="s">The address of the unmanaged string to free.</param>
		// Token: 0x060061A4 RID: 24996 RVA: 0x008814CA File Offset: 0x0087FACA
		[SecurityCritical]
		public static void ZeroFreeCoTaskMemUnicode(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (UIntPtr)((ulong)((long)(Win32Native.lstrlenW(s) * 2))));
			Marshal.FreeCoTaskMem(s);
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.Security.SecureString" /> into unmanaged memory, converting into ANSI format as it copies.</summary>
		/// <param name="s">The managed object to copy.</param>
		/// <returns>The address, in unmanaged memory, to where the <paramref name="s" /> parameter was copied, or 0 if a null object was supplied.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x060061A5 RID: 24997 RVA: 0x0074DE45 File Offset: 0x0074C445
		[SecurityCritical]
		public static IntPtr SecureStringToGlobalAllocAnsi(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToAnsiStr(true);
		}

		/// <summary>Copies the contents of a managed <see cref="T:System.Security.SecureString" /> object into unmanaged memory.</summary>
		/// <param name="s">The managed object to copy.</param>
		/// <returns>The address, in unmanaged memory, where <paramref name="s" /> was copied, or 0 if <paramref name="s" /> is a <see cref="T:System.Security.SecureString" /> object whose length is 0.</returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is <see langword="null" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The current computer is not running Windows 2000 Service Pack 3 or later.</exception>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
		// Token: 0x060061A6 RID: 24998 RVA: 0x0074DE5C File Offset: 0x0074C45C
		[SecurityCritical]
		public static IntPtr SecureStringToGlobalAllocUnicode(SecureString s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			return s.ToUniStr(true);
		}

		/// <summary>Frees an unmanaged string pointer that was allocated using the <see cref="M:System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocAnsi(System.Security.SecureString)" /> method.</summary>
		/// <param name="s">The address of the unmanaged string to free.</param>
		// Token: 0x060061A7 RID: 24999 RVA: 0x008814E6 File Offset: 0x0087FAE6
		[SecurityCritical]
		public static void ZeroFreeGlobalAllocAnsi(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (UIntPtr)((ulong)((long)Win32Native.lstrlenA(s))));
			Marshal.FreeHGlobal(s);
		}

		/// <summary>Frees an unmanaged string pointer that was allocated using the <see cref="M:System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(System.Security.SecureString)" /> method.</summary>
		/// <param name="s">The address of the unmanaged string to free.</param>
		// Token: 0x060061A8 RID: 25000 RVA: 0x00881500 File Offset: 0x0087FB00
		[SecurityCritical]
		public static void ZeroFreeGlobalAllocUnicode(IntPtr s)
		{
			Win32Native.ZeroMemory(s, (UIntPtr)((ulong)((long)(Win32Native.lstrlenW(s) * 2))));
			Marshal.FreeHGlobal(s);
		}

		// Token: 0x04002B5C RID: 11100
		private const int LMEM_FIXED = 0;

		// Token: 0x04002B5D RID: 11101
		private const int LMEM_MOVEABLE = 2;

		// Token: 0x04002B5E RID: 11102
		private const long HIWORDMASK = -65536L;

		// Token: 0x04002B5F RID: 11103
		private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

		/// <summary>Represents the default character size on the system; the default is 2 for Unicode systems and 1 for ANSI systems. This field is read-only.</summary>
		// Token: 0x04002B60 RID: 11104
		public static readonly int SystemDefaultCharSize = 2;

		/// <summary>Represents the maximum size of a double byte character set (DBCS) size, in bytes, for the current operating system. This field is read-only.</summary>
		// Token: 0x04002B61 RID: 11105
		public static readonly int SystemMaxDBCSCharSize = Marshal.GetSystemMaxDBCSCharSize();

		// Token: 0x04002B62 RID: 11106
		private const string s_strConvertedTypeInfoAssemblyName = "InteropDynamicTypes";

		// Token: 0x04002B63 RID: 11107
		private const string s_strConvertedTypeInfoAssemblyTitle = "Interop Dynamic Types";

		// Token: 0x04002B64 RID: 11108
		private const string s_strConvertedTypeInfoAssemblyDesc = "Type dynamically generated from ITypeInfo's";

		// Token: 0x04002B65 RID: 11109
		private const string s_strConvertedTypeInfoNameSpace = "InteropDynamicTypes";

		// Token: 0x04002B66 RID: 11110
		internal static readonly Guid ManagedNameGuid = new Guid("{0F21F359-AB84-41E8-9A78-36D110E6D2F9}");
	}
}
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Activation
{
	// Token: 0x0200089E RID: 2206
	internal static class RemotingXmlConfigFileParser
	{
		// Token: 0x06005D3B RID: 23867 RVA: 0x0077BD30 File Offset: 0x0077A330
		private static Hashtable CreateSyncCaseInsensitiveHashtable()
		{
			return Hashtable.Synchronized(RemotingXmlConfigFileParser.CreateCaseInsensitiveHashtable());
		}

		// Token: 0x06005D3C RID: 23868 RVA: 0x0077BD3C File Offset: 0x0077A33C
		private static Hashtable CreateCaseInsensitiveHashtable()
		{
			return new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		}

		// Token: 0x06005D3D RID: 23869 RVA: 0x0087C940 File Offset: 0x0087AF40
		public static RemotingXmlConfigFileData ParseDefaultConfiguration()
		{
			ConfigNode configNode = new ConfigNode("system.runtime.remoting", null);
			ConfigNode configNode2 = new ConfigNode("application", configNode);
			configNode.Children.Add(configNode2);
			ConfigNode configNode3 = new ConfigNode("channels", configNode2);
			configNode2.Children.Add(configNode3);
			ConfigNode configNode4 = new ConfigNode("channel", configNode2);
			configNode4.Attributes.Add(new DictionaryEntry("ref", "http client"));
			configNode4.Attributes.Add(new DictionaryEntry("displayName", "http client (delay loaded)"));
			configNode4.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode2);
			configNode4.Attributes.Add(new DictionaryEntry("ref", "tcp client"));
			configNode4.Attributes.Add(new DictionaryEntry("displayName", "tcp client (delay loaded)"));
			configNode4.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode2);
			configNode4.Attributes.Add(new DictionaryEntry("ref", "ipc client"));
			configNode4.Attributes.Add(new DictionaryEntry("displayName", "ipc client (delay loaded)"));
			configNode4.Attributes.Add(new DictionaryEntry("delayLoadAsClientChannel", "true"));
			configNode3.Children.Add(configNode4);
			configNode3 = new ConfigNode("channels", configNode);
			configNode.Children.Add(configNode3);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "http"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "http client"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "http server"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Http.HttpServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "tcp"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "tcp client"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "tcp server"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Tcp.TcpServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "ipc"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "ipc client"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcClientChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			configNode4 = new ConfigNode("channel", configNode3);
			configNode4.Attributes.Add(new DictionaryEntry("id", "ipc server"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.Ipc.IpcServerChannel, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode3.Children.Add(configNode4);
			ConfigNode configNode5 = new ConfigNode("channelSinkProviders", configNode);
			configNode.Children.Add(configNode5);
			ConfigNode configNode6 = new ConfigNode("clientProviders", configNode5);
			configNode5.Children.Add(configNode6);
			configNode4 = new ConfigNode("formatter", configNode6);
			configNode4.Attributes.Add(new DictionaryEntry("id", "soap"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapClientFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode6.Children.Add(configNode4);
			configNode4 = new ConfigNode("formatter", configNode6);
			configNode4.Attributes.Add(new DictionaryEntry("id", "binary"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode6.Children.Add(configNode4);
			ConfigNode configNode7 = new ConfigNode("serverProviders", configNode5);
			configNode5.Children.Add(configNode7);
			configNode4 = new ConfigNode("formatter", configNode7);
			configNode4.Attributes.Add(new DictionaryEntry("id", "soap"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode7.Children.Add(configNode4);
			configNode4 = new ConfigNode("formatter", configNode7);
			configNode4.Attributes.Add(new DictionaryEntry("id", "binary"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode7.Children.Add(configNode4);
			configNode4 = new ConfigNode("provider", configNode7);
			configNode4.Attributes.Add(new DictionaryEntry("id", "wsdl"));
			configNode4.Attributes.Add(new DictionaryEntry("type", "System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider, System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
			configNode7.Children.Add(configNode4);
			return RemotingXmlConfigFileParser.ParseConfigNode(configNode);
		}

		// Token: 0x06005D3E RID: 23870 RVA: 0x0087CF60 File Offset: 0x0087B560
		public static RemotingXmlConfigFileData ParseConfigFile(string filename)
		{
			ConfigTreeParser configTreeParser = new ConfigTreeParser();
			ConfigNode rootNode = configTreeParser.Parse(filename, "/configuration/system.runtime.remoting");
			return RemotingXmlConfigFileParser.ParseConfigNode(rootNode);
		}

		// Token: 0x06005D3F RID: 23871 RVA: 0x0087CF88 File Offset: 0x0087B588
		private static RemotingXmlConfigFileData ParseConfigNode(ConfigNode rootNode)
		{
			RemotingXmlConfigFileData remotingXmlConfigFileData = new RemotingXmlConfigFileData();
			if (rootNode == null)
			{
				return null;
			}
			foreach (DictionaryEntry dictionaryEntry in rootNode.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				a == "version";
			}
			ConfigNode configNode = null;
			ConfigNode configNode2 = null;
			ConfigNode configNode3 = null;
			ConfigNode configNode4 = null;
			ConfigNode configNode5 = null;
			foreach (ConfigNode configNode6 in rootNode.Children)
			{
				string name = configNode6.Name;
				if (!(name == "application"))
				{
					if (!(name == "channels"))
					{
						if (!(name == "channelSinkProviders"))
						{
							if (!(name == "debug"))
							{
								if (name == "customErrors")
								{
									if (configNode5 != null)
									{
										RemotingXmlConfigFileParser.ReportUniqueSectionError(rootNode, configNode5, remotingXmlConfigFileData);
									}
									configNode5 = configNode6;
								}
							}
							else
							{
								if (configNode4 != null)
								{
									RemotingXmlConfigFileParser.ReportUniqueSectionError(rootNode, configNode4, remotingXmlConfigFileData);
								}
								configNode4 = configNode6;
							}
						}
						else
						{
							if (configNode3 != null)
							{
								RemotingXmlConfigFileParser.ReportUniqueSectionError(rootNode, configNode3, remotingXmlConfigFileData);
							}
							configNode3 = configNode6;
						}
					}
					else
					{
						if (configNode2 != null)
						{
							RemotingXmlConfigFileParser.ReportUniqueSectionError(rootNode, configNode2, remotingXmlConfigFileData);
						}
						configNode2 = configNode6;
					}
				}
				else
				{
					if (configNode != null)
					{
						RemotingXmlConfigFileParser.ReportUniqueSectionError(rootNode, configNode, remotingXmlConfigFileData);
					}
					configNode = configNode6;
				}
			}
			if (configNode4 != null)
			{
				RemotingXmlConfigFileParser.ProcessDebugNode(configNode4, remotingXmlConfigFileData);
			}
			if (configNode3 != null)
			{
				RemotingXmlConfigFileParser.ProcessChannelSinkProviderTemplates(configNode3, remotingXmlConfigFileData);
			}
			if (configNode2 != null)
			{
				RemotingXmlConfigFileParser.ProcessChannelTemplates(configNode2, remotingXmlConfigFileData);
			}
			if (configNode != null)
			{
				RemotingXmlConfigFileParser.ProcessApplicationNode(configNode, remotingXmlConfigFileData);
			}
			if (configNode5 != null)
			{
				RemotingXmlConfigFileParser.ProcessCustomErrorsNode(configNode5, remotingXmlConfigFileData);
			}
			return remotingXmlConfigFileData;
		}

		// Token: 0x06005D40 RID: 23872 RVA: 0x0077BD48 File Offset: 0x0077A348
		private static void ReportError(string errorStr, RemotingXmlConfigFileData configData)
		{
			throw new RemotingException(errorStr);
		}

		// Token: 0x06005D41 RID: 23873 RVA: 0x0087D128 File Offset: 0x0087B728
		private static void ReportUniqueSectionError(ConfigNode parent, ConfigNode child, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NodeMustBeUnique"), child.Name, parent.Name), configData);
		}

		// Token: 0x06005D42 RID: 23874 RVA: 0x0077BD50 File Offset: 0x0077A350
		private static void ReportUnknownValueError(ConfigNode node, string value, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnknownValue"), node.Name, value), configData);
		}

		// Token: 0x06005D43 RID: 23875 RVA: 0x0077BD73 File Offset: 0x0077A373
		private static void ReportMissingAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportMissingAttributeError(node.Name, attributeName, configData);
		}

		// Token: 0x06005D44 RID: 23876 RVA: 0x0077BD82 File Offset: 0x0077A382
		private static void ReportMissingAttributeError(string nodeDescription, string attributeName, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_RequiredXmlAttribute"), nodeDescription, attributeName), configData);
		}

		// Token: 0x06005D45 RID: 23877 RVA: 0x0077BDA0 File Offset: 0x0077A3A0
		private static void ReportMissingTypeAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingTypeAttribute"), node.Name, attributeName), configData);
		}

		// Token: 0x06005D46 RID: 23878 RVA: 0x0077BDC3 File Offset: 0x0077A3C3
		private static void ReportMissingXmlTypeAttributeError(ConfigNode node, string attributeName, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_MissingXmlTypeAttribute"), node.Name, attributeName), configData);
		}

		// Token: 0x06005D47 RID: 23879 RVA: 0x0077BDE6 File Offset: 0x0077A3E6
		private static void ReportInvalidTimeFormatError(string time, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidTimeFormat"), time), configData);
		}

		// Token: 0x06005D48 RID: 23880 RVA: 0x0087D150 File Offset: 0x0087B750
		private static void ReportNonTemplateIdAttributeError(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_NonTemplateIdAttribute"), node.Name), configData);
		}

		// Token: 0x06005D49 RID: 23881 RVA: 0x0087D172 File Offset: 0x0087B772
		private static void ReportTemplateCannotReferenceTemplateError(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_TemplateCannotReferenceTemplate"), node.Name), configData);
		}

		// Token: 0x06005D4A RID: 23882 RVA: 0x0077BE03 File Offset: 0x0077A403
		private static void ReportUnableToResolveTemplateReferenceError(ConfigNode node, string referenceName, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_UnableToResolveTemplate"), node.Name, referenceName), configData);
		}

		// Token: 0x06005D4B RID: 23883 RVA: 0x0077BE26 File Offset: 0x0077A426
		private static void ReportAssemblyVersionInfoPresent(string assemName, string entryDescription, RemotingXmlConfigFileData configData)
		{
			RemotingXmlConfigFileParser.ReportError(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_VersionPresent"), assemName, entryDescription), configData);
		}

		// Token: 0x06005D4C RID: 23884 RVA: 0x0087D194 File Offset: 0x0087B794
		private static void ProcessDebugNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (a == "loadTypes")
				{
					RemotingXmlConfigFileData.LoadTypes = Convert.ToBoolean((string)dictionaryEntry.Value, CultureInfo.InvariantCulture);
				}
			}
		}

		// Token: 0x06005D4D RID: 23885 RVA: 0x0087D218 File Offset: 0x0087B818
		private static void ProcessApplicationNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string text = dictionaryEntry.Key.ToString();
				if (text.Equals("name"))
				{
					configData.ApplicationName = (string)dictionaryEntry.Value;
				}
			}
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "channels"))
				{
					if (!(name == "client"))
					{
						if (!(name == "lifetime"))
						{
							if (!(name == "service"))
							{
								if (name == "soapInterop")
								{
									RemotingXmlConfigFileParser.ProcessSoapInteropNode(configNode, configData);
								}
							}
							else
							{
								RemotingXmlConfigFileParser.ProcessServiceNode(configNode, configData);
							}
						}
						else
						{
							RemotingXmlConfigFileParser.ProcessLifetimeNode(node, configNode, configData);
						}
					}
					else
					{
						RemotingXmlConfigFileParser.ProcessClientNode(configNode, configData);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessChannelsNode(configNode, configData);
				}
			}
		}

		// Token: 0x06005D4E RID: 23886 RVA: 0x0087D354 File Offset: 0x0087B954
		private static void ProcessCustomErrorsNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string text = dictionaryEntry.Key.ToString();
				if (text.Equals("mode"))
				{
					string text2 = (string)dictionaryEntry.Value;
					CustomErrorsModes mode = CustomErrorsModes.On;
					if (string.Compare(text2, "on", StringComparison.OrdinalIgnoreCase) == 0)
					{
						mode = CustomErrorsModes.On;
					}
					else if (string.Compare(text2, "off", StringComparison.OrdinalIgnoreCase) == 0)
					{
						mode = CustomErrorsModes.Off;
					}
					else if (string.Compare(text2, "remoteonly", StringComparison.OrdinalIgnoreCase) == 0)
					{
						mode = CustomErrorsModes.RemoteOnly;
					}
					else
					{
						RemotingXmlConfigFileParser.ReportUnknownValueError(node, text2, configData);
					}
					configData.CustomErrors = new RemotingXmlConfigFileData.CustomErrorsEntry(mode);
				}
			}
		}

		// Token: 0x06005D4F RID: 23887 RVA: 0x0087D420 File Offset: 0x0087BA20
		private static void ProcessLifetimeNode(ConfigNode parentNode, ConfigNode node, RemotingXmlConfigFileData configData)
		{
			if (configData.Lifetime != null)
			{
				RemotingXmlConfigFileParser.ReportUniqueSectionError(node, parentNode, configData);
			}
			configData.Lifetime = new RemotingXmlConfigFileData.LifetimeEntry();
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "leaseTime"))
				{
					if (!(a == "sponsorshipTimeout"))
					{
						if (!(a == "renewOnCallTime"))
						{
							if (a == "leaseManagerPollTime")
							{
								configData.Lifetime.LeaseManagerPollTime = RemotingXmlConfigFileParser.ParseTime((string)dictionaryEntry.Value, configData);
							}
						}
						else
						{
							configData.Lifetime.RenewOnCallTime = RemotingXmlConfigFileParser.ParseTime((string)dictionaryEntry.Value, configData);
						}
					}
					else
					{
						configData.Lifetime.SponsorshipTimeout = RemotingXmlConfigFileParser.ParseTime((string)dictionaryEntry.Value, configData);
					}
				}
				else
				{
					configData.Lifetime.LeaseTime = RemotingXmlConfigFileParser.ParseTime((string)dictionaryEntry.Value, configData);
				}
			}
		}

		// Token: 0x06005D50 RID: 23888 RVA: 0x0087D54C File Offset: 0x0087BB4C
		private static void ProcessServiceNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "wellknown"))
				{
					if (name == "activated")
					{
						RemotingXmlConfigFileParser.ProcessServiceActivatedNode(configNode, configData);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessServiceWellKnownNode(configNode, configData);
				}
			}
		}

		// Token: 0x06005D51 RID: 23889 RVA: 0x0087D5CC File Offset: 0x0087BBCC
		private static void ProcessClientNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "url"))
				{
					if (!(a == "displayName"))
					{
					}
				}
				else
				{
					text = (string)dictionaryEntry.Value;
				}
			}
			RemotingXmlConfigFileData.RemoteAppEntry remoteAppEntry = configData.AddRemoteAppEntry(text);
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "wellknown"))
				{
					if (name == "activated")
					{
						RemotingXmlConfigFileParser.ProcessClientActivatedNode(configNode, configData, remoteAppEntry);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessClientWellKnownNode(configNode, configData, remoteAppEntry);
				}
			}
			if (remoteAppEntry.ActivatedObjects.Count > 0 && text == null)
			{
				RemotingXmlConfigFileParser.ReportMissingAttributeError(node, "url", configData);
			}
		}

		// Token: 0x06005D52 RID: 23890 RVA: 0x0087D6F0 File Offset: 0x0087BCF0
		private static void ProcessSoapInteropNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (a == "urlObjRef")
				{
					configData.UrlObjRefMode = Convert.ToBoolean(dictionaryEntry.Value, CultureInfo.InvariantCulture);
				}
			}
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "preLoad"))
				{
					if (!(name == "interopXmlElement"))
					{
						if (name == "interopXmlType")
						{
							RemotingXmlConfigFileParser.ProcessInteropXmlTypeNode(configNode, configData);
						}
					}
					else
					{
						RemotingXmlConfigFileParser.ProcessInteropXmlElementNode(configNode, configData);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessPreLoadNode(configNode, configData);
				}
			}
		}

		// Token: 0x06005D53 RID: 23891 RVA: 0x0087D7FC File Offset: 0x0087BDFC
		private static void ProcessChannelsNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (ConfigNode configNode in node.Children)
			{
				if (configNode.Name.Equals("channel"))
				{
					RemotingXmlConfigFileData.ChannelEntry value = RemotingXmlConfigFileParser.ProcessChannelsChannelNode(configNode, configData, false);
					configData.ChannelEntries.Add(value);
				}
			}
		}

		// Token: 0x06005D54 RID: 23892 RVA: 0x0087D870 File Offset: 0x0087BE70
		private static void ProcessServiceWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			string text2 = null;
			ArrayList arrayList = new ArrayList();
			string text3 = null;
			WellKnownObjectMode objMode = WellKnownObjectMode.Singleton;
			bool flag = false;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "displayName"))
				{
					if (!(a == "mode"))
					{
						if (!(a == "objectUri"))
						{
							if (a == "type")
							{
								RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
							}
						}
						else
						{
							text3 = (string)dictionaryEntry.Value;
						}
					}
					else
					{
						string strA = (string)dictionaryEntry.Value;
						flag = true;
						if (string.CompareOrdinal(strA, "Singleton") == 0)
						{
							objMode = WellKnownObjectMode.Singleton;
						}
						else if (string.CompareOrdinal(strA, "SingleCall") == 0)
						{
							objMode = WellKnownObjectMode.SingleCall;
						}
						else
						{
							flag = false;
						}
					}
				}
			}
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "contextAttribute"))
				{
					if (!(name == "lifetime"))
					{
					}
				}
				else
				{
					arrayList.Add(RemotingXmlConfigFileParser.ProcessContextAttributeNode(configNode, configData));
				}
			}
			if (!flag)
			{
				RemotingXmlConfigFileParser.ReportError(Environment.GetResourceString("Remoting_Config_MissingWellKnownModeAttribute"), configData);
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			if (text3 == null)
			{
				text3 = text + ".soap";
			}
			configData.AddServerWellKnownEntry(text, text2, arrayList, text3, objMode);
		}

		// Token: 0x06005D55 RID: 23893 RVA: 0x0087DA38 File Offset: 0x0087C038
		private static void ProcessServiceActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			string text2 = null;
			ArrayList arrayList = new ArrayList();
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (a == "type")
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
			}
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "contextAttribute"))
				{
					if (!(name == "lifetime"))
					{
					}
				}
				else
				{
					arrayList.Add(RemotingXmlConfigFileParser.ProcessContextAttributeNode(configNode, configData));
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			if (RemotingXmlConfigFileParser.CheckAssemblyNameForVersionInfo(text2))
			{
				RemotingXmlConfigFileParser.ReportAssemblyVersionInfoPresent(text2, "service activated", configData);
			}
			configData.AddServerActivatedEntry(text, text2, arrayList);
		}

		// Token: 0x06005D56 RID: 23894 RVA: 0x0087DB64 File Offset: 0x0087C164
		private static void ProcessClientWellKnownNode(ConfigNode node, RemotingXmlConfigFileData configData, RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "displayName"))
				{
					if (!(a == "type"))
					{
						if (a == "url")
						{
							text3 = (string)dictionaryEntry.Value;
						}
					}
					else
					{
						RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
					}
				}
			}
			if (text3 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingAttributeError("WellKnown client", "url", configData);
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			if (RemotingXmlConfigFileParser.CheckAssemblyNameForVersionInfo(text2))
			{
				RemotingXmlConfigFileParser.ReportAssemblyVersionInfoPresent(text2, "client wellknown", configData);
			}
			remoteApp.AddWellKnownEntry(text, text2, text3);
		}

		// Token: 0x06005D57 RID: 23895 RVA: 0x0087DC58 File Offset: 0x0087C258
		private static void ProcessClientActivatedNode(ConfigNode node, RemotingXmlConfigFileData configData, RemotingXmlConfigFileData.RemoteAppEntry remoteApp)
		{
			string text = null;
			string text2 = null;
			ArrayList arrayList = new ArrayList();
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (a == "type")
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
			}
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (name == "contextAttribute")
				{
					arrayList.Add(RemotingXmlConfigFileParser.ProcessContextAttributeNode(configNode, configData));
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			remoteApp.AddActivatedEntry(text, text2, arrayList);
		}

		// Token: 0x06005D58 RID: 23896 RVA: 0x0087DD60 File Offset: 0x0087C360
		private static void ProcessInteropXmlElementNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			string text4 = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "xml"))
				{
					if (a == "clr")
					{
						RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text3, out text4);
					}
				}
				else
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingXmlTypeAttributeError(node, "xml", configData);
			}
			if (text3 == null || text4 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "clr", configData);
			}
			configData.AddInteropXmlElementEntry(text, text2, text3, text4);
		}

		// Token: 0x06005D59 RID: 23897 RVA: 0x0087DE3C File Offset: 0x0087C43C
		private static void ProcessInteropXmlTypeNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			string text4 = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "xml"))
				{
					if (a == "clr")
					{
						RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text3, out text4);
					}
				}
				else
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingXmlTypeAttributeError(node, "xml", configData);
			}
			if (text3 == null || text4 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "clr", configData);
			}
			configData.AddInteropXmlTypeEntry(text, text2, text3, text4);
		}

		// Token: 0x06005D5A RID: 23898 RVA: 0x0087DF18 File Offset: 0x0087C518
		private static void ProcessPreLoadNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string typeName = null;
			string text = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string a = dictionaryEntry.Key.ToString();
				if (!(a == "type"))
				{
					if (a == "assembly")
					{
						text = (string)dictionaryEntry.Value;
					}
				}
				else
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out typeName, out text);
				}
			}
			if (text == null)
			{
				RemotingXmlConfigFileParser.ReportError(Environment.GetResourceString("Remoting_Config_PreloadRequiresTypeOrAssembly"), configData);
			}
			configData.AddPreLoadEntry(typeName, text);
		}

		// Token: 0x06005D5B RID: 23899 RVA: 0x0087DFD4 File Offset: 0x0087C5D4
		private static RemotingXmlConfigFileData.ContextAttributeEntry ProcessContextAttributeNode(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			string text = null;
			string text2 = null;
			Hashtable hashtable = RemotingXmlConfigFileParser.CreateCaseInsensitiveHashtable();
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string text3 = ((string)dictionaryEntry.Key).ToLower(CultureInfo.InvariantCulture);
				if (text3 == "type")
				{
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
				else
				{
					hashtable[text3] = dictionaryEntry.Value;
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			return new RemotingXmlConfigFileData.ContextAttributeEntry(text, text2, hashtable);
		}

		// Token: 0x06005D5C RID: 23900 RVA: 0x0087E094 File Offset: 0x0087C694
		private static RemotingXmlConfigFileData.ChannelEntry ProcessChannelsChannelNode(ConfigNode node, RemotingXmlConfigFileData configData, bool isTemplate)
		{
			string key = null;
			string text = null;
			string text2 = null;
			Hashtable hashtable = RemotingXmlConfigFileParser.CreateCaseInsensitiveHashtable();
			bool delayLoad = false;
			RemotingXmlConfigFileData.ChannelEntry channelEntry = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string text3 = (string)dictionaryEntry.Key;
				if (!(text3 == "displayName"))
				{
					if (!(text3 == "id"))
					{
						if (!(text3 == "ref"))
						{
							if (!(text3 == "type"))
							{
								if (!(text3 == "delayLoadAsClientChannel"))
								{
									hashtable[text3] = dictionaryEntry.Value;
									continue;
								}
								delayLoad = Convert.ToBoolean((string)dictionaryEntry.Value, CultureInfo.InvariantCulture);
								continue;
							}
						}
						else
						{
							if (isTemplate)
							{
								RemotingXmlConfigFileParser.ReportTemplateCannotReferenceTemplateError(node, configData);
								continue;
							}
							channelEntry = (RemotingXmlConfigFileData.ChannelEntry)RemotingXmlConfigFileParser._channelTemplates[dictionaryEntry.Value];
							if (channelEntry == null)
							{
								RemotingXmlConfigFileParser.ReportUnableToResolveTemplateReferenceError(node, dictionaryEntry.Value.ToString(), configData);
								continue;
							}
							text = channelEntry.TypeName;
							text2 = channelEntry.AssemblyName;
							using (IDictionaryEnumerator enumerator2 = channelEntry.Properties.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									object obj = enumerator2.Current;
									DictionaryEntry dictionaryEntry2 = (DictionaryEntry)obj;
									hashtable[dictionaryEntry2.Key] = dictionaryEntry2.Value;
								}
								continue;
							}
						}
						RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
					}
					else if (!isTemplate)
					{
						RemotingXmlConfigFileParser.ReportNonTemplateIdAttributeError(node, configData);
					}
					else
					{
						key = ((string)dictionaryEntry.Value).ToLower(CultureInfo.InvariantCulture);
					}
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			RemotingXmlConfigFileData.ChannelEntry channelEntry2 = new RemotingXmlConfigFileData.ChannelEntry(text, text2, hashtable);
			channelEntry2.DelayLoad = delayLoad;
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "clientProviders"))
				{
					if (name == "serverProviders")
					{
						RemotingXmlConfigFileParser.ProcessSinkProviderNodes(configNode, channelEntry2, configData, true);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessSinkProviderNodes(configNode, channelEntry2, configData, false);
				}
			}
			if (channelEntry != null)
			{
				if (channelEntry2.ClientSinkProviders.Count == 0)
				{
					channelEntry2.ClientSinkProviders = channelEntry.ClientSinkProviders;
				}
				if (channelEntry2.ServerSinkProviders.Count == 0)
				{
					channelEntry2.ServerSinkProviders = channelEntry.ServerSinkProviders;
				}
			}
			if (isTemplate)
			{
				RemotingXmlConfigFileParser._channelTemplates[key] = channelEntry2;
				return null;
			}
			return channelEntry2;
		}

		// Token: 0x06005D5D RID: 23901 RVA: 0x0087E390 File Offset: 0x0087C990
		private static void ProcessSinkProviderNodes(ConfigNode node, RemotingXmlConfigFileData.ChannelEntry channelEntry, RemotingXmlConfigFileData configData, bool isServer)
		{
			foreach (ConfigNode node2 in node.Children)
			{
				RemotingXmlConfigFileData.SinkProviderEntry value = RemotingXmlConfigFileParser.ProcessSinkProviderNode(node2, configData, false, isServer);
				if (isServer)
				{
					channelEntry.ServerSinkProviders.Add(value);
				}
				else
				{
					channelEntry.ClientSinkProviders.Add(value);
				}
			}
		}

		// Token: 0x06005D5E RID: 23902 RVA: 0x0087E408 File Offset: 0x0087CA08
		private static RemotingXmlConfigFileData.SinkProviderEntry ProcessSinkProviderNode(ConfigNode node, RemotingXmlConfigFileData configData, bool isTemplate, bool isServer)
		{
			bool isFormatter = false;
			string name = node.Name;
			if (name.Equals("formatter"))
			{
				isFormatter = true;
			}
			else if (name.Equals("provider"))
			{
				isFormatter = false;
			}
			else
			{
				RemotingXmlConfigFileParser.ReportError(Environment.GetResourceString("Remoting_Config_ProviderNeedsElementName"), configData);
			}
			string key = null;
			string text = null;
			string text2 = null;
			Hashtable hashtable = RemotingXmlConfigFileParser.CreateCaseInsensitiveHashtable();
			RemotingXmlConfigFileData.SinkProviderEntry sinkProviderEntry = null;
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				string text3 = (string)dictionaryEntry.Key;
				if (!(text3 == "id"))
				{
					if (!(text3 == "ref"))
					{
						if (!(text3 == "type"))
						{
							hashtable[text3] = dictionaryEntry.Value;
							continue;
						}
					}
					else
					{
						if (isTemplate)
						{
							RemotingXmlConfigFileParser.ReportTemplateCannotReferenceTemplateError(node, configData);
							continue;
						}
						if (isServer)
						{
							sinkProviderEntry = (RemotingXmlConfigFileData.SinkProviderEntry)RemotingXmlConfigFileParser._serverChannelSinkTemplates[dictionaryEntry.Value];
						}
						else
						{
							sinkProviderEntry = (RemotingXmlConfigFileData.SinkProviderEntry)RemotingXmlConfigFileParser._clientChannelSinkTemplates[dictionaryEntry.Value];
						}
						if (sinkProviderEntry == null)
						{
							RemotingXmlConfigFileParser.ReportUnableToResolveTemplateReferenceError(node, dictionaryEntry.Value.ToString(), configData);
							continue;
						}
						text = sinkProviderEntry.TypeName;
						text2 = sinkProviderEntry.AssemblyName;
						using (IDictionaryEnumerator enumerator2 = sinkProviderEntry.Properties.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								object obj = enumerator2.Current;
								DictionaryEntry dictionaryEntry2 = (DictionaryEntry)obj;
								hashtable[dictionaryEntry2.Key] = dictionaryEntry2.Value;
							}
							continue;
						}
					}
					RemotingConfigHandler.ParseType((string)dictionaryEntry.Value, out text, out text2);
				}
				else if (!isTemplate)
				{
					RemotingXmlConfigFileParser.ReportNonTemplateIdAttributeError(node, configData);
				}
				else
				{
					key = (string)dictionaryEntry.Value;
				}
			}
			if (text == null || text2 == null)
			{
				RemotingXmlConfigFileParser.ReportMissingTypeAttributeError(node, "type", configData);
			}
			RemotingXmlConfigFileData.SinkProviderEntry sinkProviderEntry2 = new RemotingXmlConfigFileData.SinkProviderEntry(text, text2, hashtable, isFormatter);
			foreach (ConfigNode node2 in node.Children)
			{
				SinkProviderData value = RemotingXmlConfigFileParser.ProcessSinkProviderData(node2, configData);
				sinkProviderEntry2.ProviderData.Add(value);
			}
			if (sinkProviderEntry != null && sinkProviderEntry2.ProviderData.Count == 0)
			{
				sinkProviderEntry2.ProviderData = sinkProviderEntry.ProviderData;
			}
			if (isTemplate)
			{
				if (isServer)
				{
					RemotingXmlConfigFileParser._serverChannelSinkTemplates[key] = sinkProviderEntry2;
				}
				else
				{
					RemotingXmlConfigFileParser._clientChannelSinkTemplates[key] = sinkProviderEntry2;
				}
				return null;
			}
			return sinkProviderEntry2;
		}

		// Token: 0x06005D5F RID: 23903 RVA: 0x0087E6E0 File Offset: 0x0087CCE0
		private static SinkProviderData ProcessSinkProviderData(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			SinkProviderData sinkProviderData = new SinkProviderData(node.Name);
			foreach (ConfigNode node2 in node.Children)
			{
				SinkProviderData value = RemotingXmlConfigFileParser.ProcessSinkProviderData(node2, configData);
				sinkProviderData.Children.Add(value);
			}
			foreach (DictionaryEntry dictionaryEntry in node.Attributes)
			{
				sinkProviderData.Properties[dictionaryEntry.Key] = dictionaryEntry.Value;
			}
			return sinkProviderData;
		}

		// Token: 0x06005D60 RID: 23904 RVA: 0x0087E7A4 File Offset: 0x0087CDA4
		private static void ProcessChannelTemplates(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (name == "channel")
				{
					RemotingXmlConfigFileParser.ProcessChannelsChannelNode(configNode, configData, true);
				}
			}
		}

		// Token: 0x06005D61 RID: 23905 RVA: 0x0087E810 File Offset: 0x0087CE10
		private static void ProcessChannelSinkProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData)
		{
			foreach (ConfigNode configNode in node.Children)
			{
				string name = configNode.Name;
				if (!(name == "clientProviders"))
				{
					if (name == "serverProviders")
					{
						RemotingXmlConfigFileParser.ProcessChannelProviderTemplates(configNode, configData, true);
					}
				}
				else
				{
					RemotingXmlConfigFileParser.ProcessChannelProviderTemplates(configNode, configData, false);
				}
			}
		}

		// Token: 0x06005D62 RID: 23906 RVA: 0x0087E894 File Offset: 0x0087CE94
		private static void ProcessChannelProviderTemplates(ConfigNode node, RemotingXmlConfigFileData configData, bool isServer)
		{
			foreach (ConfigNode node2 in node.Children)
			{
				RemotingXmlConfigFileParser.ProcessSinkProviderNode(node2, configData, true, isServer);
			}
		}

		// Token: 0x06005D63 RID: 23907 RVA: 0x0077BE44 File Offset: 0x0077A444
		private static bool CheckAssemblyNameForVersionInfo(string assemName)
		{
			if (assemName == null)
			{
				return false;
			}
			int num = assemName.IndexOf(',');
			return num != -1;
		}

		// Token: 0x06005D64 RID: 23908 RVA: 0x0087E8EC File Offset: 0x0087CEEC
		private static TimeSpan ParseTime(string time, RemotingXmlConfigFileData configData)
		{
			string time2 = time;
			string a = "s";
			int num = 0;
			char c = ' ';
			if (time.Length > 0)
			{
				c = time[time.Length - 1];
			}
			TimeSpan result = TimeSpan.FromSeconds(0.0);
			try
			{
				if (!char.IsDigit(c))
				{
					if (time.Length == 0)
					{
						RemotingXmlConfigFileParser.ReportInvalidTimeFormatError(time2, configData);
					}
					time = time.ToLower(CultureInfo.InvariantCulture);
					num = 1;
					if (time.EndsWith("ms", StringComparison.Ordinal))
					{
						num = 2;
					}
					a = time.Substring(time.Length - num, num);
				}
				int num2 = int.Parse(time.Substring(0, time.Length - num), CultureInfo.InvariantCulture);
				if (!(a == "d"))
				{
					if (!(a == "h"))
					{
						if (!(a == "m"))
						{
							if (!(a == "s"))
							{
								if (!(a == "ms"))
								{
									RemotingXmlConfigFileParser.ReportInvalidTimeFormatError(time2, configData);
								}
								else
								{
									result = TimeSpan.FromMilliseconds((double)num2);
								}
							}
							else
							{
								result = TimeSpan.FromSeconds((double)num2);
							}
						}
						else
						{
							result = TimeSpan.FromMinutes((double)num2);
						}
					}
					else
					{
						result = TimeSpan.FromHours((double)num2);
					}
				}
				else
				{
					result = TimeSpan.FromDays((double)num2);
				}
			}
			catch (Exception)
			{
				RemotingXmlConfigFileParser.ReportInvalidTimeFormatError(time2, configData);
			}
			return result;
		}

		// Token: 0x04002A08 RID: 10760
		private static Hashtable _channelTemplates = RemotingXmlConfigFileParser.CreateSyncCaseInsensitiveHashtable();

		// Token: 0x04002A09 RID: 10761
		private static Hashtable _clientChannelSinkTemplates = RemotingXmlConfigFileParser.CreateSyncCaseInsensitiveHashtable();

		// Token: 0x04002A0A RID: 10762
		private static Hashtable _serverChannelSinkTemplates = RemotingXmlConfigFileParser.CreateSyncCaseInsensitiveHashtable();
	}
}

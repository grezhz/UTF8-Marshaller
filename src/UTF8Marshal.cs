using System.Runtime.InteropServices;
using System.Text;

namespace Prisma.Marshalling;

/// <summary>
///		 Collection of methods to allow convertion between managed and unmanaged UTF-8 strings.
/// </summary>
public static unsafe class UTF8Marshaller
{
	/// <summary>
	///		Converts a null-terminated unmanaged UTF-8 string into a managed <see cref="string"/>.
	/// </summary>
	/// 
	/// <param name="ptr"> Address of the unmanaged string. </param>
	/// 
	/// <returns>
	///		A managed <see cref="string"/>, copy of the unmanaged string in <paramref name="ptr"/>, only if ptr is valid; otherwise returns
	///		<see langword="null"/>.
	/// </returns>
	public static string? UnmanagedToManaged(nint ptr)
	{
		if (ptr == nint.Zero)
			return null;
		
		byte* b = (byte*)ptr;
		int len = 0;
		for (; b[len] != 0; len++) { } // cursed shit

		return Encoding.UTF8.GetString(b, len);
	}

	/// <summary>
	///		Converts a managed <see cref="string"/> into a null-terminated unmanaged UTF-8 string.
	/// </summary>
	/// 
	/// <remarks>
	///		The returning pointer must be freed manually using <see cref="Free"/> (wrapper of <see cref="Marshal.FreeHGlobal"/>).
	/// </remarks>
	/// 
	/// <param name="str"> The managed string. </param>
	/// 
	/// <returns>
	///		A pointer to the unmanaged UTF-8 string, copy of the managed string in <paramref name="str"/>, only if str is valid; otherwise returns
	///		<see langword="null"/>.
	/// </returns>
	public static nint ManagedToUnmanaged(string? str)
	{
		if (str is null)
			return nint.Zero;

		byte[] bytes = Encoding.UTF8.GetBytes(str);

		// 'buffer' needs to be freed because it's heap-allocated.
		var bufferSize = bytes.Length;
		var buffer = Marshal.AllocHGlobal(bytes.Length);
		
		// I prefered allocate new memory instead of passing buffers around.
		Marshal.Copy(bytes, 0, buffer, bufferSize);
		return buffer;
	}

	/// <summary>
	///		Frees an unmanaged UTF-8 string allocated using <see cref="ManagedToUnmanaged"/>.
	/// </summary>
	/// 
	/// <param name="ptr"> Address of the unmanaged string. </param>
	public static void Free(nint ptr)
	{
		Marshal.FreeHGlobal(ptr);
	}
}
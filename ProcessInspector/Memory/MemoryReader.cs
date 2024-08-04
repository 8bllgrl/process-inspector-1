using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory
{
    public static class MemoryReader
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static byte[] ReadMemoryOutLoud(IntPtr processHandle, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            if (!ReadProcessMemory(processHandle, address, buffer, size, out int bytesRead) || bytesRead != size)
            {
                throw new InvalidOperationException("Failed to read memory.");
            }
            return buffer;
        }
        public static bool Read(IntPtr address, byte[] buffer, int size = -1)
        {
            if (size <= 0)
                size = buffer.Length;

            return ReadProcessMemory(ProcessInfo.Handle, address, buffer, size, out _);
        }

        public static T Read<T>(IntPtr processHandle, IntPtr address) where T : struct
        {
            // Implement the logic to read memory based on the type T.
            // For simplicity, using Marshal here.
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return result;
        }

        public static string ReadStringFromMemory(IntPtr processHandle, IntPtr address, int maxLength = 200)
        {
            byte[] buffer = new byte[maxLength];
            int bytesRead;

            if (ReadProcessMemory(processHandle, address, buffer, buffer.Length, out bytesRead))
            {
                return Encoding.ASCII.GetString(buffer, 0, bytesRead).TrimEnd('\0');
            }
            else
            {
                throw new Exception("Failed to read process memory.");
            }
        }



    }
}

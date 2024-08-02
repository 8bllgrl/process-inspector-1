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
    }
}

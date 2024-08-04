using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory
{
    public class SignatureScanner
    {

        public SignatureScanner() { }   
        public IntPtr GetStaticAddressFromSig(string signature, int offset = 0)
        {
            IntPtr instrAddr = ScanText(signature);
            instrAddr = IntPtr.Add(instrAddr, offset);
            long bAddr = (long)ProcessInfo.BaseAddress;
            int steps = 1000;

            while (steps > 0)
            {
                instrAddr = IntPtr.Add(instrAddr, 1);
                long num = ReadInt32(instrAddr) + (long)instrAddr + 4 - bAddr;
                if (num >= ProcessInfo.DataSectionOffset && num <= ProcessInfo.DataSectionOffset + ProcessInfo.DataSectionSize)
                    return IntPtr.Add(instrAddr, ReadInt32(instrAddr) + 4);

                steps--;
            }

            return IntPtr.Zero; // Handle case where address is not found
        }

        private byte ReadByte(IntPtr baseAddress, int offset = 0)
        {
            byte[] buffer = new byte[1];
            MemoryReader.Read(baseAddress + offset, buffer, 1);
            return buffer[0];
        }

        private IntPtr ReadCallSig(IntPtr sigLocation)
        {
            int jumpOffset = ReadInt32(IntPtr.Add(sigLocation, 1));
            return IntPtr.Add(sigLocation, 5 + jumpOffset);
        }

        public IntPtr ScanText(string signature)
        {
            IntPtr mBase = ProcessInfo.TextSectionStart;
            IntPtr scanRet = Scan(mBase, ProcessInfo.TextSectionSize, signature);

            if (ReadByte(scanRet) == 0xE8)
                return ReadCallSig(scanRet);

            return scanRet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte?[] ConvertSigToNeedle(string signature)
        {
            signature = signature.Replace(" ", string.Empty);

            if (signature.Length % 2 != 0)
                throw new ArgumentException("Signature without whitespaces must be divisible by two.", nameof(signature));

            int needleLength = signature.Length / 2;
            byte?[] needle = new byte?[needleLength];

            for (int i = 0; i < needleLength; i++)
            {
                string hexString = signature.Substring(i * 2, 2);
                needle[i] = hexString == "??" || hexString == "**" ? null : byte.Parse(hexString, NumberStyles.AllowHexSpecifier);
            }

            return needle;
        }

        private unsafe bool IsMatch(byte?[] needle, byte[] buffer, long offset)
        {
            for (int i = 0; i < needle.Length; i++)
            {
                byte? expected = needle[i];
                if (expected == null) continue;

                byte actual = buffer[offset + i];
                if (expected != actual) return false;
            }

            return true;
        }

        public IntPtr Scan(IntPtr baseAddress, int size, string signature)
        {
            byte?[] needle = ConvertSigToNeedle(signature);
            byte[] bigBuffer = new byte[size];
            MemoryReader.Read(baseAddress, bigBuffer, size);

            unsafe
            {
                for (long offset = 0; offset < size - needle.Length; offset++)
                {
                    if (IsMatch(needle, bigBuffer, offset))
                    {
                        return IntPtr.Add(baseAddress, (int)offset);
                    }
                }
            }

            throw new KeyNotFoundException($"Can't find a signature of {signature}");
        }

        private int ReadInt32(IntPtr baseAddress, int offset = 0)
        {
            byte[] buffer = new byte[4];
            MemoryReader.Read(baseAddress + offset, buffer, 4);
            return BitConverter.ToInt32(buffer);
        }
    }
}

using System;
using System.Diagnostics;

namespace ProcessInspector.Memory
{
    public class ProcessInfo
    {
        public static IntPtr BaseAddress { get; private set; }
        public static IntPtr PEHeaderAddress { get; private set; }
        public static int NumberOfSections { get; private set; }
        public static int OptionalHeaderSize { get; private set; }
        public static Process process { get; private set; }
        public static IntPtr TextSectionStart { get; private set; }
        public static IntPtr TextSectionEnd { get; private set; }
        public static IntPtr DataSectionStart { get; private set; }
        public static IntPtr DataSectionEnd { get; private set; }
        public static long DataSectionOffset { get; private set; }
        public static long TextSectionOffset { get; private set; }
        public static int DataSectionSize { get; private set; }
        public static int TextSectionSize { get; private set; }
        public static ProcessModule Module { get; private set; }
        public static bool Is32BitProcess { get; private set; }
        public static IntPtr Handle { get; private set; }

        private static bool isInitialized = false;

        public ProcessInfo(
            IntPtr handle,
            IntPtr baseAddress,
            IntPtr peHeaderAddress,
            int numberOfSections,
            int optionalHeaderSize,
            IntPtr textSectionStart,
            IntPtr textSectionEnd,
            IntPtr dataSectionStart,
            IntPtr dataSectionEnd,
            long dataSectionOffset,
            long textSectionOffset,
            int dataSectionSize,
            int textSectionSize,
            ProcessModule module,
            bool is32BitProcess)
        {
            Handle = handle;
            BaseAddress = baseAddress;
            PEHeaderAddress = peHeaderAddress;
            NumberOfSections = numberOfSections;
            OptionalHeaderSize = optionalHeaderSize;
            TextSectionStart = textSectionStart;
            TextSectionEnd = textSectionEnd;
            DataSectionStart = dataSectionStart;
            DataSectionEnd = dataSectionEnd;
            DataSectionOffset = dataSectionOffset;
            TextSectionOffset = textSectionOffset;
            DataSectionSize = dataSectionSize;
            TextSectionSize = textSectionSize;
            Module = module;
            Is32BitProcess = is32BitProcess;
            isInitialized = true;
        }

        public void PrintProcessInfo()
        {
            if (isInitialized)
            {
                Console.WriteLine($"Handle: {ProcessInfo.Handle}");
                Console.WriteLine($"Base Address: {ProcessInfo.BaseAddress}");
                Console.WriteLine($"PE Header Address: {ProcessInfo.PEHeaderAddress}");
                Console.WriteLine($"Number of Sections: {ProcessInfo.NumberOfSections}");
                Console.WriteLine($"Optional Header Size: {ProcessInfo.OptionalHeaderSize}");
                Console.WriteLine($"Text Section Start: {ProcessInfo.TextSectionStart}");
                Console.WriteLine($"Text Section End: {ProcessInfo.TextSectionEnd}");
                Console.WriteLine($"Data Section Start: {ProcessInfo.DataSectionStart}");
                Console.WriteLine($"Data Section End: {ProcessInfo.DataSectionEnd}");
                Console.WriteLine($"Is 32-Bit Process: {ProcessInfo.Is32BitProcess}");
            }
            else
            {
                Console.WriteLine("ProcessInfo has not been initialized.");
            }
        }

        public static bool IsInitialized()
        {
            return isInitialized;
        }
    }
}

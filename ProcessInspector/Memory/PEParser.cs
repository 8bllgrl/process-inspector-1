using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory
{
    public class PEParser
    {
        Process _targetProcess;

        public PEParser(String processName)
        {
            _targetProcess = GetProcessByName(processName);
            if (_targetProcess == null)
            {
                Console.WriteLine("No processes with the specified name found.");
                return;
            }

        }

        public ProcessInfo ParseProcess()
        {
            if (_targetProcess == null)
                throw new ArgumentNullException("Process was null.");
            if (string.IsNullOrWhiteSpace(_targetProcess.ProcessName))
                throw new ArgumentException("Process name cannot be null or empty.", nameof(_targetProcess.ProcessName));


            bool is32Bit = IsProcess32Bit(_targetProcess);
            IntPtr handle = FindHandle(_targetProcess);
            IntPtr baseAddress = _targetProcess.MainModule.BaseAddress;
            byte[] dosHeader = MemoryReader.ReadMemoryOutLoud(handle, baseAddress, 64);
            IntPtr peHeaderAddress = FindPEHeaderAddress(baseAddress, dosHeader);
            int numberOfSections = FindNumberOfSections(handle, peHeaderAddress);
            int optionalHeaderSize = FindOptionalHeaderSize(handle, peHeaderAddress);
            IntPtr sectionHeadersAddress = GetSectionHeaders(handle, peHeaderAddress, optionalHeaderSize);
            byte[] sectionHeaders = MemoryReader.ReadMemoryOutLoud(handle, sectionHeadersAddress, 40 * numberOfSections);
            if (numberOfSections <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfSections), "Number of sections must be greater than zero.");
            }
            if (sectionHeaders == null || sectionHeaders.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionHeaders), "sectionHeaders must be greater than zero.");
            }

            var (textSection, dataSection) = ParseSectionHeaders(sectionHeaders, baseAddress, numberOfSections);



            return new ProcessInfo(
                handle: handle,
                baseAddress: baseAddress,
                peHeaderAddress: peHeaderAddress,
                numberOfSections: numberOfSections,
                optionalHeaderSize: optionalHeaderSize,
                textSectionStart: textSection.start,
                textSectionEnd: textSection.end,
                dataSectionStart: dataSection.start,
                dataSectionEnd: dataSection.end,
                dataSectionOffset: dataSection.offset,
                textSectionOffset: textSection.offset,
                dataSectionSize: dataSection.size,
                textSectionSize: textSection.size,
                module: _targetProcess.MainModule, 
                is32BitProcess: is32Bit
            );
        }

        private IntPtr GetSectionHeaders(IntPtr handle, IntPtr peHeaderAddress, int optionalHeaderSize)
        {
            IntPtr sectionHeadersAddress = peHeaderAddress + 24 + optionalHeaderSize;
            return sectionHeadersAddress;
        }

        private static Process GetProcessByName(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            return processes.Length > 0 ? processes[0] : null;
        }

        private IntPtr FindPEHeaderAddress(IntPtr baseAddress, byte[] dosHeader)
        {
            int peHeaderOffset = BitConverter.ToInt32(dosHeader, 0x3C);
            return baseAddress + peHeaderOffset;
        }
        private short FindNumberOfSections(IntPtr handle, IntPtr peHeaderAddress)
        {
            byte[] peHeader = MemoryReader.ReadMemoryOutLoud(handle, peHeaderAddress, 248);
            return BitConverter.ToInt16(peHeader, 6);
        }

        private int FindOptionalHeaderSize(IntPtr handle, IntPtr peHeaderAddress)
        {
            byte[] peHeader = MemoryReader.ReadMemoryOutLoud(handle, peHeaderAddress, 248);
            return BitConverter.ToInt16(peHeader, 20);
        }


        private IntPtr FindHandle(Process targetProcess)
        {
            uint PROCESS_VM_READ = 0x0010;
            IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, targetProcess.Id);
            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process.");
            }
            return processHandle;
        }

        private bool IsProcessAlive(Process targetProcess)
        {
            try
            {
                return !targetProcess.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private bool IsProcess32Bit(Process targetProcess)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                bool isWow64;
                if (!IsWow64Process(targetProcess.Handle, out isWow64))
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                return isWow64;
            }
            return true;
        }

        private (SectionInfo textSection, SectionInfo dataSection) ParseSectionHeaders(byte[] sectionHeaders, IntPtr baseAddress, int numberOfSections)
        {
            const int sectionHeaderSize = 40; 
            SectionInfo textSection = null; 
            SectionInfo dataSection = null;

            for (int i = 0; i < numberOfSections; i++)
            {
                int sectionOffset = i * sectionHeaderSize;

                if (sectionOffset + sectionHeaderSize > sectionHeaders.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(sectionHeaders), "Attempting to read beyond the sectionHeaders array.");
                }

                string sectionName = Encoding.ASCII.GetString(sectionHeaders, sectionOffset, 8).TrimEnd('\0');
                int sectionVirtualSize = BitConverter.ToInt32(sectionHeaders, sectionOffset + 8);
                int sectionVirtualAddress = BitConverter.ToInt32(sectionHeaders, sectionOffset + 12);

                IntPtr sectionStart = baseAddress + sectionVirtualAddress;
                IntPtr sectionEnd = sectionStart + sectionVirtualSize;

                if (sectionName.Equals(".text", StringComparison.OrdinalIgnoreCase))
                {
                    textSection = new SectionInfo
                    {
                        start = sectionStart,
                        end = sectionEnd,
                        size = sectionVirtualSize,
                        offset = CalculateSectionOffset(sectionStart, baseAddress)
                    };
                }
                else if (sectionName.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    dataSection = new SectionInfo
                    {
                        start = sectionStart,
                        end = sectionEnd,
                        size = sectionVirtualSize,
                        offset = CalculateSectionOffset(sectionStart, baseAddress)
                    };
                }
            }

            return (textSection, dataSection);
        }

        public static long CalculateSectionOffset(IntPtr sectionStartPointer, IntPtr baseAddress)
        {
            return sectionStartPointer.ToInt64() - baseAddress.ToInt64();
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

    }
    class SectionInfo
    {
        public IntPtr start { get; set; }
        public IntPtr end { get; set; }
        public int size { get; set; }
        public long offset { get; set; }
    }
}
